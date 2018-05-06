module OpenIddict.Dapper.Helpers

open System
open System.Collections.Immutable
open System.Linq
open System.Threading.Tasks

open FSharp.Control.Tasks

open Microsoft.Extensions.Caching.Memory
open Newtonsoft.Json.Linq
open System.Collections.Generic
open System.Threading
open Newtonsoft.Json
open System.Collections

let inline taskVal value = Task.FromResult value

let parseStringArray (prop : 'a -> string) (cache : IMemoryCache) =
  let guid = Guid.NewGuid()
  let parse value (entry : ICacheEntry) : ImmutableArray<'b> =
    entry.SetPriority(CacheItemPriority.High)
     .SetSlidingExpiration(TimeSpan.FromMinutes(1.0)) |> ignore

    JArray.Parse(prop value)
      .Select(fun element -> element.ToObject<'b>())
      .ToImmutableArray()
  fun v -> cache.GetOrCreate(guid.ToString(), (fun entry -> (parse v) entry))

let parseObject str =
  if String.IsNullOrWhiteSpace str
  then new JObject()
  else JObject.Parse(str)

let tsk value = Task.FromResult value

let toArr (items : IEnumerable<'a> Task) = task {
  let! items = items
  return ImmutableArray.CreateRange(items)
}

let cnl () = CancellationToken.None

let countAsync (query : Func<IQueryable<'a>, IQueryable<'b>>) (getList: Nullable<int> * Nullable<int> * CancellationToken -> ImmutableArray<'a> Task) = task {
  let! items = getList(Nullable(), Nullable(), cnl())
  return
    items.AsQueryable()
    |> query.Invoke
    |> Seq.length
    |> int64
}

let listAsync (query : Func<IQueryable<'a>, 'b, IQueryable<'c>>) (state : 'b) (getList: Nullable<int> * Nullable<int> * CancellationToken -> ImmutableArray<'a> Task) = task {
  let! items = getList(Nullable(), Nullable(), cnl())
  return
    items.AsQueryable()
    |> fun q -> query.Invoke(q, state)
    |> ImmutableArray.CreateRange
}

let getAsync (query : Func<IQueryable<'a>, 'b, IQueryable<'c>>) (state : 'b) (getList: Nullable<int> * Nullable<int> * CancellationToken -> ImmutableArray<'a> Task) = task {
  let! list = listAsync query state getList
  return list |> Seq.head
}

let ignoreTask (valueTask : 'a Task) = valueTask :> Task

let objectToString (object : JObject) =
  if isNull object then
    null
  else
    object.ToString(Formatting.None)

let arrayToString (arr : ImmutableArray<'a>) =
  if arr.IsDefaultOrEmpty then
    null
  else
    let arr = new JArray(arr.ToArray())
    arr.ToString(Formatting.None)

let mapTask map (valueTask : 'a Task) = task {
  let! value = valueTask
  return map value
}

let mapSeqTask map (valueTask : 'a IEnumerable Task) = task {
  let! values = valueTask
  return Seq.map map values :> IEnumerable<'b>
}
