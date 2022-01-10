using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace BEACN.Mix.Create.Force.Default.Device.Com
{
    public class PolicyConfigClient
    {
        private readonly IPolicyConfig _policyConfig;
        private readonly IPolicyConfigVista _policyConfigVista;
        private readonly IPolicyConfig10 _policyConfig10;

        public PolicyConfigClient()
        {
            _policyConfig = new PolicyConfigClientComObject() as IPolicyConfig;
            if (_policyConfig != null)
                return;

            _policyConfigVista = new PolicyConfigClientComObject() as IPolicyConfigVista;
            if (_policyConfigVista != null)
                return;

            _policyConfig10 = new PolicyConfigClientComObject() as IPolicyConfig10;
        }

        public void SetDefaultEndpoint(string deviceId, Role role)
        {
            if (_policyConfig != null)
            {
                var errorCode = _policyConfig.SetDefaultEndpoint(deviceId, role);
                Marshal.ThrowExceptionForHR(errorCode);
                return;
            }
            if (_policyConfigVista != null)
            {
                var errorCode = _policyConfigVista.SetDefaultEndpoint(deviceId, role);
                Marshal.ThrowExceptionForHR(errorCode);
                return;
            }
            if (_policyConfig10 != null)
            {
                var errorCode = _policyConfig10.SetDefaultEndpoint(deviceId, role);
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }
    }
}
