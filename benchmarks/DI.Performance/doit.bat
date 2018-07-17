@echo off
setlocal

rem Redirect away from %USER%\.nuget\packages to avoid picking up a stale package.
set NUGET_PACKAGES=%~dp0\..\..\packages-cache

dotnet publish -c Release -r win-x64 || exit /b 1

%~dp0\bin\Release\netcoreapp2.2\win-x64\publish\DI.Performance.exe --config profile 1

