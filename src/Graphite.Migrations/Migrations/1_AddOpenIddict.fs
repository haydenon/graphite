namespace Graphite.Migrations.Migrations

open Graphite.Migrations

type Migration1() =
  let up =
    "CREATE TABLE test (
      test integer
    );"

  let down =
    "DROP TABLE test;"

  interface IMigration with
    member _this.Index    = Index.create 1
    member _this.Name     = "Add Open Iddict tables"
    member _this.Commands = [{Up = up; Down = down}]
