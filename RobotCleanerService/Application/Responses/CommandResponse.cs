namespace Application.Responses
{
    public abstract class CommandResponse<T>
    {
        public bool IsSuccess { get; }
        public T Result { get; }
        public string ErrorMessage { get; }

        protected CommandResponse(bool isSuccess, T result, string errorMessage)
        {
            IsSuccess = isSuccess;
            Result = result;
            ErrorMessage = errorMessage;
        }

        public static CommandResponse<T> Success(T result) => new SuccessCommandResponse<T>(result);
        public static CommandResponse<T> Fail(string errorMessage) => new FailCommandResponse<T>(errorMessage);
    }
}