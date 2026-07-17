#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
CONTAINER_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
COMPOSE_FILE="${COMPOSE_FILE:-${CONTAINER_DIR}/compose.production.yaml}"
ENV_FILE="${ENV_FILE:-${CONTAINER_DIR}/.env.production}"
BACKUP_ROOT="${BACKUP_ROOT:-/var/backups/portfolio}"
BACKUP_RETENTION_COUNT="${BACKUP_RETENTION_COUNT:-7}"

if ! [[ "${BACKUP_RETENTION_COUNT}" =~ ^[1-9][0-9]*$ ]]; then
    echo "BACKUP_RETENTION_COUNT doit être un entier strictement positif." >&2
    exit 1
fi

for command_name in docker gzip sha256sum; do
    if ! command -v "${command_name}" >/dev/null 2>&1; then
        echo "Commande requise introuvable : ${command_name}" >&2
        exit 1
    fi
done

if [[ ! -f "${COMPOSE_FILE}" ]]; then
    echo "Fichier Compose introuvable : ${COMPOSE_FILE}" >&2
    exit 1
fi

if [[ ! -f "${ENV_FILE}" ]]; then
    echo "Fichier d'environnement introuvable : ${ENV_FILE}" >&2
    exit 1
fi

mkdir -p -- "${BACKUP_ROOT}"
chmod 700 "${BACKUP_ROOT}"

timestamp="$(date -u +'%Y-%m-%dT%H-%M-%SZ')"
backup_name="backup-${timestamp}"
temporary_directory="${BACKUP_ROOT}/.${backup_name}.partial"
backup_directory="${BACKUP_ROOT}/${backup_name}"

cleanup() {
    rm -rf -- "${temporary_directory}"
}
trap cleanup EXIT

mkdir -- "${temporary_directory}"

echo "Sauvegarde MySQL…"
docker compose --env-file "${ENV_FILE}" -f "${COMPOSE_FILE}" exec -T mysql \
    sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" exec mysqldump --user="$MYSQL_USER" --single-transaction --quick --lock-tables=false --routines --triggers --events "$MYSQL_DATABASE"' \
    | gzip -9 > "${temporary_directory}/mysql.sql.gz"

echo "Sauvegarde des médias…"
docker compose --env-file "${ENV_FILE}" -f "${COMPOSE_FILE}" exec -T backend \
    tar -C /app/uploads -czf - . > "${temporary_directory}/media.tar.gz"

(
    cd -- "${temporary_directory}"
    sha256sum mysql.sql.gz media.tar.gz > SHA256SUMS
)

mv -- "${temporary_directory}" "${backup_directory}"
trap - EXIT

mapfile -t backups < <(
    find "${BACKUP_ROOT}" -mindepth 1 -maxdepth 1 -type d -name 'backup-*' -printf '%f\n' | sort -r
)

for ((index = BACKUP_RETENTION_COUNT; index < ${#backups[@]}; index++)); do
    expired_backup="${BACKUP_ROOT}/${backups[index]}"
    if [[ "${expired_backup}" != "${BACKUP_ROOT}/backup-"* ]]; then
        echo "Chemin de rotation inattendu : ${expired_backup}" >&2
        exit 1
    fi

    rm -rf -- "${expired_backup}"
done

echo "Sauvegarde terminée : ${backup_directory}"
