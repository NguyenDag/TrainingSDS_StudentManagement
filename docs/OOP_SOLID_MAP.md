# Bản đồ OOP & SOLID

Chỉ ra file/dòng thể hiện từng nguyên tắc. Viết dần theo milestone: phần OOP từ M1 (Bài 1), phần SOLID bổ sung ở M3.

## OOP (Bài 1)

### Encapsulation (đóng gói)

State chỉ đổi được qua method có kiểm tra; object không bao giờ rơi vào trạng thái sai.

| Ở đâu | Thể hiện |
|-------|----------|
| [Person.cs:10-17](../src/StudentManagement.Domain/Common/Person.cs#L10-L17), [Student.cs:11-12](../src/StudentManagement.Domain/Students/Student.cs#L11-L12), [Classroom.cs:12-15](../src/StudentManagement.Domain/Classrooms/Classroom.cs#L12-L15), [Entity.cs:5-8](../src/StudentManagement.Domain/Common/Entity.cs#L5-L8) | Mọi property có setter `protected`; bên ngoài chỉ đọc |
| [Person.cs:36-51](../src/StudentManagement.Domain/Common/Person.cs#L36-L51), [Student.cs:27-31](../src/StudentManagement.Domain/Students/Student.cs#L27-L31), [Classroom.cs:28-37](../src/StudentManagement.Domain/Classrooms/Classroom.cs#L28-L37) | Đổi state qua method (`ChangeCode`, `Rename`, `MoveTo`, `AssignTeacher`...), method kiểm invariant rồi mới gán, sai thì ném `DomainException` |
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
| [Person.cs:20](../src/StudentManagement.Domain/Common/Person.cs#L20) → [Student.cs:14](../src/StudentManagement.Domain/Students/Student.cs#L14) (16), [Teacher.cs:7](../src/StudentManagement.Domain/Teachers/Teacher.cs#L7) (18) | `MinimumAge` abstract, mỗi lớp con override. [Person.cs:48](../src/StudentManagement.Domain/Common/Person.cs#L48) gọi qua kiểu `Person` mà không biết lớp con nào (Template Method) |
| [Person.cs:22](../src/StudentManagement.Domain/Common/Person.cs#L22) → [Student.cs:15](../src/StudentManagement.Domain/Students/Student.cs#L15), [Teacher.cs:8](../src/StudentManagement.Domain/Teachers/Teacher.cs#L8) | `RoleName` abstract, dùng trong thông báo lỗi và `ToString` |
| [Person.cs:53](../src/StudentManagement.Domain/Common/Person.cs#L53) → [Student.cs:33](../src/StudentManagement.Domain/Students/Student.cs#L33) | `ToString()` override nhiều tầng, `Student` mở rộng bản của `Person` bằng `base.ToString()` |
| [Entity.cs:12-26](../src/StudentManagement.Domain/Common/Entity.cs#L12-L26) | Override `Equals`/`GetHashCode` của `object`; `GetUnproxiedType()` virtual để proxy NHibernate (M4) trả đúng kiểu thật |
| Test: [PersonTests.cs:53](../tests/StudentManagement.Domain.Tests/PersonTests.cs#L53), [PersonTests.cs:104](../tests/StudentManagement.Domain.Tests/PersonTests.cs#L104) | Cùng ngày sinh 17 tuổi: `Student` hợp lệ, `Teacher` bị từ chối |

### Abstraction (trừu tượng)

| Ở đâu | Thể hiện |
|-------|----------|
| [Entity.cs:3](../src/StudentManagement.Domain/Common/Entity.cs#L3), [Person.cs:5](../src/StudentManagement.Domain/Common/Person.cs#L5) | `abstract class`: không tạo được "một Person chung chung", chỉ tạo `Student` hoặc `Teacher` |
| [Person.cs:20-22](../src/StudentManagement.Domain/Common/Person.cs#L20-L22) | Lớp cha khai báo *cái gì* (`MinimumAge`, `RoleName`), lớp con quyết định *bao nhiêu* |
| [DomainException.cs:4](../src/StudentManagement.Domain/Exceptions/DomainException.cs#L4) | Người gọi chỉ cần bắt một loại lỗi nghiệp vụ, không cần biết quy tắc nào bị vi phạm |
| *(M3)* | Interface `IRepository<T>`, `IUnitOfWork`, `I*AppService` |

## SOLID (Bài 3)

*Bổ sung ở M3.*
