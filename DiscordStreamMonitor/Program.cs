using Discord;
using Discord.WebSocket;
using DiscordStreamMonitor.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.Metrics;

namespace DiscordStreamMonitor
{
    internal class Program
    {
        
        public static async Task Main()
        {
            var connectionString= Environment.GetEnvironmentVariable("ConnectionString");
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<MonitorContext>(options =>
                    {
                        options.UseMySql(connectionString,ServerVersion.AutoDetect(connectionString));
                    });
                    services.AddSingleton<DiscordMonitor>();
                })
                .UseDefaultServiceProvider(options =>
                {
                    options.ValidateScopes = false;
                })
                .Build();
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MonitorContext>();
                db.Database.Migrate();
            }
            
            await host.Services.GetRequiredService<DiscordMonitor>().StartAsync();
            await host.RunAsync();
        }

        

    }
}