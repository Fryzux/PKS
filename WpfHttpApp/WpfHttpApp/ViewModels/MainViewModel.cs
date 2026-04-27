using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        [ObservableProperty]
        private string _serverUrl = "http://localhost:8080/";

        [ObservableProperty]
        private string _testUrl = "http://localhost:8080/";

        [ObservableProperty]
        private bool _isServerRunning;

        [ObservableProperty]
        private string _statusText = "Ready";

        public ObservableCollection<LogEntry> Logs { get; } = new();

        public MainViewModel(
            IHttpServerService serverService, 
            IHttpClientService clientService, 
            ILoggerService loggerService)
        {
            _serverService = serverService;
            _clientService = clientService;
            _loggerService = loggerService;

            // Subscribe to logs
            _loggerService.OnLogReceived += (entry) =>
            {
                // UI updates must be on the Dispatcher thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Logs.Insert(0, entry);
                    if (Logs.Count > 100) Logs.RemoveAt(Logs.Count - 1);
                });
            };
        }

        [RelayCommand]
        private async Task ToggleServerAsync()
        {
            if (IsServerRunning)
            {
                _serverCts?.Cancel();
                await _serverService.StopAsync();
                IsServerRunning = false;
                StatusText = "Server Stopped";
            }
            else
            {
                try
                {
                    _serverCts = new CancellationTokenSource();
                    await _serverService.StartAsync(ServerUrl, _serverCts.Token);
                    IsServerRunning = true;
                    StatusText = "Server Running";
                }
                catch (Exception ex)
                {
                    _loggerService.Log($"Start failed: {ex.Message}", LogLevel.Error);
                }
            }
        }

        [RelayCommand]
        private async Task SendRequestAsync()
        {
            StatusText = "Sending Request...";
            var response = await _clientService.GetAsync(TestUrl);
            StatusText = "Request Completed";
        }

        [RelayCommand]
        private void ClearLogs()
        {
            Logs.Clear();
        }
    }
}
