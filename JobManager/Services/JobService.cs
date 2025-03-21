// D:\programacion\net\JobManager\Endpoints\JobService.cs
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using JobManager.Exceptions;
using JobManager.Services; // Asegúrate de tener este using

namespace JobManager.Endpoints
{
    public class JobService : IJobService
    {
        private readonly ConcurrentDictionary<string, Job> _jobs;
        private readonly IJobHostedService _jobHostedService;
        private readonly IHubContext<JobHub> _hubContext;
        private readonly ILogger<JobService> _logger;

        public JobService(
            ConcurrentDictionary<string, Job> jobs,
            IJobHostedService jobHostedService, 
            IHubContext<JobHub> hubContext,
            ILogger<JobService> logger)
        {
            _jobs = jobs;
            _jobHostedService = jobHostedService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<string> CreateJob(JobRequest request)
        {
            var errors = JobValidator.ValidateJobRequest(request, _jobs);
            if (errors.Count > 0)
                throw new ValidationException(new ErrorResponse("InvalidInput", "Validation failed", errors.ToArray()));

            var jobId = Guid.NewGuid().ToString();
            var cts = new CancellationTokenSource();
            _logger.LogInformation("Creating job {JobId} with type {JobType} and name {JobName}", jobId, request.JobType, request.JobName);
            await _jobHostedService.StartJob(jobId, request.JobType, request.JobName, cts);
            return jobId;
        }

        public async Task CancelJob(string jobId)
        {
            if (!_jobs.TryGetValue(jobId, out var job))
                throw new KeyNotFoundException($"Job {jobId} not found");
            if (!job.IsRunning)
                throw new InvalidOperationException($"Job {jobId} is not running");

            _logger.LogInformation("Cancelling job {JobId}", jobId);
            job.Cts.Cancel();
            await _hubContext.Clients.Group("JobUpdates").SendAsync("JobUpdated", new
            {
                JobId = job.Id,
                Status = "Not Running",
                job.JobType,
                job.JobName
            });
        }

        public async Task<string> RestartJob(string jobId)
        {
            if (!_jobs.TryGetValue(jobId, out var job))
                throw new KeyNotFoundException($"Job {jobId} not found");

            if (job.IsRunning)
                throw new InvalidOperationException($"Job {jobId} is still running");

            // Replace the cancellation token
            var cts = new CancellationTokenSource();
            _logger.LogInformation("Restarting job {JobId}", jobId);

            // Update job object with a fresh cancellation token
            var updatedJob = job with { Cts = cts, IsRunning = true };
            _jobs[jobId] = updatedJob;

            // Restart the same job
            await _jobHostedService.StartJob(jobId, job.JobType, job.JobName, cts);
            return jobId;
        }

        public async Task DeleteJob(string jobId)
        {
            if (!_jobs.TryRemove(jobId, out var job))
                throw new KeyNotFoundException($"Job {jobId} not found");

            if (job.IsRunning)
            {
                _logger.LogInformation("Deleting running job {JobId}", jobId);
                job.Cts.Cancel();
                await _hubContext.Clients.Group("JobUpdates").SendAsync("JobUpdated", new
                {
                    JobId = job.Id,
                    Status = "Not Running",
                    job.JobType,
                    job.JobName,
                    Message = "Job removed"
                });
            }
            else
            {
                _logger.LogInformation("Deleting stopped job {JobId}", jobId);
            }
        }

        public async Task DeleteAllJobs()
        {
            _logger.LogInformation("Deleting all jobs. Total count: {Count}", _jobs.Count);
            foreach (var job in _jobs.Values)
            {
                if (job.IsRunning)
                {
                    job.Cts.Cancel();
                    await _hubContext.Clients.Group("JobUpdates").SendAsync("JobUpdated", new
                    {
                        JobId = job.Id,
                        Status = "Not Running",
                        job.JobType,
                        job.JobName,
                        Message = "All jobs removed"
                    });
                }
            }
            _jobs.Clear();
        }
    }
}