@echo off
NuGet.exe install MSBuildTasks -OutputDirectory .\tools\ -ExcludeVersion -NonInteractive
dotnet tool install coveralls.net --version 1.0.0 --tool-path tools
