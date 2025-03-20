using Xunit;
using System.Collections.Concurrent;
using JobManager.Endpoints;

namespace JobManager.Tests
{
    public class JobValidatorTests
    {
        [Fact]
        public void ValidateJobRequest_ValidInput_ReturnsEmptyList()
        {
            var jobs = new ConcurrentDictionary<string, Job>();
            var request = new JobRequest("Report", "SalesReport");

            var errors = JobValidator.ValidateJobRequest(request, jobs);

            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateJobRequest_EmptyJobType_ReturnsError()
        {
            var jobs = new ConcurrentDictionary<string, Job>();
            var request = new JobRequest("", "SalesReport");

            var errors = JobValidator.ValidateJobRequest(request, jobs);

            Assert.Contains("JobType is required", errors);
        }

        [Fact]
        public void ValidateJobRequest_InvalidJobTypeFormat_ReturnsError()
        {
            var jobs = new ConcurrentDictionary<string, Job>();
            var request = new JobRequest("Report@!", "SalesReport");

            var errors = JobValidator.ValidateJobRequest(request, jobs);

            Assert.Contains("JobType can only contain letters, numbers, and underscores", errors);
        }

        [Fact]
        public void ValidateJobRequest_TooManyConcurrentJobs_ReturnsError()
        {
            var jobs = new ConcurrentDictionary<string, Job>();
            for (int i = 0; i < 5; i++)
                jobs.TryAdd($"job{i}", new Job($"job{i}", "Report", $"Job{i}", Task.CompletedTask, new CancellationTokenSource(), true));
            var request = new JobRequest("Report", "SalesReport");

            var errors = JobValidator.ValidateJobRequest(request, jobs);

            Assert.Contains("Maximum of 5 concurrent jobs of type Report are allowed", errors);
        }
    }
}