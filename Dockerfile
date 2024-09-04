FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# restore solution packages
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /
COPY ["src/Netstr/Netstr.csproj", "src/Netstr/"]
COPY ["test/Netstr.Tests/Netstr.Tests.csproj", "test/Netstr.Tests/"]
COPY ["Netstr.sln", ""]
RUN dotnet restore

# build the main project
FROM restore AS build
COPY . .
WORKDIR "/src/Netstr"
RUN dotnet build -c Release -o /app/build

# run tests
FROM build AS test
WORKDIR "/test/Netstr.Tests"
RUN dotnet test -c Release -o /app/build

# publish
FROM test as publish
WORKDIR "/src/Netstr"
RUN dotnet publish "Netstr.csproj" -c Release -o /app/publish

# final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Netstr.dll"]