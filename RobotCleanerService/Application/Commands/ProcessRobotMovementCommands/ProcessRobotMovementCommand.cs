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

            var stopwatch = Stopwatch.StartNew();

            var uniquePositions = new HashSet<(int, int)>();
            var currentX = request.Start.X;
            var currentY = request.Start.Y;
            uniquePositions.Add((currentX, currentY));

            foreach (var command in request.Commands)
            {
                _logger.LogInformation("Processing command: Direction={Direction}, Steps={Steps}", command.Direction, command.Steps);

                for (int i = 0; i < command.Steps; i++)
                {
                    MovementUtility.Move(command.Direction, ref currentX, ref currentY, uniquePositions);
                }
            }

            stopwatch.Stop();

            var execution = new Execution
            {
                Timestamp = DateTime.UtcNow,
                Commands = request.Commands.Count,
                Result = uniquePositions.Count,
                Duration = stopwatch.Elapsed.TotalSeconds
            };

            _logger.LogInformation("Processing complete. Duration: {Duration}s", stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Saving execution result to the database.");

            bool saved = false;
            int maxRetries = 5;
            int retryCount = 0;

            while (!saved)
            {
                try
                {
                    _context.Executions.Add(execution);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Execution result saved successfully with ID: {ExecutionId}", execution.Id);
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning("Concurrency conflict detected: {Message}", ex.Message);
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError("Max retry attempts reached. Could not save execution result.");
                        return new FailCommandResponse<ExecutionResultDto>("Concurrency conflict occurred. Please try again later.");
                    }

                    // Reload the entity and retry
                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
            }

            return CommandResponse<ExecutionResultDto>.Success(_mapper.Map<ExecutionResultDto>(execution));
        }
    }
}
