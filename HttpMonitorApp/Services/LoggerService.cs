using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using WpfHttpApp.Models;

namespace WpfHttpApp.Services
{
    public class LoggerService : ILoggerService, IDisposable
    {
        private readonly Channel<LogEntry> _logChannel;
        private readonly string _logFilePath = "logs.txt";
        private readonly SemaphoreSlim _fileSemaphore = new(1, 1);
        private readonly CancellationTokenSource _cts = new();

        public event Action<LogEntry>? OnLogReceived;

        public LoggerService()
        {
            _logChannel = Channel.CreateUnbounded<LogEntry>(new UnboundedChannelOptions
            {
                SingleReader = true,
                AllowSynchronousContinuations = false
            });
        }

        public void Log(string message,
            LogLevel level = LogLevel.Info,
            string source = "System",
            string method = "",
            int statusCode = 0,
            double processingMs = 0)
        {
            var entry = new LogEntry(DateTime.Now, level, message, source, method, statusCode, processingMs);
            _logChannel.Writer.TryWrite(entry);
        }

        public async Task StartAsync()
        {
            _ = Task.Run(async () =>
            {
                await foreach (var entry in _logChannel.Reader.ReadAllAsync(_cts.Token))
                {
                    OnLogReceived?.Invoke(entry);
                    await WriteToFileAsync(entry);
                }
            }, _cts.Token);

            await Task.CompletedTask;
        }

        private async Task WriteToFileAsync(LogEntry entry)
        {
            await _fileSemaphore.WaitAsync();
            try
            {
                var logLine = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.Level,-7}] [{entry.Method,-4}] [{entry.Source,-6}] {entry.Message}{Environment.NewLine}";
                await File.AppendAllTextAsync(_logFilePath, logLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
            finally
            {
                _fileSemaphore.Release();
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _fileSemaphore.Dispose();
            _cts.Dispose();
        }
    }
}
