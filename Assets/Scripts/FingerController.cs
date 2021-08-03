using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRM;
using ynv;

[RequireComponent(typeof(uOSC.uOscClient))]
public class FingerController : MonoBehaviour
{
    public enum Finger : int
    {
        LeftThumb, // 左親指
        LeftIndex, // 左人差し指
        LeftMiddle, // 左中指
        LeftRing, // 左薬指
        LeftLittle, // 左小指
        RightThumb, // 右親指
        RightIndex, // 右人差し指
        RightMiddle, // 右中指
        RightRing, // 右薬指
        RightLittle, // 右小指
        Max
    }

    public class ControlParameter
    {
        public int code;
        public float rate;
        public Finger finger;

        public ControlParameter(int code, float rate, Finger finger)
        {
            this.code = code;
            this.rate = rate;
            this.finger = finger;
        }
    }

    [System.Serializable]
    public class Config
    {
        [System.Serializable]
        public class Parameter
        {
            public string code;
            public float rate;
            public string finger;
        }

        public float motionSpeed;
        public Parameter[] defaultPoses;
        public Parameter[] gripPoses;
        public Parameter[] joypad;
        public Parameter[] keyboard;
        public Parameter[] mouse;

        public static Config CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<Config>(jsonString);
        }

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    uOSC.uOscClient uClient = null;

    public GameObject Model = null;
    private GameObject OldModel = null;

    Animator animator = null;
    VRMBlendShapeProxy blendShapeProxy = null;

    private ynv.Input _input;

    private GameObject _model;

    private float _motionSpeed = 1.0f;

    public bool _useFingerControl = false;
    public bool _useGripPoses = false;

    private const int MAX_FINGER_ANGLE = 90;

    private Vector3[] _opendAxis = new Vector3[(int)Finger.Max]; // 各指を曲げる軸. 親指は例外
    private float[] _rate = new float[(int)Finger.Max]; // 各指の曲げ割合

    private ynv.JoypadCode[] _axisKeyCodes = new ynv.JoypadCode[]{
        ynv.JoypadCode.XAxis,
        ynv.JoypadCode.YAxis,
        ynv.JoypadCode.ZAxis,
        ynv.JoypadCode.XAxisRotation,
        ynv.JoypadCode.YAxisRotation,
        ynv.JoypadCode.ZAxisRotation,
        ynv.JoypadCode.Slider0,
        ynv.JoypadCode.Slider1,
        ynv.JoypadCode.PointOfView0,
        ynv.JoypadCode.PointOfView1,
        ynv.JoypadCode.PointOfView2,
        ynv.JoypadCode.PointOfView3,
    };

    private static readonly Dictionary<string, Finger> fingerCodeToEnumFinger = new Dictionary<string, Finger>()
    {
        {"L0", Finger.LeftThumb },
        {"L1", Finger.LeftIndex },
        {"L2", Finger.LeftMiddle },
        {"L3", Finger.LeftRing },
        {"L4", Finger.LeftLittle },
        {"R0", Finger.RightThumb },
        {"R1", Finger.RightIndex },
        {"R2", Finger.RightMiddle },
        {"R3", Finger.RightRing },
        {"R4", Finger.RightLittle }
    };

