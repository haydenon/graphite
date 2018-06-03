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
    member __.Bind(v,f) = Option.bind f v
    member __.Return v = Some v
    member __.ReturnFrom o = o
    member __.Zero () = None

let opt = OptionBuilder()

module Result =
  let get result =
    match result with
    | Ok value -> value
    | _ -> failwith "Expected result to contain value"

  let isOk = function
    | Ok _ -> true
    | _ -> false
  
  let fromOption opt ifErr =
    match opt with
    | Some value -> Ok value
    | None -> Error ifErr

type ResultBuilder() =
    member __.Bind(v,f) = Result.bind f v
    member __.Return v = Ok v
    member __.ReturnFrom r = r

let res = ResultBuilder()

let (|NotNull|_|) value = 
  if obj.ReferenceEquals(value, null) then None 
  else Some()

let (|StartsWith|_|) (arg : string) (value : string) =
  if not(isNull value) && value.StartsWith(arg) then Some ()
  else None

let tryFun1 f inp =
  try
    Some(f inp)
  with
  | _ -> None