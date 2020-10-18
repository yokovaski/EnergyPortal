#!/bin/bash

cd /home/erwin/EnergyPortal/EnergyPortal
git pull
npm install
npm run build
sudo service energyportal stop
sudo cp /var/www/EnergyPortal/appsettings.* /tmp
sudo dotnet publish --configuration Release -o /var/www/EnergyPortal
sudo cp /tmp/appsettings.* /var/www/EnergyPortal
sudo rm /tmp/appsettings.*
sudo cp wwwroot/js/bundle.js /var/www/EnergyPortal/wwwroot/js
sudo cp wwwroot/css/bundle.css /var/www/EnergyPortal/wwwroot/css
sudo chown -R www-data:www-data /var/www/EnergyPortal/
sudo service energyportal start
cd /home/erwin