    private HumanBodyBones[] targetHumanBodyBones = new HumanBodyBones[] {
            HumanBodyBones.LeftThumbProximal, // 左親指第一指骨のボーン
            HumanBodyBones.LeftThumbIntermediate, // 左親指第二指骨のボーン
            HumanBodyBones.LeftThumbDistal, // 左親指第三指骨のボーン
            HumanBodyBones.LeftIndexProximal, // 左人差し指第一指骨のボーン
            HumanBodyBones.LeftIndexIntermediate, // 左人差し指第二指骨のボーン
            HumanBodyBones.LeftIndexDistal, // 左人差し指第三指骨のボーン
            HumanBodyBones.LeftMiddleProximal, // 左中指第一指骨のボーン
            HumanBodyBones.LeftMiddleIntermediate, // 左中指第二指骨のボーン
            HumanBodyBones.LeftMiddleDistal, // 左中指第三指骨のボーン
            HumanBodyBones.LeftRingProximal, // 左薬指第一指骨のボーン
            HumanBodyBones.LeftRingIntermediate, // 左薬指第二指骨のボーン
            HumanBodyBones.LeftRingDistal, // 左薬指第三指骨のボーン
            HumanBodyBones.LeftLittleProximal, // 左小指第一指骨のボーン
            HumanBodyBones.LeftLittleIntermediate, // 左小指第二指骨のボーン
            HumanBodyBones.LeftLittleDistal, // 左小指第三指骨のボーン
            HumanBodyBones.RightThumbProximal, // 右親指第一指骨のボーン
            HumanBodyBones.RightThumbIntermediate, // 右親指第二指骨のボーン
            HumanBodyBones.RightThumbDistal, // 右親指第三指骨のボーン
            HumanBodyBones.RightIndexProximal, // 右人差し指第一指骨のボーン
            HumanBodyBones.RightIndexIntermediate, // 右人差し指第二指骨のボーン
            HumanBodyBones.RightIndexDistal, // 右人差し指第三指骨のボーン
            HumanBodyBones.RightMiddleProximal, // 右中指第一指骨のボーン
            HumanBodyBones.RightMiddleIntermediate, // 右中指第二指骨
            HumanBodyBones.RightMiddleDistal, // 右中指第三指骨のボーン
            HumanBodyBones.RightRingProximal, // 右薬指第一指骨のボーン
            HumanBodyBones.RightRingIntermediate, // 右薬指第二指骨のボーン
            HumanBodyBones.RightRingDistal, // 右薬指第三指骨のボーン
            HumanBodyBones.RightLittleProximal, // 右小指第一指骨のボーン
            HumanBodyBones.RightLittleIntermediate, // 右小指第二指骨のボーン
            HumanBodyBones.RightLittleDistal // 右小指第三指骨のボーン
};

    List<ControlParameter> _defaultPosesControlParameter = new List<ControlParameter>();
    List<ControlParameter> _gripPosesControlParameter = new List<ControlParameter>();
    Dictionary<ynv.JoypadCode, List<ControlParameter>> _joypadCodeToControlParameter = new Dictionary<ynv.JoypadCode, List<ControlParameter>>();
    Dictionary<ynv.KeyCode, List<ControlParameter>> _keyCodeToControlParameter = new Dictionary<ynv.KeyCode, List<ControlParameter>>();
    Dictionary<ynv.MouseCode, List<ControlParameter>> _mouseCodeToControlParameter = new Dictionary<ynv.MouseCode, List<ControlParameter>>();

    public bool _debugDisable = false;

