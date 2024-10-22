using InventoryAPI.Common;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace InventoryAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddLog4Net(@"Config\log4net.config");
            builder.Services.AddTcpService(config =>
            {
                config.SetListenIPHosts(7789);
                config.ConfigurePlugins(a =>
                {
                    a.Add<MyPlugin>();
                });
            });
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}