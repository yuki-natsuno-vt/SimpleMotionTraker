using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class SMT
{
    public const int SMT_ERROR_NOEN = 0; 
    public const int SMT_ERROR_UNOPEND_CAMERA = -1;
    public const int SMT_ERROR_UNOPEND_CAMERA_PARAM_FILE = -2;
    public const int SMT_ERROR_UNOPEN_FACE_CASCADE = -3;
    public const int SMT_ERROR_UNOPEN_EYE_CASCADE = -4;

    [DllImport("SimpleMotionTracker", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void SMT_init(string videoDeviceName);
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
    private static extern int SMT_getErrorCode();
    [DllImport("SimpleMotionTracker", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private static extern bool SMT_getOpenFileName(StringBuilder outFilePath, int size);
    [DllImport("SimpleMotionTracker", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private static extern bool SMT_getSaveFileName(StringBuilder outFilePath, int size);

    public static bool isDebug = true;

    public static void init(string videoDeviceName) {
        SMT_init(videoDeviceName);
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
    /// 画面上の顏の中心、左目、右目、左虹彩、右虹彩 位置を取得
    /// z要素は半径
    /// </summary>
    public static void getFacePoints(out Vector3 face, out Vector3 leftEye, out Vector3 rightEye, out Vector3 leftIris, out Vector3 rightIris) {
        int length = 15;
        float[] points = {
            0, 0, 0, // 頭
            0, 0, 0, // 左目
            0, 0, 0, // 右目
            0, 0, 0, // 左虹彩
            0, 0, 0, // 右虹彩
        };

        System.IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * length);
        SMT_getFacePoints(ptr);
        Marshal.Copy(ptr, points, 0, length);

        face = new Vector3(points[0], points[1], points[2]);
        leftEye = new Vector3(points[3], points[4], points[5]);
        rightEye = new Vector3(points[6], points[7], points[8]);
        leftIris = new Vector3(points[9], points[10], points[11]);
        rightIris = new Vector3(points[12], points[13], points[14]);
    }

    public static void setIrisThresh(int thresh) {
        SMT_setIrisThresh(thresh);
    }

    public static int getErrorCode() {
        return SMT_getErrorCode();
    }

    // Unityのエディタ上で実行すると作業ディレクトリが変更された、のようなエラーメッセージが出て強制終了する
    // ビルド済みのexeの場合は問題ない.
    public static string getOpenFileName() {
        if (isDebug) {
            return "G:/test.cfg";
        }
        StringBuilder sb = new StringBuilder(260 * 4);
        if (SMT_getOpenFileName(sb, sb.Capacity)) {
            return sb.ToString();
        }
        return "";
    }

    public static string getSaveFileName() {
        if (isDebug) {
            return "G:/test.cfg";
        }
        StringBuilder sb = new StringBuilder(260 * 4);
        if (SMT_getSaveFileName(sb, sb.Capacity)) {
            return sb.ToString();
        }
        return "";
    }
};

