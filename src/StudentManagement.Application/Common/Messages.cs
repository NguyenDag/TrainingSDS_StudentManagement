namespace StudentManagement.Application.Common;

/// <summary>Every user-facing (Vietnamese) message of the server side lives here.</summary>
public static class Messages
{
    // Field display names, substituted for {PropertyName}.
    public const string Code = "Mã số";
    public const string FullName = "Họ tên";
    public const string DateOfBirth = "Ngày sinh";
    public const string Address = "Địa chỉ";
    public const string Classroom = "Lớp học";
    public const string ClassroomName = "Tên lớp";
    public const string Subject = "Môn học";
    public const string Teacher = "Giáo viên";

    // Validation
    public const string Required = "{PropertyName} không được để trống.";
    public const string MaxLength = "{PropertyName} tối đa {MaxLength} ký tự.";
    public const string CodeFormat = "{PropertyName} gồm 3–20 chữ cái không dấu hoặc chữ số.";
    public const string ReferenceRequired = "Vui lòng chọn {PropertyName}.";
    public const string DateOfBirthInFuture = "Ngày sinh không được ở tương lai.";
    public const string DateOfBirthTooEarly = "Ngày sinh không được trước năm 1900.";
    public const string StudentTooYoung = "Sinh viên phải từ {MinimumAge} tuổi trở lên.";
    public const string TeacherTooYoung = "Giáo viên phải từ {MinimumAge} tuổi trở lên.";
    public const string DateInvalid = "Ngày không hợp lệ.";

    // Generic outcomes
    public const string ValidationFailed = "Dữ liệu không hợp lệ.";
    public const string InternalError = "Lỗi hệ thống, vui lòng thử lại sau.";

    // Business errors
    public const string StudentNotFound = "Không tìm thấy sinh viên.";
    public const string ClassroomNotFound = "Không tìm thấy lớp học.";
    public const string TeacherNotFound = "Không tìm thấy giáo viên.";
    public const string StudentCodeDuplicated = "Mã sinh viên đã tồn tại.";
    public const string ClassroomCodeDuplicated = "Mã lớp học đã tồn tại.";
    public const string TeacherCodeDuplicated = "Mã giáo viên đã tồn tại.";
    public const string ClassroomHasStudents = "Lớp còn sinh viên, không thể xóa.";
    public const string TeacherHasClassrooms = "Giáo viên còn phụ trách lớp, không thể xóa.";
    public const string ConcurrencyConflict = "Dữ liệu đã bị người khác sửa, hãy tải lại.";
}
