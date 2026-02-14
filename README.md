# OperationsService - Transactional Outbox Example

Tento projekt demonštruje **Transactional Outbox Pattern** v ASP.NET Core štýle:

- Pri kritickej zmene (`CriticalTransactionService`) sa v **tej istej DB transakcii** ukladajú:
  - business dáta (`Jobs`, `Operations`)
  - doménové udalosti do `Outbox`
- Background worker `OutboxProcessorWorker` bezpečne spracuje outbox:
  - retry s exponenciálnym backoff
  - dead-letter flag po prekročení limitu pokusov
- Udalosti:
  - `JobStatusChanged`
  - `OperationStarted`
  - `OperationFinished`
  - `InventoryDeducted`
- `DashboardNotificationDispatcher` používa udalosti na interné notifikácie/logiku dashboard refresh bez porušenia transakčnej integrity.

## Endpointy

- `POST /jobs/{jobId}/status/{newStatus}` – vykoná kritickú transakciu a zapíše outbox.
- `GET /outbox` – kontrola stavu outbox správ.
