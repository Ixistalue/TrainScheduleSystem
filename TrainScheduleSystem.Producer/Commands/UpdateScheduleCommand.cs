namespace TrainScheduleSystem.Producer.Commands;

public record UpdateScheduleCommand(
    Guid ScheduleId,
    DateTime DepartureTime,
    DateTime ArrivalTime
);