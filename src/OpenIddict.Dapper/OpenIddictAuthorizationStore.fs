namespace OpenIddict.Dapper

open System
open System.Threading
open System.Threading.Tasks
open System.Linq

open Microsoft.Extensions.Caching.Memory


open OpenIddict.Models
open OpenIddict.Core

open Graphite.Dapper
open OpenIddict.Dapper.Helpers
open OpenIddict.Dapper.Models
open OpenIddict.Dapper.Mapper

type OpenIddictAuthorizationStore(cache: IMemoryCache, db: Db) =
  let parseStringArray (prop : OpenIddictAuthorization -> string) = parseStringArray prop cache
  let parseScopesArray = parseStringArray (fun auth -> auth.Scopes)

  let store (self : OpenIddictApplicationStore) = self :> IOpenIddictApplicationStore<OpenIddictApplication>

  let mapRun f = mapAuthorization >> f >> ignoreTask
  let mapTask = mapTask mapAuthorizationModel
  let mapSeqTask = mapSeqTask mapAuthorizationModel

  member private this.Store = this :> IOpenIddictAuthorizationStore<OpenIddictAuthorization>

  interface IOpenIddictAuthorizationStore<OpenIddictAuthorization> with
    member _this.CountAsync (_ : CancellationToken) = db.QuerySingleAsync<int64>(fun b -> b {
      script "SELECT COUNT(*) FROM public.open_iddict_authorizations;"
    })
    member this.CountAsync (query, _ : CancellationToken) = countAsync query this.Store.ListAsync
    member _this.CreateAsync(auth, _) = mapRun db.InsertAsync auth
    member _this.DeleteAsync(auth, _) = mapRun db.DeleteAsync auth
    member _this.FindAsync(subject, client, _) =
      db.QueryAsync<OpenIddictAuthorizationModel>(fun b -> b {
        parameters (dict ["Subject", box(subject); "Client", box(Guid.Parse client)])
        script "SELECT *
        FROM public.open_iddict_authorizations
        WHERE \"Subject\" = @Subject
        AND \"ApplicationId\" = @Client;"
      }) |> mapSeqTask |> toArr
    member _this.FindAsync(subject, client, status, _) =
      db.QueryAsync<OpenIddictAuthorizationModel>(fun b -> b {
        parameters (dict ["Subject", box subject; "Client", box(Guid.Parse client); "Status", box status])
        script "SELECT *
        FROM public.open_iddict_authorizations
        WHERE \"Subject\" = @Subject
        AND \"ApplicationId\" = @Client
        AND \"Status\" = @Status;"
      }) |> mapSeqTask |> toArr
    member _this.FindAsync(subject, client, status, authType, _) =
      db.QueryAsync<OpenIddictAuthorizationModel>(fun b -> b {
        parameters (dict
          [
            "Subject", box subject;
            "Client", box(Guid.Parse client);
            "Status", box status;
            "Type", box authType
          ])
        script "SELECT *
        FROM public.open_iddict_authorizations
        WHERE \"Subject\" = @Subject
        AND \"ApplicationId\" = @Client
        AND \"Status\" = @Status
        AND \"Type\" = @Type;"
      }) |> mapSeqTask |> toArr
    member _this.FindByIdAsync(id, _) = db.GetAsync<OpenIddictAuthorizationModel> id |> mapTask
    member _this.FindBySubjectAsync(subject, _) =
      db.QueryAsync<OpenIddictAuthorizationModel>(fun b -> b {
        parameters (dict ["Subject", box subject])
        script "SELECT *
        FROM public.open_iddict_authorizations
        WHERE \"Subject\" = @Subject
        AND \"ApplicationId\" = @Client
        AND \"Status\" = @Status;"
      }) |> mapSeqTask |> toArr
    member _this.GetApplicationIdAsync (auth, _) = taskVal auth.Application.Id
    member this.GetAsync (query, state, _) = getAsync query state this.Store.ListAsync
    member _this.GetIdAsync (auth, _) = taskVal auth.Id
    member _this.GetPropertiesAsync (auth, _) = parseObject auth.Properties |> taskVal
    member _this.GetScopesAsync (auth, _) = parseScopesArray auth |> taskVal
    member _this.GetStatusAsync (auth, _) = taskVal auth.Status
    member _this.GetSubjectAsync (auth, _) = taskVal auth.Subject
    member _this.GetTypeAsync (auth, _) = taskVal auth.Type
    member _this.InstantiateAsync (_) = taskVal(new OpenIddictAuthorization())
    member _this.ListAsync (count: Nullable<int>, offset: Nullable<int>, _: CancellationToken) =
      let offset = if offset.HasValue then offset.Value else 0
      db.QueryAsync<OpenIddictAuthorizationModel>(fun b -> b{
        parameters (dict ["Limit", box count; "Offset", box offset])
        script "SELECT * FROM public.open_iddict_authorizations ORDER BY \"Id\" LIMIT @Limit OFFSET @Offset"
      }) |> mapSeqTask |> toArr
    member this.ListAsync<'TS, 'TR> (query: Func<IQueryable<OpenIddictAuthorization>, 'TS, IQueryable<'TR>>, state: 'TS, _: CancellationToken) =
      listAsync query state this.Store.ListAsync
    member _this.ListInvalidAsync (count: Nullable<int>, offset: Nullable<int>, _: CancellationToken) =
      let offset = if offset.HasValue then offset.Value else 0
      db.QueryAsync<OpenIddictAuthorizationModel>(fun b -> b{
        parameters
          (dict [
            "Limit", box count;
            "Offset", box offset;
            "StatusValid", box OpenIddictConstants.Statuses.Valid;
            "AuthAdHoc", box OpenIddictConstants.AuthorizationTypes.AdHoc
          ])
        script "SELECT auth.*
        FROM public.open_iddict_authorizations as auth
        WHERE auth.\"Status\" <> @StatusValid
        OR (auth.\"Type\" = @AuthAdHoc
        AND NOT EXISTS (
          SELECT 1
          FROM public.open_iddict_tokens as tk
          WHERE tk.\"Status\" = @StatusValid
        ))
        ORDER BY auth.\"Id\"
        LIMIT @Limit OFFSET @Offset;"
      }) |> mapSeqTask |> toArr
    member _this.SetApplicationIdAsync (auth, appId, _) =
      if isNull auth.Application then auth.Application <- new OpenIddictApplication()
      auth.Application.Id <- appId
      Task.CompletedTask
    member _this.SetPropertiesAsync (auth, properties, _) =
      auth.Properties <- objectToString properties
      Task.CompletedTask
    member _this.SetScopesAsync (auth, scopes, _) =
      auth.Scopes <- arrayToString scopes
      Task.CompletedTask
    member _this.SetStatusAsync (auth, status, _) =
      auth.Status <- status
      Task.CompletedTask
    member _this.SetSubjectAsync (auth, subject, _) =
      auth.Subject <- subject
      Task.CompletedTask
    member _this.SetTypeAsync (auth, authType, _) =
      auth.Type <- authType
      Task.CompletedTask
    member _this.UpdateAsync (auth, _) =
      mapRun db.UpdateAsync auth
 