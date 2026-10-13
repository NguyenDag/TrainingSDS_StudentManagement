# Build context: repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /repo

# Project files first: the restore layer stays cached until a dependency changes.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/StudentManagement.Domain/StudentManagement.Domain.csproj src/StudentManagement.Domain/
COPY src/StudentManagement.Application/StudentManagement.Application.csproj src/StudentManagement.Application/
COPY src/StudentManagement.Infrastructure/StudentManagement.Infrastructure.csproj src/StudentManagement.Infrastructure/
COPY src/StudentManagement.GrpcServer/StudentManagement.GrpcServer.csproj src/StudentManagement.GrpcServer/
RUN dotnet restore src/StudentManagement.GrpcServer/StudentManagement.GrpcServer.csproj

COPY protos/ protos/
COPY src/ src/
RUN dotnet publish src/StudentManagement.GrpcServer/StudentManagement.GrpcServer.csproj \
    -c Release -o /app --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app .
# Endpoints come from appsettings.json (Kestrel:Endpoints); drop the image default so it does not compete.
ENV ASPNETCORE_HTTP_PORTS=
# Built-in non-root user of the .NET images; 8080/8081 need no root.
USER $APP_UID
EXPOSE 8080 8081
# busybox wget ships with alpine, so no curl install is needed.
HEALTHCHECK --interval=10s --timeout=3s --start-period=20s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/healthz || exit 1
ENTRYPOINT ["dotnet", "StudentManagement.GrpcServer.dll"]
