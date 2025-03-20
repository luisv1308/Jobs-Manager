using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace JobClient.Helpers
{
    public static class ConsoleUtils
    {
        public static async Task<string> GetUserInputWithTimeout(int timeoutSeconds)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            var inputTask = Task.Run(() => Console.ReadLine());
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), cts.Token);

            var completedTask = await Task.WhenAny(inputTask, timeoutTask);

            if (completedTask == inputTask)
            {
                cts.Cancel(); // Cancel timeout since input was received
                return await inputTask;
            }

            Log.Warning("No input received within {TimeoutSeconds} seconds. Returning to menu...", timeoutSeconds);
            return null;
        }
    }
}
