namespace Graphite.Migrations.Migrations

open Graphite.Migrations

type Migration2() =
  let storeUp = "
CREATE TABLE public.\"Store\"
(
  \"Id\" bigserial NOT NULL,
  \"UserId\" integer NOT NULL,
  \"Name\" character varying(256) NOT NULL,
  CONSTRAINT \"PK_Store\" PRIMARY KEY (\"Id\"),
  CONSTRAINT \"Store_UserId_fkey\" FOREIGN KEY (\"UserId\")
      REFERENCES dbo.\"IdentityUser\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);"

  let storeDown = "DROP TABLE public.\"Store\";"

  let tableUp = "
CREATE TABLE public.\"StoreTable\"
(
  \"Id\" bigserial NOT NULL,
  \"StoreId\" bigint NOT NULL,
  \"Name\" character varying(50) NOT NULL,
  CONSTRAINT \"StoreTable_pkey\" PRIMARY KEY (\"Id\"),
  CONSTRAINT \"StoreTable_StoreId_fkey\" FOREIGN KEY (\"StoreId\")
      REFERENCES public.\"Store\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
"

  let tableDown = "DROP TABLE public.\"StoreTable\";"

  let fieldTypeUp = "
CREATE TABLE public.\"FieldType\"
(
  \"Id\" bigserial NOT NULL,
  \"Name\" character varying(256) NOT NULL,
  CONSTRAINT \"FieldType_pkey\" PRIMARY KEY (\"Id\")
)
WITH (
  OIDS=FALSE
);"

  let fieldTypeDown = "DROP TABLE public.\"FieldType\";"

  let fieldUp = "
CREATE TABLE public.\"Field\"
(
  \"Id\" bigserial NOT NULL,
  \"TableId\" bigint NOT NULL,
  \"TypeId\" bigint NOT NULL,
  \"Name\" character varying(256) NOT NULL,
  \"DefInt\" integer,
  \"DefStr\" character varying(2000),
  \"DefDate\" timestamptz,
  CONSTRAINT \"Field_pkey\" PRIMARY KEY (\"Id\"),
  CONSTRAINT \"Field_TableId_fkey\" FOREIGN KEY (\"TableId\")
      REFERENCES public.\"StoreTable\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT \"Field_TypeId_fkey\" FOREIGN KEY (\"TypeId\")
      REFERENCES public.\"FieldType\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);"

  let fieldDown = "DROP TABLE public.\"Field\";"

  let recordUp = "
CREATE TABLE public.\"Record\"
(
  \"Id\" bigserial NOT NULL,
  \"TableId\" bigint NOT NULL,
  CONSTRAINT \"Record_pkey\" PRIMARY KEY (\"Id\"),
  CONSTRAINT \"Record_TableId_fkey\" FOREIGN KEY (\"TableId\")
      REFERENCES public.\"StoreTable\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);"

  let recordDown = "DROP TABLE public.\"Record\";"

  let recordFieldUp = "
CREATE TABLE public.\"RecordField\"
(
  \"Id\" bigserial NOT NULL,
  \"TableId\" bigint NOT NULL,
  \"RecordId\" bigint NOT NULL,
  \"TypeId\" bigint NOT NULL,
  CONSTRAINT \"RecordField_pkey\" PRIMARY KEY (\"Id\"),
  CONSTRAINT \"RecordField_TableId_fkey\" FOREIGN KEY (\"TableId\")
      REFERENCES public.\"StoreTable\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT \"RecordField_RecordId_fkey\" FOREIGN KEY (\"RecordId\")
      REFERENCES public.\"Record\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT \"RecordField_TypeId_fkey\" FOREIGN KEY (\"TypeId\")
      REFERENCES public.\"FieldType\" (\"Id\") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);"

  let recordFieldDown = "DROP TABLE public.\"RecordField\";"

  interface IMigration with
    member _this.Index    = Index.create 2
    member _this.Name     = "Add stores, tables and records"
    member _this.Commands = [
      { Up = storeUp; Down = storeDown };
      { Up = tableUp; Down = tableDown };
      { Up = fieldTypeUp; Down = fieldTypeDown };
      { Up = fieldUp; Down = fieldDown };
      { Up = recordUp; Down = recordDown };
      { Up = recordFieldUp; Down = recordFieldDown };
    ]
