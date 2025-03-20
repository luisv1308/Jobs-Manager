using JobManager.Endpoints;

namespace JobManager.Exceptions
{
    public class ValidationException : Exception
    {
        public ErrorResponse Error { get; }

        public ValidationException(ErrorResponse error) : base(error.Message)
        {
            Error = error;
        }
    }
}