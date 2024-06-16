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
            Assert.Equal(4, response.Result.Result);
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
    }
}
