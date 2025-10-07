# Use the official .NET 9.0 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY PrisonersDilemma.sln ./
COPY PrisonersDilemma.Api/PrisonersDilemma.Api.csproj ./PrisonersDilemma.Api/
COPY PrisonersDilemma.Api.Tests/PrisonersDilemma.Api.Tests.csproj ./PrisonersDilemma.Api.Tests/
COPY Directory.Build.props ./

# Restore dependencies
RUN dotnet restore

# Copy the entire source code
COPY . .

# Build and publish the application
WORKDIR /src/PrisonersDilemma.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Use the official .NET 9.0 runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install SQLite (required for the database)
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=build /app/publish .

# Create directory for the database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/PrisonersDilemma.db"

# Expose port
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "PrisonersDilemma.Api.dll"]