set AimpVersion=%~1
set ConfigurationName=%~2
set SolutionDir=%~3
set TargetName=%~4
set ProjectName=%~5
set OutDir=%~6
set TargetPath=%~7
set TargetDir=%~8

IF %ConfigurationName% == Debug (
    IF NOT EXIST "%SolutionDir%..\%AimpVersion%\Plugins\%TargetName%" mkdir "%SolutionDir%..\%AimpVersion%\Plugins\%TargetName%"
	copy "%TargetPath%" "%SolutionDir%..\%AimpVersion%\Plugins\%TargetName%\%ProjectName%_plugin.dll"
	xcopy 

	IF EXIST "%SolutionDir%AIMP.SDK\src\%ConfigurationName%\aimp_dotnet.dll" (
		copy "%SolutionDir%AIMP.SDK\src\%ConfigurationName%\aimp_dotnet.dll" "%SolutionDir%..\%AimpVersion%\Plugins\%TargetName%\%ProjectName%.dll"
	) ELSE (
		copy "%SolutionDir%packages\AimpDotNet\aimp_dotnet.dll" "%SolutionDir%..\%AimpVersion%\Plugins\%TargetName%\%ProjectName%.dll"
	)

	IF EXIST "%SolutionDir%AIMP.SDK\src\%ConfigurationName%\AIMP.SDK.dll" (
		copy "%SolutionDir%AIMP.SDK\src\%ConfigurationName%\AIMP.SDK.dll" "%SolutionDir%..\%AimpVersion%\Plugins\%TargetName%\AIMP.SDK.dll"
	) ELSE (
		copy "%SolutionDir%packages\AimpDotNet\AIMP.SDK.dll" "%SolutionDir%..\%AimpVersion%\Plugins\%TargetName%\AIMP.SDK.dll"
	)
)