using System;

namespace WpfHttpApp.Models
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public record LogEntry(DateTime Timestamp, LogLevel Level, string Message, string Source);
}
