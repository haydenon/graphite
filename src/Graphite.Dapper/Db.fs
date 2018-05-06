namespace Graphite.Dapper

open System.Data
open Dapper.Contrib.Extensions
open FSharp.Control.Tasks
open FSharp.Data.Dapper

open System.Collections.Generic

type Db(connection: IDbConnection) =
  let asyncToTask (asyncItem : Async<'a>) = task { return! asyncItem }
  let connectionF () = Connection.SqlServerConnection connection

  member _this.QueryAsync<'R> (builder : QuerySeqAsyncBuilder<'R> -> Async<IEnumerable<'R>>) =
    querySeqAsync<'R> (connectionF) |> builder |> asyncToTask

  member _this.QuerySingleAsync<'R> (builder : QuerySingleAsyncBuilder<'R> -> Async<'R>) =
    querySingleAsync<'R> (connectionF) |> builder |> asyncToTask

  member _this.QuerySingleOptionAsync (builder : QuerySingleOptionAsyncBuilder<'R> -> Async<'R option>) =
    querySingleOptionAsync<'R> (connectionF) |> builder |> asyncToTask
  
  member _this.NonQueryAsync (builder : NonQueryAsyncBuilder -> Async<int>) =
    nonQueryAsync (connectionF) |> builder |> asyncToTask
  
  member _this.InsertAsync value =
    connection.InsertAsync(value)

  member _this.UpdateAsync value =
    connection.UpdateAsync(value)

  member _this.DeleteAsync value =
    connection.DeleteAsync(value)

  member _this.GetAsync<'R when 'R : not struct> id =
    connection.GetAsync<'R> id
