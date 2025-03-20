namespace JobManager.Endpoints
{
    public record Job(string Id, string JobType, string JobName, Task Task, CancellationTokenSource Cts, bool IsRunning = true);
    public record JobRequest(string JobType, string JobName);
    public record ErrorResponse(string ErrorCode, string Message, string[] Details = null);
}