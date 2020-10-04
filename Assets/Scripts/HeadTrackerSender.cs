using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using VRM;

[RequireComponent(typeof(uOSC.uOscClient))]
public class HeadTrackerSender : MonoBehaviour {

    public enum VirtualDevice {
        HMD = 0,
        Controller = 1,
        Tracker = 2,
    }

    [Header("Virtual Device")]
    public VirtualDevice DeviceMode = VirtualDevice.Tracker;
    //public Transform DeviceTransform = null;
    public String _deviceSerial = "VIRTUAL_DEVICE";

    [Header("BlendShapeProxy")]
    public string LeftIrisBlendShapeName = "";
    public float LeftIrisBlendShapeValue = 0f;
    public string RightIrisBlendShapeName = "";
    public float RightIrisBlendShapeValue = 0f;

    uOSC.uOscClient client = null;

    public GameObject _object;
    public GameObject _lookAt;
    public GameObject _leftIris;
    public GameObject _rightIris;

    public bool _useEyeTracking = false;
    public bool _useEyesBlink = false;

    public Vector3 _vrPlayAreaOffsetTranslation;
    public Vector3 _vrPlayAreaOffsetRotation;

    public void ChangePort(int port) {
        if (client == null) {
            return;
        }
        client.enabled = false;
        var type = typeof(uOSC.uOscClient);
        var portfield = type.GetField("port", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
        portfield.SetValue(client, port);
        client.enabled = true;
    }

    void Start() {
        client = GetComponent<uOSC.uOscClient>();
    }

    void Update() {
        if (client == null) {
            return;
        }

        if (_object != null) {
            string name = null;
            switch (DeviceMode) {
                case VirtualDevice.HMD:
                    name = "/VMC/Ext/Hmd/Pos";
                    break;
                case VirtualDevice.Controller:
                    name = "/VMC/Ext/Con/Pos";
                    break;
                case VirtualDevice.Tracker:
                    name = "/VMC/Ext/Tra/Pos";
                    break;
                default:
                    name = null;
                    break;
            }
            if (name != null && _object != null && _deviceSerial != null) {
                var offsetRot = Quaternion.Euler(_vrPlayAreaOffsetRotation);
                var pos = _object.transform.position + _vrPlayAreaOffsetTranslation;
                var rot = _object.transform.rotation * offsetRot;

                client.Send(name,
                    (string)_deviceSerial,
                    (float)pos.x,
                    (float)pos.y,
                    (float)pos.z,
                    (float)rot.x,
                    (float)rot.y,
                    (float)rot.z,
                    (float)rot.w);
            }
        }

        if (_useEyeTracking) {
            if (_lookAt != null) {
                var p = _lookAt.transform.localPosition;
                client.Send("/VMC/Ext/Set/Eye", 1, -p.x, p.y, p.z);
            }
        }

        if (_useEyesBlink) {
            client.Send("/VMC/Ext/Blend/Val", LeftIrisBlendShapeName, LeftIrisBlendShapeValue);
            client.Send("/VMC/Ext/Blend/Val", RightIrisBlendShapeName, RightIrisBlendShapeValue);
            client.Send("/VMC/Ext/Blend/Apply");
        }
    }
}
