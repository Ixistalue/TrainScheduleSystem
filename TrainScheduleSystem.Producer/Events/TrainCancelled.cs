namespace TrainScheduleSystem.Producer.Events;

public record TrainCancelled(
    Guid EventId,
    Guid ScheduleId,
    string TrainNumber,
    DateTime OccurredAt
);