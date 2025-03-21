FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY JobClient/JobClient.csproj JobClient/
RUN dotnet restore JobClient/JobClient.csproj

COPY . .
WORKDIR /src/JobClient
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5049
ENTRYPOINT ["dotnet", "JobClient.dll"]
