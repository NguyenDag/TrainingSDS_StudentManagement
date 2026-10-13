using FluentValidation;
using FluentValidation.Results;
using Google.Rpc;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using StudentManagement.Domain.Exceptions;
using StudentManagement.GrpcServer.Interceptors;

namespace StudentManagement.GrpcServer.Tests;

/// <summary>Rows of §4.6 that cannot be triggered through the public API.</summary>
public class ExceptionInterceptorTests
{
    private readonly ExceptionInterceptor _interceptor = new(NullLogger<ExceptionInterceptor>.Instance);

    [Fact]
    public void Domain_rule_violation_is_InvalidArgument_without_leaking_its_english_text()
    {
        var exception = _interceptor.ToRpcException(new DomainException("Code must be 3-20 letters"), "trace-1");

        exception.StatusCode.ShouldBe(StatusCode.InvalidArgument);
        exception.Status.Detail.ShouldBe("Dữ liệu không hợp lệ.");
        exception.GetRpcStatus()!.GetDetail<ErrorInfo>()!.Reason.ShouldBe("VALIDATION_FAILED");
    }

    [Fact]
    public void Unexpected_error_is_Internal_with_a_generic_message_and_trace_id()
    {
        var exception = _interceptor.ToRpcException(new InvalidOperationException("connection string: secret"), "trace-42");

        exception.StatusCode.ShouldBe(StatusCode.Internal);
        exception.Status.Detail.ShouldBe("Lỗi hệ thống, vui lòng thử lại sau.");
        var info = exception.GetRpcStatus()!.GetDetail<ErrorInfo>()!;
        info.Reason.ShouldBe("INTERNAL_ERROR");
        info.Metadata["traceId"].ShouldBe("trace-42");
    }

    [Fact]
    public void Validation_property_names_become_proto_field_names()
    {
        var exception = _interceptor.ToRpcException(
            new ValidationException([new ValidationFailure("DateOfBirth", "x"), new ValidationFailure("Code", "y")]), "t");

        exception.GetRpcStatus()!.GetDetail<BadRequest>()!.FieldViolations.Select(v => v.Field)
            .ShouldBe(new[] { "date_of_birth", "code" });
    }
}
