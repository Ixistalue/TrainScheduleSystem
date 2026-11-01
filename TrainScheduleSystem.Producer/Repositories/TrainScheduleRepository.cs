using System.Collections.Concurrent;
using TrainScheduleSystem.Producer.Models;

namespace TrainScheduleSystem.Producer.Repositories;

public class TrainScheduleRepository
{
    // Тимчасове сховище рейсів у пам'яті
    private readonly ConcurrentDictionary<Guid, TrainSchedule> _schedules = new();

    // Додавання нового рейсу
    public void Add(TrainSchedule schedule)
    {
        if (!_schedules.TryAdd(schedule.Id, schedule))
        {
            throw new InvalidOperationException(
                "Schedule with this ID already exists.");
        }
    }

    // Пошук рейсу за ID
    public TrainSchedule GetById(Guid id)
    {
        if (!_schedules.TryGetValue(id, out var schedule))
        {
            throw new KeyNotFoundException(
                $"Schedule with ID {id} was not found.");
        }

        return schedule;
    }

    // Отримання всіх рейсів
    public IEnumerable<TrainSchedule> GetAll()
    {
        return _schedules.Values.ToList();
    }
}