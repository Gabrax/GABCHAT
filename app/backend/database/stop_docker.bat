@echo off
setlocal enabledelayedexpansion

echo =======================
echo STOP DOCKER + DB BACKUP
echo =======================

REM Folder backup
if not exist database mkdir database

set MYSQL_BACKUP=database\mysql_dump.sql
set REDIS_BACKUP=database\redis_dump.rdb

REM ======================
REM MYSQL BACKUP
REM ======================
echo.
echo [MySQL] Backup before shutdown...

docker exec mysql-db mysqldump -u chatuser -pchatpass chatdb -r /tmp/mysql_dump.sql
IF %ERRORLEVEL% NEQ 0 (
    echo BLAD: Dump MySQL failure!
    goto shutdown
)

docker cp mysql-db:/tmp/mysql_dump.sql %MYSQL_BACKUP%
IF %ERRORLEVEL% NEQ 0 (
    echo BLAD: Copying MySQL dump failure!
)

REM ======================
REM REDIS BACKUP
REM ======================
echo.
echo [Redis] Backup before shutdown...

docker exec redis-cache redis-cli SAVE
IF %ERRORLEVEL% NEQ 0 (
    echo BLAD: Redis SAVE failure!
    goto shutdown
)

docker cp redis-cache:/data/dump.rdb %REDIS_BACKUP%
IF %ERRORLEVEL% NEQ 0 (
    echo BLAD: Copying Redis dump failure!
)

REM ======================
REM SHUTDOWN
REM ======================
:shutdown
echo.
echo =============================
echo TERMINATING DOCKER CONTAINERS
echo =============================

docker compose down

echo.
echo ======================
echo BACKUP + SHUTDOWN DONE
echo ======================
pause
