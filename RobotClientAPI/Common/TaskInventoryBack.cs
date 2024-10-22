public class TaskInventoryBack
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
    /// 扫码信息
    /// </summary>
    public string? scanInfo { get; set; }
    /// <summary>
    /// 视觉盘点结果
    /// </summary>
    public List<Vision>? visionResult { get; set; }
    /// <summary>
    /// Rfid盘点结果
    /// </summary>
    public List<string>? rfidResult { get; set; }

    /// <summary>
    /// 任务开始时间
    /// </summary>
    public string startTime { get; set; }
    /// <summary>
    /// 任务结束时间
    /// </summary>
    public string endTime { get; set; }
}