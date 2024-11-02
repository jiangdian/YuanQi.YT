using TouchSocket.Core;
using TouchSocket.Sockets;

namespace YuanQi.YT.InventoryWorker
{
    public class Program
    {
        public static void Main(string[] args)
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
            Func<IServiceProvider, IFreeSql> fsqlFactory = r =>
            {
                var logger = LoggerFactory.Create(builder => builder.AddConsole().AddDebug()).CreateLogger<Program>();
                IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                    .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=SystemDb.db")
                    .UseMonitorCommand(cmd => logger.LogInformation($"Sql：{cmd.CommandText}"))
                    //.UseAutoSyncStructure(true) //自动同步实体结构到数据库，只有CRUD时才会生成表
                    .Build();
                return fsql;
            };
            builder.Services.AddSingleton(fsqlFactory);
            builder.Services.AddSingleton<RfidServerClass>();
            //builder.Configuration.AddJsonFile("appsettings.json");
            builder.Services.AddWindowsService();
            builder.Services.Configure<MyConfig>(builder.Configuration.GetSection("MyConfig"));
            var app = builder.Build();
            app.Services.GetRequiredService<ITcpClient>().Connect();
            app.Services.GetRequiredService<RfidServerClass>();
            app.Run();
        }
    }
}