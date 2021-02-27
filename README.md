# Installing this application on a server
# Prerequisities
Install docker and docker-compose

https://github.com/docker/docker-install

https://docs.docker.com/compose/install/

## Instructions for debian
Run the command in a terminal after logging into your server

```
sudo apt-get update && sudo apt-get install certbot -y

sudo certbot certonly --standalone --preferred-challenges http -d YOUR_DOMAIN_NAME
```

This will generate a cert for your domain


changes the env variables in docker-compose.yml based on your domain name

- ASPNETCORE_Kestrel__Certificates__Default__Path
- ASPNETCORE_Kestrel__Certificates__Default__KeyPath


then run the command below and your application should start
```
sudo docker-compose build
sudo docker-compose up -d
```

To stop the app from running

```
sudo docker-compose down
```