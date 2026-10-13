using FluentValidation;
using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Common;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Classrooms;

public sealed class ClassroomAppService(
    IClassroomRepository classrooms,
    IReadRepository<Teacher> teachers,
    IUnitOfWork unitOfWork,
    IValidator<ClassroomInput> validator) : IClassroomAppService
{
    public async Task<PagedResult<ClassroomDto>> ListAsync(ClassroomQuery query, CancellationToken cancellationToken = default)
    {
        var page = await classrooms.SearchAsync(query, cancellationToken);
        var counts = await classrooms.CountStudentsAsync(page.Items.Select(c => c.Id).ToList(), cancellationToken);
        return page.Map(c => c.ToDto(counts.GetValueOrDefault(c.Id)));
    }

    public async Task<ClassroomDto> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var classroom = await FindAsync(id, cancellationToken);
        return classroom.ToDto(await CountStudentsAsync(id, cancellationToken));
    }

    public async Task<ClassroomDto> CreateAsync(ClassroomInput input, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);
        await EnsureCodeIsFreeAsync(input.Code, excludeId: null, cancellationToken);
        var teacher = await FindTeacherAsync(input.TeacherId, cancellationToken);

        var classroom = new Classroom(input.Code, input.Name, input.Subject, teacher);
        await classrooms.AddAsync(classroom, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return classroom.ToDto(studentCount: 0);
    }

    public async Task<ClassroomDto> UpdateAsync(int id, int version, ClassroomInput input, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);
        var classroom = await FindAsync(id, cancellationToken);
        if (classroom.Version != version) throw new ConcurrencyException();
        await EnsureCodeIsFreeAsync(input.Code, id, cancellationToken);
        var teacher = await FindTeacherAsync(input.TeacherId, cancellationToken);

        classroom.ChangeCode(input.Code);
        classroom.Rename(input.Name);
        classroom.ChangeSubject(input.Subject);
        classroom.AssignTeacher(teacher);
        var studentCount = await CountStudentsAsync(id, cancellationToken);
        await classrooms.UpdateAsync(classroom, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return classroom.ToDto(studentCount);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var classroom = await FindAsync(id, cancellationToken);
        if (await CountStudentsAsync(id, cancellationToken) > 0)
            throw new InUseException(ErrorCodes.ClassroomHasStudents, Messages.ClassroomHasStudents);

        await classrooms.DeleteAsync(classroom, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task<Classroom> FindAsync(int id, CancellationToken cancellationToken) =>
        await classrooms.GetAsync(id, cancellationToken)
        ?? throw new NotFoundException(ErrorCodes.ClassroomNotFound, Messages.ClassroomNotFound);

    private async Task<Teacher> FindTeacherAsync(int id, CancellationToken cancellationToken) =>
        await teachers.GetAsync(id, cancellationToken)
        ?? throw new NotFoundException(ErrorCodes.TeacherNotFound, Messages.TeacherNotFound);

    private async Task<int> CountStudentsAsync(int id, CancellationToken cancellationToken) =>
        (await classrooms.CountStudentsAsync([id], cancellationToken)).GetValueOrDefault(id);

    private async Task EnsureCodeIsFreeAsync(string code, int? excludeId, CancellationToken cancellationToken)
    {
        if (await classrooms.ExistsByCodeAsync(EntityCode.Normalize(code), excludeId, cancellationToken))
            throw new ConflictException(ErrorCodes.ClassroomCodeDuplicated, Messages.ClassroomCodeDuplicated);
    }
}
