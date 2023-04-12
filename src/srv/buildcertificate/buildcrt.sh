#!/bin/bash
#sudo chmod +x buildcrt.sh
#sudo chmod -R 777 ./deployment/cert &&  cd ./deployment/cert && sudo chmod +x ./buildcrt.sh && ./buildcrt.sh "test1.hlhub.ru.conf"
#sudo ./buildcrt.sh host.docker.internal
#sudo ./buildcrt.sh docker.neva.loc
#sudo ./buildcrt.sh ubuntu22.neva.loc
CONF=$1
if [ "x$CONF" == "x" ]; then
    CONF="localhost"
fi
echo "Config file name:$CONF.conf"

#1)Created key
#openssl genpkey -algorithm RSA -out /srv/rootca/rootCA.key -aes-128-cbc      вводим пароль: blablabla
#2) Created root 
#openssl req -x509 -new -key /srv/rootca/rootCA.key -sha256 -days 3650 -out /srv/rootca/rootCA.crt
#3) For windows
#openssl pkcs12 -export -out rootCA.pfx -inkey /srv/rootca/rootCA.key -in /srv/rootca/rootCA.crt

#Create root before. you needed rootCA.crt rootCA.key 
openssl genpkey -algorithm RSA -out aspcertificat.key
openssl req -new -key aspcertificat.key -config $CONF.conf -reqexts v3_req -out aspcertificat.csr
sudo openssl x509 -req -days 730 -CA /srv/rootca/rootCA.crt -CAkey /srv/rootca/rootCA.key -extfile $CONF.conf -extensions v3_req -in aspcertificat.csr -out aspcertificat.crt
openssl pkcs12 -export -out aspcertificat.pfx -inkey aspcertificat.key -in aspcertificat.crt 


#ls 
#echo "copy files"
#cp -rf ./aspcertificat.pfx  ../Https/$CONF.pfx
#cp -rf ./aspcertificat.crt  ../nginx/certs/$CONF.crt
#cp -rf ./aspcertificat.key  ../nginx/certs/$CONF.key

#echo "exit"

#exit 0
#Buid machine
#sudo mkdir -p /https
#sudo chmod -R 777 /https
#sudo ls -la ../https
sudo mkdir -p /srv/nginx/certssso
sudo chmod -R 777 /srv/nginx/certssso
#sudo ls -la ../../ui/cert
#cp -rf ./aspcertificat.pfx /https/$CONF.pfx
cp -rf ./aspcertificat.crt /srv/nginx/certssso/$CONF.crt
cp -rf ./aspcertificat.key /srv/nginx/certssso/$CONF.key
cp -rf ../nginx/proxy.conf /srv/nginx/proxy.conf


