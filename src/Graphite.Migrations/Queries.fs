module Graphite.Migrations.Queries

let name table = 
  sprintf "\"%s\"" table

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
    position integer,
    name  varchar(200)    
  );" tableName

let currentMigration tableName =
  let tableName = name tableName
  sprintf "SELECT MAX(position) FROM %s" tableName

let currentMigrationName tableName order =
  let tableName = name tableName
  sprintf "SELECT name FROM %s WHERE position = %i" tableName order