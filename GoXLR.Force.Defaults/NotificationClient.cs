using System;
using System.Collections.Generic;
using System.Linq;
using GoXLR.Force.Defaults.Com;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace GoXLR.Force.Defaults
{
    public class NotificationClient : IMMNotificationClient
    {
        private readonly Dictionary<string, MMDevice> _devices = new Dictionary<string, MMDevice>();
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
                _devices[device.ID] = device;
            }

            //Set initial defaults to GoXLR defaults:
            InitDefault(DataFlow.Render, Role.Multimedia, "System");
            InitDefault(DataFlow.Render, Role.Communications, "Chat");
            InitDefault(DataFlow.Capture, Role.Multimedia, "Chat Mic");
            InitDefault(DataFlow.Capture, Role.Communications, "Chat Mic");

            //Subscribe to mute/unmute and volume change events:
            foreach (var mmDevice in GetGoXLRDevices())
            {
                OnDeviceAdded(mmDevice);
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
            return _devices
                .Values
                .Where(device => device.DeviceFriendlyName.IndexOf("TC-Helicon GoXLR", StringComparison.OrdinalIgnoreCase) >= 0)
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

            return _devices
                .Values
                .Where(device => device.State == DeviceState.Active)
                .Where(device => device.DataFlow == flow)
                .Where(device => device.FriendlyName.StartsWith(searchString))
                .SingleOrDefault();
        }

        private void SetDefaultAudioDevice(DataFlow flow, Role role, string pwstrDefaultDeviceId, string searchString)
        {
            if (string.IsNullOrWhiteSpace(pwstrDefaultDeviceId))
                return;

            var mmDevice = GetActiveDevice(flow, searchString);

            if (mmDevice == null)
                return;

            if (pwstrDefaultDeviceId == mmDevice.ID)
                return;

            SetDefaultDevice(mmDevice.ID, role);
        }

        private void OnDeviceAdded(MMDevice device)
        {
            //TODO: Set as default if not already set.

            //Add unmute and 100% volume fix.
            device.AudioEndpointVolume.OnVolumeNotification += data =>
            {
                if (data.Muted)
                    device.AudioEndpointVolume.Mute = false;

                if (data.MasterVolume < 1)
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;

                Console.WriteLine($"{data.Guid} {data.Muted} {data.MasterVolume}");
            };
        }
        
        void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
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

            Console.WriteLine($"OnDefaultDeviceChanged: {flow}, {role}, {defaultDeviceId}");
        }

        void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
        {
            Console.WriteLine($"OnDeviceAdded: {pwstrDeviceId}");

            var device = _deviceEnumerator.GetDevice(pwstrDeviceId);
            if(device.DeviceFriendlyName.IndexOf("TC-Helicon GoXLR", StringComparison.OrdinalIgnoreCase) < 0)
                return;

            OnDeviceAdded(device);
        }

        void IMMNotificationClient.OnDeviceRemoved(string deviceId)
        {
            //TODO: Implement:
            Console.WriteLine($"OnDeviceRemoved: {deviceId}");
        }

        void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            Console.WriteLine($"OnDeviceStateChanged: {deviceId}, {newState}");
        }

        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            Console.WriteLine($"OnPropertyValueChanged: {pwstrDeviceId}, {key.formatId} ... {key.propertyId}");
        }
    }
}