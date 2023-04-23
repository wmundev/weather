# Initialise database
See https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver16&pivots=cs1-bash

```
sudo docker exec -it sqlserver_ms_sql_db_1 "bash"
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "<YourNewStrong@Passw0rd>"
```

Run this commands when in sql mode
```
CREATE DATABASE web
GO

USE web
```

then copy paste the create table from [here](sql/student.sql)

then run 
```
GO
```

to create the table

then insert the data using the insert statement

then run
```
GO
```
