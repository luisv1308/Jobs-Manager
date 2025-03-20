using JobClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using System;
using System.Threading.Tasks;

namespace JobClient.Services
{
    

    public class JobHubService : IJobHubService
    {
        private readonly HubConnection _hubConnection;

        public JobHubService()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5049/jobHub")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<JobModel>("JobUpdated", job =>
            {
                Log.Information($"Job Update: {job.JobId} ({job.JobType} - {job.JobName}) is {job.Status}");
            });
        }

        public async Task ConnectAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
                Log.Information("Connected to SignalR Hub");
                await _hubConnection.InvokeAsync("SubscribeToJobUpdates");
            }
            catch (Exception ex)
            {
                Log.Error($"Error connecting to SignalR Hub: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            await _hubConnection.StopAsync();
            Log.Information("Disconnected from SignalR Hub");
        }

        public async Task ExitApp()
        {
            await _hubConnection.StopAsync();
            Log.Information("Disconnected from SignalR Hub");
        }
    }
}
