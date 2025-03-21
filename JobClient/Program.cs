using DotNetEnv;
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
    private static MenuService _menuService;


    static async Task Main(string[] args)
    {
        ConfigureServices();

        // Get service instances
        _jobService = _serviceProvider.GetRequiredService<IJobService>();
        _jobHubService = _serviceProvider.GetRequiredService<IJobHubService>();
        _menuService = _serviceProvider.GetRequiredService<MenuService>();


        await _jobHubService.ConnectAsync();
        await _menuService.Run();
    }

    private static void ConfigureServices()
    {
        Env.Load();

        LoggingConfig.ConfigureLogging();

        var services = new ServiceCollection();

        var apiUrl = Env.GetString("API_URL", "http://localhost:5049");

        services.AddSingleton<HttpClient>(new HttpClient { BaseAddress = new Uri(apiUrl) });
        services.AddSingleton<IJobService, JobService>();
        services.AddSingleton<IJobHubService, JobHubService>();
        services.AddSingleton<MenuService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    
}
