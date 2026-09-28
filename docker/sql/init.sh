#!/bin/bash
# Ejecuta todos los scripts .sql en orden alfabético (00-, 01-, 10-, ...).
set -e
SQLCMD=/opt/mssql-tools18/bin/sqlcmd
for f in /scripts/*.sql; do
  echo ">> Ejecutando $f"
  $SQLCMD -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -b -i "$f"
done
echo ">> Base de datos lista."
