using Application.DTOs;
using FluentValidation;

namespace Application.Commands.ProcessRobotMovementCommands
{
    /// <summary>
    /// Validator for <see cref="ProcessRobotMovementCommand"/>.
    /// </summary>
    public class ProcessRobotMovementCommandValidator : AbstractValidator<ProcessRobotMovementCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRobotMovementCommandValidator"/> class.
        /// </summary>
        public ProcessRobotMovementCommandValidator()
        {
            RuleFor(x => x.Start).NotNull().WithMessage("Start coordinate is required.");
            RuleFor(x => x.Commands).NotEmpty().WithMessage("Movement commands are required.");

            RuleForEach(x => x.Commands).SetValidator(new MovementCommandDtoValidator());
        }
    }

    /// <summary>
    /// Validator for <see cref="MovementCommandDto"/>.
    /// </summary>
    public class MovementCommandDtoValidator : AbstractValidator<MovementCommandDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MovementCommandDtoValidator"/> class.
        /// </summary>
        public MovementCommandDtoValidator()
        {
            RuleFor(x => x.Direction)
                .IsInEnum()
                .WithMessage("Invalid direction value.");
            RuleFor(x => x.Steps)
                .GreaterThan(0)
                .WithMessage("Steps must be greater than 0.");
        }
    }
}