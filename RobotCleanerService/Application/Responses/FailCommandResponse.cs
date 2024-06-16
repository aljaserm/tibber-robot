namespace Application.Responses
{
    public class FailCommandResponse<T>(string errorMessage) : CommandResponse<T>(false, default, errorMessage);
}