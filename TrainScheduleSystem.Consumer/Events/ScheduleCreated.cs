namespace TrainScheduleSystem.Consumer.Events;

public record ScheduleCreated(
    Guid EventId,
    Guid ScheduleId,
    string TrainNumber,
    string DepartureStation,
    string ArrivalStation,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    DateTime OccurredAt
);