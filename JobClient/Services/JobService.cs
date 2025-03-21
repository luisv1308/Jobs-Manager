using JobClient;
using JobClient.Services;
using Serilog;
using System.Net.Http.Json;
using System.Text.Json;
using JobClient.Helpers;
using JobClient.Configs;

public class JobService : IJobService
{
    private readonly HttpClient _httpClient;
    private static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(10);

    public JobService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
    }

    private async Task<HttpResponseMessage> SendRequestWithTimeout(Func<CancellationToken, Task<HttpResponseMessage>> requestFunc, CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(TimeoutDuration);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        return await requestFunc(linkedCts.Token);
    }

    public async Task<string> CreateJob(CancellationToken cancellationToken)
    {
        Console.Write("Enter Job Type: ");
        var jobType = Console.ReadLine();
        Console.Write("Enter Job Name: ");
        var jobName = Console.ReadLine();
        var request = new { JobType = jobType, JobName = jobName };
        var response = await _httpClient.PostAsJsonAsync("/jobs", request, cancellationToken);


        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            return result.GetProperty("jobId").GetString();
        }
        else
        {
            Log.Error(await HttpUtils.HandleErrorResponse(response));
            return null;
        }
    }

    public async Task ListJobs(CancellationToken cancellationToken)
    {
        var response = await SendRequestWithTimeout(
            token => _httpClient.GetAsync("/jobs/list", token),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var jobs = await response.Content.ReadFromJsonAsync<List<JsonElement>>(cancellationToken);
            if (jobs == null || jobs.Count == 0)
            {
                Log.Warning("No jobs found.");               
            }
            foreach (var job in jobs)
            {
                string jobId = job.GetProperty("jobId").GetString();
                string jobType = job.GetProperty("jobType").GetString();
                string jobName = job.GetProperty("jobName").GetString();
                string status = job.GetProperty("status").GetString();
                Log.Information($"ID: {jobId}, Type: {jobType}, Name: {jobName}, Status: {status}");
            }
        }
        else
        {
            Log.Error(await HttpUtils.HandleErrorResponse(response));
        }
    }

    public async Task ListJobsByType(CancellationToken cancellationToken)
    {
        Console.Write("Enter Job Type to list: ");
        var jobType = Console.ReadLine();

        var response = await SendRequestWithTimeout(
            token => _httpClient.GetAsync($"/jobs/by-type/{jobType}", token),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var jobs = await response.Content.ReadFromJsonAsync<List<JsonElement>>(cancellationToken);
            if (jobs == null || jobs.Count == 0)
            {
                Console.WriteLine($"No jobs of type {jobType} found.");
                return;
            }
            foreach (var job in jobs)
            {
                Log.Information($"ID: {job.GetProperty("jobId").GetString()}, Name: {job.GetProperty("jobName").GetString()}, Status: {job.GetProperty("status").GetString()}");
            }
        }
        else
        {
            Log.Error(await HttpUtils.HandleErrorResponse(response));
        }
    }

    public async Task JobsStats(CancellationToken cancellationToken)
    {
        var response = await SendRequestWithTimeout(
            token => _httpClient.GetAsync("/jobs/stats", token),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var stats = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            Console.WriteLine($"Total Jobs: {stats.GetProperty("totalJobs").GetInt32()}");
            Console.WriteLine($"Active Jobs: {stats.GetProperty("activeJobs").GetInt32()}");
            Console.WriteLine("Jobs by Type:");
            foreach (var type in stats.GetProperty("jobsByType").EnumerateArray())
            {
                Log.Information($"  Type: {type.GetProperty("jobType").GetString()}, Count: {type.GetProperty("count").GetInt32()}, Active: {type.GetProperty("active").GetInt32()}");
            }
        }
        else
        {
            Log.Error(await HttpUtils.HandleErrorResponse(response));
        }
    }

    public async Task ActiveJobs(CancellationToken cancellationToken)
    {
        var response = await SendRequestWithTimeout(
            token => _httpClient.GetAsync("/jobs/active", token),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var jobs = await response.Content.ReadFromJsonAsync<List<JsonElement>>(cancellationToken);
            foreach (var job in jobs)
            {
                Log.Information($"ID: {job.GetProperty("jobId").GetString()}, Type: {job.GetProperty("jobType").GetString()}, Name: {job.GetProperty("jobName").GetString()}, Status: {job.GetProperty("status").GetString()}");
            }
            if (jobs.Count == 0)
            {
                Log.Warning("No active jobs found.");
            }
        }
        else
        {
            Log.Error(await HttpUtils.HandleErrorResponse(response));
        }
    }

    public async Task CancelJob(CancellationToken cancellationToken)
    {
        Console.Write("Enter Job ID to cancel: ");
        var jobId = Console.ReadLine();
        var response = await SendRequestWithTimeout(
            token => _httpClient.PostAsync($"/jobs/{jobId}/cancel", null, token),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            Log.Information($"Job {jobId}: {result.GetProperty("message").GetString()}");
        }
        else
        {
            Log.Error(await HttpUtils.HandleErrorResponse(response));
        }
    }

    public async Task RestartJob(CancellationToken cancellationToken)
    {
        Console.Write("Enter Job ID to restart: ");
        var jobId = Console.ReadLine();
        var response = await SendRequestWithTimeout(
            token => _httpClient.PostAsync($"/jobs/{jobId}/restart", null, token),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            Log.Information($"{result.GetProperty("message").GetString()} New ID: {result.GetProperty("jobId").GetString()}");
        }
        else
        {
            Log.Error(await HttpUtils.HandleErrorResponse(response));
        }
    }

    public async Task DeleteJob(CancellationToken cancellationToken)
    {
        Console.Write("Enter Job ID to delete: ");
        var jobId = Console.ReadLine();
        var response = await SendRequestWithTimeout(
            token => _httpClient.DeleteAsync($"/jobs/{jobId}", token),
            cancellationToken
        );
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            Log.Information($"Job {jobId}: {result.GetProperty("message").GetString()}");
        }
        else
        {
            Log.Error(await HttpUtils.HandleErrorResponse(response));
        }
    }

    public async Task DeleteAllJobs(CancellationToken cancellationToken)
    {
        Console.WriteLine("Are you sure you want to delete all jobs? (y/n)");
        if (Console.ReadLine()?.ToLower() != "y") return;
        var response = await SendRequestWithTimeout(
            token => _httpClient.DeleteAsync("/jobs", token),
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            Log.Information($"{result.GetProperty("message").GetString()}");
        }
        else
        {
            Log.Error(await HttpUtils.HandleErrorResponse(response));
        }
    }
}
