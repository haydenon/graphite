namespace Graphite.Migrations.Migrations

open Graphite.Migrations

type Migration1() =
  let scopesUp = "
CREATE TABLE open_iddict_scopes (
  \"ConcurrencyToken\" uuid,
  \"Description\"      varchar(250),
  \"DisplayName\"      varchar(250),
  \"Id\"               uuid CONSTRAINT open_iddict_scopes_pk PRIMARY KEY,
  \"Name\"             varchar(250),
  \"Properties\"       varchar(1500),
  \"Resources\"        varchar(1500)
);"

  let scopesDown = "DROP TABLE open_iddict_scopes;"

  let applicationUp = "
CREATE TABLE open_iddict_applications (
  \"ClientId\"               varchar(500),
  \"ClientSecret\"           varchar(500),
  \"ConcurrencyToken\"       uuid,
  \"ConsentType\"            varchar(250),
  \"DisplayName\"            varchar(250),
  \"Id\"                     uuid CONSTRAINT open_iddict_applications_pk PRIMARY KEY,
  \"Permissions\"            varchar(1500),
  \"PostLogoutRedirectUris\" varchar(1500),
  \"Properties\"             varchar(1500),
  \"RedirectUris\"           varchar(1500),
  \"Type\"                   varchar(250)
);"

  let applicationDown = "DROP TABLE open_iddict_applications;"

  let authorizationUp = "
CREATE TABLE open_iddict_authorizations (
  \"ApplicationId\"            uuid REFERENCES open_iddict_applications(\"Id\"),
  \"ConcurrencyToken\"         uuid,
  \"Id\"                       uuid CONSTRAINT open_iddict_authorizations_pk PRIMARY KEY,
  \"Properties\"               varchar(1500),
  \"Scopes\"                   varchar(1500),
  \"Status\"                   varchar(250),
  \"Subject\"                  varchar(250),
  \"Type\"                     varchar(250)
);"

  let authorizationDown = "DROP TABLE open_iddict_authorizations;"

  let tokenUp = "
CREATE TABLE open_iddict_tokens (
  \"ApplicationId\"            uuid REFERENCES open_iddict_applications(\"Id\"),
  \"AuthorizationId\"          uuid REFERENCES open_iddict_authorizations(\"Id\"),
  \"ConcurrencyToken\"         uuid,
  \"CreationDate\"             timestamptz,
  \"ExpirationDate\"           timestamptz,
  \"Id\"                       uuid CONSTRAINT open_iddict_tokens_pk PRIMARY KEY,
  \"Payload\"                  varchar(1500),
  \"Properties\"               varchar(1500),
  \"ReferenceId\"              varchar(1500),
  \"Status\"                   varchar(250),
  \"Subject\"                  varchar(250),
  \"Type\"                     varchar(250)
);"

  let tokenDown = "DROP TABLE open_iddict_tokens;"

  interface IMigration with
    member _this.Index    = Index.create 1
    member _this.Name     = "Add Open Iddict tables"
    member _this.Commands = [
      { Up = scopesUp; Down = scopesDown };
      { Up = applicationUp; Down = applicationDown };
      { Up = authorizationUp; Down = authorizationDown };
      { Up = tokenUp; Down = tokenDown };
    ]
