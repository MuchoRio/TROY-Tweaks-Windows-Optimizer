@echo off
title TROY Tweaks Windows Optimizer Master Compiler Suite

if exist "%ProgramFiles%\dotnet\dotnet.exe" set "DOTNET_ROOT=%ProgramFiles%\dotnet"
if exist "%ProgramFiles%\dotnet\dotnet.exe" set "PATH=%ProgramFiles%\dotnet;%PATH%"

echo ===================================================================
echo   TROY Tweaks Windows Optimizer - Master Build and Compiler Suite
echo   NRTX Labs (C) 2026
echo ===================================================================
echo.
echo [*] Step 1/3: Running Automated Test Suite (Quality Gate)...
dotnet test "%~dp0NRTX.WindowsOptimizer.sln" -c Release --nologo
if errorlevel 1 (
    echo.
    echo [!] Unit tests gagal! Kompilasi dibatalkan.
    pause
    exit /b 1
)

echo.
echo [*] Step 2/3: Compiling and Publishing GUI (Desktop App)...
dotnet publish "%~dp0src\NRTX.Optimizer.Gui\NRTX.Optimizer.Gui.csproj" -c Release -r win-x64 --self-contained true -o "%~dp0publish\gui" --nologo
if errorlevel 1 (
    echo.
    echo [!] Kompilasi GUI gagal!
    pause
    exit /b 1
)

echo.
echo [*] Step 3/3: Compiling and Publishing CLI (Terminal App)...
dotnet publish "%~dp0src\NRTX.Optimizer.Cli\NRTX.Optimizer.Cli.csproj" -c Release -r win-x64 --self-contained true -o "%~dp0publish\cli" --nologo
if errorlevel 1 (
    echo.
    echo [!] Kompilasi CLI gagal!
    pause
    exit /b 1
)

echo.
echo ===================================================================
echo [SUCCESS] Semua modul TROY berhasil dikompilasi!
echo  - GUI: %~dp0publish\gui\NRTX.Optimizer.Gui.exe
echo  - CLI: %~dp0publish\cli\NRTX.Optimizer.Cli.exe
echo ===================================================================
echo.
pause
exit /b 0

