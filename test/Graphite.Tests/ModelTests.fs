namespace Graphite.Tests

open FsCheck.Xunit

open Generators

open Graphite.Server.Models
open Graphite.Server.Helpers

open Graphite.Shared.Errors
open Graphite.Shared.DataTypes

[<Properties(Arbitrary=[| typeof<EmailGenerator>; typeof<StrNonEmp256Generator> |])>]
module Models =
  [<Property>]
  let ``Cannot create an EmailAddress with an empty string`` () =
    let email = ""
    let model = EmailAddress.create email
    Error(InvalidFormat "email") = model

  [<Property>]
  let ``Cannot create an EmailAddress with invalid characters`` () =
    let email = "jone@a!.com"
    let model = EmailAddress.create email
    Error(InvalidFormat "email") = model

  [<Property>]
  let ``Cannot create a StringNonEmpty256 with an empty string`` () =
    let str = ""
    let model = WrappedString.stringNonEmpty256 str
    Error(MinLength 1) = model

  [<Property>]
  let ``Cannot create an StringNonEmpty256 with greater than 256 characters`` () =
    let str = new string('a', 257)
    let model = WrappedString.stringNonEmpty256 str
    Error(MaxLength 256) = model

  [<Property>]
  let ``Can create a String256 with an empty string`` () =
    let str = ""
    let model = WrappedString.string256 str
    Result.isOk model

  [<Property>]
  let ``Cannot create an String256 with greater than 256 characters`` () =
    let str = new string('a', 257)
    let model = WrappedString.string256 str
    Error(MaxLength 256) = model
