using InventoryAPI.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TouchSocket.Sockets;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class InventoryController : ControllerBase
    {
        ITcpService tcpService;
        ILogger<InventoryController> _logger;
        public InventoryController(ITcpService tcpService, ILogger<InventoryController> logger)
        {
            this.tcpService=tcpService;
            _logger = logger;
        }
        [HttpPost]
        public TaskOut Inventory(TaskIn taskIn)
        {
            TaskOut taskOut = new TaskOut();
            Task.Run(() => {
                Task.Delay(10000);
            });
            try
            {
                var ids = tcpService.GetIds();
                if (ids.Count()==0)
                {
                    throw new Exception();
                }
                var mes = JsonConvert.SerializeObject(taskIn);
                foreach (var clientId in ids)
                {
                    tcpService.Send(clientId, mes);
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
    }
}
