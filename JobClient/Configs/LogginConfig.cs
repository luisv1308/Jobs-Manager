using Serilog;

namespace JobClient.Configs
{
    public static class LoggingConfig
    {
        public static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/jobclient.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}
