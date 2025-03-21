using System.Collections.Concurrent;
using JobManager.Services;
using JobManager.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;

namespace JobManager.Endpoints
{
    public static class JobManagerEndpoints
    {
        public static void MapJobManagerEndpoints(this WebApplication app)
        {
            var jobs = app.Services.GetRequiredService<ConcurrentDictionary<string, Job>>();
            var jobService = app.Services.GetRequiredService<IJobService>();

            // GET Endpoints
            app.MapGet("/jobs/{jobId}", (string jobId) =>
            {
                if (!jobs.TryGetValue(jobId, out var job))
                    return Results.NotFound(new ErrorResponse("JobNotFound", $"Job {jobId} not found"));

                return Results.Ok(new
                {
                    JobId = job.Id,
                    Status = job.IsRunning ? "Running" : "Not Running",
                    job.JobType,
                    job.JobName
                });
            });

            app.MapGet("/jobs/active", () =>
            {
                var activeJobs = jobs.Values
                    .Where(j => j.IsRunning)
                    .Select(j => new
                    {
                        JobId = j.Id,
                        j.JobType,
                        j.JobName,
                        Status = "Running"
                    });
                return Results.Ok(activeJobs);
            });

            app.MapGet("/jobs/by-type/{jobType}", (string jobType) =>
            {
                var jobsByType = jobs.Values
                    .Where(j => j.JobType == jobType)
                    .Select(j => new
                    {
                        JobId = j.Id,
                        Status = j.IsRunning ? "Running" : "Not Running",
                        j.JobType,
                        j.JobName
                    });
                return Results.Ok(jobsByType);
            });

            app.MapGet("/jobs/list", () =>
            {
                var allJobs = jobs.Values.Select(j => new
                {
                    JobId = j.Id,
                    Status = j.IsRunning ? "Running" : "Not Running",
                    j.JobType,
                    j.JobName
                });
                return Results.Ok(allJobs);
            });

            app.MapGet("/jobs/stats", () =>
            {
                var totalJobs = jobs.Count;
                var activeJobs = jobs.Values.Count(j => j.IsRunning);
                var byType = jobs.Values
                    .GroupBy(j => j.JobType)
                    .Select(g => new { JobType = g.Key, Count = g.Count(), Active = g.Count(j => j.IsRunning) });

                return Results.Ok(new
                {
                    TotalJobs = totalJobs,
                    ActiveJobs = activeJobs,
                    JobsByType = byType
                });
            });

            // POST Endpoints
            app.MapPost("/jobs", async (JobRequest request) =>
            {
                var jobId = await jobService.CreateJob(request);
                return Results.Ok(new { JobId = jobId });
            });

            app.MapPost("/jobs/{jobId}/cancel", async (string jobId) =>
            {
                await jobService.CancelJob(jobId);
                return Results.Ok(new { JobId = jobId, Message = "Cancellation requested" });
            });

            app.MapPost("/jobs/{jobId}/restart", async (string jobId) =>
            {
                var newJobId = await jobService.RestartJob(jobId);
                return Results.Ok(new { JobId = newJobId, Message = "Job restarted" });
            });

            // DELETE Endpoints
            app.MapDelete("/jobs/{jobId}", async (string jobId) =>
            {
                await jobService.DeleteJob(jobId);
                return Results.Ok(new { JobId = jobId, Message = "Job removed" });
            });

            app.MapDelete("/jobs", async () =>
            {
                await jobService.DeleteAllJobs();
                return Results.Ok(new { Message = "All jobs removed" });
            });
        }
    }
}