helm version

helm repo add bitnami https://charts.bitnami.com/bitnami

helm repo list

kubectl create namespace anp-kafka

helm install anp-broker-kafka --set externalAccess.enabled=true,externalAccess.service.type=LoadBalancer,externalAccess.service.port=19092,externalAccess.autoDiscovery.enabled=true,serviceAccount.create=true,rbac.create=true bitnami/kafka -n anp-kafka

helm list

kubectl get pods -n anp-kafka

kubectl get services -n anp-kafka