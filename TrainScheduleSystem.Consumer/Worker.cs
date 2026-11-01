using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrainScheduleSystem.Consumer.Events;
using TrainScheduleSystem.Consumer.Infrastructure;
using TrainScheduleSystem.Consumer.ReadModels;

namespace TrainScheduleSystem.Consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly TrainScheduleReadModelStore _readModelStore;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        TrainScheduleReadModelStore readModelStore)
    {
        _logger = logger;
        _configuration = configuration;
        _readModelStore = readModelStore;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        // 1. Підключення до RabbitMQ
        var connection = await RabbitMqConnection.CreateAsync(
            _configuration,
            stoppingToken);

        // 2. Створення окремих каналів для двох Consumer
        var scheduleChannel = await connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        var notificationChannel = await connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        var exchangeName = _configuration
            .GetSection("RabbitMQ")["ExchangeName"]!;

        // 3. Оголошення Exchange
        await scheduleChannel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await notificationChannel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        // 4. Створення черг
        await scheduleChannel.QueueDeclareAsync(
            queue: "schedule.queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await notificationChannel.QueueDeclareAsync(
            queue: "notifications.queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // 5. Прив'язування черг до Exchange
        await scheduleChannel.QueueBindAsync(
            queue: "schedule.queue",
            exchange: exchangeName,
            routingKey: "schedule.#",
            cancellationToken: stoppingToken);

        await scheduleChannel.QueueBindAsync(
            queue: "schedule.queue",
            exchange: exchangeName,
            routingKey: "train.#",
            cancellationToken: stoppingToken);

        await notificationChannel.QueueBindAsync(
    queue: "notifications.queue",
    exchange: exchangeName,
    routingKey: "train.*",
    cancellationToken: stoppingToken);
        /*
        await notificationChannel.QueueBindAsync(
            queue: "notifications.queue",
            exchange: exchangeName,
            routingKey: "train.delayed",
            cancellationToken: stoppingToken);

        await notificationChannel.QueueBindAsync(
            queue: "notifications.queue",
            exchange: exchangeName,
            routingKey: "train.cancelled",
            cancellationToken: stoppingToken);*/

        _logger.LogInformation(
            "RabbitMQ queues and bindings configured.");

        // 6. Перший Consumer — формування Read Model
        var scheduleConsumer = new AsyncEventingBasicConsumer(
            scheduleChannel);

        scheduleConsumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                string json = Encoding.UTF8.GetString(
                    eventArgs.Body.ToArray());

                string routingKey = eventArgs.RoutingKey;

                switch (routingKey)
                {
                    case "schedule.created":
                        {
                            var @event = JsonSerializer.Deserialize<ScheduleCreated>(json)
                                ?? throw new InvalidOperationException(
                                    "Could not deserialize ScheduleCreated.");

                            _readModelStore.Apply(@event);

                            _logger.LogInformation(
                                "Schedule created in Read Model: {ScheduleId}",
                                @event.ScheduleId);

                            break;
                        }

                    case "schedule.updated":
                        {
                            var @event = JsonSerializer.Deserialize<ScheduleUpdated>(json)
                                ?? throw new InvalidOperationException(
                                    "Could not deserialize ScheduleUpdated.");

                            _readModelStore.Apply(@event);

                            _logger.LogInformation(
                                "Schedule updated in Read Model: {ScheduleId}",
                                @event.ScheduleId);

                            break;
                        }

                    case "train.delayed":
                        {
                            var @event = JsonSerializer.Deserialize<TrainDelayed>(json)
                                ?? throw new InvalidOperationException(
                                    "Could not deserialize TrainDelayed.");

                            _readModelStore.Apply(@event);

                            _logger.LogInformation(
                                "Train delayed in Read Model: {ScheduleId}",
                                @event.ScheduleId);

                            break;
                        }

                    case "train.cancelled":
                        {
                            var @event = JsonSerializer.Deserialize<TrainCancelled>(json)
                                ?? throw new InvalidOperationException(
                                    "Could not deserialize TrainCancelled.");

                            _readModelStore.Apply(@event);

                            _logger.LogInformation(
                                "Train cancelled in Read Model: {ScheduleId}",
                                @event.ScheduleId);

                            break;
                        }

                    default:
                        throw new InvalidOperationException(
                            $"Unknown routing key: {routingKey}");
                }

                // Підтверджуємо успішну обробку повідомлення
                await scheduleChannel.BasicAckAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error processing schedule event.");

                // Повідомлення повертається в чергу
                await scheduleChannel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        // 7. Другий Consumer — сповіщення
        var notificationConsumer = new AsyncEventingBasicConsumer(
            notificationChannel);

        notificationConsumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                string json = Encoding.UTF8.GetString(
                    eventArgs.Body.ToArray());

                _logger.LogInformation(
                    "NOTIFICATION | RoutingKey: {RoutingKey} | Message: {Message}",
                    eventArgs.RoutingKey,
                    json);

                await notificationChannel.BasicAckAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error processing notification.");

                await notificationChannel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        // 8. Запуск обох Consumer



        
        await scheduleChannel.BasicConsumeAsync(
            queue: "schedule.queue",
            autoAck: false,
            consumer: scheduleConsumer,
            cancellationToken: stoppingToken);

        await notificationChannel.BasicConsumeAsync(
            queue: "notifications.queue",
            autoAck: false,
            consumer: notificationConsumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Both consumers are running and waiting for messages.");

        // Worker продовжує працювати
        try
        {
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Нормальне завершення Worker
        }

        await scheduleChannel.DisposeAsync();
        await notificationChannel.DisposeAsync();
        await connection.DisposeAsync();
    }
}