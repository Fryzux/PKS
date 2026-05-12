using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "production.db");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<IProductionCalculationService, ProductionCalculationService>();
builder.Services.AddScoped<IMaterialValidationService, MaterialValidationService>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddHostedService<ProgressUpdateService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
