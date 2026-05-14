using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using TaskApi.Models;

namespace TaskApi.Services;

public class RabbitMqService : IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: "task.events",
            type: "direct",
            durable: true
        );

        await _channel.QueueDeclareAsync(
            queue: "task.completed.queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        await _channel.QueueBindAsync(
            queue: "task.completed.queue",
            exchange: "task.events",
            routingKey: "task.completed"
        );
    }

    public async Task PublishTaskCompletedAsync(TaskModel message)
    {
        try
        {
            var payload = new
            {
                taskId = message.Id,
                title = message.Title,
                completedAt = message.CompletedAt,
                priority = message.Priority.ToString()

            };
            
            var json = JsonSerializer.Serialize(payload);
            var body = Encoding.UTF8.GetBytes(json);
            
            await _channel.BasicPublishAsync(
                exchange: "task.events",
                routingKey: "task.completed",
                body: body
            );
        }
        catch (BrokerUnreachableException ex)
        {
            await Console.Error.WriteLineAsync($"RabbitMQ connection error: {ex.Message}");
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error publishing message:  {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}
