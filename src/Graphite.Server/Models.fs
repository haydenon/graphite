module Graphite.Server.Models

open System

module WrappedString = 
  type IWrappedString = 
    abstract Value : string

  let create canonicalize isValid ctor (s:string) = 
    if isNull s
    then None
    else
      let s' = canonicalize s
      if isValid s'
      then Some (ctor s') 
      else None

  let apply f (s:IWrappedString) = 
    s.Value |> f 

  let value s = apply id s

  let equals left right = 
    (value left) = (value right)
      
  let compareTo left right = 
    (value left).CompareTo (value right)

  let singleLineTrimmed s =
    System.Text.RegularExpressions.Regex.Replace(s,"\s"," ").Trim()

  let maxLengthValidator len (s:string) =
    s.Length <= len 

  let minLengthValidator len (s:string) =
    s.Length >= len 

  let minMaxLengthValidator minLen maxLen (s:string) =
    not(String.IsNullOrWhiteSpace(s)) && s.Length >= minLen && s.Length <= maxLen

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
      (WrappedString.maxLengthValidator 256 s) &&
      System.Text.RegularExpressions.Regex.IsMatch(s,@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$") 
    WrappedString.create canonicalize isValid EmailAddress

  let convert s = WrappedString.apply create s

open WrappedString

type User = {
  Email       : EmailAddress.T
  DisplayName : StringNonEmpty256
}