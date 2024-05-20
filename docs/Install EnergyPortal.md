# Install EnergyPortal

```
sudo apt update
sudo apt install postgresql
sudo -i -u postgres
createuser energycloud --interactive --pwprompt
createdb energycloud
```

# Run EnergyPortal

To run your application as a service on an Ubuntu server with systemd, you need to follow these steps:

1. Move the service file `energyportal.service` to the `/etc/systemd/system` directory. This is the directory where systemd looks for service files. You can do this with the following command:

    ```bash
    sudo mv energyportal.service /etc/systemd/system/energyportal.service
    ```
    
    Replace `/path/to/your/service/file` with the actual path to your service file.

2. Set the correct permissions for the service file:

    ```bash
    sudo chmod 644 /etc/systemd/system/energyportal.service
    ```

3. Reload the systemd daemon to recognize your new service:

    ```bash
    sudo systemctl daemon-reload
    ```

4. Enable your service to start on boot:

    ```bash
    sudo systemctl enable energyportal.service
    ```

5. Start your service:

    ```bash
    sudo systemctl start energyportal.service
    ```

6. Check the status of your service:

    ```bash
    sudo systemctl status energyportal.service
    ```
    
    This should return an output indicating that your service is running. If there are any issues, the output should give you some clues about what's going wrong.

7. If you need more information about your service, you can check the logs:

    ```bash
    sudo journalctl -u energyportal.service
    ```
    
    This will show you the logs for your service. You can use the `-f` flag to follow the logs in real time.

# Run EnergyWorker

To run your application as a service on an Ubuntu server with systemd, you need to follow these steps:

1. Move the service file `energyworker.service` to the `/etc/systemd/system` directory. This is the directory where systemd looks for service files. You can do this with the following command:

    ```bash
    sudo mv energyworker.service /etc/systemd/system/energyworker.service
    ```

   Replace `/path/to/your/service/file` with the actual path to your service file.

2. Set the correct permissions for the service file:

    ```bash
    sudo chmod 644 /etc/systemd/system/energyworker.service
    ```

3. Reload the systemd daemon to recognize your new service:

    ```bash
    sudo systemctl daemon-reload
    ```

4. Enable your service to start on boot:

    ```bash
    sudo systemctl enable energyworker.service
    ```

5. Start your service:

    ```bash
    sudo systemctl start energyworker.service
    ```

6. Check the status of your service:

    ```bash
    sudo systemctl status energyworker.service
    ```

   This should return an output indicating that your service is running. If there are any issues, the output should give you some clues about what's going wrong.

7. If you need more information about your service, you can check the logs:

    ```bash
    sudo journalctl -u energyworker.service
    ```

   This will show you the logs for your service. You can use the `-f` flag to follow the logs in real time.