using Application.Commands.ProcessRobotMovementCommands;
using Application.DTOs;
using Application.Enums;
using Application.Mappings;
using Application.Responses;
using AutoMapper;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test.Application.CommandTests.ProcessRobotMovementCommandTests
{
    /// <summary>
    /// Unit tests for the ProcessRobotMovementCommand.
    /// </summary>
    public class ProcessRobotMovementCommandTest
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProcessRobotMovementCommandHandler> _logger;
        private readonly ILogger<ProcessRobotMovementCommandTest> _testLogger;
        private readonly IMapper _mapper;
        private readonly IValidator<ProcessRobotMovementCommand> _validator;
        private readonly ProcessRobotMovementCommandHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRobotMovementCommandTest"/> class.
        /// </summary>
        public ProcessRobotMovementCommandTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ApplicationDbContext(options);

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole())
                .AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>())
                .AddScoped<IValidator<ProcessRobotMovementCommand>, ProcessRobotMovementCommandValidator>()
                .BuildServiceProvider();

            _logger = serviceProvider.GetRequiredService<ILogger<ProcessRobotMovementCommandHandler>>();
            _testLogger = serviceProvider.GetRequiredService<ILogger<ProcessRobotMovementCommandTest>>();
            _mapper = serviceProvider.GetRequiredService<IMapper>();
            _validator = serviceProvider.GetRequiredService<IValidator<ProcessRobotMovementCommand>>();
            _handler = new ProcessRobotMovementCommandHandler(_context, _logger, _mapper, _validator);
        }

        /// <summary>
        /// Tests processing a valid robot movement command with multiple directions.
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_ShouldReturnCorrectResult()
        {
            var command = new ProcessRobotMovementCommand
            {
                Start = new CoordinateDto { X = 0, Y = 0 },
                Commands = new List<MovementCommandDto>
                {
                    new() { Direction = DirectionEnum.East, Steps = 2 },
                    new() { Direction = DirectionEnum.North, Steps = 1 }
                }
            };

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(6, response.Result.Result);
        }

        /// <summary>
        /// Tests processing a robot movement command with movements in all directions.
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_MultipleDirections_ShouldReturnCorrectResult()
        {
            var command = new ProcessRobotMovementCommand
            {
                Start = new CoordinateDto { X = 0, Y = 0 },
                Commands = new List<MovementCommandDto>
                {
                    new() { Direction = DirectionEnum.East, Steps = 1 },
                    new() { Direction = DirectionEnum.North, Steps = 1 },
                    new() { Direction = DirectionEnum.West, Steps = 1 },
                    new() { Direction = DirectionEnum.South, Steps = 1 }
                }
            };

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(4, response.Result.Result); // Expecting 4 unique positions
        }

        /// <summary>
        /// Tests processing a robot movement command with no movement commands.
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_NoMovements_ShouldReturnCorrectResult()
        {
            var command = new ProcessRobotMovementCommand
            {
                Start = new CoordinateDto { X = 0, Y = 0 },
                Commands = new List<MovementCommandDto>()
            };

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal("Movement commands are required.", response.ErrorMessage);
        }

        /// <summary>
        /// Tests processing a robot movement command with a single movement command.
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_SingleMovement_ShouldReturnCorrectResult()
        {
            var command = new ProcessRobotMovementCommand
            {
                Start = new CoordinateDto { X = 0, Y = 0 },
                Commands = new List<MovementCommandDto>
                {
                    new() { Direction = DirectionEnum.East, Steps = 1 }
                }
            };

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(2, response.Result.Result); // Expecting 2 unique positions
        }

        /// <summary>
        /// Tests processing a robot movement command with duplicate movements.
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_DuplicateMovements_ShouldReturnCorrectResult()
        {
            var command = new ProcessRobotMovementCommand
            {
                Start = new CoordinateDto { X = 0, Y = 0 },
                Commands = new List<MovementCommandDto>
                {
                    new() { Direction = DirectionEnum.East, Steps = 1 },
                    new() { Direction = DirectionEnum.West, Steps = 1 },
                    new() { Direction = DirectionEnum.East, Steps = 1 }
                }
            };

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(2, response.Result.Result); // Expecting 2 unique positions (0,0) and (1,0)
        }

        /// <summary>
        /// Tests handling concurrent requests.
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_ConcurrentRequests_ShouldHandleConcurrency()
        {
            var command1 = new ProcessRobotMovementCommand
            {
                Start = new CoordinateDto { X = 0, Y = 0 },
                Commands = new List<MovementCommandDto>
                {
                    new() { Direction = DirectionEnum.East, Steps = 2 },
                    new() { Direction = DirectionEnum.North, Steps = 1 }
                }
            };

            var command2 = new ProcessRobotMovementCommand
            {
                Start = new CoordinateDto { X = 0, Y = 0 },
                Commands = new List<MovementCommandDto>
                {
                    new() { Direction = DirectionEnum.West, Steps = 2 },
                    new() { Direction = DirectionEnum.South, Steps = 1 }
                }
            };

            var tasks = new List<Task<CommandResponse<ExecutionResultDto>>>
            {
                _handler.Handle(command1, CancellationToken.None),
                _handler.Handle(command2, CancellationToken.None)
            };

            var responses = await Task.WhenAll(tasks);

            foreach (var response in responses)
            {
                Assert.True(response.IsSuccess);
            }
        }

        /// <summary>
        /// Tests handling a command with a null start coordinate.
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_NullStart_ShouldReturnNotFound()
        {
            var command = new ProcessRobotMovementCommand
            {
                Start = null,
                Commands = new List<MovementCommandDto>
                {
                    new() { Direction = DirectionEnum.East, Steps = 2 },
                    new() { Direction = DirectionEnum.North, Steps = 1 }
                }
            };

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal("Start coordinate is required.", response.ErrorMessage);
        }

        /// <summary>
        /// Tests handling a command with null movement commands.
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_NullCommands_ShouldReturnBadRequest()
        {
            var command = new ProcessRobotMovementCommand
            {
                Start = new CoordinateDto { X = 0, Y = 0 },
                Commands = null
            };

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal("Movement commands are required.", response.ErrorMessage);
        }

        /// <summary>
        /// Tests handling a command with an invalid direction value.
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_InvalidDirection_ShouldReturnBadRequest()
        {
            var command = new ProcessRobotMovementCommand
            {
                Start = new CoordinateDto { X = 0, Y = 0 },
                Commands = new List<MovementCommandDto>
                {
                    new() { Direction = (DirectionEnum)999, Steps = 1 }
                }
            };

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Contains("Invalid direction value.", response.ErrorMessage);
        }

        /// <summary>
        /// Gets the full path to the JSON file in the LargeJsonFiles directory.
        /// </summary>
        /// <param name="fileName">The name of the JSON file.</param>
        /// <returns>The full path to the JSON file.</returns>
        private static string GetJsonFilePath(string fileName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var projectDirectory = Directory.GetParent(currentDirectory).Parent.Parent.Parent.FullName;
            return Path.Combine(projectDirectory, "Test", "Application", "LargeJsonFiles", fileName);
        }

        /// <summary>
        /// Reads the content of a JSON file and converts it to a ProcessRobotMovementCommand object.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>The ProcessRobotMovementCommand object.</returns>
        private ProcessRobotMovementCommand ReadJsonFile(string filePath)
        {
            var jsonContent = File.ReadAllText(filePath);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ProcessRobotMovementCommand>(jsonContent);
        }

        /// <summary>
        /// Gets a ProcessRobotMovementCommand object from the specified JSON file.
        /// </summary>
        /// <param name="fileName">The name of the JSON file.</param>
        /// <returns>The ProcessRobotMovementCommand object.</returns>
        private ProcessRobotMovementCommand GetCommandFromFile(string fileName)
        {
            var filePath = GetJsonFilePath(fileName);
            var jsonContent = File.ReadAllText(filePath);
            var command = JsonConvert.DeserializeObject<ProcessRobotMovementCommand>(jsonContent);
            if (command == null)
            {
                throw new Exception("Deserialized command is null.");
            }
            return command;
        }

        /// <summary>
        /// Tests processing a robot movement command with large input (single JSON file).
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_LargeInput1_ShouldReturnCorrectResult()
        {
            var command = GetCommandFromFile("robotcleanerpathheavy.json");

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess, $"Expected success but got error: {response.ErrorMessage}");
            Assert.Equal(1916110409, response.Result?.Result);
        }

        /// <summary>
        /// Tests processing a robot movement command with large input (double JSON file).
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_LargeInput2_ShouldReturnCorrectResult()
        {
            var command = GetCommandFromFile("robotcleanerpathheavy_Double.json");

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess, $"Expected success but got error: {response.ErrorMessage}");
            Assert.Equal(-1860111887, response.Result?.Result);
        }

        /// <summary>
        /// Tests processing a robot movement command with large input (triple JSON file).
        /// </summary>
        [Fact]
        public async Task ProcessRobotMovementCommand_LargeInput3_ShouldReturnCorrectResult()
        {
            var command = GetCommandFromFile("robotcleanerpathheavy_Tripple.json");

            var response = await _handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess, $"Expected success but got error: {response.ErrorMessage}");
            Assert.Equal(-785121887, response.Result?.Result);
        }

    }
}
