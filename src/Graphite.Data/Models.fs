namespace Graphite.Data

open System
open Identity.Dapper.Entities

type EntityId = int64

type User() =
  inherit DapperIdentityUser()
  member val DisplayName: string = null with get, set

type Store = {
  Id     : EntityId
  UserId : int
  Name   : string
}

type Table = {
  Id     : EntityId
  BookId : EntityId
  Name   : string
}

type FieldType = {
  Id   : EntityId
  Name : string
}

type Field = {
  Id      : EntityId
  TableId : EntityId
  TypeId  : EntityId
  Name    : string
  DefInt  : int64 option
  DefStr  : string option
  DefDate : DateTimeOffset option
}

type Record = {
  Id      : EntityId
  TableId : EntityId
}

type RecordField = {
  Id       : EntityId
  TableId  : EntityId
  RecordId : EntityId
  TypeId   : EntityId
  IntVal   : int64 option
  StrVal   : string option
  DateVal  : DateTimeOffset option
}