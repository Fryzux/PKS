using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WpfHttpApp.Services;
using WpfHttpApp.ViewModels;

namespace WpfHttpApp
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // Start logger background worker
            var logger = ServiceProvider.GetRequiredService<ILoggerService>();
            logger.StartAsync();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<ILoggerService, LoggerService>();
            services.AddSingleton<IHttpServerService, HttpServerService>();
            
            // HttpClient with factory-like behavior for socket exhaustion prevention
            services.AddHttpClient<IHttpClientService, HttpClientService>();

            // ViewModels
            services.AddSingleton<MainViewModel>();

            // Views
            services.AddTransient<MainWindow>(s => new MainWindow
            {
                DataContext = s.GetRequiredService<MainViewModel>()
            });
        }
    }
}
