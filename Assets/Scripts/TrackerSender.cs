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

        {
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
                client.Send(name,
                    (string)_deviceSerial,
                    (float)_object.transform.position.x,
                    (float)_object.transform.position.y,
                    (float)_object.transform.position.z,
                    (float)_object.transform.rotation.x,
                    (float)_object.transform.rotation.y,
                    (float)_object.transform.rotation.z,
                    (float)_object.transform.rotation.w);
            }
        }

        {
            client.Send("/VMC/Ext/Blend/Val", BlendShapeName, BlendShapeValue);
            client.Send("/VMC/Ext/Blend/Apply");
        }
    }
}
