-- Checks the M2 definition of done against a running database. Leaves no data behind.
-- Usage: docker compose exec -T db psql -U postgres -d studentmanagement -v ON_ERROR_STOP=1 < db/verify.sql

BEGIN;

CREATE FUNCTION pg_temp.expect_error(statement text, expected_sqlstate text) RETURNS void
LANGUAGE plpgsql AS $$
BEGIN
    EXECUTE statement;
    RAISE EXCEPTION 'expected SQLSTATE % but succeeded: %', expected_sqlstate, statement;
EXCEPTION WHEN OTHERS THEN
    IF SQLSTATE <> expected_sqlstate THEN
        RAISE EXCEPTION 'expected SQLSTATE %, got % (%): %', expected_sqlstate, SQLSTATE, SQLERRM, statement;
    END IF;
END $$;

-- 23505 unique, 23514 check, 23503 foreign key
SELECT pg_temp.expect_error($$INSERT INTO teachers (code, full_name, date_of_birth) VALUES ('GV001', 'X Y', '1980-01-01')$$, '23505');
SELECT pg_temp.expect_error($$INSERT INTO teachers (code, full_name, date_of_birth) VALUES ('gv999', 'X Y', '1980-01-01')$$, '23514');
SELECT pg_temp.expect_error($$INSERT INTO teachers (code, full_name, date_of_birth) VALUES ('GV999', '   ', '1980-01-01')$$, '23514');
SELECT pg_temp.expect_error($$INSERT INTO teachers (code, full_name, date_of_birth) VALUES ('GV999', 'X Y', '1899-12-31')$$, '23514');
SELECT pg_temp.expect_error($$INSERT INTO classrooms (code, name, subject, teacher_id) VALUES ('LH999', 'X', 'Y', -1)$$, '23503');
SELECT pg_temp.expect_error($$DELETE FROM classrooms WHERE code = 'LH001'$$, '23503');
SELECT pg_temp.expect_error($$DELETE FROM teachers WHERE code = 'GV001'$$, '23503');

-- Vietnamese sort by given name
INSERT INTO teachers (code, full_name, date_of_birth) VALUES
    ('ZZ1', 'Trần Văn Đức', '1980-01-01'), ('ZZ2', 'Lê Thị Ánh', '1980-01-01'),
    ('ZZ3', 'Phạm  Giang ', '1980-01-01'), ('ZZ4', 'Võ Minh Dũng', '1980-01-01'),
    ('ZZ5', 'Bùi Thanh Bình', '1980-01-01'), ('ZZ6', 'Đỗ An', '1980-01-01');
DO $$
DECLARE actual text;
BEGIN
    SELECT string_agg(given_name, ' < ' ORDER BY given_name, full_name, id) INTO actual
    FROM teachers WHERE code LIKE 'ZZ%';
    IF actual IS DISTINCT FROM 'An < Ánh < Bình < Dũng < Đức < Giang' THEN
        RAISE EXCEPTION 'wrong sort order: %', actual;
    END IF;
END $$;

-- sm_app: data access yes, DDL no (42501 insufficient privilege)
SET ROLE sm_app;
INSERT INTO teachers (code, full_name, date_of_birth) VALUES ('ZZ9', 'Hồ Văn Nam', '1980-01-01');
SELECT pg_temp.expect_error('CREATE TABLE t_forbidden (id integer)', '42501');
SELECT pg_temp.expect_error('DROP TABLE students', '42501');
SELECT pg_temp.expect_error('ALTER TABLE students ADD COLUMN x integer', '42501');
RESET ROLE;

ROLLBACK;

\echo 'verify.sql: all checks passed'
