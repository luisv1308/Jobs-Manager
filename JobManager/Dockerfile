FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY JobManager/JobManager.csproj JobManager/
RUN dotnet restore JobManager/JobManager.csproj

COPY . .
WORKDIR /src/JobManager
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5049
ENTRYPOINT ["dotnet", "JobManager.dll"]
