-- Sample queries matching what the repositories will run (M4), with plans.
-- Usage: docker compose exec -T db psql -U postgres -d studentmanagement < db/sample_queries.sql
--
-- With only 60 rows the planner rightly prefers a sequential scan, so seqscan is disabled here
-- to prove the indexes are *usable*. Remove the SET to see the plan PostgreSQL would really pick.
SET enable_seqscan = off;

\echo '--- F1/F5: page of students with classroom and teacher, sorted by Vietnamese given name'
SELECT s.code, s.full_name, s.date_of_birth, c.name AS classroom, t.full_name AS teacher
FROM students s
JOIN classrooms c ON c.id = s.classroom_id
JOIN teachers t ON t.id = c.teacher_id
ORDER BY s.given_name, s.full_name, s.id
LIMIT 10 OFFSET 0;

EXPLAIN (ANALYZE, COSTS OFF)
SELECT s.id FROM students s ORDER BY s.given_name, s.full_name, s.id LIMIT 10;

\echo '--- F6: search by code prefix (uses uq_students_code thanks to COLLATE "C")'
SELECT code, full_name FROM students WHERE code LIKE 'SV00%' ORDER BY code;

EXPLAIN (ANALYZE, COSTS OFF)
SELECT code, full_name FROM students WHERE code LIKE 'SV00%';

\echo '--- F6: exact code lookup'
EXPLAIN (ANALYZE, COSTS OFF)
SELECT * FROM students WHERE code = 'SV042';

\echo '--- F8: students of one classroom / "classroom still has students?" before delete'
EXPLAIN (ANALYZE, COSTS OFF)
SELECT EXISTS (SELECT 1 FROM students WHERE classroom_id = 1);

RESET enable_seqscan;

\echo '--- F7: class size (LEFT JOIN keeps empty classrooms)'
SELECT c.code, c.name, c.subject, count(s.id) AS student_count
FROM classrooms c
LEFT JOIN students s ON s.classroom_id = c.id
GROUP BY c.id
ORDER BY student_count DESC, c.code;

\echo '--- F7: students per subject'
SELECT c.subject, count(s.id) AS student_count
FROM classrooms c
LEFT JOIN students s ON s.classroom_id = c.id
GROUP BY c.subject
ORDER BY student_count DESC;

\echo '--- F7: age distribution'
SELECT date_part('year', age(current_date, date_of_birth))::int AS age, count(*) AS student_count
FROM students
GROUP BY 1
ORDER BY 1;
