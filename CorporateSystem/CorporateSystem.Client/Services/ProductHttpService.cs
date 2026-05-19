using System.Net.Http.Json;
using CorporateSystem.Shared.Models;

namespace CorporateSystem.Client.Services;

public class ProductHttpService
{
    private readonly HttpClient _http;

    public ProductHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Product>>("api/products");
        return result ?? new List<Product>();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Product>($"api/products/{id}");
    }

    public async Task<Product?> CreateAsync(Product product)
    {
        var response = await _http.PostAsJsonAsync("api/products", product);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>();
    }

    public async Task UpdateAsync(Product product)
    {
        var response = await _http.PutAsJsonAsync($"api/products/{product.Id}", product);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/products/{id}");
        response.EnsureSuccessStatusCode();
    }
}
