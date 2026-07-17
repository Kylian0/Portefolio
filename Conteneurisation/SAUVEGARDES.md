# Sauvegardes de production

Cette procédure sauvegarde quotidiennement la base MySQL et les médias uploadés. Elle conserve par défaut les sept sauvegardes complètes les plus récentes.

Chaque sauvegarde est placée dans `/var/backups/portfolio/backup-AAAA-MM-JJTHH-MM-SSZ` et contient :

- `mysql.sql.gz` : export logique compressé de la base MySQL ;
- `media.tar.gz` : archive compressée du volume des médias ;
- `SHA256SUMS` : sommes de contrôle des deux archives.

Le script ne lit pas les mots de passe. Docker Compose charge `.env.production`, puis les commandes exécutées dans les conteneurs utilisent leurs variables d’environnement existantes.

## Installation sur le VPS

Les commandes suivantes supposent que le dépôt se trouve dans `/opt/portfolio/Application`. Adaptez ce chemin à son emplacement réel.

```bash
cd /opt/portfolio/Application
sudo chmod 750 Conteneurisation/scripts/backup-production.sh
sudo install -d -m 700 /var/backups/portfolio
sudo touch /var/log/portfolio-backup.log
sudo chmod 600 /var/log/portfolio-backup.log
```

Lancez une première sauvegarde manuelle :

```bash
sudo /opt/portfolio/Application/Conteneurisation/scripts/backup-production.sh
```

Vérifiez ensuite les archives :

```bash
cd /var/backups/portfolio/backup-AAAA-MM-JJTHH-MM-SSZ
sudo sha256sum --check SHA256SUMS
```

Le script suppose que les services `mysql` et `backend` de la configuration de production sont démarrés.

## Automatisation quotidienne avec cron

Éditez la crontab de `root`, nécessaire pour accéder à Docker et au dossier de sauvegarde :

```bash
sudo crontab -e
```

Ajoutez cette ligne pour une exécution quotidienne à 02 h 30 :

```cron
30 2 * * * BACKUP_RETENTION_COUNT=7 /opt/portfolio/Application/Conteneurisation/scripts/backup-production.sh >> /var/log/portfolio-backup.log 2>&1
```

Le script ne supprime les anciennes sauvegardes qu’après la création réussie d’une nouvelle sauvegarde. Il conserve les sept dossiers `backup-*` les plus récents. Le dossier de destination peut être remplacé avec `BACKUP_ROOT=/autre/chemin`.

Contrôlez régulièrement le journal, l’espace disque et la présence des sauvegardes :

```bash
sudo tail -n 100 /var/log/portfolio-backup.log
sudo du -sh /var/backups/portfolio
sudo ls -lah /var/backups/portfolio
```

## Restaurer MySQL

Choisissez une sauvegarde, contrôlez d’abord ses sommes, puis restaurez le dump. Cette opération remplace les données portant les mêmes clés ; réalisez-la pendant une période de maintenance et sauvegardez l’état actuel auparavant.

```bash
cd /var/backups/portfolio/backup-AAAA-MM-JJTHH-MM-SSZ
sudo sha256sum --check SHA256SUMS
gzip -dc mysql.sql.gz | docker compose \
  --env-file /opt/portfolio/Application/Conteneurisation/.env.production \
  -f /opt/portfolio/Application/Conteneurisation/compose.production.yaml \
  exec -T mysql sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" exec mysql --user="$MYSQL_USER" "$MYSQL_DATABASE"'
```

La base ciblée doit déjà exister, ce qui est le cas lorsque le service MySQL a été initialisé avec la configuration de production.

## Restaurer les médias

Arrêtez temporairement le backend afin d’éviter tout upload pendant la restauration, videz le volume actuel, puis extrayez l’archive :

```bash
cd /opt/portfolio/Application/Conteneurisation
docker compose --env-file .env.production -f compose.production.yaml stop backend
docker compose --env-file .env.production -f compose.production.yaml run --rm --no-deps backend \
  sh -c 'find /app/uploads -mindepth 1 -maxdepth 1 -exec rm -rf -- {} +'
docker compose --env-file .env.production -f compose.production.yaml run --rm --no-deps -T backend \
  tar -C /app/uploads -xzf - < /var/backups/portfolio/backup-AAAA-MM-JJTHH-MM-SSZ/media.tar.gz
docker compose --env-file .env.production -f compose.production.yaml up -d backend
```

Vérifiez ensuite plusieurs médias depuis le portfolio et la médiathèque administrateur.

## Limite importante

Une sauvegarde stockée uniquement sur le même VPS ne protège pas contre la perte totale du VPS, la panne du disque, la suppression du serveur ou la compromission de la machine. Une copie régulière vers un autre support ou une autre machine devra être ajoutée pour obtenir une véritable sauvegarde hors site.
