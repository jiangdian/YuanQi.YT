using Microsoft.Extensions.DependencyInjection;
namespace WebApplication2
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // ���Ĭ�����ã����� appsettings.json �ͻ�������֧�֣�  
            builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();
            // ���÷���  DI
            builder.Services.Configure<MyConfig>(builder.Configuration.GetSection("MyConfig"));
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddTransient(serviceProvider =>
            {
                // �ӷ����ṩ���н�������Ĳ���
                var parameter =3;
                // ʹ�ý����Ĳ����������ʵ��
                return new test(parameter);
            });
            builder.Services.AddTransient(serviceProvider =>
                new test(3)
            );

            //�����־���������ַ�ʽ����
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.AddLog4Net(@"Config\log4net.config");
            });
            var app = builder.Build();

            // ����HTTP����ܵ�
            if (app.Environment.IsDevelopment())//��鵱ǰӦ�ó����Ƿ��ڿ���������
            {
                app.UseSwagger();//�ڿ�������������Swagger�м�����Ա���/swagger/v1/swagger.json���ṩ���ɵ�Swagger JSON�ļ�
                app.UseSwaggerUI();//�ڿ�������������Swagger UI�м�����Ա��ṩһ���û��ѺõĽ������鿴�Ͳ���API�ĵ���
            }
            app.UseHttpsRedirection();//ʹ��HTTPS�ض����м����ȷ������HTTP���󶼱��ض���HTTPS��
            app.UseAuthorization();//ʹ����Ȩ�м�����Ա��API������������֤����Ȩ��
            app.MapControllers();//��MVC������·��ӳ�䵽Ӧ�ó����·�ɱ��С������������յ�HTTP����ʱ��Ӧ�ó����֪��Ӧ�õ����ĸ����������������������

            app.Run();
        }
    }
}
