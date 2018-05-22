module Graphite.Server.Mapper

open Graphite
open Graphite.Server.Models
open Graphite.Server.Validation
open Graphite.Shared.Views
open Graphite.Shared.Errors

let mapUser (user : User) : UserView =
  {
    Email       = WrappedString.value user.Email
    DisplayName = WrappedString.value user.DisplayName
  }

let mapFromUserModel (user : Data.User) : Result<User, (ValidationError * string)> = res {
  // Use the username. The email field is normalised.
  let! email   = EmailAddress.create user.UserName |> withSource "email"
  let! display = WrappedString.stringNonEmpty256 user.DisplayName |> withSource "display name"
  return {
    Email       = email
    DisplayName = display
  }
}
