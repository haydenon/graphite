namespace Graphite.Migrations.Migrations

open Graphite.Migrations

type M1() =
  let up =
    ""

  let down =
    ""

  interface IMigration with
    member _this.Index    = Index.create 1
    member _this.Name     = "Add Open Iddict tables"
    member _this.Commands = [{Up = up; Down = down}]
