open System
open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

open Giraffe

open Fable.Remoting.Giraffe

open AspNet.Security.OAuth.Validation

open Graphite.Shared
open OpenIddict.Core
open OpenIddict.Models
open OpenIddict.Dapper

let isDevelopment (environment : string) =
  environment.ToLower().Equals "development"

let clientPath = Path.Combine("..","Graphite.Client") |> Path.GetFullPath
let port = 8085us

let getInitCounter () : Task<Counter> = task { return 42 }

let webApp : HttpHandler =
  let counterProcotol = 
    { getInitCounter = getInitCounter >> Async.AwaitTask }
  // creates a HttpHandler for the given implementation
  remoting counterProcotol {
    use_route_builder Route.builder
  }

let openIddictOptions (environment : string) (options : OpenIddictBuilder) = 
  options.AddMvcBinders() |> ignore
  options.EnableTokenEndpoint(PathString("/connect/token")) |> ignore
  options.AllowPasswordFlow() |> ignore
  if isDevelopment environment then options.DisableHttpsRequirement() |> ignore

let configureApp (app : IApplicationBuilder) =
  app.UseAuthentication()
    .UseMvcWithDefaultRoute()
    .UseStaticFiles()
    .UseGiraffe webApp

let configureServices (environment : string) (services : IServiceCollection) =
  services.AddMvc() |> ignore
  services.AddGiraffe() |> ignore
  services.AddOpenIddict(openIddictOptions environment) |> ignore
  services.AddScoped<IOpenIddictApplicationStore<OpenIddictApplication>, OpenIddictApplicationStore>() |> ignore
  services.AddScoped<IOpenIddictAuthorizationStore<OpenIddictAuthorization>, OpenIddictAuthorizationStore>() |> ignore
  services.AddScoped<IOpenIddictScopeStore<OpenIddictScope>, OpenIddictScopeStore>() |> ignore
  services.AddScoped<IOpenIddictTokenStore<OpenIddictToken>, OpenIddictTokenStore>() |> ignore
  services.AddAuthentication(fun opt -> opt.DefaultScheme <- OAuthValidationDefaults.AuthenticationScheme)
    .AddOAuthValidation() |> ignore

let environment = Environment.GetEnvironmentVariable "ASPNETCORE_ENVIRONMENT"

WebHost
  .CreateDefaultBuilder()
  .UseWebRoot(clientPath)
  .UseContentRoot(clientPath)
  .Configure(Action<IApplicationBuilder> configureApp)
  .ConfigureServices(configureServices environment)
  .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
  .Build()
  .Run()