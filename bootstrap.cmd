@echo off
Nuget.exe restore "Source\FluentCommand.net40.sln"
Nuget.exe restore "Source\FluentCommand.net45.sln"

NuGet.exe install MSBuildTasks -OutputDirectory .\Tools\ -ExcludeVersion -NonInteractive
NuGet.exe install xunit.runner.console -OutputDirectory .\Tools\ -ExcludeVersion -NonInteractive
