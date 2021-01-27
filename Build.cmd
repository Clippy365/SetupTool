C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe .\SetupTool.sln -property:Configuration=Release

xcopy .\SetupTool\bin\Release\SetupTool.exe .
xcopy .\SetupTool\bin\Release\Newtonsoft.Json.dll .