using FluentValidation;
using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Common;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Teachers;

public sealed class TeacherAppService(
    ITeacherRepository teachers,
    IUnitOfWork unitOfWork,
    IValidator<TeacherInput> validator) : ITeacherAppService
{
    public async Task<PagedResult<TeacherDto>> ListAsync(TeacherQuery query, CancellationToken cancellationToken = default)
    {
        var page = await teachers.SearchAsync(query, cancellationToken);
        var counts = await teachers.CountClassroomsAsync(page.Items.Select(t => t.Id).ToList(), cancellationToken);
        return page.Map(t => t.ToDto(counts.GetValueOrDefault(t.Id)));
    }

    public async Task<TeacherDto> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var teacher = await FindAsync(id, cancellationToken);
        return teacher.ToDto(await CountClassroomsAsync(id, cancellationToken));
    }

    public async Task<TeacherDto> CreateAsync(TeacherInput input, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);
        await EnsureCodeIsFreeAsync(input.Code, excludeId: null, cancellationToken);

        var teacher = new Teacher(input.Code, input.FullName, input.DateOfBirth);
        await teachers.AddAsync(teacher, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return teacher.ToDto(classroomCount: 0);
    }

    public async Task<TeacherDto> UpdateAsync(int id, int version, TeacherInput input, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);
        var teacher = await FindAsync(id, cancellationToken);
        if (teacher.Version != version) throw new ConcurrencyException();
        await EnsureCodeIsFreeAsync(input.Code, id, cancellationToken);

        teacher.ChangeCode(input.Code);
        teacher.Rename(input.FullName);
        teacher.ChangeDateOfBirth(input.DateOfBirth);
        var classroomCount = await CountClassroomsAsync(id, cancellationToken);
        await teachers.UpdateAsync(teacher, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return teacher.ToDto(classroomCount);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var teacher = await FindAsync(id, cancellationToken);
        if (await CountClassroomsAsync(id, cancellationToken) > 0)
            throw new InUseException(ErrorCodes.TeacherHasClassrooms, Messages.TeacherHasClassrooms);

        await teachers.DeleteAsync(teacher, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task<Teacher> FindAsync(int id, CancellationToken cancellationToken) =>
        await teachers.GetAsync(id, cancellationToken)
        ?? throw new NotFoundException(ErrorCodes.TeacherNotFound, Messages.TeacherNotFound);

    private async Task<int> CountClassroomsAsync(int id, CancellationToken cancellationToken) =>
        (await teachers.CountClassroomsAsync([id], cancellationToken)).GetValueOrDefault(id);

    private async Task EnsureCodeIsFreeAsync(string code, int? excludeId, CancellationToken cancellationToken)
    {
        if (await teachers.ExistsByCodeAsync(EntityCode.Normalize(code), excludeId, cancellationToken))
            throw new ConflictException(ErrorCodes.TeacherCodeDuplicated, Messages.TeacherCodeDuplicated);
    }
}
