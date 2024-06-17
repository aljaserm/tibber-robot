using Application.DTOs;
using Application.Enums;
using Application.Responses;
using Application.Utilities;
using AutoMapper;
using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Commands.ProcessRobotMovementCommands
{
    /// <summary>
    /// Command to process robot movement.
    /// </summary>
    public class ProcessRobotMovementCommand : IRequest<CommandResponse<ExecutionResultDto>>
    {
        /// <summary>
        /// Gets or sets the starting coordinate.
        /// </summary>
        public CoordinateDto Start { get; set; }

        /// <summary>
        /// Gets or sets the list of movement commands.
        /// </summary>
        public List<MovementCommandDto>? Commands { get; set; }
    }

    /// <summary>
    /// Handles the processing of the robot movement command.
    /// </summary>
    public class ProcessRobotMovementCommandHandler : IRequestHandler<ProcessRobotMovementCommand, CommandResponse<ExecutionResultDto>>
    {
        private const int MAX_RETRIES = 5;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProcessRobotMovementCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IValidator<ProcessRobotMovementCommand> _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRobotMovementCommandHandler"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="mapper">The AutoMapper instance.</param>
        /// <param name="validator">The validator instance.</param>
        public ProcessRobotMovementCommandHandler(ApplicationDbContext context, ILogger<ProcessRobotMovementCommandHandler> logger, IMapper mapper, IValidator<ProcessRobotMovementCommand> validator)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _validator = validator;
        }

        /// <summary>
        /// Handles the processing of the robot movement command.
        /// Implements retry logic for concurrency conflicts.
        /// </summary>
        /// <param name="request">The robot movement command request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the robot movement execution.</returns>
        public async Task<CommandResponse<ExecutionResultDto>> Handle(ProcessRobotMovementCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var errorMessage = string.Join(", ", errors);
                _logger.LogWarning("Invalid request: {ErrorMessage}", errorMessage);
                return new FailCommandResponse<ExecutionResultDto>(errorMessage);
            }

            _logger.LogInformation("Starting to process robot movement command.");

            /*
              The handling of robot movement commands has been optimized to improve performance.
              Previously, a HashSet was used to store every unique position the robot visited. While this worked,
              it consumed a lot of memory and processing time, especially for large inputs.

              Now, instead of storing each position, the boundaries of the robot's movement are tracked.
              By keeping track of the minimum and maximum X and Y coordinates, the number of unique positions
              visited can be determined using this formula:

              uniquePositions = (maxX - minX + 1) * (maxY - minY + 1)

              This approach reduces memory usage and speeds up processing, making it much more efficient for
              handling large inputs.
           */

            var stopwatch = Stopwatch.StartNew();

            int minX = request.Start.X, maxX = request.Start.X, minY = request.Start.Y, maxY = request.Start.Y;
            int currentX = request.Start.X, currentY = request.Start.Y;

            foreach (var command in request.Commands)
            {
                _logger.LogInformation("Processing command: Direction={Direction}, Steps={Steps}", command.Direction, command.Steps);
                MovementUtility.UpdateBoundaries(command, ref minX, ref maxX, ref minY, ref maxY, ref currentX, ref currentY);
            }

            stopwatch.Stop();

            int uniquePositions = (maxX - minX + 1) * (maxY - minY + 1);

            var execution = new Execution
            {
                Timestamp = DateTime.UtcNow,
                Commands = request.Commands.Count,
                Result = uniquePositions,
                Duration = stopwatch.Elapsed.TotalSeconds
            };

            _logger.LogInformation("Processing complete. Duration: {Duration}s", stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Saving execution result to the database.");

            try
            {
                await ConcurrencyUtility.SaveWithRetriesAsync(_context, execution, _logger, MAX_RETRIES, cancellationToken);
            }
            catch (Exception ex)
            {
                return new FailCommandResponse<ExecutionResultDto>(ex.Message);
            }

            return CommandResponse<ExecutionResultDto>.Success(_mapper.Map<ExecutionResultDto>(execution));
        }
    }
}
