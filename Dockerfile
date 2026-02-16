FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Weda.Template.sln", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/Weda.Template.Api/Weda.Template.Api.csproj", "src/Weda.Template.Api/"]
COPY ["src/Weda.Template.Application/Weda.Template.Application.csproj", "src/Weda.Template.Application/"]
COPY ["src/Weda.Template.Domain/Weda.Template.Domain.csproj", "src/Weda.Template.Domain/"]
COPY ["src/Weda.Template.Contracts/Weda.Template.Contracts.csproj", "src/Weda.Template.Contracts/"]
COPY ["src/Weda.Template.Infrastructure/Weda.Template.Infrastructure.csproj", "src/Weda.Template.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Weda.Template.Api/Weda.Template.Api.csproj"

# Copy source code
COPY src/ src/

# Build
WORKDIR /src/src/Weda.Template.Api
RUN dotnet build "Weda.Template.Api.csproj" -c Release -o /app/build --no-restore

# Publish
FROM build AS publish
RUN dotnet publish "Weda.Template.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Weda.Template.Api.dll"]
