using StudentManagement.Application.Common;

namespace StudentManagement.Application.Students;

/// <summary>Student use cases.</summary>
public interface IStudentAppService
{
    /// <summary>Lists, sorts and filters students (code prefix, classroom).</summary>
    Task<PagedResult<StudentDto>> ListAsync(StudentQuery query, CancellationToken cancellationToken = default);

    /// <exception cref="Common.Exceptions.NotFoundException"/>
    Task<StudentDto> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Exact lookup by student code, case-insensitive.</summary>
    /// <exception cref="Common.Exceptions.NotFoundException"/>
    Task<StudentDto> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <exception cref="FluentValidation.ValidationException"/>
    /// <exception cref="Common.Exceptions.NotFoundException">The classroom does not exist.</exception>
    /// <exception cref="Common.Exceptions.ConflictException">The code is taken.</exception>
    Task<StudentDto> CreateAsync(StudentInput input, CancellationToken cancellationToken = default);

    /// <param name="version">Version the caller read; a mismatch means someone else saved in between.</param>
    /// <exception cref="FluentValidation.ValidationException"/>
    /// <exception cref="Common.Exceptions.NotFoundException">The student or the classroom does not exist.</exception>
    /// <exception cref="Common.Exceptions.ConcurrencyException"/>
    /// <exception cref="Common.Exceptions.ConflictException">The code is taken by another student.</exception>
    Task<StudentDto> UpdateAsync(int id, int version, StudentInput input, CancellationToken cancellationToken = default);

    /// <exception cref="Common.Exceptions.NotFoundException"/>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
