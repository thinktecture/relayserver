#! /bin/sh
MIGRATION_NAME=$1

usage()
{
  echo "Usage: $0 [MigrationName]"
  exit;
}

if [ -z "$MIGRATION_NAME" ]; then
  usage
fi

for d in */ ; do
  (cd "$d" && ./create-migration.sh "$MIGRATION_NAME")
done
