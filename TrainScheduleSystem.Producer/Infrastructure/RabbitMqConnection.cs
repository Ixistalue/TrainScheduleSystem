using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace TrainScheduleSystem.Producer.Infrastructure;

public sealed class RabbitMqConnection : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public IChannel Channel => _channel;

    private RabbitMqConnection(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqConnection> CreateAsync(
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        // Отримуємо налаштування з appsettings.json
        var settings = configuration.GetSection("RabbitMQ");

        var factory = new ConnectionFactory
        {
            HostName = settings["HostName"]!,
            Port = settings.GetValue<int>("Port"),
            UserName = settings["UserName"]!,
            Password = settings["Password"]!
        };

        // Створюємо мережеве з'єднання з RabbitMQ
        var connection = await factory.CreateConnectionAsync(cancellationToken);

        try
        {
            // Створюємо канал для роботи з RabbitMQ
            var channel = await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

            // Створюємо Topic Exchange, якщо його ще немає
            var exchangeName = settings["ExchangeName"]!;

            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken);

            return new RabbitMqConnection(connection, channel);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    // Коректно закриваємо канал і з'єднання
    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}