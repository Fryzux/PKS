using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CorporateSystem.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Регистрация HttpClient с базовым адресом сервера
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Регистрация сервисов
builder.Services.AddScoped<ProductHttpService>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
