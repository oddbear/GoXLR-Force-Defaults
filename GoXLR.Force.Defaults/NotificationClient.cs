using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GoXLR.Force.Defaults.Com;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

// ReSharper disable ReplaceWithSingleCallToFirstOrDefault
// ReSharper disable ReplaceWithSingleCallToSingleOrDefault

namespace GoXLR.Force.Defaults
{
    public class NotificationClient : IMMNotificationClient
    {
        public bool IsActive { get; set; } = true;

        private readonly Dictionary<string, MMDevice> _goXlrDevices = new Dictionary<string, MMDevice>();
        private readonly PolicyConfigClient _policyConfigClient;
        private readonly MMDeviceEnumerator _deviceEnumerator;
        
        public NotificationClient()
        {
            _policyConfigClient = new PolicyConfigClient();
            _deviceEnumerator = new MMDeviceEnumerator();
            _deviceEnumerator.RegisterEndpointNotificationCallback(this);
            
            //Fill up device list (once):
            var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All);
            foreach (var device in devices)
            {
                if (!IsGoXLR(device)) //Skip all non-goxlr devices.
                    continue;

                //DeviceId:
                //{0.0.0.00000000} -> Renderer
                //{0.0.1.00000000} -> Capture
                _goXlrDevices[device.ID] = device;

                //Subscribe to mute/unmute and volume change events:
                device.AudioEndpointVolume.OnVolumeNotification += data =>
                {
                    //Do not set to default if not active:
                    if (!IsActive)
                        return;

                    //Ex ... [OnVolumeNotification]: 00000000-0000-0000-0000-000000000000 False 0,54
                    Console.WriteLine($"{DateTime.Now} [OnVolumeNotification]: {data.Guid} {data.Muted} {data.MasterVolume}");

                    //If device is muted, it automatically unmutes:
                    if (data.Muted)
                        device.AudioEndpointVolume.Mute = false;

                    //If device volume is not 100%, it automatically is set to 100%:
                    if (data.MasterVolume < 1)
                        device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
                };
            }
        }

        /// <summary>
        /// Forces all default values to be correct.
        /// Use this on start or continue.
        /// </summary>
        public void EnsureDefaultState()
        {
            InitDefault(DataFlow.Render, Role.Multimedia, "System");
            InitDefault(DataFlow.Render, Role.Communications, "Chat");
            InitDefault(DataFlow.Capture, Role.Multimedia, "Chat Mic");
            InitDefault(DataFlow.Capture, Role.Communications, "Chat Mic");

            foreach (var device in _goXlrDevices.Values)
            {
                if (device.AudioEndpointVolume.Mute)
                    device.AudioEndpointVolume.Mute = false;

                if (device.AudioEndpointVolume.MasterVolumeLevelScalar < 1)
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
            }
        }

        private void InitDefault(DataFlow flow, Role role, string searchString)
        {
            var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(flow, role);
            SetDefaultAudioDevice(flow, role, defaultDevice.ID, searchString);
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
                .FirstOrDefault();
        }

        private void SetDefaultAudioDevice(DataFlow flow, Role role, string newDefaultDeviceId, string searchString)
        {
            //Do not set to default if not active:
            if (!IsActive)
                return;

            Console.WriteLine($"{DateTime.Now} [SetDefaultAudioDevice]: {flow}, {role}, {newDefaultDeviceId}");

            //Get either a Render or Capture device, based on flow and searched device name:
            var device = GetActiveDevice(flow, searchString);

            //If no active device matches the search string, there is nothing to set:
            if (device is null)
                return;

            //Checks if device is not already set as default:
            if (newDefaultDeviceId == device.ID)
                return; //New default device is the correct GoXLR device, ignore.

            //The device is another device, set the new device to be the correct one:
            _policyConfigClient.SetDefaultEndpoint(device.ID, role);
        }

        /// <summary>
        /// Gets the newly set default device, and it's Flow and Role.
        /// Used to search for expected default device, and set if the new is not the correct one.
        /// </summary>
        /// <param name="flow">Render or Capture</param>
        /// <param name="role">Multimedia or Communications</param>
        /// <param name="defaultDeviceId">The new default deviceId.</param>
        void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            switch (flow)
            {
                case DataFlow.Render when role == Role.Multimedia:
                    SetDefaultAudioDevice(flow, role, defaultDeviceId, "System");
                    return;
                case DataFlow.Render when role == Role.Communications:
                    SetDefaultAudioDevice(flow, role, defaultDeviceId, "Chat");
                    return;
                case DataFlow.Capture when role == Role.Multimedia:
                    SetDefaultAudioDevice(flow, role, defaultDeviceId, "Chat Mic");
                    return;
                case DataFlow.Capture when role == Role.Communications:
                    SetDefaultAudioDevice(flow, role, defaultDeviceId, "Chat Mic");
                    return;
            }
        }

        void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            if (!_goXlrDevices.ContainsKey(deviceId))
                return;

            Console.WriteLine($"{DateTime.Now} [OnDeviceStateChanged]: {deviceId}, {newState}");
        }

        void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
        {
            if (!_goXlrDevices.ContainsKey(pwstrDeviceId))
                return;

            //This does not seem to do anything, if I unplug the device or replug it, I get:
            //OnDeviceStateChanged: Active -> NotPresent or NotPresent -> Active
            //Might have something to do if the drivers are installed or uninstalled?
            Console.WriteLine($"{DateTime.Now} [OnDeviceAdded]: {pwstrDeviceId}");
        }

        void IMMNotificationClient.OnDeviceRemoved(string deviceId)
        {
            if (!_goXlrDevices.ContainsKey(deviceId))
                return;
            
            //See OnDeviceAdded.
            Console.WriteLine($"{DateTime.Now} [OnDeviceRemoved]: {deviceId}");
        }

        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            if (!_goXlrDevices.ContainsKey(pwstrDeviceId))
                return;
            
            //Events triggered on change of different properties:
            //Ex. Volume change and mute, triggers one event.
            //Other like 16/24bit 48K HZ, triggers a lot of events.
            Console.WriteLine($"{DateTime.Now} [OnPropertyValueChanged]: {pwstrDeviceId} || {key.formatId} || {key.propertyId}");
        }

        private bool IsGoXLR(MMDevice device)
        {
            try
            {
                return device.DeviceFriendlyName
                    .IndexOf("TC-Helicon GoXLR", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch (COMException)
            {
                return false;
            }
        }

        private void SetApplicationDefaults(MMDevice device)
        {
            //TODO: Implement this method correctly (just for testing right now):
            //This method can be used to set defaults on application using the device.
            //Ex. ApplicationX should always be 100% volume and unmuted.
            //sessionManager.OnSessionCreated
            var sessionManager = device.AudioSessionManager;
            var sessions = sessionManager.Sessions;
            for (int i = 0; i < sessions.Count; i++)
            {
                var simpleAudioVolume = sessions[i].SimpleAudioVolume;
                simpleAudioVolume.Mute = false;
                simpleAudioVolume.Volume = 1;
            }
        }
    }
}