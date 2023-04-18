
Setup VM
1) Get Linux image ubuntu-22.04.2-live-server-amd64
a) network time sink
sudo apt -y install ntp
sudo nano /var/lib/ntp/ntp.drift
server 0.pool.ntp.org
server 1.pool.ntp.org
server 2.pool.ntp.org
server 3.pool.ntp.org
sudo ntpd start
sudo timedatectl set-timezone Europe/Moscow

b) Setup router for fixed IP, Set local domain name prod.neva.loc

sudo hostnamectl set-hostname prod.neva.loc
sudo nano /etc/hosts
127.0.0.1 localhost
127.0.1.1 prod
192.168.0.532 prod.neva.loc prod
192.168.0.532 shop.neva.example.com shop
192.168.0.532 api1.neva.example.com api1

sudo nano /etc/systemd/resolved.conf
Domains=neva.example.com
DNSSEC=no
DNSOverTLS=no
sudo systemctl restart systemd-resolved
#systemd-resolve --status
#journalctl -u systemd-resolved -f
#check:
hostname -f

2)Create Free DNS on https://www.cloudns.net/
shop.neva.example.com
api1.neva.example.com

3) Get letsEncript certificate
https://serverspace.ru/support/help/lets-encrypt-ubuntu-20-04/
https://trueconf.ru/blog/baza-znaniy/sozdanie-sertifikata-lets-encrypt-na-linux.html
Для добавления пользователя с логином user в группу sudo выполните usermod -a -G sudo user под учётной записью root.
sudo apt install -yq certbot
sudo apt install net-tools
sudo netstat -tulpn | grep LISTEN
ping shop.neva.example.com
sudo ufw allow 80
sudo ufw allow 443
sudo certbot certonly --standalone --agree-tos --preferred-challenges http -d shop.neva.example.com,api1.neva.example.com

Successfully received certificate.
Certificate is saved at: /etc/letsencrypt/live/shop.neva.example.com/fullchain.pem
Key is saved at:         /etc/letsencrypt/live/shop.neva.example.com/privkey.pem
This certificate expires on 2023-07-09.

sudo ls /etc/letsencrypt/live/shop.neva.example.com
sudo cp /etc/letsencrypt/live/shop.neva.example.com/cert.pem /opt/trueconf/server/etc/webmanager/ssl/custom.crt
sudo cp /etc/letsencrypt/live/test.domain.ru/privkey.pem /opt/trueconf/server/etc/webmanager/ssl/custom.key

4) Install v1.26.3+k3s1 https://docs.k3s.io/quick-start  
sudo su 
curl -sfL https://get.k3s.io | sh -
systemctl status k3s
kubectl get all --all-namespaces

https://0to1.nl/post/k3s-kubectl-permission/
exit
echo K3S_KUBECNFIG_MODE=\"644\" >> /etc/systemd/system/k3s.service.env
mkdir ~/.kube
sudo cp /etc/rancher/k3s/k3s.yaml ~/.kube/config 
sudo chown $USER ~/.kube/config 
sudo chmod 600 ~/.kube/config 
export KUBECONFIG=~/.kube/config
sudo chmod 644 /etc/rancher/k3s/k3s.yaml

5) Install gitlab Agent
a) Install helm 
curl https://baltocdn.com/helm/signing.asc | gpg --dearmor | sudo tee /usr/share/keyrings/helm.gpg > /dev/null
sudo apt-get install apt-transport-https --yes
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/helm.gpg] https://baltocdn.com/helm/stable/debian/ all main" | sudo tee /etc/apt/sources.list.d/helm-stable-debian.list
sudo apt-get update
sudo apt-get install helm

b) Create commad agent registration
From the left sidebar, select Infrastructure > Kubernetes clusters.
Select Connect a cluster (agent).
If you want to create a configuration with CI/CD defaults, type a name "prod", click "Create Agent" and click "register" .
and recive:
AccessToken=<AccessToken text>
and base command: helm upgrade --install prod gitlab/gitlab-agent 

c)Gitlab Helm blocked from russia
download gitlab-agent-main.tar from https://gitlab.com/gitlab-org/charts/gitlab-agent with proxy

tar -xvf gitlab-agent-main.tar

d)  Install gitlab-agent
cat rootCA.crt rootCA.key > rootCA.pem
cp rootCA.pem /etc/ssl/certs/myCA.pem

helm upgrade --install prod gitlab-agent-main/ \
    --namespace gitlab-agent \
    --create-namespace \
    --set image.tag=v15.5.1 \
    --set config.token=<AccessToken text> \
    --set config.kasAddress=wss://gitlab.neva.loc/-/kubernetes-agent/ \
    --set config.caCert="-----BEGIN CERTIFICATE 1
    result of rootCA.pem
    END ENCRYPTED PRIVATE KEY-----"


kubectl config view --raw -o=jsonpath='{.clusters[0].cluster.certificate-authority-data}' | base64 --decode

helm uninstall prod --namespace gitlab-agent


6) Test ingress
a) 80: https://www.virtualizationhowto.com/2022/05/traefik-ingress-example-yaml-and-setup-in-k3s/
kubectl get all --all-namespaces
kubectl create deploy nginx --image nginx
kubectl expose deploy nginx --port 80
kubectl apply -f testk3s/k3singrestest.yaml
kubectl describe ingress nginx

b) 443: https://devopscube.com/configure-ingress-tls-kubernetes/
prod.neva.loc.conf
openssl genpkey -algorithm RSA -out prod.neva.loc.key
openssl req -new -key prod.neva.loc.key -config prod.neva.loc.conf -reqexts v3_req -out prod.neva.loc.csr
sudo openssl x509 -req -days 730 -CA rootCA.crt -CAkey rootCA.key -extfile prod.neva.loc.conf -extensions v3_req -in prod.neva.loc.csr -out prod.neva.loc.crt


kubectl create secret tls prod-local-tls --key prod.neva.loc.key --cert prod.neva.loc.crt
sudo ls /etc/letsencrypt/live/shop.neva.example.com/
sudo cp /etc/letsencrypt/live/shop.neva.example.com/privkey.pem privkey.key
sudo cp /etc/letsencrypt/live/shop.neva.example.com/cert.pem cert.crt
sudo kubectl create secret tls prod-ext-tls --key privkey.key --cert cert.crt

kubectl delete -f testk3s/k3singrestest.yaml
kubectl apply -f testk3s/k3singrestesttls.yaml
kubectl describe ingress nginx
curl prod.neva.loc

kubectl get ingress
kubectl delete -f testk3s/k3singrestesttls.yaml
kubectl apply -f testk3s/k3singrestesttlsext.yaml
kubectl describe ingress nginx
curl shop.neva.example.com
kubectl delete deploy nginx 
kubectl delete service nginx 

kubectl apply -f 11_whoamiapi.yaml

7) Get gitlab registry access
Navigate to your group settings, then Repository . There is a section called Deploy Tokens .
Create a new token, with only read_registry box ticked. Get REGISTRY_USERNAME and REGISTRY_PASSWORD

Create file 01_gitlab-pull-secret.yaml
kubectl create secret docker-registry gitlab-credentials \
--docker-server=gitlab.neva.loc:5050 \
--docker-username=REGISTRY_USERNAME \
--docker-password=REGISTRY_PASSWORD \
-n default  --dry-run=client -o yaml > 01_gitlab-pull-secret.yaml


kubectl apply -f 01_gitlab-pull-secret.yaml
kubectl delete secret gitlab-credentials

8) Db secret

kubectl create secret generic keycloak-db-secret \
  --from-literal=username=gpadmin \
  --from-literal=password=






