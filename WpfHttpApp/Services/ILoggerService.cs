using System;
using System.Threading.Tasks;
using WpfHttpApp.Models;

namespace WpfHttpApp.Services
{
    public interface ILoggerService
    {
        void Log(string message,
            LogLevel level = LogLevel.Info,
            string source = "System",
            string method = "",
            int statusCode = 0,
            double processingMs = 0);

        Task StartAsync();

        event Action<LogEntry> OnLogReceived;
    }
}
