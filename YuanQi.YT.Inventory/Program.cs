using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TouchSocket.Core;
using TouchSocket.Sockets;
namespace YuanQi.YT.Inventory
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Logging.AddLog4Net(@"Config\log4net.config");
            builder.Services.AddSingletonTcpClient(config =>
            {
                config.SetRemoteIPHost("127.0.0.1:7789");
                config.ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                });
                config.ConfigurePlugins(a =>
                {
                    a.UseReconnection()  //断线重连
                        .SetTick(TimeSpan.FromSeconds(1))
                        .UsePolling();
                    a.Add<InventoryPlugin>();
                });
            });
            builder.Services.AddSingleton<RfidServerClass>();
            builder.Configuration.AddJsonFile("appsettings.json");
            var app = builder.Build();
            app.Services.GetRequiredService<ITcpClient>().Connect();
            app.Services.GetRequiredService<ITcpClient>().Logger.Info("连接成功!");
            app.Services.GetRequiredService<RfidServerClass>();
            app.Run();
        }
    }
}