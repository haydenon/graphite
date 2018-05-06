module OpenIddict.Dapper.Mapper

open AutoMapper
open System
open OpenIddict.Models
open OpenIddict.Dapper.Models

open System.Linq.Expressions

let private application id =
  let app = new OpenIddictApplication()
  app.Id <- id.ToString()
  app

let private authorization id =
  let auth = new OpenIddictAuthorization()
  auth.Id <- id.ToString()
  auth


type AutoMapper.IMappingExpression<'TSource, 'TDestination> with
  member this.ForMemberFs<'TMember, 'TSourceMember>
    (destGetter : Expression<Func<'TDestination, 'TMember>>,
     sourceGetter : Expression<Func<'TSource, 'TSourceMember>>) =
    this.ForMember(destGetter, (fun opts -> opts.MapFrom(sourceGetter)))

let private config = new MapperConfiguration(fun cfg ->
  cfg.CreateMap<Guid, string>().ConvertUsing(fun guid -> guid.ToString)
  cfg.CreateMap<string, Guid>().ConvertUsing(Guid.Parse)

  cfg.CreateMap<OpenIddictScope, OpenIddictScopeModel>().ReverseMap() |> ignore
  cfg.CreateMap<OpenIddictApplication, OpenIddictApplicationModel>().ReverseMap() |> ignore

  cfg.CreateMap<OpenIddictAuthorization, OpenIddictAuthorizationModel>()
    .ForMemberFs((fun a -> a.ApplicationId), (fun a -> a.Application.Id)) |> ignore

  cfg.CreateMap<OpenIddictAuthorizationModel, OpenIddictAuthorization>()
    .ForMemberFs((fun a -> a.Application), (fun a -> application a.ApplicationId)) |> ignore

  cfg.CreateMap<OpenIddictToken, OpenIddictTokenModel>()
    .ForMemberFs((fun t -> t.ApplicationId), (fun t -> t.Application.Id))
    .ForMemberFs((fun t -> t.AuthorizationId), (fun t -> t.Authorization.Id)) |> ignore

  cfg.CreateMap<OpenIddictTokenModel, OpenIddictToken>()
    .ForMemberFs((fun t -> t.Application), (fun t -> application t.ApplicationId))
    .ForMemberFs((fun t -> t.Authorization), (fun t -> authorization t.AuthorizationId)) |> ignore
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
