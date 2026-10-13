#!/bin/bash
# Creates the least-privilege role the application connects with.
# A shell script because plain .sql init files cannot read environment variables;
# psql's :'var' syntax quotes the password safely.
psql -v ON_ERROR_STOP=1 -v app_password="$APP_DB_PASSWORD" \
     --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<'SQL'
CREATE ROLE sm_app LOGIN PASSWORD :'app_password';
SQL
