module AuthHandler

open System.Security.Claims

open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open AspNet.Security.OpenIdConnect.Primitives
open AspNet.Security.OpenIdConnect.Server

open FSharp.Control.Tasks.ContextInsensitive

let private signIn (ctx : HttpContext) scheme principal : HttpFuncResult = task {
  do! ctx.GetService<IAuthenticationService>().SignInAsync(ctx, scheme, principal, null);
  return Some ctx
}

let signInHandler : HttpHandler =
  fun (next : HttpFunc) (ctx : HttpContext) ->
    let identity =
      new ClaimsIdentity(
        OpenIdConnectServerDefaults.AuthenticationScheme,
        OpenIdConnectConstants.Claims.Name,
        OpenIdConnectConstants.Claims.Role);
    identity.AddClaim(
      new Claim(
        OpenIdConnectConstants.Claims.Subject,
        "71346D62-9BA5-4B6D-9ECA-755574D628D8",
        OpenIdConnectConstants.Destinations.AccessToken))
    identity.AddClaim(
      new Claim(
        OpenIdConnectConstants.Claims.Name,
        "Alice",
        OpenIdConnectConstants.Destinations.AccessToken));
    let principal = new ClaimsPrincipal(identity);
    let scheme = OpenIdConnectServerDefaults.AuthenticationScheme
    signIn ctx scheme principal