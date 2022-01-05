using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using AudioSwitcher.AudioApi.CoreAudio;

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
            var controller = new CoreAudioController();
            while (true)
            {
                try
                {
                    _manualResetEvent.WaitOne();

                    var audioDevices = controller
                        .GetDevices()
                        .Where(device => device.InterfaceName.IndexOf("TC-Helicon GoXLR", StringComparison.OrdinalIgnoreCase) >= 0);

                    foreach (var audioDevice in audioDevices)
                    {
                        //Un-mute if muted:
                        if (audioDevice.IsMuted)
                        {
                            audioDevice.Mute(false);
                        }

                        //Set Volume to 100% if lower:
                        if (audioDevice.Volume < 100)
                        {
                            audioDevice.Volume = 100;
                        }

                        if (audioDevice.Name.StartsWith("Chat Mic"))
                        {
                            if (!audioDevice.IsDefaultDevice)
                            {
                                audioDevice.SetAsDefault();
                            }

                            if (!audioDevice.IsDefaultCommunicationsDevice)
                            {
                                audioDevice.SetAsDefaultCommunications();
                            }
                        }

                        if (audioDevice.Name.StartsWith("System"))
                        {
                            if (!audioDevice.IsDefaultDevice)
                            {
                                audioDevice.SetAsDefault();
                            }
                        }

                        if (audioDevice.Name.StartsWith("Chat"))
                        {
                            if (!audioDevice.IsDefaultCommunicationsDevice)
                            {
                                audioDevice.SetAsDefaultCommunications();
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
                    Thread.Sleep(TimeSpan.FromMilliseconds(5)); //5000
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
