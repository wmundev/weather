# Installing this application on a server
# Prerequisities
Install docker and docker-compose

https://github.com/docker/docker-install

https://docs.docker.com/compose/install/

# Deploying (new)
Run the command
```
cd .\weather-backend
dotnet aws deploy --deployment-project ../weather-backend.Deployment
```

We use this tool to deploy to aws to AWS ECS - https://github.com/aws/aws-dotnet-deploy

## Instructions for debian deploy (deprecated)
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

## Installing git hooks
There are git hooks being used in this project. To activate it, run the command in the root of this repository.

```
git config core.hooksPath .githooks
```

## Installing AWS Profile
ssh into the server and run

```
aws --profile default configure set aws_access_key_id "my-20-digit-id"
```

```
aws --profile default configure set aws_secret_access_key "my-40-digit-secret-key"
```

replacing the `"my-20-digit-id"` and `"my-40-digit-secret-key"` with the correct id and password before running the command, it can be found via the terraform stack that created this user in the folder `weather-infra`