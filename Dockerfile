# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WorkflowEngine.API/WorkflowEngine.API.csproj", "WorkflowEngine.API/"]
COPY ["WorkflowEngine.Core/WorkflowEngine.Core.csproj", "WorkflowEngine.Core/"]
COPY ["WorkflowEngine.Infrastructure/WorkflowEngine.Infrastructure.csproj", "WorkflowEngine.Infrastructure/"]
RUN dotnet restore "WorkflowEngine.API/WorkflowEngine.API.csproj"
COPY . .
WORKDIR "/src/WorkflowEngine.API"
RUN dotnet build "WorkflowEngine.API.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "WorkflowEngine.API.csproj" -c Release -o /app/publish

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkflowEngine.API.dll"]
