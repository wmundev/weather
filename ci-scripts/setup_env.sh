#!/bin/bash
# Exit bash script on any command that returns a non zero error code
set -e

#sed -i "s/{{OPEN_WEATHER_API_KEY}}/${OPEN_WEATHER_API_KEY}/g" weather-backend/appsettings.json
#sed -i "s/{{SMTP_USERNAME}}/${SMTP_USERNAME}/g" weather-backend/appsettings.json
#sed -i "s/{{SMTP_PASSWORD}}/${SMTP_PASSWORD}/g" weather-backend/appsettings.json
#sed -i "s/{{DB_PASSWORD}}/${DB_PASSWORD}/g" weather-backend/appsettings.json
#sed -i "s/{{DB_USER}}/${DB_USER}/g" weather-backend/appsettings.json
#sed -i "s/{{DB_DATABASE}}/${DB_DATABASE}/g" weather-backend/appsettings.json

sed -i "s|{{REPLACE_ME_CONFIG_CAT_SDK_KEY}}|${CONFIG_CAT_SDK_KEY_PROD}|g" weather-backend/appsettings.Development.json
sed -i "s|{{REPLACE_ME_CONFIG_CAT_SDK_KEY}}|${CONFIG_CAT_SDK_KEY_PROD}|g" weather-backend/appsettings.Production.json

# for docker compose config
#echo "$DB_PASSWORD" >> weather-backend/POSTGRES_PASSWORD.txt
#echo "$DB_USER" >> weather-backend/POSTGRES_USER.txt
#echo "$DB_DATABASE" >> weather-backend/POSTGRES_DB.txt
# for nginx ssl
#echo VIRTUAL_HOST="$VIRTUAL_HOST" >> weather-backend/.env
#echo VIRTUAL_PORT="$PORT" >> weather-backend/.env
#echo LETSENCRYPT_HOST="$VIRTUAL_HOST" >> weather-backend/.env
#echo LETSENCRYPT_EMAIL="$LETSENCRYPT_EMAIL" >> weather-backend/.env
# for aspnet core app
#echo ASPNETCORE_URLS="http://+:${PORT}" >> weather-backend/.env
#echo AWS_ACCESS_KEY_ID="$AWS_ACCESS_KEY_ID" >> weather-backend/.env
#echo AWS_SECRET_ACCESS_KEY="$AWS_SECRET_ACCESS_KEY" >> weather-backend/.env
