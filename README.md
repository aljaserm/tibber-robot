# Robot Cleaner Service

## Table of Contents

- [Overview](#overview)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
  - [Example .env](#example-env)
  - [Example .env.docker](#example-envdocker)
- [Running the Application](#running-the-application)
  - [Running with Docker](#running-with-docker)
  - [Running with .NET CLI](#running-with-net-cli)
  - [Running with Visual Studio](#running-with-visual-studio)
- [Running Database Migrations](#running-database-migrations)
  - [Using .NET CLI](#using-net-cli)
  - [Using Visual Studio](#using-visual-studio)
- [Running Unit Tests](#running-unit-tests)
- [Optimizations](#optimizations)
- [Production Considerations](#production-considerations)
- [Conclusion](#conclusion)

## Overview

The Robot Cleaner Service is designed to fit into the Tibber Platform environment, simulating a robot moving in an office space and cleaning the places it visits. The service processes the robot's movement commands, tracks the number of unique places cleaned, stores the results in a PostgreSQL database, and returns the created record in JSON format.

## Project Structure

```
|-- RobotCleanerService
    |-- Application
        |-- Application.csproj
        |-- Commands
        |-- DTOs
        |-- DependencyInjection.cs
        |-- Enums
        |-- Exceptions
        |-- Mappings
        |-- Responses
        |-- Utilities
    |-- Infrastructure
        |-- Data
        |-- DependencyInjection.cs
        |-- Infrastructure.csproj
        |-- Migrations
        |-- Models
    |-- Test
        |-- Application
        |-- LargeJsonFiles
            |-- robotcleanerpathheavy.json
            |-- robotcleanerpathheavy_Double.json
            |-- robotcleanerpathheavy_Tripple.json
        |-- Test.csproj
    |-- TibberRobotService.sln
    |-- TibberRobotService.sln.DotSettings.user
    |-- deploy
        |-- Docker
    |-- web
        |-- Controllers
        |-- Program.cs
        |-- Properties
        |-- web.csproj
        |-- web.csproj.user
```

## Prerequisites

Ensure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop)
- [PostgreSQL](https://www.postgresql.org/download/)

## Configuration

Before running the application, you need to set up your environment variables. Create a `.env` file in the `web` directory based on the structure provided in `.env.example` and `.env.docker.example`.

Place the `.env` files in the appropriate directories as follows:
```
|-- RobotCleanerService
    |-- deploy
        |-- .env.docker
        |-- .env.docker.example
    |-- web
        |-- .env
        |-- .env.example
```
### Example .env

```plaintext
PG_DB_USERNAME=your_username
PG_DB_PASSWORD=your_password
PG_DB_NAME=tibber
PG_DB_HOST=localhost
```

### Example .env.docker

```plaintext
PG_DB_HOST=tibber-db
PG_DB_USERNAME=your_username
PG_DB_PASSWORD=your_password
PG_DB_NAME=tibber
ASPNETCORE_ENVIRONMENT=Docker
```

## Running the Application

### Running with Docker

1. Ensure Docker and Docker Compose are installed on your machine.
2. Navigate to the `deploy` directory.

```sh
cd \path-to-your-project\RobotCleanerService\deploy
```

3. Build and start the services:

```sh
docker-compose up --build
```

4. Access the service at `http://localhost:5000`.

### Running with .NET CLI

1. Ensure .NET 8 SDK is installed on your machine.
2. Navigate to the root directory of the project.

```sh
cd \path-to-your-project\RobotCleanerService
```

3. Apply the database migrations:

```sh
dotnet ef database update --project Infrastructure --startup-project web
```

4. Run the application:

```sh
dotnet run --project web
```

5. Access the service at `http://localhost:5000`.

### Running with Visual Studio

1. Open the `TibberRobotService.sln` solution file in Visual Studio.
2. Create a `.env` file in the `web` directory based on the structure provided in `.env.example` or configure the environment variables in the project settings.
3. Apply the database migrations:
    - Open the Package Manager Console.
    - Run the following command:

```powershell
dotnet ef database update --project Infrastructure --startup-project web
```

4. Set the `web` project as the startup project.
5. Run the application by clicking the "Start" button or pressing `F5`.
6. Access the service at `http://localhost:5000`.

## Running Database Migrations

### Using .NET CLI

1. Ensure .NET 8 SDK is installed on your machine.
2. Navigate to the root directory of the project.

```sh
cd \path-to-your-project/RobotCleanerService
```

3. Add a new migration:

```sh
dotnet ef migrations add AddRowVersionToExecution --project Infrastructure --startup-project web
```

4. Apply the database migrations:

```sh
dotnet ef database update --project Infrastructure --startup-project web
```

### Using Visual Studio

1. Open the `TibberRobotService.sln` solution file in Visual Studio.
2. Open the Package Manager Console.
3. Add a new migration:

```powershell
Add-Migration AddRowVersionToExecution -Project Infrastructure -StartupProject web
```

4. Apply the database migrations:

```powershell
Update-Database -Project Infrastructure -StartupProject web
```

## Running Unit Tests

### Using .NET CLI

1. Navigate to the `Test` directory.

```sh
cd \path-to-your-project\RobotCleanerService\Test
```

2. Run the tests:

```sh
dotnet test
```

### Using Visual Studio

1. Open the `TibberRobotService.sln` solution file in Visual Studio.
2. In the Test Explorer, build the solution to discover all tests.
3. Run the tests by clicking on the "Run All" button in the Test Explorer.

## Optimizations

### Handling Robot Movement Commands

The handling of robot movement commands has been optimized to improve performance. Previously, a HashSet was used to store every unique position the robot visited. While this worked, it consumed a lot of memory and processing time, especially for large inputs.

Now, instead of storing each position, the boundaries of the robot's movement are tracked. By keeping track of the minimum and maximum X and Y coordinates, the number of unique positions visited can be determined using this formula:

```plaintext
uniquePositions = (maxX - minX + 1) * (maxY - minY + 1)
```

This approach reduces memory usage and speeds up processing, making it much more efficient for handling large inputs.

## Production Considerations

- Ensure that all sensitive information is stored securely and not hard-coded.
- Implement logging and monitoring to track the application’s performance and errors in production.
- Set up automated CI/CD pipelines to facilitate seamless deployment and scaling of the application.
- Use Docker for managing containers in a production environment.

## Conclusion

This solution is designed to be simple, clear, and efficient, adhering to best practices for structure, readability, maintainability, performance, reusability, and testability. The included Docker and docker-compose configurations ensure easy deployment and scalability within the Tibber Platform environment. For detailed configurations, please refer to the script files in the project directory.