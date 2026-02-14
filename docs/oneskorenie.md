# Pravidlá pre určenie oneskorenia (`IsDelayed`)

## 1) Jednoznačné pravidlo oneskorenia

Systém používa nasledovné pravidlo:

```text
IsDelayed = (DueDateUtc < NowUtc) AND (Status NOT IN [Completed, Shipped, Closed])
```

Kde:
- `DueDateUtc` je termín prevedený do UTC,
- `NowUtc` je aktuálny čas backend servera v UTC,
- `Status` je aktuálny stav objednávky/úlohy.

> Hraničné správanie: ak `DueDateUtc == NowUtc`, položka ešte **nie je** oneskorená.

## 2) Časová zóna systému a jednotné porovnanie dátumov

- Lokálna časová zóna fabriky: `Europe/Bratislava`.
- Vstupné termíny sa na backende interpretujú v lokálnej fabríkovej zóne (`Europe/Bratislava`), následne sa normalizujú na UTC.
- Všetky porovnania dátumov pre výpočet `IsDelayed` prebiehajú výhradne v UTC.

Odporúčaný postup na backende:
1. Načítaj `DueDate`.
2. Ak je bez časovej zóny, interpretuj ho ako `Europe/Bratislava`.
3. Preveď na UTC (`DueDateUtc`).
4. Porovnaj s `NowUtc`.

## 3) Kalendárne vs. pracovné dni

Aktuálne sa oneskorenie vyhodnocuje podľa **kalendárnych dní/času** (bez pracovného kalendára).

To znamená, že:
- víkendy a sviatky sa neodpočítavajú,
- neberú sa do úvahy smeny.

Ak bude neskôr potrebné vyhodnocovanie podľa pracovného kalendára, pravidlo sa rozšíri o továrenský kalendár (pracovné dni/smeny), ale aktuálna implementácia je kalendárna.

## 4) Zodpovednosť backendu a frontendu

- **Backend** vždy vracia odvodený príznak `IsDelayed`.
- **Frontend** hodnotu `IsDelayed` iba renderuje (bez vlastnej biznis logiky pre výpočet oneskorenia).

Príklad API odpovede:

```json
{
  "id": "WO-2026-00125",
  "status": "InProgress",
  "dueDate": "2026-02-10T14:00:00+01:00",
  "isDelayed": true
}
```

## 5) Referenčná implementácia (pseudokód)

```ts
const CLOSED_STATUSES = new Set(["Completed", "Shipped", "Closed"]);

function computeIsDelayed(dueDateInput: string, status: string, nowUtc: Date = new Date()): boolean {
  const dueDateUtc = toUtc(dueDateInput, "Europe/Bratislava");
  return dueDateUtc.getTime() < nowUtc.getTime() && !CLOSED_STATUSES.has(status);
}
```
