module Graphite.Server.Mapper

open System

open Graphite
open Graphite.Server.Flow
open Graphite.Server.Flow.AppResult
open Graphite.Server.Models
open Graphite.Server.Validation

open Graphite.Shared.DataTypes
open Graphite.Shared.Views
open Graphite.Shared.Errors

module User = 
  let mapUserToView (user : User) : UserView =
    {
      Email       = WrappedString.value user.Email
      DisplayName = WrappedString.value user.DisplayName
    }

  let mapFromModel (user : Data.User) : AppResult<User> =
    User.Create
      <!> (EmailAddress.create user.UserName |> Validation.withSource "email")
      <*> (WrappedString.stringNonEmpty256 user.DisplayName |> Validation.withSource "display name")

module Store = 
  let mapStoreFromView decodeId getUserId (store : StoreView) : AppResult<Store> =
    Store.Create
      <!> (EntityId.create(decodeId store.Id) |> Validation.withSource "id")
      <*> (Result.fromOption (getUserId()) [NotAuthenticated])
      <*> (WrappedString.stringNonEmpty256 store.Name |> Validation.withSource "name")

  let mapStoreToView encodeId (store : Store) : StoreView = 
    {
      Id = encodeId(EntityId.value store.Id)
      Name = WrappedString.value store.Name
    }
