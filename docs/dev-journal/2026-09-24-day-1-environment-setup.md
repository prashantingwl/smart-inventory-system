# Dev Journal — 2026-09-24 (Day 1)
## Environment Setup & Infrastructure Bootstrap

**Context:** First day of building the SmartInventory portfolio project.
Goal was to set up a professional .NET 8 + Docker + Postgres development environment
on a fresh Windows 11 machine (Ryzen 7 5800X, 16 GB RAM, 465 GB C: drive).

---

## 🎯 Session Summary

Went from "PC with Windows and VS Code" to "working .NET solution with
Postgres + Redis + RabbitMQ running in Docker, first EF Core migration applied,
pushed to GitHub."

**Total time: ~6 hours.** Includes ~2 hours of debugging a port collision issue.

---

## ✅ Milestones Achieved

1. Environment audit — discovered missing .NET SDK, broken Docker state, low disk space
2. Fixed .NET SDK PATH ordering (32-bit vs 64-bit conflict)
3. Freed ~45 GB by removing Ubisoft games + Downloads cleanup
4. Bootstrapped .NET solution with 4 projects (Api, Core, Infrastructure, UnitTests)
5. Pushed initial commit to GitHub portfolio repo
6. Spun up Postgres 16, Redis 7, RabbitMQ 3.13 in Docker
7. Wrote `Product` entity, `SmartInventoryDbContext`, and first EF Core migration
8. Created `Products` table in Postgres — verified via psql

---

## 🐛 Issues Hit & Fixes

### Issue 1: .NET SDK "not found" despite being installed

**Symptom:**
```
$ dotnet --version
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application '--version' does not exist.
  * You intended to execute a .NET SDK command:
      No .NET SDKs were found.
```

**Diagnosis:**
```
$ where.exe dotnet
C:\Program Files (x86)\dotnet\dotnet.exe   ← 32-bit, listed FIRST
C:\Program Files\dotnet\dotnet.exe         ← 64-bit, listed SECOND
```

The x86 PATH entry was winning. The 32-bit `dotnet.exe` couldn't resolve
SDKs installed to the x64 path.

**Root cause:** PATH ordering — Windows prioritizes earlier entries.

**Fix:**
1. `winget install --id Microsoft.DotNet.SDK.8 --source winget --force`
2. Manual PATH fix: move `C:\Program Files\dotnet\` above
   `C:\Program Files (x86)\dotnet\` in System Environment Variables → Path.
3. Restart all PowerShell sessions.

**Lesson learned:** Always verify `where.exe dotnet` returns the x64 path first.
Mixed 32/64-bit .NET installs are a common source of confusion.

---

### Issue 2: Disk space at 11.55 GB free (dangerously low)

**Diagnosis:**
```powershell
Get-ChildItem "$env:USERPROFILE" -Directory | ForEach-Object {
    $size = (Get-ChildItem $_.FullName -Recurse -File -ErrorAction SilentlyContinue |
             Measure-Object Length -Sum).Sum
    [PSCustomObject]@{ Folder = $_.Name; SizeGB = [math]::Round($size/1GB, 2) }
} | Sort-Object SizeGB -Descending
```

**Findings:**
- `C:\Program Files (x86)\Steam` — 169 GB
- `C:\Program Files (x86)\Ubisoft` — 45 GB
- `Downloads` — 34 GB
- `Videos` — 21 GB

**Fix:** Removed Ubisoft games (uninstalled via Ubisoft Connect launcher,
then manually cleaned residual folders). Result: 60.44 GB free.

**Commands used:**
```powershell
Start-Process "C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\Uninstall.exe" -Wait

Test-Path "C:\Program Files (x86)\Ubisoft"
Test-Path "C:\ProgramData\Ubisoft"
Test-Path "$env:LOCALAPPDATA\Ubisoft Game Launcher"
```

**Lesson learned:** Games are the #1 developer machine disk killer.
Move them to an external SSD or separate drive to keep C: clean.

---

### Issue 3: Git identity not set (commit failed)

**Symptom:**
```
Author identity unknown
*** Please tell me who you are.
fatal: unable to auto-detect email address
```

**Fix:**
```powershell
git config --global user.name "Prashant Singh"
git config --global user.email "your.email@example.com"
```

**Important:** Use the email registered on GitHub, otherwise commits won't
link to your profile and green contribution squares won't appear.

---

### Issue 4: GitHub signup rejected `@outlook.com` email

**Symptom:** "Email domain could not be verified" during signup.

**Cause:** GitHub's anti-spam blocks Outlook domain at signup.

**Fix:** Logged into existing GitHub account and added the email via
`Settings → Emails → Add email address`. Secondary email verification is
more permissive than signup.

**Lesson learned:** GitHub's signup risk engine is stricter than its
account-management flow.

---

### Issue 5: First `git push` rejected — "remote contains work you do not have"

**Symptom:**
```
! [rejected]        main -> main (fetch first)
error: failed to push some refs
```

**Cause:** GitHub created an initial commit (README) when the repo was created,
and my local repo had unrelated history.

**Fix (rebase — cleaner than merge):**
```powershell
git pull --rebase origin main
git push -u origin main
```

**Lesson learned:** When creating a GitHub repo, DON'T check "Initialize with
README" if you already have a local repo with commits.

---

### Issue 6: 🚨 THE BIG ONE — Port 5432 collision with native Postgres 9.5

**Symptom:** EF Core migration failed with "password authentication failed
for user 'smartinv'" from Windows, even though:
- Postgres container was healthy
- Same credentials worked via `docker exec`
- Password had been verified inside the container

**The debugging saga (this took ~2 hours):**

| Hypothesis | Test | Result |
|---|---|---|
| Wrong password in config | `docker exec printenv POSTGRES_PASSWORD` | Correct ✅ |
| Timing issue | Waited 30s, retried | Still failed ❌ |
| SCRAM channel binding issue | `SHOW password_encryption` | scram-sha-256 |
| SCRAM hash mismatch | Changed to md5, retried | Still failed ❌ |
| Stale volume | `docker compose down -v` + up | Still failed ❌ |
| pg_hba.conf rules | Inspected rules | Looked correct |
| Wrong port / DNS | `Test-NetConnection localhost 5432` | TCP succeeded |
| Native Postgres squatting | `Get-NetTCPConnection -LocalPort 5432` | **🎯 FOUND IT** |

**Root cause discovery:**
```powershell
Get-NetTCPConnection -LocalPort 5432 -State Listen | Select-Object LocalAddress,
    LocalPort, OwningProcess,
    @{N='ProcessName';E={(Get-Process -Id $_.OwningProcess).ProcessName}},
    @{N='ProcessPath';E={(Get-Process -Id $_.OwningProcess).Path}}
