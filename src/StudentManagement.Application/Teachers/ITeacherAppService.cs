using StudentManagement.Application.Common;

namespace StudentManagement.Application.Teachers;

/// <summary>Teacher use cases.</summary>
public interface ITeacherAppService
{
    Task<PagedResult<TeacherDto>> ListAsync(TeacherQuery query, CancellationToken cancellationToken = default);

    /// <exception cref="Common.Exceptions.NotFoundException"/>
    Task<TeacherDto> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <exception cref="FluentValidation.ValidationException"/>
    /// <exception cref="Common.Exceptions.ConflictException">The code is taken.</exception>
    Task<TeacherDto> CreateAsync(TeacherInput input, CancellationToken cancellationToken = default);

    /// <param name="version">Version the caller read; a mismatch means someone else saved in between.</param>
    /// <exception cref="FluentValidation.ValidationException"/>
    /// <exception cref="Common.Exceptions.NotFoundException"/>
    /// <exception cref="Common.Exceptions.ConcurrencyException"/>
    /// <exception cref="Common.Exceptions.ConflictException">The code is taken by another teacher.</exception>
    Task<TeacherDto> UpdateAsync(int id, int version, TeacherInput input, CancellationToken cancellationToken = default);

    /// <exception cref="Common.Exceptions.NotFoundException"/>
    /// <exception cref="Common.Exceptions.InUseException">The teacher still has classrooms.</exception>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
