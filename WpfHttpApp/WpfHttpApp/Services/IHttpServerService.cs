using System.Threading;
using System.Threading.Tasks;

namespace WpfHttpApp.Services
{
    public interface IHttpServerService
    {
        bool IsRunning { get; }
        Task StartAsync(string prefix, CancellationToken ct);
        Task StopAsync();
    }
}
