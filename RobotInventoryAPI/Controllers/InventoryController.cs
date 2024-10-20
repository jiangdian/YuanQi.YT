using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

[ApiController]
[Route("[controller]/[action]")]
public class InventoryController : ControllerBase
{
    ILogger<InventoryController> _logger;
    RfidServerClass _serverClass;
    static TaskInventoryBack _taskInventoryBack=new TaskInventoryBack();
    public InventoryController(ILogger<InventoryController> logger, RfidServerClass serverClass)
    {
        _logger = logger;
        _serverClass = serverClass;
    }
    [HttpPost]
    public TaskOut Inventory(TaskIn taskIn)
    {
        TaskOut taskOut = new TaskOut();
        try
        {
            Task.Run(() => {
                switch (taskIn.taskType)
                {
                    case TaskType.rfid:
                        InitTaskInventoryBack(taskIn);
                        _serverClass.ReadRfid();
                        break;
                    case TaskType.vision:
                        InitTaskInventoryBack(taskIn);
                        //todo:视觉盘点
                        //InitTaskInventoryVisionBack(true);//盘点结果填入
                        break;
                    case TaskType.stop:
                        
                        InitTaskInventoryRfidBack(_serverClass.CloseRfid());
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
        _logger.LogInformation("开始{0}盘点任务，任务ID{1}", taskIn.taskType, taskIn.taskId);
    }
    /// <summary>
    /// rfid盘点结束，记录盘点结果
    /// </summary>
    private async void InitTaskInventoryRfidBack(List<string> strings)
    {

        _taskInventoryBack.endTime = DateTime.Now.ToString("yyyy-MMdd HH:mm:ss");
        _taskInventoryBack.rfidResult = strings;
        _logger.LogInformation("结束rfid盘点任务，任务ID{0}", _taskInventoryBack.taskId);
        await PostDataToApi(_taskInventoryBack);
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
                string apiUrl = "http:/192.168.10.150:10086/Inventory/Inventory";//todo:wcs地址
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
