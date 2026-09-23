# Smart Inventory System

A production-grade .NET 8 solution exploring modern architecture patterns for building resilient, observable, and scalable backends.

## 🎯 Purpose

This is a **living reference architecture** — not a tutorial. Every design decision here is deliberate, documented, and driven by real-world constraints like:

- Domain complexity (DDD, bounded contexts)
- Distributed system failure modes (resilience patterns)
- Operational visibility (observability)
- Delivery speed with quality (CI/CD, DORA metrics)

## 🏗️ Architecture

- **Domain-Driven Design** — bounded contexts, aggregates, ubiquitous language
- **CQRS** — separate read/write models with MediatR
- **Event-Driven** — RabbitMQ for async communication, transactional outbox
- **Resilience** — Polly for circuit breakers, retries, bulkheads
- **Observability** — OpenTelemetry, structured logging, distributed tracing
- **Cloud-Native** — Docker, IaC, container orchestration

## 🧱 Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 |
| API | ASP.NET Core Web API |
| Data | PostgreSQL + EF Core |
| Cache | Redis |
| Messaging | RabbitMQ |
| Observability | OpenTelemetry, Serilog |
| Resilience | Polly |
| Testing | xUnit, Testcontainers, BenchmarkDotNet |
| DevOps | Docker, GitHub Actions |

## 📁 Project Structure
smart-inventory-system/
```
smart-inventory-system/
├── src/
│   ├── SmartInventory.Api/              # HTTP endpoints, DI setup
│   ├── SmartInventory.Core/             # Domain models, interfaces, business logic
│   └── SmartInventory.Infrastructure/   # EF Core, external services, persistence
├── tests/
│   └── SmartInventory.UnitTests/        # Unit tests
└── docker-compose.yml                   # Local infra (Postgres, Redis, RabbitMQ)
```


## 🚀 Status

- [x] Phase 0: Solution bootstrap
- [ ] Phase 1: Docker infrastructure + EF Core
- [ ] Phase 2: Domain model + first endpoint
- [ ] Phase 3: Resilience patterns
- [ ] Phase 4: Observability
- [ ] Phase 5: CI/CD + deployment

## 📜 License

MIT — see [LICENSE](LICENSE)
