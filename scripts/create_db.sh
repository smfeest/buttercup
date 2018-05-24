#!/bin/bash
#
# Creates a new database
#
# The first argument is the database name. The remaining arguments are passed through to the MySQL
# command line tool.
#
# Usage:
#   $ create_db.sh buttercup_app -u root -p

script_path="`dirname \"$0\"`"
database_name=$1
shift
sed "s/{DatabaseName}/$database_name/g" $script_path/../db/schema.sql | mysql "$@"
