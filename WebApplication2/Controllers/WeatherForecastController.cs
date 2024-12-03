using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Drawing;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        test tt;
        private readonly IOptionsSnapshot<MyConfig> _mySettings;
        public WeatherForecastController(ILogger<WeatherForecastController> logger,test t, IOptionsSnapshot<MyConfig> options)
        {
            _mySettings = options;
            _logger = logger;
            tt = t;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        //[HttpGet(Name = "GetWeatherForecast1")]
        //public IEnumerable<WeatherForecast> Get1()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}
        [HttpGet(Name = "Gettooo")]
        public int Gett()
        {
            return _mySettings.Value.set2;
        }
        [HttpPost]
        public async Task<IActionResult> SaveP()
        {
            //var image= Image.FromFile("03.jpg");
            //image.Save(@"\\192.168.1.100\1\04.jpg");
            // ����Դ�ļ���·��  
            var sourcePath = @"D:\03.jpg";

            // ����Ŀ���ļ���·��  
            var targetPath = @"\\192.168.10.150\picture\record\04.jpg";

            // ���Դ�ļ��Ƿ����  
            if (!System.IO.File.Exists(sourcePath))
            {
                return NotFound(); // ���Դ�ļ������ڣ�����404  
            }

            // ��ȡ�ļ����������ļ�  
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            {
                using (var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }
            }

            return Ok($"Image saved to {targetPath}"); // ���سɹ���Ϣ  
        }
    }
}
