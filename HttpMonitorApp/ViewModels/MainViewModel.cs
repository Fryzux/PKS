using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfHttpApp.Models;
using WpfHttpApp.Services;

namespace WpfHttpApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IHttpServerService _serverService;
        private readonly IHttpClientService _clientService;
        private readonly ILoggerService _loggerService;
        private CancellationTokenSource? _serverCts;
        private readonly DispatcherTimer _uiTimer;

        // ── Server settings ────────────────────────────────────────────────────
        [ObservableProperty]
        private string _port = "8080";

        [ObservableProperty]
        private bool _isServerRunning;

        [ObservableProperty]
        private string _statusText = "Ready";

        // ── Client settings ────────────────────────────────────────────────────
        [ObservableProperty]
        private string _clientUrl = "https://jsonplaceholder.typicode.com/posts";

        [ObservableProperty]
        private string _selectedMethod = "GET";

        [ObservableProperty]
        private string _requestBody = "{\n  \"message\": \"Hello from WPF!\"\n}";

        [ObservableProperty]
        private string _responseText = "";

        [ObservableProperty]
        private bool _isSending;

        // ── Analytics (from server) ────────────────────────────────────────────
        [ObservableProperty]
        private int _getCount;

        [ObservableProperty]
        private int _postCount;

        [ObservableProperty]
        private double _avgProcessingMs;

        [ObservableProperty]
        private string _uptimeText = "--:--:--";

        [ObservableProperty]
        private int _storedMessagesCount;

        // ── Log filter ─────────────────────────────────────────────────────────
        [ObservableProperty]
        private string _filterMethod = "All";

        [ObservableProperty]
        private string _filterStatus = "";

        // ── Collections ────────────────────────────────────────────────────────
        private readonly ObservableCollection<LogEntry> _allLogs = new();

        public ObservableCollection<LogEntry> FilteredLogs { get; } = new();
        public ObservableCollection<MessageEntry> StoredMessages => _serverService.StoredMessages;
        public ObservableCollection<RequestBucket> Buckets => _serverService.Buckets;

        public string[] Methods { get; } = { "GET", "POST" };
        public string[] FilterMethods { get; } = { "All", "GET", "POST" };

        // ── Constructor ────────────────────────────────────────────────────────
        public MainViewModel(
            IHttpServerService serverService,
            IHttpClientService clientService,
            ILoggerService loggerService)
        {
            _serverService = serverService;
            _clientService = clientService;
            _loggerService = loggerService;

            _loggerService.OnLogReceived += OnLog;

            // Refresh analytics every second
            _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _uiTimer.Tick += OnTimerTick;
            _uiTimer.Start();
        }

        private void OnLog(LogEntry entry)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _allLogs.Insert(0, entry);
                if (_allLogs.Count > 500) _allLogs.RemoveAt(_allLogs.Count - 1);
                ApplyFilter();
            });
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            GetCount = _serverService.GetCount;
            PostCount = _serverService.PostCount;
            AvgProcessingMs = Math.Round(_serverService.AvgProcessingMs, 2);
            StoredMessagesCount = _serverService.StoredMessages.Count;

            if (_serverService.StartedAt.HasValue)
                UptimeText = (DateTime.Now - _serverService.StartedAt.Value).ToString(@"hh\:mm\:ss");
            else
                UptimeText = "--:--:--";
        }

        partial void OnFilterMethodChanged(string value) => ApplyFilter();
        partial void OnFilterStatusChanged(string value) => ApplyFilter();

        private void ApplyFilter()
        {
            FilteredLogs.Clear();
            var filtered = _allLogs.AsEnumerable();

            if (FilterMethod != "All")
                filtered = filtered.Where(l =>
                    l.Method.Equals(FilterMethod, StringComparison.OrdinalIgnoreCase));

            if (int.TryParse(FilterStatus, out int code) && code > 0)
                filtered = filtered.Where(l => l.StatusCode == code);

            foreach (var e in filtered.Take(200))
                FilteredLogs.Add(e);
        }

        // ── Commands ───────────────────────────────────────────────────────────
        [RelayCommand]
        private async Task ToggleServerAsync()
        {
            if (IsServerRunning)
            {
                _serverCts?.Cancel();
                await _serverService.StopAsync();
                IsServerRunning = false;
                StatusText = "Server stopped";
            }
            else
            {
                if (!int.TryParse(Port, out int port) || port < 1 || port > 65535)
                {
                    StatusText = "Invalid port number";
                    return;
                }
                try
                {
                    _serverCts = new CancellationTokenSource();
                    string prefix = $"http://localhost:{port}/";
                    await _serverService.StartAsync(prefix, _serverCts.Token);
                    IsServerRunning = true;
                    StatusText = $"Server running on port {port}";
                    ClientUrl = $"http://localhost:{port}/status";
                }
                catch (Exception ex)
                {
                    _loggerService.Log($"Start failed: {ex.Message}", LogLevel.Error);
                    StatusText = $"Error: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        private async Task SendRequestAsync()
        {
            if (IsSending) return;
            IsSending = true;
            StatusText = $"Sending {SelectedMethod} request…";
            ResponseText = "Sending…";

            try
            {
                string result;
                if (SelectedMethod == "POST")
                    result = await _clientService.PostAsync(ClientUrl, RequestBody);
                else
                    result = await _clientService.GetAsync(ClientUrl);

                ResponseText = result;
                StatusText = "Request completed";
            }
            catch (Exception ex)
            {
                ResponseText = $"Error: {ex.Message}";
                StatusText = "Request failed";
            }
            finally
            {
                IsSending = false;
            }
        }

        [RelayCommand]
        private void ClearLogs()
        {
            _allLogs.Clear();
            FilteredLogs.Clear();
        }

        [RelayCommand]
        private void SaveLogs()
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                foreach (var e in _allLogs.Reverse())
                    sb.AppendLine($"[{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{e.Level,-7}] [{e.Method,-4}] [{e.Source,-6}] {e.Message}");

                System.IO.File.AppendAllText("logs_export.txt", sb.ToString());
                StatusText = "Logs saved to logs_export.txt";
                _loggerService.Log("Logs exported to logs_export.txt", LogLevel.Info, "System");
            }
            catch (Exception ex)
            {
                StatusText = $"Save failed: {ex.Message}";
            }
        }
    }
}
