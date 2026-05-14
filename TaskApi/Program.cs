using FluentValidation;
using FluentValidation.AspNetCore;
using TaskApi.Controllers;
using TaskApi.Services.Validators;
using Microsoft.EntityFrameworkCore;
using TaskApi.Models;
using TaskApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllersWithViews();

//Validator services
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<TasksController.PostTaskTdo>, VerifyPostTaskDtoValidator>();

//Add DB contexts
builder.Services.AddDbContext<TaskContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TaskConnection")));

builder.Services.AddSingleton<RabbitMqService>();

var app = builder.Build();

// Initialize RabbitMqService
var rabbitMqService = app.Services.GetRequiredService<RabbitMqService>();
await rabbitMqService.InitializeAsync();

var appLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

appLifetime.ApplicationStopping.Register(async () =>
{
    Console.WriteLine("Application is stopping...");
    await rabbitMqService.DisposeAsync();
    Console.WriteLine("RabbitMQ resources disposed.");
});

app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();

app.Run();

