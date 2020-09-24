using UnityEngine;

public class Parameter {
    public bool isForcedTPose;
    //public bool useARMarker;
    public bool useFaceTracking;
    public float faceAngleBaseDistance;
    public float translationMagnification;
    public Vector3 translationMagnifications;
    public float rotationMagnification;
    public Vector3 rotationMagnifications;
    public bool useEyeTracking;
    public bool useEyesLRSync;
    public bool useEyesBlink;
    public int irisThreshold;
    public Vector2 irisOffset;
    public Vector2 irisTranslationMagnifications;
    public float maxEyeOpenThreshold;
    public float minEyeOpenThreshold;
    public bool useHandTracking;
    public float handMovingThresholdMin;
    public float handMovingThresholdMax;
    public float handUndetectedDuration;
    public Vector3 handOffset;
    public Vector3 handTranslationMagnifications;
    public int smoothingLevel;
    public float autoAdjustmentRatio;
    public float autoAdjustmentDelay;
    public string deviceName;
    public float mirror;
    public int port;
}
