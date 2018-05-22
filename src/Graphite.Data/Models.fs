namespace Graphite.Data

open System
open Identity.Dapper.Entities

type User() =
  inherit DapperIdentityUser()
  member val DisplayName: string = null with get, set
