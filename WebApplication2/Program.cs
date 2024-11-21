using Microsoft.Extensions.DependencyInjection;
namespace WebApplication2
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // 添加默认配置（包括 appsettings.json 和环境变量支持）  
            builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();
            // 配置服务  DI
            builder.Services.Configure<MyConfig>(builder.Configuration.GetSection("MyConfig"));
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddTransient(serviceProvider =>
            {
                // 从服务提供者中解析所需的参数
                var parameter =3;
                // 使用解析的参数创建类的实例
                return new test(parameter);
            });
            builder.Services.AddTransient(serviceProvider =>
                new test(3)
            );

            //添加日志，以下两种方式都可
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.AddLog4Net(@"Config\log4net.config");
            });
            var app = builder.Build();

            // 配置HTTP请求管道
            if (app.Environment.IsDevelopment())//检查当前应用程序是否处于开发环境。
            {
                app.UseSwagger();//在开发环境中启用Swagger中间件，以便在/swagger/v1/swagger.json上提供生成的Swagger JSON文件
                app.UseSwaggerUI();//在开发环境中启用Swagger UI中间件，以便提供一个用户友好的界面来查看和测试API文档。
            }
            app.UseHttpsRedirection();//使用HTTPS重定向中间件，确保所有HTTP请求都被重定向到HTTPS。
            app.UseAuthorization();//使用授权中间件，以便对API请求进行身份验证和授权。
            app.MapControllers();//将MVC控制器路由映射到应用程序的路由表中。这样，当接收到HTTP请求时，应用程序就知道应该调用哪个控制器方法来处理该请求。

            app.Run();
        }
    }
}
