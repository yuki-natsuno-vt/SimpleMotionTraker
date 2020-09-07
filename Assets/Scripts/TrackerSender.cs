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
public class TrackerSender : MonoBehaviour {

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
    public string BlendShapeName = "";
    public float BlendShapeValue = 0f;

    uOSC.uOscClient client = null;

    public GameObject _object;
    public GameObject _lookAt;

    public Vector3 _realityAreaOffsetTrnslation;
    public Vector3 _realityAreaOffsetRotation;

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

    public void setClientEnabled(bool isEnabled) {
        if (client == null) {
            return;
        }
        client.enabled = isEnabled;
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
                var pos = _object.transform.position + _realityAreaOffsetTrnslation;
                var rot = _object.transform.rotation * Quaternion.Euler(_realityAreaOffsetRotation);

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

        if (_lookAt != null) {
            var p = _lookAt.transform.localPosition;
            client.Send("/VMC/Ext/Set/Eye", 1, p.x, p.y, p.z);
        }

        if (!String.IsNullOrEmpty(BlendShapeName)) {
            client.Send("/VMC/Ext/Blend/Val", BlendShapeName, BlendShapeValue);
            client.Send("/VMC/Ext/Blend/Apply");
        }
    }
}
