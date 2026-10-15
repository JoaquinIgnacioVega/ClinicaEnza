# Etapa 1: Build y Publish de la API .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["src/ClinicaEnza.Domain/ClinicaEnza.Domain.csproj", "ClinicaEnza.Domain/"]
COPY ["src/ClinicaEnza.Application/ClinicaEnza.Application.csproj", "ClinicaEnza.Application/"]
COPY ["src/ClinicaEnza.Infrastructure/ClinicaEnza.Infrastructure.csproj", "ClinicaEnza.Infrastructure/"]
COPY ["src/ClinicaEnza.Api/ClinicaEnza.Api.csproj", "ClinicaEnza.Api/"]
RUN dotnet restore "ClinicaEnza.Api/ClinicaEnza.Api.csproj"

# Copiar el código fuente completo y compilar
COPY src/ .
WORKDIR "/src/ClinicaEnza.Api"
RUN dotnet publish "ClinicaEnza.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime ligero con ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Directorio para la persistencia de base de datos
RUN mkdir -p /app/data

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "ClinicaEnza.Api.dll"]
