namespace InventoryAPI.Common
{
    public class TaskOut
    {
        /// <summary>
        /// 返回状态： 200-成功 500-失败
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string? msg { get; set; }
    }
}