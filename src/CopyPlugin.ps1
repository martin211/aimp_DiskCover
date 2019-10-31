[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)]
    [string] $ConfigurationName,
    [Parameter(Mandatory=$true)]
    [string] $TargetName,
    [string] $TargetDir,
    [string] $SolutionDir,
    [string] $ProjectDir,
    [string] $AimpVersion = "AIMP4.60"
)

Write-Output "Windows PowerShell $($Host.Version)"

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { exit 1 }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

###########################################################################
# CONFIGURATION
###########################################################################

$CorePath = "$SolutionDir\$ConfigurationName"
$RootFolder = $(Get-Item $SolutionDir).Parent.FullName
$OutputPath = "$RootFolder\$AimpVersion\Plugins\$TargetName"
$exclude = @('aimp_dotnet.dll','AIMP.SDK.dll')

###########################################################################

function CopyCore {
	$coreLibrary = "$TargetDir\aimp_dotnet.dll"
	$coreSdk = "$TargetDir\AIMP.SDK.dll"

	Write-Output "Copy: $coreLibrary to: $OutputPath"
	Write-Output "Copy: $coreSdk to: $OutputPath"

    Copy-Item $coreLibrary -Destination "$OutputPath\$TargetName.dll" -Force
    Copy-Item $coreSdk -Destination "$OutputPath\AIMP.SDK.dll" -Force
}

function CopyPlugin {
    $childs = Get-ChildItem "$TargetDir\*.dll" -Exclude $exclude
    ForEach ($file in $childs) {
        if ($file.BaseName -eq $TargetName) {
            Copy-Item $file.FullName -Destination "$OutputPath\${TargetName}_plugin.dll"
        }
        else {
            Copy-Item $file.FullName -Destination "$OutputPath"
        }
    }
}

function CopyLang {
	Copy-Item -Path "$ProjectDir\langs\" -Filter "*.lng" -Recurse -Destination "$OutputPath\Langs\" -Force -Container
}

Write-Output "Copy plugin: $TargetName"
Write-Output "Configuration: $ConfigurationName"
Write-Output "TargetName: $TargetName"
Write-Output "TargetDir: $TargetDir"
Write-Output "SolutionDir: $SolutionDir"
Write-Output "ProjectDir: $ProjectDir"
Write-Output "AimpVersion: $AimpVersion"
Write-Output "Output: $OutputPath"

if ((Test-Path $OutputPath) -eq $false) {
    New-Item -ItemType directory -Path $OutputPath
}

Write-Output "Copy Plugin libraries"
CopyPlugin

Write-Output "Copy Core libraries"
CopyCore

Write-Output "Copy Language files"
CopyLang