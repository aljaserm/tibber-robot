namespace Application.Responses
{
    public class ForbiddenCommandResponse<T>(string errorMessage) : FailCommandResponse<T>(errorMessage);
}