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
            // Unbounded channel for logs (Producer-Consumer)
            _logChannel = Channel.CreateUnbounded<LogEntry>(new UnboundedChannelOptions
            {
                SingleReader = true,
                AllowSynchronousContinuations = false
            });
        }

        public void Log(string message, LogLevel level = LogLevel.Info, string source = "System")
        {
            var entry = new LogEntry(DateTime.Now, level, message, source);
            _logChannel.Writer.TryWrite(entry);
            // We don't block here. The background worker handles the rest.
        }

        public async Task StartAsync()
        {
            _ = Task.Run(async () =>
            {
                await foreach (var entry in _logChannel.Reader.ReadAllAsync(_cts.Token))
                {
                    // Update UI via event (subscriber will handle Dispatcher)
                    OnLogReceived?.Invoke(entry);

                    // Write to file
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
                var logLine = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] [{entry.Source}] {entry.Message}{Environment.NewLine}";
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
