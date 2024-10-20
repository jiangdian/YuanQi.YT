using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;

public class RfidServerClass : IHostedService
{
    GClient gClient = new GClient();
    public HashSet<string> recevid = new HashSet<string>();
    public RfidServerClass()
    {
        //GServer gServer = new GServer();
        //gServer.OnGClientConnected += new delegateGClientConnected(OnGClientConnected);
        //gServer.Open("192.168.0.66", 8160);
    }

    private void OnGClientConnected(GClient client)
    {
        if (gClient != client)
        {
            gClient = client;
            client.OnTcpDisconnected += new delegateTcpDisconnected(OnTcpDisconnected);
            client.OnEncapedTagEpcLog += new delegateEncapedTagEpcLog(OnEncapedTagEpcLog);
            client.OnEncapedTagEpcOver += new delegateEncapedTagEpcOver(OnEncapedTagEpcOver);
        }
    }

    private void OnEncapedTagEpcOver(EncapedLogBaseEpcOver msg)
    {

    }

    private void OnEncapedTagEpcLog(EncapedLogBaseEpcInfo msg)
    {
        if (null != msg && 0 == msg.logBaseEpcInfo.Result)
        {
            recevid.Add(msg.logBaseEpcInfo.Epc);
        }
    }

    private void OnTcpDisconnected(string readerName)
    {
        gClient.Close();
    }
    public bool ReadRfid()
    {
        MsgBaseInventoryEpc msgBaseInventoryEpc = new MsgBaseInventoryEpc();
        msgBaseInventoryEpc.AntennaEnable = (uint)(eAntennaNo._1 | eAntennaNo._2 | eAntennaNo._3 | eAntennaNo._4);
        msgBaseInventoryEpc.InventoryMode = (byte)eInventoryMode.Inventory;
        recevid.Clear();
        gClient.SendSynMsg(msgBaseInventoryEpc);
        if (0 == msgBaseInventoryEpc.RtCode)
        {
            return true;
        }
        else { return false; }
    }
    public List<string> CloseRfid()
    {
        MsgBaseStop msgBaseStop = new MsgBaseStop();
        gClient.SendSynMsg(msgBaseStop);
        if (0 == msgBaseStop.RtCode)
        {
            return recevid?.ToList();
        }
        else { return null; }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        GServer gServer = new GServer();
        gServer.OnGClientConnected += new delegateGClientConnected(OnGClientConnected);
        gServer.Open("192.168.0.66", 8160);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        gClient.Close();
        return Task.CompletedTask;
    }

}