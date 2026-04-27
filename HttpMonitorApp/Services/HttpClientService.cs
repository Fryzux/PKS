using System;
using System.Net.Http;
using System.Text;
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
                _logger.Log($"► GET {url}", Models.LogLevel.Info, "Client", "GET");
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(url);
                sw.Stop();
                string body = await response.Content.ReadAsStringAsync();
                int status = (int)response.StatusCode;
                _logger.Log($"◄ GET {url} → {status} ({sw.ElapsedMilliseconds} ms, {body.Length} bytes)",
                    status >= 400 ? Models.LogLevel.Warning : Models.LogLevel.Info, "Client", "GET", status, sw.ElapsedMilliseconds);
                return body;
            }
            catch (Exception ex)
            {
                _logger.Log($"Client GET error: {ex.Message}", Models.LogLevel.Error, "Client", "GET");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> PostAsync(string url, string jsonBody)
        {
            try
            {
                _logger.Log($"► POST {url} | Body: {(jsonBody.Length > 200 ? jsonBody[..200] + "…" : jsonBody)}",
                    Models.LogLevel.Info, "Client", "POST");
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                sw.Stop();
                string body = await response.Content.ReadAsStringAsync();
                int status = (int)response.StatusCode;
                _logger.Log($"◄ POST {url} → {status} ({sw.ElapsedMilliseconds} ms, {body.Length} bytes)",
                    status >= 400 ? Models.LogLevel.Warning : Models.LogLevel.Info, "Client", "POST", status, sw.ElapsedMilliseconds);
                return body;
            }
            catch (Exception ex)
            {
                _logger.Log($"Client POST error: {ex.Message}", Models.LogLevel.Error, "Client", "POST");
                return $"Error: {ex.Message}";
            }
        }
    }
}
