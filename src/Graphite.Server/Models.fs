module Graphite.Server.Models

open Graphite.Shared.DataTypes
open WrappedString

type User = {
  Email       : EmailAddress.T
  DisplayName : StringNonEmpty256
} with 
  static member Create email displayName : User = {
    Email = email
    DisplayName = displayName
  }

type Store = {
  Id     : EntityId.T
  UserId : int
  Name   : StringNonEmpty256
} with
  static member Create id userId name : Store = {
    Id = id
    UserId = userId
    Name = name
  }
