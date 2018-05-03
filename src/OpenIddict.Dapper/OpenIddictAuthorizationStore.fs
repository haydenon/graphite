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

type OpenIddictAuthorizationStore(cache: IMemoryCache) =
  let parseStringArray (prop : OpenIddictAuthorization -> string) = parseStringArray prop cache
  let parseScopesArray = parseStringArray (fun auth -> auth.Scopes)

  interface IOpenIddictAuthorizationStore<OpenIddictAuthorization> with
    member _this.CountAsync (_ : CancellationToken)  = taskVal 0L
    member _this.CountAsync (_query, _ : CancellationToken)  = taskVal 0L
    member _this.CreateAsync(_auth, _) = Task.CompletedTask
    member _this.DeleteAsync(_auth, _) = Task.CompletedTask
    member _this.FindAsync(_subject, _client, _) = taskVal ImmutableArray<OpenIddictAuthorization>.Empty
    member _this.FindAsync(_subject, _client, _status, _) = taskVal ImmutableArray<OpenIddictAuthorization>.Empty
    member _this.FindAsync(_subject, _client, _status, _type, _) = taskVal ImmutableArray<OpenIddictAuthorization>.Empty
    member _this.FindByIdAsync(_id, _) = taskVal(new OpenIddictAuthorization())
    member _this.FindBySubjectAsync(_subject, _) = taskVal ImmutableArray<OpenIddictAuthorization>.Empty
    member _this.GetApplicationIdAsync (auth, _) = taskVal auth.Application.Id
    member _this.GetAsync (_query, _state, _) = [] |> Seq.head |> taskVal
    member _this.GetIdAsync (auth, _) = taskVal auth.Id
    member _this.GetPropertiesAsync (auth, _) = parseObject auth.Properties |> taskVal
    member _this.GetScopesAsync (auth, _) = parseScopesArray auth |> taskVal
    member _this.GetStatusAsync (auth, _) = taskVal auth.Status
    member _this.GetSubjectAsync (auth, _) = taskVal auth.Subject
    member _this.GetTypeAsync (auth, _) = taskVal auth.Type
    member _this.InstantiateAsync (_) = taskVal(new OpenIddictAuthorization())
    member _this.ListAsync (_count: Nullable<int>, _offset: Nullable<int>, _: CancellationToken) =
      taskVal ImmutableArray<OpenIddictAuthorization>.Empty
    member _this.ListAsync<'TS, 'TR> (_query: Func<IQueryable<OpenIddictAuthorization>, 'TS, IQueryable<'TR>>, _state: 'TS, _: CancellationToken) =
      taskVal ImmutableArray<'TR>.Empty
    member _this.ListInvalidAsync (_count: Nullable<int>, _offset: Nullable<int>, _: CancellationToken) =
      taskVal ImmutableArray<OpenIddictAuthorization>.Empty
    member _this.SetApplicationIdAsync (_auth, _appId, _) = Task.CompletedTask
    member _this.SetPropertiesAsync (_auth, _properties, _) = Task.CompletedTask
    member _this.SetScopesAsync (_auth, _scopes, _) = Task.CompletedTask
    member _this.SetStatusAsync (_auth, _status, _) = Task.CompletedTask
    member _this.SetSubjectAsync (_auth, _subject, _) = Task.CompletedTask
    member _this.SetTypeAsync (_auth, _type, _) = Task.CompletedTask
    member _this.UpdateAsync (_auth, _) = Task.CompletedTask
 