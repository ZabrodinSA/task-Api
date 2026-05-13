namespace TaskApi.Models;

public class TaskModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } // не null, не пустой, максимум 200
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; } // когда завершили
    public Priority Priority { get; set; } // Low, Medium, High
}

public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3
}