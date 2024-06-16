using Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult CommandResult<T>(CommandResponse<T> response)
        {
            if (response.IsSuccess)
            {
                return Ok(response.Result);
            }

            return response switch
            {
                NotFoundCommandResponse<T> => NotFound(new { message = response.ErrorMessage }),
                _ => BadRequest(new { message = response.ErrorMessage })
            };
        }
    }
}