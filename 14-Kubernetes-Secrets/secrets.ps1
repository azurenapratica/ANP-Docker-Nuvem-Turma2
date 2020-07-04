kubectl create namespace anp-teste03

kubectl create secret generic teste-groffe --from-literal=VlTesteAmbiente='Teste utilizando secret from-literal no Kubernetes' -n "anp-teste03"

kubectl describe secret teste-groffe -n "anp-teste03"

kubectl create -f .\apicontagem-deployment.yml -n "anp-teste03"

kubectl create -f .\apicontagem-service.yml -n "anp-teste03"