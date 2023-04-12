
docker exec -it 94f41704b81d cat /etc/nginx/nginx.conf
docker exec -it 94f41704b81d ls /etc/nginx/certs
docker exec -it 94f41704b81d cat /etc/nginx/conf.d/default.conf

curl -v https://docker.neva.loc
curl -k https://host.docker.internal
curl -k https://docker.neva.loc


sudo cp /srv/nginx/certs/docker.neva.loc.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates
