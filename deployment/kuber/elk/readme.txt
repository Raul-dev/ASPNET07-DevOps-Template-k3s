ELK Установка
к оригинальным Helm и контейнерам доступ заблокирован.
0) Устанавливаем Helm 3.10
https://devopscube.com/install-configure-helm-kubernetes/
curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3
curl -LO https://git.io/get_helm.sh
chmod +x get_helm.sh
./get_helm.sh
helm version

1) bitnami/elasticsearch по умолчанию не работает на k3s, не поднимается контейнер data c ошибкой 0/1 nodes are available: 1 Insufficient memory. preemption: 0/1 nodes are available. 
a)Команды для отладки:
kubectl create namespace efk
kubectl get all -n efk
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
kubectl get node -o wide
на всех нодах применить:
sudo sysctl -w vm.max_map_count=262144
sudo sysctl -w fs.file-max=65536
helm install elasticsearch bitnami/elasticsearch -n efk
helm delete elasticsearch --namespace efk
kubectl port-forward --namespace efk svc/elasticsearch 9200:9200 
curl http://127.0.0.1:9200/
curl http://localhost:9200/_cluster/state?pretty
 https://linuxhint.com/kibana-server-not-ready-yet/
 список индексов должен пустым
 curl -XGET "http://localhost:9200/_cat/indices?v&index=.kib*&h=index"

helm install kibana --namespace efk bitnami/kibana --set image.tag=8.6.2-debian-11-r3 --set elasticsearch.hosts[0]=elasticsearch.efk.svc.cluster.local --set elasticsearch.port=9200 --set ingress.enabled=true --set ingress.hostname=shop.example.com
helm delete kibana --namespace efk
kubectl port-forward svc/kibana 8080:5601 -n efk
  echo "Visit http://127.0.0.1:8080 to use your application"
выдает ошибку: Kibana not ready yet
curl -XGET "http://localhost:9200/_cat/indices?v&index=.kib*&h=index"
после старта кибаны  список индексов должен состоять из 3х следующих индексов, но нехватает .kibana-event-log-8.6.2-000001 https://github.com/elastic/kibana/issues/66968
index
.kibana_8.6.2_001
.kibana_task_manager_8.6.2_001
.kibana-event-log-8.6.2-000001

b) Урезаем ресурсы требуемые на дата контейнер по умолчанию
data:
  resources:
    limits: {}
    requests:
      cpu: 25m
      memory: 2048Mi
  heapSize: 1024m
  replicaCount: 2
helm install elasticsearch bitnami/elasticsearch -n efk --set data.resources.requests.memory=1024m --set data.heapSize=512m 
helm upgrade elasticsearch bitnami/elasticsearch -n efk --set master.replicaCount=1 --set coordinating.replicaCount=1 --set ingest.replicaCount=1 --set data.replicaCount=1 --set data.resources.requests.memory=256m --set data.heapSize=128m 
Удаляем кибану и индексы которые она создала в эластике
curl -XPUT "http://localhost:9200/_cluster/settings" -H 'Content-Type: application/json' -d'
{
  "persistent" : {
    "action.destructive_requires_name" : false
  }
}'

curl -XDELETE "http://localhost:9200/.kibana*?expand_wildcards=open"
curl -XGET "http://localhost:9200/_cat/indices?v&index=.kib*&h=index"
helm test elasticsearch -n efk
Следующая ошибка:
c) Failed to connect to elasticsearch-coordinating-0.elasticsearch-coordinating-hl.efk.svc.cluster.local port 9200
kubectl delete pvc data-elasticsearch-master-1 -n efk \
kubectl delete pvc data-elasticsearch-master-0  -n efk \
kubectl delete pvc data-elasticsearch-data-0  -n efk \
kubectl delete pvc data-elasticsearch-data-1  -n efk

d) Для k8s ошибка Kubernetes Persistent Volume in pending status
sudo mkdir -p /mnt/data/elk/master \
sudo mkdir -p /mnt/data/elk/data \
sudo chmod 777 /mnt/data/elk/master \
sudo chmod 777 /mnt/data/elk/data
https://www.weave.works/blog/kubernetes-faq-configure-storage-for-bare-metal-cluster
https://medium.com/devityoself/kubernetes-bare-metal-storage-49b69d090dfa
rancher/local-path-provisioner
https://github.com/rancher/local-path-provisioner      
How To use local path
kubectl apply -f https://raw.githubusercontent.com/rancher/local-path-provisioner/v0.0.24/deploy/local-path-storage.yaml
kubectl apply -f elasticsearch-pv.yaml
How To Set Up a Ceph Cluster within Kubernetes Using Rook
https://www.digitalocean.com/community/tutorials/how-to-set-up-a-ceph-cluster-within-kubernetes-using-rook

https://rook.io/docs/rook/v1.11/Helm-Charts/ceph-cluster-chart/#release
helm repo add rook-release https://charts.rook.io/release
helm install --create-namespace --namespace rook-ceph rook-ceph rook-release/rook-ceph
helm install --create-namespace --namespace rook-ceph rook-ceph-cluster --set operatorNamespace=rook-ceph rook-release/rook-ceph-cluster -f values-override.yaml
для организации rook-ceph нужно много ресурсов

Используем PersistentVolume elk-pv-volume и
elk-master-pv-claim
elk-data-pv-claim
kubectl apply -f elasticsearch-sc.yaml
kubectl apply -f elasticsearch-pv.yaml -n efk
echo test > /mnt/data/elk/data/test.txt 
cat /mnt/data/elk/data/test.txt

helm install elasticsearch bitnami/elasticsearch -n efk --set master.persistence.enabled=true --set master.persistence.existingClaim=elk-master-pv-claim --set data.persistence.enabled=true \
--set data.persistence.existingClaim=elk-data-pv-claim --set master.replicaCount=1 --set coordinating.replicaCount=1 --set ingest.replicaCount=1 --set data.replicaCount=1 --set data.resources.requests.memory=256m --set data.heapSize=128m 
проблему java.nio.file.AccessDeniedException: /bitnami/elasticsearch/data/node.lock
на самодельном StorageClass provisioner: kubernetes.io/no-provisioner побороть не удалось
отключаем хранение файлов на шаре --set master.persistence.enabled=false --set data.persistence.enabled=false
helm install elasticsearch bitnami/elasticsearch -n efk --set master.persistence.enabled=false --set data.persistence.enabled=false --set master.replicaCount=1 --set coordinating.replicaCount=1 --set ingest.replicaCount=1 --set data.replicaCount=1 --set data.resources.requests.memory=256m --set data.heapSize=128m 
заработало на k8s


2) простые манифесты с 1 подом версии 8.6.2 работают
kubectl apply -f elk/elasticsearch-pod.yaml -n efk
kubectl apply -f elk/kibana-pod.yaml -n efk
kubectl port-forward --namespace efk svc/elasticsearch 9200:9200 
curl -XGET "http://localhost:9200/_cat/indices?v&index=.kib*&h=index"
http://shop.example.com

3) переключение контекстов кубер
kubectl config get-contexts
kubectl config use-context cluster.local
kubectl config use-context default
kubectl config view --minify --raw
elastic-system

рабочий вариант
helm install elasticsearch bitnami/elasticsearch -n efk --set master.persistence.enabled=false --set data.persistence.enabled=false --set master.replicaCount=1 --set coordinating.replicaCount=1 --set ingest.replicaCount=1 --set data.replicaCount=1 --set data.resources.requests.memory=256m --set data.heapSize=128m 
helm install kibana --namespace efk bitnami/kibana --set image.tag=8.6.2-debian-11-r3 --set elasticsearch.hosts[0]=elasticsearch.efk.svc.cluster.local --set elasticsearch.port=9200 --set ingress.enabled=true --set ingress.hostname=api1.example.com

4) Установка kubernetes через kubespray (local-storage, установка Elasticsearch + Fluentd + Kibana, prometheus)
https://habr.com/ru/articles/426959/

helm install fluentd bitnami/fluentd --set elasticsearch.host=elasticsearch.efk.svc.cluster.local
helm install fluentd bitnami/fluentd -n kube-system --set elasticsearch.host=elasticsearch.efk.svc.cluster.local
helm show values bitnami/fluentd
helm delete fluentd
https://stackoverflow.com/questions/59530536/fluentd-is-working-but-no-index-is-being-created-on-elastcisearch
https://www.digitalocean.com/community/tutorials/how-to-set-up-an-elasticsearch-fluentd-and-kibana-efk-logging-stack-on-kubernetes

