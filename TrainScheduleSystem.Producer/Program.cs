using TrainScheduleSystem.Producer.Infrastructure;


using TrainScheduleSystem.Producer.Messaging;
using TrainScheduleSystem.Producer.Repositories;



using TrainScheduleSystem.Producer.Services;












var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();










//////moyo
///
// Створюємо підключення до RabbitMQ
var rabbitMqConnection = await RabbitMqConnection.CreateAsync(
    builder.Configuration);

// Реєструємо підключення в Dependency Injection
builder.Services.AddSingleton(rabbitMqConnection);

builder.Services.AddSingleton<TrainScheduleRepository>();
builder.Services.AddSingleton<EventPublisher>();


builder.Services.AddSingleton<TrainScheduleService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    await app.RunAsync();
}
finally
{
    await rabbitMqConnection.DisposeAsync();
}
