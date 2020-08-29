using System.Runtime.InteropServices;
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
    private static extern int SMT_getErrorCode();

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
    /// 画面上の顏の中心、左目、右目位置を取得
    /// z要素は半径
    /// </summary>
    public static void getFacePoints(out Vector3 face, out Vector3 leftEye, out Vector3 rightEye) {
        int length = 9;
        float[] points = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        System.IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * length);
        SMT_getFacePoints(ptr);
        Marshal.Copy(ptr, points, 0, length);

        face = new Vector3(points[0], points[1], points[2]);
        leftEye = new Vector3(points[3], points[4], points[5]);
        rightEye = new Vector3(points[6], points[7], points[8]);
    }


    public static int getErrorCode() {
        return SMT_getErrorCode();
    }
};