    public void ChangePort(int port)
    {
        if (uClient == null)
        {
            return;
        }
        uClient.enabled = false;
        var type = typeof(uOSC.uOscClient);
        var portfield = type.GetField("port", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
        portfield.SetValue(uClient, port);
        uClient.enabled = true;
    }

    public void SetModel(GameObject model)
    {
        Model = model;
        animator = Model.GetComponent<Animator>();
        InitFinger();
    }

    public void InitFinger()
    {
        calcFingersRotationAxis(Finger.LeftThumb, HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbDistal);
        calcFingersRotationAxis(Finger.LeftIndex, HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexDistal);
        calcFingersRotationAxis(Finger.LeftMiddle, HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleDistal);
        calcFingersRotationAxis(Finger.LeftRing, HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingDistal);
        calcFingersRotationAxis(Finger.LeftLittle, HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleDistal);
        calcFingersRotationAxis(Finger.RightThumb, HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbDistal);
        calcFingersRotationAxis(Finger.RightIndex, HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexDistal);
        calcFingersRotationAxis(Finger.RightMiddle, HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleDistal);
        calcFingersRotationAxis(Finger.RightRing, HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingDistal);
        calcFingersRotationAxis(Finger.RightLittle, HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleDistal);
    }


    public void LoadConfig()
    {
        // jsonファイルから設定を読み込み.
        string path = "./SimpleMotionTracker_Data/StreamingAssets/finger_control_config.json";
        if (!File.Exists(path))
        {
            path = "./Assets/StreamingAssets/finger_control_config.json"; // 開発時のパス
            if (!File.Exists(path))
            {
                Debug.LogError("File not found. : " + path);
            }
        }
        string json = File.ReadAllText(path);
        var config = Config.CreateFromJSON(json);

        _motionSpeed = config.motionSpeed;

        // string形式の設定を扱いやすい方に成形.
        // デフォルトポーズ
        _defaultPosesControlParameter.Clear();
        foreach (var param in config.defaultPoses)
        {
            _defaultPosesControlParameter.Add(new ControlParameter((int)JoypadCode.None, param.rate, fingerCodeToEnumFinger[param.finger]));
        }
        // グリップ(握り)ポーズ
        _gripPosesControlParameter.Clear();
        foreach (var param in config.gripPoses)
        {
            _gripPosesControlParameter.Add(new ControlParameter((int)JoypadCode.None, param.rate, fingerCodeToEnumFinger[param.finger]));
        }
        // ジョイパッド
        _joypadCodeToControlParameter.Clear();
        foreach (var param in config.joypad)
        {
            ynv.JoypadCode joypadCode;
            Enum.TryParse<ynv.JoypadCode>(param.code, out joypadCode);

            if (_joypadCodeToControlParameter.ContainsKey(joypadCode))
            {
                _joypadCodeToControlParameter[joypadCode].Add(new ControlParameter((int)joypadCode, param.rate, fingerCodeToEnumFinger[param.finger]));
            }
            else
            {
                _joypadCodeToControlParameter.Add(joypadCode, new List<ControlParameter> {
                    new ControlParameter((int)joypadCode, param.rate, fingerCodeToEnumFinger[param.finger])
                });
            }
        }
        // キーボード
        _keyCodeToControlParameter.Clear();
        foreach (var param in config.keyboard)
        {
            ynv.KeyCode keyCode;
            Enum.TryParse<ynv.KeyCode>(param.code, out keyCode);

            if (_keyCodeToControlParameter.ContainsKey(keyCode))
            {
                _keyCodeToControlParameter[keyCode].Add(new ControlParameter((int)keyCode, param.rate, fingerCodeToEnumFinger[param.finger]));
            }
            else
            {
                _keyCodeToControlParameter.Add(keyCode, new List<ControlParameter> {
                    new ControlParameter((int)keyCode, param.rate, fingerCodeToEnumFinger[param.finger])
                });
            }
        }
        // マウス
        _mouseCodeToControlParameter.Clear();
        foreach (var param in config.mouse)
        {
            ynv.MouseCode mouseCode;
            Enum.TryParse<ynv.MouseCode>(param.code, out mouseCode);

            if (_mouseCodeToControlParameter.ContainsKey(mouseCode))
            {
                _mouseCodeToControlParameter[mouseCode].Add(new ControlParameter((int)mouseCode, param.rate, fingerCodeToEnumFinger[param.finger]));
            }
            else
            {
                _mouseCodeToControlParameter.Add(mouseCode, new List<ControlParameter> {
                    new ControlParameter((int)mouseCode, param.rate, fingerCodeToEnumFinger[param.finger])
                });
            }
        }
        Debug.Log("Load finger_control_config.json complete.");
    }

    public void InitDevices(string deviceName)
    {
        _input = new ynv.Input(deviceName);
    }

    // Start is called before the first frame update
    void Start()
    {
        uClient = GetComponent<uOSC.uOscClient>();
    }

    void calcFingersRotationAxis(Finger finger, HumanBodyBones proximal, HumanBodyBones distal)
    {
        // X軸からの角度のずれを計算
        //左中指
        var p1 = GetBoneTransform(proximal).position; // 第一関節
        var p2 = GetBoneTransform(distal).position; // 第三関節
        var dir = p2 - p1;
        dir.y = 0; // xz平面での角度ずれを計算したいのでyは無視
        dir = dir.normalized;
        var radian = Mathf.Atan2(dir.z, dir.x);
        var deg = Mathf.Rad2Deg * radian;
        
        var rotY = Quaternion.AngleAxis (deg, Vector3.down);
        var axis = rotY * Vector3.back;
        _opendAxis[(int)finger] = axis;
    }

    Transform GetBoneTransform(HumanBodyBones bone)
    {
        if (Model == null)
        {
            return null;
        }
        return Model.GetComponent<Animator>().GetBoneTransform(bone);
    }

    void SendBones()
    {
        //モデルが更新されたときのみ読み込み
        if (Model != null && OldModel != Model)
        {
            animator = Model.GetComponent<Animator>();
            blendShapeProxy = Model.GetComponent<VRMBlendShapeProxy>();
            OldModel = Model;
        }

        if (Model != null && animator != null && uClient != null)
        {
            //Root
            var RootTransform = Model.transform;
            if (RootTransform != null)
            {
                uClient.Send("/VMC/Ext/Root/Pos",
                    "root",
                    RootTransform.position.x, RootTransform.position.y, RootTransform.position.z,
                    RootTransform.rotation.x, RootTransform.rotation.y, RootTransform.rotation.z, RootTransform.rotation.w);
            }

            //Bones
            foreach (HumanBodyBones bone in targetHumanBodyBones)
            {
                if (bone != HumanBodyBones.LastBone)
                {
                    var Transform = animator.GetBoneTransform(bone);
                    if (Transform != null)
                    {
                        uClient.Send("/VMC/Ext/Bone/Pos",
                            bone.ToString(),
                            Transform.localPosition.x, Transform.localPosition.y, Transform.localPosition.z,
                            Transform.localRotation.x, Transform.localRotation.y, Transform.localRotation.z, Transform.localRotation.w);
                    }
                }
            }

            //BlendShape
            if (blendShapeProxy != null)
            {
                foreach (var b in blendShapeProxy.GetValues())
                {
                    uClient.Send("/VMC/Ext/Blend/Val",
                        b.Key.ToString(),
                        (float)b.Value
                        );
                }
                uClient.Send("/VMC/Ext/Blend/Apply");
            }

            //Available
            uClient.Send("/VMC/Ext/OK", 1);
        }
        else
        {
            uClient.Send("/VMC/Ext/OK", 0);
        }
        uClient.Send("/VMC/Ext/T", Time.time);

    }

    void UpdateModelBones()
    {
        var axis = Vector3.zero;
        float rate = 0.0f;

        //axis = _opendAxis[(int)Finger.LeftThumb];
        rate = _rate[(int)Finger.LeftThumb];
        GetBoneTransform(HumanBodyBones.LeftThumbProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE * 0.75f, Vector3.right);
        GetBoneTransform(HumanBodyBones.LeftThumbIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE * 0.5f, Vector3.left)
                                                                            * Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, Vector3.down);
        GetBoneTransform(HumanBodyBones.LeftThumbDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, Vector3.down);

        axis = _opendAxis[(int)Finger.LeftIndex];
        rate = _rate[(int)Finger.LeftIndex];
        GetBoneTransform(HumanBodyBones.LeftIndexProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.LeftIndexIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.LeftIndexDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);

        axis = _opendAxis[(int)Finger.LeftMiddle];
        rate = _rate[(int)Finger.LeftMiddle];
        GetBoneTransform(HumanBodyBones.LeftMiddleProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.LeftMiddleDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);

        axis = _opendAxis[(int)Finger.LeftRing];
        rate = _rate[(int)Finger.LeftRing];
        GetBoneTransform(HumanBodyBones.LeftRingProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.LeftRingIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.LeftRingDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);

        axis = _opendAxis[(int)Finger.LeftLittle];
        rate = _rate[(int)Finger.LeftLittle];
        GetBoneTransform(HumanBodyBones.LeftLittleProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.LeftLittleIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.LeftLittleDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);


        //axis = _opendAxis[(int)Finger.RightThumb];
        rate = _rate[(int)Finger.RightThumb];
        GetBoneTransform(HumanBodyBones.RightThumbProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE * 0.75f, Vector3.right);
        GetBoneTransform(HumanBodyBones.RightThumbIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE * 0.5f, Vector3.left)
                                                                            * Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, Vector3.up);
        GetBoneTransform(HumanBodyBones.RightThumbDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, Vector3.up);

