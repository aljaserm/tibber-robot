# Use the official .NET image as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore as distinct layers
COPY ["RobotCleanerService/Application/Application.csproj", "RobotCleanerService/Application/"]
COPY ["RobotCleanerService/Infrastructure/Infrastructure.csproj", "RobotCleanerService/Infrastructure/"]
COPY ["RobotCleanerService/web/web.csproj", "RobotCleanerService/web/"]

RUN dotnet restore "RobotCleanerService/web/web.csproj"

# Copy the rest of the application code
COPY . .

# Build the application
WORKDIR "/src/RobotCleanerService/web"
RUN dotnet build "web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "web.csproj" -c Release -o /app/publish

# Create the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "web.dll"]
