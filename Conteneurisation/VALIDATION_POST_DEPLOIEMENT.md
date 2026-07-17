# Checklist de validation après le premier déploiement

À exécuter manuellement après le premier lancement de production. Les commandes
supposent que le dépôt est installé dans `/opt/portfolio/Application` et que la
configuration de production est dans `Conteneurisation/.env.production`.

> Les tests de création, modification, suppression et restauration modifient
> des données. Réalisez-les avec un projet de test identifiable et une
> sauvegarde récente.

## 1. État des conteneurs et exposition réseau

- [ ] Les services attendus sont démarrés :

  ```bash
  cd /opt/portfolio/Application/Conteneurisation
  docker compose --env-file .env.production -f compose.production.yaml ps
  ```

  Attendu : `frontend`, `backend`, `mysql` et `caddy` sont actifs ; MySQL est
  `healthy`.

- [ ] Seuls les ports publics 80 et 443 sont publiés par Docker :

  ```bash
  docker ps --format 'table {{.Names}}\t{{.Ports}}'
  sudo ss -lntup | grep -E ':(80|443|3306|3307|8080|8081|5097|5200)\b'
  ```

  Attendu : aucun port MySQL, phpMyAdmin, API ou frontend n'est publié sur
  l'hôte ; seuls Caddy/80 et Caddy/443 sont accessibles publiquement.

- [ ] MySQL n'est pas accessible depuis une autre machine :

  ```bash
  mysql -h <IP_DU_VPS> -P 3306 -u test -p
  ```

  Attendu : échec de connexion. Ne lancez pas ce test depuis le VPS lui-même.

- [ ] phpMyAdmin n'est pas accessible publiquement :

  ```bash
  curl -I --max-time 10 http://<IP_DU_VPS>:8081
  ```

  Attendu : échec de connexion. Il ne doit pas apparaître dans `docker ps` de
  production.

## 2. Domaine, HTTPS, frontend et API

- [ ] Le domaine répond en HTTPS :

  ```bash
  curl -I https://portfolio-garnier.fr
  ```

  Attendu : réponse HTTP réussie (`200` ou redirection applicative attendue).

- [ ] HTTP redirige vers HTTPS :

  ```bash
  curl -I http://portfolio-garnier.fr
  ```

  Attendu : `301`, `302`, `307` ou `308` avec un en-tête `Location` en
  `https://portfolio-garnier.fr/...`.

- [ ] Le certificat est valide et délivré pour le domaine :

  ```bash
  curl -vI https://portfolio-garnier.fr 2>&1 | grep -E 'SSL certificate verify ok|subject:|issuer:'
  ```

  Attendu : vérification TLS réussie. Vérifiez aussi dans un navigateur sans
  avertissement de certificat.

- [ ] Le frontend et ses routes client se chargent, notamment `/`, `/projects`,
  `/contact` et `/admin` :

  ```bash
  curl -I https://portfolio-garnier.fr/
  curl -I https://portfolio-garnier.fr/admin
  ```

  Vérifiez visuellement dans un navigateur la navigation et le rechargement
  direct de ces routes.

- [ ] L'API répond via le chemin public `/api` :

  ```bash
  curl -i https://portfolio-garnier.fr/api/projects
  ```

  Attendu : `200 OK` et une liste JSON, éventuellement vide. Il ne doit pas
  être nécessaire d'utiliser un port distinct.

## 3. En-têtes HTTP de sécurité

- [ ] Les en-têtes de sécurité attendus sont présents :

  ```bash
  curl -sI https://portfolio-garnier.fr | grep -Ei 'strict-transport-security|x-content-type-options|referrer-policy|permissions-policy'
  ```

  Attendu : au minimum `Strict-Transport-Security` et
  `X-Content-Type-Options: nosniff`; le frontend doit aussi retourner
  `Referrer-Policy` et `Permissions-Policy`.

## 4. Administration et autorisations

- [ ] `/admin` est accessible, et la connexion avec l'identifiant administrateur
  configuré dans `.env.production` fonctionne.

