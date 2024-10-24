using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RobotClientAPI.Common;
using System.Text;

[ApiController]
[Route("[controller]/[action]")]
public class InventoryController : ControllerBase
{
    ILogger<InventoryController> _logger;
    static TaskInventoryBack _taskInventoryBack = new TaskInventoryBack();
    static FrontShutterTaskBack _frontShutterTaskBack = new FrontShutterTaskBack();
    static BehindShutterTaskBack _behindShutterTaskBack = new BehindShutterTaskBack();
    IConfiguration _configuration;
    public InventoryController(ILogger<InventoryController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    [HttpPost]
    public TaskOut Inventory(TaskIn taskIn)
    {
        TaskOut taskOut = new TaskOut();
        try
        {
            Task.Run(() =>
            {
                switch (taskIn.taskType)
                {
                    case TaskType.rfid:
                        RfidClass.Instance.RfidOpen();
                        InitTaskInventoryBack(taskIn);
                        RfidClass.Instance.ReadRfid();
                        break;
                    case TaskType.vision://视觉盘点
                        _logger.LogInformation("接收到视觉盘点任务");
                        InitTaskInventoryBack(taskIn);
                        //todo:视觉盘点
                        InitTaskInventoryVisionBack(new List<Vision>());//盘点结果填入
                        break;
                    case TaskType.scan://视觉盘
                        _logger.LogInformation("接收到视觉扫码任务");
                        InitFrontTaskVisionBack(new List<string>());
                        break;
                    case TaskType.record:
                        _logger.LogInformation("接收到视觉拍照任务");
                        VisionClass.Instance.GrabImage();
                        //todo:调用视觉拍照反馈
                        InitBehindTaskVisionBack(true);
                        break;
                    case TaskType.stop:
                        InitTaskInventoryRfidBack(RfidClass.Instance.CloseRfid());
                        break;
                    default:
                        break;
                }
            });
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
    /// 开始盘点时，记录任务信息
    /// </summary>
    /// <param name="taskid"></param>
    private void InitTaskInventoryBack(TaskIn taskIn)
    {
        _taskInventoryBack = new TaskInventoryBack()
        {
            robotId = taskIn.robotId,
            taskId = taskIn.taskId,
            startTime = DateTime.Now.ToString("yyyy-MMdd HH:mm:ss")
        };
        _logger.LogInformation($"开始{taskIn.taskType}盘点任务，任务ID{taskIn.taskId}");
    }
    /// <summary>
    /// rfid盘点结束，记录盘点结果
    /// </summary>
    private async void InitTaskInventoryRfidBack(List<string> strings)
    {
        _taskInventoryBack.endTime = DateTime.Now.ToString("yyyy-MMdd HH:mm:ss");
        _taskInventoryBack.rfidResult = strings;
        _logger.LogInformation($"结束rfid盘点任务，任务ID{_taskInventoryBack.taskId}");
        await PostDataToApi(_taskInventoryBack);
    }

    private async void InitTaskInventoryVisionBack(List<Vision> result)
    {
        _taskInventoryBack.endTime = DateTime.Now.ToString("yyyy-MMdd HH:mm:ss");
        _taskInventoryBack.visionResult = result;
        _logger.LogInformation($"结束视觉盘点任务，任务ID{_taskInventoryBack.taskId}");
        await PostDataToApi(_taskInventoryBack);
    }

    private async void InitFrontTaskVisionBack(List<string> strings)
    {
        _frontShutterTaskBack.scanInfo = strings;
        _logger.LogInformation($"结束视觉扫描任务，任务ID{_frontShutterTaskBack.taskId}");
        await PostDataToApi(_frontShutterTaskBack);
    }

    private async void InitBehindTaskVisionBack(bool result)
    {
        _behindShutterTaskBack.photoSignal = result;
        _logger.LogInformation($"结束视觉拍照任务，任务ID{_behindShutterTaskBack.taskId}");
        await PostDataToApi(_behindShutterTaskBack);
    }
    /// <summary>
    /// 调用wcs任务反馈接口
    /// </summary>
    /// <param name="taskIn"></param>
    /// <returns></returns>
    private async Task<string> PostDataToApi(TaskInventoryBack taskIn)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string apiUrl = _configuration["InventoryUrl"].ToString();
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

    /// <summary>
    /// 调用wcs任务反馈接口
    /// </summary>
    /// <param name="taskIn"></param>
    /// <returns></returns>
    private async Task<string> PostDataToApi(FrontShutterTaskBack taskIn)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string apiUrl = _configuration["ScanUrl"].ToString();
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

    /// <summary>
    /// 调用wcs任务反馈接口
    /// </summary>
    /// <param name="taskIn"></param>
    /// <returns></returns>
    private async Task<string> PostDataToApi(BehindShutterTaskBack taskIn)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string apiUrl = _configuration["PhotoUrl"].ToString();
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