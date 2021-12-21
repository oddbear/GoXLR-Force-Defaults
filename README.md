# This is a fork used to change the Windows defaults for the BEACN Mix Create audio devices

### Orignally created by oddbear


This service is made because Windows is quite eager to change the default sound devices. <br />
It will check every 5 seconds if this change has happened, and change this back.<br />
Additionally it will also set all the GoXLR Channels to 100% Volume, and unmute them if detected.

### Download

Download latest from [releases](https://github.com/GinjahWolf92/GoXLR-Force-Defaults/releases/download/v1.0/v1.0.zip)

### Install

1. Extract zip file to a install location on the disk.
2. Open up cmd.exe as Administrator (you will need this access to install a service).
3. Run the following command to install the application as a windows service (you will need to change the path part):
> sc create BEACN.Mix.Create.Force.Default.Device binPath="`<change path here>`\v1.0\BEACN Mix Create - Force Default Device.exe" start=auto
4. To start the service, run the fallowing command:
> sc start BEACN.Mix.Create.Force.Default.Device

### Uninstall

1. Run cmd.exe as administrator
2. Run the fallowing command to stop the service:
> sc stop BEACN.Mix.Create.Force.Default.Device
3. Run the following command to delete the service:
> sc delete BEACN.Mix.Create.Force.Default.Device
4. Delete the folder

### Issues ?

If you get any issues, please [create a ticket](https://github.com/GinjahWolf92/GoXLR-Force-Defaults/issues).

### Dependencies
* [AudioDeviceCmdlets](https://github.com/frgnca/AudioDeviceCmdlets)

### Similar projected (changes defaults)
- [EarTrumpet](https://github.com/File-New-Project/EarTrumpet/)
- [AudioDeviceCmdlets](https://github.com/frgnca/AudioDeviceCmdlets)
- [SoundSwitch](https://github.com/Belphemur/SoundSwitch)
  - [AudioEndPointLibrary](https://github.com/Belphemur/AudioEndPointLibrary)
Any more?
