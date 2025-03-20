namespace JobManager.Services
{
    public interface IJobHostedService
    {
        Task StartJob(string jobId, string jobType, string jobName, CancellationTokenSource cts);
    }
}