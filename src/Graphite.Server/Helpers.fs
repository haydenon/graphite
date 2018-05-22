[<AutoOpen>]
module Graphite.Server.Helpers

open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

module Task =
  let map f (tsk : 'a Task) = task {
    let! value = tsk
    return f value
  }
  let value = Task.FromResult

  let result (tsk : 'a Task) = tsk.Result

module Option =
  let fromResult res =
    match res with
    | Ok value -> Some value
    | _        -> None

type OptionBuilder() =
    member _t.Bind(v,f) = Option.bind f v
    member _t.Return v = Some v
    member _t.ReturnFrom o = o
    member _t.Zero () = None

let opt = OptionBuilder()

module Result =
  let get result =
    match result with
    | Ok value -> value
    | _ -> failwith "Expected result to contain value"

  let isOk = function
    | Ok _ -> true
    | _ -> false

type ResultBuilder() =
    member _t.Bind(v,f) = Result.bind f v
    member _t.Return v = Ok v
    member _t.ReturnFrom r = r

let res = ResultBuilder()

let (|NotNull|_|) value = 
  if obj.ReferenceEquals(value, null) then None 
  else Some()

let (|StartsWith|_|) (arg : string) (value : string) =
  if not(isNull value) && value.StartsWith(arg) then Some ()
  else None