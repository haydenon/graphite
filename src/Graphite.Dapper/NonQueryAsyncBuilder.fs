namespace Graphite.Dapper 

open System.Collections

open Dapper
open FSharp.Data.Dapper
open FSharp.Data.Dapper.Table
open FSharp.Data.Dapper.QueryAsyncRunner

[<AutoOpen>]
module QuerySingleAsyncBuilder =
  let private unwrapScript state =
    match state.Script with
    | Some s -> s
    | None   -> failwith "Script should not be empty"

  let private unwrapParameters state =
    match state.Parameters with
    | Some p -> p
    | None   -> null

  let runNonQuery state specificConnection = async {
    let script = unwrapScript state
    let parameters = unwrapParameters state

    let row = Scope state.Tables state.Values specificConnection (fun connection -> 
        connection.ExecuteAsync(script, parameters) |> Async.AwaitTask
    )

    return! row
  }

  type NonQueryAsyncBuilder(connectionF: unit -> Connection) =

    member __.Run state = runNonQuery state (connectionF())

    member __.Yield (()) = 
        { Script     = None
          Tables     = []
          Values     = []
          Parameters = None }

    [<CustomOperation("script")>]
    member __.Script (state, content : string) = 
        { state with Script = Some content } 

    [<CustomOperation("table")>]
    member __.Table (state, name : string, rows : IEnumerable) =            
        { state with Tables = { Name = name; Rows = rows } :: state.Tables }

    [<CustomOperation("values")>]
    member __.Values (state, name, rows) = 
        { state with Values = { Name = name; Rows = rows } :: state.Values }

    [<CustomOperation("parameters")>]
    member __.Parameters (state, parameters : obj) = 
        { state with Parameters = Some parameters } 

  let nonQueryAsync connectionF = new NonQueryAsyncBuilder(connectionF)