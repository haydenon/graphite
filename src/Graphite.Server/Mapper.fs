module Graphite.Server.Mapper

open Graphite
open Graphite.Server.Models
open Graphite.Shared.Views

type OptionBuilder() =
    member _t.Bind(v,f) = Option.bind f v
    member _t.Return v = Some v
    member _t.ReturnFrom o = o
    member _t.Zero () = None

let opt = OptionBuilder()

let mapUser (user : User) : UserView =
  {
    Email       = WrappedString.value user.Email
    DisplayName = WrappedString.value user.DisplayName
  }

let mapFromUserModel (user : Data.User) : User option = opt {
  let! email   = EmailAddress.create user.Email
  let! display = WrappedString.stringNonEmpty256 user.DisplayName
  return {
    Email       = email
    DisplayName = display
  }
}
