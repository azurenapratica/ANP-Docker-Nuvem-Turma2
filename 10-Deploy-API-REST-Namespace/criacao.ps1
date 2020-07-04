kubectl create namespace anp-teste02

kubectl create -f .\apicontagem-deployment.yml -n anp-teste02

kubectl create -f .\apicontagem-service.yml -n anp-teste02
