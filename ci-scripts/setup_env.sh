sed -i "s/{{OPEN_WEATHER_API_KEY}}/${OPEN_WEATHER_API_KEY}/g" weather-backend/appsettings.json
sed -i "s/{{SMTP_USERNAME}}/${SMTP_USERNAME}/g" weather-backend/appsettings.json
sed -i "s/{{SMTP_PASSWORD}}/${SMTP_PASSWORD}/g" weather-backend/appsettings.json
sed -i "s/{{DB_PASSWORD}}/${DB_PASSWORD}/g" weather-backend/appsettings.json
sed -i "s/{{DB_USER}}/${DB_USER}/g" weather-backend/appsettings.json
sed -i "s/{{DB_DATABASE}}/${DB_DATABASE}/g" weather-backend/appsettings.json
# for docker compose config
echo $DB_PASSWORD >> weather-backend/POSTGRES_PASSWORD.txt
echo $DB_USER >> weather-backend/POSTGRES_USER.txt
echo $DB_DATABASE >> weather-backend/POSTGRES_DB.txt