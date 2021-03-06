# GoXLR-Force-Defaults

This service is made because Windows is quite eager to change the default sound devices. <br />
It will check every 5 seconds if this change has happened, and change this back.<br />
Additionally it will also set all the GoXLR Channels to 100% Volume, and unmute them if detected.

### Download

Download latest from [releases](https://github.com/oddbear/GoXLR-Force-Defaults/releases/download/v1/publish.zip)

### Install

1. Extract zip file to a install location on the disk.
2. Open up cmd.exe as Administrator (you will need this access to install a service).
3. Run the fallowing command to install the application as a windows service (you will need to change the path part):
> sc create GoXLR.Force.Defaults binPath="`<change path here>`\publish\GoXLR.Force.Defaults.exe" start=auto
4. To start the service, run the fallowing command:
> sc start GoXLR.Force.Defaults

### Uninstall

1. Run cmd.exe as administrator
2. Run the fallowing command to stop the service:
> sc stop GoXLR.Force.Defaults
3. Run the following command to delete the service:
> sc delete GoXLR.Force.Defaults
4. Delete the folder

### Alternative install
Alternativly you can run this as a hidden background task under Task Scheduler.
1. Unzip to `%appdata%` folder, rename the new folder publish to whatever you want.
2. Create a scheduled task that:
> Under General: check `Hidden` and `Run whether user is logged in or not`.<br />
> Under triggers: trigger `At startup`<br />
> Under Actions: `Start a program`, and point to the exe file `GoXLR.Force.Defaults.exe`, with argument `standalone`.

### Issues ?

If you get any issues, please [create a ticket](https://github.com/oddbear/GoXLR-Force-Defaults/issues).

### Dependencies
* [AudioDeviceCmdlets](https://github.com/frgnca/AudioDeviceCmdlets)

### Similar projected (changes defaults)
- [EarTrumpet](https://github.com/File-New-Project/EarTrumpet/)
- [AudioDeviceCmdlets](https://github.com/frgnca/AudioDeviceCmdlets)
- [SoundSwitch](https://github.com/Belphemur/SoundSwitch)
  - [AudioEndPointLibrary](https://github.com/Belphemur/AudioEndPointLibrary)
Any more?
