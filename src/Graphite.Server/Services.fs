module Graphite.Server.Services 

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity

open Graphite.Data
open Graphite.Server.Helpers
open Graphite.Server.Mapper
open System.Security.Claims
open FSharp.Control.Tasks.ContextInsensitive

type IServices =
  abstract member Get<'TService> : unit -> 'TService

type UserService(ctxAccessor : IHttpContextAccessor, usrMng: UserManager<User>, sgnMng: SignInManager<User>) =
  let ctx = ctxAccessor.HttpContext
  let isAuthenticated (usr : ClaimsPrincipal) = 
    if not(isNull usr) && not(isNull usr.Identity)
    then usr.Identity.IsAuthenticated
    else false

  member private _this.User () = ctx.User

  member this.IsAuthenticated () =
    let user = this.User()
    isAuthenticated user
    
  member this.CurrentUser () =
    let user = this.User()
    match isAuthenticated user with
    | true  -> usrMng.GetUserAsync(user) |> Task.map mapFromUserModel
    | false -> Task.value None
  
  member _this.IsLockedOut email = task {
    let! user = usrMng.FindByEmailAsync(email)
    return!
      match user with
      | NotNull -> usrMng.IsLockedOutAsync(user)
      | _ -> Task.value false
  }

  member _this.PasswordSignIn (email : string, password, remember) = task {
    let! result = sgnMng.PasswordSignInAsync(email, password, remember, false)
    return result.Succeeded
  }
  
  member _this.GetUserByEmail email = task {
    let! usr = usrMng.FindByEmailAsync email
    return
      match usr with
      | NotNull -> mapFromUserModel usr
      | _ -> None
  }