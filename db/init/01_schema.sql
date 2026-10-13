-- Schema for the student management system. Runs once, when the data volume is empty.

-- Vietnamese ordering: An < Ánh < Bình < Dũng < Đức < Giang.
CREATE COLLATION vi_vn (provider = icu, locale = 'vi-VN');

-- Codes use the "C" collation so the unique B-tree index also serves prefix search (LIKE 'SV00%').
-- given_name is the last word of full_name; "sort by name" means sort by it for Vietnamese names.

CREATE TABLE teachers (
    id            integer GENERATED ALWAYS AS IDENTITY,
    code          varchar(20)  COLLATE "C"   NOT NULL,
    full_name     varchar(100) COLLATE vi_vn NOT NULL,
    given_name    varchar(50)  COLLATE vi_vn
                  GENERATED ALWAYS AS (regexp_replace(btrim(full_name), '^.*\s', '')) STORED,
    date_of_birth date        NOT NULL,
    version       integer     NOT NULL DEFAULT 1,
    created_at    timestamptz NOT NULL DEFAULT now(),
    updated_at    timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_teachers PRIMARY KEY (id),
    CONSTRAINT uq_teachers_code UNIQUE (code),
    CONSTRAINT ck_teachers_code CHECK (code ~ '^[A-Z0-9]{3,20}$'),
    CONSTRAINT ck_teachers_full_name CHECK (length(btrim(full_name)) > 0),
    -- Future dates and minimum age are checked in the domain: CHECK must be immutable, so no current_date.
    CONSTRAINT ck_teachers_date_of_birth CHECK (date_of_birth >= DATE '1900-01-01')
);

CREATE INDEX ix_teachers_given_name_full_name ON teachers (given_name, full_name, id);

CREATE TABLE classrooms (
    id         integer GENERATED ALWAYS AS IDENTITY,
    code       varchar(20)  COLLATE "C"   NOT NULL,
    name       varchar(100) COLLATE vi_vn NOT NULL,
    subject    varchar(100) COLLATE vi_vn NOT NULL,
    teacher_id integer     NOT NULL,
    version    integer     NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_classrooms PRIMARY KEY (id),
    CONSTRAINT uq_classrooms_code UNIQUE (code),
    CONSTRAINT ck_classrooms_code CHECK (code ~ '^[A-Z0-9]{3,20}$'),
    CONSTRAINT ck_classrooms_name CHECK (length(btrim(name)) > 0),
    CONSTRAINT ck_classrooms_subject CHECK (length(btrim(subject)) > 0),
    CONSTRAINT fk_classrooms_teachers FOREIGN KEY (teacher_id) REFERENCES teachers (id) ON DELETE RESTRICT
);

-- PostgreSQL does not index foreign keys automatically.
CREATE INDEX ix_classrooms_teacher_id ON classrooms (teacher_id);

CREATE TABLE students (
    id            integer GENERATED ALWAYS AS IDENTITY,
    code          varchar(20)  COLLATE "C"   NOT NULL,
    full_name     varchar(100) COLLATE vi_vn NOT NULL,
    given_name    varchar(50)  COLLATE vi_vn
                  GENERATED ALWAYS AS (regexp_replace(btrim(full_name), '^.*\s', '')) STORED,
    date_of_birth date        NOT NULL,
    address       varchar(255) COLLATE vi_vn NULL,
    classroom_id  integer     NOT NULL,
    version       integer     NOT NULL DEFAULT 1,
    created_at    timestamptz NOT NULL DEFAULT now(),
    updated_at    timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_students PRIMARY KEY (id),
    CONSTRAINT uq_students_code UNIQUE (code),
    CONSTRAINT ck_students_code CHECK (code ~ '^[A-Z0-9]{3,20}$'),
    CONSTRAINT ck_students_full_name CHECK (length(btrim(full_name)) > 0),
    CONSTRAINT ck_students_date_of_birth CHECK (date_of_birth >= DATE '1900-01-01'),
    CONSTRAINT fk_students_classrooms FOREIGN KEY (classroom_id) REFERENCES classrooms (id) ON DELETE RESTRICT
);

CREATE INDEX ix_students_given_name_full_name ON students (given_name, full_name, id);
CREATE INDEX ix_students_classroom_id ON students (classroom_id);

-- Application role (created by 00_app_role.sh): data access only, no DDL.
-- PostgreSQL 15+ no longer lets PUBLIC create objects in schema public, so sm_app cannot CREATE TABLE.
GRANT CONNECT ON DATABASE studentmanagement TO sm_app;
GRANT USAGE ON SCHEMA public TO sm_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO sm_app;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO sm_app;
