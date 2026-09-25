# Dev Journal — 2026-09-25 (Day 2, cont.)
## Cold-Chain Domain — Phase 3.7.1

**Context:** After Phase 3.6 (multi-tenancy foundation), we started
expanding the domain to reflect actual cold-chain needs. This phase
adds Category and Warehouse with temperature zones.

---

## 🎯 What We Built

### Entities
- `TemperatureZone` enum (Frozen, Chilled, Ambient)
- `Category` — product grouping with required temperature zone
- `Warehouse` — dark store with supported zone, code, and address
- `Product` now has `CategoryId` FK

### Services
- `ICategoryService` + `CategoryService`
- `IWarehouseService` + `WarehouseService`

### API
- `POST/GET /api/categories`
- `POST/GET /api/warehouses`
- Updated `Products` DTOs to include `CategoryId`

### Database
- Migration: `ColdChainDomainFoundation`
- 2 new tables: `Categories`, `Warehouses`
- New FK: `Products.CategoryId → Categories.Id`
- Composite unique indexes: `(TenantId, Name)` for Category, `(TenantId, Code)` for Warehouse

---

## 🐛 Migration Gotcha — NOT NULL FK on Existing Data

**Symptom:** Migration failed with:
```
23503: insert or update on table "Products" violates foreign key constraint
"FK_Products_Categories_CategoryId"
```

**Cause:** The migration tried to add `CategoryId` as NOT NULL with default
empty GUID to 2 existing product rows. No category exists with empty GUID.

**Fix (dev):** Delete existing products, retry migration.

**Fix (production):** Would require expand-contract pattern:
1. Add `CategoryId` as NULLABLE
2. Create default "Uncategorized" category per tenant
3. Backfill all existing products
4. Alter column to NOT NULL
5. Add FK constraint

**Lesson:** Migrations that add required foreign keys to existing data
cannot be done in one step. They require a phased approach.

This is a **top-5 production incident pattern**. Documented here so we
never forget.

---

## ✅ End-to-End Verification

Tested with 3 tenants (Blinkit, Zepto, Instamart):

| Test | Result |
|---|---|
| Create Category (Dairy, Chilled) as Blinkit | 201 Created |
| Create Warehouse (Blinkit-Koramangala-01) as Blinkit | 201 Created |
| Create Product linked to Category | 201 Created |
| Postgres JOIN (Product ↔ Category) | Confirmed FK relationship |
| Zepto's category list | Empty (isolation works) |

**All 5 tests passed.**

---

## 🎓 Lessons Learned

### 1. Migration failures leave half-applied state

When a migration fails mid-way, the schema is partially updated. We had
to manually clean up and retry. In production, this is a critical
incident that requires investigation and careful rollforward.

### 2. Unique indexes with tenant scope

Category names are unique **per tenant**, not globally. Composite index
`(TenantId, Name)` allows two tenants to have a category called "Dairy"
without conflict. Same for Warehouse codes.

### 3. FK delete behavior matters

`OnDelete(DeleteBehavior.Restrict)` prevents deleting a category while
products reference it. Default `Cascade` would silently delete products.

### 4. Temperature zones are just the beginning

We added the enum and the entities. The actual *enforcement* (rejecting
moves where zones don't match) comes in a later phase with `StockLevel`
and `StockMovement`.

### 5. The dependency rule keeps paying off

`CategoryService` and `WarehouseService` live in Infrastructure because
they use `SmartInventoryDbContext` (EF Core). Their interfaces
(`ICategoryService`, `IWarehouseService`) live in Core. Controllers in
API depend only on the interfaces. **Zero infrastructure leakage into
controllers or Core.**

---

## 🎯 Next Phase

**Phase 3.7.2 — Batches and Stock Levels**
- `Batch` — specific production lot with expiry date
- `StockLevel` — aggregate quantity per Product × Warehouse
- Begin modeling perishability
- First hints of FEFO logic

---

## 📊 Git History

```
(new) 5d7f7049 feat(domain): add Category and Warehouse for cold-chain
      791d16b  feat(multi-tenancy): add tenant isolation foundation
      c02b3ce  feat(api): harden CreateProductRequest validation
      513d788  feat(api): add Products API endpoints with Swagger fix + dev journal
      f04c0f1  feat(data): add Product entity, DbContext, and initial migration
      ce4b744  chore: bootstrap solution with API, Core, Infrastructure, and Tests
      69284fb  docs: add README with project overview and structure
```

---

*End of Phase 3.7.1 journal entry.*