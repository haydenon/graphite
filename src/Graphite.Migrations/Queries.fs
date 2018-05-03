module Graphite.Migrations.Queries

let name value = 
  sprintf "\"%s\"" value

let str (value : string) =
  value.Replace("'", "''")
  |> sprintf "'%s'"

let checkMigrationTable tableName =
  sprintf "SELECT EXISTS (
   SELECT 1
   FROM   information_schema.tables
   WHERE  table_schema = 'public'
   AND    table_name = '%s'
   );" tableName
  
let createMigrationTable tableName =
  let tableName = name tableName
  sprintf "CREATE TABLE %s (
    position integer not null,
    name  varchar(200) not null
  );" tableName

let currentMigration tableName =
  let tableName = name tableName
  sprintf "SELECT MAX(position) FROM %s;" tableName

let currentMigrationName tableName order =
  let tableName = name tableName
  sprintf "SELECT name FROM %s WHERE position = %i;" tableName order

let insertMigration tableName (migration : IMigration) =
  let tableName = name tableName
  sprintf "INSERT INTO %s
  (position, name)
  VALUES
  (%i, %s);" tableName (Index.value migration.Index) (str migration.Name)

let removeMigration tableName (migration : IMigration) =
  let tableName = name tableName
  sprintf "DELETE FROM %s
  WHERE position = %i;" tableName (Index.value migration.Index)