kubectl -n kube-system describe svc elasticsearch-logging

kube-system  elasticsearch-logging

https://stackoverflow.com/questions/51133077/how-can-i-debug-why-fluentd-is-not-sending-data-to-elasticsearch
kubectl get all -n efk
kubectl -n efk exec -it fluentd-0 -- sh
kubectl -n efk exec -it fluentd-kbkn2 -- sh

cat /etc/os-release
не поставить curl
install curl on debian
curl http://elasticsearch:9200/_cat/indices

https://www.digitalocean.com/community/tutorials/how-to-centralize-your-docker-logs-with-fluentd-and-elasticsearch-on-ubuntu-16-04

docker run --log-driver=fluentd ubuntu /bin/echo 'Hello world'

https://docs.dapr.io/operations/monitoring/logging/fluentd/
kubectl apply -f ./fluentd-config-map.yaml
kubectl apply -f ./fluentd-dapr-with-rbac.yaml

Видео
https://www.youtube.com/watch?v=PZHEgNKORbY
https://github.com/justmeandopensource/elk

https://www.rancher.com/quick-start
sudo docker run --privileged -d --restart=unless-stopped -p 80:80 -p 443:443 rancher/rancher
docker logs  container-id  2>&1 | grep "Bootstrap Password:"

Видео
https://www.youtube.com/watch?v=Gp0-7oVOtPw

helm repo add dapr https://dapr.github.io/helm-charts/
helm repo update
helm install dapr dapr/dapr --namespace dapr-system --create-namespace --set global.logAsJson=true
helm delete dapr --namespace dapr-system

https://github.com/k3s-io/k3s/discussions/5530
В итоге k3s оказался непригодным для сбора логов с stdout

k3s
https://community.traefik.io/t/log-aggregation-for-traefik-and-kubernetes-with-the-elastic-stack/9723
helm show values elastic/filebeat
helm install filebeat elastic/filebeat -f ./filebeat-values.yaml

Как работает RBAC в Kubernetes
https://habr.com/ru/companies/southbridge/articles/655409/

Вообщем урезанная версия ELK для k3s из файлов:
kubectl config use-context default
kubectl get all -n efk
helm install elasticsearch bitnami/elasticsearch -n efk --set master.persistence.enabled=false --set data.persistence.enabled=false --set master.replicaCount=1 --set coordinating.replicaCount=1 --set ingest.replicaCount=1 --set data.replicaCount=1 --set data.resources.requests.memory=256m --set data.heapSize=128m 
helm install kibana --namespace efk bitnami/kibana --set image.tag=8.6.2-debian-11-r3 --set elasticsearch.hosts[0]=elasticsearch.efk.svc.cluster.local --set elasticsearch.port=9200 --set ingress.enabled=true --set ingress.hostname=api1.example.com
kubectl apply -f logstash-deployment.yaml -n efk
kubectl apply -f filebeat-deployment.yaml -n efk


5) Логи с  экрана k8s

kubectl config use-context kubernetes-admin@kubernetes  

helm install elasticsearch bitnami/elasticsearch -n efk --set master.persistence.enabled=false --set data.persistence.enabled=false --set master.replicaCount=1 --set coordinating.replicaCount=1 --set ingest.replicaCount=1 --set data.replicaCount=1 --set data.resources.requests.memory=256m --set data.heapSize=128m 
  kubectl port-forward --namespace efk svc/elasticsearch 9200:9200 
  curl http://127.0.0.1:9200/
helm show values bitnami/kibana

helm install kibana --namespace efk bitnami/kibana --set image.tag=8.6.2-debian-11-r3 --set elasticsearch.hosts[0]=elasticsearch.efk.svc.cluster.local --set elasticsearch.port=9200 --set persistence.enabled=false
  echo "Visit http://127.0.0.1:8080 to use your application"
  kubectl port-forward svc/kibana 8080:5601 --namespace efk

kubectl apply -f demo-multiline.yaml 

kubectl apply -f fluentd-config-map.yaml  
kubectl apply -f fluentd-daemonset.yaml
kubectl get all -n kube-system
kubernetes.labels.k8s-app
kubectl delete -f fluentd-daemonset.yaml
