using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ActiveAppMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddCors(options =>
                        {
                            options.AddPolicy("AllowAllOrigins",
                                builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
                        });
                    })
                    .Configure(app =>
                    {
                        app.UseCors("AllowAllOrigins");
                        app.Run(async context =>
                        {
                            var activeApps = GetActiveApplications();
                            var json = JsonSerializer.Serialize(activeApps);
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(json);
                        });
                    });
                });

        private static string GetActiveApplications()
        {
            var excludedTitles = new[]
    {
        "C:\\Users\\91964\\source\\repos\\ActiveAppMonitor\\ActiveAppMonitor\\bin\\Debug\\net8.0\\ActiveAppMonitor.exe",
        "ActiveAppMonitor - Microsoft Visual Studio",
        "HelloWorldWebApp - Microsoft Visual Studio",
        "Settings",
        "Home Page - HelloWorldWebApp - Brave",
        "Settings"
    };

            var firstActiveApp = Process.GetProcesses()
                                        .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle) && !excludedTitles.Contains(p.MainWindowTitle))
                                        .OrderByDescending(p => p.StartTime)
                                        .Select(p => p.MainWindowTitle)
                                        .FirstOrDefault();

            return firstActiveApp;

        }




    }
}
