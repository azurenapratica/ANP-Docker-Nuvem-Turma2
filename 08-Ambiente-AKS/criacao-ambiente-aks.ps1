az provider register -n Microsoft.ContainerService

az aks create --resource-group TesteKubernetes --name AKSCluster --node-count 2 --generate-ssh-keys

az aks get-credentials --resource-group TesteKubernetes --name AKSCluster --overwrite-existing