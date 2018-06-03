module Graphite.Server.Validation

open Graphite.Server.Flow
open Graphite.Shared.Errors

type ValidationFailure = ValidationError * string

// type ValidationResult<'a> = Result<'a, ValidationFailure list>

module Validation =
  let withSource source result : AppResult<_> =
    match result with
    | Error err -> Error [ValidationFailure (err, source)]
    | Ok v      -> Ok v