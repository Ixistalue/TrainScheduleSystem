using RabbitMQ.Client;

namespace TrainScheduleSystem.Consumer.Infrastructure;

public static class RabbitMqConnection
{
    public static async Task<IConnection> CreateAsync(
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var settings = configuration.GetSection("RabbitMQ");

        var factory = new ConnectionFactory
        {
            HostName = settings["HostName"]!,
            Port = settings.GetValue<int>("Port"),
            UserName = settings["UserName"]!,
            Password = settings["Password"]!
        };

        // Встановлюємо з'єднання з RabbitMQ
        var connection = await factory.CreateConnectionAsync(
            cancellationToken);

        return connection;
    }
}