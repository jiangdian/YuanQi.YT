using HalconDotNet;
using System.Net.Sockets;
using System.Text;

public class VisionClass
{
    private static VisionClass instance;

    private static readonly object lockobj = new object();//线程锁

    HTuple hv_AcqHandle1 = new HTuple();

    HTuple hv_AcqHandle2 = new HTuple();

    IConfiguration configuration;

    ILogger<InventoryController> logger;

    private VisionClass(IConfiguration _configuration, ILogger<InventoryController> _logger)
    {
        configuration = _configuration;
        logger = _logger;
    }

    public static VisionClass GetInstance(IConfiguration _configuration, ILogger<InventoryController> _logger)
    {
        if (instance == null)
        {
            lock (lockobj)
            {
                if (instance == null)
                {
                    instance = new VisionClass(_configuration, _logger);
                }
            }
        }
        return instance;
    }

    public string GetCodeNode(string ip, int port)
    {
        string messageReceived = "";
        try
        {
            // 创建一个TcpClient实例
            using (TcpClient client = new TcpClient(ip, port))
            {
                // 获取网络流，用于发送和接收数据
                NetworkStream stream = client.GetStream();

                // 要发送的消息
                string messageToSend = "start";
                byte[] dataToSend = Encoding.ASCII.GetBytes(messageToSend);

                // 发送数据
                stream.Write(dataToSend, 0, dataToSend.Length);

                // 接收数据（如果需要）
                byte[] dataReceived = new byte[1024];
                int bytesReceived = stream.Read(dataReceived, 0, dataReceived.Length);
                messageReceived = Encoding.ASCII.GetString(dataReceived, 0, bytesReceived);

                // 关闭网络流和TcpClient
                stream.Close();
                client.Close();
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex.Message);
        }
        return messageReceived;
    }

