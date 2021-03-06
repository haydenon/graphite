module Graphite.Server.Handlers.Auth

open System
open System.Threading.Tasks

open Microsoft.Extensions.DependencyInjection

open Giraffe

open Graphite.Server
open Graphite.Server.Handlers.Helpers
open Graphite.Server.Flow
open Graphite.Server.Mapper
open Graphite.Server.Services
open Graphite.Server.Models
open Graphite.Server.Validation

open Graphite.Shared.DataTypes
open Graphite.Shared.Views
open Graphite.Shared.Errors

let checkLockout (isLockedOut : string -> bool Task) =
  !>>=! (fun (model : SignInView) -> task {
    let! lockedOut = isLockedOut(model.Email)
    return
      match lockedOut with
      | true  -> failure [LockedOut model.Email]
      | false -> Ok model
  })

let passwordSignIn (trySignIn : string * string * bool -> User option Task) = 
  !>>=! (fun (model : SignInView) -> task {
    let! result = trySignIn(model.Email, model.Password, model.RememberMe)
    return
      match result with
      | Some user -> Ok user
      | None      -> failure [IncorrectUserOrPassword model.Email]
  })

let userSignIn (trySignIn : User -> bool Task) =
  !>>=! (fun (usr : User) -> task {
    let! result = trySignIn usr
    return
      match result with
      | true  -> Ok usr
      | false -> unexpectedFailure()
  })

type RegisterDetails = EmailAddress.T * WrappedString.StringNonEmpty256 * string

let createUser (tryCreate : RegisterDetails -> User AppResult Task) =
  !>>=! (fun (view : RegisterView) -> task {
    let details = res {
      let! email = EmailAddress.create view.Email |> Validation.withSource "email"
      let! displayName = WrappedString.stringNonEmpty256 view.DisplayName |> Validation.withSource "display name"
      return (email, displayName, view.Password)
    }
    return!
      match details with
      | Ok details  -> tryCreate details
      | Error errors -> Task.value(Error errors)
  })

let register
  (createUser : ActionStep<RegisterView, User>)
  (signIn : ActionStep<User>)
  (view : RegisterView) =
  success view
  |> createUser
  |> signIn
  
let registerIfNotAuthenticated isAuthenticated currentUser register mapUser view =
  match isAuthenticated() with
  | true -> currentUser() |> Task.map (Option.get >> Ok)
  | false -> register view
  |> Flow.map mapUser

let registerWithServices : AppAction<RegisterView, UserView> =
  fun (services : IServiceProvider) (model : RegisterView) ->
    let usr = services.GetRequiredService<UserService>()
    let isAuthenticated = usr.IsAuthenticated
    let currentUser = usr.CurrentUser
    let createUser = createUser usr.CreateUser
    let signIn = userSignIn usr.UserSignIn
    let register =
      register createUser signIn
    registerIfNotAuthenticated isAuthenticated currentUser register User.mapUserToView model

let signIn
  (checkLockout : ActionStep<SignInView>)
  (passwordSignIn : ActionStep<SignInView, User>) 
  (view : SignInView) =
  success view
  |> checkLockout
  |> passwordSignIn

let signInIfNotAuthenticated isAuthenticated currentUser signIn mapUser usr =
  match isAuthenticated() with
  | true -> currentUser() |> Task.map (Option.get >> Ok)
  | false -> signIn usr
  |> Flow.map mapUser

let signInWithServices : AppAction<SignInView, UserView> =
  fun services model ->
    let usr = services.GetRequiredService<UserService>()
    let isAuthenticated = usr.IsAuthenticated
    let currentUser = usr.CurrentUser
    let checkLockout = (checkLockout usr.IsLockedOut)
    let passwordSignIn = passwordSignIn usr.PasswordSignIn
    let signIn =
      signIn checkLockout passwordSignIn
    signInIfNotAuthenticated isAuthenticated currentUser signIn User.mapUserToView model

let authHandler : HttpHandler =
  choose [
    POST >=> choose [
      routex "/signin(/?)" >=> returnMessage signInWithServices defaultApi
      routex "/register(/?)" >=> returnMessage registerWithServices defaultApi
    ]
  ]
