@echo off
title Launching TROY Tweaks Windows Optimizer GUI...

if exist "%ProgramFiles%\dotnet\dotnet.exe" set "DOTNET_ROOT=%ProgramFiles%\dotnet"
if exist "%ProgramFiles%\dotnet\dotnet.exe" set "PATH=%ProgramFiles%\dotnet;%PATH%"

echo [*] Launching TROY Tweaks Windows Optimizer GUI (Latest Build)...
cd /d "%~dp0"
dotnet run --project "%~dp0src\NRTX.Optimizer.Gui\NRTX.Optimizer.Gui.csproj" --no-restore
if errorlevel 1 (
    echo [!] Dotnet run encountered an issue. Compiling and launching standalone executable...
    dotnet publish "%~dp0src\NRTX.Optimizer.Gui\NRTX.Optimizer.Gui.csproj" -c Release -r win-x64 --self-contained true -o "%~dp0publish\gui"
    if exist "%~dp0publish\gui\NRTX.Optimizer.Gui.exe" (
        start "" "%~dp0publish\gui\NRTX.Optimizer.Gui.exe"
    )
)
