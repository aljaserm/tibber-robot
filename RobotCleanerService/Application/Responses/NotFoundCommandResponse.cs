namespace Application.Responses
{
    public class NotFoundCommandResponse<T>(string errorMessage) : FailCommandResponse<T>(errorMessage);
}