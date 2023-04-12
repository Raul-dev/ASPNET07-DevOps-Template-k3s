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
#openssl genpkey -algorithm RSA -out aspcertificat.key
#openssl req -new -key aspcertificat.key -config $CONF.conf -reqexts v3_req -out aspcertificat.csr
#sudo openssl x509 -req -days 730 -CA /srv/rootca/rootCA.crt -CAkey /srv/rootca/rootCA.key -extfile $CONF.conf -extensions v3_req -in aspcertificat.csr -out aspcertificat.crt
#openssl pkcs12 -export -out aspcertificat.pfx -inkey aspcertificat.key -in aspcertificat.crt 


# for Ubuntu didnt work rootCA certificate

openssl req \
-x509 \
-newkey rsa:4096 \
-sha256 \
-days 365 \
-nodes \
-keyout $CONF.key \
-out $CONF.crt \
-subj "/CN=${CONF}" \
-extensions v3_ca \
-extensions v3_req \
-config <( \
  echo '[req]'; \
  echo 'default_bits= 4096'; \
  echo 'distinguished_name=req'; \
  echo 'x509_extension = v3_ca'; \
  echo 'req_extensions = v3_req'; \
  echo '[v3_req]'; \
  echo 'basicConstraints = CA:FALSE'; \
  echo 'keyUsage = nonRepudiation, digitalSignature, keyEncipherment'; \
  echo 'subjectAltName = @alt_names'; \
  echo '[ alt_names ]'; \
  echo "DNS.1 = www.${CONF}"; \
  echo "DNS.2 = ${CONF}"; \
  echo '[ v3_ca ]'; \
  echo 'subjectKeyIdentifier=hash'; \
  echo 'authorityKeyIdentifier=keyid:always,issuer'; \
  echo 'basicConstraints = critical, CA:TRUE, pathlen:0'; \
  echo 'keyUsage = critical, cRLSign, keyCertSign'; \
  echo 'extendedKeyUsage = serverAuth, clientAuth')

#ls 
#echo "copy files"
#cp -rf ./aspcertificat.pfx  ../Https/$CONF.pfx
#cp -rf ./aspcertificat.crt  ../nginx/certs/$CONF.crt
#cp -rf ./aspcertificat.key  ../nginx/certs/$CONF.key

echo "exit"

exit 0
#Buid machine
#sudo mkdir -p /https
#sudo chmod -R 777 /https
#sudo ls -la ../https
sudo mkdir -p /srv/nginx/certs
sudo chmod -R 777 /srv/nginx/certs
#sudo ls -la ../../ui/cert
#cp -rf ./aspcertificat.pfx /https/$CONF.pfx
cp -rf ./$CONF.crt /srv/nginx/certs/$CONF.crt
cp -rf ./$CONF.key /srv/nginx/certs/$CONF.key
cp -rf ../nginx/proxy.conf /srv/nginx/proxy.conf


