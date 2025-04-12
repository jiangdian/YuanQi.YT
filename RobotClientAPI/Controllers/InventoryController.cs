using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RobotClientAPI.Common;
using System.Text;
using HalconDotNet;
using RobotClientAPI.Vision;
using System.IO.Ports;

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
                        ComClass com = ComClass.GetInstance(_configuration, Parity.None, StopBits.One, Handshake.None);
                        LightClass lightVision = LightClass.GetInstance(_configuration, Parity.None, StopBits.One, Handshake.None);
                        VisionClass vision = VisionClass.GetInstance(_configuration, _logger);
                        _logger.LogInformation("接收到视觉盘点任务");
                        lightVision.OpenLight();
                        InitTaskInventoryBack(taskIn);
                        com.StartRead(out string trayCodeLeft, out string trayCodeRight);
                        vision.GrabImageVision(_configuration["deviceLeft"], _configuration["deviceRight"], trayCodeLeft, trayCodeRight,
                            out HObject? ho_VisionImageLeft, out HObject? ho_VisionImageRight, out string? imageLeftUrl, out string? imageRightUrl);
                        if (ho_VisionImageLeft != null && ho_VisionImageRight != null)
                        {
                            List<Vision> visionResultList = new List<Vision>();
                            vision.GetStorageInformation(ho_VisionImageLeft, ho_VisionImageRight, trayCodeLeft, trayCodeRight,
                                out List<string> materialNoListLeft, out List<string> materialNoListRight, out bool compareResultLeft, out bool compareResultRight);
                            Vision leftResult = new Vision()
                            {
                                storageArea = taskIn.leftStorageCode,
                                materialNoList = materialNoListLeft,
                                compareResult = true,
                                imgUrl = imageLeftUrl,
                                trayNo = trayCodeLeft
                            };
                            visionResultList.Add(leftResult);

                            Vision rightResult = new Vision()
                            {
                                storageArea = taskIn.rightStorageCode,
                                materialNoList = materialNoListRight,
                                compareResult = true,
                                imgUrl = imageRightUrl,
                                trayNo = trayCodeRight
                            };
                            visionResultList.Add(rightResult);
                            //todo:视觉盘点
                            InitTaskInventoryVisionBack(visionResultList, taskIn);//盘点结果填入
                            lightVision.CloseLight();
                        }
                        break;
                    case TaskType.scan://视觉盘
                        VisionClass scan = VisionClass.GetInstance(_configuration, _logger);
                        LightClass lightScan = LightClass.GetInstance(_configuration, Parity.None, StopBits.One, Handshake.None);
                        _logger.LogInformation("接收到视觉扫码任务");
                        lightScan.OpenLightScan();
                        string receive1 = scan.GetCodeNode("169.254.15.1", 2001);
                        _logger.LogInformation($"读码器1结果{receive1}");
                        Thread.Sleep(1500);
                        string receive2 = scan.GetCodeNode("169.254.90.1", 2001);
                        _logger.LogInformation($"读码器2结果{receive2}");
                        List<string> visionResult = new List<string>();
                        List<string> tempList1 = new List<string>();
                        List<string> tempList2 = new List<string>();
                        if ((receive1 != null && receive1 != "" && receive1 != "NoRead") || (receive2 != null && receive2 != "" && receive2 != "NoRead"))
                        {
                            if (receive1 != null && receive1 != "" && receive1 != "NoRead")
                                tempList1 = receive1.Split(";").ToList();
                            if (receive2 != null && receive2 != "" && receive2 != "NoRead")
                                tempList2 = receive2.Split(";").ToList();
                            visionResult = tempList1.Concat(tempList2).ToList();
                            visionResult = visionResult.Distinct().ToList();
                        }
                        string log = string.Join(";", visionResult);
                        _logger.LogInformation($"最终结果为{log}");
                        InitFrontTaskVisionBack(visionResult, taskIn);
                        lightScan.CloseLight();
                        break;
                    case TaskType.record:
                        LightClass lightRecord = LightClass.GetInstance(_configuration, Parity.None, StopBits.One, Handshake.None);
                        _logger.LogInformation("接收到视觉拍照任务");
                        lightRecord.OpenLightRecord();
                        VisionClass record = VisionClass.GetInstance(_configuration, _logger);
                        _logger.LogInformation("开始拍照");
                        record.GrabImageRecord(_configuration["deviceFront"], _configuration["deviceBehind"], taskIn.trayCode, out HObject? ho_Image1, out HObject? ho_Image2);
                        _logger.LogInformation("拍照完成");
                        if (ho_Image1 != null && ho_Image2 != null)
                            //todo:调用视觉拍照反馈
                            InitBehindTaskVisionBack(true, taskIn);
                        else
                            InitBehindTaskVisionBack(false, taskIn);
                        lightRecord.CloseLight();
                        break;
                    case TaskType.stop:
                        RfidClass.Instance.RfidOpen();
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
        _logger.LogInformation($"结束rfid盘点任务，任务ID{_taskInventoryBack.taskId},条码信息:{string.Join(",", strings)}");
        await PostDataToApi(_taskInventoryBack);
    }

    private async void InitTaskInventoryVisionBack(List<Vision> result, TaskIn taskIn)
    {
        _taskInventoryBack = new TaskInventoryBack()
        {
            endTime = DateTime.Now.ToString("yyyy-MMdd HH:mm:ss"),
            visionResult = result,
            robotId = taskIn.robotId,
            taskId = taskIn.taskId
        };
        _logger.LogInformation($"结束视觉盘点任务，任务ID{_taskInventoryBack.taskId}");
        await PostDataToApi(_taskInventoryBack);
    }

    private async void InitFrontTaskVisionBack(List<string> strings, TaskIn taskIn)
    {
        _frontShutterTaskBack = new FrontShutterTaskBack()
        {
            robotId = taskIn.robotId,
            taskId = taskIn.taskId,
            scanInfo = strings,
            trayCode = taskIn.trayCode
        };
        _logger.LogInformation($"结束视觉扫描任务，任务ID{_frontShutterTaskBack.taskId}");
        await PostDataToApi(_frontShutterTaskBack);
    }

    private async void InitBehindTaskVisionBack(bool result, TaskIn taskIn)
    {
        _behindShutterTaskBack = new BehindShutterTaskBack()
        {
            robotId = taskIn.robotId,
            taskId = taskIn.taskId,
            photoSignal = result
        };
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