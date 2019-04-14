@echo off

:: DEBUGGING: Set SLIPE_INSTALLER_DEBUG env variable to 1 (`setx SLIPE_INSTALLER_DEBUG 1` from command line)

:: Request admin rights
if not "%1"=="am_admin" (
	call :trace "Might be running as non-admin, elevating"
	powershell start -verb runas '%0' am_admin & exit /b 0
)

:: We're working in current script's directory
cd %~dp0
call :trace "Working directory is %~dp0"

set slipe_dir=%programfiles(x86)%\Slipe
call :trace "Destination directory is %slipe_dir%"

:: Create target directory
IF NOT EXIST "%slipe_dir%" (
	call :trace "Destination directory does not exist, creating"
	if "%slipe_installer_debug%" == "1" (
		mkdir "%slipe_dir%"
	) else (
		mkdir "%slipe_dir%" > NUL
	)
	call :trace "After destination directory creation"
)

IF NOT EXIST "%slipe_dir%" (
	call :fatal "The directory %slipe_dir% does not exist and could not be created for some reason. Please report this issue in our Github project."
	exit /b %errorlevel%
)

:: Copy files into target directory
call :trace "About to copy files"

if "%slipe_installer_debug%" == "1" (
	robocopy "%~dp0\Slipe" "%slipe_dir%" /s
) else (
	robocopy "%~dp0\Slipe" "%slipe_dir%" /s > NUL
)

if errorlevel 1 (
	call :fatal "Some files could not be copied"
	exit /b %errorlevel%
)


:: Add directory to path if it does not yet exist
call :trace "Checking PATH"
call :is_in_path is_in_path
call :trace "Result: %is_in_path%"

IF "%is_in_path%" == "0" (
	echo Slipe is not in PATH, adding...
	call :add_to_system_path "%slipe_dir%"
	call :trace "ERRORLEVEL = %errorlevel%"
	call :trace "PATH = %PATH%"
	echo Slipe has been succesfully added to PATH. Remember to restart any already running command line windows, otherwise the 'slipe' command won't be recognized by them.
)

set /p temp="Installation completed succesfully. Press enter to close..."

goto :success

:trace
if "%slipe_installer_debug%" == "1" echo %~1
exit /b 0

:fatal
echo ERROR: %~1
set /p temp="Installation failed. Press enter to close..."
exit /b 1

:is_in_path
echo ;%PATH%; | find /C /I ";%slipe_dir%;" > temp
set /p found=<temp
set "%~1=%found%"
exit /b 0

:add_to_system_path
:: WARNING: Simply calling `setx path "%PATH;..."` is dangerous and often fails miserably.
:: This tool is much safer workaround
:: For more info, see: https://stackoverflow.com/a/41379378/2279914
"%~dp0\helpers\pathed.exe" /append "%~1" /machine
exit /b 0

:success
exit /b 0