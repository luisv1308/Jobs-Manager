// D:\programacion\net\JobManager\Services\JobHostedService.cs
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using JobManager.Endpoints;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace JobManager.Services
{
    public class JobHostedService : BackgroundService, IJobHostedService
    {
        private readonly ConcurrentDictionary<string, Job> _jobs;
        private readonly IHubContext<JobHub> _hubContext;
        private readonly ILogger<JobHostedService> _logger;

        public JobHostedService(ConcurrentDictionary<string, Job> jobs, IHubContext<JobHub> hubContext, ILogger<JobHostedService> logger)
        {
            _jobs = jobs;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task StartJob(string jobId, string jobType, string jobName, CancellationTokenSource cts)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), cts.Token);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation($"Job {jobId} was canceled");
                }
                finally
                {
                    if (_jobs.TryGetValue(jobId, out var job))
                    {
                        _jobs[jobId] = job with { IsRunning = false };
                        await NotifyJobUpdate(_jobs[jobId]);
                    }
                }
            }, cts.Token);

            // Update existing job instead of re-adding
            if (_jobs.ContainsKey(jobId))
            {
                var existingJob = _jobs[jobId];
                _jobs[jobId] = existingJob with { Task = task, Cts = cts, IsRunning = true };
            }
            else
            {
                var newJob = new Job(jobId, jobType, jobName, task, cts);
                _jobs.TryAdd(jobId, newJob);
            }

            await NotifyJobUpdate(_jobs[jobId]);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var job in _jobs.Values.Where(j => j.IsRunning))
            {
                job.Cts.Cancel();
                await NotifyJobUpdate(job with { IsRunning = false });
            }
            await base.StopAsync(cancellationToken);
        }

        private async Task NotifyJobUpdate(Job job)
        {
            Console.WriteLine($"Notifying update for Job {job.Id}: {job.JobName} - {job.IsRunning}");
            await _hubContext.Clients.Group("JobUpdates").SendAsync("JobUpdated", new
            {
                JobId = job.Id,
                Status = job.IsRunning ? "Running" : "Not Running",
                job.JobType,
                job.JobName
            });
        }
    }
}