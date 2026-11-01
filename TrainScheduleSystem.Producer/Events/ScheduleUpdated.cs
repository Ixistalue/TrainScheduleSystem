namespace TrainScheduleSystem.Producer.Events;

public record ScheduleUpdated(
    Guid EventId,
    Guid ScheduleId,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    DateTime OccurredAt
);