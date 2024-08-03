namespace InventoryAPI.Common
{
    public class TaskScanbACK
    {
        /// <summary>
        /// 机器人id
        /// </summary>
        public long queryRobotId { get; set; }
        /// <summary>
        /// 任务id
        /// </summary>
        public long taskId { get; set; }
        /// <summary>
        /// 扫码信息
        /// </summary>
        public string? scanInfo { get; set; }
        /// <summary>
        /// Rfid盘点结果
        /// </summary>
        public List<string>? rfidResult { get; set; }
    }
}
