using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfHttpApp.Services
{
    public class HttpServerService : IHttpServerService, IDisposable
    {
        private HttpListener? _listener;
        private readonly ILoggerService _logger;
        private CancellationTokenSource? _cts;

        public bool IsRunning => _listener?.IsListening ?? false;

        public HttpServerService(ILoggerService logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(string prefix, CancellationToken ct)
        {
            if (IsRunning) return;

            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _listener.Start();

            _logger.Log($"Server started on {prefix}", Models.LogLevel.Info, "Server");

            _ = Task.Run(async () =>
            {
                try
                {
                    while (_listener.IsListening && !ct.IsCancellationRequested)
                    {
                        var context = await _listener.GetContextAsync();
                        _ = ProcessRequestAsync(context); // Fire and forget request processing for concurrency
                    }
                }
                catch (HttpListenerException) when (!IsRunning) { /* Listener stopped */ }
                catch (OperationCanceledException) { /* Task cancelled */ }
                catch (Exception ex)
                {
                    _logger.Log($"Server Loop Error: {ex.Message}", Models.LogLevel.Error, "Server");
                }
            }, ct);

            await Task.CompletedTask;
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                string requestUrl = context.Request.Url?.ToString() ?? "Unknown";
                _logger.Log($"Request received: {requestUrl}", Models.LogLevel.Info, "Server");

                byte[] buffer = Encoding.UTF8.GetBytes("<html><body><h1>WPF HTTP Server Response</h1></body></html>");
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "text/html";
                
                await context.Response.OutputStream.WriteAsync(buffer);
                context.Response.Close();
            }
            catch (Exception ex)
            {
                _logger.Log($"Request processing error: {ex.Message}", Models.LogLevel.Error, "Server");
            }
        }

        public Task StopAsync()
        {
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
                _logger.Log("Server stopped", Models.LogLevel.Info, "Server");
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _listener?.Close();
        }
    }
}
