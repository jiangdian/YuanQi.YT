using TouchSocket.Core;
using TouchSocket.Sockets;

namespace InventoryAPI.Common
{
    class MyPlugin : PluginBase, ITcpReceivedPlugin, ITcpConnectedPlugin
    {
        ILogger<MyPlugin> _logger;
        public MyPlugin(ILogger<MyPlugin> logger)
        {
            _logger = logger;
        }
        public Task OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            _logger.LogInformation(e.ByteBlock.ToString());
            return Task.CompletedTask;
        }

        public Task OnTcpConnected(ITcpClientBase client, ConnectedEventArgs e)
        {
            _logger.LogInformation(client.IP + "连接");
            return Task.CompletedTask;
        }
    }
}