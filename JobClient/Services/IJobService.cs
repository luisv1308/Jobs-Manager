using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobClient.Services
{
    public interface IJobService
    {
        Task<string> CreateJob(CancellationToken cancellationToken);
        Task ListJobs(CancellationToken cancellationToken);
        Task ListJobsByType(CancellationToken cancellationToken);
        Task JobsStats(CancellationToken cancellationToken);
        Task ActiveJobs(CancellationToken cancellationToken);
        Task CancelJob(CancellationToken cancellationToken);
        Task RestartJob(CancellationToken cancellationToken);
        Task DeleteJob(CancellationToken cancellationToken);
        Task DeleteAllJobs(CancellationToken cancellationToken);
    }
}
