using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpService service = new TcpService();
        public MainWindow()
        {
            InitializeComponent();

            service.Connecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在连接
            service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
            service.Disconnecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在断开连接，只有当主动断开时才有效。
            service.Disconnected = (client, e) =>
            {
                return EasyTask.CompletedTask;
            };//有客户端断开连接
            service.Received = (client, e) =>
            {
                //从客户端收到信息
                var mes = Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);//注意：数据长度是byteBlock.Len
                client.Logger.Info($"已从{client.Id}接收到信息：{mes}");

                client.Send(mes);//将收到的信息直接返回给发送方

                //client.Send("id",mes);//将收到的信息返回给特定ID的客户端

                //将收到的信息返回给在线的所有客户端。
                //注意：这里只是一个建议思路，实际上群发应该使用生产者消费者模式设计
                //var ids = service.GetIds();
                //foreach (var clientId in ids)
                //{
                //    if (clientId != client.Id)//不给自己发
                //    {
                //        service.Send(clientId, mes);
                //    }
                //}

                return EasyTask.CompletedTask;
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    //a.Add();//此处可以添加插件
                }));

            service.Start();//启动
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            TaskIn taskIn = new TaskIn()
            {
                robotId = Convert.ToInt64(JQ.Text),
                taskId = Convert.ToInt64(RW.Text),
                taskType = LX.Text
            };
            await PostDataToApi(taskIn);

            //Testt testt = new Testt() {
            //    assetCode =new List<string>{ "111" },
            //     trayCode = new List<string> { "111111" }
            //};
            //await PostDataToApi2(testt);
        }
        public async Task<string> PostDataToApi(TaskIn taskIn)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiUrl = "http://localhost:5000/Inventory/Inventory";
                    //string apiUrl = "http://127.0.0.1:10010/Inventory/Inventory";
                    //string apiUrl = "http://127.0.0.1:10011/Inventory/Inventory";
                    var jsonString = JsonConvert.SerializeObject(taskIn);
                    HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var ss = await response.Content.ReadAsStringAsync();
                        return ss;
                    }
                    else
                    {
                        throw new Exception($"Error calling API: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }
        public async Task<string> PostDataToApi2(Testt taskIn)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiUrl = "http://10.10.109.12:5000/Inflow/ListenInflow";
                    var jsonString = JsonConvert.SerializeObject(taskIn);
                    HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        throw new Exception($"Error calling API: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            TaskIn taskIn = new TaskIn()
            {
                robotId = Convert.ToInt64(JQ.Text),
                taskId = Convert.ToInt64(RW.Text),
                taskType = LX.Text
            };
            var mes = JsonConvert.SerializeObject(taskIn);
            var ids = service.GetIds();
            foreach (var clientId in ids)
            {
                service.Send(clientId, mes);
            }
        }
    }
}