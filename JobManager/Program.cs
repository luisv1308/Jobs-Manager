using JobManager.Endpoints;
using JobManager.Middleware;
using JobManager.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Register the ConcurrentDictionary as a singleton
builder.Services.AddSingleton(new ConcurrentDictionary<string, Job>());

// Register SignalR
builder.Services.AddSignalR();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Register the JobHostedService
builder.Services.AddSingleton<IJobHostedService, JobHostedService>();
builder.Services.AddHostedService<JobHostedService>();
builder.Services.AddSingleton<IJobService, JobService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseExceptionMiddleware();

app.UseCors("AllowAll");

Console.WriteLine("Starting endpoint configuration...");
app.MapJobManagerEndpoints();
app.MapHub<JobHub>("/jobHub");
Console.WriteLine("Endpoints configured. Server running on http://localhost:5049");

app.Run("http://localhost:5049");