using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using RabbitMQ.Client;
using TaskApi.Models;

namespace TaskApi.IntegrationTest;

public class TaskControllerTest(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    
    [Fact]
    public async Task GetTasks_ShouldReturn200()
    {
        // Arrange
        var newTask = new
        {
            Title = "Test Task",
            Priority = (int)Priority.Medium
        };
        
        var rabbitConnectionFactory = new ConnectionFactory { HostName = "localhost" };
        var rabbitConnection = await rabbitConnectionFactory.CreateConnectionAsync();
        var rabbitChannel = await rabbitConnection.CreateChannelAsync();
        
        // Act
        var createTaskResponse = await _client.PostAsJsonAsync("/tasks", newTask);

        createTaskResponse.EnsureSuccessStatusCode();
        var createdTask = await createTaskResponse.Content.ReadFromJsonAsync<TaskModel>();
        Assert.NotNull(createdTask);

        var completeTaskResponse = await _client.PutAsJsonAsync($"/tasks/{createdTask.Id}/complete", new { });
        completeTaskResponse.EnsureSuccessStatusCode();

        // Assert
        var messageReceived = false;
        var consumer = new CustomAsyncConsumer(rabbitChannel);
        
        consumer.OnMessageReceived += (body) =>
        {
            var message = Encoding.UTF8.GetString(body.ToArray());
            var payload = JsonConvert.DeserializeObject<dynamic>(message);
            if (payload != null && payload?.taskId == createdTask.Id.ToString())
            {
                messageReceived = true;
            }
        };
        
        await rabbitChannel.BasicConsumeAsync("task.completed.queue", false, consumer);
        
        await Task.Delay(1000); // Wait for the message to be consumed
        Assert.True(messageReceived, "RabbitMQ did not receive the expected message.");
    }
    
    public class CustomAsyncConsumer(IChannel channel) : AsyncDefaultBasicConsumer(channel)
    {
        private readonly IChannel _channel = channel;
        
        public Action<ReadOnlyMemory<byte>> OnMessageReceived { get; set; }

        public override async Task HandleBasicDeliverAsync(
            string consumerTag,
            ulong deliveryTag,
            bool redelivered,
            string exchange,
            string routingKey,
            IReadOnlyBasicProperties properties,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken = default)
        {
            OnMessageReceived(body);
        }
    }
}