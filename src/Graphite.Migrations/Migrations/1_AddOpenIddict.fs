namespace Graphite.Migrations.Migrations

open Graphite.Migrations

type Migration1() =

  let schemaUp = "CREATE SCHEMA dbo;"
  let schemaDown = "DROP SCHEMA dbo;"
  let userUp = "
CREATE TABLE dbo.\"IdentityUser\"
(
  \"DisplayName\" character varying(256) NOT NULL,
  \"UserName\" character varying(256) NOT NULL,
  \"Email\" character varying(256) NOT NULL,
  \"EmailConfirmed\" boolean NOT NULL,
  \"PasswordHash\" text,
  \"SecurityStamp\" character varying(38),
  \"PhoneNumber\" character varying(50),
  \"PhoneNumberConfirmed\" boolean,
  \"TwoFactorEnabled\" boolean NOT NULL,
  \"LockoutEnd\" timestamp without time zone,
  \"LockoutEnabled\" boolean NOT NULL,
  \"AccessFailedCount\" integer NOT NULL,
  \"Id\" serial NOT NULL,
  CONSTRAINT \"PK_IdentityUser\" PRIMARY KEY (\"Id\")
)
WITH (
  OIDS=FALSE
);"

  let userDown = "DROP TABLE dbo.\"IdentityUser\";"

  let roleUp = "
CREATE TABLE dbo.\"IdentityRole\"
(
  \"Id\" serial NOT NULL,
  \"Name\" character varying(50) NOT NULL,
  CONSTRAINT \"IdentityRole_pkey\" PRIMARY KEY (\"Id\")
)
WITH (
  OIDS=FALSE
);
"

  let roleDown = "DROP TABLE dbo.\"IdentityRole\";"

  let loginUp = "
CREATE TABLE dbo.\"IdentityLogin\"
(
  \"LoginProvider\" character varying(256) NOT NULL,
  \"ProviderKey\" character varying(128) NOT NULL,
  \"UserId\" integer NOT NULL,
  \"Name\" character varying(256) NOT NULL,
  CONSTRAINT \"IdentityLogin_pkey\" PRIMARY KEY (\"LoginProvider\", \"ProviderKey\", \"UserId\"),
  CONSTRAINT \"IdentityLogin_UserId_fkey\" FOREIGN KEY (\"UserId\")
      REFERENCES dbo.\"IdentityUser\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);"

  let loginDown = "DROP TABLE dbo.\"IdentityLogin\";"

  let claimUp = "
CREATE TABLE dbo.\"IdentityUserClaim\"
(
  \"Id\" serial NOT NULL,
  \"UserId\" integer NOT NULL,
  \"ClaimType\" character varying(256) NOT NULL,
  \"ClaimValue\" character varying(256),
  CONSTRAINT \"IdentityUserClaim_pkey\" PRIMARY KEY (\"Id\"),
  CONSTRAINT \"IdentityUserClaim_UserId_fkey\" FOREIGN KEY (\"UserId\")
      REFERENCES dbo.\"IdentityUser\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);"

  let claimDown = "DROP TABLE dbo.\"IdentityUserClaim\";"

  let userRoleUp = "
CREATE TABLE dbo.\"IdentityUserRole\"
(
  \"UserId\" integer NOT NULL,
  \"RoleId\" integer NOT NULL,
  CONSTRAINT \"IdentityUserRole_pkey\" PRIMARY KEY (\"UserId\", \"RoleId\"),
  CONSTRAINT \"IdentityUserRole_RoleId_fkey\" FOREIGN KEY (\"RoleId\")
      REFERENCES dbo.\"IdentityRole\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT \"IdentityUserRole_UserId_fkey\" FOREIGN KEY (\"UserId\")
      REFERENCES dbo.\"IdentityUser\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);"

  let userRoleDown = "DROP TABLE dbo.\"IdentityUserRole\";"

  interface IMigration with
    member _this.Index    = Index.create 1
    member _this.Name     = "Add identity tables"
    member _this.Commands = [
      { Up = schemaUp; Down = schemaDown };
      { Up = userUp; Down = userDown };
      { Up = roleUp; Down = roleDown };
      { Up = loginUp; Down = loginDown };
      { Up = claimUp; Down = claimDown };
      { Up = userRoleUp; Down = userRoleDown };
    ]
