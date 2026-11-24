# MySQL dump.sql -> PostgreSQL migration checklist (single shared PostgreSQL)

This setup uses one PostgreSQL instance: the `postgres` service from the root [docker-compose.yml](docker-compose.yml).
The migration stack only runs MySQL source + pgloader.

## 1) Start services

From repository root:

```powershell
docker compose up -d postgres
docker compose -f deploy/migration/docker-compose.migration.yml up -d mysql-source
```

```bash
docker compose up -d postgres
docker compose -f deploy/migration/docker-compose.migration.yml up -d mysql-source
```

Check health/status:

```powershell
docker compose ps
docker compose -f deploy/migration/docker-compose.migration.yml ps
```

```bash
docker compose ps
docker compose -f deploy/migration/docker-compose.migration.yml ps
```

## 2) Restore dump.sql into MySQL source

```powershell
Get-Content -Path dump.sql | docker compose -f deploy/migration/docker-compose.migration.yml exec -T mysql-source mysql -uenergycloud -penergycloud energycloud
```

PowerShell alternative:

```powershell
cmd /c "docker compose -f deploy/migration/docker-compose.migration.yml exec -T mysql-source mysql -uenergycloud -penergycloud energycloud < dump.sql"
```

```bash
docker compose -f deploy/migration/docker-compose.migration.yml exec -T mysql-source mysql -uenergycloud -penergycloud energycloud < dump.sql
```

Quick source check:

```powershell
docker compose -f deploy/migration/docker-compose.migration.yml exec mysql-source mysql -uenergycloud -penergycloud -e "SELECT COUNT(*) AS users_count FROM energycloud.AspNetUsers;"
```

```bash
docker compose -f deploy/migration/docker-compose.migration.yml exec mysql-source mysql -uenergycloud -penergycloud -e "SELECT COUNT(*) AS users_count FROM energycloud.AspNetUsers;"
```

## 3) Run pgloader (MySQL -> PostgreSQL)

```powershell
docker compose -f deploy/migration/docker-compose.migration.yml run --rm pgloader pgloader /work/pgloader.load
```

```bash
docker compose -f deploy/migration/docker-compose.migration.yml run --rm pgloader pgloader /work/pgloader.load
```

## 3.5) Align imported data with EF migration schema (recommended)

`pgloader` loads staging tables in schema `energycloud` with MySQL-style naming. Your app expects EF migration schema in `public`.

1. Apply EF migrations to target PostgreSQL.

```powershell
Set-Content -Path MigrateDatabase/connectionstring.txt -Value "Host=localhost;Port=5432;Database=energycloud;Username=energycloud;Password=energycloud"
dotnet run --project MigrateDatabase/MigrateDatabase.csproj
```

```bash
printf 'Host=localhost;Port=5432;Database=energycloud;Username=energycloud;Password=energycloud' > MigrateDatabase/connectionstring.txt
dotnet run --project MigrateDatabase/MigrateDatabase.csproj
```

2. Copy staging data into EF tables.

```powershell
Get-Content -Path deploy/migration/staging-to-ef.sql | docker compose exec -T postgres psql -U energycloud -d energycloud -v ON_ERROR_STOP=1
```

```bash
docker compose exec -T postgres psql -U energycloud -d energycloud -v ON_ERROR_STOP=1 < deploy/migration/staging-to-ef.sql
```

If you already imported data and get `Numeric value does not fit in a System.Decimal`, normalize settings decimal precision:

```powershell
Get-Content -Path deploy/migration/fix-settings-decimals.sql | docker compose exec -T postgres psql -U energycloud -d energycloud -v ON_ERROR_STOP=1
```

```bash
docker compose exec -T postgres psql -U energycloud -d energycloud -v ON_ERROR_STOP=1 < deploy/migration/fix-settings-decimals.sql
```

## 4) Validate migration

List all non-system tables:

```powershell
docker compose exec -T postgres psql -U energycloud -d energycloud -c "SELECT schemaname, tablename FROM pg_catalog.pg_tables WHERE schemaname NOT IN ('pg_catalog','information_schema') ORDER BY schemaname, tablename;"
```

