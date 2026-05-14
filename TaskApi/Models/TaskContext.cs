using Microsoft.EntityFrameworkCore;

namespace TaskApi.Models;

public sealed class TaskContext : DbContext
{
    public DbSet<TaskModel> Tasks { get; set; } = null!;

    public TaskContext(DbContextOptions<TaskContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskModel>(entity =>
        {
            entity.ToTable("tasks");

            // Primary key
            entity.HasKey(x => x.Id);

            // Id (можно генерировать на стороне БД)
            entity.Property(x => x.Id)
                .HasColumnType("uuid");

            // Title (обязательное, максимум 200)
            entity.Property(x => x.Title)
                .IsRequired();

            // IsCompleted
            entity.Property(x => x.IsCompleted)
                .IsRequired();

            // CreatedAt
            entity.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            // CompletedAt (nullable)
            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.Priority)
                .IsRequired()
                .HasConversion<int>();
        });
        
        modelBuilder.Entity<TaskModel>()
            .Property(b => b.Version)
            .IsRowVersion();
    }
}