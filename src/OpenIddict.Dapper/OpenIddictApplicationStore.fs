namespace OpenIddict.Dapper

open System
open System.Threading
open System.Threading.Tasks
open System.Collections.Immutable
open System.Linq

open FSharp.Control.Tasks

open Microsoft.Extensions.Caching.Memory

open OpenIddict.Models
open OpenIddict.Core

open OpenIddict.Dapper.Helpers
open OpenIddict.Dapper.Mapper
open Graphite.Dapper
open Graphite.Dapper.Helpers
open OpenIddict.Dapper.Models

type OpenIddictApplicationStore(cache: IMemoryCache, db: Db) =
  let parseAppStringArray (prop : OpenIddictApplication -> string) = parseStringArray prop cache

  let parsePermissions = parseAppStringArray (fun app -> app.Permissions)

  let parsePostLogoutRedirects = parseAppStringArray (fun app -> app.PostLogoutRedirectUris)

  let parseRedirects = parseAppStringArray (fun app -> app.RedirectUris)

  let mapRun f = mapApplication >> f >> ignoreTask
  let mapTask = mapTask mapApplicationModel
  let mapSeqTask = mapSeqTask mapApplicationModel

  member private this.Store = this :> IOpenIddictApplicationStore<OpenIddictApplication>

  interface IOpenIddictApplicationStore<OpenIddictApplication> with
    member _this.CountAsync (_ : CancellationToken) = db.QuerySingleAsync<int64>(fun b -> b {
      script "SELECT COUNT(*) FROM public.open_iddict_applications"
    })
    member this.CountAsync (query, _ : CancellationToken)  = countAsync query this.Store.ListAsync
    member _this.CreateAsync(app, _) = mapRun db.InsertAsync app
    member _this.DeleteAsync(app, _) = mapRun db.DeleteAsync app
    member _this.FindByIdAsync(id, _) = db.GetAsync<OpenIddictApplicationModel> id |> mapTask
    member _this.FindByClientIdAsync(id, _) =
      db.QuerySingleAsync<OpenIddictApplicationModel>(fun b -> b {
        parameters (dict ["ClientId", box id])
        script "SELECT * FROM public.open_iddict_applications WHERE \"ClientId\" = @ClientId LIMIT 1"
      }) |> mapTask
    member _this.FindByPostLogoutRedirectUriAsync(uri, _) = task {
        let! apps =
          db.QueryAsync<OpenIddictApplicationModel>(fun b -> b {
            parameters (dict ["Uri", box(like uri)])
            script "SELECT * FROM public.open_iddict_applications WHERE \"PostLogoutRedirectUris\" LIKE @Uri LIMIT 1"
          })
        return
          apps
          |> Seq.map (mapApplicationModel >> fun app -> app, (parsePostLogoutRedirects app))
          |> Seq.filter (fun appUris -> (snd appUris).Contains(uri))
          |> Seq.map fst
          |> ImmutableArray.CreateRange
    }
    member _this.FindByRedirectUriAsync(uri, _) = task {
        let! apps =
          db.QueryAsync<OpenIddictApplicationModel>(fun b -> b {
            parameters (dict ["Uri", box(like uri)])
            script "SELECT * FROM public.open_iddict_applications WHERE \"RedirectUris\" LIKE @Uri LIMIT 1"
          })
        return
          apps
          |> Seq.map (mapApplicationModel >> fun app -> app, (parseRedirects app))
          |> Seq.filter (fun appUris -> (snd appUris).Contains(uri))
          |> Seq.map fst
          |> ImmutableArray.CreateRange
    }
    member this.GetAsync (query, state, _) = getAsync query state this.Store.ListAsync
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
    member _this.ListAsync (count: Nullable<int>, offset: Nullable<int>, _: CancellationToken) =
      let offset = if offset.HasValue then offset.Value else 0
      db.QueryAsync<OpenIddictApplicationModel>(fun b -> b{
        parameters (dict ["Limit", box count; "Offset", box offset])
        script "SELECT * FROM public.open_iddict_applications ORDER BY \"Id\" LIMIT @Limit OFFSET @Offset"
      }) |> mapSeqTask |> toArr
    member this.ListAsync<'TS, 'TR> (query: Func<IQueryable<OpenIddictApplication>, 'TS, IQueryable<'TR>>, state: 'TS, _: CancellationToken) =
      listAsync query state this.Store.ListAsync
    member _this.SetClientIdAsync (app, id, _) =
      app.ClientId <- id
      Task.CompletedTask
    member _this.SetClientSecretAsync (app, secret, _) =
      app.ClientSecret <- secret
      Task.CompletedTask
    member _this.SetClientTypeAsync (app, clientType, _) =
      app.Type <- clientType
      Task.CompletedTask
    member _this.SetConsentTypeAsync (app, consentType, _) =
      app.ConsentType <- consentType
      Task.CompletedTask
    member _this.SetDisplayNameAsync (app, display, _) =
      app.DisplayName <- display
      Task.CompletedTask
    member _this.SetPermissionsAsync (app, permissions, _) =
      app.Permissions <- arrayToString permissions
      Task.CompletedTask
    member _this.SetPostLogoutRedirectUrisAsync (app, uris, _) =
      app.PostLogoutRedirectUris <- arrayToString uris
      Task.CompletedTask
    member _this.SetPropertiesAsync (app, properties, _) =
      app.Properties <- objectToString properties
      Task.CompletedTask
    member _this.SetRedirectUrisAsync (app, uris, _) =
      app.RedirectUris <- arrayToString uris
      Task.CompletedTask
    member _this.UpdateAsync (app, _) =
      mapRun db.UpdateAsync app
