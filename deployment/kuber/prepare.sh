
cp ../srv/filebeatjson/filebeat.yml /srv/filebeatjson/filebeat.yml
cp ../srv/sso/realm.json  /srv/sso/realm.json
cp ../srv/nginx/templates/default.conf.template /srv/nginx/templates/k3s/default.conf.template

sudo cp /etc/letsencrypt/live/shop.neva.cloudns.nz/cert.pem /srv/nginx/certssso/shop.neva.cloudns.nz.crt
sudo cp /etc/letsencrypt/live/shop.neva.cloudns.nz/privkey.pem /srv/nginx/certssso/shop.neva.cloudns.nz.key
sudo ls /srv/nginx/certssso -l
sudo chmod 644 /srv/nginx/certssso/shop.neva.cloudns.nz.key
sudo chmod 644 /srv/nginx/certssso/prod.neva.loc.key