    public void GrabImageVision(string device1, string device2, string trayCode1, string trayCode2, out HObject? ho_Image1, out HObject? ho_Image2, out string? image1Url, out string? image2Url)
    {
        try
        {
            HOperatorSet.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive",
            -1, "default", -1, "false", "default", device1, 0, -1, out hv_AcqHandle1);
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle1, "ExposureTime", 100000.0);
            HOperatorSet.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive",
            -1, "default", -1, "false", "default", device2, 0, -1, out hv_AcqHandle2);
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle2, "ExposureTime", 100000.0);

            HOperatorSet.GrabImage(out ho_Image1, hv_AcqHandle1);
            HOperatorSet.GrabImage(out ho_Image2, hv_AcqHandle2);

            foreach (string file in Directory.GetFiles(@"\\192.168.10.150\picture\vision"))
            {
                if (Path.GetFileName(file).Contains($"{trayCode1}"))
                {
                    File.Delete(file);
                    break;
                }
            }
            image1Url = @$"\\192.168.10.150\picture\vision\{DateTime.Now:yyddMMHHmmss}_{trayCode1}.jpg";
            HOperatorSet.WriteImage(ho_Image1, "jpg", 255, image1Url);
            foreach (string file in Directory.GetFiles(@"\\192.168.10.150\picture\vision"))
            {
                if (Path.GetFileName(file).Contains($"{trayCode2}"))
                {
                    File.Delete(file);
                    break;
                }
            }
            image2Url = @$"\\192.168.10.150\picture\vision\{DateTime.Now:yyddMMHHmmss}_{trayCode2}.jpg";
            HOperatorSet.WriteImage(ho_Image2, "jpg", 255, image2Url);

            HOperatorSet.CloseFramegrabber(hv_AcqHandle1);
            HOperatorSet.CloseFramegrabber(hv_AcqHandle2);
        }
        catch (Exception ex)
        {
            ho_Image1 = null;
            ho_Image2 = null;
            image1Url = null;
            image2Url = null;
            logger.LogError(ex.Message);
        }
    }

    public void GrabImageRecord(string device1, string device2, string trayCode, out HObject? ho_Image1, out HObject? ho_Image2)
    {
        try
        {
            HOperatorSet.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive",
            -1, "default", -1, "false", "default", device1, 0, -1, out hv_AcqHandle1);
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle1, "ExposureTime", 60000.0);

            HOperatorSet.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive",
            -1, "default", -1, "false", "default", device2, 0, -1, out hv_AcqHandle2);
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle2, "ExposureTime", 60000.0);

            HOperatorSet.GrabImage(out ho_Image1, hv_AcqHandle1);
            HOperatorSet.GrabImage(out ho_Image2, hv_AcqHandle2);

            foreach (string file in Directory.GetFiles(@"\\192.168.10.150\picture\record"))
            {
                if (Path.GetFileName(file).Contains($"{trayCode}_front"))
                {
                    File.Delete(file);
                    break;
                }
            }
            HOperatorSet.WriteImage(ho_Image1, "jpg", 255, $@"\\192.168.10.150\picture\record\{trayCode}_front.jpg");
            logger.LogInformation("图片1写入完毕");

            foreach (string file in Directory.GetFiles(@"\\192.168.10.150\picture\record"))
            {
                if (Path.GetFileName(file).Contains($"{trayCode}_behind"))
                {
                    File.Delete(file);
                    break;
                }
            }
            HOperatorSet.WriteImage(ho_Image2, "jpg", 255, $@"\\192.168.10.150\picture\record\{trayCode}_behind.jpg"); 
            logger.LogInformation("图片2写入完毕");

            HOperatorSet.CloseFramegrabber(hv_AcqHandle1);
            HOperatorSet.CloseFramegrabber(hv_AcqHandle2);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            HOperatorSet.CloseFramegrabber(hv_AcqHandle1);
            HOperatorSet.CloseFramegrabber(hv_AcqHandle2);
            ho_Image1 = null;
            ho_Image2 = null;
        }
    }

    public void GetStorageInformation(HObject ho_ImageLeft, HObject ho_ImageRight, string trayCodeLeft, string trayCodeRight,
        out List<string> materialNoListLeft, out List<string> materialNoListRight, out bool compareResultLeft, out bool compareResultRight)
    {
        materialNoListLeft = new List<string>(); materialNoListRight = new List<string>();
        compareResultLeft = false; compareResultRight = false;
        try
        {
            if (trayCodeLeft != null)
            {
                compareResultLeft = CompareStorage(trayCodeLeft, "front", "left", ho_ImageLeft, compareResultLeft, Convert.ToDouble(configuration["visionLeftRow1"]),
                    Convert.ToDouble(configuration["visionLeftColumn1"]), Convert.ToDouble(configuration["visionLeftRow2"]), Convert.ToDouble(configuration["visionLeftColumn2"]));
                materialNoListLeft = FindBarCode(ho_ImageLeft, materialNoListLeft);
                materialNoListLeft = FindData2dCode(ho_ImageLeft, materialNoListLeft);
                materialNoListLeft = materialNoListLeft.Distinct().ToList();
            }

            if (trayCodeRight != null)
            {
                compareResultRight = CompareStorage(trayCodeRight, "behind", "right", ho_ImageRight, compareResultRight, Convert.ToDouble(configuration["visionRightRow1"]),
                    Convert.ToDouble(configuration["visionRightColumn1"]), Convert.ToDouble(configuration["visionRightRow2"]), Convert.ToDouble(configuration["visionRightColumn2"]));
                materialNoListRight = FindBarCode(ho_ImageRight, materialNoListRight);
                materialNoListRight = FindData2dCode(ho_ImageRight, materialNoListRight);
                materialNoListRight = materialNoListRight.Distinct().ToList();
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }

    private bool CompareStorage(string trayCode, string recordLocation, string storageLocation, HObject ho_RealStorageImage, bool compareResult, double row1, double column1, double row2, double column2)
    {
        string imageUrl = "";
        try
        {
            foreach (string file in Directory.GetFiles(@"\\192.168.10.150\picture\record"))
            {
                if (Path.GetFileName(file).Contains($"{trayCode}_{recordLocation}"))
                {
                    imageUrl = Path.GetFullPath(file);
                    break;
                }
            }
            HOperatorSet.ReadImage(out HObject ho_InitRecordImage, $@"\\192.168.10.150\picture\record\{recordLocation}.jpg");
            HOperatorSet.ReadImage(out HObject ho_InitStorageImage, $@"D:\InitImage\{storageLocation}.jpg");
            HOperatorSet.ReadImage(out HObject ho_RealRecordImage, imageUrl);
            VisionCompare(ho_InitRecordImage, ho_RealRecordImage, out double recordHeight, out double recordArea);
            VisionCompare(ho_InitStorageImage, ho_RealStorageImage, out double storageHeight, out double storageArea);
            if ((recordHeight / storageHeight <= 1) && (recordHeight / storageHeight >= 0.5) && (recordArea / storageArea <= 1) && (recordArea / storageArea >= 0.5))
                compareResult = true;
            else
                compareResult = false;
        }
        catch(Exception ex)
        {
            logger.LogError(ex.Message);
        }
        return compareResult;
    }

    private void VisionCompare(HObject ho_InitImage, HObject ho_RealImage, out double height, out double area)
    {
        try
        {
            HOperatorSet.AbsDiffImage(ho_InitImage, ho_RealImage, out HObject ho_ImageAbsDiff, 1);
            HOperatorSet.Rgb1ToGray(ho_ImageAbsDiff, out HObject ho_GrayImage);
            HOperatorSet.Threshold(ho_GrayImage, out HObject ho_Region, 10, 200);
            HOperatorSet.ErosionRectangle1(ho_Region, out HObject ho_RegionErosion, 1, 21);
            HOperatorSet.DilationRectangle1(ho_RegionErosion, out HObject ho_RegionDilation, 25, 25);
            HOperatorSet.Connection(ho_RegionDilation, out HObject ho_ConnectedRegions);
            HOperatorSet.SelectShape(ho_ConnectedRegions, out HObject ho_SelectedRegions, "area", "and", 99999, 9999999);
            HOperatorSet.AreaCenter(ho_SelectedRegions, out HTuple hv_Area, out _, out _);
            HOperatorSet.TupleSortIndex(hv_Area, out HTuple hv_Indices);
            HOperatorSet.CountObj(ho_SelectedRegions, out HTuple hv_Number);
            HOperatorSet.SelectObj(ho_SelectedRegions, out HObject ho_ObjectSelected, hv_Indices[hv_Number.I - 1] + 1);
            HOperatorSet.SmallestRectangle1(ho_ObjectSelected, out _, out HTuple hv_Column1, out _, out HTuple hv_Column2);
            height = Math.Abs(hv_Column2.D - hv_Column1.D);
            area = hv_Area.D;
        }
        catch(Exception ex)
        {
            height = 0;
            area = 0;
        }
    }

    private List<string> FindBarCode(HObject ho_Image, List<string> materialNoList)
    {
        try
        {
            HOperatorSet.CreateBarCodeModel(new HTuple(), new HTuple(), out HTuple hv_BarCodeHandle);
            HOperatorSet.FindBarCode(ho_Image, out _, hv_BarCodeHandle, "auto", out HTuple hv_DecodedDataStrings);
            if (hv_DecodedDataStrings != null)
            {
                for (int i = 0; i < hv_DecodedDataStrings.Length; i++)
                    materialNoList.Add(hv_DecodedDataStrings[i].S);
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex.Message);
        }
        return materialNoList;
    }

    private List<string> FindData2dCode(HObject ho_Image, List<string> materialNoList)
    {
        try
        {
            HOperatorSet.CreateDataCode2dModel("QR Code", new HTuple(), new HTuple(), out HTuple hv_DataCodeHandle);
            HOperatorSet.FindDataCode2d(ho_Image, out _, hv_DataCodeHandle, new HTuple(), new HTuple(), out _, out HTuple hv_DecodedDataStrings);
            if (hv_DecodedDataStrings != null)
            {
                for (int i = 0; i < hv_DecodedDataStrings.Length; i++)
                    materialNoList.Add(hv_DecodedDataStrings[i].S);
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex.Message);
        }
        return materialNoList;
    }
}