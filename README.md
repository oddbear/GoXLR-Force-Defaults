# Force Default Audio Devices for BEACN Mix Create

### NOTE: Orignally created for use with GoXLR but all functionality has been adapted to suit the BEACN Mix Create.

This service is made because Windows is quite eager to change the default sound devices. <br />
It will check every 5 seconds if this change has happened, and change this back.<br />
Additionally it will also set all the BEACN Devices to 100% Volume, and unmute them if detected.

### Download

Download the latest .zip from [releases](https://github.com/GinjahWolf92/BEACN-Mix-Create-Force-Default/releases).

### Install

1. Extract .zip file to an install location on the disk.
2. Run the install.bat file **as an administrator** (required to install the service).

### Alternative install
Alternativly you can run this as a hidden background task under Task Scheduler.
1. Unzip to `%appdata%` folder, rename the new folder publish to whatever you want.
2. Create a scheduled task that:
> Under General: check `Hidden` and `Run whether user is logged in or not`.<br />
> Under triggers: trigger `At startup`<br />
> Under Actions: `Start a program`, and point to the exe file `GoXLR.Force.Defaults.exe`, with argument `standalone`.

### Uninstall

1. Run the uninstall.bat file **as an administrator** (required to remove the service).
2. Delete the folder
