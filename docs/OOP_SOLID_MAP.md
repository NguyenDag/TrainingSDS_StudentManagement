# Bản đồ OOP & SOLID

Chỉ ra file/dòng thể hiện từng nguyên tắc. Viết dần theo milestone: phần OOP từ M1 (Bài 1), phần SOLID bổ sung ở M3.

## OOP (Bài 1)

### Encapsulation (đóng gói)

State chỉ đổi được qua method có kiểm tra; object không bao giờ rơi vào trạng thái sai.

| Ở đâu | Thể hiện |
|-------|----------|
| [Person.cs:10-17](../src/StudentManagement.Domain/Common/Person.cs#L10-L17), [Student.cs:11-12](../src/StudentManagement.Domain/Students/Student.cs#L11-L12), [Classroom.cs:12-15](../src/StudentManagement.Domain/Classrooms/Classroom.cs#L12-L15), [Entity.cs:5-8](../src/StudentManagement.Domain/Common/Entity.cs#L5-L8) | Mọi property có setter `protected`; bên ngoài chỉ đọc |
| [Person.cs:36-51](../src/StudentManagement.Domain/Common/Person.cs#L36-L51), [Student.cs:29-33](../src/StudentManagement.Domain/Students/Student.cs#L29-L33), [Classroom.cs:28-37](../src/StudentManagement.Domain/Classrooms/Classroom.cs#L28-L37) | Đổi state qua method (`ChangeCode`, `Rename`, `MoveTo`, `AssignTeacher`...), method kiểm invariant rồi mới gán, sai thì ném `DomainException` |
| [Guard.cs:6](../src/StudentManagement.Domain/Common/Guard.cs#L6) | Quy tắc chuẩn hóa/kiểm tra dùng chung là `internal`, không lộ ra ngoài assembly Domain |
| [Person.cs:55](../src/StudentManagement.Domain/Common/Person.cs#L55) | Cách tính tuổi là chi tiết `private` |
| Test: [PersonTests.cs:69](../tests/StudentManagement.Domain.Tests/PersonTests.cs#L69) | Thay đổi bị từ chối thì giá trị cũ được giữ nguyên |

### Inheritance (kế thừa)

| Ở đâu | Thể hiện |
|-------|----------|
| [Entity.cs:3](../src/StudentManagement.Domain/Common/Entity.cs#L3) | Gốc chung: `Id`, `Version`, so sánh bằng theo identity |
| [Person.cs:5](../src/StudentManagement.Domain/Common/Person.cs#L5) | `Person : Entity` gom phần chung của người: mã, họ tên, ngày sinh, quy tắc tuổi |
| [Student.cs:7](../src/StudentManagement.Domain/Students/Student.cs#L7), [Teacher.cs:5](../src/StudentManagement.Domain/Teachers/Teacher.cs#L5) | `Student`/`Teacher : Person` chỉ thêm phần riêng (địa chỉ, lớp; tuổi tối thiểu) |
| [Classroom.cs:7](../src/StudentManagement.Domain/Classrooms/Classroom.cs#L7) | `Classroom : Entity` |

### Polymorphism (đa hình)

| Ở đâu | Thể hiện |
|-------|----------|
| [Person.cs:20](../src/StudentManagement.Domain/Common/Person.cs#L20) → [Student.cs:16](../src/StudentManagement.Domain/Students/Student.cs#L16) (16), [Teacher.cs:9](../src/StudentManagement.Domain/Teachers/Teacher.cs#L9) (18) | `MinimumAge` abstract, mỗi lớp con override. [Person.cs:48](../src/StudentManagement.Domain/Common/Person.cs#L48) gọi qua kiểu `Person` mà không biết lớp con nào (Template Method) |
| [Person.cs:22](../src/StudentManagement.Domain/Common/Person.cs#L22) → [Student.cs:17](../src/StudentManagement.Domain/Students/Student.cs#L17), [Teacher.cs:10](../src/StudentManagement.Domain/Teachers/Teacher.cs#L10) | `RoleName` abstract, dùng trong thông báo lỗi và `ToString` |
| [Person.cs:53](../src/StudentManagement.Domain/Common/Person.cs#L53) → [Student.cs:35](../src/StudentManagement.Domain/Students/Student.cs#L35) | `ToString()` override nhiều tầng, `Student` mở rộng bản của `Person` bằng `base.ToString()` |
| [Entity.cs:12-26](../src/StudentManagement.Domain/Common/Entity.cs#L12-L26) | Override `Equals`/`GetHashCode` của `object`; `GetUnproxiedType()` virtual để proxy lazy-load của NHibernate trả đúng kiểu thật |
| Test: [PersonTests.cs:53](../tests/StudentManagement.Domain.Tests/PersonTests.cs#L53), [PersonTests.cs:104](../tests/StudentManagement.Domain.Tests/PersonTests.cs#L104) | Cùng ngày sinh 17 tuổi: `Student` hợp lệ, `Teacher` bị từ chối |

### Abstraction (trừu tượng)

| Ở đâu | Thể hiện |
|-------|----------|
| [Entity.cs:3](../src/StudentManagement.Domain/Common/Entity.cs#L3), [Person.cs:5](../src/StudentManagement.Domain/Common/Person.cs#L5) | `abstract class`: không tạo được "một Person chung chung", chỉ tạo `Student` hoặc `Teacher` |
| [Person.cs:20-22](../src/StudentManagement.Domain/Common/Person.cs#L20-L22) | Lớp cha khai báo *cái gì* (`MinimumAge`, `RoleName`), lớp con quyết định *bao nhiêu* |
| [DomainException.cs:4](../src/StudentManagement.Domain/Exceptions/DomainException.cs#L4) | Người gọi chỉ cần bắt một loại lỗi nghiệp vụ, không cần biết quy tắc nào bị vi phạm |
| [IRepository.cs:6](../src/StudentManagement.Application/Abstractions/IRepository.cs#L6), [IUnitOfWork.cs:7](../src/StudentManagement.Application/Abstractions/IUnitOfWork.cs#L7), [IStudentAppService.cs:6](../src/StudentManagement.Application/Students/IStudentAppService.cs#L6) | Service chỉ biết interface "lưu được, đọc được, commit được", không biết NHibernate hay bộ nhớ |

## SOLID (Bài 3)

Đường đi của một use case: validator → app service → repository/unit of work (interface) → implementation (in-memory trong test của Application; NHibernate trong Infrastructure).

### S — Single Responsibility

Mỗi class chỉ có một lý do để thay đổi.

| Class | Lý do thay đổi duy nhất |
|-------|-------------------------|
| [StudentAppService.cs:11](../src/StudentManagement.Application/Students/StudentAppService.cs#L11) (và [Classroom](../src/StudentManagement.Application/Classrooms/ClassroomAppService.cs#L11), [Teacher](../src/StudentManagement.Application/Teachers/TeacherAppService.cs#L10)) | Luồng nghiệp vụ của use case (kiểm trùng, tra tham chiếu, commit) |
| [StudentInputValidator.cs:8](../src/StudentManagement.Application/Students/StudentInputValidator.cs#L8) | Quy tắc dữ liệu nhập |
| [ValidationRules.cs:10](../src/StudentManagement.Application/Common/ValidationRules.cs#L10) | Quy tắc nhập dùng chung (mã, chuỗi bắt buộc, ngày sinh) |
| [DtoMappings.cs:12](../src/StudentManagement.Application/DtoMappings.cs#L12) | Hình dạng dữ liệu trả ra |
| [StudentGrpcService.cs:10](../src/StudentManagement.GrpcServer/Services/StudentGrpcService.cs#L10) | Chuyển đổi transport (proto ↔ DTO), không chứa nghiệp vụ |
| [ExceptionInterceptor.cs](../src/StudentManagement.GrpcServer/Interceptors/ExceptionInterceptor.cs), [LoggingInterceptor.cs](../src/StudentManagement.GrpcServer/Interceptors/LoggingInterceptor.cs) | Ánh xạ lỗi sang gRPC status; ghi log mỗi call |
| [ProtoMappings.cs](../src/StudentManagement.GrpcServer/Mapping/ProtoMappings.cs) | Hình dạng message proto |
| [Messages.cs:4](../src/StudentManagement.Application/Common/Messages.cs#L4) | Câu chữ tiếng Việt hiển thị cho người dùng |
| [AppExceptions.cs:24](../src/StudentManagement.Application/Common/Exceptions/AppExceptions.cs#L24) | Mã lỗi ổn định cho client/log |
| [StudentRepository.cs:10](../src/StudentManagement.Infrastructure/Persistence/Repositories/StudentRepository.cs#L10) | Cách truy vấn dữ liệu (NHibernate LINQ) |
| [Mappings.cs](../src/StudentManagement.Infrastructure/Persistence/Mappings/Mappings.cs) | Ánh xạ class ↔ bảng |
| [DbExceptionTranslator.cs:32](../src/StudentManagement.Infrastructure/Persistence/DbExceptionTranslator.cs#L32) | Dịch lỗi DB sang lỗi nghiệp vụ |
| [UnitOfWork.cs:10](../src/StudentManagement.Infrastructure/Persistence/UnitOfWork.cs#L10) | Ranh giới transaction |

### O — Open/Closed

Mở rộng bằng cách thêm code mới, không sửa code đang chạy.

| Ở đâu | Thể hiện |
|-------|----------|
| [IRepository.cs:6](../src/StudentManagement.Application/Abstractions/IRepository.cs#L6) | Đổi kho dữ liệu (in-memory → NHibernate) = thêm một implementation, service không đổi một dòng |
| [PagedQuery.cs:10](../src/StudentManagement.Application/Common/PagedQuery.cs#L10) | Thêm loại danh sách mới = thêm một `record : PagedQuery`, quy tắc phân trang có sẵn |
| [ValidationRules.cs:10](../src/StudentManagement.Application/Common/ValidationRules.cs#L10) | Validator mới ghép từ các rule có sẵn |
| [AppExceptions.cs:7](../src/StudentManagement.Application/Common/Exceptions/AppExceptions.cs#L7) | Loại lỗi mới = thêm lớp con `AppException`; [ExceptionInterceptor](../src/StudentManagement.GrpcServer/Interceptors/ExceptionInterceptor.cs#L39) map theo kiểu |
| [ExceptionInterceptor.cs:39](../src/StudentManagement.GrpcServer/Interceptors/ExceptionInterceptor.cs#L39) | Bảng `Exception → StatusCode`: thêm loại lỗi = thêm một dòng, không service nào phải sửa |
| [StudentRepository.cs:13](../src/StudentManagement.Infrastructure/Persistence/Repositories/StudentRepository.cs#L13) | Sort: bảng `SortField → Expression`; thêm kiểu sắp xếp = thêm một dòng |
| [SnakeCaseNamingConvention.cs:14](../src/StudentManagement.Infrastructure/Persistence/SnakeCaseNamingConvention.cs#L14) | Tên bảng/cột suy ra bằng convention; thêm property không phải khai báo tên cột |
| M4 nói chung | Thêm cả tầng NHibernate mà **không sửa dòng nào** trong Application ([EndToEndTests.cs](../tests/StudentManagement.Infrastructure.Tests/EndToEndTests.cs) chạy chính các service của M3 trên PostgreSQL) |

### L — Liskov Substitution

Lớp con / implementation thay được cho kiểu cha mà không làm sai hành vi.

| Ở đâu | Thể hiện |
|-------|----------|
| [RepositoryContractTests.cs](../tests/StudentManagement.Application.Tests/Repositories/RepositoryContractTests.cs) | Một bộ test hành vi chung cho **mọi** `IRepository<T>`: [InMemoryRepositoryContractTests.cs](../tests/StudentManagement.Application.Tests/Repositories/InMemoryRepositoryContractTests.cs) chạy nó trên bản in-memory, [NhRepositoryContractTests.cs](../tests/StudentManagement.Infrastructure.Tests/NhRepositoryContractTests.cs) chạy **cùng bộ test** trên [Repository<T>](../src/StudentManagement.Infrastructure/Persistence/Repositories/Repository.cs#L11) (NHibernate) |
| [Person.cs:41](../src/StudentManagement.Domain/Common/Person.cs#L41) | `Student`/`Teacher` dùng được ở mọi chỗ cần `Person`; lớp con chỉ đổi tham số (`MinimumAge`), không đổi luật |

### I — Interface Segregation

Không ép ai phụ thuộc vào method mình không dùng.

| Ở đâu | Thể hiện |
|-------|----------|
| [IReadRepository.cs:9](../src/StudentManagement.Application/Abstractions/IReadRepository.cs#L9) tách khỏi [IRepository.cs:6](../src/StudentManagement.Application/Abstractions/IRepository.cs#L6) | `StudentAppService` chỉ cần tra lớp nên nhận [`IReadRepository<Classroom>`](../src/StudentManagement.Application/Students/StudentAppService.cs#L13), không thể sửa/xóa lớp; tương tự [`ClassroomAppService`](../src/StudentManagement.Application/Classrooms/ClassroomAppService.cs#L13) với `IReadRepository<Teacher>` |
| [IUnitOfWork.cs:7](../src/StudentManagement.Application/Abstractions/IUnitOfWork.cs#L7) | Chỉ một method `CommitAsync`; service không thấy `ISession`/`ITransaction` |
| `I*Repository` | Mỗi repository chuyên biệt chỉ thêm đúng query use case của nó cần (`GetByCodeAsync` chỉ có ở sinh viên) |

### D — Dependency Inversion

Tầng cao (Application) định nghĩa interface; tầng thấp (Infrastructure) implement. Mũi tên phụ thuộc chỉ vào trong.

| Ở đâu | Thể hiện |
|-------|----------|
| [IStudentRepository.cs:7](../src/StudentManagement.Application/Students/IStudentRepository.cs#L7), [IUnitOfWork.cs:7](../src/StudentManagement.Application/Abstractions/IUnitOfWork.cs#L7) | Interface nằm ở Application, không phải ở Infrastructure |
| [StudentAppService.cs:11-15](../src/StudentManagement.Application/Students/StudentAppService.cs#L11-L15) | Constructor nhận toàn interface; không `new` phụ thuộc nào |
| [StudentManagement.Application.csproj](../src/StudentManagement.Application/StudentManagement.Application.csproj) | Chỉ reference Domain + FluentValidation, không có NHibernate |
| [StudentRepository.cs:10](../src/StudentManagement.Infrastructure/Persistence/Repositories/StudentRepository.cs#L10), [DependencyInjection.cs:41](../src/StudentManagement.Infrastructure/DependencyInjection.cs#L41) | Infrastructure implement interface của Application và tự đăng ký qua `AddInfrastructure()`; mọi kiểu NHibernate đều `internal` |
| [Program.cs:11-12](../src/StudentManagement.GrpcServer/Program.cs#L11-L12) | Composition root: nơi duy nhất biết mọi tầng, ghép `AddApplication()` + `AddInfrastructure()` |

### Dependency Injection

| Ở đâu | Thể hiện |
|-------|----------|
| [DependencyInjection.cs:15-20](../src/StudentManagement.Application/DependencyInjection.cs#L15-L20) | `AddApplication()`: app service **Scoped** (dùng chung unit of work của request), validator **Singleton** (không giữ state) |
| [DependencyInjectionTests.cs](../tests/StudentManagement.Application.Tests/DependencyInjectionTests.cs) | Build container với `ValidateOnBuild` + `ValidateScopes`: resolve được mọi service, không có singleton giữ phụ thuộc scoped |

### Design patterns

| Pattern | Ở đâu |
|---------|-------|
| Repository / Generic Repository | [IRepository.cs](../src/StudentManagement.Application/Abstractions/IRepository.cs), [IStudentRepository.cs](../src/StudentManagement.Application/Students/IStudentRepository.cs) |
| Unit of Work | [IUnitOfWork.cs](../src/StudentManagement.Application/Abstractions/IUnitOfWork.cs) |
| Query Object | [StudentQuery.cs](../src/StudentManagement.Application/Students/StudentQuery.cs) kế thừa [PagedQuery.cs](../src/StudentManagement.Application/Common/PagedQuery.cs) |
| DTO + Mapper | [StudentDtos.cs](../src/StudentManagement.Application/Students/StudentDtos.cs), [DtoMappings.cs](../src/StudentManagement.Application/DtoMappings.cs) |
| Template Method | [Person.cs:41-51](../src/StudentManagement.Domain/Common/Person.cs#L41-L51) gọi hook `MinimumAge` |
| Test Double (Fake) | [InMemoryRepository.cs](../tests/StudentManagement.Application.Tests/Fakes/InMemoryRepository.cs), [FakeRepositories.cs](../tests/StudentManagement.Application.Tests/Fakes/FakeRepositories.cs) |
| Interceptor (Chain of Responsibility) | [LoggingInterceptor.cs](../src/StudentManagement.GrpcServer/Interceptors/LoggingInterceptor.cs) → [ExceptionInterceptor.cs](../src/StudentManagement.GrpcServer/Interceptors/ExceptionInterceptor.cs) → service |
