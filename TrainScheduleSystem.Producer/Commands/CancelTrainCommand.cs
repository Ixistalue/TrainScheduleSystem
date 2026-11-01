namespace TrainScheduleSystem.Producer.Commands;

public record CancelTrainCommand(
    Guid ScheduleId
);