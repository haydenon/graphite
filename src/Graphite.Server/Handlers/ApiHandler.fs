module Graphite.Server.Handlers.Api

open Giraffe

open Graphite.Server.Handlers.Helpers
open Graphite.Server.Handlers.Auth
open Graphite.Server.Handlers.Store
open Graphite.Server.Handlers.Remote

let apiHandler : HttpHandler = 
    choose [
      subRoute "/auth" authHandler
      subRoute "/store" (mustBeAuthenticated >=> storeHandler)
      subRoute "/remote" remote
    ]
   
