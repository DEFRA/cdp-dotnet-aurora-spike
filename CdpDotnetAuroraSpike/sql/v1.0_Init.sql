create schema "aurora-spike"

create table "aurora-spike".foos
(
    id  integer,
    bar varchar
);

alter table "aurora-spike".foos
    owner to postgres;

create table "aurora-spike".bars
(
    id  integer,
    foo varchar
);

alter table "aurora-spike".bars
    owner to postgres;

