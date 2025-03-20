using JobClient;
using JobClient.Configs;
using JobClient.Helpers;
using JobClient.Models;
using JobClient.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


class Program
{
    private static IServiceProvider _serviceProvider;
    private static IJobService _jobService;
    private static IJobHubService _jobHubService;

    static async Task Main(string[] args)
    {
        ConfigureServices();

        // Get service instances
        _jobService = _serviceProvider.GetRequiredService<IJobService>();
        _jobHubService = _serviceProvider.GetRequiredService<IJobHubService>();

        await _jobHubService.ConnectAsync();
        await RunMenu();
    }

    private static void ConfigureServices()
    {
        LoggingConfig.ConfigureLogging();

        var services = new ServiceCollection();

        services.AddSingleton<HttpClient>(new HttpClient { BaseAddress = new Uri("http://localhost:5049") });
        services.AddSingleton<IJobService, JobService>();
        services.AddSingleton<IJobHubService, JobHubService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private static async Task RunMenu()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== Job Client Menu ===");
            Console.ResetColor();

            Console.WriteLine("[1] Create Job");
            Console.WriteLine("[2] List Jobs");
            Console.WriteLine("[3] List Jobs by Type");
            Console.WriteLine("[4] Jobs Stats");
            Console.WriteLine("[5] Active Jobs");
            Console.WriteLine("[6] Cancel Job");
            Console.WriteLine("[7] Restart Job");
            Console.WriteLine("[8] Delete Job");
            Console.WriteLine("[9] Delete All Jobs");
            Console.WriteLine("[10] Disconnect");
            Console.Write("Select an option (timeout in 30s): ");

            var option = await ConsoleUtils.GetUserInputWithTimeout(30);

            if (string.IsNullOrEmpty(option))
            {
                Log.Warning("No input received. Returning to menu...");
                continue; // Resets back to the menu without user action
            }

            try
            {
                switch (option)
                {
                    case "1":
                        await _jobService.CreateJob(CancellationToken.None);
                        break;
                    case "2":
                        await _jobService.ListJobs(CancellationToken.None);
                        break;
                    case "3":
                        await _jobService.ListJobsByType(CancellationToken.None);
                        break;
                    case "4":
                        await _jobService.JobsStats(CancellationToken.None);
                        break;
                    case "5":
                        await _jobService.ActiveJobs(CancellationToken.None);
                        break;
                    case "6":
                        await _jobService.CancelJob(CancellationToken.None);
                        break;
                    case "7":
                        await _jobService.RestartJob(CancellationToken.None);
                        break;
                    case "8":
                        await _jobService.DeleteJob(CancellationToken.None);
                        break;
                    case "9":
                        await _jobService.DeleteAllJobs(CancellationToken.None);
                        break;
                    case "10":
                        Console.Write("Are you sure you want to disconnect? (y/n): ");
                        if (Console.ReadLine()?.ToLower() == "y")
                        {
                            await _jobHubService.ExitApp();
                            return;
                        }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Log.Error("Invalid option: {Option}", option);
                        Console.ResetColor();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while executing option {Option}", option);
            }
        }
    }
}
