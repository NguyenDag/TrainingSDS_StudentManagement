namespace StudentManagement.Web.Resources;

/// <summary>Every Vietnamese label of the web UI. Messages about data come from the server.</summary>
public static class UiText
{
    public const string AppName = "Quản lý sinh viên";

    // Menu / pages
    public const string Students = "Sinh viên";
    public const string Classrooms = "Lớp học";
    public const string Teachers = "Giáo viên";
    public const string NotFoundTitle = "Không tìm thấy trang";
    public const string NotFoundSubtitle = "Địa chỉ bạn mở không tồn tại.";
    public const string BackHome = "Về trang chủ";

    // Fields
    public const string Code = "Mã số";
    public const string StudentCode = "MSSV";
    public const string FullName = "Họ tên";
    public const string DateOfBirth = "Ngày sinh";
    public const string Age = "Tuổi";
    public const string Address = "Địa chỉ";
    public const string Classroom = "Lớp học";
    public const string ClassroomName = "Tên lớp";
    public const string Subject = "Môn học";
    public const string Teacher = "Giáo viên";
    public const string StudentCount = "Sĩ số";
    public const string ClassroomCount = "Số lớp";
    public const string Actions = "Thao tác";

    // Actions
    public const string Add = "Thêm mới";
    public const string Edit = "Sửa";
    public const string Delete = "Xóa";
    public const string Save = "Lưu";
    public const string Cancel = "Hủy";
    public const string Yes = "Đồng ý";
    public const string No = "Không";
    public const string Reload = "Tải lại";

    // Titles
    public const string AddStudent = "Thêm sinh viên";
    public const string EditStudent = "Sửa sinh viên";
    public const string AddClassroom = "Thêm lớp học";
    public const string EditClassroom = "Sửa lớp học";
    public const string AddTeacher = "Thêm giáo viên";
    public const string EditTeacher = "Sửa giáo viên";

    // Search / filters
    public const string SearchStudentCode = "Tìm MSSV (Enter: tìm chính xác)";
    public const string SearchClassroom = "Tìm theo mã hoặc tên lớp";
    public const string SearchTeacher = "Tìm theo mã hoặc họ tên";
    public const string FilterByClassroom = "Lọc theo lớp";
    public const string FilterByTeacher = "Lọc theo giáo viên";
    public const string SelectClassroom = "Chọn lớp (gõ để tìm)";
    public const string SelectTeacher = "Chọn giáo viên (gõ để tìm)";
    public const string SelectDate = "dd/MM/yyyy";

    // Feedback
    public const string Saved = "Đã lưu.";
    public const string Deleted = "Đã xóa.";
    public const string ConfirmDelete = "Xóa bản ghi này?";
    public const string StudentCodeNotFound = "Không có sinh viên mang mã này.";
    public const string ServerUnavailable = "Không kết nối được máy chủ. Hãy kiểm tra server và thử lại.";
    public const string UnexpectedError = "Đã xảy ra lỗi không mong muốn.";
    public const string StaleTitle = "Dữ liệu đã thay đổi";
    public const string StaleReloadQuestion = "Bản ghi đã bị người khác sửa. Tải lại dữ liệu mới nhất vào form?";

    // Client-side validation (the server re-checks everything)
    public const string Required = "Vui lòng nhập {0}.";
    public const string RequiredSelect = "Vui lòng chọn {0}.";
    public const string MaxLength = "{0} tối đa {1} ký tự.";
    public const string CodeFormat = "{0} gồm 3–20 chữ cái không dấu hoặc chữ số.";
    public const string DateFormat = "dd/MM/yyyy";
}
