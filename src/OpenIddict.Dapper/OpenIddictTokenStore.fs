namespace OpenIddict.Dapper

open System
open System.Threading
open System.Threading.Tasks
open System.Collections.Immutable
open System.Linq

open OpenIddict.Models
open OpenIddict.Core

open OpenIddict.Dapper.Helpers

type OpenIddictTokenStore() =
  interface IOpenIddictTokenStore<OpenIddictToken> with
    member _this.CountAsync (_ : CancellationToken)  = taskVal 0L
    member _this.CountAsync (_query, _ : CancellationToken)  = taskVal 0L
    member _this.CreateAsync(_token, _) = Task.CompletedTask
    member _this.DeleteAsync(_token, _) = Task.CompletedTask
    member _this.FindByApplicationIdAsync(_id, _) = taskVal ImmutableArray<OpenIddictToken>.Empty
    member _this.FindByAuthorizationIdAsync(_id, _) = taskVal ImmutableArray<OpenIddictToken>.Empty
    member _this.FindByIdAsync(_id, _) = taskVal(new OpenIddictToken())
    member _this.FindByReferenceIdAsync(_id, _) = taskVal(new OpenIddictToken())
    member _this.FindBySubjectAsync(_id, _) = taskVal ImmutableArray<OpenIddictToken>.Empty
    member _this.GetApplicationIdAsync (token, _) = taskVal token.Application.Id
    member _this.GetAsync (_query, _state, _) = [] |> Seq.head |> taskVal
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
    member _this.ListAsync (_count: Nullable<int>, _offset: Nullable<int>, _: CancellationToken) =
      taskVal ImmutableArray<OpenIddictToken>.Empty
    member _this.ListAsync<'TS, 'TR> (_query: Func<IQueryable<OpenIddictToken>, 'TS, IQueryable<'TR>>, _state: 'TS, _: CancellationToken) =
      taskVal ImmutableArray<'TR>.Empty
    member _this.ListInvalidAsync (_count: Nullable<int>, _offset: Nullable<int>, _: CancellationToken) =
      taskVal ImmutableArray<OpenIddictToken>.Empty
    member _this.SetApplicationIdAsync (_token, _properties, _) = Task.CompletedTask
    member _this.SetAuthorizationIdAsync (_token, _properties, _) = Task.CompletedTask
    member _this.SetCreationDateAsync (_token, _properties, _) = Task.CompletedTask
    member _this.SetExpirationDateAsync (_token, _properties, _) = Task.CompletedTask
    member _this.SetPayloadAsync (_token, _properties, _) = Task.CompletedTask
    member _this.SetPropertiesAsync (_token, _properties, _) = Task.CompletedTask
    member _this.SetReferenceIdAsync (_token, _properties, _) = Task.CompletedTask
    member _this.SetStatusAsync (_token, _properties, _) = Task.CompletedTask
    member _this.SetSubjectAsync (_token, _properties, _) = Task.CompletedTask
    member _this.SetTokenTypeAsync (_token, _properties, _) = Task.CompletedTask
    member _this.UpdateAsync (_token, _) = Task.CompletedTask
 