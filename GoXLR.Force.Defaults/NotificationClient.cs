using System;
using System.Collections.Generic;
using System.Linq;
using AudioDeviceCmdlets;
using CoreAudioApi;

namespace GoXLR.Force.Defaults
{
    public class NotificationClient : IMMNotificationClient
    {
        private readonly Dictionary<string, MMDevice> _devices = new Dictionary<string, MMDevice>();
        private readonly MMDeviceEnumerator _deviceEnumerator;
        private readonly PolicyConfigClient _configClient;
        
        public NotificationClient()
        {
            _configClient = new PolicyConfigClient();

            _deviceEnumerator = new MMDeviceEnumerator();
            _deviceEnumerator.RegisterEndpointNotificationCallback(this);

            var defaultRenderMultimedia = _deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            var defaultRenderCommunication = _deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eCommunications);
            var defaultCaptureMultimedia = _deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eMultimedia);
            var defaultCaptureCommunication = _deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eCommunications);

            var devices = _deviceEnumerator.EnumerateAudioEndPoints(EDataFlow.eAll, EDeviceState.DEVICE_STATEMASK_ALL);
            var count = devices.Count;

            for (int i = 0; i < count; i++)
            {
                var mmDevice = devices[i];
                _devices[mmDevice.ID] = mmDevice;
            }

            //Set initial defaults:
            OnDefaultDeviceChanged(EDataFlow.eRender, ERole.eMultimedia, defaultRenderMultimedia.ID);
            OnDefaultDeviceChanged(EDataFlow.eRender, ERole.eCommunications, defaultRenderCommunication.ID);
            OnDefaultDeviceChanged(EDataFlow.eCapture, ERole.eMultimedia, defaultCaptureMultimedia.ID);
            OnDefaultDeviceChanged(EDataFlow.eCapture, ERole.eCommunications, defaultCaptureCommunication.ID);
        }

        public int OnDeviceStateChanged(string pwstrDeviceId, int dwNewState)
        {
            //Does nothing?
            Console.WriteLine($"OnDeviceStateChanged: {pwstrDeviceId}, {dwNewState}");
            return 0;
        }

        public int OnDeviceAdded(string pwstrDeviceId)
        {
            //TODO: Implement:
            Console.WriteLine($"OnDeviceAdded: {pwstrDeviceId}");
            return 0;
        }

        public int OnDeviceRemoved(string pwstrDeviceId)
        {
            //TODO: Implement:
            Console.WriteLine($"OnDeviceRemoved: {pwstrDeviceId}");
            return 0;
        }

        private void SetAudioDevice(EDataFlow flow, ERole role, string pwstrDefaultDeviceId, string searchString)
        {
            if (string.IsNullOrWhiteSpace(pwstrDefaultDeviceId))
                return;

            pwstrDefaultDeviceId = pwstrDefaultDeviceId.Replace("\0", string.Empty);

            var mmDevice = _devices
                .Values
                .Where(device => device.State == EDeviceState.DEVICE_STATE_ACTIVE)
                .Where(device => device.DataFlow == flow)
                .Where(device => device.FriendlyName.StartsWith(searchString))
                .SingleOrDefault();

            if (mmDevice == null)
                return;

            if (pwstrDefaultDeviceId == mmDevice.ID)
                return;

            _configClient.SetDefaultEndpoint(mmDevice.ID, role);
        }

        public int OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId)
        {
            if (flow == EDataFlow.eRender && role == ERole.eMultimedia)
            {
                SetAudioDevice(flow, role, pwstrDefaultDeviceId, "System");
                return 0;
            }
            
            if (flow == EDataFlow.eRender && role == ERole.eCommunications)
            {
                SetAudioDevice(flow, role, pwstrDefaultDeviceId, "Chat");
                return 0;
            }

            if (flow == EDataFlow.eCapture && role == ERole.eMultimedia)
            {
                SetAudioDevice(flow, role, pwstrDefaultDeviceId, "Chat Mic");
                return 0;
            }

            if (flow == EDataFlow.eCapture && role == ERole.eCommunications)
            {
                SetAudioDevice(flow, role, pwstrDefaultDeviceId, "Chat Mic");
                return 0;
            }

            Console.WriteLine($"OnDefaultDeviceChanged: {flow}, {role}, {pwstrDefaultDeviceId}");
            return 0;
        }

        public int OnPropertyValueChanged(string pwstrDeviceId, ref PropertyKey key)
        {
            //Does nothing?
            Console.WriteLine($"OnPropertyValueChanged: {pwstrDeviceId}, {key.fmtid} ... {key.pid}");
            return 0;
        }
    }
}