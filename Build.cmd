@echo off

if not exist "nuget.exe" (
powershell -command "(New-Object System.Net.WebClient).DownloadFile('https://dist.nuget.org/win-x86-commandline/latest/nuget.exe', 'nuget.exe')"
)

nuget.exe install Newtonsoft.Json -OutputDirectory packages -Version 13.0.1

echo.
echo --------------------------------------------------------
echo.

rem Remove "coming-from-the-internet-flag" recursively
powershell "Get-ChildItem -Recurse | Unblock-File"

C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe .\SetupTool.sln -property:Configuration=Release

xcopy .\SetupTool\bin\Release\SetupTool.exe . /Y
xcopy .\SetupTool\bin\Release\Newtonsoft.Json.dll . /Y