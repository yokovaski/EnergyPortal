# Restoring from backup

Backups are made by `borg-pull-backup.sh` (hourly, on the backup machine) into a borg repository.
Each archive is a snapshot of the staging folder: database dumps plus copies of volumes, host config and project folders.
What exactly is in it is listed in `~/backup/backup.conf` on the backup machine.

Placeholders used below:

| Placeholder | Meaning |
|---|---|
| `<server>` | `user@host` of the server to restore to |
| `<archive>` | archive name from `borg list`, e.g. `2026-11-19T22:38` |
| `<volume>` | docker volume name, e.g. `energyportal_portal_volume` |
| `<app>.db` | SQLite database file name |

You need the borg **passphrase** (`~/.config/borg/passphrase` on the backup machine). Keep a copy outside the backup machines,
e.g. in a password manager, otherwise an offsite copy is useless when the backup machine is gone.

## 1. Get the files out of borg (on the backup machine)

```bash
export BORG_REPO=$HOME/backup/borg BORG_PASSCOMMAND="cat $HOME/.config/borg/passphrase"   # BORG_REPO: see backup.conf
borg list                                   # pick an archive
mkdir ~/restore && cd ~/restore
borg extract ::<archive>                    # everything
borg extract ::<archive> home/EnergyPortal/docker-compose.yml   # or just one file / folder
```

Archive layout:

| Path | Content |
|---|---|
| `*.sql` | `pg_dumpall` of a Postgres container (all databases + roles), plain SQL |
| `*.db` | SQLite databases, consistent copies |
| `volumes/<volume>/` | contents of docker volumes (original owners kept as xattrs) |
| `host/` | host paths such as `/etc/nginx` and `/etc/letsencrypt` (original owners kept as xattrs) |
| `home/` | project folders from the server user's home, incl. git-ignored `.env` files |

From an offsite copy: same commands, with `BORG_REPO=user@offsite-host:/path/to/repo`.

## 2. Restore a Postgres database

The dumps are made with `pg_dumpall`: they contain `CREATE ROLE`, `CREATE DATABASE` and all data.
Run them against the `postgres` maintenance database.

**Fresh, empty Postgres** (new server, or the data folder was lost): start only the database container, then:

```bash
ssh <server> 'docker exec -i postgres psql -q -U energycloud -d postgres' < energyportal.sql
```

Errors like `role "energycloud" already exists` and `database "energycloud" already exists` are expected:
the container creates those itself on first start. Output like `setval` and `set_config` is normal; any other `ERROR` is not.

**Over an existing database** (roll back to an earlier point): stop the apps first, drop the database, then restore as above.

```bash
ssh <server> 'cd ~/EnergyPortal && docker compose stop energy_portal energy_worker'
ssh <server> 'docker exec postgres psql -U energycloud -d postgres -c "DROP DATABASE energycloud WITH (FORCE)"'
ssh <server> 'docker exec -i postgres psql -q -U energycloud -d postgres' < energyportal.sql
ssh <server> 'cd ~/EnergyPortal && docker compose start energy_portal energy_worker'
```

Other apps' dumps work the same way: use their container name, superuser and database name.

## 3. Restore a docker volume

Runs rsync as root inside a throwaway container that mounts the volume, so the original owners are restored
(`--fake-super` reads them from the xattrs). Stop the container that uses the volume first.

```bash
cd ~/restore
rsync -a --fake-super --numeric-ids --delete -e ssh \
  --rsync-path="docker run --rm -i -v <volume>:/v backup-tools rsync" \
  volumes/<volume>/ <server>:/v/
```

Leave out `--delete` to only add files back. The volume is created if it does not exist.
`backup-tools` is the helper image from the header of `borg-pull-backup.sh`; build it first on a new server.

### SQLite databases

The `.db` file in the archive root is the consistent copy; the copy in the volume folder is excluded on purpose.
Stop the app, restore the volume as above, then put the database back and remove stale WAL files:

```bash
rsync -a -e ssh --rsync-path="docker run --rm -i -v <volume>:/v backup-tools rsync" <app>.db <server>:/v/<app>.db
ssh <server> 'docker run --rm -v <volume>:/v backup-tools sh -c "rm -f /v/<app>.db-wal /v/<app>.db-shm && sqlite3 /v/<app>.db \"pragma integrity_check\""'
```

## 4. Restore host files

Same trick with a bind mount, e.g. nginx config and certificates:

```bash
rsync -a --fake-super --numeric-ids -e ssh --rsync-path="docker run --rm -i -v /etc/nginx:/v backup-tools rsync" host/nginx/ <server>:/v/
rsync -a --fake-super --numeric-ids -e ssh --rsync-path="docker run --rm -i -v /etc/letsencrypt:/v backup-tools rsync" host/letsencrypt/ <server>:/v/
ssh <server> 'sudo nginx -t && sudo systemctl reload nginx'
```

Project folders (`home/`) are owned by the server user, plain rsync is enough:

```bash
rsync -a home/EnergyPortal/ <server>:EnergyPortal/
```

## 5. Rebuilding a whole server

1. New machine with docker, nginx and certbot; create the same user; add the backup machine's SSH key.
2. Build `backup-tools` (see the header of `borg-pull-backup.sh`).
3. Extract the latest archive (section 1) and copy back `home/` (section 4). This includes the `.env` files.
4. Per app: build the images and start **only the database container** (`docker compose up -d postgres`).
5. Restore the database dumps into the fresh databases (section 2).
6. Restore the volumes (section 3) and SQLite files.
7. Restore `/etc/nginx` and `/etc/letsencrypt` (section 4), then point DNS to the new machine.
8. Start everything (`docker compose up -d` per app) and check the sites.
9. Run `borg-pull-backup.sh` once by hand on the backup machine to confirm backups work against the new server.

## Restore drill

Do this a few times a year, so you know the backups work and the steps are still right:

```bash
cd ~/restore && borg extract ::<archive>
docker run -d --name restoretest -e POSTGRES_USER=energycloud -e POSTGRES_PASSWORD=x -e POSTGRES_DB=energycloud postgres:16
until docker exec restoretest pg_isready -q; do sleep 1; done; sleep 2
docker exec -i restoretest psql -q -U energycloud -d postgres < energyportal.sql
docker exec restoretest psql -U energycloud -d energycloud -c "select count(*) from hour_metrics"
docker rm -f restoretest && rm -rf ~/restore
```
