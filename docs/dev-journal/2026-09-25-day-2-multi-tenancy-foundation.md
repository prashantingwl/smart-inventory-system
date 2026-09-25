# Dev Journal — 2026-09-25 (Day 2)
## Multi-Tenancy Foundation — Phase 3.6 Complete

**Context:** First phase of the pivot to multi-tenant cold-chain
Q-commerce. Goal: prove tenant isolation works end-to-end before
adding more domain complexity.

**Time:** ~3 hours (including research + design discussion).

---

## 🎯 What We Built

### Entities
- `Tenant` — Id, Name, Slug, IsActive, CreatedAtUtc
- `Product` — now has `TenantId` + `Tenant` navigation
- `Product` implements `ITenantEntity`

### Core Abstractions
- `ITenantEntity` — marker interface for tenant-scoped entities
- `ITenantProvider` — resolve current tenant
- `TenantContext` — scoped data holder

### Infrastructure
- `TenantProvider` — reads from `TenantContext` (no HTTP dependency)
- `TenantInterceptor` — `SaveChangesInterceptor` auto-stamps TenantId on inserts
- `SmartInventoryDbContext` — global query filter on Product

### API
- `TenantResolutionMiddleware` — reads `X-Tenant-Id` header, sets TenantContext
- DI registrations in `Program.cs`

### Database
- Fresh `MultiTenancyFoundation` migration
- `Tenants` table
- `Products.TenantId` FK to `Tenants`
- Composite unique index `(TenantId, Sku)` — same SKU allowed across tenants

---

## 🧠 The Five Layers of Tenant Enforcement

Defense in depth — every layer assumes the previous might fail:

| Layer | Where | What |
|---|---|---|
| 1 | API middleware | Resolves tenant from HTTP header |
| 2 | Core `TenantContext` | Scoped per-request state |
| 3 | `TenantInterceptor` | Auto-stamps `TenantId` on inserts |
| 4 | `DbContext` query filter | Filters reads by tenant |
| 5 | Postgres FK constraint | Rejects invalid `TenantId` |

This is the difference between "we use TenantId columns" and
"we enforce multi-tenancy."

---

## ✅ End-to-End Verification

Seeded 3 tenants: Blinkit, Zepto, Instamart.

| Test | Result |
|---|---|
| POST as Blinkit | 201 Created, TenantId auto-stamped |
| POST as Zepto | 201 Created, TenantId auto-stamped |
| SQL SELECT | Both rows with correct TenantIds |
| GET as Blinkit | Only Blinkit's product |
| GET as Zepto | Only Zepto's product |
| GET as Instamart | Empty array |
| GET without header | 500 with clear error message |

**All 7 passed.** Tenant isolation works end-to-end.

---

## 🎓 Lessons Learned

### 1. The Dependency Rule paid off

`IHttpContextAccessor` (ASP.NET Core type) initially went into
`SmartInventory.Infrastructure` — a classlib. It failed to compile
because Infrastructure doesn't reference ASP.NET Core.

**Fix:** Introduced `TenantContext` in Core. The API layer sets it
from HTTP. Infrastructure just reads it. **No HTTP dependency
leaked into Infrastructure.**

This is what Clean Architecture buys you: the ability to keep
HTTP out of your domain and infrastructure layers.

### 2. Query filters + Interceptor = combo

Query filters alone only enforce reads. Interceptors alone only
enforce writes. **Together they cover both.** Single-layer solutions
always have gaps.

### 3. Row-level isolation is real

Same `Products` table serves all tenants. `WHERE TenantId = x` is
injected automatically. No separate schemas, no separate databases.
At our scale, this is optimal.

### 4. Composite unique indexes are subtle

Without `(TenantId, Sku)`, the first tenant to claim a SKU blocks
all others. **The composite allows same SKU across tenants** while
preventing duplicates within a tenant.

### 5. Silent success is dangerous

When the tenant header is missing, we **throw** rather than return
empty. Empty results look like "no data" — a security bug. A 500
with a clear message points directly at the missing tenant header.

---

## 🔒 Security Note — Trust Model

**Current state:** Tenant resolved from `X-Tenant-Id` HTTP header.
Any client can send any tenant ID. **Impersonation is trivial.**

**Why this is OK for now:**
- Solo development
- Local-only (localhost)
- The pattern is what matters, not the trust source

**The fix (Phase 6):**
Replace header with JWT claim:
1. Add JWT auth middleware
2. Server issues token with `tenant` claim
3. Token is signed — clients can't forge
4. Extract tenant from `HttpContext.User.FindFirst("tenant")`

**Impact on codebase:** ~4 lines in one middleware.
**Impact on rest of system:** None.

**This is the value of layering.** Tenant *source* is swappable.
Tenant *enforcement* (interceptor, query filter, FK) is untouched.

**Additional layers planned:**
- Tenant existence validation (reject ghost tenants)
- Rate limiting per tenant
- Audit logging
- Role-based access inside tenant

---

## ⚠️ Known Gaps (Planned)

| Gap | When we'll fix |
|---|---|
| No tenant-existence validation (fake GUIDs accepted) | Phase 3.7 |
| Header is trusted (no auth) | Phase 6 |
| 500 instead of 400 for missing tenant | Phase 3.7 |
| No `IgnoreQueryFilters()` policy for admin | Future |
| No tenant-level rate limiting | Future |

---

## 🎯 Next Phase

**Phase 3.7 — Cold-Chain Domain Expansion**
- `Category` (with TemperatureZone requirement)
- `Warehouse` (dark store with SupportedZone)
- `Batch` (with expiry date, quantity)
- `StockLevel` (aggregate per Product × Warehouse)
- `StockMovement` (immutable ledger)
- Replace generic Product schema with cold-chain model
- **FEFO picking logic**

---

## 📊 Git History

```
(new) feat(multi-tenancy): add tenant isolation foundation
c02b3ce feat(api): harden CreateProductRequest validation
513d788 feat(api): add Products API endpoints with Swagger fix + dev journal
f04c0f1 feat(data): add Product entity, DbContext, and initial migration
ce4b744 chore: bootstrap solution with API, Core, Infrastructure, and Tests
69284fb docs: add README with project overview and structure
```