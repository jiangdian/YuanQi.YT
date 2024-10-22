namespace YuanQi.YT.InventoryWorker
{
    public class TaskIn
    {
        /// <summary>
        /// 机器人id
        /// </summary>
        public long robotId { get; set; }
        /// <summary>
        /// 任务id
        /// </summary>
        public long taskId { get; set; }
        /// <summary>
        /// 任务类型：rfid视-射频盘点 vision-觉盘点  stop-结束盘点  scan-入库口组盘信息
        /// </summary>
        public string taskType { get; set; }
    }
}