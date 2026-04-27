using System.Threading.Tasks;
using WpfHttpApp.Models;

namespace WpfHttpApp.Services
{
    /// <summary>
    /// Service for thread-safe logging and UI updates.
    /// </summary>
    public interface ILoggerService
    {
        /// <summary>
        /// Logs a message.
        /// </summary>
        void Log(string message, LogLevel level = LogLevel.Info, string source = "System");

        /// <summary>
        /// Starts the background worker for file logging and exposes the stream for UI.
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Event triggered when a new log entry is available for the UI.
        /// </summary>
        event Action<LogEntry> OnLogReceived;
    }
}
