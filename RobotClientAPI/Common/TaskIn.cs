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
    /// 左侧货位编号 当taskType为vision时，货位编号不允许为空
    /// </summary>
    public string? leftStorageCode { get; set; }
    /// <summary>
    /// 托盘编号 当taskType为scan和record时，当前的托盘编号附在该字段下，且不允许为空
    /// </summary>
    public string? trayCode { get; set; }
    /// <summary>
    /// 右侧货位编号 当taskType为vision时，货位编号不允许为空
    /// </summary>
    public string? rightStorageCode { get; set; }
    /// <summary>
    /// 任务类型：rfid视-射频盘点 vision-觉盘点  stop-结束盘点  scan-入库口组盘信息  record  射频门后拍照
    /// </summary>
    public string taskType { get; set; }
}