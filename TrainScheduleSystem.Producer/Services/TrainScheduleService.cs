using TrainScheduleSystem.Producer.Commands;
using TrainScheduleSystem.Producer.Events;
using TrainScheduleSystem.Producer.Messaging;
using TrainScheduleSystem.Producer.Models;
using TrainScheduleSystem.Producer.Repositories;

namespace TrainScheduleSystem.Producer.Services;

public class TrainScheduleService
{
    private readonly TrainScheduleRepository _repository;
    private readonly EventPublisher _eventPublisher;

    // Не дозволяє декільком командам одночасно змінювати агрегати
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public TrainScheduleService(
        TrainScheduleRepository repository,
        EventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    // Створення розкладу
    public async Task<TrainSchedule> CreateScheduleAsync(
        CreateScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var schedule = TrainSchedule.Create(
                command.TrainNumber,
                command.DepartureStation,
                command.ArrivalStation,
                command.DepartureTime,
                command.ArrivalTime);

            _repository.Add(schedule);

            var @event = new ScheduleCreated(
                Guid.NewGuid(),
                schedule.Id,
                schedule.TrainNumber,
                schedule.DepartureStation,
                schedule.ArrivalStation,
                schedule.DepartureTime,
                schedule.ArrivalTime,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(
                @event,
                "schedule.created",
                cancellationToken);

            return schedule;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Оновлення розкладу
    public async Task<TrainSchedule> UpdateScheduleAsync(
        UpdateScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var schedule = _repository.GetById(command.ScheduleId);

            schedule.UpdateSchedule(
                command.DepartureTime,
                command.ArrivalTime);

            var @event = new ScheduleUpdated(
                Guid.NewGuid(),
                schedule.Id,
                schedule.DepartureTime,
                schedule.ArrivalTime,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(
                @event,
                "schedule.updated",
                cancellationToken);

            return schedule;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Затримка потяга
    public async Task<TrainSchedule> DelayTrainAsync(
        DelayTrainCommand command,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var schedule = _repository.GetById(command.ScheduleId);

            schedule.DelayTrain(command.DelayMinutes);

            var @event = new TrainDelayed(
                Guid.NewGuid(),
                schedule.Id,
                schedule.TrainNumber,
                schedule.DelayMinutes,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(
                @event,
                "train.delayed",
                cancellationToken);

            return schedule;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Скасування потяга
    public async Task<TrainSchedule> CancelTrainAsync(
        CancelTrainCommand command,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var schedule = _repository.GetById(command.ScheduleId);

            schedule.CancelTrain();

            var @event = new TrainCancelled(
                Guid.NewGuid(),
                schedule.Id,
                schedule.TrainNumber,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(
                @event,
                "train.cancelled",
                cancellationToken);

            return schedule;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Отримання одного розкладу
    public TrainSchedule GetSchedule(Guid id)
    {
        return _repository.GetById(id);
    }

    // Отримання всіх розкладів
    public IEnumerable<TrainSchedule> GetAllSchedules()
    {
        return _repository.GetAll();
    }
}