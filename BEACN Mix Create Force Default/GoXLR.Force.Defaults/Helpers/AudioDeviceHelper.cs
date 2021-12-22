using System;
using System.Collections.Generic;
using CoreAudioApi;
using GoXLR.Force.Defaults.Models;

namespace GoXLR.Force.Defaults.Helpers
{
    public static class AudioDeviceHelper
    {
        /// <summary>
        /// Parameter called to list all devices
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<AudioDeviceExtended> GetAllDevices()
        {
            var audioDevices = new List<AudioDeviceExtended>();

            // Create a new MMDeviceEnumerator
            var devEnum = new MMDeviceEnumerator();

            // Create a MMDeviceCollection of every devices that are enabled
            var deviceCollection = devEnum.EnumerateAudioEndPoints(EDataFlow.eAll, EDeviceState.DEVICE_STATE_ACTIVE);

            var defaultPlaybackDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            var defaultPlaybackCommunicationDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eCommunications);
            var defaultRecordingDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eMultimedia);
            var defaultRecordingCommunicationDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eCommunications);

            // For every MMDevice in DeviceCollection
            for (int i = 0; i < deviceCollection.Count; i++)
            {
                var isDefault = deviceCollection[i].ID == defaultPlaybackDevice.ID
                             || deviceCollection[i].ID == defaultRecordingDevice.ID;

                var isDefaultCommunication = deviceCollection[i].ID == defaultPlaybackCommunicationDevice.ID
                                          || deviceCollection[i].ID == defaultRecordingCommunicationDevice.ID;

                audioDevices.Add(new AudioDeviceExtended(i + 1, deviceCollection[i], isDefault, isDefaultCommunication));
            }

            return audioDevices;
        }

        /// <summary>
        /// Parameter receiving the ID of the device to set as default
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="eRole"></param>
        /// <returns></returns>
        public static AudioDeviceExtended SetDefaultDeviceForRole(AudioDeviceExtended audioDevice, ERole eRole)
        {
            // Create a new MMDeviceEnumerator
            var devEnum = new MMDeviceEnumerator();

            // Create a MMDeviceCollection of every devices that are enabled
            var deviceCollection = devEnum.EnumerateAudioEndPoints(EDataFlow.eAll, EDeviceState.DEVICE_STATE_ACTIVE);

            // For every MMDevice in DeviceCollection
            for (int i = 0; i < deviceCollection.Count; i++)
            {
                // If this MMDevice's ID is the same as the string received by the ID parameter
                if (string.Compare(deviceCollection[i].ID, audioDevice.ID, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    // Create a new audio PolicyConfigClient
                    var client = new PolicyConfigClient();

                    // Using PolicyConfigClient, set the given device as the default device (for its type)
                    client.SetDefaultEndpoint(deviceCollection[i].ID, eRole);

                    // Output the result of the creation of a new AudioDevice while assigning it the index, and the MMDevice itself, and a default value of true
                    return new AudioDeviceExtended(i + 1, deviceCollection[i], eRole == ERole.eMultimedia, eRole == ERole.eCommunications);
                }
            }

            // Throw an exception about the received ID not being found
            throw new ArgumentException("No enabled AudioDevice found with that ID");
        }
    }
}
