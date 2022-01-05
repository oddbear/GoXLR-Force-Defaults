using System;
using System.ServiceProcess;
using System.Threading;
using AudioDeviceCmdlets;
using CoreAudioApi;

namespace GoXLR.Force.Defaults
{
    public class WorkerService : ServiceBase
    {
        private readonly Thread _workerThread = null;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        public WorkerService()
        {
            var name = "GoXLR Force Defaults";

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

        public class NotificationClient : IMMNotificationClient
        {
            public int OnDeviceStateChanged(string pwstrDeviceId, int dwNewState)
            {
                Console.WriteLine($"OnDeviceStateChanged: {pwstrDeviceId}, {dwNewState}");
                return 0;
            }

            public int OnDeviceAdded(string pwstrDeviceId)
            {
                Console.WriteLine($"OnDeviceAdded: {pwstrDeviceId}");
                return 0;
            }

            public int OnDeviceRemoved(string pwstrDeviceId)
            {
                Console.WriteLine($"OnDeviceRemoved: {pwstrDeviceId}");
                return 0;
            }

            public int OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId)
            {
                Console.WriteLine($"OnDefaultDeviceChanged: {flow}, {role}, {pwstrDefaultDeviceId}");
                return 0;
            }

            public int OnPropertyValueChanged(string pwstrDeviceId, ref PropertyKey key)
            {
                Console.WriteLine($"OnPropertyValueChanged: {pwstrDeviceId}, {key.fmtid} ... {key.pid}");
                return 0;
            }
        }

        private void WorkerMethod()
        {
            var enumerator = new MMDeviceEnumerator();

            var client = new NotificationClient();
            enumerator.RegisterEndpointNotificationCallback(client);

            while (true)
            {
                try
                {
                    _manualResetEvent.WaitOne();

                    //var audioDevices = enumerator.EnumerateAudioEndPoints(EDataFlow.eAll, EDeviceState.DEVICE_STATE_ACTIVE);

                    //for (int i = 0; i < audioDevices.Count; i++)
                    //{
                    //    var device = audioDevices[i];
                    //    if (device.FriendlyName.IndexOf("TC-Helicon GoXLR", StringComparison.OrdinalIgnoreCase) < 0)
                    //        continue;
                        
                    //    //Un-mute if muted:
                    //    if (device.AudioEndpointVolume.Mute)
                    //    {
                    //        device.AudioEndpointVolume.Mute = false;
                    //    }

                    //    //Set Volume to 100% if lower:
                    //    if (device.AudioEndpointVolume.MasterVolumeLevelScalar < 1)
                    //    {
                    //        device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
                    //    }

                    //    if (device.FriendlyName.StartsWith("Chat Mic"))
                    //    {
                    //        //if (!device.Default)
                    //        {
                    //            //AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eMultimedia);
                    //        }

                    //        //if (!device.DefaultCommunication)
                    //        {
                    //            //AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eCommunications);
                    //        }
                    //    }

                    //    if (device.FriendlyName.StartsWith("System"))
                    //    {
                    //        //if (!audioDevice.Default)
                    //        {
                    //            //AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eMultimedia);
                    //        }
                    //    }

                    //    if (device.FriendlyName.StartsWith("Chat"))
                    //    {
                    //        //if (!audioDevice.DefaultCommunication)
                    //        {
                    //            //AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eCommunications);
                    //        }
                    //    }
                    //}
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    Thread.Sleep(5);
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
