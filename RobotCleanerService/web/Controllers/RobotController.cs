using Application.Commands.ProcessRobotMovementCommands;
using Application.DTOs;
using Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("tibber-developer-test")]
    public class RobotController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator instance.</param>
        public RobotController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Processes the robot movement commands.
        /// </summary>
        /// <param name="command">The robot movement command.</param>
        /// <returns>The result of the robot movement execution.</returns>
        /// <response code="200">Returns the execution result of the robot movement</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the requested resource is not found</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpPost("enter-path")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExecutionResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EnterPath([FromBody] ProcessRobotMovementCommand command)
        {
            var response = await _mediator.Send(command);
            return CommandResult(response);
        }
    }
}