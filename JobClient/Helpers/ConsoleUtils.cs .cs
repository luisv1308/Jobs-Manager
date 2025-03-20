using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace JobClient.Helpers
{
    public static class ConsoleUtils
    {
        public static async Task<string> GetUserInput()
        {
            return await Task.Run(() => Console.ReadLine());
        }
    }
}
