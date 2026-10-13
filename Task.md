# Tài liệu đào tạo

Lộ trình gồm 6 phần. Mỗi phần có **lý thuyết** (đọc tài liệu tham khảo) và **bài thực hành**. Các bài thực hành nối tiếp nhau: bài sau nâng cấp từ chương trình của bài trước, cuối cùng thành một website quản lý sinh viên hoàn chỉnh.

| # | Chủ đề | Bài thực hành | Kế thừa từ |
|---|--------|---------------|-----------|
| 1 | Lập trình hướng đối tượng (OOP) | Quản lý sinh viên (console) | — |
| 2 | Cơ sở dữ liệu quan hệ | Thiết kế CSDL quản lý sinh viên | Bài 1.c |
| 3 | SOLID, Dependency Injection, Design Pattern | Áp dụng SOLID, DI, Repository pattern | Bài 2.b |
| 4 | NHibernate framework | Kết nối CSDL bằng NHibernate | Bài 3.d |
| 5 | gRPC for .NET | Truy xuất CSDL qua gRPC | Bài 4.b |
| 6 | Fullstack .NET Core | Website Blazor + gRPC + NHibernate, có biểu đồ báo cáo | Bài 5.b |

---

## 1. Lập trình hướng đối tượng (OOP)

### Lý thuyết

- **a. Khái niệm lập trình hướng đối tượng**
- **b. Nguyên tắc lập trình hướng đối tượng**

### c. Bài thực hành: Quản lý sinh viên (console)

Viết chương trình quản lý sinh viên chạy trên console.

**Chức năng**

- Menu chức năng
- Xem danh sách sinh viên
- Thêm mới sinh viên
- Chỉnh sửa thông tin sinh viên
- Xóa sinh viên
- Sắp xếp sinh viên theo tên
- Tìm kiếm sinh viên theo mã số sinh viên

**Thiết kế class**

| Class | Thuộc tính |
|-------|-----------|
| `SinhVien` (Sinh viên) | Mã số, Tên, Ngày sinh, Địa chỉ, ***Lớp học*** |
| `LopHoc` (Lớp học) | Mã số, Tên lớp, Môn học, ***Giáo viên*** |
| `GiaoVien` (Giáo viên) | Mã số, Tên, Ngày sinh |

> **Lưu ý:** Trường in nghiêng đậm là **đối tượng** (tham chiếu tới class khác). Các trường còn lại tự chọn kiểu dữ liệu phù hợp.

---

## 2. Cơ sở dữ liệu quan hệ

### Lý thuyết

- **a. Khái niệm cơ bản về hệ quản trị cơ sở dữ liệu**

### b. Bài thực hành: Thiết kế CSDL quản lý sinh viên

Thiết kế cơ sở dữ liệu lưu trữ được toàn bộ dữ liệu của bài thực hành **1.c**.

---

## 3. Nguyên lý SOLID, Dependency Injection và Design Pattern

### Lý thuyết

| Mục | Chủ đề | Tham khảo |
|-----|--------|-----------|
| a | Nguyên lý SOLID | [toidicodedao.com – SOLID là gì](https://toidicodedao.com/2015/03/24/solid-la-gi-ap-dung-cac-nguyen-ly-solid-de-tro-thanh-lap-trinh-vien-code-cung/) |
| b | Dependency Injection | [codelearn.io – Hiểu rõ về Dependency Injection](https://codelearn.io/sharing/hieu-ro-ve-dependency-injection) |
| c | Design Pattern | [refactoring.guru – Design Patterns in C#](https://refactoring.guru/design-patterns/csharp), [tedu.com.vn – Repository pattern](https://tedu.com.vn/lap-trinh-aspnet/tim-hieu-ve-repository-pattern-va-generic-repository-pattern-36.html) |

### d. Bài thực hành: Áp dụng SOLID, DI, Repository pattern

Áp dụng các nguyên lý và kỹ thuật trên để nâng cấp chương trình quản lý sinh viên console ở mục **2.b**.

---

## 4. NHibernate framework

### Lý thuyết

- **a. Tổng quan về NHibernate framework**

Tham khảo: [NHibernate Documentation](https://nhibernate.info/doc/index.html)

### b. Bài thực hành: Kết nối CSDL bằng NHibernate

Nâng cấp chương trình ở mục **3.d** để kết nối và thao tác với CSDL thông qua NHibernate.

---

## 5. gRPC for .NET

### Lý thuyết

- **a. Tìm hiểu framework gRPC**

Tham khảo: [gRPC trên .NET – Microsoft Docs](https://docs.microsoft.com/vi-vn/aspnet/core/grpc/?view=aspnetcore-6.0)

### b. Bài thực hành: Quản lý sinh viên dùng gRPC

Nâng cấp chương trình ở mục **4.b**, sử dụng gRPC để truy xuất CSDL sinh viên.

---

## 6. Fullstack .NET Core

### Lý thuyết

| Mục | Chủ đề | Tham khảo |
|-----|--------|-----------|
| a | Công nghệ ASP.NET Core | [Giới thiệu ASP.NET Core](https://docs.microsoft.com/vi-vn/aspnet/core/introduction-to-aspnet-core?view=aspnetcore-6.0) |
| b | Mô hình MVC, API | [Overview of ASP.NET Core MVC](https://docs.microsoft.com/vi-vn/aspnet/core/mvc/overview?view=aspnetcore-6.0) |
| c | Blazor cho web SPA | [ASP.NET Core Blazor](https://docs.microsoft.com/vi-vn/aspnet/core/blazor/?view=aspnetcore-6.0) |

### d. Bài thực hành: Website quản lý sinh viên

Viết website quản lý sinh viên sử dụng **Blazor + gRPC + NHibernate**, kế thừa chương trình ở mục **5.b**.

### e. Bổ sung biểu đồ báo cáo

Thêm biểu đồ báo cáo (loại biểu đồ tùy chọn) sử dụng [Ant Design Blazor](https://antblazor.com/en-US/).