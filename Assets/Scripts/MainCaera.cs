﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class MainCaera : MonoBehaviour {

    [SerializeField] GameObject _head;
    [SerializeField] GameObject _leftHand;
    [SerializeField] GameObject _rightHand;
    [SerializeField] GameObject _leftIris;
    [SerializeField] GameObject _rightIris;
    [SerializeField] GameObject _lookAt;
    [SerializeField] GameObject _leftHandPropes;
    [SerializeField] GameObject _rightHandPropes;

    [SerializeField] Toggle _forceTPoseToggle;
    [SerializeField] Toggle _useFaceTrackingToggle;
    [SerializeField] InputField _faceAngleBaseDistanceInputField;
    [SerializeField] InputField _translationMagnificationInputField;
    [SerializeField] InputField _translationMagnificationXInputField;
    [SerializeField] InputField _translationMagnificationYInputField;
    [SerializeField] InputField _translationMagnificationZInputField;
    [SerializeField] InputField _rotationMagnificationInputField;
    [SerializeField] InputField _rotationMagnificationXInputField;
    [SerializeField] InputField _rotationMagnificationYInputField;
    [SerializeField] InputField _rotationMagnificationZInputField;

    [SerializeField] Toggle _useEyeTrackingToggle;
    [SerializeField] Toggle _useEyeBlinkToggle;
    [SerializeField] Toggle _useEyeLRSyncToggle;
    [SerializeField] InputField _irisThresholdInputField;
    [SerializeField] InputField _irisOffsetXInputField;
    [SerializeField] InputField _irisOffsetYInputField;
    [SerializeField] InputField _irisTranslationMagnificationXInputField;
    [SerializeField] InputField _irisTranslationMagnificationYInputField;
    [SerializeField] InputField _eyeOpenThresholdMinInputField;
    [SerializeField] InputField _eyeOpenThresholdMaxInputField;
    
    [SerializeField] Toggle _useHandTrackingToggle;
    [SerializeField] InputField _handMovingThresholdMinInputField;
    [SerializeField] InputField _handMovingThresholdMaxInputField;
    [SerializeField] InputField _handUndetectedDurationInputField;
    [SerializeField] InputField _handOffsetXInputField;
    [SerializeField] InputField _handOffsetYInputField;
    [SerializeField] InputField _handOffsetZInputField;
    [SerializeField] InputField _handTranslationMagnificationXInputField;
    [SerializeField] InputField _handTranslationMagnificationYInputField;
    [SerializeField] InputField _handTranslationMagnificationZInputField;

    [SerializeField] InputField _smoothingLevelInputField;
    [SerializeField] InputField _autoAdjustmentInputField;
    [SerializeField] InputField _autoAdjustmentDelayInputField;

    [SerializeField] Dropdown _videoDeviceList;
    [SerializeField] Text _selectedVideoDeviceName;
    [SerializeField] Text _cameraStartButtonText;
    [SerializeField] Toggle _mirrorToggle;
    [SerializeField] InputField _uOSCInputField;
    [SerializeField] Toggle _showCaptureImageToggle;

    [SerializeField] InputField _vrPlayAreaOffsetTranslationXInputField;
    [SerializeField] InputField _vrPlayAreaOffsetTranslationYInputField;
    [SerializeField] InputField _vrPlayAreaOffsetTranslationZInputField;
    [SerializeField] InputField _vrPlayAreaOffsetRotationXInputField;
    [SerializeField] InputField _vrPlayAreaOffsetRotationYInputField;
    [SerializeField] InputField _vrPlayAreaOffsetRotationZInputField;

    int _zAxisFlipFrameCount = 0; // フリップ発生フレーム数.

    int _cameraId = 0;

    bool _isForcedTPose = false;
    bool _useARMarker = false;
    bool _useFaceTracking = false;

    float _faceAngleBaseDistance = 0.20f;
    float _translationMagnification = 1.0f;
    Vector3 _translationMagnifications = Vector3.one;
    float _rotationMagnification = 1.0f;
    Vector3 _rotationMagnifications = Vector3.one;

    Vector3 _calibratedLeftHandPosition;
    Quaternion _calibratedLeftHandRotation;
    Vector3 _calibratedRightHandPosition;
    Quaternion _calibratedRightHandRotation;
    
    Vector3 _calibratedFacePosition;
    Vector3 _calibratedFaceAngle;
    Vector3 _calibratedLeftEyePosition;
    Vector3 _calibratedRigthEyePosition;

    bool _useEyeTracking = false;
    bool _useEyesBlink = true;
    bool _useEyesLRSync = true;
    int _irisThreshold = 30;
    Vector2 _irisOffset = Vector2.zero;
    Vector2 _irisTranslationMagnifications = Vector2.one;
    float _maxEyeOpenThreshold = 0.41f;
    float _minEyeOpenThreshold = 0.3f;

    const int MAX_EYE_SMOOTHING = 20;
    List<Vector3> _leftEyeList;
    List<Vector3> _rightEyeList;

    const int MAX_IRIS_SMOOTHING = 5;
    List<Vector3> _leftIrisList;
    List<Vector3> _rightIrisList;

    bool _useHandTracking = false;
    const int MAX_HAND_SMOOTHING = 30;
    int _handSmoothingLevel = 10;
    Vector3 _leftHandPositionAve = Vector3.zero;
    Vector3 _rightHandPositionAve = Vector3.zero;
    List<Vector3> _leftHandList;
    List<Vector3> _rightHandList;
    Vector3 _handOffset = Vector3.zero;
    Vector3 _handTranslationMagnifications = Vector3.one;
    float _handStandardPoseMergeRatioAcceleration = 0.005f;
    float _leftHandStandardPoseMergeRatio = 0.0f;
    float _rightHandStandardPoseMergeRatio = 0.0f;
    float _handStandardPoseMergeDelay = 5.0f;
    float _leftHandStandardPoseMergeDelayStartTime = 0;
    float _rightHandStandardPoseMergeDelayStartTime = 0;
    Vector3 _leftHandPositionBackup = Vector3.zero;
    Vector3 _rightHandPositionBackup = Vector3.zero;
    int _leftHandSmoothingDelay = 0;
    int _rightHandSmoothingDelay = 0;

    const int MAX_SMOOTHING_LEVEL = 30;
    int _smoothingLevel = 10;
    List<Vector3> _facePositionList;
    List<Vector3> _faceRotationList;

    float _autoAdjustmentRatio = 0.0f;
    float _autoAdjustmentDelay = 0.0f;
    float _autoAdjustmentDelayStartTime = 0;

    string _deviceName;
    float _mirror = 1;

    int _port = 0;

    bool _isCaptureShown = false;


    Vector3 _vrPlayAreaOffsetTranslation = Vector3.zero;
    Vector3 _vrPlayAreaOffsetRotation = Vector3.zero;

    // 軸の種類
    enum Axis {
        X,
        Y,
        Z,
    }

    public void OnUseARMarker() {
        _useARMarker = !_useARMarker;
        SMT.setUseARMarker(_useARMarker);
    }

    public void OnUseFaceTracking() {
        _useFaceTracking = _useFaceTrackingToggle.isOn;
        SMT.setUseFaceTracking(_useFaceTracking);
    }

    public void OnChangeForceTPose() {
        _isForcedTPose = _forceTPoseToggle.isOn;
    }

    public void OnChangeFaceAngleBaseDistance() {
        _faceAngleBaseDistance = float.Parse(_faceAngleBaseDistanceInputField.text);
    }

    public void OnChangeTranslationMagnification() {
        _translationMagnification = float.Parse(_translationMagnificationInputField.text);
    }

    public void OnChangeTranslationMagnificationX() {
        _translationMagnifications.x = float.Parse(_translationMagnificationXInputField.text);
    }

    public void OnChangeTranslationMagnificationY() {
        _translationMagnifications.y = float.Parse(_translationMagnificationYInputField.text);
    }

    public void OnChangeTranslationMagnificationZ() {
        _translationMagnifications.z = float.Parse(_translationMagnificationZInputField.text);
    }

    public void OnChangeRotationMagnification() {
        _rotationMagnification = float.Parse(_rotationMagnificationInputField.text);
    }

    public void OnChangeRotationMagnificationX() {
        _rotationMagnifications.x = float.Parse(_rotationMagnificationXInputField.text);
    }

    public void OnChangeRotationMagnificationY() {
        _rotationMagnifications.y = float.Parse(_rotationMagnificationYInputField.text);
    }

    public void OnChangeRotationMagnificationZ() {
        _rotationMagnifications.z = float.Parse(_rotationMagnificationZInputField.text);
    }

    public void OnClickCalibrateFacePoints() {
        Vector3 face;
        Vector3 faceAngle;
        Vector3 leftEye;
        Vector3 rightEye;
        Vector3 leftIris;
        Vector3 rightIris;
        SMT.getFacePoints(out face, out faceAngle, out leftEye, out rightEye, out leftIris, out rightIris);

        _calibratedFacePosition = face;
        _calibratedLeftEyePosition = leftEye;
        _calibratedRigthEyePosition = rightEye;
        // 顏の角度は誤差が大きいので平均を基準とする
        smooth(ref _calibratedFaceAngle, _faceRotationList, MAX_SMOOTHING_LEVEL);
    }

    public void OnChangeUseEyeTracking() {
        _useEyeTracking = _useEyeTrackingToggle.isOn;
        SMT.setUseEyeTracking(_useEyeTracking);
        _head.GetComponent<HeadTrackerSender>()._useEyeTracking = _useEyeTracking;
    }

    public void OnChangeUseBlink() {
        _useEyesBlink = _useEyeBlinkToggle.isOn;
        _head.GetComponent<HeadTrackerSender>()._useEyesBlink = _useEyesBlink;
    }

    public void OnChangeLRSync() {
        _useEyesLRSync = _useEyeLRSyncToggle.isOn;
    }

    public void OnChangeIrisThreshold() {
        _irisThreshold = Mathf.Clamp(int.Parse(_irisThresholdInputField.text), 0, 255);
        _irisThresholdInputField.text = _irisThreshold.ToString();
        SMT.setIrisThresh(_irisThreshold);
    }

    public void OnChangeIrisOffsetX() {
        _irisOffset.x = Mathf.Clamp(float.Parse(_irisOffsetXInputField.text), -1.0f, 1.0f);
        _irisOffsetXInputField.text = _irisOffset.x.ToString();
    }

    public void OnChangeIrisOffsetY() {
        _irisOffset.y = Mathf.Clamp(float.Parse(_irisOffsetYInputField.text), -1.0f, 1.0f);
        _irisOffsetYInputField.text = _irisOffset.y.ToString();
    }

    public void OnChangeIrisTranslationMagnificationX() {
        _irisTranslationMagnifications.x = float.Parse(_irisTranslationMagnificationXInputField.text);
    }

    public void OnChangeIrisTranslationMagnificationY() {
        _irisTranslationMagnifications.y = float.Parse(_irisTranslationMagnificationYInputField.text);
    }

    public void OnChangeEyeOpenThresholdMin() {
        _minEyeOpenThreshold = float.Parse(_eyeOpenThresholdMinInputField.text);
    }

    public void OnChangeEyeOpenThresholdMax() {
        _maxEyeOpenThreshold = float.Parse(_eyeOpenThresholdMaxInputField.text);
    }

    public void OnChangeUseHandTracking() {
        _useHandTracking = _useHandTrackingToggle.isOn;
        SMT.setUseHandTracking(_useHandTracking);
    }

    public void OnChangeHandMovingThresholdMin() {
        SMT.setMinHandTranslationThreshold(float.Parse(_handMovingThresholdMinInputField.text));
    }

    public void OnChangeHandMovingThresholdMax() {
        SMT.setMaxHandTranslationThreshold(float.Parse(_handMovingThresholdMaxInputField.text));
    }

    public void OnChangeHandUndetectedDuration() {
        _handStandardPoseMergeDelay = float.Parse(_handUndetectedDurationInputField.text);
        SMT.setHandUndetectedDuration((int)(_handStandardPoseMergeDelay * 1000));
    }

    public void OnChangeHandOffsetX() {
        _handOffset.x = float.Parse(_handOffsetXInputField.text);
    }

    public void OnChangeHandOffsetY() {
        _handOffset.y = float.Parse(_handOffsetYInputField.text);
    }

    public void OnChangeHandOffsetZ() {
        _handOffset.z = float.Parse(_handOffsetZInputField.text);
    }

    public void OnChangeHandTranslationMagnificationX() {
        _handTranslationMagnifications.x = float.Parse(_handTranslationMagnificationXInputField.text);
    }

    public void OnChangeHandTranslationMagnificationY() {
        _handTranslationMagnifications.y = float.Parse(_handTranslationMagnificationYInputField.text);
    }

    public void OnChangeHandTranslationMagnificationZ() {
        _handTranslationMagnifications.z = float.Parse(_handTranslationMagnificationZInputField.text);
    }

    public void OnChangeSmoothingLevel() {
        _smoothingLevel = Mathf.Clamp(int.Parse(_smoothingLevelInputField.text), 1, MAX_SMOOTHING_LEVEL);
        _smoothingLevelInputField.text = _smoothingLevel.ToString();
    }
    
    public void OnChangeAutoAdjustment() {
        _autoAdjustmentRatio = Mathf.Clamp(float.Parse(_autoAdjustmentInputField.text), 0, 1.0f);
        _autoAdjustmentInputField.text = _autoAdjustmentRatio.ToString();
    }

    public void OnChangeAutoAdjustmentDelay() {
        _autoAdjustmentDelay = float.Parse(_autoAdjustmentDelayInputField.text);
        if (_autoAdjustmentDelay < 0) {
            _autoAdjustmentDelay = 0;
        }
        _autoAdjustmentDelayInputField.text = _autoAdjustmentDelay.ToString();
    }

    public void OnChangeVideoDeviceList() {
        SMT.destroy();
        _cameraStartButtonText.text = "Start";
        _deviceName = _selectedVideoDeviceName.text;
    }

    public void OnClickApplyCameraId() {
        _cameraStartButtonText.text = "Running";
        SMT.destroy();
        SMT.init(_selectedVideoDeviceName.text);
        SMT.setCaptureShown(_isCaptureShown);
        SMT.setUseARMarker(_useARMarker);
        SMT.setUseFaceTracking(_useFaceTracking);
        SMT.setUseEyeTracking(_useEyeTracking);
        SMT.setUseHandTracking(_useHandTracking);

        OnChangeHandMovingThresholdMin();
        OnChangeHandMovingThresholdMax();
        OnChangeHandUndetectedDuration();
    }

    public void OnChangeMirror() {
        if (_mirrorToggle.isOn) {
            _mirror = -1;
        } else {
            _mirror = 1;
        }
    }

    public void OnChangedOSCPort() {
        _port = int.Parse(_uOSCInputField.text);
        _head.GetComponent<HeadTrackerSender>().ChangePort(_port);
        _leftHand.GetComponent<TrackerSender>().ChangePort(_port);
        _rightHand.GetComponent<TrackerSender>().ChangePort(_port);
    }
    
    public void OnChangeCaptureShown() {
        _isCaptureShown = _showCaptureImageToggle.isOn;
        SMT.setCaptureShown(_isCaptureShown);
    }

    private void applyVRPlayAreaOffset() {
        _head.GetComponent<HeadTrackerSender>()._vrPlayAreaOffsetTranslation = _vrPlayAreaOffsetTranslation;
        _leftHand.GetComponent<TrackerSender>()._vrPlayAreaOffsetTranslation = _vrPlayAreaOffsetTranslation;
        _rightHand.GetComponent<TrackerSender>()._vrPlayAreaOffsetTranslation = _vrPlayAreaOffsetTranslation;

        _head.GetComponent<HeadTrackerSender>()._vrPlayAreaOffsetRotation = _vrPlayAreaOffsetRotation;
        _leftHand.GetComponent<TrackerSender>()._vrPlayAreaOffsetRotation = _vrPlayAreaOffsetRotation;
        _rightHand.GetComponent<TrackerSender>()._vrPlayAreaOffsetRotation = _vrPlayAreaOffsetRotation;
    }

    public void OnChangeVRPlayAreaOffsetTranslationX() {
        _vrPlayAreaOffsetTranslation.x = float.Parse(_vrPlayAreaOffsetTranslationXInputField.text);
        applyVRPlayAreaOffset();
    }

    public void OnChangeVRPlayAreaOffsetTranslationY() {
        _vrPlayAreaOffsetTranslation.y = float.Parse(_vrPlayAreaOffsetTranslationYInputField.text);
        applyVRPlayAreaOffset();
    }

    public void OnChangeVRPlayAreaOffsetTranslationZ() {
        _vrPlayAreaOffsetTranslation.z = float.Parse(_vrPlayAreaOffsetTranslationZInputField.text);
        applyVRPlayAreaOffset();
    }

    public void OnChangeVRPlayAreaOffsetRotationX() {
        _vrPlayAreaOffsetRotation.x = float.Parse(_vrPlayAreaOffsetRotationXInputField.text);
        applyVRPlayAreaOffset();
    }

    public void OnChangeVRPlayAreaOffsetRotationY() {
        _vrPlayAreaOffsetRotation.y = float.Parse(_vrPlayAreaOffsetRotationYInputField.text);
        applyVRPlayAreaOffset();
    }

    public void OnChangeVRPlayAreaOffsetRotationZ() {
        _vrPlayAreaOffsetRotation.z = float.Parse(_vrPlayAreaOffsetRotationZInputField.text);
        applyVRPlayAreaOffset();
    }

    public void OnClickSave() {
        var fileName = SMT.getSaveFileName();
        if (string.IsNullOrEmpty(fileName)) {
            return;
        }
        var p = new Parameter();
        p.deviceName = _deviceName;
        p.isForcedTPose = _isForcedTPose;
        //p.useARMarker = _useARMarker;
        p.useFaceTracking = _useFaceTracking;
        p.faceAngleBaseDistance = _faceAngleBaseDistance;
        p.translationMagnification = _translationMagnification;
        p.translationMagnifications = _translationMagnifications;
        p.rotationMagnification = _rotationMagnification;
        p.rotationMagnifications = _rotationMagnifications;
        p.useEyeTracking = _useEyeTracking;
        p.useEyesLRSync = _useEyesLRSync;
        p.useEyesBlink = _useEyesBlink;
        p.irisThreshold = _irisThreshold;
        p.irisOffset = _irisOffset;
        p.irisTranslationMagnifications = _irisTranslationMagnifications;
        p.maxEyeOpenThreshold = _maxEyeOpenThreshold;
        p.minEyeOpenThreshold = _minEyeOpenThreshold;
        p.useHandTracking = _useHandTracking;
        p.handMovingThresholdMin = float.Parse(_handMovingThresholdMinInputField.text);
        p.handMovingThresholdMax = float.Parse(_handMovingThresholdMaxInputField.text);
        p.handUndetectedDuration = float.Parse(_handUndetectedDurationInputField.text);
        p.handOffset = _handOffset;
        p.handTranslationMagnifications = _handTranslationMagnifications;
        p.smoothingLevel = _smoothingLevel;
        p.autoAdjustmentRatio = _autoAdjustmentRatio;
        p.autoAdjustmentDelay = _autoAdjustmentDelay;
        p.mirror = _mirror;
        p.port = _port;
        p.vrPlayAreaOffsetTranslation = _vrPlayAreaOffsetTranslation;
        p.vrPlayAreaOffsetRotation = _vrPlayAreaOffsetRotation;

        string json = JsonUtility.ToJson(p);
        File.WriteAllText(fileName, json);
    }

    public void OnClickLoad() {
        var fileName = SMT.getOpenFileName();
        if (string.IsNullOrEmpty(fileName)) {
            return;
        }
        string json = File.ReadAllText(fileName);
        Parameter p = JsonUtility.FromJson<Parameter>(json);

        _forceTPoseToggle.isOn = p.isForcedTPose;
        // = p.useARMarker;
        _useFaceTrackingToggle.isOn = p.useFaceTracking;
        _faceAngleBaseDistanceInputField.text = p.faceAngleBaseDistance.ToString();
        _translationMagnificationInputField.text = p.translationMagnification.ToString();
        _translationMagnificationXInputField.text = p.translationMagnifications.x.ToString();
        _translationMagnificationYInputField.text = p.translationMagnifications.y.ToString();
        _translationMagnificationZInputField.text = p.translationMagnifications.z.ToString();
        _rotationMagnificationInputField.text = p.rotationMagnification.ToString();
        _rotationMagnificationXInputField.text = p.rotationMagnifications.x.ToString();
        _rotationMagnificationYInputField.text = p.rotationMagnifications.y.ToString();
        _rotationMagnificationZInputField.text = p.rotationMagnifications.z.ToString();
        _useEyeTrackingToggle.isOn = p.useEyeTracking;
        _useEyeLRSyncToggle.isOn = p.useEyesLRSync;
        _useEyeBlinkToggle.isOn = p.useEyesBlink;
        _irisThresholdInputField.text = p.irisThreshold.ToString();
        _irisOffsetXInputField.text = p.irisOffset.x.ToString();
        _irisOffsetYInputField.text = p.irisOffset.y.ToString();
        _irisTranslationMagnificationXInputField.text = p.irisTranslationMagnifications.x.ToString();
        _irisTranslationMagnificationYInputField.text = p.irisTranslationMagnifications.y.ToString();
        _eyeOpenThresholdMaxInputField.text = p.maxEyeOpenThreshold.ToString();
        _eyeOpenThresholdMinInputField.text = p.minEyeOpenThreshold.ToString();
        _useHandTrackingToggle.isOn = p.useHandTracking;
        _handMovingThresholdMinInputField.text = p.handMovingThresholdMin.ToString();
        _handMovingThresholdMaxInputField.text = p.handMovingThresholdMax.ToString();
        _handUndetectedDurationInputField.text = p.handUndetectedDuration.ToString();
        _handOffsetXInputField.text = p.handOffset.x.ToString();
        _handOffsetYInputField.text = p.handOffset.y.ToString();
        _handOffsetZInputField.text = p.handOffset.z.ToString();
        _handTranslationMagnificationXInputField.text = p.handTranslationMagnifications.x.ToString();
        _handTranslationMagnificationYInputField.text = p.handTranslationMagnifications.y.ToString();
        _handTranslationMagnificationZInputField.text = p.handTranslationMagnifications.z.ToString();
        _smoothingLevelInputField.text = p.smoothingLevel.ToString();
        _autoAdjustmentInputField.text = p.autoAdjustmentRatio.ToString();
        _autoAdjustmentDelayInputField.text = p.autoAdjustmentDelay.ToString();
        _mirrorToggle.isOn = (p.mirror == -1);
        _uOSCInputField.text = p.port.ToString();
        _vrPlayAreaOffsetTranslationXInputField.text = p.vrPlayAreaOffsetTranslation.x.ToString();
        _vrPlayAreaOffsetTranslationYInputField.text = p.vrPlayAreaOffsetTranslation.y.ToString();
        _vrPlayAreaOffsetTranslationZInputField.text = p.vrPlayAreaOffsetTranslation.z.ToString();
        _vrPlayAreaOffsetRotationXInputField.text = p.vrPlayAreaOffsetRotation.x.ToString();
        _vrPlayAreaOffsetRotationYInputField.text = p.vrPlayAreaOffsetRotation.y.ToString();
        _vrPlayAreaOffsetRotationZInputField.text = p.vrPlayAreaOffsetRotation.z.ToString();

        int i = 0;
        foreach (var item in _videoDeviceList.options) {
            if (item.text == p.deviceName) {
                _videoDeviceList.value = i;
                _deviceName = p.deviceName;
                break;
            }
            i++;
        }

        refreshUI();
    }

    void refreshUI() {
        OnUseFaceTracking();
        OnChangeForceTPose();
        OnChangeFaceAngleBaseDistance();
        OnChangeTranslationMagnification();
        OnChangeTranslationMagnificationX();
        OnChangeTranslationMagnificationY();
        OnChangeTranslationMagnificationZ();
        OnChangeRotationMagnification();
        OnChangeRotationMagnificationX();
        OnChangeRotationMagnificationY();
        OnChangeRotationMagnificationZ();
        //OnClickCalibrateFacePoints();
        OnChangeUseEyeTracking();
        OnChangeUseBlink();
        OnChangeLRSync();
        OnChangeIrisThreshold();
        OnChangeIrisOffsetX();
        OnChangeIrisOffsetY();
        OnChangeIrisTranslationMagnificationX();
        OnChangeIrisTranslationMagnificationY();
        OnChangeEyeOpenThresholdMin();
        OnChangeEyeOpenThresholdMax();
        OnChangeUseHandTracking();
        OnChangeHandMovingThresholdMin();
        OnChangeHandMovingThresholdMax();
        OnChangeHandUndetectedDuration();
        OnChangeHandOffsetX();
        OnChangeHandOffsetY();
        OnChangeHandOffsetZ();
        OnChangeHandTranslationMagnificationX();
        OnChangeHandTranslationMagnificationY();
        OnChangeHandTranslationMagnificationZ();
        OnChangeSmoothingLevel();
        OnChangeUseHandTracking();
        OnChangeAutoAdjustment();
        OnChangeAutoAdjustmentDelay();
        OnChangeVideoDeviceList();
        OnChangeMirror();
        OnChangedOSCPort();
        OnChangeVRPlayAreaOffsetTranslationX();
        OnChangeVRPlayAreaOffsetTranslationY();
        OnChangeVRPlayAreaOffsetTranslationZ();
        OnChangeVRPlayAreaOffsetRotationX();
        OnChangeVRPlayAreaOffsetRotationY();
        OnChangeVRPlayAreaOffsetRotationZ();
    }

    Vector3 halfAngleVector3(Vector3 eulerAngles) {
        var a = new Vector3(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        if (a.x > 180) a.x = a.x - 360;
        if (a.y > 180) a.y = a.y - 360;
        if (a.z > 180) a.z = a.z - 360;
        return a;
    }

    void smoothTransform(ref Vector3 t, ref Vector3 r) {
        _facePositionList.RemoveAt(_facePositionList.Count - 1);
        _facePositionList.Insert(0, t);

        _faceRotationList.RemoveAt(_faceRotationList.Count - 1);
        _faceRotationList.Insert(0, r);

        t.Set(0, 0, 0);
        r.Set(0, 0, 0);
        for (int i = 0; i < _smoothingLevel; i++) {
            t = t + _facePositionList[i];
            r = r + _faceRotationList[i];
        }
        t = t / _smoothingLevel;
        r = r / _smoothingLevel;
    }

    void smooth(ref Vector3 v, List<Vector3> list, int length) {
        list.RemoveAt(list.Count - 1);
        list.Insert(0, v);

        v.Set(0, 0, 0);
        for (int i = 0; i < length; i++) {
            v = v + list[i];
        }
        v = v / length;
    }

    // Use this for initialization
    void Start() {
        WebCamDevice[] webCamDevice;
        webCamDevice = WebCamTexture.devices;
        for (int i = 0; i < webCamDevice.Length; i++) {
            var device = webCamDevice[i];
            //Debug.Log("id=" + i + ", name=" + device.name);
            _videoDeviceList.options.Add(new Dropdown.OptionData(device.name));
        }

        //SMT.init(_cameraId);
        SMT.setCaptureShown(false);
        SMT.setARMarkerEdgeLength(0.036f);

        SMT.setUseARMarker(_useARMarker);
        SMT.setUseFaceTracking(_useFaceTracking);
        SMT.setUseHandTracking(_useHandTracking);

        _calibratedFacePosition = Vector3.zero;
        _calibratedFaceAngle = Vector3.zero;
        _calibratedLeftEyePosition = Vector3.zero;
        _calibratedRigthEyePosition = Vector3.zero;

        _forceTPoseToggle.isOn = false;
        _useFaceTrackingToggle.isOn = false;
        _faceAngleBaseDistanceInputField.text = "0.2";
        _translationMagnificationInputField.text = "1.0";
        _translationMagnificationXInputField.text = "1.0";
        _translationMagnificationYInputField.text = "1.0";
        _translationMagnificationZInputField.text = "1.0";
        _rotationMagnificationInputField.text = "1.0";
        _rotationMagnificationXInputField.text = "1.0";
        _rotationMagnificationYInputField.text = "1.0";
        _rotationMagnificationZInputField.text = "1.0";

        _useEyeTrackingToggle.isOn = false;
        _useEyeBlinkToggle.isOn = true;
        _useEyeLRSyncToggle.isOn = true;
        _irisThresholdInputField.text = "30";
        _irisOffsetXInputField.text = "0";
        _irisOffsetYInputField.text = "0";
        _irisTranslationMagnificationXInputField.text = "2.0";
        _irisTranslationMagnificationYInputField.text = "2.0";
        _eyeOpenThresholdMinInputField.text = "0.3";
        _eyeOpenThresholdMaxInputField.text = "0.6";

        _useHandTrackingToggle.isOn = false;
        _handMovingThresholdMinInputField.text = "0.05";
        _handMovingThresholdMaxInputField.text = "3.0";
        _handUndetectedDurationInputField.text = "5.0";
        _handOffsetXInputField.text = "0.0";
        _handOffsetYInputField.text = "0.0";
        _handOffsetZInputField.text = "-0.30";
        _handTranslationMagnificationXInputField.text = "1.0";
        _handTranslationMagnificationYInputField.text = "1.0";
        _handTranslationMagnificationZInputField.text = "0.3";
        _leftHandStandardPoseMergeDelayStartTime = Time.time;
        _rightHandStandardPoseMergeDelayStartTime = Time.time;

        _smoothingLevelInputField.text = "5";

        _autoAdjustmentInputField.text = "0.05";
        _autoAdjustmentDelayInputField.text = "0.5";
        _autoAdjustmentDelayStartTime = Time.time;
        _mirrorToggle.isOn = false;

        _uOSCInputField.text = "39540";

        _vrPlayAreaOffsetTranslationXInputField.text = "0";
        _vrPlayAreaOffsetTranslationYInputField.text = "0";
        _vrPlayAreaOffsetTranslationZInputField.text = "0";
        _vrPlayAreaOffsetRotationXInputField.text = "0";
        _vrPlayAreaOffsetRotationYInputField.text = "0";
        _vrPlayAreaOffsetRotationZInputField.text = "0";

        refreshUI();

        _facePositionList = new List<Vector3>();
        _faceRotationList = new List<Vector3>();
        for (int i = 0; i < MAX_SMOOTHING_LEVEL; i++) {
            _facePositionList.Add(Vector3.zero);
            _faceRotationList.Add(Vector3.zero);
        }

        _leftIrisList = new List<Vector3>();
        _rightIrisList = new List<Vector3>();
        for (int i = 0; i < MAX_IRIS_SMOOTHING; i++) {
            _leftIrisList.Add(Vector3.zero);
            _rightIrisList.Add(Vector3.zero);
        }

        _leftEyeList = new List<Vector3>();
        _rightEyeList = new List<Vector3>();
        for (int i = 0; i < MAX_EYE_SMOOTHING; i++) {
            _leftEyeList.Add(Vector3.zero);
            _rightEyeList.Add(Vector3.zero);
        }

        _leftHandPositionAve = Vector3.zero;
        _rightHandPositionAve = Vector3.zero;
        _leftHandList = new List<Vector3>();
        _rightHandList = new List<Vector3>();
        for (int i = 0; i < MAX_HAND_SMOOTHING; i++) {
            _leftHandList.Add(Vector3.zero);
            _rightHandList.Add(Vector3.zero);
        }
    }

    void OnDestroy() {
        SMT.destroy();
    }

    void trackingHeadARMarker() {
        Vector3 t = Vector3.zero;
        Vector3 r = Vector3.zero;
        if (SMT.isARMarkerDetected(0)) {
            SMT.getARMarker6DoF(0, out t, out r);
        }

        Vector3 t2 = Vector3.zero;
        Vector3 r2 = Vector3.zero;
        if (SMT.isARMarkerDetected(1)) {
            SMT.getARMarker6DoF(1, out t2, out r2);
        }

        // Z Axis Flipping 対策
        if (SMT.isARMarkerDetected(0) && SMT.isARMarkerDetected(1)) {
            var f1 = Vector3.forward;
            var q = Quaternion.Euler(r);
            f1 = q * f1;

            var f2 = Vector3.forward;
            q = Quaternion.Euler(r2);
            f2 = q * f2;

            float dot = Vector3.Dot(f1, f2);
            if (dot < 0.75f) {
                //return; // サブマーカーの作り方、角度によって変わる
            }
        }

        if (SMT.isARMarkerDetected(0)) {
            //var tempSmoothingLevel = _smoothingLevel;
            float ignoreAngle = 30;
            if (-ignoreAngle < r.x && r.x < ignoreAngle &&
                -ignoreAngle < r.y && r.y < ignoreAngle) {
                if (SMT.isARMarkerDetected(1)) {
                    if (r.y < r2.y) {
                        return; // Z Axis Flipping
                    }

                    r = r2;
                    var subMarkerAdjustmentAngle = 50.0f;
                    r.y = r.y + subMarkerAdjustmentAngle; // 横並びのサブマーカーの角度を補正する
                    r.z = r.z + (subMarkerAdjustmentAngle * Mathf.Sin(r.x * Mathf.Deg2Rad)); // Yの角度が異なるとX軸で上下に振った場合Zの補正が必要
                }
                //_smoothingLevel = 10;
            }
            else {
                // Z Axis Flippingの確認
                // パネル正面向きのVecをZ軸で180度回転させて内積をとる.
                var currentEulerAngle = _head.transform.eulerAngles;
                // 現在の向きをZ軸で反転させた正面ベクトルを作る
                var cq = Quaternion.Euler(0, 0, 180) * Quaternion.Euler(currentEulerAngle);
                var zAxisFlipF = cq * Vector3.forward;
                // 新しい向きの正面ベクトル
                var f = cq * Vector3.forward;
                cq = Quaternion.Euler(r);
                f = cq * f;

                float dot = Vector3.Dot(f, zAxisFlipF);
                if (dot > 0.75f) {
                    if (_zAxisFlipFrameCount < 2) { // 一定フレーム数以上その姿勢が続く場合は誤検出の可能性が高い
                        _zAxisFlipFrameCount++;
                        return;
                    }
                }
            }
            _zAxisFlipFrameCount = 0;

            // マーカーの奥方向(+Z)に移動させる
            t = t + ((Quaternion.Euler(r) * Vector3.forward) * 0.1f); // 順番大事、クオータニオンを先に置かないとエラーになる

            smoothTransform(ref t, ref r);
            //_smoothingLevel = tempSmoothingLevel;

            _head.transform.rotation = Quaternion.Euler(r);
            _head.transform.position = t;

        }
    }

    void calcStandardHandPose(out Vector3 leftPos, out Quaternion leftRot, out Vector3 rightPos, out Quaternion rightRot, out Quaternion headRot) {
        // 頭に合わせて腕の位置を調整
        var headRotationEuler = halfAngleVector3(_head.transform.eulerAngles);
        headRotationEuler.x = headRotationEuler.x / 3;
        headRotationEuler.y = headRotationEuler.y / 2;
        headRotationEuler.z = headRotationEuler.z / 3;
        headRot = Quaternion.Euler(headRotationEuler);
        // 左手
        var leftOffset = new Vector3(0.4f, -0.85f, -0.2f);
        leftOffset = headRot * leftOffset;
        leftPos = _head.transform.position + leftOffset;
        leftRot = headRot * Quaternion.Euler(-25, 120, -107);

        // 右手
        var rightOffset = new Vector3(-0.4f, -0.85f, -0.2f);
        rightOffset = headRot * rightOffset;
        rightPos = _head.transform.position + rightOffset;
        rightRot = headRot * Quaternion.Euler(-25, -120, 107);
    }

    void setStandardHandPose() {
        Vector3 leftPos;
        Quaternion leftRot;
        Vector3 rightPos;
        Quaternion rightRot;
        Quaternion headRot;
        calcStandardHandPose(out leftPos, out leftRot, out rightPos, out rightRot, out headRot);

        _leftHand.transform.position = leftPos;
        _leftHand.transform.rotation = leftRot;

        _rightHand.transform.position = rightPos;
        _rightHand.transform.rotation = rightRot;
    }

    void trackingEyes(Vector3 eye, List<Vector3> eyeList, 
                      Vector3 iris, List<Vector3> irisList, 
                      GameObject go, float headRotationZ, out Vector3 lookAt) {
        // 目の開閉を計算
        smooth(ref eye, eyeList, MAX_EYE_SMOOTHING);
        smooth(ref iris, irisList, MAX_IRIS_SMOOTHING);
        float open = iris.z;
        open = Mathf.Clamp(open, 0, 1.0f);
        if (open > _maxEyeOpenThreshold) {
            open = _maxEyeOpenThreshold;
        }
        else if (open < _minEyeOpenThreshold) {
            open = _minEyeOpenThreshold;
        }
        open -= _minEyeOpenThreshold;
        var range = _maxEyeOpenThreshold - _minEyeOpenThreshold;
        if (range <= 0) {
            range = 0.0001f;
        }
        open = open / (_maxEyeOpenThreshold - _minEyeOpenThreshold);
        go.transform.localScale = new Vector3(1, 1, open);

        // 虹彩の移動量と注視点を計算
        lookAt = Vector3.zero;
        if (open > 0) {
            // 目の中心から虹彩に向けたベクトル
            var dir = iris - eye;
            dir.z = 0;
            // 頭の回転分だけ補正
            dir = Quaternion.Euler(0, 0, headRotationZ) * dir;
            // 目の範囲内でどれくらい移動しているか
            var movingRatio = dir.magnitude / eye.z;
            movingRatio = Mathf.Clamp(movingRatio, -1, 1);
            // 向きベクトルの長さを移動割合に補正
            dir = dir.normalized * movingRatio;
            dir.x *= _mirror;
            go.transform.localPosition = new Vector3(dir.x, 0, -dir.y) * 10;

            lookAt.x = dir.x; // メモ：頭の位置から相対座標で視線が決まるはずだが玉とVMCでXの向きが逆になる。ここでは玉の向きを優先してVMCに送信するときに*-1する
            lookAt.y = -dir.y;
            lookAt.z = -1;
        }
    }

    void trackingFacePoints() {
        if (SMT.isFacePointsDetected()) {
            Vector3 face;
            Vector3 faceAngle;
            Vector3 leftEye;
            Vector3 rightEye;
            Vector3 leftIris;
            Vector3 rightIris;
            SMT.getFacePoints(out face, out faceAngle, out leftEye, out rightEye, out leftIris, out rightIris);

            // 目の間の距離
            var eyeLtoR = rightEye - leftEye;
            eyeLtoR.z = 0;
            //float eyeDistance = eyeLtoR.magnitude;
            var faceLength = face.z * 2; // 顏の直径
            var standardLength = 0.15f; // 15cm
            var pixParM = standardLength / faceLength;

            var facePosition = face - _calibratedFacePosition;
            facePosition.y *= -1;
            facePosition *= pixParM;

            if (_calibratedFacePosition.z < face.z) {
                facePosition.z = -1 + (_calibratedFacePosition.z / face.z);
            }
            else {
                facePosition.z = 0;
            }

            var faceRotationEuler = faceAngle - _calibratedFaceAngle;

            // 倍率適用
            facePosition = facePosition * _translationMagnification;
            facePosition.x *= _translationMagnifications.x * _mirror;
            facePosition.y *= _translationMagnifications.y;
            facePosition.z *= _translationMagnifications.z;

            faceRotationEuler = faceRotationEuler * _rotationMagnification;
            faceRotationEuler.x *= _rotationMagnifications.x;
            faceRotationEuler.y *= _rotationMagnifications.y * _mirror;
            faceRotationEuler.z *= _rotationMagnifications.z * _mirror;
            if (faceRotationEuler.x < -75) faceRotationEuler.x = -75;
            if (faceRotationEuler.x > 75) faceRotationEuler.x = 75;
            if (faceRotationEuler.z < -75) faceRotationEuler.z = -75;
            if (faceRotationEuler.z > 75) faceRotationEuler.z = 75;
            
            // オフセット適用
            facePosition.y += 1.6f;

            // スムージング
            smooth(ref faceRotationEuler, _faceRotationList, _smoothingLevel);
            smooth(ref facePosition, _facePositionList, _smoothingLevel);

            var faceRotation = Quaternion.Euler(faceRotationEuler);
            if (_useFaceTracking) {
                _head.transform.position = facePosition;
                _head.transform.rotation = faceRotation;
            }

            // 顏の中央位置を自動調整
            if (_autoAdjustmentRatio > 0) {
                // 一定角度内は常に調整、角度が大きい場合はDelayだけ待ってから調整開始
                bool isNarrowAngle = (Mathf.Abs(faceRotationEuler.x) < 5 && Mathf.Abs(faceRotationEuler.y) < 5);
                bool isExceededWaitingTime = ((Time.time - _autoAdjustmentDelayStartTime) > _autoAdjustmentDelay);
                if (isNarrowAngle || isExceededWaitingTime) {
                    var z = _calibratedFacePosition.z; // Zは維持、XYのみずれやすいので調整する
                    _calibratedFacePosition = (_calibratedFacePosition * (1.0f - _autoAdjustmentRatio)) + (face * _autoAdjustmentRatio);
                    _calibratedFacePosition.z = z;
                }

                // 正面を向いた時にオートアジャストの発動カウントをリセット
                if (isNarrowAngle) {
                    _autoAdjustmentDelayStartTime = Time.time;
                }
            }


            // アイトラキング(虹彩追跡)
            if (_useEyeTracking) {
                float headRotationZ = -Mathf.Atan(eyeLtoR.y / eyeLtoR.x) * Mathf.Rad2Deg;

                Vector3 leftLookAt;
                trackingEyes(leftEye, _leftEyeList, leftIris, _leftIrisList, _leftIris, headRotationZ, out leftLookAt);
                _head.GetComponent<HeadTrackerSender>().LeftIrisBlendShapeValue = 1.0f - _leftIris.transform.localScale.z;

                Vector3 rightLookAt;
                trackingEyes(rightEye, _rightEyeList, rightIris, _rightIrisList, _rightIris, headRotationZ, out rightLookAt);
                _head.GetComponent<HeadTrackerSender>().RightIrisBlendShapeValue = 1.0f - _rightIris.transform.localScale.z;

                var hts = _head.GetComponent<HeadTrackerSender>();
                if (_mirror == -1) {
                    var tmp = hts.LeftIrisBlendShapeValue;
                    hts.LeftIrisBlendShapeValue = hts.RightIrisBlendShapeValue;
                    hts.RightIrisBlendShapeValue = tmp;
                }
                if (_useEyesLRSync) {
                    var eyeOpenAve = (hts.LeftIrisBlendShapeValue + hts.RightIrisBlendShapeValue) / 2;
                    hts.LeftIrisBlendShapeValue = eyeOpenAve;
                    hts.RightIrisBlendShapeValue = eyeOpenAve;
                }

                Vector3 lookAt = leftLookAt + rightLookAt;
                if (lookAt.z > 0) {
                    lookAt = lookAt / Mathf.Abs(lookAt.z); // 注視点は-1なので-2になっていたら平均値を取る
                 } else {
                    lookAt.z = -1;
                }
                lookAt.x += _irisOffset.x;
                lookAt.y += _irisOffset.y;
                lookAt.x *= _irisTranslationMagnifications.x;
                lookAt.y *= _irisTranslationMagnifications.y;
                lookAt.y += 0.33f;
                _lookAt.transform.localPosition = lookAt;
            }
        }
    }

    Quaternion calcRotationAffectedByControlPoints(Vector3 p, Quaternion currentRotation, GameObject cp, Axis axis) {
        float from = 0;
        float to = 0;
        switch (axis) {
            case Axis.X:
                from = p.x;
                to = cp.transform.position.x;
                break;
            case Axis.Y:
                from = p.y;
                to = cp.transform.position.y;
                break;
            case Axis.Z:
                from = p.z;
                to = cp.transform.position.z;
                break;
        }

        var targetRotation = cp.transform.rotation;

        // 制御点との距離で影響度を計算
        var len = Mathf.Abs(to - from);
        var influence = len / cp.transform.localScale.z; // スケール値を分母(球の半径)にする
        if (influence > 1.0f) { influence = 1.0f; }
        return Quaternion.Lerp(targetRotation, currentRotation, influence);
    }

    void calcHandPosture(Vector3 handCircle, GameObject go, GameObject handPropes,  ref Vector3 positionAve, ref List<Vector3> handList, float handOffsetX) {
        // 手の半径は顏の半分 の更に半分。動体検知では平均を取るため。
        var calibratedHandPosition = new Vector3(_calibratedFacePosition.x, _calibratedFacePosition.y, _calibratedFacePosition.z * 0.25f);

        if (calibratedHandPosition.z == 0) { return; }
        if (handCircle.z == 0) { return; }
        
        var handLength = handCircle.z * 2; // 手の直径
        var standardLength = 0.06f; // 6cm
        var pixParM = standardLength / handLength;
        var handPosition = handCircle - calibratedHandPosition;
        handPosition.y *= -1;
        handPosition *= pixParM;

        if (calibratedHandPosition.z < handCircle.z) {
            var radiusRatio = handCircle.z / (calibratedHandPosition.z + 1);
            var distance = (radiusRatio - 1) * 0.6f; // ゼロ基準してから 0～60cm
            handPosition.z = -distance;
        }
        else {
            handPosition.z = 0;
        }

        // 大きく動かしてみるテスト
        handPosition.x *= _handTranslationMagnifications.x * _mirror;
        handPosition.y *= _handTranslationMagnifications.y;
        handPosition.z *= _handTranslationMagnifications.z;

        // オフセット適用
        handPosition.x += handOffsetX;
        handPosition.y += _handOffset.y + 1.6f;
        handPosition.z += _handOffset.z + _head.transform.position.z; 

        // スムージング
        //float smoothingRatio = 1.0f / _handSmoothingLevel;
        //float radiusSmoothigRatio = 1.0f / (_handSmoothingLevel * 2);
        float smoothingRatio = 1.0f / _smoothingLevel;
        float radiusSmoothigRatio = 1.0f / (_smoothingLevel * 2);
        positionAve.x = (positionAve.x * (1.0f - smoothingRatio)) + (handPosition.x * smoothingRatio);
        positionAve.y = (positionAve.y * (1.0f - smoothingRatio)) + (handPosition.y * smoothingRatio);
        positionAve.z = (positionAve.z * (1.0f - radiusSmoothigRatio)) + (handPosition.z * radiusSmoothigRatio);
        go.transform.position = positionAve;

        // スムージング2
        //smooth(ref handPosition, handList, _handSmoothingLevel);
        //go.transform.position = handPosition;

        // 手の向きを計算
        handPropes.transform.position = _head.transform.position;
        var C = handPropes.transform.Find("C").gameObject;
        var F = handPropes.transform.Find("F").gameObject;
        var O = handPropes.transform.Find("O").gameObject;
        var T = handPropes.transform.Find("T").gameObject;
        var B = handPropes.transform.Find("B").gameObject;

        var p = go.transform.position;
        var rot = C.transform.rotation;
        rot = calcRotationAffectedByControlPoints(p, rot, F, Axis.Z);
        rot = calcRotationAffectedByControlPoints(p, rot, O, Axis.X);
        rot = calcRotationAffectedByControlPoints(p, rot, T, Axis.Y);
        rot = calcRotationAffectedByControlPoints(p, rot, B, Axis.Y);

        go.transform.rotation = rot;
        
    }

    void trackingHandPoints() {
        Vector3 leftCircle;
        Vector3 rightCircle;
        SMT.getHandPoints(out leftCircle, out rightCircle);
        bool isLeftHandDetected = SMT.isLeftHandDetected();
        bool isRightHandDetected = SMT.isRightHandDetected();
        bool isLeftHandDown = SMT.isLeftHandDown();
        bool isRightHandDown = SMT.isRightHandDown();

        // 動体を見失った時に元の姿勢に戻す為の値
        Vector3 leftStandardPos;
        Quaternion leftStandardRot;
        Vector3 rightStandardPos;
        Quaternion rightStandardRot;
        Quaternion headRot;
        calcStandardHandPose(out leftStandardPos, out leftStandardRot, out rightStandardPos, out rightStandardRot, out headRot);
        float ratioAcceleration = 0.005f;

        if (_mirror == -1) {
            for (int i = 0; i < 3; i++) {
                float temp = leftCircle[i];
                leftCircle[i] = rightCircle[i];
                rightCircle[i] = temp;
            }
            var detected = isLeftHandDetected;
            isLeftHandDetected = isRightHandDetected;
            isRightHandDetected = detected;

            var down = isLeftHandDown;
            isLeftHandDown = isRightHandDown;
            isRightHandDown = down;
        }

        if (isLeftHandDetected || _leftHandSmoothingDelay < _smoothingLevel) {
            // 非検知の場合でもスムージングのフレーム数だけ更新を続ける
            if (isRightHandDetected) { _leftHandSmoothingDelay = 0; }
            else { _leftHandSmoothingDelay += 1; }

            calcHandPosture(leftCircle, _leftHand, _leftHandPropes, ref _leftHandPositionAve, ref _leftHandList, _handOffset.x);
            _leftHandStandardPoseMergeRatio = 0;
            _leftHandStandardPoseMergeDelayStartTime = Time.time;
            _leftHandPositionBackup = _leftHand.transform.position - _head.transform.position;
        }
        else {
            if (isLeftHandDown) {
                _leftHandStandardPoseMergeDelayStartTime -= _handStandardPoseMergeDelay;
                if (_leftHandStandardPoseMergeDelayStartTime < 0) { _leftHandStandardPoseMergeDelayStartTime = 0; }
                if (_leftHandStandardPoseMergeRatio < 0.1f) { _leftHandStandardPoseMergeRatio = 0.1f; }
            }
            if (Time.time - _leftHandStandardPoseMergeDelayStartTime > _handStandardPoseMergeDelay) {
                var pos = _leftHand.transform.position;
                var rot = _leftHand.transform.rotation;
                _leftHand.transform.position = (pos * (1.0f - _leftHandStandardPoseMergeRatio)) + leftStandardPos * _leftHandStandardPoseMergeRatio;
                _leftHand.transform.rotation = Quaternion.Lerp(rot, leftStandardRot, _leftHandStandardPoseMergeRatio);
                _leftHandStandardPoseMergeRatio += ratioAcceleration;
                if (_leftHandStandardPoseMergeRatio > 1) { _leftHandStandardPoseMergeRatio = 1; }
            }
            else {
                _leftHand.transform.position = (headRot * _leftHandPositionBackup) + _head.transform.position;
            }
        }


        if (isRightHandDetected || _rightHandSmoothingDelay < _smoothingLevel) {
            // 非検知の場合でもスムージングのフレーム数だけ更新を続ける
            if (isRightHandDetected) { _rightHandSmoothingDelay = 0; }
            else { _rightHandSmoothingDelay += 1; }

            calcHandPosture(rightCircle, _rightHand, _rightHandPropes, ref _rightHandPositionAve, ref _rightHandList, -_handOffset.x);
            _rightHandStandardPoseMergeRatio = 0;
            _rightHandStandardPoseMergeDelayStartTime = Time.time;
            _rightHandPositionBackup = _rightHand.transform.position - _head.transform.position;
        }
        else {
            if (isRightHandDown) {
                _rightHandStandardPoseMergeDelayStartTime -= _handStandardPoseMergeDelay;
                if (_rightHandStandardPoseMergeDelayStartTime < 0) { _rightHandStandardPoseMergeDelayStartTime = 0; }
                if (_rightHandStandardPoseMergeRatio < 0.1f) { _rightHandStandardPoseMergeRatio = 0.1f; }
            }
            if (Time.time - _rightHandStandardPoseMergeDelayStartTime > _handStandardPoseMergeDelay) {
                var pos = _rightHand.transform.position;
                var rot = _rightHand.transform.rotation;
                _rightHand.transform.position = (pos * (1.0f - _rightHandStandardPoseMergeRatio)) + rightStandardPos * _rightHandStandardPoseMergeRatio;
                _rightHand.transform.rotation = Quaternion.Lerp(rot, rightStandardRot, _rightHandStandardPoseMergeRatio);
                _rightHandStandardPoseMergeRatio += ratioAcceleration;
                if (_rightHandStandardPoseMergeRatio > 1) { _rightHandStandardPoseMergeRatio = 1; }
            }
            else {
                _rightHand.transform.position = (headRot * _rightHandPositionBackup) + _head.transform.position;
            }
        }
    }

    // Update is called once per frame
    void Update() {
        // バモキャキャリブレーション用に現在の頭の位置を基準にTポーズで固定
        if (_isForcedTPose) {
            // 左手
            var leftOffset = new Vector3(0.8f, -0.2f, 0.0f);
            leftOffset = _head.transform.rotation * leftOffset;
            _leftHand.transform.position = _head.transform.position + leftOffset;
            _leftHand.transform.rotation = _head.transform.rotation;

            _calibratedLeftHandPosition = _leftHand.transform.position;
            _calibratedLeftHandRotation = _leftHand.transform.rotation;

            // 右手
            var rightOffset = new Vector3(-0.8f, -0.2f, 0.0f);
            rightOffset = _head.transform.rotation * rightOffset;
            _rightHand.transform.position = _head.transform.position + rightOffset;
            _rightHand.transform.rotation = _head.transform.rotation;

            _calibratedRightHandPosition = _rightHand.transform.position;
            _calibratedRightHandRotation = _rightHand.transform.rotation;
            return;
        }

        SMT.update();
        if (_useARMarker) {
            trackingHeadARMarker();
        }
        if (_useFaceTracking || _useEyeTracking) {
            trackingFacePoints();
        }
        if (_useHandTracking) {
            trackingHandPoints();
        }
        else {
            setStandardHandPose();
        }
    }
}
