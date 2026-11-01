using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TrainScheduleSystem.Producer.Infrastructure;

namespace TrainScheduleSystem.Producer.Messaging;

public class EventPublisher
{
    private readonly RabbitMqConnection _rabbitMqConnection;
    private readonly string _exchangeName;

    // Захищає спільний канал від одночасних публікацій
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public EventPublisher(
        RabbitMqConnection rabbitMqConnection,
        IConfiguration configuration)
    {
        _rabbitMqConnection = rabbitMqConnection;

        _exchangeName = configuration
            .GetSection("RabbitMQ")["ExchangeName"]!;
    }

    public async Task PublishAsync<T>(
        T @event,
        string routingKey,
        CancellationToken cancellationToken = default)
    {
        // 1. Перетворюємо об'єкт події на JSON
        string json = JsonSerializer.Serialize(@event);

        // 2. Перетворюємо JSON на масив байтів
        byte[] body = Encoding.UTF8.GetBytes(json);

        // 3. Створюємо властивості повідомлення
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        // 4. Отримуємо доступ до каналу для публікації
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await _rabbitMqConnection.Channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}