using CoreAudioApi;

namespace BEACN.Mix.Create.Force.Default.Device.Models
{
    public class AudioDeviceExtended
    {
        // Order in which this MMDevice appeared from MMDeviceEnumerator
        public int Index;

        // Default (for its Type) is either true or false
        public bool Default;

        public bool DefaultCommunication;

        // Type is either "Playback" or "Recording"
        public string Type;

        // Name of the MMDevice ex: "Speakers (Realtek High Definition Audio)"
        public string Name;

        // ID of the MMDevice ex: "{0.0.0.00000000}.{c4aadd95-74c7-4b3b-9508-b0ef36ff71ba}"
        public string ID;

        // The MMDevice itself
        public MMDevice Device;

        // To be created, a new AudioDevice needs an Index, and the MMDevice it will communicate with
        public AudioDeviceExtended(int Index, MMDevice BaseDevice, bool Default = false, bool DefaultCommunication = false)
        {
            // Set this object's Index to the received integer
            this.Index = Index;

            // Set this object's Default to the received boolean
            this.Default = Default;

            this.DefaultCommunication = DefaultCommunication;

            // If the received MMDevice is a playback device
            if (BaseDevice.DataFlow == EDataFlow.eRender)
            {
                // Set this object's Type to "Playback"
                this.Type = "Playback";
            }
            // If not, if the received MMDevice is a recording device
            else if (BaseDevice.DataFlow == EDataFlow.eCapture)
            {
                // Set this object's Type to "Recording"
                this.Type = "Recording";
            }

            // Set this object's Name to that of the received MMDevice's FriendlyName
            this.Name = BaseDevice.FriendlyName;

            // Set this object's Device to the received MMDevice
            this.Device = BaseDevice;

            // Set this object's ID to that of the received MMDevice's ID
            this.ID = BaseDevice.ID;
        }
    }
}
