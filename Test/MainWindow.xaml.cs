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

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            TaskIn taskIn = new TaskIn() 
            { 
                robotId=Convert.ToInt64(JQ.Text),
                taskId=Convert.ToInt64(RW.Text),
                taskType=LX.Text
            };
            await PostDataToApi(taskIn);
        }
        public async Task<string> PostDataToApi(TaskIn taskIn)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiUrl = "http://localhost:5000/Inventory/Inventory";
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
    }
}