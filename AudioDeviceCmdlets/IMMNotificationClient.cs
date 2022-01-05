using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CoreAudioApi;

namespace AudioDeviceCmdlets
{
    [ComImport]
    [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMNotificationClient
    {
        int OnDeviceStateChanged(string pwstrDeviceId, int dwNewState);

        int OnDeviceAdded(string pwstrDeviceId);

        int OnDeviceRemoved(string pwstrDeviceId);

        int OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId);

        int OnPropertyValueChanged(string pwstrDeviceId, ref PropertyKey key);
    }
}
