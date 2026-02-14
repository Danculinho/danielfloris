using OperationsService.Application;
using OperationsService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase("ops"));
builder.Services.AddScoped<CriticalTransactionService>();
builder.Services.AddScoped<IInternalEventDispatcher, DashboardNotificationDispatcher>();
builder.Services.AddHostedService<OutboxProcessorWorker>();

var app = builder.Build();

app.MapPost("/jobs/{jobId:guid}/status/{newStatus}", async (
    Guid jobId,
    string newStatus,
    CriticalTransactionService service,
    CancellationToken cancellationToken) =>
{
    await service.UpdateJobStatusAsync(jobId, newStatus, "system", cancellationToken);
    return Results.Accepted();
});

app.MapGet("/outbox", async (AppDbContext db, CancellationToken cancellationToken) =>
{
    var data = await db.Outbox
        .OrderByDescending(x => x.OccurredUtc)
        .Select(x => new
        {
            x.Id,
            x.EventType,
            x.RetryCount,
            x.ProcessedUtc,
            x.DeadLettered,
            x.LastError
        })
        .ToListAsync(cancellationToken);

    return Results.Ok(data);
});

app.Run();