```

**Output:**
```
LocalAddress : ::
LocalPort    : 5432
OwningProcess: 6912
ProcessName  : postgres
ProcessPath  : C:\Program Files\PostgreSQL\9.5\bin\postgres.exe
```

A native Windows PostgreSQL 9.5 (installed years ago, forgotten) was listening
on port 5432. Docker couldn't bind. External connections hit the native
Postgres — which had no `smartinv` user — hence "password authentication failed."

**The fix:**
```powershell
Get-Service -Name "postgresql-x64-9.5" | Stop-Service -Force
Set-Service -Name "postgresql-x64-9.5" -StartupType Disabled

cd C:\dev\smart-inventory-system
docker compose down
docker compose up -d
Start-Sleep -Seconds 20

Get-NetTCPConnection -LocalPort 5432 -State Listen | Select-Object LocalAddress,
    OwningProcess, @{N='Name';E={(Get-Process -Id $_.OwningProcess).ProcessName}}
```

**Result:**
```
LocalAddress   OwningProcess   Name
::             16816           wslrelay
::             47124           com.docker.backend   ← SUCCESS
```

**Lesson learned (CRITICAL):**
> When a network service appears to be running but connections fail,
> **ALWAYS check what process actually owns the port.**
>
> The command: `Get-NetTCPConnection -LocalPort <port> -State Listen`
>
> A "ghost service" (old installs, disabled tools, forgotten daemons) can
> silently hold a port. The new service looks healthy but never serves.

---

## 🛠️ Tooling Decisions

| Decision | Choice | Why |
|---|---|---|
| IDE | VS Code | Lightweight, works fine for API dev, keeps CLI skills sharp |
| Runtime | .NET 8 (LTS) | Latest LTS, matches EF Core 8.x |
| Database | PostgreSQL 16 | Free, production-grade, Docker-friendly |
| Cache | Redis 7 | Industry standard |
| Messaging | RabbitMQ 3.13 | Free, has UI, well-documented |
| Container | Docker Desktop (WSL2) | Easiest Windows workflow |

**Deferred:**
- Visual Studio 2026 — not needed yet; VS Code covers current workflow

---

## 🔐 Security Notes (Deferred to Phase 4)

**Current state:** Credentials hardcoded in `docker-compose.yml` and
`appsettings.json`. Acceptable for **local solo dev** but not for:
- Team environments
- CI/CD pipelines
- Cloud deployments

**Future refactor (Phase 4):**
1. Extract to `.env` + `.env.example`
2. Add `.env` to `.gitignore`
3. Use `${VAR}` placeholders in `docker-compose.yml`
4. In cloud: use Azure Key Vault / AWS Secrets Manager

---

## 📊 Environment Baseline (End of Session)

**Machine:**
- Windows 11 Pro (build 26200)
- AMD Ryzen 7 5800X (8 core)
- 16 GB RAM
- 465 GB C: drive (~60 GB free after cleanup)

**Installed & Verified:**
```
Git             2.41.0
.NET SDK        8.0.425
Node.js         22.14.0
npm             10.9.2
Docker          28.1.1
Docker Compose  2.35.1-desktop.1
PostgreSQL      16.15 (in Docker)
Redis           7.x (in Docker)
RabbitMQ        3.13 (in Docker)
VS Code         (latest)
```

**Running Services (via Docker):**
- `smartinv-postgres` — port 5432 → 5432
- `smartinv-redis` — port 6379 → 6379
- `smartinv-rabbitmq` — ports 5672, 15672 → 5672, 15672

**Database state:**
- `smartinventory` DB created
- `Products` table created (migration `20260923193058_InitialCreate`)
- `__EFMigrationsHistory` table tracks applied migrations

---

## 🎓 Key Lessons from Day 1

1. **Check `where.exe <tool>` when a CLI tool behaves strangely** — often
   a PATH ordering issue, not a missing install.

2. **`Get-NetTCPConnection -LocalPort <port> -State Listen` is your best
   friend when network services misbehave.**

3. **A "healthy" container doesn't mean port forwarding works.** Healthchecks
   run *inside* the container. External reachability is separate.

4. **Docker Desktop + WSL2 hold onto RAM aggressively.** Use `wsl --shutdown`
   to reclaim memory. Consider `.wslconfig` limits.

5. **Free disk space is not optional.** Aim for 40+ GB free minimum on a dev
   machine.

6. **Rebase > Merge for pulling remote changes into local.** Keeps history
   linear and clean for portfolio repos.

7. **Never commit secrets — even dev ones — without acknowledging the pattern.**
   Document the future refactor.

8. **Debugging is a loop, not a leap.** Observe → hypothesize → test →
   eliminate → repeat.

---

*End of Day 1 environment setup journal.*