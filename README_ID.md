<div align="center">

# ⚡ TROY TWEAKS WINDOWS OPTIMIZER (Community Edition) 🌸

### *Suite Optimasi Native Windows, System Debloater & Gaming Tuner Berkecepatan Tinggi*

<br/>

[![English Version](https://img.shields.io/badge/Language-English-blue?style=for-the-badge)](README.md)
[![Bahasa Indonesia](https://img.shields.io/badge/Terjemahan-Bahasa_Indonesia-red?style=for-the-badge)](#)

<br/>

[![Framework: .NET 10 & 8](https://img.shields.io/badge/Framework-.NET_10.0_(LTS)_WPF_+_CLI-512bd4?style=for-the-badge&logo=dotnet&logoColor=white)](#)
[![Interface: Dual WPF & CLI](https://img.shields.io/badge/Interface-WPF_Fluent_+_Spectre.Console-0284c7?style=for-the-badge&logo=windows11&logoColor=white)](#)
[![Unit Tests: 40/40 Passing](https://img.shields.io/badge/Unit_Tests-40%2F40_Passing_(100%25)-22c55e?style=for-the-badge&logo=xunit&logoColor=white)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-22c55e?style=for-the-badge)](#)
[![Edition: Community Free](https://img.shields.io/badge/Edition-Community_Free-f59e0b?style=for-the-badge)](#)
[![VIP Tier: NRTX Labs](https://img.shields.io/badge/VIP_Access-NRTX_Labs_Org-ec4899?style=for-the-badge)](#)

<br/>

> **TROY Tweaks Windows Optimizer (Community Edition)** adalah suite optimasi Windows native enterprise-grade berbasis **C# (.NET 10 LTS)** murni untuk membersihkan bloatware, menonaktifkan telemetri privasi, mengelola aplikasi startup, membersihkan cache sistem, dan memaksimalkan performa PC harian secara aman tanpa perantara wrapper pihak ketiga.

</div>

---

## ✨ Fitur Versi Community (Free)

- 🛡️ **Privacy & Telemetry Guard**: Matikan telemetri Windows, Connected User Experiences (DiagTrack), Advertising ID, dan Activity History.
- ⚡ **Ultimate Performance Power Plan**: Aktifkan skema daya workstation tersembunyi dan atur prioritas thread CPU (`Win32PrioritySeparation`).
- 📦 **UWP AppX Debloater**: Bersihkan aplikasi bawaan bersponsor Windows tanpa merusak Microsoft Store, Kalkulator, dan Windows Terminal.
- 🧹 **Deep Storage & Cache Cleaner**: Bersihkan `%TEMP%`, `C:\Windows\Temp`, Shader Cache (DirectX/NVIDIA/AMD), delivery optimization cache, dan sampah browser.
- 🚀 **Instant Quick Clean**: Trim memori RAM, bersihkan cache temporary, dan flush DNS resolver hanya dengan 1-klik.
- 📱 **Startup Applications Manager**: Pantau dan nonaktifkan aplikasi startup secara real-time di HKCU, HKLM, WOW6432Node, folder Startup, dan Task Scheduler.
- 🎯 **1-Click Optimization Profiles**: Preset siap pakai untuk *Gaming*, *eSports Latency*, *Developer & Creator*, *Privacy Maximum*, *Balanced Workstation*, dan *Safe Defaults*.
- 💾 **P0 Safety Governance**: Otomatis buat Windows System Restore Point, backup state registri, dan fitur rollback 1-klik.
- 🌐 **Dukungan Dua Bahasa**: Penggantian bahasa langsung secara real-time antara Bahasa Indonesia (id-ID) dan English (en-US).

---

## 🚀 Panduan Memulai

### Opsi 1: Menggunakan Script Launcher Otomatis (Rekomendasi)
Cukup dobel-klik salah satu file `.bat` di bawah. Script akan otomatis mendeteksi .NET SDK, mengompilasi aplikasi saat pertama kali di-clone, dan menjalankannya:

- **Jalankan Desktop GUI**: Dobel-klik `Launch_Troy_Optimizer_GUI.bat`
- **Jalankan Terminal CLI**: Dobel-klik `Launch_Troy_Optimizer_CLI.bat`
- **Compile & Publish Suite**: Dobel-klik `Build_All.bat`

### Opsi 2: Menggunakan .NET CLI Manual
```powershell
# Clone repository
git clone https://github.com/MuchoRio/TROY-Tweaks-Windows-Optimizer.git
cd TROY-Tweaks-Windows-Optimizer

# Jalankan Desktop GUI (WPF)
dotnet run --project src/NRTX.Optimizer.Gui

# Atau jalankan Terminal CLI (Spectre.Console TUI)
dotnet run --project src/NRTX.Optimizer.Cli
```

### Opsi 3: Otomasi Headless CLI
CLI mendukung perintah headless non-interaktif untuk kebutuhan skrip dan CI/CD:
```powershell
# Jalankan scan diagnostik cepat
dotnet run --project src/NRTX.Optimizer.Cli -- scan

# Inspeksi telemetri memori NT kernel
dotnet run --project src/NRTX.Optimizer.Cli -- mem-stats

# Ekspor laporan kesehatan ke JSON atau Markdown
dotnet run --project src/NRTX.Optimizer.Cli -- export --format json --output report.json
dotnet run --project src/NRTX.Optimizer.Cli -- export --format md --output report.md

# Trim memori RAM instan
dotnet run --project src/NRTX.Optimizer.Cli -- trim-ram

# Terapkan profil optimasi di latar belakang (gaming, esports, dev, privacy, safe)
dotnet run --project src/NRTX.Optimizer.Cli -- apply --profile gaming

# Mode simulasi (Dry-run)
dotnet run --project src/NRTX.Optimizer.Cli -- apply --profile esports --dry-run
```

---

## 🧪 Testing & Kualitas Kode

Jalankan test suite menggunakan `dotnet test`:
```powershell
dotnet test
```
Seluruh **40 unit test** lulus (100% green) mencakup validasi engine utama, keamanan registri, mekanisme rollback, diagnostik, dan layanan telemetri perangkat keras.

---

## 💎 Ingin Akses Fitur Penuh & eSports Kernel? Gabung NRTX Labs VIP Organization!

Dapatkan akses eksklusif seumur hidup (**Lifetime Full Access**) ke seluruh source code, proprietary algorithms, private tools, dan **TROY Enterprise / eSports Edition** di GitHub Organization **[nrtxlabs](https://github.com/nrtxlabs/)**!

### 👑 Perbandingan Versi Community vs NRTX Labs VIP Edition

| Fitur & Kapabilitas | Community Edition (Free) | 👑 NRTX Labs VIP (Enterprise) |
|:---|:---:|:---:|
| **Modul Optimasi Native** | 28 Modul Esensial | **58+ Modul Lengkap** |
| **P0 Safety Gate (Restore Point & Snapshot)** | ✅ Ada | ✅ Ada |
| **UWP Bloatware Debloater & Privacy Guard** | ✅ Ada | ✅ Ada |
| **Storage & Multi-Browser Cache Cleaner** | ✅ Standar | ✅ Deep Win32 Dynamic Scan |
| **Startup Manager & Live Autoruns Toggle** | ✅ Standar | ✅ Task Scheduler Logon Scan |
| **⚡ Global 0.5000ms Kernel Timer Resolution Lock** | ❌ | ✅ **Eksklusif VIP** (0.5000ms Lock) |
| **🛰️ TROY FastRoute: Game QoS & DSCP 46 (ExitLag Tech)** | ❌ | ✅ **Eksklusif VIP** (Expedited Forwarding) |
| **🎮 Bare-Metal Gaming (Hyper-V & VBS/HVCI Bypass)** | ❌ | ✅ **Eksklusif VIP** (Max 1% Low FPS) |
| **🎧 MMCSS Pro Audio Tuning (Footstep Emas Valorant/CS2)**| ❌ | ✅ **Eksklusif VIP** (Spatial Audio Boost) |
| **🖱️ 8000Hz Mouse Radar, MarkC Fix & HID Buffer 256** | ❌ | ✅ **Eksklusif VIP** (Anti-Packet Drop) |
| **🧠 Full NT Kernel MemReduct Pro (Standby List Syscalls)**| ❌ | ✅ **Eksklusif VIP** (Direct NT Syscalls) |
| **🚀 Smart Game Booster Auto-Detection Daemon** | ❌ | ✅ **Eksklusif VIP** (Auto Game Boost) |
| **🔴 AMD 3D V-Cache (X3D) & 🔵 Intel Thread Director (EPP 0)**| ❌ | ✅ **Eksklusif VIP** (P-Core Affinity) |

---

### 💳 Biaya & Metode Pembayaran Membership

> ### **💰 Biaya Membership: Rp 5.000.000,- (5 JT) / $350 USD (Lifetime Full Access)**

Pilih salah satu metode pembayaran resmi di bawah ini:

1. **🅿️ PayPal**: [**`paypal.me/riop4u`**](https://paypal.me/riop4u)
2. **📧 Direct Email Confirmation**: [**`det.rio1337@gmail.com`**](mailto:det.rio1337@gmail.com)
3. **🪙 Crypto (USDT / BNB - BEP20 Network)**:
   ```text
   0x9cca7b1f6524f2876a2c208c842cd3252bce60a8
   ```

*Setelah melakukan pembayaran, kirimkan bukti transfer beserta **username GitHub** kamu ke [**`det.rio1337@gmail.com`**](mailto:det.rio1337@gmail.com) untuk langsung di-invite ke GitHub Organization **NRTX Labs**!* 🌸

---

<div align="center">

*Maintained with 💖 by **Rio** ([@MuchoRio](https://github.com/MuchoRio)) & **NRTX Labs**.*  
*© 2026 NRTX Labs. All rights reserved.*

</div>
