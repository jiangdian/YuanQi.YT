public class Vision
{
    /// <summary>
    /// 库位号
    /// </summary>
    public string storageArea { get; set; }
    /// <summary>
    /// 托盘码
    /// </summary>
    public string? trayNo { get; set; }
    /// <summary>
    /// 物料集合
    /// </summary>
    public List<string>? materialNoList { get; set; }
    /// <summary>
    /// 盘点是否一致
    /// </summary>
    public bool? compareResult { get; set; }
    /// <summary>
    /// 图片地址
    /// </summary>
    public string imgUrl { get; set; }
}