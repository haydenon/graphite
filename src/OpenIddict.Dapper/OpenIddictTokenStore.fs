namespace OpenIddict.Dapper

open System
open System.Threading
open System.Threading.Tasks
open System.Linq

open OpenIddict.Models
open OpenIddict.Core

open OpenIddict.Dapper.Helpers
open OpenIddict.Dapper.Models
open OpenIddict.Dapper.Mapper
open Graphite.Dapper

type OpenIddictTokenStore(db: Db) =
  let mapTask = mapTask mapTokenModel
  let mapSeqTask = mapSeqTask mapTokenModel
  let mapRun f = mapToken >> f >> ignoreTask

  member private this.Store = this :> IOpenIddictTokenStore<OpenIddictToken>

  interface IOpenIddictTokenStore<OpenIddictToken> with
    member _this.CountAsync (_ : CancellationToken) = db.QuerySingleAsync<int64>(fun b -> b {
      script "SELECT COUNT(*) FROM public.open_iddict_tokens;"
    })
    member this.CountAsync (query, _ : CancellationToken) = countAsync query this.Store.ListAsync
    member _this.CreateAsync(token, _) = mapRun db.InsertAsync token
    member _this.DeleteAsync(token, _) = mapRun db.DeleteAsync token
    member _this.FindByApplicationIdAsync(appId, _) =
      db.QueryAsync<OpenIddictTokenModel>(fun b -> b {
        parameters (dict ["AppId", box appId])
        script "SELECT * FROM public.open_iddict_tokens WHERE \"ApplicationId\" = @AppId;"
      }) |> mapSeqTask |> toArr
    member _this.FindByAuthorizationIdAsync(authId, _) =
      db.QueryAsync<OpenIddictTokenModel>(fun b -> b {
        parameters (dict ["AuthId", box authId])
        script "SELECT * FROM public.open_iddict_tokens WHERE \"AuthorizationId\" = @AuthId;"
      }) |> mapSeqTask |> toArr
    member _this.FindByIdAsync(id, _) =  db.GetAsync<OpenIddictTokenModel> id |> mapTask
    member _this.FindByReferenceIdAsync(refId, _) =
      db.QuerySingleAsync<OpenIddictTokenModel>(fun b -> b {
        parameters (dict ["RefId", box refId])
        script "SELECT * FROM public.open_iddict_tokens WHERE \"ReferenceId\" = @RefId;"
      }) |> mapTask
    member _this.FindBySubjectAsync(subject, _) =
      db.QueryAsync<OpenIddictTokenModel>(fun b -> b {
        parameters (dict ["Subject", box subject])
        script "SELECT * FROM public.open_iddict_tokens WHERE \"Subject\" = @Subject;"
      }) |> mapSeqTask |> toArr
    member _this.GetApplicationIdAsync (token, _) = taskVal token.Application.Id
    member this.GetAsync (query, state, _) = getAsync query state this.Store.ListAsync
    member _this.GetAuthorizationIdAsync (token, _) = taskVal token.Authorization.Id
    member _this.GetCreationDateAsync (token, _) = taskVal token.CreationDate
    member _this.GetExpirationDateAsync (token, _) = taskVal token.ExpirationDate
    member _this.GetIdAsync (token, _) = taskVal token.Id
    member _this.GetPayloadAsync (token, _) = taskVal token.Payload
    member _this.GetPropertiesAsync (token, _) = parseObject token.Properties |> taskVal
    member _this.GetReferenceIdAsync (token, _) = taskVal token.ReferenceId
    member _this.GetStatusAsync (token, _) = taskVal token.Status
    member _this.GetSubjectAsync (token, _) = taskVal token.Subject
    member _this.GetTokenTypeAsync (token, _) = taskVal token.Type
    member _this.InstantiateAsync (_) = taskVal(new OpenIddictToken())
    member _this.ListAsync (count: Nullable<int>, offset: Nullable<int>, _: CancellationToken) =
      let offset = if offset.HasValue then offset.Value else 0
      db.QueryAsync<OpenIddictTokenModel>(fun b -> b{
        parameters (dict ["Limit", box count; "Offset", box offset])
        script "SELECT * FROM public.open_iddict_tokens ORDER BY \"Id\" LIMIT @Limit OFFSET @Offset"
      }) |> mapSeqTask |> toArr
    member this.ListAsync<'TS, 'TR> (query: Func<IQueryable<OpenIddictToken>, 'TS, IQueryable<'TR>>, state: 'TS, _: CancellationToken) =
      listAsync query state this.Store.ListAsync
    member _this.ListInvalidAsync (count: Nullable<int>, offset: Nullable<int>, _: CancellationToken) =
      let offset = if offset.HasValue then offset.Value else 0
      db.QueryAsync<OpenIddictTokenModel>(fun b -> b{
        parameters
          (dict [
            "Limit", box count;
            "Offset", box offset;
            "CurrentTime", box DateTimeOffset.UtcNow;
            "StatusValid", box OpenIddictConstants.Statuses.Valid;
          ])
        script "SELECT *
        FROM public.open_iddict_tokens
        WHERE \"ExpirationDate\" < @CurrentTime
        OR \"Status\" <> @StatusValid
        ORDER BY \"Id\"
        LIMIT @Limit OFFSET @Offset;"
      }) |> mapSeqTask |> toArr
    member _this.SetApplicationIdAsync (token, appId, _) =
      if isNull token.Application then token.Application <- new OpenIddictApplication()
      token.Application.Id <- appId
      Task.CompletedTask
    member _this.SetAuthorizationIdAsync (token, authId, _) =
      if isNull token.Authorization then token.Authorization <- new OpenIddictAuthorization()
      token.Authorization.Id <- authId
      Task.CompletedTask
    member _this.SetCreationDateAsync (token, creationDate, _) =
      token.CreationDate <- creationDate
      Task.CompletedTask
    member _this.SetExpirationDateAsync (token, expirationDate, _) =
      token.ExpirationDate <- expirationDate
      Task.CompletedTask
    member _this.SetPayloadAsync (token, payload, _) =
      token.Payload <- payload
      Task.CompletedTask
    member _this.SetPropertiesAsync (token, properties, _) =
      token.Properties <- objectToString properties
      Task.CompletedTask
    member _this.SetReferenceIdAsync (token, refId, _) =
      token.ReferenceId <- refId
      Task.CompletedTask
    member _this.SetStatusAsync (token, status, _) =
      token.Status <- status
      Task.CompletedTask
    member _this.SetSubjectAsync (token, subject, _) =
      token.Subject <- subject
      Task.CompletedTask
    member _this.SetTokenTypeAsync (token, tokenType, _) =
      token.Type <- tokenType
      Task.CompletedTask
    member _this.UpdateAsync (token, _) =
      mapRun db.UpdateAsync token
 