using JobManager.Endpoints;
using JobManager.Middleware;
using JobManager.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Register shared services
builder.Services.AddSingleton(new ConcurrentDictionary<string, Job>());
builder.Services.AddSingleton<IJobHostedService, JobHostedService>();
builder.Services.AddHostedService<JobHostedService>();
builder.Services.AddSingleton<IJobService, JobService>();

// Enable SignalR
builder.Services.AddSignalR();

// CORS (allow all)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
    });
});

builder.WebHost.UseUrls("http://*:5049");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Global error handling middleware
app.UseExceptionMiddleware();

// Enable CORS
app.UseCors("AllowAll");

// Map endpoints and SignalR hub
Console.WriteLine("Starting endpoint configuration...");
app.MapJobManagerEndpoints();
app.MapHub<JobHub>("/jobHub");
Console.WriteLine("Endpoints configured. Server running on http://*:5049");

app.Run();
