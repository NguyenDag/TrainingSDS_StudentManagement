# Kịch bản demo (10 phút)

Đi theo thứ tự 6 bài trong [Task.md](../Task.md): mỗi bài là một lớp của cùng một sản phẩm. Mỗi bước ghi rõ **làm gì** và **chỉ vào đâu**.

## Chuẩn bị (trước buổi demo)

```bash
docker compose down -v && docker compose up --build -d --wait   # DB sạch, có seed
docker compose ps                                               # 3 service healthy
```

Mở sẵn:
- Trình duyệt ở <http://localhost:8080>, thêm một tab thứ hai cùng địa chỉ (dùng cho bước concurrency)
- Terminal 1: `docker compose logs -f server`
- Terminal 2: ở thư mục repo
- IDE mở [docs/OOP_SOLID_MAP.md](OOP_SOLID_MAP.md)

## 0. Tổng quan — 1 phút

- Mở [README.md](../README.md), chỉ vào sơ đồ kiến trúc: Browser → nginx → gRPC server → PostgreSQL.
- Bảng project: mỗi bài ứng với một project. Chạy `docker compose ps` cho thấy 3 container đều healthy.

## 1. OOP (Bài 1) — 1.5 phút

- Trên UI: menu chức năng bên trái (Báo cáo, Sinh viên, Lớp học, Giáo viên).
- **Polymorphism**: thêm giáo viên sinh cách đây 17 năm → bị từ chối (tối thiểu 18). Tạo sinh viên với cùng ngày sinh → hợp lệ (tối thiểu 16).
  Chỉ vào `MinimumAge` trong [Person.cs](../src/StudentManagement.Domain/Common/Person.cs), được override trong [Student.cs](../src/StudentManagement.Domain/Students/Student.cs) và [Teacher.cs](../src/StudentManagement.Domain/Teachers/Teacher.cs).
- **Encapsulation**: setter `protected`, chỉ đổi được state qua method có kiểm tra (phần Encapsulation của OOP_SOLID_MAP).

## 2. CSDL (Bài 2) — 1.5 phút

- Mở [db/erd.md](../db/erd.md): 3 bảng, PK/FK/UNIQUE/CHECK, generated column `given_name`.
- Sắp xếp tiếng Việt bằng collation ICU:
  ```bash
  docker compose exec db psql -U postgres -d studentmanagement \
    -c "SELECT full_name FROM students ORDER BY given_name, full_name LIMIT 8;"
  ```
  Kết quả: An < Ánh < Bình < Dũng...
- Ràng buộc chặn dữ liệu sai, và role của app không được đổi schema:
  ```bash
  docker compose exec db psql -U postgres -d studentmanagement \
    -c "UPDATE students SET code = 'sv001' WHERE code = 'SV001';"      # vi phạm ck_students_code
  docker compose exec db psql -U postgres -d studentmanagement \
    -c "SET ROLE sm_app; CREATE TABLE hack (id int);"                  # permission denied
  ```

## 3. SOLID + DI + Repository (Bài 3) — 1.5 phút

- [StudentAppService.cs](../src/StudentManagement.Application/Students/StudentAppService.cs): constructor chỉ nhận interface. Nhận `IReadRepository<Classroom>` chứ không phải `IRepository` (ISP).
- [StudentManagement.Application.csproj](../src/StudentManagement.Application/StudentManagement.Application.csproj) không tham chiếu NHibernate (DIP). [ArchitectureTests.cs](../tests/StudentManagement.GrpcServer.Tests/ArchitectureTests.cs) tự động kiểm điều này.
- LSP: cùng một bộ [RepositoryContractTests](../tests/StudentManagement.Application.Tests/Repositories/RepositoryContractTests.cs) pass trên cả repository in-memory lẫn NHibernate.
- Chạy nhanh: `dotnet test tests/StudentManagement.Application.Tests`.

## 4. NHibernate (Bài 4) — 1.5 phút

- Mở trang Sinh viên. Terminal 1 (log SQL) chỉ hiện **2 câu**: một `count` và một `SELECT` có join lớp + giáo viên, không bị N+1.
  Code: `Fetch(...).ThenFetch(...)` + `ToFuture` trong [StudentRepository.cs](../src/StudentManagement.Infrastructure/Persistence/Repositories/StudentRepository.cs). Test: `A_page_of_20_students_..._takes_at_most_2_statements` trong [RepositoryQueryTests.cs](../tests/StudentManagement.Infrastructure.Tests/RepositoryQueryTests.cs).
- **Optimistic concurrency**: mở cùng một sinh viên để sửa ở cả 2 tab, lưu tab 1, rồi lưu tab 2 → server trả `Aborted`, UI hỏi có muốn tải lại bản mới nhất không. Nguồn: cột `Version` trong [Mappings.cs](../src/StudentManagement.Infrastructure/Persistence/Mappings/Mappings.cs).
- Mapping-by-code + naming convention snake_case: [SnakeCaseNamingConvention.cs](../src/StudentManagement.Infrastructure/Persistence/SnakeCaseNamingConvention.cs).

## 5. gRPC (Bài 5) — 1.5 phút

- Contract: [protos/student.proto](../protos/student.proto).
- Gọi trực tiếp, không qua UI (cần [grpcurl](https://github.com/fullstorydev/grpcurl)):
  ```bash
  grpcurl -plaintext localhost:8081 list
  grpcurl -plaintext -d '{"code":"SV001"}' localhost:8081 studentmanagement.v1.StudentService/GetStudentByCode
  grpcurl -plaintext -d '{"code":"SV999"}' localhost:8081 studentmanagement.v1.StudentService/GetStudentByCode   # NotFound
  ```
- Terminal 1: mỗi call log ra method + status + thời gian ([LoggingInterceptor.cs](../src/StudentManagement.GrpcServer/Interceptors/LoggingInterceptor.cs)).
- Bảng ánh xạ exception → status code: [ExceptionInterceptor.cs](../src/StudentManagement.GrpcServer/Interceptors/ExceptionInterceptor.cs).

## 6. Blazor + Ant Design + biểu đồ (Bài 6) — 2 phút

- **Sinh viên**: bấm cột Họ tên để sắp xếp theo tên; gõ `SV00` để lọc theo tiền tố, nhấn Enter với `SV001` để tìm chính xác; lọc theo lớp; thêm / sửa / xóa.
- **Lỗi từ server**: thêm sinh viên với mã đã tồn tại (vd. `SV001`) → thông báo tiếng Việt "mã đã tồn tại" (`AlreadyExists`). Lỗi validation từ server (`InvalidArgument` + `BadRequest`) thì hiện dưới đúng ô nhập ([GrpcErrorHandler.cs](../src/StudentManagement.Web/Services/GrpcErrorHandler.cs)).
- **Lớp học**: xóa một lớp còn sinh viên → bị chặn, có thông báo rõ ràng. Bấm vào sĩ số → chuyển sang danh sách sinh viên của lớp đó.
- **Báo cáo**: 3 thẻ tổng + 4 biểu đồ. Thêm một sinh viên, quay lại trang Báo cáo → số liệu thay đổi.
- F5 ở `/students` không bị 404 (nginx `try_files`).

## Kết (nếu còn thời gian)

- Khả năng tự hồi phục: `docker compose restart db`. Trong lúc DB khởi động lại, UI báo lỗi hệ thống; khi DB lên lại thì UI dùng tiếp bình thường mà không phải restart server.
- CI xanh trên GitHub Actions: build → test (Testcontainers) → docker build.
