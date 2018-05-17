open System
open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open Giraffe

open Fable.Remoting.Giraffe

open Npgsql
open Microsoft.Extensions.Configuration

open Identity.Dapper
open Identity.Dapper.Entities
open Identity.Dapper.PostgreSQL.Models
open Identity.Dapper.PostgreSQL.Connections
open Identity.Dapper.Models

open Graphite.Shared
open Graphite.Dapper
open Graphite.Server.AuthHandler
open Graphite.Server.Services
open Graphite.Data

let isDevelopment (environment : string) =
  environment.ToLower().Equals "development"

let clientPath = Path.Combine("..","Graphite.Client") |> Path.GetFullPath
let port = 8085us

let getInitCounter<'Provider> (ctx : 'Provider) : Task<Counter> = task {
  printfn "%A" ctx
  return 42
}

type ICounterProtocol<'Provider> = {
  getInitCounter : 'Provider -> Counter Async
}

let webApp : HttpHandler =
  let counterProtocol : ICounterProtocol<IServiceProvider> = {
    getInitCounter = getInitCounter<IServiceProvider> >> Async.AwaitTask
  }
  // creates a HttpHandler for the given implementation
  let remote = remoting counterProtocol {
    use_route_builder Route.server
  }
  choose [
      subRoute "/api/auth" authHandler
      subRoute "/api/remote" remote
  ]

let cookieAuth environment (options : CookieAuthenticationOptions) =
  options.Cookie.HttpOnly <- true
  options.Cookie.SameSite <- SameSiteMode.Strict

  if not(isDevelopment environment) then
    options.Cookie.SecurePolicy <- CookieSecurePolicy.Always
  else
    options.Cookie.SecurePolicy <- CookieSecurePolicy.SameAsRequest

  options.SlidingExpiration <- true
  options.ExpireTimeSpan    <- TimeSpan.FromDays 14.0

let configureIdentity (options : IdentityOptions) =
  let pass = options.Password
  pass.RequiredLength <- 8
  pass.RequireDigit <- false
  pass.RequireUppercase <- false
  pass.RequireNonAlphanumeric <- false
  let lockout = options.Lockout
  lockout.DefaultLockoutTimeSpan <- TimeSpan.FromMinutes(5.0);
  lockout.MaxFailedAccessAttempts <- 5; 
  lockout.AllowedForNewUsers <- true;

let internalServerError : HttpHandler = setStatusCode 500 >=> json (dict ["message", "An unexpected error occurred"]) //(toErrorModel [UnexpectedError])

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> internalServerError

let configureApp (app : IApplicationBuilder) =
  app.UseAuthentication()
    .UseGiraffeErrorHandler(errorHandler)
    .UseStaticFiles()
    .UseGiraffe webApp

let configureServices (environment : string) (config : IConfiguration) (services : IServiceCollection) =
  services.AddMvc() |> ignore
  services.AddGiraffe() |> ignore
  let connectionString = config.GetSection("Data").["ConnectionString"]
  let db = new Db(new NpgsqlConnection(connectionString))
  services.AddScoped<Db>(fun _ -> db) |> ignore
  services.AddAuthentication()
    .AddCookie(cookieAuth environment) |> ignore
  services.ConfigureDapperConnectionProvider<PostgreSqlConnectionProvider>(config.GetSection("DapperIdentity"))
    .ConfigureDapperIdentityCryptography(config.GetSection("DapperIdentityCryptography")) |> ignore
  services.AddIdentity<User, DapperIdentityRole<Guid>>(Action<IdentityOptions> configureIdentity)
    .AddDapperIdentityFor<PostgreSqlConfiguration, Guid>()
    .AddDefaultTokenProviders() |> ignore
  services.AddSingleton<DapperIdentityOptions>(new DapperIdentityOptions()) |> ignore
  services.AddScoped<UserService>() |> ignore
  services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>() |> ignore

let getConfig configRoot (environment : string) =
    let mutable settings =
        ConfigurationBuilder()
          .SetBasePath(configRoot)
          .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
          .AddJsonFile((sprintf "appsettings.%s.json" (environment.ToLower())), optional = true)
          .AddEnvironmentVariables()
    if isDevelopment environment
    then settings <- settings.AddUserSecretsToSettings("aspnet-Graphite-74d8bd7c-a4c6-46dd-a8cc-a7a2d63134fe")
    settings.Build()

let configureLogging (environment : string) (builder : ILoggingBuilder) =
    let filter (l : LogLevel) =
        (isDevelopment environment
            || l.Equals LogLevel.Error)
            && not (l.Equals LogLevel.Debug)
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

let environment = Environment.GetEnvironmentVariable "ASPNETCORE_ENVIRONMENT"
let configRoot = Directory.GetCurrentDirectory()
let config = getConfig configRoot environment
WebHost
  .CreateDefaultBuilder()
  .UseWebRoot(clientPath)
  .UseContentRoot(clientPath)
  .Configure(Action<IApplicationBuilder> configureApp)
  .ConfigureServices(configureServices environment config)
  .ConfigureLogging(configureLogging(environment))
  .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
  .Build()
  .Run()