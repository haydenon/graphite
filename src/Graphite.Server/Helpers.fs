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

let (|NotNull|_|) value = 
  if obj.ReferenceEquals(value, null) then None 
  else Some()