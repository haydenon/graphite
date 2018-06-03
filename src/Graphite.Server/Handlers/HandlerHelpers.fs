module Graphite.Server.Handlers.Helpers

open System
open System.Net

open Microsoft.Extensions.DependencyInjection

open Hashids
open Hashids.HashidConfiguration

open Giraffe

open Graphite.Server.Helpers
open Graphite.Server.Flow
open Graphite.Server.Services
open Graphite.Shared.Errors

let notAuthenticated : HttpHandler = setStatusCode 401 >=> json (toErrorModel [NotAuthenticated])

let internalServerError : HttpHandler = setStatusCode 500 >=> json (dict ["message", "An unexpected error occurred"])

let mustBeAuthenticated : HttpHandler = requiresAuthentication notAuthenticated

let defaultApi result =
  match result with
  | Ok  value -> Success value
  | Error err -> Failure(err, HttpStatusCode.BadRequest)

let encodeId (services : IServiceProvider) = 
  let config = services.GetService<HashidConfiguration>()
  fun id -> Hashid.encode64 config [| id |]

let decodeId (services : IServiceProvider) = 
  let config = services.GetService<HashidConfiguration>()
  tryFun1(Hashid.decode64 config >> Array.head) >> (Option.defaultValue -1L)

let getUserId (services : IServiceProvider) =
  let usrMng = services.GetService<UserService>()
  usrMng.CurrentUserId
