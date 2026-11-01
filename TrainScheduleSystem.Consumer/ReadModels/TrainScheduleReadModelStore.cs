using TrainScheduleSystem.Consumer.Events;

namespace TrainScheduleSystem.Consumer.ReadModels;

public class TrainScheduleReadModelStore
{
    private readonly Dictionary<Guid, TrainScheduleReadModel> _schedules = new();

    private readonly object _lock = new();

    // Обробка створення розкладу
    public void Apply(ScheduleCreated @event)
    {
        lock (_lock)
        {
            var schedule = new TrainScheduleReadModel
            {
                Id = @event.ScheduleId,
                TrainNumber = @event.TrainNumber,
                DepartureStation = @event.DepartureStation,
                ArrivalStation = @event.ArrivalStation,
                DepartureTime = @event.DepartureTime,
                ArrivalTime = @event.ArrivalTime,
                Status = "Scheduled",
                DelayMinutes = 0
            };

            _schedules[@event.ScheduleId] = schedule;
        }
    }

    // Обробка оновлення розкладу
    public void Apply(ScheduleUpdated @event)
    {
        lock (_lock)
        {
            if (!_schedules.TryGetValue(@event.ScheduleId, out var schedule))
                return;

            schedule.DepartureTime = @event.DepartureTime;
            schedule.ArrivalTime = @event.ArrivalTime;
        }
    }

    // Обробка затримки потяга
    public void Apply(TrainDelayed @event)
    {
        lock (_lock)
        {
            if (!_schedules.TryGetValue(@event.ScheduleId, out var schedule))
                return;

            schedule.DelayMinutes = @event.DelayMinutes;
            schedule.Status = "Delayed";
        }
    }

    // Обробка скасування потяга
    public void Apply(TrainCancelled @event)
    {
        lock (_lock)
        {
            if (!_schedules.TryGetValue(@event.ScheduleId, out var schedule))
                return;

            schedule.Status = "Cancelled";
        }
    }

    // Отримання всіх розкладів
    public IEnumerable<TrainScheduleReadModel> GetAll()
    {
        lock (_lock)
        {
            return _schedules.Values
                .Select(schedule => new TrainScheduleReadModel
                {
                    Id = schedule.Id,
                    TrainNumber = schedule.TrainNumber,
                    DepartureStation = schedule.DepartureStation,
                    ArrivalStation = schedule.ArrivalStation,
                    DepartureTime = schedule.DepartureTime,
                    ArrivalTime = schedule.ArrivalTime,
                    Status = schedule.Status,
                    DelayMinutes = schedule.DelayMinutes
                })
                .ToList();
        }
    }
}