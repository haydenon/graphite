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

type OpenIddictScopeStore(cache: IMemoryCache) =
  let parseStringArray (prop : OpenIddictScope -> string) = parseStringArray prop cache
  let parseResources = parseStringArray (fun s -> s.Resources)

  interface IOpenIddictScopeStore<OpenIddictScope> with
    member _this.CountAsync (_ : CancellationToken)  = taskVal 0L
    member _this.CountAsync (_query, _ : CancellationToken)  = taskVal 0L
    member _this.CreateAsync(_scope, _) = Task.CompletedTask
    member _this.DeleteAsync(_scope, _) = Task.CompletedTask
    member _this.FindByIdAsync(_id, _) = taskVal(new OpenIddictScope())
    member _this.FindByNameAsync(_name, _) = taskVal(new OpenIddictScope())
    member _this.FindByNamesAsync(_name, _) = taskVal ImmutableArray<OpenIddictScope>.Empty
    member _this.GetAsync (_query, _state, _) = [] |> Seq.head |> taskVal
    member _this.GetDescriptionAsync (scope, _) = taskVal scope.Description
    member _this.GetDisplayNameAsync (scope, _) = taskVal scope.DisplayName
    member _this.GetIdAsync (scope, _) = taskVal scope.Id
    member _this.GetNameAsync (scope, _) = taskVal scope.Name
    member _this.GetPropertiesAsync (scope, _) = parseObject scope.Properties |> taskVal
    member _this.GetResourcesAsync (scope, _) = parseResources scope |> taskVal
    member _this.InstantiateAsync (_) = taskVal(new OpenIddictScope())
    member _this.ListAsync (_count: Nullable<int>, _offset: Nullable<int>, _: CancellationToken) =
      taskVal ImmutableArray<OpenIddictScope>.Empty
    member _this.ListAsync<'TS, 'TR> (_query: Func<IQueryable<OpenIddictScope>, 'TS, IQueryable<'TR>>, _state: 'TS, _: CancellationToken) =
      taskVal ImmutableArray<'TR>.Empty
    member _this.SetDescriptionAsync (_scope, _description, _) = Task.CompletedTask
    member _this.SetDisplayNameAsync (_scope, _display, _) = Task.CompletedTask
    member _this.SetNameAsync (_scope, _name, _) = Task.CompletedTask
    member _this.SetPropertiesAsync (_scope, _properties, _) = Task.CompletedTask
    member _this.SetResourcesAsync (_scope, _resources, _) = Task.CompletedTask
    member _this.UpdateAsync (_scope, _) = Task.CompletedTask
 