        axis = _opendAxis[(int)Finger.RightIndex];
        rate = _rate[(int)Finger.RightIndex];
        GetBoneTransform(HumanBodyBones.RightIndexProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.RightIndexIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.RightIndexDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);

        axis = _opendAxis[(int)Finger.RightMiddle];
        rate = _rate[(int)Finger.RightMiddle];
        GetBoneTransform(HumanBodyBones.RightMiddleProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.RightMiddleIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.RightMiddleDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);

        axis = _opendAxis[(int)Finger.RightRing];
        rate = _rate[(int)Finger.RightRing];
        GetBoneTransform(HumanBodyBones.RightRingProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.RightRingIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.RightRingDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);

        axis = _opendAxis[(int)Finger.RightLittle];
        rate = _rate[(int)Finger.RightLittle];
        GetBoneTransform(HumanBodyBones.RightLittleProximal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.RightLittleIntermediate).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);
        GetBoneTransform(HumanBodyBones.RightLittleDistal).localRotation = Quaternion.AngleAxis(rate * MAX_FINGER_ANGLE, axis);

    }

    bool isIgnoreAxisKeyCodes(ynv.JoypadCode joypadCode)
    {
        foreach (var ignore in _axisKeyCodes)
        {
            if (ignore == joypadCode)
            {
                return true;
            }
        }
        return false;
    }

    void UpdateRate(ControlParameter param) {
        var finger = (int)param.finger;
        if (_rate[finger] < param.rate)
        {
            _rate[finger] += _motionSpeed;
            if (_rate[finger] > param.rate) { _rate[finger] = param.rate; }
        }
        else if (_rate[finger] > param.rate)
        {
            _rate[finger] -= _motionSpeed;
            if (_rate[finger] < param.rate) { _rate[finger] = param.rate; }
        }
    }

    void UpdateGripRate(bool[] isUpdatedFinger, List<ControlParameter> controlParams)
    {
        foreach (var param in controlParams)
        {
            UpdateRate(param);
            isUpdatedFinger[(int)param.finger] = true;
        }
    }

    void UpdateOpenRate(bool[] isUpdatedFinger, List<ControlParameter> controlParams) {
        foreach (var param in controlParams)
        {
            var finger = (int)param.finger;
            if (isUpdatedFinger[finger] == false)
            {
                UpdateRate(param);
            }
        }
    }

    void UpdateFingerRotationRate()
    {
        _input.Update();

        var speed = _motionSpeed;
        bool[] isUpdatedFinger = new bool[(int)Finger.Max];
        for (int i = 0; i < (int)Finger.Max; i++)
        {
            isUpdatedFinger[i] = false;
        }

        // 曲げる(Joypad)
        foreach (var kv in _joypadCodeToControlParameter)
        {
            if (_input.GetButton(kv.Key))
            {
                UpdateGripRate(isUpdatedFinger, kv.Value);
            }
        }
        
        // 曲げる(Keyboard)
        foreach (var kv in _keyCodeToControlParameter)
        {
            if (_input.GetButton(kv.Key))
            {
                UpdateGripRate(isUpdatedFinger, kv.Value);
            }
        }

        // 曲げる(Mouse)
        foreach (var kv in _mouseCodeToControlParameter)
        {
            if (_input.GetButton(kv.Key))
            {
                UpdateGripRate(isUpdatedFinger, kv.Value);
            }
        }

        // 伸ばす
        if (_useGripPoses)
        {
            UpdateOpenRate(isUpdatedFinger, _gripPosesControlParameter);
        }
        else
        {
            UpdateOpenRate(isUpdatedFinger, _defaultPosesControlParameter);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (_debugDisable) { return; }
        if (_input == null) { return; }
        if (uClient == null) { return; }
        if (Model == null) { return; }

        if (_useFingerControl)
        {
            UpdateFingerRotationRate();
            UpdateModelBones();
            SendBones();
        }
    }
}
