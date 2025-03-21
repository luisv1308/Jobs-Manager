# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY JobManager/JobManager.csproj JobManager/
COPY JobManagerTest/JobManagerTest.csproj JobManagerTest/

# Restore dependencies
RUN dotnet restore JobManagerTest/JobManagerTest.csproj

# Copy everything else
COPY . .

# Build the test project
RUN dotnet build JobManagerTest/JobManagerTest.csproj -c Release --no-restore

# Final stage - test runner
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS testrunner
WORKDIR /app

# Copy the built output from build stage
COPY --from=build /src .

# Run the tests (project path is relative to /app)
ENTRYPOINT ["dotnet", "test", "JobManagerTest/JobManagerTest.csproj", "--verbosity", "minimal"]
