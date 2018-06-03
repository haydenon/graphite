module Graphite.Shared.DataTypes

open System
open System.Text.RegularExpressions

open Graphite.Shared.Errors

module WrappedString = 
  type IWrappedString = 
    abstract Value : string

  let create canonicalize (isValid : string -> ValidationError option) ctor (s:string) = 
    if isNull s
    then Error(NullValue)
    else
      let s' = canonicalize s
      let errors = isValid s'
      match errors with
      | None   -> Ok(ctor s') 
      | Some err -> Error(err)

  let apply f (s:IWrappedString) = 
    s.Value |> f 

  let value s = apply id s

  let equals left right = 
    (value left) = (value right)
      
  let compareTo left right = 
    (value left).CompareTo (value right)

  let singleLineTrimmed s =
    System.Text.RegularExpressions.Regex.Replace(s,"\s"," ").Trim()

  let inline validator (cond : 'a -> bool) err value =
    if cond value then None
    else Some err

  let maxLengthValidator len =
    validator (fun (s : string) -> s.Length <= len) (MaxLength len)

  let minLengthValidator len  =
    validator (fun (s:string) -> s.Length >= len) (MinLength len)

  let minMaxLengthValidator minLen maxLen (s:string) =
    if String.IsNullOrWhiteSpace(s) || s.Length < minLen then
      Some(MinLength minLen)
    else if s.Length > maxLen then
      Some(MaxLength maxLen)
    else
      None


  type String256 = String256 of string with
    interface IWrappedString with
      member this.Value = let (String256 s) = this in s

  let string256 = create singleLineTrimmed (maxLengthValidator 256) String256 

  let convertTo256 s = apply string256 s

  type StringNonEmpty256 = StringNonEmpty256 of string with
    interface IWrappedString with
      member this.Value = let (StringNonEmpty256 s) = this in s

  let stringNonEmpty256 = create singleLineTrimmed (minMaxLengthValidator 1 256) StringNonEmpty256 

  let convertToNonEmpty256 s = apply stringNonEmpty256 s


module EmailAddress = 

  type T = EmailAddress of string with 
    interface WrappedString.IWrappedString with
      member this.Value = let (EmailAddress s) = this in s

  let create = 
    let canonicalize = WrappedString.singleLineTrimmed 
    let isValid s = 
      match WrappedString.maxLengthValidator 256 s with
      | None ->
        match Regex.IsMatch(s,@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$") with
        | true  -> None
        | false -> Some(InvalidFormat "email")
      | err -> err
    WrappedString.create canonicalize isValid EmailAddress

  let convert s = WrappedString.apply create s

module EntityId =
  type T = EntityId of int64

  let value (EntityId id) = id

  let create id =
    if id < 1L then
      Error(InvalidId id)
    else
      Ok(EntityId id)