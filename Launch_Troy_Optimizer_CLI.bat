@echo off
title TROY Tweaks Windows Optimizer CLI Suite

if exist "%ProgramFiles%\dotnet\dotnet.exe" set "DOTNET_ROOT=%ProgramFiles%\dotnet"
if exist "%ProgramFiles%\dotnet\dotnet.exe" set "PATH=%ProgramFiles%\dotnet;%PATH%"

cd /d "%~dp0"
dotnet run --project "%~dp0src\NRTX.Optimizer.Cli\NRTX.Optimizer.Cli.csproj" --no-restore -- %*
if errorlevel 1 (
    echo [!] Dotnet run encountered an issue. Falling back to published CLI executable...
    if exist "%~dp0publish\cli\NRTX.Optimizer.Cli.exe" (
        "%~dp0publish\cli\NRTX.Optimizer.Cli.exe" %*
    )
)

if "%~1"=="" pause
