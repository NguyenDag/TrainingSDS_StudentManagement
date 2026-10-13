using System.Text.RegularExpressions;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using Grpc.Core.Interceptors;
using StudentManagement.Application.Common;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Domain.Exceptions;
using Status = Google.Rpc.Status;

namespace StudentManagement.GrpcServer.Interceptors;

/// <summary>
/// Maps every exception to a gRPC status, so services stay free of try/catch.
/// Each status carries google.rpc.ErrorInfo (Reason = stable error code) and, for validation,
/// google.rpc.BadRequest with one violation per form field. Clients read both from the
/// grpc-status-details-bin trailer.
/// </summary>
public sealed partial class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger) : Interceptor
{
    private const string ErrorDomain = "studentmanagement.v1";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception exception) when (exception is not RpcException)
        {
            throw ToRpcException(exception, context.GetHttpContext().TraceIdentifier);
        }
    }

    internal RpcException ToRpcException(Exception exception, string traceId)
    {
        var (statusCode, errorCode, message) = exception switch
        {
            ValidationException => (StatusCode.InvalidArgument, ErrorCodes.ValidationFailed, Messages.ValidationFailed),
            // The validator should have caught it; its English text is not meant for users.
            DomainException => (StatusCode.InvalidArgument, ErrorCodes.ValidationFailed, Messages.ValidationFailed),
            NotFoundException e => (StatusCode.NotFound, e.ErrorCode, e.Message),
            ConflictException e => (StatusCode.AlreadyExists, e.ErrorCode, e.Message),
            InUseException e => (StatusCode.FailedPrecondition, e.ErrorCode, e.Message),
            ConcurrencyException e => (StatusCode.Aborted, e.ErrorCode, e.Message),
            OperationCanceledException => (StatusCode.Cancelled, "CANCELLED", "Request was cancelled."),
            _ => (StatusCode.Internal, ErrorCodes.InternalError, Messages.InternalError),
        };

        if (exception is DomainException)
            logger.LogWarning(exception, "Domain rule violated past validation (trace {TraceId})", traceId);
        else if (statusCode == StatusCode.Internal)
            logger.LogError(exception, "Unhandled exception (trace {TraceId})", traceId);

        var status = new Status
        {
            Code = (int)statusCode,
            Message = message,
            Details =
            {
                Any.Pack(new ErrorInfo
                {
                    Reason = errorCode,
                    Domain = ErrorDomain,
                    // Lets a user quote the id so the matching log entry can be found.
                    Metadata = { ["traceId"] = traceId },
                }),
            },
        };
        if (exception is ValidationException validation)
            status.Details.Add(Any.Pack(ToBadRequest(validation)));
        return status.ToRpcException();
    }

    private static BadRequest ToBadRequest(ValidationException exception) => new()
    {
        FieldViolations =
        {
            exception.Errors.Select(e => new BadRequest.Types.FieldViolation
            {
                // Input property names (FullName) become proto field names (full_name).
                Field = UpperCaseInsideWord().Replace(e.PropertyName, "_$1").ToLowerInvariant(),
                Description = e.ErrorMessage,
            }),
        },
    };

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex UpperCaseInsideWord();
}
