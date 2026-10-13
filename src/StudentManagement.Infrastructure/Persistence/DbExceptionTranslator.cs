using NHibernate;
using Npgsql;
using StudentManagement.Application.Common;
using StudentManagement.Application.Common.Exceptions;

namespace StudentManagement.Infrastructure.Persistence;

/// <summary>
/// Turns database errors the user can act on into Application exceptions, so callers never see
/// NHibernate or Npgsql types. The services check these rules first; this covers the race where
/// another request changes the data in between, with the database constraint as the final guard.
/// </summary>
internal static class DbExceptionTranslator
{
    public static async Task RunAsync(Func<Task> action, bool deleting = false)
    {
        try
        {
            await action();
        }
        catch (Exception exception) when (Translate(exception, deleting) is { } translated)
        {
            throw translated;
        }
    }

    /// <param name="deleting">
    /// PostgreSQL reports "row is still referenced" (delete) and "referenced row is missing" (insert/update)
    /// with the same SQLSTATE and constraint name, so the caller says which operation failed.
    /// </param>
    /// <returns>The business exception, or null when the error is not a known rule violation.</returns>
    public static AppException? Translate(Exception exception, bool deleting)
    {
        if (exception is StaleStateException)
            return new ConcurrencyException();

        return FindPostgresException(exception) switch
        {
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: var name } => name switch
            {
                "uq_students_code" => new ConflictException(ErrorCodes.StudentCodeDuplicated, Messages.StudentCodeDuplicated),
                "uq_classrooms_code" => new ConflictException(ErrorCodes.ClassroomCodeDuplicated, Messages.ClassroomCodeDuplicated),
                "uq_teachers_code" => new ConflictException(ErrorCodes.TeacherCodeDuplicated, Messages.TeacherCodeDuplicated),
                _ => null,
            },
            { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: var name } => (name, deleting) switch
            {
                ("fk_students_classrooms", true) => new InUseException(ErrorCodes.ClassroomHasStudents, Messages.ClassroomHasStudents),
                ("fk_classrooms_teachers", true) => new InUseException(ErrorCodes.TeacherHasClassrooms, Messages.TeacherHasClassrooms),
                ("fk_students_classrooms", false) => new NotFoundException(ErrorCodes.ClassroomNotFound, Messages.ClassroomNotFound),
                ("fk_classrooms_teachers", false) => new NotFoundException(ErrorCodes.TeacherNotFound, Messages.TeacherNotFound),
                _ => null,
            },
            _ => null,
        };
    }

    // NHibernate wraps ADO.NET errors (GenericADOException), possibly more than once.
    private static PostgresException? FindPostgresException(Exception? exception)
    {
        for (; exception is not null; exception = exception.InnerException)
            if (exception is PostgresException postgres) return postgres;
        return null;
    }
}
