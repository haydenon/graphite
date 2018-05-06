namespace OpenIddict.Dapper.Models

open System
open Dapper.Contrib.Extensions

[<Table("open_iddict_scopes")>]
type OpenIddictScopeModel = {
  ConcurrencyToken   : Guid
  Description        : string
  DisplayName        : string
  [<ExplicitKey>] Id : Guid
  Name               : string
  Properties         : string
  Resources          : string
}

[<Table("open_iddict_applications")>]
type OpenIddictApplicationModel = {
  ClientId               : string
  ClientSecret           : string
  ConcurrencyToken       : Guid
  ConsentType            : string
  DisplayName            : string
  [<ExplicitKey>] Id     : Guid
  Permissions            : string
  PostLogoutRedirectUris : string
  Properties             : string
  RedirectUris           : string
  Type                   : string
}

[<Table("open_iddict_authorizations")>]
type OpenIddictAuthorizationModel = {
  ApplicationId            : Guid
  ConcurrencyToken         : Guid
  [<ExplicitKey>] Id       : Guid
  Properties               : string
  Scopes                   : string
  Status                   : string
  Subject                  : string
  Type                     : string
}

[<Table("open_iddict_tokens")>]
type OpenIddictTokenModel = {
  ApplicationId            : Guid
  AuthorizationId          : Guid
  ConcurrencyToken         : Guid
  CreationDate             : DateTimeOffset
  ExpirationDate           : DateTimeOffset
  [<ExplicitKey>] Id       : Guid
  Payload                  : string
  Properties               : string
  ReferenceId              : string
  Status                   : string
  Subject                  : string
  Type                     : string
}