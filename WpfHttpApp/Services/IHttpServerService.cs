using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using WpfHttpApp.Models;

namespace WpfHttpApp.Services
{
    public interface IHttpServerService
    {
        bool IsRunning { get; }

        int GetCount { get; }
        int PostCount { get; }
        double AvgProcessingMs { get; }
        System.DateTime? StartedAt { get; }

        ObservableCollection<MessageEntry> StoredMessages { get; }
        ObservableCollection<RequestBucket> Buckets { get; }

        Task StartAsync(string prefix, CancellationToken ct);
        Task StopAsync();
    }
}
