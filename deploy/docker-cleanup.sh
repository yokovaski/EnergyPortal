#!/usr/bin/env bash
# Weekly docker cleanup for the server. Safe replacement for `docker system prune -a`:
# - removes only untagged (dangling) images and build cache older than a week;
# - keeps every tagged image (backup-tools, base images, the current deploy);
# - keeps the 3 newest pulled sha-* tags per ghcr.io app image (rollback points), removes older ones;
# - keeps the 2 newest rollback tags (pre-*) of the old, locally built images, removes older ones;
# - never touches volumes or containers (an image a container still uses is left alone).
# Crontab on the server:
#   30 4 * * 0 $HOME/docker-cleanup.sh >> $HOME/docker-cleanup.log 2>&1
set -euo pipefail

echo "$(date -Is) start"
# docker images lists newest first
for repo in ghcr.io/yokovaski/energyportal-portal ghcr.io/yokovaski/energyportal-worker; do
  docker images "$repo" --format '{{.Tag}}' | { grep '^sha-' || true; } | tail -n +4 | while read -r tag; do
    docker rmi "$repo:$tag" || true
  done
done
for repo in energy_portal energy_worker; do
  docker images "$repo" --format '{{.Tag}}' | { grep '^pre-' || true; } | tail -n +3 | while read -r tag; do
    docker rmi "$repo:$tag" || true
  done
done
docker image prune -f --filter until=168h
docker builder prune -f --filter until=168h
df -h / | tail -1
