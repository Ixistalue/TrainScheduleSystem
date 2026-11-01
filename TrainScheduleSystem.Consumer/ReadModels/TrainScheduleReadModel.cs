namespace TrainScheduleSystem.Consumer.ReadModels;

public class TrainScheduleReadModel
{
    public Guid Id { get; set; }

    public string TrainNumber { get; set; } = string.Empty;

    public string DepartureStation { get; set; } = string.Empty;

    public string ArrivalStation { get; set; } = string.Empty;

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public string Status { get; set; } = "Scheduled";

    public int DelayMinutes { get; set; }
}