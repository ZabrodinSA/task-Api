using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Hello, World!");
    }
    
    [HttpPost]
    public IActionResult Post([FromBody] PostTdo tdo)
    {
        return Ok("Task created! title: " + tdo.Title + ", priority: " + tdo.Priority );
    }
    
    [HttpPut("{id}/complete")]
    public IActionResult Complete(int id)
    {
        return Ok($"Task {id} marked as completed!");
    }
    
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        return Ok($"Task {id} deleted!");
    }

    public record PostTdo(string Title, int Priority);
}