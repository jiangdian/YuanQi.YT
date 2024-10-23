namespace RobotClientAPI.Common
{
    public class FrontShutterTaskBack
    {
        public long robotId { get; set; }

        public long taskId { get; set; }

        public string trayCode { get; set; }

        public List<string>? scanInfo { get; set; }
    }
}