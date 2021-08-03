using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SharpDX.DirectInput;

namespace ynv
{
    public class Input
    {
        static DirectInput directInput = null;
        static List<DeviceInstance> joypadDeviceInstances;

        private Joystick _joystick = null;
        private Keyboard _keyboard = null;
        private Mouse _mouse = null;

        private Dictionary<JoystickOffset, int> _prevData;
        private Dictionary<JoystickOffset, int> _currentData;

        private Dictionary<Key, int> _prevKeyboardState;
        private Dictionary<Key, int> _currentKeyboardState;

        private Dictionary<MouseOffset, int> _prevMouseData;
        private Dictionary<MouseOffset, int> _currentMouseData;


        public static List<string> GetDeviceNames()
        {
            var names = new List<string>();
            foreach (var device in joypadDeviceInstances)
            {
                names.Add(device.ProductName);
            }
            return names;
        }

        public static int DeviceNameToIndex(string deviceName)
        {
            int index = 0;
            foreach (var device in joypadDeviceInstances)
            {
                if (deviceName == device.ProductName)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        static Input()
        {
            if (directInput == null)
            {
                directInput = new DirectInput();
                joypadDeviceInstances = directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices)
                                .Concat(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                                .ToList();
            }
        }

        public Input(string deviceName)
        {
            InitDevices(DeviceNameToIndex(deviceName));
        }

        public void InitDevices(int deviceIndex)
        {
            if (_prevData != null) _prevData.Clear();
            if (_currentData != null) _currentData.Clear();
            if (_prevKeyboardState != null) _prevKeyboardState.Clear();
            if (_currentKeyboardState != null) _currentKeyboardState.Clear();
            if (_prevMouseData != null) _prevMouseData.Clear();
            if (_currentMouseData != null) _currentMouseData.Clear();

            var instanceGuid = Guid.Empty;

            var devices = directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices)
                .Concat(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                .ToList();

            if ((devices.Count > deviceIndex) && (deviceIndex >= 0))
            {
                instanceGuid = devices[deviceIndex].InstanceGuid;

                var joystick = new Joystick(directInput, instanceGuid);
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
                joystick.Poll();
                _joystick = joystick;

                _prevData = new Dictionary<JoystickOffset, int>();
                _currentData = new Dictionary<JoystickOffset, int>();
                var data = _joystick.GetBufferedData();
                foreach (var state in data)
                {
                    _prevData[state.Offset] = state.Value;
                    _currentData[state.Offset] = state.Value;
                }
            }

            // キーボード
            var keyboardDevices = directInput.GetDevices(SharpDX.DirectInput.DeviceType.Keyboard, DeviceEnumerationFlags.AllDevices);
            if (keyboardDevices.Count > 0)
            {
                instanceGuid = keyboardDevices[0].InstanceGuid;
                var keyboard = new Keyboard(directInput);
                keyboard.Properties.BufferSize = 128;
                keyboard.Acquire();
                keyboard.Poll();
                _keyboard = keyboard;

                _prevKeyboardState = new Dictionary<Key, int>();
                _currentKeyboardState = new Dictionary<Key, int>();
                foreach ( var key in keyboard.GetCurrentState().AllKeys)
                {
                    _prevKeyboardState[key] = 0;
                    _currentKeyboardState[key] = 0;
                }
            }

            // マウス
            var mouseDevices = directInput.GetDevices(SharpDX.DirectInput.DeviceType.Mouse, DeviceEnumerationFlags.AllDevices);
            if (mouseDevices.Count > 0)
            {
                instanceGuid = mouseDevices[0].InstanceGuid;
                var mouse = new Mouse(directInput);
                mouse.Properties.BufferSize = 128;
                mouse.Acquire();
                mouse.Poll();
                _mouse = mouse;

                _prevMouseData = new Dictionary<MouseOffset, int>();
                _currentMouseData = new Dictionary<MouseOffset, int>();
                foreach (var data in mouse.GetBufferedData())
                {
                    _prevMouseData[data.Offset] = data.Value;
                    _currentMouseData[data.Offset] = data.Value;
                }
            }
        }

        public void Update()
        {
            if (_joystick != null)
            {
                foreach (var state in _currentData)
                {
                    _prevData[state.Key] = state.Value;
                }
                _joystick.Poll();
                var data = _joystick.GetBufferedData();
                foreach (var state in data)
                {
                    if (!_currentData.ContainsKey(state.Offset))
                    {
                        _currentData[state.Offset] = 0;
                    }
                    _currentData[state.Offset] = state.Value;

                    switch (state.Offset)
                    {
                        case JoystickOffset.X:
                        case JoystickOffset.Y:
                        case JoystickOffset.Z:
                        case JoystickOffset.RotationX:
                        case JoystickOffset.RotationY:
                        case JoystickOffset.RotationZ:
                            // アナログ、デジタルのモードによってニュートラル時の値に誤差があるので、計算上の中間から500はニュートラル扱いにする.
                            _currentData[state.Offset] -= 32767;
                            if (Mathf.Abs(_currentData[state.Offset]) < 500)
                            {
                                _currentData[state.Offset] = 0;
                            }
                            break;
                        case JoystickOffset.PointOfViewControllers0:
                        case JoystickOffset.PointOfViewControllers1:
                        case JoystickOffset.PointOfViewControllers2:
                        case JoystickOffset.PointOfViewControllers3:
                            _currentData[state.Offset] += 1; // ニュートラルが -1
                            break;
                        default:
                            break;
                    }
                }
            }


            foreach (var key in _keyboard.GetCurrentState().AllKeys)
            {
                _prevKeyboardState[key] = _currentKeyboardState[key];
                _currentKeyboardState[key] = 0;
            }
            _keyboard.Poll();
            var keyboardState = _keyboard.GetCurrentState();
            foreach (var key in keyboardState.PressedKeys)
            {
                _currentKeyboardState[key] = 1;
            }

            foreach (var state in _currentMouseData)
            {
                _prevMouseData[state.Key] = state.Value;
            }
            _mouse.Poll();
            var mouseData = _mouse.GetBufferedData();
            foreach (var state in mouseData)
            {
                if (!_currentMouseData.ContainsKey(state.Offset))
                {
                    _currentMouseData[state.Offset] = 0;
                }
                _currentMouseData[state.Offset] = state.Value;
            }
        }

        public bool GetButton(JoypadCode joypadCode)
        {
            if (_joystick == null) return false;
            var joystickOffset = (JoystickOffset)joypadCode;
            if (!_currentData.ContainsKey(joystickOffset))
            {
                return false; // 一度も押されていない
            }

            if (_currentData[joystickOffset] != 0)
            {
                return true;
            }
            return false;
        }

        public bool GetButtonDown(JoypadCode joypadCode)
        {
            if (_joystick == null) return false;
            var joystickOffset = (JoystickOffset)joypadCode;
            if (!_currentData.ContainsKey(joystickOffset))
            {
                return false; // 一度も押されていない
            }

            int prevValue = 0;
            if (_prevData.ContainsKey(joystickOffset))
            {
                prevValue = _prevData[joystickOffset];
            }

            if ((_currentData[joystickOffset] != 0) && // 現在押下
                (prevValue == 0)) // 前回非押下
            {
                return true;
            }
            return false;
        }

        public bool GetButtonUp(JoypadCode joypadCode)
        {
            if (_joystick == null) return false;
            var joystickOffset = (JoystickOffset)joypadCode;
            if (!_currentData.ContainsKey(joystickOffset))
            {
                return false; // 一度も押されていない
            }

            int prevValue = 0;
            if (_prevData.ContainsKey(joystickOffset))
            {
                prevValue = _prevData[joystickOffset];
            }

            if ((_currentData[joystickOffset] == 0) && // 現在非押下
                (prevValue != 0)) // 前回押下
            {
                return true;
            }
            return false;
        }

        public float GetAxis(JoypadCode joypadCode)
        {
            if (_joystick == null) return 0;
            var joystickOffset = (JoystickOffset)joypadCode;
            int raw = 0;
            if (_currentData.ContainsKey(joystickOffset))
            {
                raw = _currentData[joystickOffset];
            }
            else if (_prevData.ContainsKey(joystickOffset))
            {
                raw = _prevData[joystickOffset];
            }
            else
            {
                return 0; // 一度も押されていない
            }
            float value = (float)(raw - 32767) / 32767;
            if (value < -1) { value = -1; }
            if (value > 1) { value = 1; }
            return value;
        }

        public bool GetButton(KeyCode keyCode)
        {
            var key = (Key)keyCode;
            if (_currentKeyboardState[key] != 0)
            {
                return true;
            }
            return false;
        }

        public bool GetButtonDown(KeyCode keyCode)
        {
            var key = (Key)keyCode;
            int prevValue = _prevKeyboardState[key];
            if ((_currentKeyboardState[key] != 0) && // 現在押下
                (prevValue == 0)) // 前回非押下
            {
                return true;
            }
            return false;
        }

        public bool GetButtonUp(KeyCode keyCode)
        {
            var key = (Key)keyCode;
            int prevValue = _prevKeyboardState[key];
            if ((_currentKeyboardState[key] == 0) && // 現在非押下
                (prevValue != 0)) // 前回押下
            {
                return true;
            }
            return false;
        }
        
        public bool GetButton(MouseCode mouseCode)
        {
            var mouseOffset = (MouseOffset)mouseCode;
            if (!_currentMouseData.ContainsKey(mouseOffset))
            {
                return false; // 一度も押されていない
            }

            if (_currentMouseData[mouseOffset] != 0)
            {
                return true;
            }
            return false;
        }

        public bool GetButtonDown(MouseCode mouseCode)
        {
            var mouseOffset = (MouseOffset)mouseCode;
            if (!_currentMouseData.ContainsKey(mouseOffset))
            {
                return false; // 一度も押されていない
            }

            int prevValue = 0;
            if (_prevMouseData.ContainsKey(mouseOffset))
            {
                prevValue = _prevMouseData[mouseOffset];
            }

            if ((_currentMouseData[mouseOffset] != 0) && // 現在押下
                (prevValue == 0)) // 前回非押下
            {
                return true;
            }
            return false;
        }

        public bool GetButtonUp(MouseCode mouseCode)
        {
            var mouseOffset = (MouseOffset)mouseCode;
            if (!_currentMouseData.ContainsKey(mouseOffset))
            {
                return false; // 一度も押されていない
            }

            int prevValue = 0;
            if (_prevMouseData.ContainsKey(mouseOffset))
            {
                prevValue = _prevMouseData[mouseOffset];
            }

            if ((_currentMouseData[mouseOffset] == 0) && // 現在非押下
                (prevValue != 0)) // 前回押下
            {
                return true;
            }
            return false;
        }
    }
}