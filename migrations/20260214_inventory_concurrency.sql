-- Concurrency and idempotency hardening for reservations and consumption modules.

-- 1) Add optimistic concurrency token on stock state tables.
ALTER TABLE InventoryBalance
    ADD RowVersion rowversion NOT NULL;

ALTER TABLE InventoryReservation
    ADD RowVersion rowversion NOT NULL;

-- 2) Add immutable business key for movement records.
ALTER TABLE InventoryMovement
    ADD BusinessKey AS CONCAT(CAST(JobId AS nvarchar(50)), ':', MovementType) PERSISTED;

CREATE UNIQUE INDEX UX_InventoryMovement_JobId_MovementType
    ON InventoryMovement (JobId, MovementType);

-- 3) Add idempotency marker table for 1st operation consumption.
CREATE TABLE ConsumptionIdempotency (
    Id bigint IDENTITY(1,1) PRIMARY KEY,
    JobId uniqueidentifier NOT NULL,
    OperationKey nvarchar(100) NOT NULL,
    CreatedAt datetime2 NOT NULL CONSTRAINT DF_ConsumptionIdempotency_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UX_ConsumptionIdempotency_JobId_OperationKey UNIQUE (JobId, OperationKey)
);
