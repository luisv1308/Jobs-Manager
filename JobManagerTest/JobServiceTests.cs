// D:\programacion\net\JobManagerTest\JobServiceTests.cs
using Xunit;
using Moq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using JobManager.Endpoints;
using JobManager.Exceptions;
using JobManager.Services;

namespace JobManager.Tests
{
    public class JobServiceTests
    {
        private readonly Mock<IJobHostedService> _jobHostedServiceMock;
        private readonly Mock<IHubContext<JobHub>> _hubContextMock;
        private readonly Mock<ILogger<JobService>> _loggerMock;
        private readonly ConcurrentDictionary<string, Job> _jobs;

        public JobServiceTests()
        {
            _jobs = new ConcurrentDictionary<string, Job>();
            _hubContextMock = new Mock<IHubContext<JobHub>>();
            _loggerMock = new Mock<ILogger<JobService>>();

            _jobHostedServiceMock = new Mock<IJobHostedService>();
            _jobHostedServiceMock.Setup(x => x.StartJob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationTokenSource>()))
                .Returns(Task.CompletedTask);

            // Configurar _hubContextMock sin mockear SendAsync directamente
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(x => x.Group("JobUpdates")).Returns(clientProxyMock.Object);
            _hubContextMock.Setup(x => x.Clients).Returns(clientsMock.Object);
            // No mockeamos SendAsync como extensión, usamos el método subyacente SendCoreAsync
            clientProxyMock.Setup(x => x.SendCoreAsync("JobUpdated", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task CreateJob_ValidRequest_CreatesJob()
        {
            var service = new JobService(_jobs, _jobHostedServiceMock.Object, _hubContextMock.Object, _loggerMock.Object);
            var request = new JobRequest("Report", "SalesReport");

            var jobId = await service.CreateJob(request);

            Assert.False(string.IsNullOrEmpty(jobId));
            _jobHostedServiceMock.Verify(x => x.StartJob(jobId, "Report", "SalesReport", It.IsAny<CancellationTokenSource>()), Times.Once());
        }

        [Fact]
        public async Task CreateJob_InvalidRequest_ThrowsValidationException()
        {
            var service = new JobService(_jobs, _jobHostedServiceMock.Object, _hubContextMock.Object, _loggerMock.Object);
            var request = new JobRequest("", "SalesReport");

            var exception = await Assert.ThrowsAsync<ValidationException>(() => service.CreateJob(request));
            Assert.Equal("InvalidInput", exception.Error.ErrorCode);
            Assert.Equal("Validation failed", exception.Error.Message);
        }

        [Fact]
        public async Task CancelJob_ExistingRunningJob_CancelsSuccessfully()
        {
            var jobId = "job1";
            var cts = new CancellationTokenSource();
            _jobs.TryAdd(jobId, new Job(jobId, "Report", "SalesReport", Task.CompletedTask, cts, true));
            var service = new JobService(_jobs, _jobHostedServiceMock.Object, _hubContextMock.Object, _loggerMock.Object);

            await service.CancelJob(jobId);

            Assert.True(cts.IsCancellationRequested);
            _hubContextMock.Verify(x => x.Clients.Group("JobUpdates")
                .SendCoreAsync("JobUpdated", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CancelJob_NonExistingJob_ThrowsKeyNotFoundException()
        {
            var service = new JobService(_jobs, _jobHostedServiceMock.Object, _hubContextMock.Object, _loggerMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CancelJob("nonexistent"));
        }
    }
}