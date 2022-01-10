pushd "%~dp0"
sc create GoXLR.Force.Defaults binPath="%CD%\GoXLR.Force.Defaults.exe" start=auto
sc start GoXLR.Force.Defaults
set /p DUMMY=Hit ENTER to continue...