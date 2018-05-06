module OpenIddict.Dapper.Extensions

open Microsoft.Extensions.DependencyInjection
open OpenIddict.Core
open OpenIddict.Models

type OpenIddictBuilder with
  member this.AddDapperStores() =
    this.Services.AddScoped<IOpenIddictApplicationStore<OpenIddictApplication>, OpenIddictApplicationStore>() |> ignore
    this.Services.AddScoped<IOpenIddictAuthorizationStore<OpenIddictAuthorization>, OpenIddictAuthorizationStore>() |> ignore
    this.Services.AddScoped<IOpenIddictScopeStore<OpenIddictScope>, OpenIddictScopeStore>() |> ignore
    this.Services.AddScoped<IOpenIddictTokenStore<OpenIddictToken>, OpenIddictTokenStore>() |> ignore
    this

let addDapperStores (builder : OpenIddictBuilder) =
  builder.AddDapperStores()
