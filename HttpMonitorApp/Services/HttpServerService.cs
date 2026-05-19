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

        public ObservableCollection<MessageEntry> StoredMessages { get; } = new();
        public ObservableCollection<RequestBucket> Buckets { get; } = new();

        public HttpServerService(ILoggerService logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(string prefix, CancellationToken ct)
        {
            if (IsRunning) return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(prefix);
                _listener.Start();
                StartedAt = DateTime.Now;

                _logger.Log($"Server started on {prefix}", LogLevel.Info, "Server");

                Application.Current?.Dispatcher.BeginInvoke(new Action(SeedBuckets));

                _ = Task.Run(async () =>
                {
                    try
                    {
                        while (_listener is { IsListening: true } && !ct.IsCancellationRequested)
                        {
                            var context = await _listener.GetContextAsync();
                            _ = ProcessRequestAsync(context);
                        }
                    }
                    catch (HttpListenerException ex) when (!IsRunning) 
                    {
                        _logger.Log("Listener stopped normally.", LogLevel.Info, "Server");
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Server loop crashed: {ex.Message}", LogLevel.Error, "Server");
                        StartedAt = null;
                    }
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.Log($"Critical start error: {ex.Message}", LogLevel.Error, "Server");
                throw;
            }

            await Task.CompletedTask;
        }

        public Task StopAsync()
        {
            if (_listener is { IsListening: true })
            {
                try
                {
                    _listener.Stop();
                    _listener.Close();
                    _logger.Log("Server stopped", LogLevel.Info, "Server");
                }
                catch (Exception ex)
                {
                    _logger.Log($"Stop error: {ex.Message}", LogLevel.Error, "Server");
                }
                finally
                {
                    StartedAt = null;
                }
            }
            return Task.CompletedTask;
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var sw = Stopwatch.StartNew();
            var req = context.Request;
            var resp = context.Response;
            string method = req.HttpMethod.ToUpper();
            string path = req.Url?.AbsolutePath ?? "/";
            int statusCode = 200;
            string responseBody = "";

            try
            {
                string requestBody = "";
                if (req.HasEntityBody)
                {
                    using var reader = new StreamReader(req.InputStream, req.ContentEncoding ?? Encoding.UTF8);
                    requestBody = await reader.ReadToEndAsync();
                }

                _logger.Log($"► {method} {path} | Body: {(requestBody.Length > 100 ? requestBody[..100] + "…" : requestBody)}",
                    LogLevel.Info, "Server", method);

                // Validation for ALL POST requests
                if (method == "POST")
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(requestBody);
                        if (path == "/messages")
                        {
                            if (!doc.RootElement.TryGetProperty("message", out var msgProp))
                                throw new Exception("Missing 'message' field");
                            
                            string msg = msgProp.GetString() ?? "";
                            var entry = new MessageEntry { Message = msg };
                            Application.Current?.Dispatcher.BeginInvoke(() => StoredMessages.Insert(0, entry));
                            
                            responseBody = JsonSerializer.Serialize(new { id = entry.Id, status = "received" });
                            Interlocked.Increment(ref _postCount);
                        }
                        else
                        {
                            statusCode = 404;
                            responseBody = JsonSerializer.Serialize(new { error = "Endpoint not found. Use /messages" });
                        }
                    }
                    catch (Exception ex)
                    {
                        statusCode = 400;
                        responseBody = JsonSerializer.Serialize(new { error = "Invalid JSON", details = ex.Message });
                        _logger.Log($"Bad Request: {ex.Message}", LogLevel.Warning, "Server", method, 400);
                    }
                }
                else if (method == "GET")
                {
                    if (path == "/" || path == "/status")
                    {
                        Interlocked.Increment(ref _getCount);
                        responseBody = JsonSerializer.Serialize(new { status = "running", get = _getCount, post = _postCount });
                    }
                    else
                    {
                        statusCode = 404;
                        responseBody = JsonSerializer.Serialize(new { error = "Not found" });
                    }
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseBody);
                resp.StatusCode = statusCode;
                resp.ContentType = "application/json";
                resp.ContentLength64 = buffer.Length;
                resp.AddHeader("Access-Control-Allow-Origin", "*");
                
                await resp.OutputStream.WriteAsync(buffer);
                resp.Close();

                sw.Stop();
                TrackStats(method, sw.Elapsed, statusCode);
                TrackBucket();
            }
            catch (Exception ex)
            {
                _logger.Log($"Request error: {ex.Message}", LogLevel.Error, "Server");
                try { resp.Abort(); } catch { }
            }
        }

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
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                for (int i = 0; i < Buckets.Count; i++)
                {
                    if (Buckets[i].Time == key) { Buckets[i].Count++; return; }
                }
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
                var t = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(-i);
                Buckets.Add(new RequestBucket { Time = t, Count = 0 });
            }
        }

        public void Dispose()
        {
            _listener?.Close();
        }
    }
}
