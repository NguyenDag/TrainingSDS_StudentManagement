using FluentValidation;
using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Common;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Students;

namespace StudentManagement.Application.Students;

public sealed class StudentAppService(
    IStudentRepository students,
    IReadRepository<Classroom> classrooms,
    IUnitOfWork unitOfWork,
    IValidator<StudentInput> validator) : IStudentAppService
{
    public async Task<PagedResult<StudentDto>> ListAsync(StudentQuery query, CancellationToken cancellationToken = default)
    {
        // Codes are stored upper-case, so the prefix must be too.
        var normalized = query.CodePrefix is null ? query : query with { CodePrefix = EntityCode.Normalize(query.CodePrefix) };
        var page = await students.SearchAsync(normalized, cancellationToken);
        return page.Map(s => s.ToDto());
    }

    public async Task<StudentDto> GetAsync(int id, CancellationToken cancellationToken = default) =>
        (await FindAsync(id, cancellationToken)).ToDto();

    public async Task<StudentDto> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var student = await students.GetByCodeAsync(EntityCode.Normalize(code), cancellationToken)
                      ?? throw NotFound();
        return student.ToDto();
    }

    public async Task<StudentDto> CreateAsync(StudentInput input, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);
        await EnsureCodeIsFreeAsync(input.Code, excludeId: null, cancellationToken);
        var classroom = await FindClassroomAsync(input.ClassroomId, cancellationToken);

        var student = new Student(input.Code, input.FullName, input.DateOfBirth, input.Address, classroom);
        await students.AddAsync(student, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return student.ToDto();
    }

    public async Task<StudentDto> UpdateAsync(int id, int version, StudentInput input, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);
        var student = await FindAsync(id, cancellationToken);
        if (student.Version != version) throw new ConcurrencyException();
        await EnsureCodeIsFreeAsync(input.Code, id, cancellationToken);
        var classroom = await FindClassroomAsync(input.ClassroomId, cancellationToken);

        student.ChangeCode(input.Code);
        student.Rename(input.FullName);
        student.ChangeDateOfBirth(input.DateOfBirth);
        student.ChangeAddress(input.Address);
        student.MoveTo(classroom);
        await students.UpdateAsync(student, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return student.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await FindAsync(id, cancellationToken);
        await students.DeleteAsync(student, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task<Student> FindAsync(int id, CancellationToken cancellationToken) =>
        await students.GetAsync(id, cancellationToken) ?? throw NotFound();

    private async Task<Classroom> FindClassroomAsync(int id, CancellationToken cancellationToken) =>
        await classrooms.GetAsync(id, cancellationToken)
        ?? throw new NotFoundException(ErrorCodes.ClassroomNotFound, Messages.ClassroomNotFound);

    private async Task EnsureCodeIsFreeAsync(string code, int? excludeId, CancellationToken cancellationToken)
    {
        if (await students.ExistsByCodeAsync(EntityCode.Normalize(code), excludeId, cancellationToken))
            throw new ConflictException(ErrorCodes.StudentCodeDuplicated, Messages.StudentCodeDuplicated);
    }

    private static NotFoundException NotFound() => new(ErrorCodes.StudentNotFound, Messages.StudentNotFound);
}
