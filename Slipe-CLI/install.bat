@echo off

:: Request admin rights
if not "%1"=="am_admin" (powershell start -verb runas '%0' am_admin & exit /b)

:: Change directory
cd %~dp0

:: Create target directory
IF NOT EXIST "\Program Files (x86)\Slipe" (
	mkdir "\Program Files (x86)\Slipe" > NUL
)

:: Copy files into target directory
robocopy ".\Slipe" "C:\Program Files (x86)\Slipe" /s > NUL


:: Add directory to path if it does not yet exist
echo ;%PATH%; | find /C /I ";C:\Program Files (x86)\Slipe;" > temp
set /p hasValue=<temp

IF "%hasValue%" == "0" (
	echo "ADDING TO PATH"
	setx path "%PATH%;C:\Program Files (x86)\Slipe" /M
)

set /p temp="Installation is completed. Press enter to close..."