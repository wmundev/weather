#!/bin/bash
cd /home/admin/weather-backend
sudo docker-compose build
sudo docker-compose down
sudo docker-compose pull
sudo docker-compose up -d
