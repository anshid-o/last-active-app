using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
                            var activeApp = GetLastActiveApplication();
                            var json = JsonSerializer.Serialize(activeApp);
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(json);
                        });
                    });
                });

        private static string GetLastActiveApplication()
        {
            var excludedTitles = new[]
            {
                "C:\\Users\\91964\\source\\repos\\ActiveAppMonitor\\ActiveAppMonitor\\bin\\Debug\\net8.0\\ActiveAppMonitor.exe",
                "ActiveAppMonitor - Microsoft Visual Studio",
                "HelloWorldWebApp - Microsoft Visual Studio",
                "Settings",
                "Home Page - HelloWorldWebApp - Brave",
                "Windows Input Experience",
                "NVIDIA GeForce Overlay",
                "Recording toolbar"
            };

            var activeApps = Process.GetProcesses()
                                    .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle) && !excludedTitles.Contains(p.MainWindowTitle))
                                    .OrderByDescending(p => GetWindowZOrder(p.MainWindowHandle))
                                    .Select(p => p.MainWindowTitle)
                                    .ToArray();

            return activeApps.LastOrDefault(); // Get only the last active application
        }

        private static int GetWindowZOrder(IntPtr hWnd)
        {
            IntPtr hCurrent = hWnd;
            int zOrder = 0;
            while (hCurrent != IntPtr.Zero)
            {
                hCurrent = GetWindow(hCurrent, GW_HWNDPREV);
                zOrder++;
            }
            return zOrder;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        private const uint GW_HWNDPREV = 3;
    }
}
