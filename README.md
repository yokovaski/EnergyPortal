# EnergyPortal

To be used together with EnergyReader

## Docker images

GitHub Actions (`.github/workflows/docker-images.yml`) builds both images on every push to `master`, and on demand
via *Actions → Docker images → Run workflow*. It pushes them to the GitHub Container Registry:

| Image | Tags |
|---|---|
| `ghcr.io/yokovaski/energyportal-portal` | `latest` (newest `master` build), `sha-<short commit>` |
| `ghcr.io/yokovaski/energyportal-worker` | `latest` (newest `master` build), `sha-<short commit>` |

The server never builds images: a build needs far more memory than the server has to spare next to the running
apps. `docker-compose.yml` therefore only references the images.

### Deploy

On the server, in the checkout:

```bash
git pull
docker compose pull energy_portal energy_worker
docker compose up -d energy_portal energy_worker
```

### Roll back or pin a version

Set `IMAGE_TAG` in `.env` to an earlier build, e.g. `IMAGE_TAG=sha-2c5cdd9`, and run the `pull` and `up -d` commands
above again. Remove the line to follow `latest` again. `deploy/docker-cleanup.sh` keeps the three newest pulled
`sha-*` tags on the server.

### Build locally

```bash
docker compose -f docker-compose.yml -f docker-compose.build.yml build
docker compose -f docker-compose.yml -f docker-compose.build.yml up -d
```

## Run with Docker Compose

### Prerequisites

First prepare a disk for persistent storage. In this example we use `/mnt/disk1`. 
Make sure the disk is formatted (e.g. ext4). To find the UUID of the disk, use 

```bash
sudo lsblk -o UUID,NAME,FSTYPE,SIZE,MOUNTPOINT,LABEL,MODEL
```

Copy the UUID of the disk you want to use and edit the fstab file:

```bash
sudo nano /etc/fstab
```

Add the following line to the end of the file (replace `<UUID-HERE>` with the actual UUID of your disk):

```
UUID=<UUID-HERE> /mnt/disk1 ext4 defaults,auto,users,rw,nofail 0 0
```

Create the mount point and mount the disk:

```bash
sudo mount -a
sudo systemctl daemon-reload
```

Navigate to the disk and create the necessary directories:

```bash
cd /mnt/disk1
```

Make sure you own the disk:

```bash
sudo chown -R $USER:$USER /mnt/disk1
```

Now create the required directories:

```bash
mkdir postgresql
mkdir energycloud
mkdir energycloud/portal
mkdir energycloud/portal/dev
```

It might be necessary to adjust permissions for the directories, e.g. `chmod 777 /mnt/disk1/energycloud/portal/dev`

### Setting up Docker Compose

```bash
cd ~
mkdir energycloud
cd energycloud
nano docker-compose.yml
```

Copy and paste the following into `docker-compose.yml`:

```yaml
services:

  energy_portal:
    image: yokovaski/energycloud_portal:latest
    ports:
      - "80:8080"
      - "443:8081"
    depends_on:
      - "postgres"
    networks:
      - portal-network
    volumes:
      - /mnt/disk1/energycloud/portal/dev:/var/dpkeys:rw

  energy_worker:
    image: yokovaski/energycloud_worker:latest
    depends_on:
      - "postgres"
    networks:
      - worker-network

  postgres:
    container_name: 'postgres'
    image: postgres
#    ports:
#      - "5432:5432"
    networks:
      - portal-network
      - worker-network
    environment:
      POSTGRES_USER: "energycloud"
      POSTGRES_PASSWORD: "energycloud"
      POSTGRES_DB: "energycloud"
    restart: always
    volumes:
      - /mnt/disk1/postgresql/energycloud_dev:/var/lib/postgresql/data

networks:
  portal-network:
    driver: bridge
  worker-network:
    driver: bridge

volumes:
  portal_volume:
```

Now start the services:

```bash
docker compose up -d
```

### E-mail (password reset and address confirmation)

The portal sends account mails ("Wachtwoord vergeten", confirming a (new) e-mail address) over SMTP. Copy
`.env.example` to `.env` next to `docker-compose.yml` and fill in the `SMTP_*` values:

```bash
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587            # 587 = STARTTLS, 465 = implicit TLS
SMTP_USER=you@gmail.com
SMTP_PASSWORD=<app password>
SMTP_FROM=               # optional, defaults to SMTP_USER
```

For Gmail, turn on 2-step verification for the account and create an app password at
<https://myaccount.google.com/apppasswords>. Use that, not the normal account password. Then restart with
`docker compose up -d`. Without these settings no mail is sent, and the pages tell the user that e-mail is
not available right now. Send failures are logged by the `energy_portal` container.

### Check logs

```bash
docker compose logs -f
```

### Update

```bash
docker compose down
docker compose pull
docker compose up -d
```
