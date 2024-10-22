using Newtonsoft.Json;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace YuanQi.YT.InventoryWorker
{
    class InventoryPlugin : PluginBase, ITcpReceivedPlugin<TcpClient>
    {
        ILogger<InventoryPlugin> _logger;
        long robotId;
        RfidServerClass _serverClass;
        TaskInventoryBack _taskInventoryBack;
        public IFreeSql freeSql;
        public InventoryPlugin(ILogger<InventoryPlugin> logger, IConfiguration configuration, RfidServerClass serverClass, IFreeSql freeSql)
        {
            _logger = logger;
            robotId = Convert.ToInt64(configuration["robotId"]);
            _serverClass = serverClass;
            this.freeSql = freeSql;
        }
        public Task OnTcpReceived(TcpClient client, ReceivedDataEventArgs e)
        {
            var taskin = JsonConvert.DeserializeObject<TaskIn>(e.ByteBlock.ToString());
            if (taskin?.robotId == robotId)
            {

                switch (taskin.taskType)
                {
                    case TaskType.rfid:
                        InitTaskInventoryBack(taskin);
                        _serverClass.ReadRfid();
                        //        var values = freeSql.Select<ComSet>()
                        //.Where(x => x.ComType != null)
                        //.ToList();
                        break;
                    case TaskType.vision:
                        InitTaskInventoryBack(taskin);
                        //todo:视觉盘点
                        InitTaskInventoryVisionBack(true);//盘点结果填入
                        break;
                    case TaskType.stop:
                        _serverClass.CloseRfid();
                        InitTaskInventoryRfidBack();
                        break;
                    default:
                        break;
                }
            }
            return Task.CompletedTask;
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
        private async void InitTaskInventoryRfidBack()
        {
            _taskInventoryBack.endTime = DateTime.Now.ToString("yyyy-MMdd HH:mm:ss");
            _taskInventoryBack.rfidResult = _serverClass.recevid.ToList();
            _logger.LogInformation("结束rfid盘点任务，任务ID{0}", _taskInventoryBack.taskId);
            await PostDataToApi(_taskInventoryBack);
        }
        /// <summary>
        /// 视觉盘点结束，记录盘点结果
        /// </summary>
        /// <param name="result">视觉盘点结果</param>
        private async void InitTaskInventoryVisionBack(bool result)
        {
            _taskInventoryBack.endTime = DateTime.Now.ToString("yyyy-MMdd HH:mm:ss");
            _taskInventoryBack.visionResult = result;
            _logger.LogInformation("结束vision盘点任务，任务ID{0}", _taskInventoryBack.taskId);
            await PostDataToApi(_taskInventoryBack);
        }
        /// <summary>
        /// 调用wcs任务反馈接口
        /// </summary>
        /// <param name="taskIn"></param>
        /// <returns></returns>
        public async Task<string> PostDataToApi(TaskInventoryBack taskIn)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiUrl = "http://localhost:5000/Inventory/Inventory";//todo:wcs地址
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
}