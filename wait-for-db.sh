#!/usr/bin/env bash
set -e

DB_HOST="${DB_HOST:-db}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-postgres}"
DB_NAME="${DB_NAME:-finance_tracker_dev}"
RETRIES="${RETRIES:-20}"
SLEEP_SEC="${SLEEP_SEC:-3}"

i=0
until pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1; do
  i=$((i+1))
  if [ "$i" -ge "$RETRIES" ]; then
    echo "[wait-for-db] Timeout waiting for Postgres on ${DB_HOST}:${DB_PORT}" >&2
    exit 1
  fi
  echo "[wait-for-db] waiting for postgres... ($i/$RETRIES)"
  sleep "$SLEEP_SEC"
done

echo "[wait-for-db] Postgres is ready."
exec "$@"