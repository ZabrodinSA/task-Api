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
    
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private int _activePublications = 0;
    private bool _disposed = false;

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
        Interlocked.Increment(ref _activePublications);

        try
        {
            await _semaphore.WaitAsync();

            var payload = new
            {
                taskId = message.Id,
                title = message.Title,
                completedAt = message.CompletedAt,
                priority = message.Priority.ToString()

            };

            var json = JsonSerializer.Serialize(payload);
            var body = Encoding.UTF8.GetBytes(json);

            if (_channel != null)
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
        finally
        {
            _semaphore.Release();
            Interlocked.Decrement(ref _activePublications);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return; 
        _disposed = true;
        
        var timeout = TimeSpan.FromSeconds(5);
        var start = DateTime.UtcNow;
        
        while (_activePublications > 0 && DateTime.UtcNow - start < timeout)
        {
            await Task.Delay(100); 
        }
        
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
