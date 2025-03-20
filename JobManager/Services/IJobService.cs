using JobManager.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobManager.Services
{
    public interface IJobService
    {
        Task<string> CreateJob(JobRequest request);
        Task CancelJob(string jobId);
        Task<string> RestartJob(string jobId);
        Task DeleteJob(string jobId);
        Task DeleteAllJobs();

    }
}
