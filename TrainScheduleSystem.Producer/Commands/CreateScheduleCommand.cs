namespace TrainScheduleSystem.Producer.Commands;

public record CreateScheduleCommand(
    string TrainNumber,
    string DepartureStation,
    string ArrivalStation,
    DateTime DepartureTime,
    DateTime ArrivalTime
);