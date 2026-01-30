@echo off
setlocal enabledelayedexpansion

echo ============================
echo BACKUP -> RESTORE -> Docker
echo ============================

REM Folder backup
if not exist database mkdir database

set MYSQL_BACKUP=database\mysql_dump.sql
set REDIS_BACKUP=database\redis_dump.rdb

REM ======================
REM STOP CONTAINERS
REM ======================
echo.
echo [System] TERMINATING DOCKER CONTAINERS...
docker compose down

REM ======================
REM MYSQL RESTORE
REM ======================
if exist %MYSQL_BACKUP% (
    echo.
    echo [MySQL] BACKUP FOUND -> RESTORE...

    docker volume rm %cd%_mysql-data 2>nul

) else (
    echo.
    echo [MySQL] NO BACKUP -> START CLEAN
)

REM ======================
REM REDIS RESTORE
REM ======================
if exist %REDIS_BACKUP% (
    echo.
    echo [Redis] BACKUP FOUND -> RESTORE...

    docker volume rm %cd%_redis-data 2>nul

) else (
    echo.
    echo [Redis] NO BACKUP -> START CLEAN
)

REM ======================
REM START SYSTEMU
REM ======================
echo.
echo [System] LAUNCHING DOCKER CONTAINERS...
docker compose up -d

REM ======================
REM REDIS LOAD
REM ======================
if exist %REDIS_BACKUP% (
    echo.
    echo [Redis] LOADING DUMP TO VOLUME...

    timeout /t 5 > nul
    docker cp database/redis_dump.rdb redis-cache:/data/dump.rdb
    docker restart redis-cache
)

echo.
echo ============
echo SYSTEM READY
echo ============
pause