using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WpfHttpApp.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggerService _logger;

        public HttpClientService(HttpClient httpClient, ILoggerService logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetAsync(string url)
        {
            try
            {
                _logger.Log($"Sending GET request to {url}", Models.LogLevel.Info, "Client");
                var response = await _httpClient.GetStringAsync(url);
                _logger.Log($"Resource fetched successfully ({response.Length} bytes)", Models.LogLevel.Info, "Client");
                return response;
            }
            catch (Exception ex)
            {
                _logger.Log($"Client Error: {ex.Message}", Models.LogLevel.Error, "Client");
                return $"Error: {ex.Message}";
            }
        }
    }
}
