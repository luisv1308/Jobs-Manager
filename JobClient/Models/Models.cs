using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobClient.Models
{
    public record JobUpdate(string JobId, string JobType, string JobName, string Status);
    public record ErrorResponse(string ErrorCode, string Message, string[] Details = null);
    public record JobModel(string JobId, string JobType, string JobName, string Status);
}
