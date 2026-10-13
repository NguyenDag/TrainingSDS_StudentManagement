using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace StudentManagement.GrpcServer.Interceptors;

/// <summary>One log line per call: method, final status and duration.</summary>
public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var started = Stopwatch.GetTimestamp();
        var status = StatusCode.OK;
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException exception)
        {
            status = exception.StatusCode;
            throw;
        }
        catch
        {
            status = StatusCode.Unknown;
            throw;
        }
        finally
        {
            logger.LogInformation("gRPC {Method} responded {StatusCode} in {ElapsedMs:0.0} ms",
                context.Method, status, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
    }
}
