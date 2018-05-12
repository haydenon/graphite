module Graphite.AuthHandler

open System.Threading.Tasks
open System.Net

open Giraffe

open Graphite.Flow
open Graphite.Shared.Views
open Graphite.Shared.Errors
open Graphite.Data
open Graphite.Server.Mapper
open Graphite.Server.Helpers
open Graphite.Server.Services

let checkLockout (isLockedOut : string -> bool Task) =
  !>>=! (fun (model : SignInView) -> task {
    let! lockedOut = isLockedOut(model.Email)
    return
      match lockedOut with
      | true  -> failure(LockedOut model.Email)
      | false -> Ok model
  })

let passwordSignIn (trySignIn : string * string * bool -> bool Task) = 
  !>>=! (fun (model : SignInView) -> task {
    let! result = trySignIn(model.Email, model.Password, model.RememberMe)
    return
      match result with
      | true  -> Ok model
      | false -> failure(IncorrectUserOrPassword model.Email)
  })

let getUserFromEmail (getUser : string -> User Option Task) mapUser =
  !>>=! (fun (model : SignInView) -> task {
    let! user = getUser model.Email
    return
      match user with
      | Some user -> mapUser user |> Ok
      | None      -> unexpectedFailure()
  })

let signIn
  (validate : Validator<SignInView>)
  (checkLockout : ActionStep<SignInView>)
  (passwordSignIn : ActionStep<SignInView>) 
  (getUserFromEmail : ActionStep<SignInView, UserView>)
  (view : SignInView) =
  validate view
  |> checkLockout
  |> passwordSignIn
  |> getUserFromEmail

let signInIfNotAuthenticated isAuthenticated currentUser signIn usr =
  match isAuthenticated() with
  | true -> currentUser() |> Task.map (Option.get >> Ok)
  | false -> signIn usr

let signInWithServices : Action<SignInView, UserView> =
  fun (services : IServices) (model : SignInView) ->
    let usr = services.Get<UserService>()
    let isAuthenticated = usr.IsAuthenticated
    let currentUser = usr.CurrentUser >> Task.map (Option.map mapUser)
    let validate u = Ok u |> Task.value
    let checkLockout = (checkLockout usr.IsLockedOut)
    let passwordSignIn = passwordSignIn usr.PasswordSignIn
    let getUserFromEmail = getUserFromEmail usr.GetUserByEmail mapUser
    let signIn =
      signIn validate checkLockout passwordSignIn getUserFromEmail
    signInIfNotAuthenticated isAuthenticated currentUser signIn model

let signInApi result =
  match result with
  | Ok  value -> Success value
  | Error err -> Failure(err, HttpStatusCode.BadRequest)

let authHandler : HttpHandler =
  choose [
    POST >=> choose [
      routex "/signin(/?)" >=> returnMessage signInWithServices signInApi
    ]
  ]
