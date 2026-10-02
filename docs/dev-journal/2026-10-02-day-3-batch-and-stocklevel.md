# Dev Journal — 2026-10-02 (Day 8)
## Batch & StockLevel — Phase 3.7.2 Complete

**Context:** Returned to the project after a 7-day break. Repo was
clean, Docker was down (restarted easily), everything synced.

This phase introduces perishability — the defining characteristic of
cold-chain inventory.

---

## 🎯 What We Built

### Entities

**`BatchStatus` enum**
- `Fresh` — normal, sellable
- `NearExpiry` — approaching expiry, still sellable
- `Expired` — past expiry, must not be sold
- `Spoiled` — confirmed unusable (damage, contamination)
- `Quarantined` — under investigation (e.g., temperature excursion)

**`Batch`** — a specific production lot with:
- `BatchNumber` (human-readable, e.g., "MILK-20260926-A")
- `ManufacturingDate`
- `ExpiryDate` ← the FEFO field
- `QuantityOnHand`
- `Status`
- FKs: `ProductId`, `WarehouseId`, `TenantId`

**`StockLevel`** — aggregate per Product × Warehouse:
- `QuantityOnHand`
- `QuantityReserved`
- `QuantityAvailable` (OnHand - Reserved)
- `EarliestExpiryDate` (denormalized for fast FEFO queries)
- `LastMovementAtUtc`

### Database
- Migration: `BatchAndStockLevel`
- 2 new tables: `Batches`, `StockLevels`
- No data loss — new tables only, existing `Products` unchanged

---

## 🧠 Why Two Entities Instead of One?

This is a subtle but important design decision:

### `Batch` — the truth
- Every physical lot gets its own row
- Same SKU, different expiry = different batches
- Immutable-ish: quantities change via movements, not direct edits
- **The source of truth for perishability**

### `StockLevel` — the cache
- One row per (Product, Warehouse)
- Aggregates batch quantities
- **Denormalized for read performance**
- Recomputed whenever a stock movement occurs

**Why both?**
- Querying "how much milk do we have in Bangalore?" by summing batches = slow at scale
- Querying it from a single denormalized row = instant

This is a **CQRS-lite pattern**: write to batches, read from stock levels.

---

## 🔍 The FEFO Index

The key index:

```sql
CREATE INDEX "IX_Batches_TenantId_WarehouseId_ExpiryDate"
    ON "Batches" ("TenantId", "WarehouseId", "ExpiryDate");
```

**Why this exact column order?**
- Leftmost columns filter first (`TenantId`, then `WarehouseId`)
- `ExpiryDate` is sorted within that partition
- **Postgres can answer "earliest-expiring batch for tenant X warehouse Y" in O(log n)**

Column order matters in composite indexes. Leading columns should be the
most selective filters, and the last column should be the sort key. This
index is optimized for exactly one query — FEFO picking.

---

## ✅ Verification

| Check | Result |
|---|---|
| `dotnet build` | 0 warnings, 0 errors |
| Migration created | ✅ |
| Migration applied | ✅ |
| 7 tables in Postgres | ✅ |
| `Batches` schema verified | ✅ |
| FEFO index exists | ✅ |
| All FKs present | ✅ |

---

## 🎓 Lessons Learned

### 1. Migrations are easy when the table is new

Unlike the CategoryId migration (which required backfilling existing
rows), adding `Batches` and `StockLevels` was trivial — no existing
data to reconcile. **The hardest migrations are always the ones that
change existing data, not the ones that add new tables.**

### 2. Composite indexes need careful column ordering

The `(TenantId, WarehouseId, ExpiryDate)` order isn't arbitrary.
Leftmost = filter, rightmost = sort. Reversing it would make the
index useless for FEFO.

### 3. Denormalization is a deliberate trade-off

`StockLevel` duplicates data that exists in `Batches`. This is
intentional — read performance wins over storage savings at scale.
The key is ensuring `StockLevel` is **always** recomputed after
stock movements. That's a future phase.

### 4. Enums map to integers in Postgres

`BatchStatus` becomes an `integer` column. This is efficient, but
slightly less readable in raw SQL. If we ever need to inspect
production data, we'll need to translate `0 = Fresh`, `1 = NearExpiry`,
etc. Alternative: use Postgres `CREATE TYPE` for real enums. We're
skipping that for simplicity.

---

## ⚠️ Known Gaps (Planned)

| Gap | When we'll fix |
|---|---|
| No `StockMovement` entity yet — batch quantities can't change | Phase 3.7.3 |
| No FEFO picking service | Phase 3.7.4 |
| `StockLevel` isn't auto-recomputed | Phase 3.7.3 |
| `BatchStatus` isn't auto-updated (Fresh → NearExpiry → Expired) | Phase 3.7.4 (background job) |
| No batch lifecycle (create, consume, dispose) | Phase 3.7.4 |

---

## 🎯 Next Phase

**Phase 3.7.3 — StockMovement Ledger**
- `StockMovement` entity (immutable)
- Movement types: Purchase, Sale, Adjustment, Transfer, Damage, Spoilage
- Service to record movements
- **Automatically recompute `StockLevel` and `Batch.QuantityOnHand`**

---

## 📊 Git History

```
(new) feat(domain): add Batch and StockLevel for perishability
c87935f docs: add Phase 3.7.1 journal entry
5d7f7049 feat(domain): add Category and Warehouse for cold-chain
791d16b docs: add Phase 3.6 journal entry to index
9ee5e6a feat(multi-tenancy): add tenant isolation foundation
...
```

---

*End of Phase 3.7.2 journal entry.*