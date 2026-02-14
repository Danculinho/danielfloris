# BEMI METAL – upresnený návrh ERP/MES

Tento dokument zapracúva tvoje odpovede a mení pôvodnú špecifikáciu na implementovateľný návrh.

## 1) JobOrder môže mať viac kusov/sérií

Rozhodnutie:
- `JobOrder` nebude reprezentovať iba 1 kus, ale výrobnú zákazku s množstvom.

Doplnenie modelu:
- `JobOrder`
  - `PlannedQuantity` (int, > 0)
  - `CompletedQuantity` (int, >= 0)
  - voliteľne `UnitLabel` (ks/sada)

Pravidlá:
- `CompletedQuantity` sa navyšuje až po dokončení poslednej operácie pre kus/sériu.
- `JobOrder.Status = Completed` až keď `CompletedQuantity == PlannedQuantity`.
- Ak je výroba po dávkach, odporúčané je doplniť `JobBatch`:
  - `Id`, `JobOrderId`, `BatchNumber`, `PlannedQty`, `CompletedQty`, `Status`.

## 2) Scrap/waste bez presnej evidencie množstiev

Rozhodnutie:
- presné množstvá odpadu sa neevidujú.

Odporúčaný kompromis:
- ponechať typ pohybu `WasteNote` iba ako kvalitatívny záznam (poznámka/checkbox), bez vplyvu na skladové množstvá.
- Cieľ: operátor vie zaznamenať problém, ale systém zostáva jednoduchý a rýchly.

## 3) Štart/stop operátora – jednoduchosť + funkčnosť

Rozhodnutie:
- primárny režim: veľké tlačidlá `START` / `DONE` na shopfloor obrazovke.
- QR scan: voliteľný „fast entry", nie povinný.

Prečo:
- manuálne tlačidlá sú robustné, nevyžadujú kameru ani riešenie kvality osvetlenia.
- QR je dobrý doplnok na rýchle otvorenie správnej operácie, ale nie hard dependency.

Odporúčaný UX flow:
1. Operátor otvorí „Moje pracovisko".
2. Vidí zoznam `Ready/InProgress` operácií.
3. Vyberie operáciu alebo načíta QR z pracovného lístka.
4. Stlačí `START`.
5. Po vykonaní stlačí `DONE`.

Backend guard pravidlá (povinné):
- `START` povoliť iba ak predchádzajúca operácia = `Done`.
- pri prvom `START` na jobe vykonať transakčný odpis rezervácie.
- `DONE` povoliť len pre operáciu v stave `InProgress`.

## 4) Offline režim netreba

Rozhodnutie:
- systém je online-only v LAN.

Dopad:
- žiadna lokálna synchronizácia, žiadne queue v prehliadači.
- zjednodušená implementácia a menšie riziko nekonzistentných dát.

## 5) Ročná inventúra + možnosť freeze režimu

Rozhodnutie:
- pridať voliteľný `InventoryFreeze` režim pre ročnú inventúru.

Návrh:
- entita `InventoryPeriod`
  - `Id`, `Name`, `StartedAt`, `EndedAt`, `Status` (`Open`, `Frozen`, `Closed`), `CreatedBy`.
- keď `Frozen`:
  - zakázať manuálne skladové úpravy a nové rezervácie (alebo ich dať do čakajúceho stavu podľa konfigurácie),
  - povoliť iba inventúrne korekcie role `Admin`.

Po inventúre:
- vykonať `StockAdjustment` pohyb pre dorovnanie na fyzický stav.
- uložiť inventúrny protokol (CSV/PDF).

## 6) Minimálne DB doplnenia (PostgreSQL)

- `job_orders`
  - pridať `planned_quantity int not null check (planned_quantity > 0)`
  - pridať `completed_quantity int not null default 0 check (completed_quantity >= 0)`
- `job_operations`
  - unikátny index `(job_id, sequence_number)`
- `inventory_balances`
  - `row_version bigint not null default 0` (optimistická konkurencia)
- `inventory_movements`
  - `movement_type` (`Reserve`, `Deduct`, `Adjust`, `WasteNote`)
  - `business_key` unique (idempotencia)
- `inventory_periods`
  - tabuľka pre freeze/inventúrne okná

## 7) API pravidlá (ASP.NET Core)

- `POST /api/jobs` – vytvorenie jobu vrátane `PlannedQuantity`.
- `POST /api/jobs/{id}/reserve-materials` – rezervácia s kontrolou dostupnosti.
- `POST /api/operations/{id}/start` – sekvenčná validácia + prípadný prvý odpis materiálu.
- `POST /api/operations/{id}/done` – dokončenie operácie.
- `POST /api/inventory/freeze` – zapnutie freeze režimu (`Admin`).
- `POST /api/inventory/unfreeze` – ukončenie freeze režimu (`Admin`).

Všetky skladové zmeny musia ísť cez transakčnú service vrstvu.

## 8) Konečné odporúčanie pre tvoj prípad

Na základe tvojich odpovedí:
- multi-quantity JobOrder je správne rozhodnutie,
- scrap evidovať iba poznámkovo (bez množstva),
- shopfloor ponechať primárne tlačidlový, QR ako bonus,
- offline režim vynechať,
- inventúrny freeze doplniť ako admin funkciu (1× ročne využiteľné).

Toto drží systém jednoduchý pre operátorov, ale stále bezpečný pre sklad a plánovanie.
