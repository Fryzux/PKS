using System.Threading.Tasks;

namespace WpfHttpApp.Services
{
    public interface IHttpClientService
    {
        Task<string> GetAsync(string url);
        Task<string> PostAsync(string url, string jsonBody);
    }
}
