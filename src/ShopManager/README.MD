## Start project localy
for Windows
```
./start.ps1

or

cd src
docker-compose -f docker-compose.misc.yml up -d
docker-compose --env-file ./.env.win up
docker-compose -f docker-compose.elk.yml up -d
```

For Linux
```
cd src
docker-compose -f docker-compose.misc.yml up -d
docker-compose --env-file ./.env.linux -f docker-compose.yml -f docker-compose.override.yml up -d
docker-compose -f docker-compose.elk.yml up -d
```
## Project url
http://host.docker.internal

https://host.docker.internal/ref

## reset chrome cahe
```
chrome://net-internals/#hsts
chrome:restart
```
## Build frontend only
```
rm -rf nevashop
cd nevashop
git clone --branch Feature_ADMIN git@gitlab.neva.loc:shop/nevashop.git
docker-compose build --force-rm --no-cache --progress plain ShopManager
docker-compose -f docker-compose.yml -f docker-compose.override.yml up

git reset HEAD~ --hard
git checkout uat
git pull


```
