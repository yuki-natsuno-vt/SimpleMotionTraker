using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;


//using System.Windows.Forms;

public class SMT
{
    public const int SMT_ERROR_NOEN = 0; 
    public const int SMT_ERROR_UNOPEND_CAMERA = -1;
    public const int SMT_ERROR_UNOPEND_CAMERA_PARAM_FILE = -2;
    public const int SMT_ERROR_UNOPEN_FACE_CASCADE = -3;
    public const int SMT_ERROR_UNOPEN_EYE_CASCADE = -4;
    public const int SMT_ERROR_UNREADABLE_CAMERA = -5;
    public const int SMT_ERROR_INSUFFICIENT_CAMERA_CAPTURE_SPEED = -6;

    [DllImport("SimpleMotionTracker", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void SMT_init(string videoDeviceName, string dataPath);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_destroy();
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_update();
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setUseARMarker(bool useARMarker);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setUseFaceTracking(bool useFaceTracking);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setUseEyeTracking(bool useEyeTracking);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setUseHandTracking(bool useHandTracking);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setCaptureShown(bool isShown);
    [DllImport("SimpleMotionTracker")]
    private static extern bool SMT_isCaptureShown();
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setARMarkerEdgeLength(float length);
    [DllImport("SimpleMotionTracker")]
    private static extern bool SMT_isARMarkerDetected(int id);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_getARMarker6DoF(int id, System.IntPtr outArray);
    [DllImport("SimpleMotionTracker")]
    private static extern bool SMT_isFacePointsDetected();
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_getFacePoints(System.IntPtr outArray);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setIrisThresh(int thresh);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setMinHandTranslationThreshold(float thresh);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setMaxHandTranslationThreshold(float thresh);
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_setHandUndetectedDuration(int msec);
    [DllImport("SimpleMotionTracker")]
    private static extern bool SMT_isLeftHandDetected();
    [DllImport("SimpleMotionTracker")]
    private static extern bool SMT_isRightHandDetected();
    [DllImport("SimpleMotionTracker")]
    private static extern bool SMT_isLeftHandDown();
    [DllImport("SimpleMotionTracker")]
    private static extern bool SMT_isRightHandDown();
    [DllImport("SimpleMotionTracker")]
    private static extern void SMT_getHandPoints(System.IntPtr outArray);
    [DllImport("SimpleMotionTracker")]
    private static extern int SMT_getErrorCode();

    public static bool isDebug = true;

    public static void init(string videoDeviceName) {
        string dataPath = "SimpleMotionTracker_Data/StreamingAssets/";
        if (isDebug)
        {
            dataPath = "Assets/StreamingAssets/";
        }
        SMT_init(videoDeviceName, dataPath);
    }

    public static void destroy() {
        SMT_destroy();
    }

    public static void update() {
        SMT_update();
    }

    public static void setUseARMarker(bool useARMarker) {
        SMT_setUseARMarker(useARMarker);
    }

    public static void setUseFaceTracking(bool useFaceTracking) {
        SMT_setUseFaceTracking(useFaceTracking);
    }

    public static void setUseEyeTracking(bool useEyeTracking) {
        SMT_setUseEyeTracking(useEyeTracking);
    }

    public static void setUseHandTracking(bool useHandTracking) {
        SMT_setUseHandTracking(useHandTracking);
    }

    public static void setCaptureShown(bool isShown) {
        SMT_setCaptureShown(isShown);
    }

    public static bool isCaptureShown() {
        return SMT_isCaptureShown();
    }

    public static void setARMarkerEdgeLength(float length) {
        SMT_setARMarkerEdgeLength(length);
    }

    public static bool isARMarkerDetected(int id) {
        return SMT_isARMarkerDetected(id);
    }

    public static void getARMarker6DoF(int id, out Vector3 t, out Vector3 r) {
        int length = 6;
        float[] tr = { 0, 0, 0, 0, 0, 0 };

        System.IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * length);
        SMT_getARMarker6DoF(id, ptr);
        Marshal.Copy(ptr, tr, 0, length);

        t = new Vector3(tr[0], tr[1], tr[2]);
        r = new Vector3(tr[3], tr[4], tr[5]);

        // 補正
        r.x = r.x + 180;
        if (r.x > 180) r.x = r.x - 360;
        r.x = r.x * -1;
        r.z = r.z * -1;
        t.y = t.y * -1;
    }


    public static bool isFacePointsDetected() {
        return SMT_isFacePointsDetected();
    }

    /// <summary>
    /// 画面上の顏の中心、顏の角度、左目、右目、左虹彩、右虹彩 位置を取得
    /// 顏、目のz要素は半径、虹彩のz要素は縦横比
    /// </summary>
    public static void getFacePoints(out Vector3 face, out Vector3 faceAngle, out Vector3 leftEye, out Vector3 rightEye, out Vector3 leftIris, out Vector3 rightIris) {
        int length = 18;
        float[] points = {
            0, 0, 0, // 頭
            0, 0, 0, // 左目
            0, 0, 0, // 右目
            0, 0, 0, // 左虹彩
            0, 0, 0, // 右虹彩
            0, 0, 0, // 頭角度
        };

        System.IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * length);
        SMT_getFacePoints(ptr);
        Marshal.Copy(ptr, points, 0, length);

        face = new Vector3(points[0], points[1], points[2]);
        leftEye = new Vector3(points[3], points[4], points[5]);
        rightEye = new Vector3(points[6], points[7], points[8]);
        leftIris = new Vector3(points[9], points[10], points[11]);
        rightIris = new Vector3(points[12], points[13], points[14]);
        faceAngle = new Vector3(points[15], points[16], points[17]);
    }

