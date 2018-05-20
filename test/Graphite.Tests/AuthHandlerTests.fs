namespace Graphite.Tests

open FsCheck.Xunit

open Generators
open Graphite.Server.AuthHandler
open Graphite.Server.Flow
open Graphite.Server.Helpers
open Graphite.Server.Models

open Graphite.Shared.Views
open Graphite.Shared.Errors

[<Properties(Arbitrary=[| typeof<SignInViewGenerator> |], MaxTest = 5)>]
module AuthHandler =

  [<Property>]
  let ``Lockout check returns failure when user is locked out`` (view : SignInView) =
    let result =
      success view
      |> checkLockout (fun _email -> Task.value true)
      |> Task.result
    let expected = Error(LockedOut view.Email)
    result = expected

  [<Property>]
  let ``Lockout check returns success when user is not locked out`` (view : SignInView) =
    let result =
      success view
      |> checkLockout (fun _email -> Task.value false)
      |> Task.result
    let expected = Ok(view)
    result = expected

  [<Property>]
  let ``Password sign in returns failure if result is invalid`` (view : SignInView) =
    let result =
      success view
      |> passwordSignIn (fun _signIn -> Task.value false)
      |> Task.result
    let expected = Error(IncorrectUserOrPassword view.Email)
    result = expected

  [<Property>]
  let ``Password sign in returns success if result is valid`` (view : SignInView) =
    let result =
      success view
      |> passwordSignIn (fun _signIn -> Task.value true)
      |> Task.result
    let expected = Ok(view)
    result = expected

  [<Property>]
  let ``Get user returns unexpected error if user cannot be found`` (view : SignInView) =
    let result =
      success view
      |> getUserFromEmail (fun _email -> Task.value(None))
      |> Task.result
    let expected = Error(UnexpectedError)
    result = expected

  [<Property>]
  let ``Get user returns user if user can be found`` (view : SignInView) (user : User) =
    let result =
      success view
      |> getUserFromEmail (fun _email -> Task.value(Some user))
      |> Task.result
    let expected = Ok(user)
    result = expected
