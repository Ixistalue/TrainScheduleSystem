namespace TrainScheduleSystem.Producer.Models;

public class TrainSchedule
{
    // Унікальний ідентифікатор рейсу
    public Guid Id { get; private set; }

    // Номер потяга
    public string TrainNumber { get; private set; }

    // Станції
    public string DepartureStation { get; private set; }
    public string ArrivalStation { get; private set; }

    // Запланований час
    public DateTime DepartureTime { get; private set; }
    public DateTime ArrivalTime { get; private set; }

    // Поточний стан рейсу
    public TrainStatus Status { get; private set; }

    // Кількість хвилин затримки
    public int DelayMinutes { get; private set; }

    // Приватний конструктор
    // Об'єкт створюється через метод Create
    private TrainSchedule()
    {
        TrainNumber = string.Empty;
        DepartureStation = string.Empty;
        ArrivalStation = string.Empty;
    }

    // Створення нового рейсу
    public static TrainSchedule Create(
        string trainNumber,
        string departureStation,
        string arrivalStation,
        DateTime departureTime,
        DateTime arrivalTime)
    {
        // Перевірка вхідних даних
        if (string.IsNullOrWhiteSpace(trainNumber))
            throw new ArgumentException("Train number is required.");

        if (string.IsNullOrWhiteSpace(departureStation))
            throw new ArgumentException("Departure station is required.");

        if (string.IsNullOrWhiteSpace(arrivalStation))
            throw new ArgumentException("Arrival station is required.");

        if (departureStation == arrivalStation)
            throw new ArgumentException(
                "Departure and arrival stations must be different.");

        if (arrivalTime <= departureTime)
            throw new ArgumentException(
                "Arrival time must be later than departure time.");

        return new TrainSchedule
        {
            Id = Guid.NewGuid(),
            TrainNumber = trainNumber,
            DepartureStation = departureStation,
            ArrivalStation = arrivalStation,
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime,
            Status = TrainStatus.Scheduled,
            DelayMinutes = 0
        };
    }

    // Зміна запланованого часу рейсу
    public void UpdateSchedule(
        DateTime departureTime,
        DateTime arrivalTime)
    {
        EnsureNotCancelledOrCompleted();

        if (arrivalTime <= departureTime)
            throw new ArgumentException(
                "Arrival time must be later than departure time.");

        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
    }

    // Реєстрація затримки
    public void DelayTrain(int delayMinutes)
    {
        EnsureNotCancelledOrCompleted();

        if (delayMinutes <= 0)
            throw new ArgumentException(
                "Delay must be greater than zero.");

        DelayMinutes = delayMinutes;
        Status = TrainStatus.Delayed;
    }

    // Скасування рейсу
    public void CancelTrain()
    {
        if (Status == TrainStatus.Completed)
            throw new InvalidOperationException(
                "Completed train cannot be cancelled.");

        if (Status == TrainStatus.Cancelled)
            throw new InvalidOperationException(
                "Train is already cancelled.");

        Status = TrainStatus.Cancelled;
    }

    // Перевірка, чи дозволено змінювати рейс
    private void EnsureNotCancelledOrCompleted()
    {
        if (Status == TrainStatus.Cancelled)
            throw new InvalidOperationException(
                "Cancelled train cannot be modified.");

        if (Status == TrainStatus.Completed)
            throw new InvalidOperationException(
                "Completed train cannot be modified.");
    }
}