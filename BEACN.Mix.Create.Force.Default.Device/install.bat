pushd "%~dp0"
sc create BEACN.Mix.Create.Force.Default.Device binPath="%CD%\BEACN.Mix.Create.Force.Default.Device.exe" start=auto
sc start BEACN.Mix.Create.Force.Default.Device
set /p DUMMY=Hit ENTER to continue...