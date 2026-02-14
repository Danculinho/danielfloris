namespace Inventory.Domain;

public enum MovementType
{
    Reservation = 1,
    Consumption = 2,
    Release = 3
}

public sealed class InventoryBalance
{
    public Guid ItemId { get; init; }
    public decimal AvailableQty { get; set; }
    public decimal ReservedQty { get; set; }

    // Optimistic concurrency token.
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class InventoryReservation
{
    public Guid JobId { get; init; }
    public Guid ItemId { get; init; }
    public decimal ReservedQty { get; set; }

    // Optimistic concurrency token.
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class InventoryMovement
{
    public long Id { get; init; }
    public Guid JobId { get; init; }
    public Guid ItemId { get; init; }
    public MovementType MovementType { get; init; }
    public decimal Quantity { get; init; }

    // Business uniqueness is enforced by (JobId, MovementType).
    public string BusinessKey => $"{JobId}:{MovementType}";
}

public sealed class ConsumptionIdempotency
{
    public long Id { get; init; }
    public Guid JobId { get; init; }
    public string OperationKey { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
