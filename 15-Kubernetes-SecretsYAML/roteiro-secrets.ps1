kubectl create namespace "anp-teste04"

kubectl apply -f .\teste-groffe-secret.yaml -n "anp-teste04"

kubectl describe secret teste-groffe-yaml -n "anp-teste04"

kubectl create -f .\apicontagem-deployment.yml -n "anp-teste04"

kubectl create -f .\apicontagem-service.yml -n "anp-teste04"