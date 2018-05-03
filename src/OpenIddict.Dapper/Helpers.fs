module OpenIddict.Dapper.Helpers

open System
open System.Collections.Immutable
open System.Linq
open System.Threading.Tasks

open Microsoft.Extensions.Caching.Memory
open Newtonsoft.Json.Linq

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