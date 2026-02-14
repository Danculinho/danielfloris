CREATE TABLE IF NOT EXISTS audit_events (
    id BIGSERIAL PRIMARY KEY,
    timestamp_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    user_id TEXT NULL,
    entity_name TEXT NOT NULL,
    entity_id TEXT NOT NULL,
    action_type TEXT NOT NULL,
    old_values JSONB NULL,
    new_values JSONB NULL,
    correlation_id TEXT NOT NULL,
    operation_name TEXT NULL
);

CREATE INDEX IF NOT EXISTS ix_audit_events_timestamp_utc ON audit_events (timestamp_utc DESC);
CREATE INDEX IF NOT EXISTS ix_audit_events_entity ON audit_events (entity_name, entity_id);
CREATE INDEX IF NOT EXISTS ix_audit_events_correlation_id ON audit_events (correlation_id);
