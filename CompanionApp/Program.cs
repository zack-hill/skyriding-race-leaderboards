using CompanionApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CompanionApp;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSerilog((_, configuration) =>
                    configuration.ReadFrom.Configuration(hostContext.Configuration));
                services.AddHttpClient();
                services.AddSingleton<GamePathService>();
                services.AddSingleton<TrayIconService>();
                services.AddSingleton<AddonDataService>();
                services.AddHostedService<Worker>();
            });
}