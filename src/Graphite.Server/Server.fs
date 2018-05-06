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
open Graphite.Dapper
open OpenIddict.Dapper.Extensions

open Npgsql
open Microsoft.Extensions.Configuration

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
  options.AddDapperStores() |> ignore
  if isDevelopment environment then options.DisableHttpsRequirement() |> ignore

let configureApp (app : IApplicationBuilder) =
  app.UseAuthentication()
    .UseMvcWithDefaultRoute()
    .UseStaticFiles()
    .UseGiraffe webApp

let configureServices (environment : string) (config : IConfiguration) (services : IServiceCollection) =
  services.AddMvc() |> ignore
  services.AddGiraffe() |> ignore
  services.AddOpenIddict(openIddictOptions environment) |> ignore
  let connectionString = config.GetSection("Data").["ConnectionString"]
  let db = new Db(new NpgsqlConnection(connectionString))
  services.AddScoped<Db>(fun _ -> db) |> ignore
  services.AddAuthentication(fun opt -> opt.DefaultScheme <- OAuthValidationDefaults.AuthenticationScheme)
    .AddOAuthValidation() |> ignore

let getConfig configRoot (environment : string) =
    let mutable settings =
        ConfigurationBuilder()
          .SetBasePath(configRoot)
          .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
          .AddJsonFile((sprintf "appsettings.%s.json" (environment.ToLower())), optional = true)
          .AddEnvironmentVariables()
    if environment.ToLower() = "development"
    then settings <- settings.AddUserSecretsToSettings("aspnet-Graphite-74d8bd7c-a4c6-46dd-a8cc-a7a2d63134fe")
    settings.Build()

let environment = Environment.GetEnvironmentVariable "ASPNETCORE_ENVIRONMENT"
let configRoot = Directory.GetCurrentDirectory()
let config = getConfig configRoot environment
WebHost
  .CreateDefaultBuilder()
  .UseWebRoot(clientPath)
  .UseContentRoot(clientPath)
  .Configure(Action<IApplicationBuilder> configureApp)
  .ConfigureServices(configureServices environment config)
  .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
  .Build()
  .Run()