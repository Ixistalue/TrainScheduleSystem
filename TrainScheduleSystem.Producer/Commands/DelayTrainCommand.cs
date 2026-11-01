namespace TrainScheduleSystem.Producer.Commands;

public record DelayTrainCommand(
    Guid ScheduleId,
    int DelayMinutes
);