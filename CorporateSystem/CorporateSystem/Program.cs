using CorporateSystem.Client.Pages;
using CorporateSystem.Components;
using CorporateSystem.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// EF Core с SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=corporate.db"));

// Регистрация сервисов для пре-рендеринга
builder.Services.AddScoped<CorporateSystem.Client.Services.ThemeService>();
builder.Services.AddScoped<CorporateSystem.Client.Services.ProductHttpService>();

// Регистрация HttpClient (сервер может обращаться сам к себе для пре-рендеринга)
builder.Services.AddScoped(sp => 
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

// REST API контроллеры
builder.Services.AddControllers();

var app = builder.Build();

// Авто-миграция БД при старте
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

// API endpoints
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CorporateSystem.Client._Imports).Assembly);

app.Run();

// Для доступа из тестов
public partial class Program { }
