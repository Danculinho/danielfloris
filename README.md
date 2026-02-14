# Audit a korelačné ID

Tento commit pridáva základ pre audit trail:

- `AuditEvent` entita s poliami: čas (`TimestampUtc`), používateľ (`UserId`), entita (`EntityName` + `EntityId`), typ akcie (`ActionType`), old/new hodnoty (`OldValues`, `NewValues` ako JSON), korelačné ID (`CorrelationId`).
- API middleware `CorrelationIdMiddleware`, ktorý:
  - načíta `X-Correlation-ID` z requestu alebo vygeneruje nové,
  - uloží ho do `HttpContext.Items`, `TraceIdentifier` a do response headera,
  - propaguje ho do `Activity` tag/baggage.
- `AuditLogger` s minimálnymi metódami pre:
  - zmenu `JobOrder.Status`,
  - štart/koniec operácie,
  - vytvorenie/zmenu rezervácie,
  - inventory movement.

## Zapojenie do API

V `Program.cs` (alebo startup):

```csharp
builder.Services.AddCorrelationId();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
// IAuditSink -> vaša DB implementácia

app.UseCorrelationId();
```

V doménových/službových operáciách volajte `DomainAuditHooks` (alebo priamo `IAuditLogger`) pri každej požadovanej udalosti.

## DB migrácia

SQL migrácia je v `database/migrations/001_create_audit_events.sql`.
