namespace TrainScheduleSystem.Producer.Events;

public record TrainDelayed(
    Guid EventId,
    Guid ScheduleId,
    string TrainNumber,
    int DelayMinutes,
    DateTime OccurredAt
);