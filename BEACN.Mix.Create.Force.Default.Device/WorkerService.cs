using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using CoreAudioApi;
using BEACN.Mix.Create.Force.Default.Device.Helpers;

namespace BEACN.Mix.Create.Force.Default.Device
{
    public class WorkerService : ServiceBase
    {
        private readonly Thread _workerThread = null;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        public WorkerService()
        {
            var name = "BEACN Mix Create Force Default Device";

            base.ServiceName = name;
            base.CanStop = true;
            base.CanPauseAndContinue = true;
            base.AutoLog = true;

            _workerThread = new Thread(WorkerMethod)
            {
                Name = $"{name} Worker Thread",
                IsBackground = true
            };
            _workerThread.Start();
        }

        private void WorkerMethod()
        {
            while (true)
            {
                try
                {
                    _manualResetEvent.WaitOne();

                    var audioDevices = AudioDeviceHelper.GetAllDevices()
                        .Where(audioDevice => audioDevice.Name.IndexOf("BEACN Mix Create", StringComparison.OrdinalIgnoreCase) >= 0);

                    foreach (var audioDevice in audioDevices)
                    {
                        var device = audioDevice.Device;

                        //Un-mute if muted:
                        if (device.AudioEndpointVolume.Mute)
                        {
                            device.AudioEndpointVolume.Mute = false;
                        }

                        //Set Volume to 100% if lower:
                        if (device.AudioEndpointVolume.MasterVolumeLevelScalar < 1)
                        {
                            device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
                        }

                        if (audioDevice.Name.StartsWith("Chat Mic"))
                        {
                            if (!audioDevice.Default)
                            {
                                AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eMultimedia);
                            }

                            if (!audioDevice.DefaultCommunication)
                            {
                                AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eCommunications);
                            }
                        }

                        if (audioDevice.Name.StartsWith("System"))
                        {
                            if (!audioDevice.Default)
                            {
                                AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eMultimedia);
                            }
                        }

                        if (audioDevice.Name.StartsWith("Chat"))
                        {
                            if (!audioDevice.DefaultCommunication)
                            {
                                AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eCommunications);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    Thread.Sleep(5000);
                }
                catch (ThreadInterruptedException)
                {
                    //Thread was interupted (service stoped).
                }
            }
        }

#if DEBUG
        public void Start()
        {
            OnStart(null);
        }
#endif

        protected override void OnStart(string[] args)
        {
            try
            {
                _manualResetEvent.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnContinue()
        {
            try
            {
                _manualResetEvent.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                _manualResetEvent.Reset();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                _manualResetEvent.Reset();
                _workerThread.Interrupt();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}