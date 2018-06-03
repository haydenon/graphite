module Graphite.Tests.Generators

open FsCheck

open Graphite.Shared.Views
open Graphite.Shared.DataTypes

open Graphite.Server.Helpers
open Graphite.Server.Models

let createSignIn email password remember : SignInView =
    {
      Email = WrappedString.value email
      Password = password
      RememberMe = remember
    }

let createRegister email password displayName : RegisterView =
    {
      Email = WrappedString.value email
      DisplayName = WrappedString.value displayName
      Password = password
    }

let createUser email displayName : User =
    {
        Email = email
        DisplayName = displayName
    }

let emailNames = ["john"; "alex"; "samantha"; "maria"]

let domains = ["gmail.com"; "hotmail.com"]

let cartesian xs ys = 
    xs |> List.collect (fun x -> ys |> List.map (fun y -> x, y))

let emailpairs = cartesian emailNames domains

let generateEmail () =
    Gen.elements emailpairs
    |> Gen.map ((fun (n, h) -> sprintf "%s@%s.com" n h) >> EmailAddress.create >> Result.get)

let generatePassword =
    Arb.Default.NonEmptyString >> Arb.toGen >> (Gen.map (fun s -> s.ToString()))

let generateStringNonEmpty256 =
    Arb.Default.NonEmptyString
    >> Arb.filter(fun s -> s.Get.Length <= 256)
    >> Arb.toGen
    >> (Gen.map (fun s -> WrappedString.stringNonEmpty256 s.Get |> Result.get))

let names = ["John"; "Alex"; "Samantha"; "Maria"]

let generateName =
    Gen.elements names

let generateSignIn =
    createSignIn
    <!> generateEmail()
    <*> generatePassword()
    <*> (Arb.Default.Bool() |>  Arb.toGen)

let generateRegister =
    createRegister
    <!> generateEmail()
    <*> generatePassword()
    <*> generateStringNonEmpty256()

let generateUser =
    createUser
    <!> generateEmail()
    <*> generateStringNonEmpty256()

type EmailGenerator() =
    static member EmailAddress() =
        generateEmail() |> Arb.fromGen

type StrNonEmp256Generator() =
    static member StrNonEmp256() =
        generateStringNonEmpty256() |> Arb.fromGen

type SignInViewGenerator() =
    static member SignInView() =
        generateSignIn |> Arb.fromGen

type RegisterViewGenerator() =
    static member RegisterView() =
        generateRegister |> Arb.fromGen