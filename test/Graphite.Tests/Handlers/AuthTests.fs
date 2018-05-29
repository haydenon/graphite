namespace Graphite.Tests

open FsCheck.Xunit

open Generators
open Graphite.Server.Handlers.Auth
open Graphite.Server.Flow
open Graphite.Server.Helpers
open Graphite.Server.Models

open Graphite.Shared.Views
open Graphite.Shared.Errors

[<Properties(Arbitrary=[| typeof<SignInViewGenerator>; typeof<RegisterViewGenerator> |], MaxTest = 5)>]
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
      |> passwordSignIn (fun _signIn -> Task.value None)
      |> Task.result
    let expected = Error(IncorrectUserOrPassword view.Email)
    result = expected

  [<Property>]
  let ``Password sign in returns success if result is valid`` (view : SignInView) (user : User) =
    let result =
      success view
      |> passwordSignIn (fun _signIn -> Task.value(Some user))
      |> Task.result
    let expected = Ok(user)
    result = expected

  [<Property>]
  let ``User sign in returns failure if user cannot be signed in`` (model : User) =
    let result =
      success model
      |> userSignIn (fun _user -> Task.value false)
      |> Task.result
    let expected = unexpectedFailure()
    result = expected

  [<Property>]
  let ``User sign in returns user if user is signed in`` (user : User) =
    let result =
      success user
      |> userSignIn (fun _user -> Task.value true)
      |> Task.result
    let expected = Ok(user)
    result = expected

  [<Property>]
  let ``Create user returns failure if user cannot be created`` (view : RegisterView) (user : User) =
    let view = { view with Email = "" }
    let result =
      success view
      |> createUser (fun _details -> Task.value(Ok user))
      |> Task.result
    let expected = Error(ValidationFailures [(InvalidFormat "email", "email")])
    result = expected

  [<Property>]
  let ``Create user returns success if user is created`` (view : RegisterView) (user : User) =
    let result =
      success view
      |> createUser (fun _details -> Task.value(Ok user))
      |> Task.result
    let expected = Ok(user)
    result = expected
