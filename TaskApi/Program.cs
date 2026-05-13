using FluentValidation;
using FluentValidation.AspNetCore;
using TaskApi.Controllers;
using TaskApi.Services.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllersWithViews();

//Validator services
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<TasksController.PostTaskTdo>, VerifyPostTaskDtoValidator>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();

app.Run();

