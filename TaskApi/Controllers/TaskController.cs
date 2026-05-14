using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApi.Models;
using TaskApi.Services;

namespace TaskApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController(
    TaskContext db,
    RabbitMqService rabbitMqService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return await db.Tasks.AnyAsync() ? Ok(db.Tasks.ToList()) : NotFound("No tasks found!");
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostTaskTdo taskTdo)
    {
        var task = new TaskModel
        {
            Id = Guid.NewGuid(),
            Title = taskTdo.Title,
            CreatedAt = DateTimeOffset.Now.ToUniversalTime(),
            Priority = (Priority)taskTdo.Priority,
            IsCompleted = false
        };
        await db.Tasks.AddAsync(task);
        await db.SaveChangesAsync();

        return Ok(task );
    }
    
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var task = await db.Tasks.FindAsync(id);
        
        if (task == null)
        {
            return NotFound($"Task with id {id} not found!");
        }
        
        if (task.IsCompleted) 
        {
            return BadRequest($"Task with id {id} is already completed!");
        }
        
        task.IsCompleted = true;
        task.CompletedAt = DateTimeOffset.Now.ToUniversalTime();
        
        try
        {
            await db.SaveChangesAsync();
            await rabbitMqService.PublishTaskCompletedAsync(task);

            return Ok(task);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Task was already modified");
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await db.Tasks.FindAsync(id);
        
        if (task == null)
        {
            return NotFound($"Task with id {id} not found!");
        }

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();

        return Ok(task);
    }

    public record PostTaskTdo(string Title, int Priority);
}