kubectl create namespace anp-teste05

kubectl create -f .\cronjob-monitoramentosites.yaml -n "anp-teste05"

kubectl get cronjobs -n "anp-teste05"