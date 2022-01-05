using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using NAudio.CoreAudioApi;

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

        private void WorkerMethod()
        {
            while (true)
            {
                try
                {
                    _manualResetEvent.WaitOne();

                    using (var enumerator = new MMDeviceEnumerator())
                    {
                        var defaultRenderMultimedia = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                        var defaultRenderCommunications = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications);
                        var defaultCaptureMultimedia = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                        var defaultCaptureCommunications = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);

                        var audioDevices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All);

                        foreach (var device in audioDevices)
                        {
                            var deviceName = device.DeviceFriendlyName;
                            if (deviceName.IndexOf("TC-Helicon GoXLR", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                device.Dispose();
                                continue;
                            }

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

                            if (device.FriendlyName.StartsWith("Chat Mic"))
                            {
                                if (device == defaultCaptureMultimedia)
                                {
                                    //TODO: Any implementation of this in NAudio, or do I need something custom?
                                    //AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eMultimedia);
                                    Console.WriteLine("Chat is not default capture multimedia");
                                }

                                if (device == defaultCaptureCommunications)
                                {
                                    //TODO: Any implementation of this in NAudio, or do I need something custom?
                                    //AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eCommunications);
                                    Console.WriteLine("Chat is not default capture communication");
                                }
                            }

                            if (device.FriendlyName.StartsWith("System"))
                            {
                                if (device == defaultRenderMultimedia)
                                {
                                    //TODO: Any implementation of this in NAudio, or do I need something custom?
                                    //AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eMultimedia);
                                    Console.WriteLine("Chat is not default render multimedia");
                                }
                            }

                            if (device.FriendlyName.StartsWith("Chat"))
                            {
                                if (device == defaultRenderCommunications)
                                {
                                    //TODO: Any implementation of this in NAudio, or do I need something custom?
                                    //AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eCommunications);
                                    Console.WriteLine("Chat is not default render communication");
                                }
                            }

                            device.Dispose();
                        }

                        defaultRenderMultimedia.Dispose();
                        defaultRenderCommunications.Dispose();
                        defaultCaptureMultimedia.Dispose();
                        defaultCaptureCommunications.Dispose();
                        
                        //Not Working :(
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
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
