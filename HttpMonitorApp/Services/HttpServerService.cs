using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfHttpApp.Models;

namespace WpfHttpApp.Services
{
    public class HttpServerService : IHttpServerService, IDisposable
    {
        private HttpListener? _listener;
        private readonly ILoggerService _logger;
        private readonly object _statsLock = new();

        // ── stats ──────────────────────────────────────────────────────────────
        private int _getCount;
        private int _postCount;
        private long _totalProcessingTicks;
        private int _processedCount;

        public int GetCount => _getCount;
        public int PostCount => _postCount;
        public double AvgProcessingMs => _processedCount == 0
            ? 0
            : TimeSpan.FromTicks(_totalProcessingTicks / _processedCount).TotalMilliseconds;

        public DateTime? StartedAt { get; private set; }
        public bool IsRunning => _listener?.IsListening ?? false;

        // ── collections ────────────────────────────────────────────────────────
        public ObservableCollection<MessageEntry> StoredMessages { get; } = new();
        public ObservableCollection<RequestBucket> Buckets { get; } = new();

        public HttpServerService(ILoggerService logger)
        {
            _logger = logger;
        }

        // ── start / stop ───────────────────────────────────────────────────────
        public async Task StartAsync(string prefix, CancellationToken ct)
        {
            if (IsRunning) return;

            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            StartedAt = DateTime.Now;

            _logger.Log($"Server started on {prefix}", LogLevel.Info, "Server");

            // Seed buckets for last 10 minutes
            Application.Current.Dispatcher.Invoke(SeedBuckets);

            _ = Task.Run(async () =>
            {
                try
                {
                    while (_listener.IsListening && !ct.IsCancellationRequested)
                    {
                        var context = await _listener.GetContextAsync();
                        _ = ProcessRequestAsync(context);  // fire-and-forget = multi-threaded
                    }
                }
                catch (HttpListenerException) when (!IsRunning) { }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.Log($"Server loop error: {ex.Message}", LogLevel.Error, "Server");
                }
            }, ct);

            await Task.CompletedTask;
        }

        public Task StopAsync()
        {
            if (_listener is { IsListening: true })
            {
                _listener.Stop();
                _listener.Close();
                StartedAt = null;
                _logger.Log("Server stopped", LogLevel.Info, "Server");
            }
            return Task.CompletedTask;
        }

        // ── request processing ─────────────────────────────────────────────────
        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var sw = Stopwatch.StartNew();
            var req = context.Request;
            var resp = context.Response;
            string method = req.HttpMethod.ToUpper();
            string path = req.Url?.AbsolutePath ?? "/";
            int statusCode = 200;
            string responseBody;

            try
            {
                // Read body (for POST)
                string requestBody = "";
                if (req.HasEntityBody)
                {
                    using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
                    requestBody = await reader.ReadToEndAsync();
                }

                // Log headers
                var headers = new StringBuilder();
                foreach (string key in req.Headers.Keys)
                    headers.Append($"{key}: {req.Headers[key]}; ");

                _logger.Log(
                    $"► {method} {path} | Headers: {headers} | Body: {(requestBody.Length > 200 ? requestBody[..200] + "…" : requestBody)}",
                    LogLevel.Info, "Server", method, 0);

                // ── Route ──────────────────────────────────────────────────────
                if (method == "GET" && (path == "/" || path == "/status"))
                {
                    Interlocked.Increment(ref _getCount);
                    var uptime = StartedAt.HasValue
                        ? (DateTime.Now - StartedAt.Value).ToString(@"hh\:mm\:ss")
                        : "00:00:00";

                    responseBody = JsonSerializer.Serialize(new
                    {
                        status = "running",
                        uptime,
                        getRequests = _getCount,
                        postRequests = _postCount,
                        totalRequests = _getCount + _postCount,
                        avgProcessingMs = Math.Round(AvgProcessingMs, 2),
                        storedMessages = StoredMessages.Count
                    });
                    resp.ContentType = "application/json";
                }
                else if (method == "POST" && path == "/messages")
                {
                    Interlocked.Increment(ref _postCount);

                    MessageEntry? entry = null;
                    try
                    {
                        var doc = JsonDocument.Parse(requestBody);
                        string msg = doc.RootElement.GetProperty("message").GetString() ?? "";
                        entry = new MessageEntry { Message = msg };
                    }
                    catch
                    {
                        statusCode = 400;
                        responseBody = JsonSerializer.Serialize(new { error = "Invalid JSON or missing 'message' field" });
                        resp.ContentType = "application/json";
                        goto WriteResponse;
                    }

                    Application.Current.Dispatcher.Invoke(() => StoredMessages.Insert(0, entry));
                    responseBody = JsonSerializer.Serialize(new { id = entry.Id, message = entry.Message, receivedAt = entry.ReceivedAt });
                    resp.ContentType = "application/json";
                }
                else
                {
                    statusCode = 404;
                    responseBody = JsonSerializer.Serialize(new
                    {
                        error = "Not found. Available: GET /status, POST /messages"
                    });
                    resp.ContentType = "application/json";
                }

                WriteResponse:
                sw.Stop();
                TrackStats(method, sw.Elapsed, statusCode);
                TrackBucket();

                byte[] buffer = Encoding.UTF8.GetBytes(responseBody);
                resp.StatusCode = statusCode;
                resp.ContentLength64 = buffer.Length;
                resp.AddHeader("Access-Control-Allow-Origin", "*");
                await resp.OutputStream.WriteAsync(buffer);
                resp.Close();

                _logger.Log(
                    $"◄ {method} {path} → {statusCode} ({sw.ElapsedMilliseconds} ms)",
                    statusCode >= 400 ? LogLevel.Warning : LogLevel.Info, "Server", method, statusCode, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.Log($"Request processing error: {ex.Message}", LogLevel.Error, "Server");
                try { resp.StatusCode = 500; resp.Close(); } catch { }
            }
        }

        // ── helpers ────────────────────────────────────────────────────────────
        private void TrackStats(string method, TimeSpan elapsed, int status)
        {
            lock (_statsLock)
            {
                _totalProcessingTicks += elapsed.Ticks;
                _processedCount++;
            }
        }

        private void TrackBucket()
        {
            var now = DateTime.Now;
            var key = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Find existing bucket
                for (int i = 0; i < Buckets.Count; i++)
                {
                    if (Buckets[i].Time == key) { Buckets[i].Count++; return; }
                }
                // New bucket – add and trim to 15 entries
                Buckets.Add(new RequestBucket { Time = key, Count = 1 });
                while (Buckets.Count > 15) Buckets.RemoveAt(0);
            });
        }

        private void SeedBuckets()
        {
            Buckets.Clear();
            var now = DateTime.Now;
            for (int i = 9; i >= 0; i--)
            {
                var t = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0)
                    .AddMinutes(-i);
                Buckets.Add(new RequestBucket { Time = t, Count = 0 });
            }
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _listener?.Close();
        }
    }
}
