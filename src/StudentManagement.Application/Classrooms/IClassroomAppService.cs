using StudentManagement.Application.Common;

namespace StudentManagement.Application.Classrooms;

/// <summary>Classroom use cases.</summary>
public interface IClassroomAppService
{
    Task<PagedResult<ClassroomDto>> ListAsync(ClassroomQuery query, CancellationToken cancellationToken = default);

    /// <exception cref="Common.Exceptions.NotFoundException"/>
    Task<ClassroomDto> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <exception cref="FluentValidation.ValidationException"/>
    /// <exception cref="Common.Exceptions.NotFoundException">The teacher does not exist.</exception>
    /// <exception cref="Common.Exceptions.ConflictException">The code is taken.</exception>
    Task<ClassroomDto> CreateAsync(ClassroomInput input, CancellationToken cancellationToken = default);

    /// <param name="version">Version the caller read; a mismatch means someone else saved in between.</param>
    /// <exception cref="FluentValidation.ValidationException"/>
    /// <exception cref="Common.Exceptions.NotFoundException">The classroom or the teacher does not exist.</exception>
    /// <exception cref="Common.Exceptions.ConcurrencyException"/>
    /// <exception cref="Common.Exceptions.ConflictException">The code is taken by another classroom.</exception>
    Task<ClassroomDto> UpdateAsync(int id, int version, ClassroomInput input, CancellationToken cancellationToken = default);

    /// <exception cref="Common.Exceptions.NotFoundException"/>
    /// <exception cref="Common.Exceptions.InUseException">The classroom still has students.</exception>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
