using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobClient.Services
{
    public interface IJobHubService
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task ExitApp();
    }
}