    public static void setIrisThresh(int thresh) {
        SMT_setIrisThresh(thresh);
    }

    public static void setMinHandTranslationThreshold(float thresh) {
        SMT_setMinHandTranslationThreshold(thresh);
    }

    public static void setMaxHandTranslationThreshold(float thresh) {
        SMT_setMaxHandTranslationThreshold(thresh);
    }

    public static void setHandUndetectedDuration(int msec) {
        SMT_setHandUndetectedDuration(msec);
    }

    public static bool isLeftHandDetected() {
        return SMT_isLeftHandDetected();
    }

    public static bool isRightHandDetected() {
        return SMT_isRightHandDetected();
    }

    public static bool isLeftHandDown() {
        return SMT_isLeftHandDown();
    }

    public static bool isRightHandDown() {
        return SMT_isRightHandDown();
    }

    /// <summary>
    /// 画面上の左手、右手 位置を取得
    /// z要素は半径
    /// </summary>
    public static void getHandPoints(out Vector3 leftHand, out Vector3 rightHand) {
        int length = 6;
        float[] points = {
            0, 0, 0, // 左手
            0, 0, 0, // 右手
         };

        System.IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * length);
        SMT_getHandPoints(ptr);
        Marshal.Copy(ptr, points, 0, length);

        leftHand = new Vector3(points[0], points[1], points[2]);
        rightHand = new Vector3(points[3], points[4], points[5]);
    }

    public static int getErrorCode() {
        return SMT_getErrorCode();
    }

    // Unityのエディタ上で実行すると作業ディレクトリが変更された、のようなエラーメッセージが出て強制終了する
    // ビルド済みのexeの場合は問題ない.
    public static string getOpenFileName() {
        var workDir = System.IO.Directory.GetCurrentDirectory();

        ProcessStartInfo pInfo = new ProcessStartInfo();
        pInfo.FileName = workDir + "/SelectFile.exe";
        pInfo.Arguments = "-l \"Open Files\" \"Config files(*.cfg)\\0 *.cfg\\0All files(*.*)\\0 *.*\\0\\0\" \"cfg\"";
        Process p = Process.Start(pInfo);
        p.WaitForExit();

        var fileName = File.ReadAllText("temp_path");
        return fileName;
    }

    public static string getSaveFileName() {
        var workDir = System.IO.Directory.GetCurrentDirectory();

        ProcessStartInfo pInfo = new ProcessStartInfo();
        pInfo.FileName = workDir + "/SelectFile.exe";
        pInfo.Arguments = "-s \"Save Files\" \"Config files(*.cfg)\\0 *.cfg\\0All files(*.*)\\0 *.*\\0\\0\" \"cfg\"";
        Process p = Process.Start(pInfo);
        p.WaitForExit();

        var fileName = File.ReadAllText("temp_path");
        return fileName;
    }
};

