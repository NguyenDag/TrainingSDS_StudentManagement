-- Sample data: 5 teachers, 8 classrooms, 60 students.
-- Names cover the tricky Vietnamese sort cases (An/Ánh/Bình/Dũng/Đức/Giang);
-- classroom sizes and ages are deliberately uneven so report charts have some shape.

INSERT INTO teachers (code, full_name, date_of_birth) VALUES
    ('GV001', 'Nguyễn Thị Hoa',   DATE '1978-04-12'),
    ('GV002', 'Trần Văn Đức',     DATE '1985-09-30'),
    ('GV003', 'Lê Minh Anh',      DATE '1990-01-05'),
    ('GV004', 'Phạm Quốc Bảo',    DATE '1972-11-21'),
    ('GV005', 'Võ Thị Thu Giang', DATE '1995-06-18');

INSERT INTO classrooms (code, name, subject, teacher_id)
SELECT c.code, c.name, c.subject, t.id
FROM (VALUES
    ('LH001', 'Lớp 10A1', 'Toán',      'GV001'),
    ('LH002', 'Lớp 10A2', 'Ngữ văn',   'GV002'),
    ('LH003', 'Lớp 11B1', 'Tiếng Anh', 'GV003'),
    ('LH004', 'Lớp 11B2', 'Vật lý',    'GV004'),
    ('LH005', 'Lớp 12C1', 'Hóa học',   'GV005'),
    ('LH006', 'Lớp 12C2', 'Toán',      'GV001'),
    ('LH007', 'Lớp 10A3', 'Sinh học',  'GV003'),
    ('LH008', 'Lớp 11B3', 'Lịch sử',   'GV002')
) AS c (code, name, subject, teacher_code)
JOIN teachers t ON t.code = c.teacher_code;

-- Deterministic combinations instead of 60 hand-written rows.
INSERT INTO students (code, full_name, date_of_birth, address, classroom_id)
SELECT
    'SV' || lpad(i::text, 3, '0'),
    (ARRAY['Nguyễn', 'Trần', 'Lê', 'Phạm', 'Hoàng', 'Huỳnh', 'Phan', 'Vũ', 'Võ', 'Đặng', 'Bùi', 'Đỗ'])[1 + i % 12]
        || ' ' || (ARRAY['Văn', 'Thị', 'Minh', 'Quốc', 'Ngọc', 'Thanh', 'Hữu'])[1 + i % 7]
        || ' ' || (ARRAY['An', 'Ánh', 'Bình', 'Dũng', 'Đức', 'Giang', 'Hà', 'Hùng', 'Khánh', 'Lan',
                         'Long', 'Mai', 'Nam', 'Ngân', 'Phúc', 'Quân', 'Sơn', 'Thảo', 'Trang', 'Uyên'])[1 + (i * 7) % 20],
    DATE '2004-01-01' + (i * 97) % 2190,
    CASE WHEN i % 6 = 0 THEN NULL
         ELSE (ARRAY['Hà Nội', 'TP. Hồ Chí Minh', 'Đà Nẵng', 'Hải Phòng', 'Cần Thơ', 'Huế', 'Nha Trang', 'Đà Lạt'])[1 + i % 8]
    END,
    c.id
FROM generate_series(1, 60) AS i
JOIN classrooms c ON c.code = 'LH' || lpad(((ARRAY[1, 1, 1, 2, 2, 3, 3, 3, 3, 4, 5, 5, 6, 7, 7, 8])[1 + i % 16])::text, 3, '0');
