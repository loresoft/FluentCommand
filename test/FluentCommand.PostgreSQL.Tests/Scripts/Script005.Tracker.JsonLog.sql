CREATE TABLE IF NOT EXISTS "JsonLog" (
    "Id" serial NOT NULL,
    "Data" text NULL,
    "Created" timestamptz NOT NULL DEFAULT (now() at time zone 'utc'),
    CONSTRAINT "PK_JsonLog" PRIMARY KEY ("Id")
);
