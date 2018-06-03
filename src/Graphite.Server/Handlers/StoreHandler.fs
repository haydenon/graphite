module Graphite.Server.Handlers.Store

open System

open Giraffe

open Graphite.Server
open Graphite.Server.Handlers.Helpers
open Graphite.Server.Flow
open Graphite.Server.Mapper
open Graphite.Server.Services
open Graphite.Server.Models
open Graphite.Server.Validation

open Graphite.Shared.Views

let createStore
  (mapFromView : ActionStep<StoreView, Store>)
  (mapToView : ActionStep<Store, StoreView>)
  (view : StoreView) =
  success view 
  |> mapFromView
  |> mapToView

let createStoreWithServices : AppAction<StoreView> =
  fun services view ->
    let userId = getUserId services
    let mapFromView = !>>= (Store.mapStoreFromView(decodeId services) userId)
    let mapToView = Flow.map (Store.mapStoreToView(encodeId services))
    createStore mapFromView mapToView view

let storeHandler : HttpHandler =
  choose [
    POST >=> choose [
      routex "(/?)" >=> returnMessage createStoreWithServices defaultApi
    ]
  ]