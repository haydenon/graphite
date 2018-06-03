module Graphite.Server.Handlers.Remote

open System
open System.Threading.Tasks

open FSharp.Control.Tasks.ContextInsensitive

open Fable.Remoting.Giraffe

open Graphite.Shared

let getInitCounter<'Provider> (ctx : 'Provider) : Task<Counter> = task {
  printfn "%A" ctx
  return 42
}

type ICounterProtocol<'Provider> = {
  getInitCounter : 'Provider -> Counter Async
}

let counterProtocol : ICounterProtocol<IServiceProvider> = {
  getInitCounter = getInitCounter<IServiceProvider> >> Async.AwaitTask
}
let remote = remoting counterProtocol {
  use_route_builder Route.server
}