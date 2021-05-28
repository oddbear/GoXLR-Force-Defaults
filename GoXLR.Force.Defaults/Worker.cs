using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreAudioApi;
using GoXLR.Force.Defaults.Helpers;

namespace GoXLR.Force.Defaults
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var audioDevices = AudioDeviceHelper.GetAllDevices()
                    .Where(audioDevice => audioDevice.Name.EndsWith("(TC-Helicon GoXLR)"));

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
                    
                    if (audioDevice.Name == "Chat Mic (TC-Helicon GoXLR)")
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

                    if (audioDevice.Name == "System (TC-Helicon GoXLR)")
                    {
                        if (!audioDevice.Default)
                        {
                            AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eMultimedia);
                        }
                    }

                    if (audioDevice.Name == "Chat (TC-Helicon GoXLR)")
                    {
                        if (!audioDevice.DefaultCommunication)
                        {
                            AudioDeviceHelper.SetDefaultDeviceForRole(audioDevice, ERole.eCommunications);
                        }
                    }
                }
                
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
