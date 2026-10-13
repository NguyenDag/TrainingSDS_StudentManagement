# Build context: repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /repo

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/StudentManagement.Web/StudentManagement.Web.csproj src/StudentManagement.Web/
RUN dotnet restore src/StudentManagement.Web/StudentManagement.Web.csproj

COPY protos/ protos/
COPY src/StudentManagement.Web/ src/StudentManagement.Web/
RUN dotnet publish src/StudentManagement.Web/StudentManagement.Web.csproj -c Release -o /publish --no-restore

FROM nginx:1.27-alpine
COPY src/StudentManagement.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /publish/wwwroot /usr/share/nginx/html
