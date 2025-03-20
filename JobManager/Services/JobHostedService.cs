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

        public JobHostedService(ConcurrentDictionary<string, Job> jobs, IHubContext<JobHub> hubContext)
        {
            _jobs = jobs;
            _hubContext = hubContext;
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
                    Console.WriteLine($"Job {jobId} was canceled");
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

            var job = new Job(jobId, jobType, jobName, task, cts);
            _jobs.TryAdd(jobId, job);
            await NotifyJobUpdate(job);
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