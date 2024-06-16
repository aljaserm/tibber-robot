namespace Application.Responses
{
    public class SuccessCommandResponse<T>(T result) : CommandResponse<T>(true, result, null);
}