#!/usr/bin/env bash
# Pulls a backup of a docker host into a local borg repo. Runs hourly from cron on the backup machine.
# Pull, not push: the server holds no credentials for the backup machine, so a compromised server cannot touch the backups.
# Restoring is described in RESTORE.md.
#
# What to back up is defined in ~/backup/backup.conf (see backup.conf.example). It stays out of git.
#
# Server prerequisite (helper image with rsync + sqlite3):
#   printf "FROM alpine:3.20\nRUN apk add --no-cache rsync sqlite\n" | docker build -t backup-tools -
# Backup machine crontab:
#   17 * * * * $HOME/backup/borg-pull-backup.sh >> $HOME/backup/backup.log 2>&1
set -euo pipefail

BASE=$HOME/backup
STAGE=$BASE/staging
export BORG_PASSCOMMAND="cat $HOME/.config/borg/passphrase"
# shellcheck source=backup.conf.example
source "$BASE/backup.conf"
: "${SERVER:?set SERVER in backup.conf}" "${BORG_REPO:?set BORG_REPO in backup.conf}"
export BORG_REPO

mkdir -p "$STAGE"
exec 9>"$BASE/.lock"
flock -n 9 || { echo "$(date -Is) previous run still busy, skipping"; exit 1; }
# Marker for the offsite copy, which pulls this staging folder: present only after a fully successful run.
rm -f "$STAGE/.complete"

remote() { ssh -o BatchMode=yes "$SERVER" "$@"; }

# No container keeps the helper image in use, so `docker system prune -a` on the server deletes it: rebuild when missing.
remote 'docker image inspect backup-tools >/dev/null 2>&1 || printf "FROM alpine:3.20\nRUN apk add --no-cache rsync sqlite\n" | docker build -q -t backup-tools - >/dev/null'

# Plain SQL, not gzipped, so borg can deduplicate unchanged data between hours.
# $1 container, $2 database superuser, $3 file name in staging.
dump_postgres() { remote docker exec "$1" pg_dumpall -U "$2" > "$STAGE/$3"; }

# SQLite in WAL mode: copying the raw files of a live db can be inconsistent, use the online backup API.
# $1 volume, $2 db path inside the volume, $3 file name in staging.
dump_sqlite() {
  remote "docker run --rm -v $1:/v backup-tools sh -c \"sqlite3 /v/$2 '.backup /tmp/b.db' && cat /tmp/b.db\"" > "$STAGE/$3"
}

# Volumes and some host paths are only readable by root on the server, so rsync runs inside a container that mounts them.
# $1 volume name or absolute host path, $2 folder in staging, rest extra rsync args.
pull_root() {
  local src=$1 dest=$STAGE/$2; shift 2
  mkdir -p "$dest"
  # --fake-super keeps the original owners as xattrs (we are not root here); borg stores them and RESTORE.md sends them back.
  rsync -a --fake-super --numeric-ids --delete "$@" -e "ssh -o BatchMode=yes" \
    --rsync-path="docker run --rm -i -v $src:/v:ro backup-tools rsync" "$SERVER:/v/" "$dest/"
}
pull_volume() { local vol=$1; shift; pull_root "$vol" "volumes/$vol" "$@"; }

# Paths relative to the server user's home (project folders incl. git-ignored .env files).
# Args starting with - go to rsync, so write options as one word: --exclude=/path/.
pull_home() {
  local args=() a
  for a in "$@"; do [[ $a == -* ]] && args+=("$a") || args+=("$SERVER:$a"); done
  mkdir -p "$STAGE/home"
  rsync -a --delete -e "ssh -o BatchMode=yes" --exclude node_modules/ --exclude bin/ --exclude obj/ "${args[@]}" "$STAGE/home/"
}

backup_items

cd "$STAGE"
borg create --compression zstd,6 '::{now:%Y-%m-%dT%H:%M}' .
# Last hourly + the last backup of each of the past 14 days + of each of the past 8 weeks.
borg prune --keep-hourly 1 --keep-daily 14 --keep-weekly 8
borg compact

date -Is > "$STAGE/.complete"
echo "$(date -Is) OK"
