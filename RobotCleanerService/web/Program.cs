using Application;
using FluentValidation;
using Application.Commands.ProcessRobotMovementCommands;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using dotenv.net;
using Npgsql;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Determine if running in Docker
var isDocker = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker";

// Load environment variables from the correct .env file
var envFilePath = isDocker ? "./.env" : "../../web/.env";

DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, envFilePaths: new[] { envFilePath }));

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Load configuration files
if (isDocker)
{
    builder.Configuration.AddJsonFile("appsettings.docker.json", optional: false, reloadOnChange: true);
}
else
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
}

// Load environment variables
var pgHost = Environment.GetEnvironmentVariable("PG_DB_HOST") ?? "localhost";
var pgPort = Environment.GetEnvironmentVariable("PG_DB_PORT") ?? "5432";
var pgUsername = Environment.GetEnvironmentVariable("PG_DB_USERNAME");
var pgPassword = Environment.GetEnvironmentVariable("PG_DB_PASSWORD");
var pgDbName = Environment.GetEnvironmentVariable("PG_DB_NAME");


// Ensure these variables are not null
if (string.IsNullOrEmpty(pgHost) || string.IsNullOrEmpty(pgPort) || string.IsNullOrEmpty(pgUsername) || string.IsNullOrEmpty(pgPassword) || string.IsNullOrEmpty(pgDbName))
{
    throw new InvalidOperationException("Database credentials are not set in environment variables.");
}

// Build the connection string
var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = connectionStringTemplate
    .Replace("{PG_DB_HOST}", pgHost)
    .Replace("{PG_DB_PORT}", pgPort)
    .Replace("{PG_DB_NAME}", pgDbName)
    .Replace("{PG_DB_USERNAME}", pgUsername)
    .Replace("{PG_DB_PASSWORD}", pgPassword);

// Configure DbContext with the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TibberRobotService", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Add application and infrastructure services
var tempProvider = builder.Services.BuildServiceProvider();
var tempLoggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();
var tempLogger = tempLoggerFactory.CreateLogger("Program");

builder.Services.AddApplicationServices(tempLogger);
builder.Services.AddInfrastructureServices(builder.Configuration, connectionString, tempLogger);

// Register the validator
builder.Services.AddTransient<IValidator<ProcessRobotMovementCommand>, ProcessRobotMovementCommandValidator>();

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Enable detailed error messages and Swagger in development environment
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TibberRobotService v1"));

    // Redirect root URL to Swagger
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }

        await next();
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
