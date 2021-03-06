module Graphite.Server.Flow

open System
open System.Net
open System.Threading.Tasks

open Microsoft.AspNetCore.Http

open Giraffe

open Graphite.Shared.Errors

type AppResult<'T> = Result<'T, AppError list>

module AppResult =
  let private apply (f : AppResult<('a -> 'b)>) (value : AppResult<'a>) =
    match (f, value) with
    | (Ok func, Ok value) -> Ok(func value)
    | (Error err1, Error err2) -> Error(List.concat [err1; err2])
    | (Error err, _) | (_, Error err) -> Error(err)

  let (<!>) (f : 'a -> 'b) (value : AppResult<'a>) : AppResult<'b> =
    match value with
    | Ok ok -> f ok |> Ok
    | Error err -> Error err

  let (<*>) (f : AppResult<('a -> 'b)>) (value : AppResult<'a>) : AppResult<'b> =
    apply f value

let failure (errors : AppError list) =
  Result.Error errors

let unexpectedFailure () =
  failure [UnexpectedError]

let success value =
  value
  |> Result.Ok
  |> Task.value

type Validator<'T> = 'T -> 'T AppResult Task

type ActionStep<'T> = 'T AppResult Task -> 'T AppResult Task

type ActionStep<'T, 'R> = 'T AppResult Task -> 'R AppResult Task

type AppAction<'T> = IServiceProvider -> 'T -> AppResult<'T> Task

type AppAction<'T, 'R> = IServiceProvider -> 'T -> AppResult<'R> Task

let bind func input =
  match input with
  | Ok s -> func s
  | err -> err

let bindToTask func input =
  match input with
  | Ok s -> func s
  | err -> Task.FromResult err

let bindFromTask func input : 'b AppResult Task =
  task {
    let! inp = input
    return
      match inp with
      | Ok s -> func s
      | Error err -> Error err
  }

let bindFromTaskToTask func input : 'b AppResult Task =
  task {
    let! inp = input
    return!
      match inp with
      | Ok s -> func s
      | Error err -> Task.FromResult <| Error err
  }

let onError func input : 'a AppResult Task =
  task {
    let! inp = input
    return
      match inp with
      | Error err -> func err
      | _ -> inp
  }

let (>>=) input func =
  bind func input

let (>>=!) input func =
  bindFromTaskToTask func input

let (!>>=!) func =
  bindFromTaskToTask func

let (!>>=) func =
  bindFromTask func

let map func =
  !>>= (func >> Ok)

let tryRun (func : 'a -> 'b AppResult Task) error input =
  try 
    func input
  with
  | _ -> failure [error] |> Task.FromResult

let teeAsync (func : 'a -> Task<unit>) input : 'a AppResult Task =
  task {
    do! func input
    return Ok input 
  }

let tee (func : 'a -> unit) input : 'a AppResult =
  func input
  Ok input 
 
let noCache handler = setHttpHeader "Cache-Control" "no-store,no-cache" >=> handler

type ApiFailure = AppError list * HttpStatusCode

type ApiResult<'T> =
    | Success of 'T
    | Failure of ApiFailure

type ApiAction<'T, 'R> = IServiceProvider -> 'T -> ApiResult<'R> Task

let toApiAction (action : IServiceProvider -> 'a AppResult) mapFailure =
  (fun services ->
    let result = action services
    match result with
    | Ok model -> Success model
    | Error err -> mapFailure err |> Failure)

let endWith (func : 'a ApiResult -> HttpFuncResult) input : HttpFuncResult =
  task {
    let! inp = input
    return! func inp
  }

let private returnError error =
  let code = snd error |> unbox
  let error = toErrorModel(fst error)
  setStatusCode code
  >=> json error
  |> noCache

let private toJson = function 
| Success model -> json model |> noCache
| Failure err -> returnError err

// type HttpContextServices(ctx: HttpContext) =
//   interface IServiceProvider with
//     member _this.Get<'TService> () = ctx.GetService<'TService>()
  
let getModel<'T> (ctx : HttpContext) : 'T AppResult Task =
  task {
    let! model = ctx.BindJsonAsync<'T>()
    return
      match model with
      | NotNull -> Ok model 
      | _       -> failure [BadModel]
  }

let mapToApi (mapper : AppResult<'R> -> ApiResult<'R>) (result : 'R AppResult Task) = task {
  let! result = result
  return mapper result
}

let returnMessage (f : AppAction<'T, 'R>) (mapper : AppResult<'R> -> ApiResult<'R>) : HttpHandler =
  fun next ctx ->
    let f = f ctx.RequestServices
    tryRun getModel<'T> BadModel ctx
    |> !>>=! f
    |> mapToApi mapper
    |> endWith (fun r -> toJson r next ctx)

let noContent (f : AppAction<'T, 'R>) (mapper : AppResult<'R> -> ApiResult<'R>) : HttpHandler =
  let toStatus =
    function
    | Success _ -> noCache(setStatusCode 204)
    | Failure err -> returnError err
  fun next ctx ->
    let f = f ctx.RequestServices
    tryRun getModel<'T> BadModel ctx
    |> !>>=! f
    |> mapToApi mapper
    |> endWith (fun r -> toStatus r next ctx)