module Graphite.Server.Mapper

open Graphite.Data
open Graphite.Shared.Views

let mapUser (user : User) : UserView =
  {
    Email = user.Email
  }