- [ ] Sans session administrateur, une opération d'écriture API est refusée :

  ```bash
  curl -i -X POST https://portfolio-garnier.fr/api/projects \
    -H 'Content-Type: application/json' \
    -d '{}'
  ```

  Attendu : `401 Unauthorized` ou `403 Forbidden`, et non une création.

- [ ] Après connexion, avec un projet de test :

  - [ ] créer un projet ;
  - [ ] modifier son titre ou sa description ;
  - [ ] supprimer ce même projet ;
  - [ ] vérifier que la liste d'administration s'actualise après chaque action.

  Vérifiez également qu'une confirmation explicite apparaît avant suppression.

## 5. Médias et persistance

- [ ] Depuis la médiathèque, envoyer une image autorisée de taille raisonnable.
  Vérifier l'aperçu, l'URL publique et l'affichage dans le projet ou la
  documentation concernés.

- [ ] Tenter l'envoi d'un fichier interdit (par exemple `test.exe`).
  Attendu : refus serveur avec une erreur claire, sans création de média.

- [ ] Redémarrer les services, puis vérifier que le média est toujours visible :

  ```bash
  cd /opt/portfolio/Application/Conteneurisation
  docker compose --env-file .env.production -f compose.production.yaml restart
  docker compose --env-file .env.production -f compose.production.yaml ps
  ```

- [ ] Vérifier de la même manière qu'un projet et ses données MySQL existent
  toujours après le redémarrage.

## 6. Formulaire de contact et limitation

- [ ] Envoyer un message valide depuis la page Contact ; vérifier la
  confirmation côté client et sa présence dans l'administration.

- [ ] Vérifier qu'un envoi invalide (email ou message manquant) est refusé.

- [ ] Tester le rate limiting depuis une adresse IP de test en effectuant des
  soumissions répétées dans la fenêtre de limitation configurée.

  Attendu : les premiers envois autorisés reçoivent une réponse normale, puis
  l'API retourne `429 Too Many Requests`. Attendez la fin de la fenêtre avant
  de recommencer afin de ne pas gêner les visiteurs.

## 7. Sauvegardes et restaurations

- [ ] Créer une sauvegarde manuelle :

  ```bash
  sudo /opt/portfolio/Application/Conteneurisation/scripts/backup-production.sh
  sudo ls -lah /var/backups/portfolio
  ```

- [ ] Vérifier que la sauvegarde contient MySQL, les médias et les checksums :

  ```bash
  cd /var/backups/portfolio/backup-AAAA-MM-JJTHH-MM-SSZ
  sudo sha256sum --check SHA256SUMS
  sudo ls -lh mysql.sql.gz media.tar.gz SHA256SUMS
  ```

- [ ] Vérifier la rotation après plusieurs sauvegardes :

  ```bash
  sudo find /var/backups/portfolio -maxdepth 1 -type d -name 'backup-*' | wc -l
  ```

  Attendu : au plus sept dossiers si `BACKUP_RETENTION_COUNT=7` est utilisé.

- [ ] Effectuer une restauration MySQL de test sur un environnement de test ou
  après sauvegarde préalable de l'état courant. Suivre exactement la procédure
  [SAUVEGARDES.md](SAUVEGARDES.md#restaurer-mysql), puis vérifier les données
  depuis l'administration.

- [ ] Effectuer une restauration de médias de test avec une archive connue,
  également selon [SAUVEGARDES.md](SAUVEGARDES.md#restaurer-les-médias), puis
  vérifier l'affichage public et dans la médiathèque.

## 8. Validation finale

- [ ] Consulter les journaux après les tests :

  ```bash
  cd /opt/portfolio/Application/Conteneurisation
  docker compose --env-file .env.production -f compose.production.yaml logs --tail=200 caddy backend mysql
  ```

- [ ] Aucune erreur inattendue, aucun secret ni mot de passe dans les journaux.
- [ ] Conserver une copie de la première sauvegarde validée hors du VPS dès que
  possible.
