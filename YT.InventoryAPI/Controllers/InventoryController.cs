using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

[ApiController]
[Route("[controller]/[action]")]
public class InventoryController : ControllerBase
{
    ILogger<InventoryController> _logger;

    public InventoryController(ILogger<InventoryController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public TaskOut Inventory(TaskIn taskIn)
    {
        TaskOut taskOut = new TaskOut();
        try
        {
            switch (taskIn.taskType)
            {
                case TaskType.scan:
                case TaskType.record:
                    PostDataToApi(taskIn, UrlType.UrlFour);
                    break;
                case TaskType.rfid:
                case TaskType.stop:
                case TaskType.vision:
                    switch (taskIn.robotId)
                    {
                        case RobotType.robotOne:
                            PostDataToApi(taskIn, UrlType.UrlOne);
                            break;
                        case RobotType.robotTwo:
                            PostDataToApi(taskIn, UrlType.UrlTwo);
                            break;
                        case RobotType.robotThree:
                            PostDataToApi(taskIn, UrlType.UrlThree);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            taskOut.status = 200;
            taskOut.msg = "任务发送成功";
            _logger.LogInformation($"任务ID{taskIn.taskId}下发成功!");
            return taskOut;
        }
        catch (Exception)
        {
            taskOut.status = 500;
            taskOut.msg = "任务发送失败";
            _logger.LogInformation($"任务ID{taskIn.taskId}下发失败!");
            return taskOut;
        }
    }

    /// <summary>
    /// 调用盘点接口
    /// </summary>
    /// <param name="taskIn"></param>
    /// <returns></returns>
    private async Task<string> PostDataToApi(TaskIn taskIn, string sUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string apiUrl = sUrl;
                var jsonString = JsonConvert.SerializeObject(taskIn);
                HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"Error calling API: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return $"Error: {ex.Message}";
            }
        }
    }
}