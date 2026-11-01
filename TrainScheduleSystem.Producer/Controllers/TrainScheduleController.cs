using Microsoft.AspNetCore.Mvc;
using TrainScheduleSystem.Producer.Commands;
using TrainScheduleSystem.Producer.Services;

namespace TrainScheduleSystem.Producer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainScheduleController : ControllerBase
{
    private readonly TrainScheduleService _service;

    public TrainScheduleController(TrainScheduleService service)
    {
        _service = service;
    }

    // Створення розкладу
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var schedule = await _service.CreateScheduleAsync(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = schedule.Id },
            schedule);
    }

    // Оновлення розкладу
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.ScheduleId)
        {
            return BadRequest("ID in URL does not match ID in request body.");
        }

        var schedule = await _service.UpdateScheduleAsync(
            command,
            cancellationToken);

        return Ok(schedule);
    }

    // Встановлення затримки
    [HttpPost("{id:guid}/delay")]
    public async Task<IActionResult> Delay(
        Guid id,
        [FromBody] DelayTrainCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.ScheduleId)
        {
            return BadRequest("ID in URL does not match ID in request body.");
        }

        var schedule = await _service.DelayTrainAsync(
            command,
            cancellationToken);

        return Ok(schedule);
    }

    // Скасування потяга
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CancelTrainCommand(id);

        var schedule = await _service.CancelTrainAsync(
            command,
            cancellationToken);

        return Ok(schedule);
    }

    // Отримання одного розкладу
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var schedule = _service.GetSchedule(id);

        return Ok(schedule);
    }

    // Отримання всіх розкладів
    [HttpGet]
    public IActionResult GetAll()
    {
        var schedules = _service.GetAllSchedules();

        return Ok(schedules);
    }
}