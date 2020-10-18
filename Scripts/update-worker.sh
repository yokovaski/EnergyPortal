#!/bin/bash

cd /home/erwin/EnergyPortal/EnergyWorker
git pull
sudo service energyworker stop
sudo cp /var/www/EnergyWorker/appsettings.* /tmp
sudo dotnet publish --configuration Release -o /var/www/EnergyWorker
sudo cp /tmp/appsettings.* /var/www/EnergyWorker
sudo rm /tmp/appsettings.*
sudo chown -R www-data:www-data /var/www/EnergyWorker/
sudo service energyworker start
cd /home/erwin
