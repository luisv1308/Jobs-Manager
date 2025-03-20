using System.Collections.Concurrent;
using System.Net;

namespace JobManager.Endpoints
{
    public static class JobValidator
    {
        public static List<string> ValidateJobRequest(JobRequest request, ConcurrentDictionary<string, Job> jobs)
        {
            var errors = new List<string>();

            // Validaciones para JobType
            if (string.IsNullOrEmpty(request.JobType))
                errors.Add("JobType is required");
            else if (request.JobType.Length < 3 || request.JobType.Length > 50)
                errors.Add("JobType must be between 3 and 50 characters");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(request.JobType, @"^[a-zA-Z0-9_]+$"))
                errors.Add("JobType can only contain letters, numbers, and underscores");

            // Validaciones para JobName
            if (string.IsNullOrEmpty(request.JobName))
                errors.Add("JobName is required");
            else if (request.JobName.Length < 3 || request.JobName.Length > 100)
                errors.Add("JobName must be between 3 and 100 characters");

            if (errors.Count == 0) // Solo verificar límite si no hay errores previos
            {
                var sameTypeCount = jobs.Values.Count(j => j.JobType == request.JobType && j.IsRunning);
                if (sameTypeCount >= 5)
                    errors.Add($"Maximum of 5 concurrent jobs of type {request.JobType} are allowed");
            }

            return errors;
        }
    }
}