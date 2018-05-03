namespace Graphite.Migrations.Migrations

open Graphite.Migrations

type Migration1() =
  let scopesUp = "
CREATE TABLE open_iddict_scopes (
  concurrency_token uuid,
  description       varchar(250),
  display_name      varchar(250),
  id                uuid CONSTRAINT open_iddict_scopes_pk PRIMARY KEY,
  name              varchar(250),
  properties        varchar(1500),
  resources         varchar(1500)
);"

  let scopesDown = "DROP TABLE open_iddict_scopes;"

  let applicationUp = "
CREATE TABLE open_iddict_applications (
  client_id                 varchar(500),
  client_secret             varchar(500),
  concurrency_token         uuid,
  consent_type              varchar(250),
  display_name              varchar(250),
  id                        uuid CONSTRAINT open_iddict_applications_pk PRIMARY KEY,
  permissions               varchar(1500),
  post_logout_redirect_uris varchar(1500),
  properties                varchar(1500),
  redirect_uris             varchar(1500),
  type                      varchar(250)
);"

  let applicationDown = "DROP TABLE open_iddict_applications;"

  let authorizationUp = "
CREATE TABLE open_iddict_authorizations (
  application_id            uuid REFERENCES open_iddict_applications(id),
  concurrency_token         uuid,
  id                        uuid CONSTRAINT open_iddict_authorizations_pk PRIMARY KEY,
  properties                varchar(1500),
  scopes                    varchar(1500),
  status                    varchar(250),
  subject                   varchar(250),
  type                      varchar(250)
);"

  let authorizationDown = "DROP TABLE open_iddict_authorizations;"

  let tokenUp = "
CREATE TABLE open_iddict_tokens (
  application_id            uuid REFERENCES open_iddict_applications(id),
  authorization_id          uuid REFERENCES open_iddict_authorizations(id),
  concurrency_token         uuid,
  creation_date             timestamptz,
  expiration_date           timestamptz,
  id                        uuid CONSTRAINT open_iddict_tokens_pk PRIMARY KEY,
  payload                   varchar(1500),
  properties                varchar(1500),
  reference_id              varchar(1500),
  status                    varchar(250),
  subject                   varchar(250),
  type                      varchar(250)
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
