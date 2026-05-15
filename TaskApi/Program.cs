using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using TaskApi.Controllers;
using TaskApi.Services.Validators;
using Microsoft.EntityFrameworkCore;
using TaskApi.Models;
using TaskApi.Services;

Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllersWithViews();

//Validator services
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<TasksController.PostTaskTdo>, VerifyPostTaskDtoValidator>();

//Add DB contexts
var dbUsername = builder.Configuration["DB_USERNAME"];
var dbPassword = builder.Configuration["DB_PASSWORD"];
var dbHost = builder.Configuration["BD_HOST"];

if (string.IsNullOrEmpty(dbUsername) || string.IsNullOrEmpty(dbPassword) || string.IsNullOrEmpty(dbHost))
{
    throw new InvalidOperationException("Database configuration is missing. Please set DB_USERNAME, DB_PASSWORD, and BD_HOST environment variables.");
}

builder.Services.AddDbContext<TaskContext>(options =>
        options.UseNpgsql(
            $"Host={dbHost};Port=5432;Database=tasks;Username={dbUsername};Password={dbPassword}"));

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

// app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program
{
}