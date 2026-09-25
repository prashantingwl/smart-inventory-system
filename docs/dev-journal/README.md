# Dev Journal

A running log of the engineering decisions, bugs, and lessons learned while
building SmartInventory. Each entry is a real story: what I tried, what
failed, and what I learned.

## Purpose

This journal serves three purposes:

1. **Personal reference** — when I hit a similar issue in 6 months, I have a
   written record of how I solved it.
2. **Portfolio signal** — shows how I approach problems, not just the final
   code. Recruiters and hiring managers value process over polish.
3. **Interview prep** — every entry is a story I can tell in a real interview.

## Entries

| Date | Topic | Tags |
|---|---|---|
| 2026-09-24 | [Day 1: Environment Setup & Infrastructure Bootstrap](./2026-09-24-day-1-environment-setup.md) | `dotnet` `docker` `postgres` `setup` |
| 2026-09-24 | [Day 1: Swagger OpenAPI 3.0 vs 3.1 Mismatch](./2026-09-24-day-1-swagger-fix.md) | `swagger` `middleware` `aspnet-core` |
| 2026-09-25 | [Day 2: Multi-Tenancy Foundation](./2026-09-25-day-2-multi-tenancy-foundation.md) | `multi-tenancy` `ef-core` `interceptors` `ddd` |
| 2026-09-25 | [Day 2: Cold-Chain Domain — Category & Warehouse](./2026-09-25-day-2-coldchain-domain.md) | `ddd` `cold-chain` `ef-core` `migration` |

## How to Read

- **Skim** the tables for symptom → fix.
- **Deep-read** the "Lessons Learned" section — that's where the value is.

## Format

Each entry follows a consistent structure:

- **Context** — what I was trying to do
- **Symptom** — what went wrong
- **Root Cause** — the actual underlying issue
- **Fix** — the solution (with code)
- **Lessons Learned** — generalizable principles
- **References** — links for future reading