```bash
docker compose exec -T postgres psql -U energycloud -d energycloud -c "SELECT schemaname, tablename FROM pg_catalog.pg_tables WHERE schemaname NOT IN ('pg_catalog','information_schema') ORDER BY schemaname, tablename;"
```

Row count snapshot (source MySQL):

```powershell
docker compose -f deploy/migration/docker-compose.migration.yml exec mysql-source mysql -uenergycloud -penergycloud -N -e "
SELECT 'AspNetUsers', COUNT(*) FROM energycloud.AspNetUsers
UNION ALL SELECT 'AspNetRoles', COUNT(*) FROM energycloud.AspNetRoles
UNION ALL SELECT 'RaspberryPis', COUNT(*) FROM energycloud.RaspberryPis
UNION ALL SELECT 'TenSecondMetrics', COUNT(*) FROM energycloud.TenSecondMetrics
UNION ALL SELECT 'MinuteMetrics', COUNT(*) FROM energycloud.MinuteMetrics
UNION ALL SELECT 'HourMetrics', COUNT(*) FROM energycloud.HourMetrics
UNION ALL SELECT 'Settings', COUNT(*) FROM energycloud.Settings;"
```

```bash
docker compose -f deploy/migration/docker-compose.migration.yml exec mysql-source mysql -uenergycloud -penergycloud -N -e "
SELECT 'AspNetUsers', COUNT(*) FROM energycloud.AspNetUsers
UNION ALL SELECT 'AspNetRoles', COUNT(*) FROM energycloud.AspNetRoles
UNION ALL SELECT 'RaspberryPis', COUNT(*) FROM energycloud.RaspberryPis
UNION ALL SELECT 'TenSecondMetrics', COUNT(*) FROM energycloud.TenSecondMetrics
UNION ALL SELECT 'MinuteMetrics', COUNT(*) FROM energycloud.MinuteMetrics
UNION ALL SELECT 'HourMetrics', COUNT(*) FROM energycloud.HourMetrics
UNION ALL SELECT 'Settings', COUNT(*) FROM energycloud.Settings;"
```

Row count snapshot (EF target in PostgreSQL `public`):

```powershell
docker compose exec postgres psql -U energycloud -d energycloud -At -c "
SELECT 'AspNetUsers', COUNT(*) FROM \"AspNetUsers\"
UNION ALL SELECT 'AspNetRoles', COUNT(*) FROM \"AspNetRoles\"
UNION ALL SELECT 'raspberry_pis', COUNT(*) FROM public.raspberry_pis
UNION ALL SELECT 'ten_second_metrics', COUNT(*) FROM public.ten_second_metrics
UNION ALL SELECT 'minute_metrics', COUNT(*) FROM public.minute_metrics
UNION ALL SELECT 'hour_metrics', COUNT(*) FROM public.hour_metrics
UNION ALL SELECT 'settings', COUNT(*) FROM public.settings;"
```

```bash
docker compose exec postgres psql -U energycloud -d energycloud -At -c "
SELECT 'AspNetUsers', COUNT(*) FROM \"AspNetUsers\"
UNION ALL SELECT 'AspNetRoles', COUNT(*) FROM \"AspNetRoles\"
UNION ALL SELECT 'raspberry_pis', COUNT(*) FROM public.raspberry_pis
UNION ALL SELECT 'ten_second_metrics', COUNT(*) FROM public.ten_second_metrics
UNION ALL SELECT 'minute_metrics', COUNT(*) FROM public.minute_metrics
UNION ALL SELECT 'hour_metrics', COUNT(*) FROM public.hour_metrics
UNION ALL SELECT 'settings', COUNT(*) FROM public.settings;"
```

Sequence sanity:

```powershell
docker compose exec postgres psql -U energycloud -d energycloud -c "SELECT pg_get_serial_sequence('public.raspberry_pis', 'id');"
```

```bash
docker compose exec postgres psql -U energycloud -d energycloud -c "SELECT pg_get_serial_sequence('public.raspberry_pis', 'id');"
```

## 5) Cleanup

```powershell
docker compose -f deploy/migration/docker-compose.migration.yml down -v
```

```bash
docker compose -f deploy/migration/docker-compose.migration.yml down -v
```
