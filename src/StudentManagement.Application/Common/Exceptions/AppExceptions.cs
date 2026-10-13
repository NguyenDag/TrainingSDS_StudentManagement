namespace StudentManagement.Application.Common.Exceptions;

/// <summary>
/// A business error the caller can act on. <see cref="ErrorCode"/> is a stable English key for
/// clients and logs; <see cref="Exception.Message"/> is meant for the end user.
/// </summary>
public abstract class AppException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}

public sealed class NotFoundException(string errorCode, string message) : AppException(errorCode, message);

/// <summary>Another record already uses the same unique value (e.g. code).</summary>
public sealed class ConflictException(string errorCode, string message) : AppException(errorCode, message);

/// <summary>The record is still referenced by others and cannot be deleted.</summary>
public sealed class InUseException(string errorCode, string message) : AppException(errorCode, message);

/// <summary>The record was changed by someone else after the caller read it.</summary>
public sealed class ConcurrencyException()
    : AppException(ErrorCodes.ConcurrencyConflict, Messages.ConcurrencyConflict);

public static class ErrorCodes
{
    public const string StudentNotFound = "STUDENT_NOT_FOUND";
    public const string ClassroomNotFound = "CLASSROOM_NOT_FOUND";
    public const string TeacherNotFound = "TEACHER_NOT_FOUND";
    public const string StudentCodeDuplicated = "STUDENT_CODE_DUPLICATED";
    public const string ClassroomCodeDuplicated = "CLASSROOM_CODE_DUPLICATED";
    public const string TeacherCodeDuplicated = "TEACHER_CODE_DUPLICATED";
    public const string ClassroomHasStudents = "CLASSROOM_HAS_STUDENTS";
    public const string TeacherHasClassrooms = "TEACHER_HAS_CLASSROOMS";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
}
