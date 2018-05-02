// module Graphite.OpenIddictStores.ApplicationStore

// open System.Threading.Tasks
// open System.Collections.Immutable
// open System.Linq
// open Microsoft.Extensions.Caching.Memory
// open System
// open Newtonsoft.Json.Linq
// open System.Threading

// open OpenIddict.Models
// open OpenIddict.Core

// let inline taskVal value = Task.FromResult value

// let parseStringArray (prop : 'a -> string) (cache : IMemoryCache) =
//   let guid = Guid.NewGuid()
//   let parse value (entry : ICacheEntry) : ImmutableArray<'b> =
//     entry.SetPriority(CacheItemPriority.High)
//      .SetSlidingExpiration(TimeSpan.FromMinutes(1.0)) |> ignore

//     JArray.Parse(prop value)
//       .Select(fun element -> element.ToObject<'b>())
//       .ToImmutableArray()
//   fun v -> cache.GetOrCreate(guid.ToString(), (fun entry -> (parse v) entry))

// let inline parseAppStringArray (prop : OpenIddictApplication -> string) = parseStringArray prop

// let parseAppPermissions = parseAppStringArray (fun app -> app.Permissions)
// let parseAppPostLogoutRedirects = parseAppStringArray (fun app -> app.PostLogoutRedirectUris)
// let parseAppRedirects = parseAppStringArray (fun app -> app.RedirectUris)

// type ApplicationStore(cache: IMemoryCache) =
//   interface IOpenIddictApplicationStore<OpenIddictApplication> with
//     member _this.CountAsync (_ : CancellationToken)  = taskVal 0L
//     member _this.CreateAsync(_app, _) = Task.CompletedTask
//     member _this.DeleteAsync(_app, _) = Task.CompletedTask
//     member _this.FindByIdAsync(_id, _) = taskVal(new OpenIddictApplication())
//     member _this.FindByClientIdAsync(_id, _) = taskVal(new OpenIddictApplication())
//     member _this.FindByPostLogoutRedirectUriAsync(_id, _) = taskVal(ImmutableArray.Empty)
//     member _this.FindByRedirectUriAsync(_id, _) = taskVal(ImmutableArray.Empty)
//     member _this.GetAsync (_query, _state, _) = [] |> Seq.head |> taskVal
//     member _this.GetClientTypeAsync (app, _) = taskVal app.Type
//     member _this.GetClientIdAsync (app, _) = taskVal app.ClientId
//     member _this.GetClientSecretAsync (app, _) = taskVal app.ClientSecret
//     member _this.GetConsentTypeAsync (app, _) = taskVal app.ConsentType
//     member _this.GetDisplayNameAsync (app, _) = taskVal app.DisplayName
//     member _this.GetIdAsync (app, _) = taskVal app.Id
//     member _this.GetPermissionsAsync (app, _) = parseAppPermissions cache app |> taskVal
//     member _this.GetPropertiesAsync (app, _) =
//       if String.IsNullOrWhiteSpace app.Properties
//       then new JObject()
//       else JObject.Parse(app.Properties)
//       |> taskVal
//     member _this.GetPostLogoutRedirectUrisAsync (app, _) = parseAppPostLogoutRedirects cache app |> taskVal
//     member _this.GetRedirectUrisAsync (app, _) = parseAppRedirects cache app |> taskVal
//     member _this.InstantiateAsync (_) = taskVal(new OpenIddictApplication())
//     member _this.ListAsync (_count: Nullable<int>, _offset: Nullable<int>, _: CancellationToken) =
//       taskVal ImmutableArray<OpenIddictApplication>.Empty
//     member _this.SetClientIdAsync (_app, _id, _) = Task.CompletedTask
//     member _this.SetClientSecretAsync (_app, _secret, _) = Task.CompletedTask
//     member _this.SetClientTypeAsync (_app, _type, _) = Task.CompletedTask
//     member _this.SetConsentTypeAsync (_app, _type, _) = Task.CompletedTask
//     member _this.SetDisplayNameAsync (_app, _display, _) = Task.CompletedTask
//     member _this.SetPermissionsAsync (_app, _permissions, _) = Task.CompletedTask
//     member _this.SetPostLogoutRedirectUrisAsync (_app, _uris, _) = Task.CompletedTask
//     member _this.SetPropertiesAsync (_app, _properties, _) = Task.CompletedTask
//     member _this.SetRedirectUrisAsync (_app, _uris, _) = Task.CompletedTask
//     member _this.UpdateAsync (_app, _) = Task.CompletedTask

//   member this.CountAsync (cancel: CancellationToken) = (this :> IOpenIddictApplicationStore<OpenIddictApplication>).CountAsync(cancel)
  
// // type ApplicationStoreResolver() =
// //   interface IOpenIddictApplicationStoreResolver with
// //     member _this.Hello = ()

// let test (appStore : ApplicationStore) =
//   let appStore = appStore :> IOpenIddictApplicationStore<OpenIddictApplication>
//   appStore.CountAsync