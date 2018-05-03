namespace OpenIddict.Dapper

open System
open System.Threading
open System.Threading.Tasks
open System.Collections.Immutable
open System.Linq

open Microsoft.Extensions.Caching.Memory

open OpenIddict.Models
open OpenIddict.Core

open OpenIddict.Dapper.Helpers

type OpenIddictApplicationStore(cache: IMemoryCache) =
  let parseAppStringArray (prop : OpenIddictApplication -> string) = parseStringArray prop cache

  let parsePermissions = parseAppStringArray (fun app -> app.Permissions)

  let parsePostLogoutRedirects = parseAppStringArray (fun app -> app.PostLogoutRedirectUris)

  let parseRedirects = parseAppStringArray (fun app -> app.RedirectUris)

  interface IOpenIddictApplicationStore<OpenIddictApplication> with
    member _this.CountAsync (_ : CancellationToken)  = taskVal 0L
    member _this.CountAsync (_query, _ : CancellationToken)  = taskVal 0L
    member _this.CreateAsync(_app, _) = Task.CompletedTask
    member _this.DeleteAsync(_app, _) = Task.CompletedTask
    member _this.FindByIdAsync(_id, _) = taskVal(new OpenIddictApplication())
    member _this.FindByClientIdAsync(_id, _) = taskVal(new OpenIddictApplication())
    member _this.FindByPostLogoutRedirectUriAsync(_id, _) = taskVal(ImmutableArray.Empty)
    member _this.FindByRedirectUriAsync(_id, _) = taskVal(ImmutableArray.Empty)
    member _this.GetAsync (_query, _state, _) = [] |> Seq.head |> taskVal
    member _this.GetClientTypeAsync (app, _) = taskVal app.Type
    member _this.GetClientIdAsync (app, _) = taskVal app.ClientId
    member _this.GetClientSecretAsync (app, _) = taskVal app.ClientSecret
    member _this.GetConsentTypeAsync (app, _) = taskVal app.ConsentType
    member _this.GetDisplayNameAsync (app, _) = taskVal app.DisplayName
    member _this.GetIdAsync (app, _) = taskVal app.Id
    member _this.GetPermissionsAsync (app, _) = parsePermissions app |> taskVal
    member _this.GetPropertiesAsync (app, _) = parseObject app.Properties |> taskVal
    member _this.GetPostLogoutRedirectUrisAsync (app, _) = parsePostLogoutRedirects app |> taskVal
    member _this.GetRedirectUrisAsync (app, _) = parseRedirects app |> taskVal
    member _this.InstantiateAsync (_) = taskVal(new OpenIddictApplication())
    member _this.ListAsync (_count: Nullable<int>, _offset: Nullable<int>, _: CancellationToken) =
      taskVal ImmutableArray<OpenIddictApplication>.Empty
    member _this.ListAsync<'TS, 'TR> (_query: Func<IQueryable<OpenIddictApplication>, 'TS, IQueryable<'TR>>, _state: 'TS, _: CancellationToken) =
      taskVal ImmutableArray<'TR>.Empty
    member _this.SetClientIdAsync (_app, _id, _) = Task.CompletedTask
    member _this.SetClientSecretAsync (_app, _secret, _) = Task.CompletedTask
    member _this.SetClientTypeAsync (_app, _type, _) = Task.CompletedTask
    member _this.SetConsentTypeAsync (_app, _type, _) = Task.CompletedTask
    member _this.SetDisplayNameAsync (_app, _display, _) = Task.CompletedTask
    member _this.SetPermissionsAsync (_app, _permissions, _) = Task.CompletedTask
    member _this.SetPostLogoutRedirectUrisAsync (_app, _uris, _) = Task.CompletedTask
    member _this.SetPropertiesAsync (_app, _properties, _) = Task.CompletedTask
    member _this.SetRedirectUrisAsync (_app, _uris, _) = Task.CompletedTask
    member _this.UpdateAsync (_app, _) = Task.CompletedTask
  