namespace Graphite.Tests

open FsCheck.Xunit

open Generators

open Graphite.Server.Models

[<Properties(Arbitrary=[| typeof<EmailGenerator>; typeof<StrNonEmp256Generator> |])>]
module Models =
  [<Property>]
  let ``Cannot create an EmailAddress with an empty string`` () =
    let email = ""
    let model = EmailAddress.create email
    Option.isNone model

  [<Property>]
  let ``Cannot create an EmailAddress with invalid characters`` () =
    let email = "jone@a!.com"
    let model = EmailAddress.create email
    Option.isNone model

  [<Property>]
  let ``Cannot create a StringNonEmpty256 with an empty string`` () =
    let str = ""
    let model = WrappedString.stringNonEmpty256 str
    Option.isNone model

  [<Property>]
  let ``Cannot create an StringNonEmpty256 with greater than 256 characters`` () =
    let str = new string('a', 257)
    let model = WrappedString.stringNonEmpty256 str
    Option.isNone model

  [<Property>]
  let ``Can create a String256 with an empty string`` () =
    let str = ""
    let model = WrappedString.string256 str
    Option.isSome model

  [<Property>]
  let ``Cannot create an String256 with greater than 256 characters`` () =
    let str = new string('a', 257)
    let model = WrappedString.string256 str
    Option.isNone model
