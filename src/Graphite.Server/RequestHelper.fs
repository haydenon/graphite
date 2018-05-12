module Graphite.RequestHelpers

// open System.Net
// open System.Threading.Tasks
// open FSharp.Control.Tasks.ContextInsensitive

// open Graphite.Flow
// open Graphite.Errors

// let validate (validateFunc : IServices -> 'a -> string list Task) =
//   !>>=! (fun inp : 'a Result Task -> task {
//     let! results = validateFunc inp
//     return
//       match results with
//       | [] -> Success inp
//       | errors -> failure (ValidationFailures errors) HttpStatusCode.BadRequest
//   })
