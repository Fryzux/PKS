using System.Threading.Tasks;

namespace WpfHttpApp.Services
{
    public interface IHttpClientService
    {
        Task<string> GetAsync(string url);
    }
}
