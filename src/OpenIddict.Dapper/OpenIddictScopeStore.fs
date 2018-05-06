namespace OpenIddict.Dapper

open System
open System.Threading
open System.Threading.Tasks
open System.Linq

open Microsoft.Extensions.Caching.Memory

open OpenIddict.Models
open OpenIddict.Core

open OpenIddict.Dapper.Helpers
open OpenIddict.Dapper.Mapper

open Graphite.Dapper
open OpenIddict.Dapper.Models


type OpenIddictScopeStore(cache: IMemoryCache, db: Db) =
  let parseStringArray (prop : OpenIddictScope -> string) = parseStringArray prop cache
  let parseResources = parseStringArray (fun s -> s.Resources)
  let mapRun f = mapScope >> f >> ignoreTask
  let mapTask = mapTask mapScopeModel
  let mapSeqTask = mapSeqTask mapScopeModel
  member private this.Store = this :> IOpenIddictScopeStore<OpenIddictScope>

  interface IOpenIddictScopeStore<OpenIddictScope> with
    member _this.CountAsync (_ : CancellationToken) = db.QuerySingleAsync<int64>(fun b -> b {
      script "SELECT COUNT(*) FROM public.open_iddict_scopes"
    })
    member this.CountAsync (query, _ : CancellationToken) = countAsync query this.Store.ListAsync
    member _this.CreateAsync(scope, _) = mapRun db.InsertAsync scope
    member _this.DeleteAsync(scope, _) = mapRun db.DeleteAsync scope
    member _this.FindByIdAsync(id, _) = db.GetAsync<OpenIddictScopeModel> id |> mapTask
    member _this.FindByNameAsync(name, _) =
      db.QuerySingleAsync<OpenIddictScopeModel>(fun b -> b {
        parameters (dict ["Name", box name])
        script "SELECT * FROM public.open_iddict_scopes WHERE \"Name\" = @Name LIMIT 1"
      }) |> mapTask
    member _this.FindByNamesAsync(names, _) =
      db.QueryAsync<OpenIddictScopeModel>(fun b -> b {
        parameters (dict ["Names", box names])
        script "SELECT * FROM public.open_iddict_scopes WHERE \"Name\" IN @Names"
      }) |> mapSeqTask |> toArr
    member this.GetAsync (query, state, _) = getAsync query state this.Store.ListAsync
    member _this.GetDescriptionAsync (scope, _) = taskVal scope.Description
    member _this.GetDisplayNameAsync (scope, _) = taskVal scope.DisplayName
    member _this.GetIdAsync (scope, _) = taskVal scope.Id
    member _this.GetNameAsync (scope, _) = taskVal scope.Name
    member _this.GetPropertiesAsync (scope, _) = parseObject scope.Properties |> taskVal
    member _this.GetResourcesAsync (scope, _) = parseResources scope |> taskVal
    member _this.InstantiateAsync (_) = taskVal(new OpenIddictScope())
    member _this.ListAsync (count: Nullable<int>, offset: Nullable<int>, _: CancellationToken) =
      let offset = if offset.HasValue then offset.Value else 0
      db.QueryAsync<OpenIddictScopeModel>(fun b -> b{
        parameters (dict ["Limit", box count; "Offset", box offset])
        script "SELECT * FROM public.open_iddict_scopes ORDER BY \"Id\" LIMIT @Limit OFFSET @Offset"
      }) |> mapSeqTask |> toArr
    member this.ListAsync<'TS, 'TR> (query: Func<IQueryable<OpenIddictScope>, 'TS, IQueryable<'TR>>, state: 'TS, _: CancellationToken) =
      listAsync query state this.Store.ListAsync
    member _this.SetDescriptionAsync (scope, description, _) =
      scope.Description <- description
      Task.CompletedTask
    member _this.SetDisplayNameAsync (scope, display, _) =
      scope.DisplayName <- display
      Task.CompletedTask
    member _this.SetNameAsync (scope, name, _) =
      scope.Name <- name
      Task.CompletedTask
    member _this.SetPropertiesAsync (scope, properties, _) =
      scope.Properties <- objectToString properties
      Task.CompletedTask
    member _this.SetResourcesAsync (scope, resources, _) =
      scope.Resources <- arrayToString resources
      Task.CompletedTask
    member _this.UpdateAsync (scope, _) =
      mapRun db.UpdateAsync scope
 