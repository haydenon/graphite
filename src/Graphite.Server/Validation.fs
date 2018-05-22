module Graphite.Server.Validation

let withSource source result  =
  match result with
  | Error err -> Error(err, source)
  | Ok v      -> Ok v