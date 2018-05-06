// namespace OpenIddict.Dapper

// open System.Data
// open FSharp.Control.Tasks
// open FSharp.Data.Dapper

// open System.Collections.Generic

// type Db(connection: IDbConnection) =
//   let asyncToTask (asyncItem : Async<'a>) = task { return! asyncItem }
//   let connectionF () = Connection.SqlServerConnection connection

//   member _this.QuerySingleAsync<'R> (builder : QuerySingleAsyncBuilder<'R> -> Async<'R>) =
//     querySingleAsync<'R> (connectionF) |> builder |> asyncToTask

//   member _this.QueryAsync<'R> (builder : QuerySeqAsyncBuilder<'R> -> Async<IEnumerable<'R>>) =
//     querySeqAsync<'R> (connectionF) |> builder |> asyncToTask
