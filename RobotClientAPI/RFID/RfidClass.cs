using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;

public class RfidClass
{
    GClient clientConn;
    eConnectionAttemptEventStatusType status;
    private static RfidClass instance;
    public static RfidClass Instance => GetInstance();
    private static readonly object lockobj = new object();//线程锁
    public HashSet<string> recevid = new HashSet<string>();
    private string rfidName;
    public RfidClass()
    {
        clientConn = new GClient();
    }
    public static RfidClass GetInstance()
    {
        if (instance == null)
        {
            lock (lockobj)
            {
                if (instance == null)
                {
                    instance = new RfidClass();
                }
            }
        }
        return instance;
    }
    public bool RfidOpen()
    {
        if (clientConn.OpenTcp("192.168.0.30:8160", 3000, out status))
        {
            if (status == eConnectionAttemptEventStatusType.OK)
            {
                clientConn.OnEncapedTagEpcLog += new delegateEncapedTagEpcLog(OnEncapedTagEpcLog);
                clientConn.OnEncapedTagEpcOver += new delegateEncapedTagEpcOver(OnEncapedTagEpcOver);
                clientConn.OnTcpDisconnected += new delegateTcpDisconnected(OnTcpDisconnected);
            }
            return true;
        }
        else
        {
            return false;
        }

    }

    private void OnTcpDisconnected(string readerName)
    {
        clientConn.Close();
        //rfidName = readerName;
        //clientConn.OpenTcpRetry(readerName, 3000, out status, 3);
    }

    public bool RfidConnect()
    {
        return clientConn.OpenTcpRetry(rfidName, 3000, out status, 3);
    }
    public List<string> CloseRfid()
    {
        MsgBaseStop msgBaseStop = new MsgBaseStop();
        clientConn.SendSynMsg(msgBaseStop);
        if (0 == msgBaseStop.RtCode)
        {
            return recevid.ToList();
        }
        else { return null; }
    }
    public bool ReadRfid()
    {
        MsgBaseInventoryEpc msgBaseInventoryEpc = new MsgBaseInventoryEpc();
        msgBaseInventoryEpc.AntennaEnable = (uint)(eAntennaNo._1 | eAntennaNo._2 | eAntennaNo._3 | eAntennaNo._4);
        msgBaseInventoryEpc.InventoryMode = (byte)eInventoryMode.Inventory;
        recevid.Clear();
        clientConn.SendSynMsg(msgBaseInventoryEpc);
        if (0 == msgBaseInventoryEpc.RtCode)
        {
            return true;
        }
        else { return false; }
    }
    private static void OnEncapedTagEpcOver(EncapedLogBaseEpcOver msg)
    {

    }

    private void OnEncapedTagEpcLog(EncapedLogBaseEpcInfo msg)
    {
        if (null != msg && 0 == msg.logBaseEpcInfo.Result)
        {
            recevid.Add(msg.logBaseEpcInfo.Epc);
        }
    }
}