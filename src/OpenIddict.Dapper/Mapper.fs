module OpenIddict.Dapper.Mapper

open AutoMapper
open System
open OpenIddict.Models
open OpenIddict.Dapper.Models

open System.Linq.Expressions
open Microsoft.FSharp.Quotations
open FSharp.Quotations.Evaluator.QuotationEvaluationExtensions

module private Expr =
  let rec private translateSimpleExpr expr =
    match expr with
    | Patterns.Var(var) ->
      // Variable access
      Expression.Variable(var.Type, var.Name) :> Expression
    | Patterns.PropertyGet(Some inst, pi, []) ->
      // Getter of an instance property
      let instExpr = translateSimpleExpr inst
      Expression.Property(instExpr, pi) :> Expression
    | Patterns.Call(Some inst, mi, args) ->
      // Method call - translate instance & arguments recursively
      let argsExpr = Seq.map translateSimpleExpr args
      let instExpr = translateSimpleExpr inst
      Expression.Call(instExpr, mi, argsExpr) :> Expression
    | Patterns.Call(None, mi, args) ->
      // Static method call - no instance
      let argsExpr = Seq.map translateSimpleExpr args
      Expression.Call(mi, argsExpr) :> Expression
    | _ -> failwith "not supported"

  let ToAutoMapperGet (expr:Expr<'a -> 'b>) =
    match expr with
    | Patterns.Lambda(v, body) ->
      // Build LINQ style lambda expression
      let bodyExpr = Expression.Convert(translateSimpleExpr body, typeof<obj>)
      let paramExpr = Expression.Parameter(v.Type, v.Name)
      Expression.Lambda<Func<'a, obj>>(bodyExpr, paramExpr)
    | _ -> failwith "not supported"

  let ToFuncExpression (expr : Expr<'a -> 'b>) =
    let call = expr.ToLinqExpressionUntyped() :?> MethodCallExpression
    let lambda = call.Arguments.[0] :?> LambdaExpression
    Expression.Lambda<Func<'a, 'b>>(lambda.Body, lambda.Parameters) 

let private forMember (destMember: Expr<'dest -> 'mbr>) 
              (memberOpts: IMemberConfigurationExpression<'source, 'dest, obj> -> unit) 
              (map: IMappingExpression<'source, 'dest>) =
  map.ForMember(Expr.ToAutoMapperGet destMember, memberOpts)

let private mapMember destMember (sourceMap:Expr<'source -> 'mapped>) =
    forMember destMember (fun o -> o.MapFrom(Expr.ToFuncExpression sourceMap))

let private application id =
  let app = new OpenIddictApplication()
  app.Id <- id.ToString()
  app

let private authorization id =
  let auth = new OpenIddictAuthorization()
  auth.Id <- id.ToString()
  auth

let private config = new MapperConfiguration(fun cfg ->
  cfg.CreateMap<Guid, string>().ConvertUsing(fun guid -> guid.ToString)
  cfg.CreateMap<string, Guid>().ConvertUsing(Guid.Parse)

  cfg.CreateMap<OpenIddictScope, OpenIddictScopeModel>().ReverseMap() |> ignore
  cfg.CreateMap<OpenIddictApplication, OpenIddictApplicationModel>().ReverseMap() |> ignore

  cfg.CreateMap<OpenIddictAuthorization, OpenIddictAuthorizationModel>()
  |> mapMember <@ fun a -> a.ApplicationId @> <@ fun a -> a.Application.Id @>
  |> ignore

  cfg.CreateMap<OpenIddictAuthorizationModel, OpenIddictAuthorization>()
  |> mapMember <@ fun a -> a.Application @> <@ fun a -> application a.ApplicationId @>
  |> ignore

  cfg.CreateMap<OpenIddictToken, OpenIddictTokenModel>()
  |> mapMember <@ fun t -> t.ApplicationId @> <@ fun t -> t.Application.Id @>
  |> mapMember <@ fun t -> t.AuthorizationId @> <@ fun t -> t.Authorization.Id @>
  |> ignore

  cfg.CreateMap<OpenIddictTokenModel, OpenIddictToken>()
  |> mapMember <@ fun t -> t.Application @> <@ fun t -> application t.ApplicationId @>
  |> mapMember <@ fun t -> t.Authorization @> <@ fun t -> authorization t.AuthorizationId @>
  |> ignore
)

let private mapper = config.CreateMapper()

let mapScope = mapper.Map<OpenIddictScope, OpenIddictScopeModel>
let mapScopeModel = mapper.Map<OpenIddictScopeModel, OpenIddictScope>

let mapApplication = mapper.Map<OpenIddictApplication, OpenIddictApplicationModel>
let mapApplicationModel = mapper.Map<OpenIddictApplicationModel, OpenIddictApplication>

let mapAuthorization = mapper.Map<OpenIddictAuthorization, OpenIddictAuthorizationModel>
let mapAuthorizationModel = mapper.Map<OpenIddictAuthorizationModel, OpenIddictAuthorization>

let mapToken = mapper.Map<OpenIddictToken, OpenIddictTokenModel>
let mapTokenModel = mapper.Map<OpenIddictTokenModel, OpenIddictToken>
