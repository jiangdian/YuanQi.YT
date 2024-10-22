using HalconDotNet;

public class VisionClass
{
    private static VisionClass instance;

    private static readonly object lockobj = new object();//线程锁

    public static VisionClass Instance => GetInstance();

    HTuple hv_AcqHandle1 = new HTuple();

    HTuple hv_AcqHandle2 = new HTuple();

    public VisionClass()
    {
        HOperatorSet.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive",
        -1, "default", -1, "false", "default", "00214905024a_DahengImaging_MER220006GC",
        0, -1, out hv_AcqHandle1);

        HOperatorSet.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive",
        -1, "default", -1, "false", "default", "00214905024e_DahengImaging_MER220006GC",
        0, -1, out hv_AcqHandle2);
    }

    public static VisionClass GetInstance()
    {
        if (instance == null)
        {
            lock (lockobj)
            {
                if (instance == null)
                {
                    instance = new VisionClass();
                }
            }
        }
        return instance;
    }

    public void GrabImage()
    {
        HOperatorSet.GrabImage(out HObject ho_Image1, hv_AcqHandle1);
        HOperatorSet.GrabImage(out HObject ho_Image2, hv_AcqHandle2);
        HOperatorSet.WriteImage(ho_Image1, "jpg", 255, "./test1.jpg");
        HOperatorSet.WriteImage(ho_Image2, "jpg", 255, "./test2.jpg");
    }
}