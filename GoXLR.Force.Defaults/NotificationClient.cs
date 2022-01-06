using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GoXLR.Force.Defaults.Com;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace GoXLR.Force.Defaults
{
    [SuppressMessage("ReSharper", "ReplaceWithSingleCallToSingleOrDefault")]
    public class NotificationClient : IMMNotificationClient
    {
        private readonly Dictionary<string, MMDevice> _goXlrDevices = new Dictionary<string, MMDevice>();
        private readonly MMDeviceEnumerator _deviceEnumerator;
        private readonly PolicyConfigClient _policyConfigClient;
        
        public NotificationClient()
        {
            _policyConfigClient = new PolicyConfigClient();
            _deviceEnumerator = new MMDeviceEnumerator();
            _deviceEnumerator.RegisterEndpointNotificationCallback(this);
            
            //Fill up device list (once):
            var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All);
            foreach (var device in devices)
            {
                if (!IsGoXLR(device))
                    continue;

                _goXlrDevices[device.ID] = device;
            }

            //Set initial defaults to GoXLR defaults:
            InitDefault(DataFlow.Render, Role.Multimedia, "System");
            InitDefault(DataFlow.Render, Role.Communications, "Chat");
            InitDefault(DataFlow.Capture, Role.Multimedia, "Chat Mic");
            InitDefault(DataFlow.Capture, Role.Communications, "Chat Mic");

            //Subscribe to mute/unmute and volume change events:
            foreach (var mmDevice in GetGoXLRDevices())
            {
                //{0.0.0.00000000} -> Renderer
                //{0.0.1.00000000} -> Capture
                Console.WriteLine($"{DateTime.Now}: GoXLR Found | {mmDevice.ID}");
                MonitorNewDevice(mmDevice);
            }
        }

        private void InitDefault(DataFlow flow, Role role, string searchString)
        {
            var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(flow, role);
            SetDefaultAudioDevice(flow, role, defaultDevice.ID, searchString);
        }

        private MMDevice[] GetGoXLRDevices()
        {
            //TODO: What if it's not connected yet? Need to subscribe when connected and disconnected.
            return _goXlrDevices
                .Values
                .Where(device => IsGoXLR(device))
                .Where(device => device.State == DeviceState.Active)
                .ToArray();
        }

        private void SetDefaultDevice(string deviceId, Role role)
        {
            _policyConfigClient.SetDefaultEndpoint(deviceId, role);
        }

        private MMDevice GetActiveDevice(DataFlow flow, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return null;

            return _goXlrDevices
                .Values
                .Where(device => device.State == DeviceState.Active)
                .Where(device => device.DataFlow == flow)
                .Where(device => device.FriendlyName.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();
        }

        private void SetDefaultAudioDevice(DataFlow flow, Role role, string newDefaultDeviceId, string searchString)
        {
            //Get either a Render or Capture device:
            var device = GetActiveDevice(flow, searchString);
            if (device is null)
                return;

            //Checks if device is not already set as default:
            if (newDefaultDeviceId == device.ID)
                return; //New default device is a GoXLR device, ignore.
            
            SetDefaultDevice(device.ID, role);
        }

        private void MonitorNewDevice(MMDevice device)
        {
            if (!IsGoXLR(device))
                return;

            _goXlrDevices[device.ID] = device;

            //TODO: Set as default if not already set.

            //Add unmute and 100% volume fix.
            device.AudioEndpointVolume.OnVolumeNotification += data =>
            {
                Console.WriteLine($"{DateTime.Now}: {data.Guid} {data.Muted} {data.MasterVolume}");

                if (data.Muted)
                    device.AudioEndpointVolume.Mute = false;

                if (data.MasterVolume < 1)
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
            };
        }
        
        void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            Console.WriteLine($"{DateTime.Now}: OnDefaultDeviceChanged: {flow}, {role}, {defaultDeviceId}");

            if (flow == DataFlow.Render && role == Role.Multimedia)
            {
                SetDefaultAudioDevice(flow, role, defaultDeviceId, "System");
                return;
            }

            if (flow == DataFlow.Render && role == Role.Communications)
            {
                SetDefaultAudioDevice(flow, role, defaultDeviceId, "Chat");
                return;
            }

            if (flow == DataFlow.Capture && role == Role.Multimedia)
            {
                SetDefaultAudioDevice(flow, role, defaultDeviceId, "Chat Mic");
                return;
            }

            if (flow == DataFlow.Capture && role == Role.Communications)
            {
                SetDefaultAudioDevice(flow, role, defaultDeviceId, "Chat Mic");
                return;
            }
        }

        void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
        {
            var device = _deviceEnumerator.GetDevice(pwstrDeviceId);
            if (!IsGoXLR(device))
                return;

            Console.WriteLine($"{DateTime.Now} [OnDeviceAdded]: Registering new device id '{pwstrDeviceId}' with name '{device.FriendlyName}'");

            MonitorNewDevice(device);
        }

        void IMMNotificationClient.OnDeviceRemoved(string deviceId)
        {
            if (!_goXlrDevices.TryGetValue(deviceId, out var device))
                return;
            
            //TODO: Implement:
            Console.WriteLine($"{DateTime.Now}: OnDeviceRemoved: {deviceId}");
        }

        void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            if (!_goXlrDevices.ContainsKey(deviceId))
                return;

            Console.WriteLine($"{DateTime.Now}: OnDeviceStateChanged: {deviceId}, {newState}");
        }

        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            if (!_goXlrDevices.ContainsKey(pwstrDeviceId))
                return;

            Console.WriteLine($"{DateTime.Now}: OnPropertyValueChanged: {pwstrDeviceId} || {key.formatId} || {key.propertyId}");
        }

        private bool IsGoXLR(MMDevice device)
        {
            return device.DeviceFriendlyName.IndexOf("TC-Helicon GoXLR", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}