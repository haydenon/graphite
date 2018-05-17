module Tests

open Xunit

open Graphite.Server.AuthHandler
open Graphite.Server.Helpers
open Graphite.Shared.Views
open Graphite.Shared.Errors

[<Fact>]
let ``Lockout check returns failure when user is locked out`` () =
    let view: SignInView = { Email = ""; Password = ""; RememberMe = false}
    let result =
        Ok(view)
        |> Task.value
        |> checkLockout (fun _email -> Task.value(true))
        |> Task.result
    let expected = Error(LockedOut view.Email)
    Assert.True((result = expected))
