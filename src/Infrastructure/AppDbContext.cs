using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OperationsService.Domain;

namespace OperationsService.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>().HasKey(x => x.Id);
        modelBuilder.Entity<Operation>().HasKey(x => x.Id);

        modelBuilder.Entity<OutboxMessage>(cfg =>
        {
            cfg.HasKey(x => x.Id);
            cfg.HasIndex(x => new { x.DeadLettered, x.ProcessedUtc, x.NextAttemptUtc });
            cfg.Property(x => x.EventType).HasMaxLength(256);
            cfg.Property(x => x.Payload).HasColumnType("nvarchar(max)");
            cfg.Property(x => x.LastError).HasColumnType("nvarchar(max)");
        });
    }
}

public sealed class Job
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}

public sealed class Operation
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string State { get; set; } = "Created";
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredUtc { get; set; }
    public DateTime? ProcessedUtc { get; set; }
    public int RetryCount { get; set; }
    public DateTime NextAttemptUtc { get; set; } = DateTime.UtcNow;
    public bool DeadLettered { get; set; }
    public string? LastError { get; set; }

    public static OutboxMessage FromEvent(IDomainEvent domainEvent)
    {
        return new OutboxMessage
        {
            Id = domainEvent.EventId,
            EventType = domainEvent.EventType,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            OccurredUtc = domainEvent.OccurredUtc,
            NextAttemptUtc = DateTime.UtcNow
        };
    }
}
