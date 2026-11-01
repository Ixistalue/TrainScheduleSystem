using TrainScheduleSystem.Consumer;
using TrainScheduleSystem.Consumer.ReadModels;

var builder = WebApplication.CreateBuilder(args);

// Реєструємо сховище Read Model
builder.Services.AddSingleton<TrainScheduleReadModelStore>();

// Реєструємо фоновий Worker
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Отримання всіх розкладів
app.MapGet("/api/schedules", (
    TrainScheduleReadModelStore store) =>
{
    return Results.Ok(store.GetAll());
});

// Отримання конкретного розкладу
app.MapGet("/api/schedules/{id:guid}", (
    Guid id,
    TrainScheduleReadModelStore store) =>
{
    var schedule = store.GetAll()
        .FirstOrDefault(x => x.Id == id);

    if (schedule == null)
    {
        return Results.NotFound(
            new { message = "Schedule not found." });
    }

    return Results.Ok(schedule);
});

// Запускаємо HTTP API та Worker
app.Urls.Add("http://localhost:5080");

await app.RunAsync();