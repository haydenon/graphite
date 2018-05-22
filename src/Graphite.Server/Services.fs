module Graphite.Server.Services 

open System

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity

open FSharp.Control.Tasks.ContextInsensitive

open Graphite.Data
open Graphite.Server
open Graphite.Server.Mapper
open Graphite.Shared.Errors
open System.Security.Claims

type RegisterDetails = Models.EmailAddress.T * Models.WrappedString.StringNonEmpty256 * string

type UserService(ctxAccessor : IHttpContextAccessor, usrMng: UserManager<User>, sgnMng: SignInManager<User>) =
  let ctx = ctxAccessor.HttpContext
  let isAuthenticated (usr : ClaimsPrincipal) = 
    if not(isNull usr) && not(isNull usr.Identity)
    then usr.Identity.IsAuthenticated
    else false

  let errorForCreateResult (error : IdentityError) email =
    match error.Code with
    | "DuplicateUserName" -> DuplicateEmailAddress email
    | StartsWith "Password" -> PasswordMustContain
    | _ -> UnexpectedError

  member private _this.User () = ctx.User

  member this.IsAuthenticated () =
    let user = this.User()
    isAuthenticated user
    
  member this.CurrentUser () =
    let user = this.User()
    match isAuthenticated user with
    | true  ->
      usrMng.GetUserAsync(user)
      |> Task.map (fun usr -> 
        match usr with
        | NotNull -> mapFromUserModel usr |> Option.fromResult
        | _ -> None)
    | false -> Task.value None
  
  member _this.IsLockedOut email = task {
    let! user = usrMng.FindByEmailAsync(email)
    return!
      match user with
      | NotNull -> usrMng.IsLockedOutAsync(user)
      | _ -> Task.value false
  }

  member this.PasswordSignIn (email : string, password, remember) = task {
    let! result = sgnMng.PasswordSignInAsync(email, password, remember, false)
    if not result.Succeeded then
      return None
    else
      let! usr = this.GetUserByEmail(email)
      return
        match usr with
        | Some usr -> mapFromUserModel usr |> Option.fromResult
        | _ -> None
  }

  member this.UserSignIn (usr : Models.User) = task {
    let! usr = this.GetUserByEmail(Models.WrappedString.value usr.Email)
    if Option.isNone usr then
      return false
    else
      do! sgnMng.SignInAsync(Option.get usr, false)
      return true
  }


  member _this.CreateUser (details : RegisterDetails) = task {
    let (email, displayName, password) = details
    let email = Models.WrappedString.value email
    let user = User()
    user.Email <- email
    user.UserName <- email
    user.DisplayName <- Models.WrappedString.value displayName
    let! result = usrMng.CreateAsync(user, password)
    return
      match result.Succeeded with
      | true  -> 
        match mapFromUserModel user with
        | Error err -> Error(ValidationFailures [err])
        | Ok ok -> Ok ok
      | false -> Error (errorForCreateResult (Seq.head result.Errors) email)
  }
  
  member private _this.GetUserByEmail email = task {
    let! user = usrMng.FindByEmailAsync(email)
    return
      match user with
      | NotNull -> Some user
      | _ -> None
  }