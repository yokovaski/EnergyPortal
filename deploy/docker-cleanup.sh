#!/usr/bin/env bash
# Weekly docker cleanup for the server. Safe replacement for `docker system prune -a`:
# - removes only untagged (dangling) images and build cache older than a week;
# - keeps every tagged image (backup-tools, base images, the current deploy);
# - keeps the 2 newest rollback tags (pre-*) per app image, removes older ones;
# - never touches volumes or containers.
# Crontab on the server:
#   30 4 * * 0 $HOME/docker-cleanup.sh >> $HOME/docker-cleanup.log 2>&1
set -euo pipefail

echo "$(date -Is) start"
for repo in energy_portal energy_worker; do
  # docker images lists newest first
  docker images "$repo" --format '{{.Tag}}' | { grep '^pre-' || true; } | tail -n +3 | while read -r tag; do
    docker rmi "$repo:$tag"
  done
done
docker image prune -f --filter until=168h
docker builder prune -f --filter until=168h
df -h / | tail -1
