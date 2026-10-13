using AntDesign;
using Google.Rpc;
using Grpc.Core;
using StudentManagement.Web.Resources;

namespace StudentManagement.Web.Services;

/// <summary>What the server said went wrong, decoded from the gRPC status.</summary>
/// <param name="FieldErrors">Keyed by form model property (FullName), translated from proto field names (full_name).</param>
public sealed record ServerError(StatusCode Status, string Message, IReadOnlyDictionary<string, string[]> FieldErrors)
{
    public static ServerError From(RpcException exception)
    {
        var status = exception.GetRpcStatus();
        var fieldErrors = status?.GetDetail<BadRequest>()?.FieldViolations
            .GroupBy(v => ToPropertyName(v.Field))
            .ToDictionary(g => g.Key, g => g.Select(v => v.Description).ToArray())
            ?? [];

        var message = exception.StatusCode switch
        {
            // The browser could not reach the server at all; there is no server message.
            StatusCode.Unavailable => UiText.ServerUnavailable,
            _ when status is not null => exception.Status.Detail, // Vietnamese text set by the server
            _ => UiText.UnexpectedError,
        };
        return new ServerError(exception.StatusCode, message, fieldErrors);
    }

    // full_name → FullName
    private static string ToPropertyName(string protoField) =>
        string.Concat(protoField.Split('_').Select(part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part[1..]));
}

/// <summary>Runs server calls so a failure becomes a message on screen instead of an unhandled exception.</summary>
public sealed class GrpcErrorHandler(IMessageService messages)
{
    /// <param name="handle">Optional special handling; return true when it fully dealt with the error.</param>
    /// <returns>True when the call succeeded.</returns>
    public async Task<bool> RunAsync(Func<Task> call, Func<ServerError, Task<bool>>? handle = null)
    {
        try
        {
            await call();
            return true;
        }
        catch (RpcException exception)
        {
            var error = ServerError.From(exception);
            if (handle is null || !await handle(error))
                _ = messages.ErrorAsync(error.Message); // not awaited: it completes only when the toast closes
            return false;
        }
    }

    /// <summary>Shows each field error under its form field, then the general message.</summary>
    public static Func<ServerError, Task<bool>> ShowOnForm<TModel>(Form<TModel>? form) => error =>
    {
        foreach (var (field, fieldMessages) in error.FieldErrors)
            form?.SetValidationMessages(field, fieldMessages);
        return Task.FromResult(false);
    };
}
