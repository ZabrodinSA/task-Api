using FluentValidation;
using FluentValidation.AspNetCore;
using TaskApi.Controllers;
using TaskApi.Services.Validators;
using Microsoft.EntityFrameworkCore;
using TaskApi.Models;

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

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();

app.Run();

