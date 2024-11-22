#!/bin/bash

servername="$1"
databasename="$2"

for migration in ./Migrations/*.sql; do
	echo "Running migration for $migration"
	sqlcmd -S "$servername" -d "$databasename" -E -i "$migration"
done
