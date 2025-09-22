# EnergyPortal

To be used together with EnergyReader

## Docker EnergyPortal

To build and push the Docker image for EnergyPortal, use the following commands (replace `<version>` with the desired 
version tag):

```bash
docker build -t energyportal -f EnergyPortal/Dockerfile .
docker tag energyportal:latest yokovaski/energycloud_portal:<version>-arm64
docker push yokovaski/energycloud_portal:<version>-arm64
```

## Docker EnergyWorker

To build and push the Docker image for EnergyWorker, use the following commands (replace `<version>` with the desired
version tag):

```bash
docker build -t energyworker -f EnergyWorker/Dockerfile .
docker tag energyworker:latest yokovaski/energycloud_worker:<version>-arm64
docker push yokovaski/energycloud_worker:<version>-arm64
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
