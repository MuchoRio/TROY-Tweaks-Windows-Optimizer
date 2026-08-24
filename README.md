<div align="center">

# ⚡ TROY TWEAKS WINDOWS OPTIMIZER (Community Edition) 🌸

### *High-Performance Native Windows Optimization Suite, System Debloater & Gaming Tuner*

<br/>

[![Language: English](https://img.shields.io/badge/Language-English-blue?style=for-the-badge)](#)
[![Terjemahan: Bahasa Indonesia](https://img.shields.io/badge/Terjemahan-Bahasa_Indonesia-red?style=for-the-badge)](README_ID.md)

<br/>

[![Framework: .NET 10 & 8](https://img.shields.io/badge/Framework-.NET_10.0_(LTS)_WPF_+_CLI-512bd4?style=for-the-badge&logo=dotnet&logoColor=white)](#)
[![Interface: Dual WPF & CLI](https://img.shields.io/badge/Interface-WPF_Fluent_+_Spectre.Console-0284c7?style=for-the-badge&logo=windows11&logoColor=white)](#)
[![Unit Tests: 40/40 Passing](https://img.shields.io/badge/Unit_Tests-40%2F40_Passing_(100%25)-22c55e?style=for-the-badge&logo=xunit&logoColor=white)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-22c55e?style=for-the-badge)](#)
[![Edition: Community Free](https://img.shields.io/badge/Edition-Community_Free-f59e0b?style=for-the-badge)](#)
[![VIP Tier: NRTX Labs](https://img.shields.io/badge/VIP_Access-NRTX_Labs_Org-ec4899?style=for-the-badge)](#)

<br/>

> **TROY Tweaks Windows Optimizer (Community Edition)** is an enterprise-grade native Windows optimization suite built in pure **C# (.NET 10 LTS)** to strip telemetry, remove bloatware, manage startup items, clean system caches, and tune PC performance safely with zero third-party wrappers.

</div>

---

## ✨ Community Edition (Free) Features

- 🛡️ **Privacy & Telemetry Guard**: Disable Windows tracking, Connected User Experiences (DiagTrack), Advertising ID, and Activity History.
- ⚡ **Ultimate Performance Power Plan**: Unlock the hidden workstation power scheme and tune CPU thread prioritization (`Win32PrioritySeparation`).
- 📦 **UWP AppX Debloater**: Safely remove preinstalled sponsored Windows bloatware while preserving Microsoft Store, Calculator, and Windows Terminal.
- 🧹 **Deep Storage & Cache Cleaner**: Purge `%TEMP%`, `C:\Windows\Temp`, Shader Caches (DirectX/NVIDIA/AMD), delivery optimization cache, and browser junk.
- 🚀 **Instant Quick Clean**: One-click RAM working set trimming, temporary cache purge, and DNS resolver flushing.
- 📱 **Startup Applications Manager**: Real-time inspection and safe toggling of startup applications across HKCU, HKLM, WOW6432Node, Startup folders, and Task Scheduler.
- 🎯 **1-Click Optimization Profiles**: Preconfigured presets for *Gaming*, *eSports Latency*, *Developer & Creator*, *Privacy Maximum*, *Balanced Workstation*, and *Safe Defaults*.
- 💾 **P0 Safety Governance**: Automated Windows System Restore Point creation, registry state backups, and one-click rollback capabilities.
- 🌐 **Dual Language Support**: Full real-time switching between English (en-US) and Bahasa Indonesia (id-ID).

---

## 🚀 Getting Started

### Option 1: Automated Launcher Scripts (Recommended)
Double-click any of the batch files below. The launcher automatically detects your .NET SDK, compiles the project upon first clone, and launches the application:

- **Launch Desktop GUI**: Double-click `Launch_Troy_Optimizer_GUI.bat`
- **Launch Terminal CLI**: Double-click `Launch_Troy_Optimizer_CLI.bat`
- **Build & Publish Suite**: Double-click `Build_All.bat`

### Option 2: Manual .NET CLI
```powershell
# Clone repository
git clone https://github.com/MuchoRio/TROY-Tweaks-Windows-Optimizer.git
cd TROY-Tweaks-Windows-Optimizer

# Run Desktop GUI (WPF)
dotnet run --project src/NRTX.Optimizer.Gui

# Or run Terminal CLI (Spectre.Console TUI)
dotnet run --project src/NRTX.Optimizer.Cli
```

### Option 3: Headless CLI Automation
The CLI supports non-interactive headless commands for scripts and CI/CD:
```powershell
# Run a quick diagnostic scan
dotnet run --project src/NRTX.Optimizer.Cli -- scan

# Inspect NT kernel memory telemetry
dotnet run --project src/NRTX.Optimizer.Cli -- mem-stats

# Export health report to JSON or Markdown
dotnet run --project src/NRTX.Optimizer.Cli -- export --format json --output report.json
dotnet run --project src/NRTX.Optimizer.Cli -- export --format md --output report.md

# Instant memory trim
dotnet run --project src/NRTX.Optimizer.Cli -- trim-ram

# Apply an optimization profile silently (e.g. gaming, esports, dev, privacy, safe)
dotnet run --project src/NRTX.Optimizer.Cli -- apply --profile gaming

# Dry-run simulation mode
dotnet run --project src/NRTX.Optimizer.Cli -- apply --profile esports --dry-run
```

---

## 🧪 Testing & Code Quality

Run the test suite via `dotnet test`:
```powershell
dotnet test
```
All **40 unit tests** pass with 100% coverage on core engines, safe registry access, rollback mechanisms, diagnostics, and hardware telemetry services.

---

## 💎 Want Full Access & eSports Kernel? Join NRTX Labs VIP Organization!

Gain lifetime exclusive access (**Lifetime Full Access**) to the complete source code, proprietary algorithms, private tools, and the **TROY Enterprise / eSports Edition** on the official **[nrtxlabs](https://github.com/nrtxlabs/)** GitHub Organization!

### 👑 Feature Matrix: Community Edition vs NRTX Labs VIP Edition

| Features & Capabilities | Community Edition (Free) | 👑 NRTX Labs VIP (Enterprise) |
|:---|:---:|:---:|
| **Native Optimization Modules** | 28 Essential Modules | **58+ Full Modules** |
| **P0 Safety Gate (Restore Point & Snapshot)** | ✅ Included | ✅ Included |
| **UWP Bloatware Debloater & Privacy Guard** | ✅ Included | ✅ Included |
| **Storage & Multi-Browser Cache Cleaner** | ✅ Standard | ✅ Deep Win32 Dynamic Scan |
| **Startup Manager & Live Autoruns Toggle** | ✅ Standard | ✅ Task Scheduler Logon Scan |
| **⚡ Global 0.5000ms Kernel Timer Resolution Lock** | ❌ | ✅ **VIP Exclusive** (0.5000ms Lock) |
| **🛰️ TROY FastRoute: Game QoS & DSCP 46 (ExitLag Tech)** | ❌ | ✅ **VIP Exclusive** (Expedited Forwarding) |
| **🎮 Bare-Metal Gaming (Hyper-V & VBS/HVCI Bypass)** | ❌ | ✅ **VIP Exclusive** (Max 1% Low FPS) |
| **🎧 MMCSS Pro Audio Tuning (Valorant/CS2 Footsteps)** | ❌ | ✅ **VIP Exclusive** (Spatial Audio Boost) |
| **🖱️ 8000Hz Mouse Radar, MarkC Fix & HID Buffer 256** | ❌ | ✅ **VIP Exclusive** (Anti-Packet Drop) |
| **🧠 Full NT Kernel MemReduct Pro (Standby List Syscalls)**| ❌ | ✅ **VIP Exclusive** (Direct NT Syscalls) |
| **🚀 Smart Game Booster Auto-Detection Daemon** | ❌ | ✅ **VIP Exclusive** (Auto Game Boost) |
| **🔴 AMD 3D V-Cache (X3D) & 🔵 Intel Thread Director (EPP 0)**| ❌ | ✅ **VIP Exclusive** (P-Core Affinity) |

---

### 💳 Membership Pricing & Payment Channels

> ### **💰 Lifetime Membership: $350 USD / Rp 5.000.000 IDR (One-time Payment)**

Select one of the official payment methods below:

1. **🅿️ PayPal**: [**`paypal.me/riop4u`**](https://paypal.me/riop4u)
2. **📧 Direct Email Confirmation**: [**`det.rio1337@gmail.com`**](mailto:det.rio1337@gmail.com)
3. **🪙 Crypto (USDT / BNB - BEP20 Network)**:
   ```text
   0x9cca7b1f6524f2876a2c208c842cd3252bce60a8
   ```

*After completing your payment, send your proof of transfer and **GitHub username** to [**`det.rio1337@gmail.com`**](mailto:det.rio1337@gmail.com) to receive an immediate invite to the **NRTX Labs** GitHub Organization!* 🌸

---

<div align="center">

*Maintained with 💖 by **Rio** ([@MuchoRio](https://github.com/MuchoRio)) & **NRTX Labs**.*  
*© 2026 NRTX Labs. All rights reserved.*

</div>
