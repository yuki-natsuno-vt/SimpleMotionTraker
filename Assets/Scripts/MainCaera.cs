using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] InputField _uOSCInputField;
    [SerializeField] InputField _faceAngleBaseDistanceInputField;
    [SerializeField] InputField _translationMagnificationInputField;
    [SerializeField] InputField _rotationMagnificationInputField;
    [SerializeField] InputField _smoothingLevelInputField;
    [SerializeField] InputField _autoAdjustmentInputField;
    [SerializeField] Dropdown _videoDeviceList;
    [SerializeField] Text _selectedVideoDeviceName;
    [SerializeField] Text _cameraStartButtonText;

    int _zAxisFlipFrameCount = 0; // フリップ発生フレーム数.

    int _cameraId = 0;

    bool _isCaptureShown = false;
    bool _isForcedTPose = false;
    bool _useARMarker = false;
    bool _useFaceTracking = false;
    bool _useEyeTracking = false;

    float _faceAngleBaseDistance = 0.20f;
    float _translationMagnification = 1.0f;
    float _rotationMagnification = 1.0f;

    Vector3 _calibratedLeftHandPosition;
    Quaternion _calibratedLeftHandRotation;
    Vector3 _calibratedRightHandPosition;
    Quaternion _calibratedRightHandRotation;


    Vector3 _calibratedFacePosition;
    Vector3 _calibratedLeftEyePosition;
    Vector3 _calibratedRigthEyePosition;

    const int MAX_SMOOTHING_LEVEL = 30;
    int _smoothingLevel = 3;
    List<Vector3> _facePositionList;
    List<Vector3> _faceRotationList;

    float _autoAdjustmentRatio = 0.0f;


    bool _useEyesLRBlinkSynced = true;
    float _maxEyeOpenThreshold = 0.41f;
    float _minEyeOpenThreshold = 0.3f;
    float _irisTranslationScale = 2.0f;

    const int MAX_EYE_SMOOTHING = 20;
    List<Vector3> _leftEyeList;
    List<Vector3> _rightEyeList;

    const int MAX_IRIS_SMOOTHING = 5;
    List<Vector3> _leftIrisList;
    List<Vector3> _rightIrisList;


    public void OnChangeVideoDeviceList() {
        SMT.destroy();
        _cameraStartButtonText.text = "Start";
    }

    public void OnClickApplyCameraId() {
        _cameraStartButtonText.text = "Running";
        SMT.destroy();
        SMT.init(_selectedVideoDeviceName.text);
        SMT.setCaptureShown(_isCaptureShown);
        SMT.setUseARMarker(_useARMarker);
        SMT.setUseFaceTracking(_useFaceTracking);
        SMT.setUseEyeTracking(_useEyeTracking);
    }

    public void OnChangedOSCPort() {
        var port = int.Parse(_uOSCInputField.text);
        _head.GetComponent<TrackerSender>().ChangePort(port);
        _leftHand.GetComponent<TrackerSender>().ChangePort(port);
        _rightHand.GetComponent<TrackerSender>().ChangePort(port);
        _leftIris.GetComponent<TrackerSender>().ChangePort(port);
        _rightIris.GetComponent<TrackerSender>().ChangePort(port);
    }

    public void OnChangeCaptureShown() {
        _isCaptureShown = !_isCaptureShown;
        SMT.setCaptureShown(_isCaptureShown);
    }
    
    public void OnUseARMarker() {
        _useARMarker = !_useARMarker;
        SMT.setUseARMarker(_useARMarker);
    }

    public void OnUseFaceTracking() {
        _useFaceTracking = !_useFaceTracking;
        SMT.setUseFaceTracking(_useFaceTracking);
    }

    public void OnUseEyeTracking() {
        _useEyeTracking = !_useEyeTracking;
        SMT.setUseEyeTracking(_useEyeTracking);
    }

    public void OnChangeForceTPose() {
        _isForcedTPose = !_isForcedTPose;
    }

    public void OnChangeFaceAngleBaseDistance() {
        _faceAngleBaseDistance = float.Parse(_faceAngleBaseDistanceInputField.text);
    }

    public void OnChangeTranslationMagnification() {
        _translationMagnification = float.Parse(_translationMagnificationInputField.text);
    }

    public void OnChangeRotationMagnification() {
        _rotationMagnification = float.Parse(_rotationMagnificationInputField.text);
    }

    public void OnChangeSmoothingLevel() {
        _smoothingLevel = int.Parse(_smoothingLevelInputField.text);
        if (_smoothingLevel < 1) {
            _smoothingLevel = 1;
        }
        if (_smoothingLevel >= MAX_SMOOTHING_LEVEL) {
            _smoothingLevel = MAX_SMOOTHING_LEVEL;
        }
        _smoothingLevelInputField.text = _smoothingLevel.ToString();
    }
    
    public void OnChangeAutoAdjustment() {
        _autoAdjustmentRatio = float.Parse(_autoAdjustmentInputField.text);
        if (_autoAdjustmentRatio < 0) {
            _autoAdjustmentRatio = 0;
        }
        if (_autoAdjustmentRatio > 1) {
            _autoAdjustmentRatio = 1;
        }
        _autoAdjustmentInputField.text = _autoAdjustmentRatio.ToString();
    }

    public void OnClickCalibrateFacePoints() {
        Vector3 face;
        Vector3 leftEye;
        Vector3 rightEye;
        Vector3 leftIris;
        Vector3 rightIris;
        SMT.getFacePoints(out face, out leftEye, out rightEye, out leftIris, out rightIris);

        _calibratedFacePosition = face;
        _calibratedLeftEyePosition = leftEye;
        _calibratedRigthEyePosition = rightEye;
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

    void smooth(ref Vector3 t, ref List<Vector3> list, int length) {
        list.RemoveAt(list.Count - 1);
        list.Insert(0, t);

        t.Set(0, 0, 0);
        for (int i = 0; i < length; i++) {
            t = t + list[i];
        }
        t = t / length;
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
        
        _calibratedFacePosition = Vector3.zero;
        _calibratedLeftEyePosition = Vector3.zero;
        _calibratedRigthEyePosition = Vector3.zero;

        _faceAngleBaseDistanceInputField.text = "0.2";
        OnChangeFaceAngleBaseDistance();
        _translationMagnificationInputField.text = "1.0";
        OnChangeTranslationMagnification();
        _rotationMagnificationInputField.text = "1.0";
        OnChangeRotationMagnification();

        _smoothingLevelInputField.text = "5";
        OnChangeSmoothingLevel();

        _autoAdjustmentInputField.text = "0.05";
        OnChangeAutoAdjustment();

        _uOSCInputField.text = "39540";
        OnChangedOSCPort();

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

    void setStandardHandPose() {
        // 頭に合わせて腕の位置を調整
        var headRotationEuler = halfAngleVector3(_head.transform.eulerAngles);
        headRotationEuler.x = headRotationEuler.x / 3;
        headRotationEuler.y = headRotationEuler.y / 2;
        headRotationEuler.z = headRotationEuler.z / 3;
        var headRotation = Quaternion.Euler(headRotationEuler);
        // 左手
        var leftOffset = new Vector3(0.4f, -0.8f, -0.1f);
        leftOffset = headRotation * leftOffset;
        _leftHand.transform.position = _head.transform.position + leftOffset;
        _leftHand.transform.rotation = headRotation * Quaternion.Euler(-25, 45, -67);

        // 右手
        var rightOffset = new Vector3(-0.4f, -0.8f, -0.1f);
        rightOffset = headRotation * rightOffset;
        _rightHand.transform.position = _head.transform.position + rightOffset;
        _rightHand.transform.rotation = headRotation * Quaternion.Euler(-25, -45, 67);
    }

    void trackingEyes(Vector3 eye, List<Vector3> eyeList, 
                      Vector3 iris, List<Vector3> irisList, 
                      GameObject go, float headRotationZ, out Vector3 lookAt) {
        // 目の開閉を計算
        smooth(ref eye, ref eyeList, MAX_EYE_SMOOTHING);
        smooth(ref iris, ref irisList, MAX_IRIS_SMOOTHING);
        float open = iris.z;
        open = Mathf.Clamp(open, 0, 1.0f);
        if (open > _maxEyeOpenThreshold) {
            open = _maxEyeOpenThreshold;
        }
        else if (open < _minEyeOpenThreshold) {
            open = _minEyeOpenThreshold;
        }
        open -= _minEyeOpenThreshold;
        open = open / (_maxEyeOpenThreshold - _minEyeOpenThreshold);
        go.transform.localScale = new Vector3(1, 1, open);
        go.GetComponent<TrackerSender>().BlendShapeValue = 1.0f - open;

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
            go.transform.localPosition = new Vector3(dir.x, 0, -dir.y) * 10;

            lookAt.x = dir.x * _irisTranslationScale;
            lookAt.y = -dir.y * _irisTranslationScale;
            lookAt.z = -1;
        }
    }

    void trackingFacePoints() {
        if (SMT.isFacePointsDetected()) {
            Vector3 face;
            Vector3 leftEye;
            Vector3 rightEye;
            Vector3 leftIris;
            Vector3 rightIris;
            SMT.getFacePoints(out face, out leftEye, out rightEye, out leftIris, out rightIris);

            // 目の間の距離
            var eyeLtoR = rightEye - leftEye;
            eyeLtoR.z = 0;
            //float eyeDistance = eyeLtoR.magnitude;
            var faceLength = face.z * 2; // 顏の直径
            var standardLength = 0.20f; // 20cm
            var pixParM = standardLength / faceLength;

            var facePosition = face - _calibratedFacePosition;
            facePosition.y *= -1;
            facePosition *= pixParM;

            var faceRotationEuler =Vector3.zero;
            if (_calibratedFacePosition.z < face.z) {
                facePosition.z = -1 + (_calibratedFacePosition.z / face.z);
            }
            else {
                facePosition.z = 0;
            }

            faceRotationEuler.z = -Mathf.Atan(eyeLtoR.y / eyeLtoR.x) * Mathf.Rad2Deg;

            var radY = facePosition.x / _faceAngleBaseDistance;
            if (Mathf.Abs(radY) > 1.0f) {
                radY = radY / Mathf.Abs(radY); // -1 or 1に補正
            }
            faceRotationEuler.y = -Mathf.Asin(radY) * Mathf.Rad2Deg;


            var radX = facePosition.y / _faceAngleBaseDistance;
            if (Mathf.Abs(radX) > 1.0f) {
                radX = radX / Mathf.Abs(radX); // -1 or 1に補正
            }
            faceRotationEuler.x = Mathf.Asin(radX) * Mathf.Rad2Deg;
            faceRotationEuler.x = faceRotationEuler.x * 2; // 縦方向の検出は弱いので大きく動くように補正.

            // 倍率適用
            facePosition = facePosition * _translationMagnification;
            faceRotationEuler = faceRotationEuler * _rotationMagnification;
            if (faceRotationEuler.x < -75) faceRotationEuler.x = -75;
            if (faceRotationEuler.x > 75) faceRotationEuler.x = 75;
            if (faceRotationEuler.z < -75) faceRotationEuler.z = -75;
            if (faceRotationEuler.z > 75) faceRotationEuler.z = 75;

            // オフセット適用
            facePosition.y += 1.3f;

            smoothTransform(ref facePosition, ref faceRotationEuler);

            var faceRotation = Quaternion.Euler(faceRotationEuler);
            if (_useFaceTracking) {
                _head.transform.position = facePosition;
                _head.transform.rotation = faceRotation;
            }

            // 顏の中央位置を自動調整
            if (_autoAdjustmentRatio > 0) {
                var z = _calibratedFacePosition.z; // Zは維持、XYのみずれやすいので調整する
                _calibratedFacePosition = (_calibratedFacePosition * (1.0f - _autoAdjustmentRatio)) + (face * _autoAdjustmentRatio);
                _calibratedFacePosition.z = z;
            }


            // アイトラキング(虹彩追跡)
            if (_useEyeTracking) {
                float headRotationZ = -Mathf.Atan(eyeLtoR.y / eyeLtoR.x) * Mathf.Rad2Deg;

                Vector3 leftLookAt;
                trackingEyes(leftEye, _leftEyeList, leftIris, _leftIrisList, _leftIris, headRotationZ, out leftLookAt);

                Vector3 rightLookAt;
                trackingEyes(rightEye, _rightEyeList, rightIris, _rightIrisList, _rightIris, headRotationZ, out rightLookAt);

                if (_useEyesLRBlinkSynced) {
                    var lts = _leftIris.GetComponent<TrackerSender>();
                    var rts = _rightIris.GetComponent<TrackerSender>();
                    var eyeOpenAve = (lts.BlendShapeValue + rts.BlendShapeValue) / 2;
                    lts.BlendShapeValue = eyeOpenAve;
                    rts.BlendShapeValue = eyeOpenAve;
                }

                Vector3 lookAt = leftLookAt + rightLookAt;
                if (lookAt.z > 0) {
                    lookAt = lookAt / Mathf.Abs(lookAt.z); // 注視点は-1なので-2になっていたら平均値を取る
                 } else {
                    lookAt.z = -1;
                }
                lookAt.y += 0.33f;
                _lookAt.transform.localPosition = lookAt;
            }
        }
    }

    // Update is called once per frame
    void Update() {
        // バモキャキャリブレーション用に現在の頭の位置を基準にTポーズで固定
        if (_isForcedTPose) {
            // 左手
            var leftOffset = new Vector3(0.8f, -0.2f, 0);
            leftOffset = _head.transform.rotation * leftOffset;
            _leftHand.transform.position = _head.transform.position + leftOffset;
            _leftHand.transform.rotation = _head.transform.rotation;

            _calibratedLeftHandPosition = _leftHand.transform.position;
            _calibratedLeftHandRotation = _leftHand.transform.rotation;

            // 右手
            var rightOffset = new Vector3(-0.8f, -0.2f, 0);
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

        setStandardHandPose();
    }
}
