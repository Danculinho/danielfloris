using Inventory.Domain;

namespace Inventory.Application;

public interface IInventoryStore
{
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct);
    Task<InventoryBalance> GetBalanceAsync(Guid itemId, CancellationToken ct);
    Task<InventoryReservation?> GetReservationAsync(Guid jobId, Guid itemId, CancellationToken ct);
    Task InsertReservationAsync(InventoryReservation reservation, CancellationToken ct);
    Task UpdateBalanceAsync(InventoryBalance balance, byte[] expectedRowVersion, CancellationToken ct);
    Task UpdateReservationAsync(InventoryReservation reservation, byte[] expectedRowVersion, CancellationToken ct);
    Task InsertMovementAsync(InventoryMovement movement, CancellationToken ct);
    Task<bool> TryInsertIdempotencyMarkerAsync(Guid jobId, string operationKey, CancellationToken ct);
}

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}

public sealed class InventoryTransactionService
{
    private const int MaxRetries = 3;
    private readonly IInventoryStore _store;

    public InventoryTransactionService(IInventoryStore store)
    {
        _store = store;
    }

    public async Task ReserveAsync(Guid jobId, Guid itemId, decimal qty, CancellationToken ct)
    {
        await ExecuteWithOptimisticRetry(async () =>
        {
            await using var tx = await _store.BeginTransactionAsync(ct);

            var balance = await _store.GetBalanceAsync(itemId, ct);
            var balanceVersion = balance.RowVersion;

            if (balance.AvailableQty < qty)
                throw new InvalidOperationException("Insufficient stock.");

            var reservation = await _store.GetReservationAsync(jobId, itemId, ct)
                ?? new InventoryReservation { JobId = jobId, ItemId = itemId, ReservedQty = 0m };

            var reservationVersion = reservation.RowVersion;

            balance.AvailableQty -= qty;
            balance.ReservedQty += qty;
            reservation.ReservedQty += qty;

            await _store.UpdateBalanceAsync(balance, balanceVersion, ct);

            if (reservationVersion.Length == 0)
                await _store.InsertReservationAsync(reservation, ct);
            else
                await _store.UpdateReservationAsync(reservation, reservationVersion, ct);

            await _store.InsertMovementAsync(new InventoryMovement
            {
                JobId = jobId,
                ItemId = itemId,
                Quantity = qty,
                MovementType = MovementType.Reservation
            }, ct);

            await tx.CommitAsync(ct);
        }, ct);
    }

    public async Task ConsumeFirstOperationAsync(Guid jobId, Guid itemId, decimal qty, CancellationToken ct)
    {
        await ExecuteWithOptimisticRetry(async () =>
        {
            await using var tx = await _store.BeginTransactionAsync(ct);

            var inserted = await _store.TryInsertIdempotencyMarkerAsync(jobId, "first-operation-consumption", ct);
            if (!inserted)
            {
                // Duplicate request (double click/retry) -> idempotent no-op.
                await tx.CommitAsync(ct);
                return;
            }

            var balance = await _store.GetBalanceAsync(itemId, ct);
            var balanceVersion = balance.RowVersion;

            if (balance.ReservedQty < qty)
                throw new InvalidOperationException("Insufficient reserved stock.");

            balance.ReservedQty -= qty;

            await _store.UpdateBalanceAsync(balance, balanceVersion, ct);

            await _store.InsertMovementAsync(new InventoryMovement
            {
                JobId = jobId,
                ItemId = itemId,
                Quantity = qty,
                MovementType = MovementType.Consumption
            }, ct);

            await tx.CommitAsync(ct);
        }, ct);
    }

    private static async Task ExecuteWithOptimisticRetry(Func<Task> operation, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await operation();
                return;
            }
            catch (ConcurrencyConflictException) when (attempt < MaxRetries)
            {
                // Retry with fresh RowVersion values.
                await Task.Delay(TimeSpan.FromMilliseconds(40 * attempt), ct);
            }
        }

        throw new ConcurrencyConflictException("Max optimistic retry attempts exceeded.");
    }
}

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message) : base(message)
    {
    }
}
