# Mathmagic: Interactive Educational Math Game & Web Admin Dashboard

[![Unity](https://img.shields.io/badge/Unity-6000.0.35f1_LTS-blue.svg?logo=unity)](https://unity.com/)
[![Firebase](https://img.shields.io/badge/Firebase-Firestore_%26_Auth-FFCA28.svg?logo=firebase)](https://firebase.google.com/)
[![Vite](https://img.shields.io/badge/Vite-Web_Dashboard-646CFF.svg?logo=vite)](https://vitejs.dev/)
[![HKI Registered](https://img.shields.io/badge/HKI_DJKI-Registered-blueviolet.svg)](Doc/LAPORAN_HKI.md)
[![Live App](https://img.shields.io/badge/Live_App-Play_Now-brightgreen.svg?logo=googleplay)](https://shorturl.at/cb55f)

**Mathmagic** is an interactive, math-based educational game ecosystem designed for elementary and secondary school students. It features real-time cloud backend synchronization and a web-based administration panel for teachers and administrators to monitor student progress and manage game parameters dynamically.

📱 **Live Application Link**: [https://shorturl.at/cb55f](https://shorturl.at/cb55f)

---

## 🌟 Ecosystem Overview

The **Mathmagic** ecosystem consists of two primary components:

1. **Unity Game Client (Mobile Application)**: Built with **Unity 6 (v6000.0.35f1 LTS)** to deliver a high-performance, visually engaging mobile experience with interactive mathematics gameplay, offline fallback caching, and live cloud synchronization.
2. **Web Admin Dashboard**: Powered by **Vite + Vanilla JS (ES6)** and the **Firebase SDK**, featuring a modern *Bento Grid* interface to track student metrics, view real-time leaderboards, and adjust game balance remotely without requiring app rebuilds.

```
                    +-------------------------------------+
                    |       Cloud Firebase Service        |
                    |  (Firestore DB & Authentication)    |
                    +------------------+------------------+
                                       |
                +----------------------+----------------------+
                |                                             |
                v                                             v
  +---------------------------+                 +---------------------------+
  |     Unity Game Client     |                 |    Web Admin Dashboard    |
  |  (Mobile App - Unity 6)   |                 | (Vite + Vanilla JS + CSS) |
  +---------------------------+                 +---------------------------+
```

---

## 📱 1. Unity Game Client (Mobile Application)

### Technical Specifications
* **Engine Version**: Unity 6 (`6000.0.35f1 LTS`)
* **Programming Language**: C# (.NET Standard 2.1)
* **Local Storage**: `PlayerPrefs` (offline caching & account session isolation)
* **Cloud Infrastructure**: Firebase Authentication & Firestore NoSQL Database
* **UI & Performance**: Sinusoidal math wave processing (`Mathf.Sin`) in `HealthUIUpdater.cs` for smooth UI heartbeat animations with minimal CPU/GPU overhead on mobile devices.

### Key Client Directory Structure (`/Assets`)
```
Assets/
├── 1 Script/
│   ├── Level Script/          # Level progression, timers, & UI overlays
│   │   ├── LevelManager.cs
│   │   ├── Timer.cs
│   │   └── OverlayManager.cs
│   ├── Login Register Script/ # Authentication & user session management
│   │   ├── FirebaseAuthController.cs
│   │   └── DoLogout.cs
│   ├── Score/                 # Score tracking, HP mechanics, & Firestore sync
│   │   ├── ScoreManager.cs
│   │   ├── ProfileManager.cs
│   │   └── New/
│   │       ├── HealthManager.cs
│   │       └── HealthUIUpdater.cs
│   └── Web Admin Interop/    # Remote configuration & live balance fetcher
│       └── RemoteSettingsManager.cs
```

---

## 💻 2. Web Admin Dashboard

### Technical Specifications
* **Bundler / Framework**: Vite (Vanilla ES6 JavaScript)
* **Styling**: Modern CSS Glassmorphism with **Bento Grid** layout
* **Database & Auth**: Firebase Web SDK v10+ (Firestore & Authentication)
* **Key Features**: Student Performance Analytics, Real-time Leaderboards, Remote Game Balance & Settings Manager.

### Key Web Dashboard Directory Structure (`/Web Dashboard`)
```
Web Dashboard/
├── index.html              # Main HTML structure & Bento Grid layout
├── main.js                 # App controller & Firestore communication logic
├── styles.css              # Custom Glassmorphism styling & layout rules
├── package.json            # npm package manifest & dependencies
└── vite.config.js          # Vite bundler configuration
```

---

## 🛠️ 3. Installation & Getting Started

### Prerequisites
* **Unity Hub** with **Unity 6 (6000.0.35f1 LTS)** installed.
* **Node.js** (v18+ recommended) and **npm** installed.

### A. Running Unity Game Client
1. Open **Unity Hub** and select **Add project from disk**.
2. Navigate to the project root directory (`Mathmagic-New`).
3. Ensure the project is opened using Unity Version **`6000.0.35f1 LTS`**.
4. Open the main scene located at `Assets/Scenes/MainLoginScene.unity`.
5. Press the **Play** button in the Unity Editor to start the simulation.

### B. Running Web Admin Dashboard (Local Development)
1. Open your terminal and navigate to the `Web Dashboard` directory:
   ```bash
   cd "Web Dashboard"
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Launch the development server:
   ```bash
   npm run dev
   ```
4. Open the local address printed in the terminal (typically `http://localhost:5173`).

---

## 📚 4. Project Documentation

Detailed technical documentation, ISO 5807 flowchart specifications, and narrative user guides are located in the [`/Doc`](./Doc) directory:

| Document File | Description |
| :--- | :--- |
| [`Doc/Manual Book Mathmagic.pdf`](./Doc/Manual%20Book%20Mathmagic.pdf) | Official application user manual & operational book (PDF). |
| [`Doc/SURAT PENGAKUAN INDUSTRI.pdf`](./Doc/SURAT%20PENGAKUAN%20INDUSTRI.pdf) | Official industry recognition & validation letter (PDF). |
| [`Doc/FLOWCHART_DOCUMENTATION.md`](./Doc/FLOWCHART_DOCUMENTATION.md) | Master index for system flowcharts & ISO 5807 standards. |
| [`Doc/Flowchart_Game.md`](./Doc/Flowchart_Game.md) | Detailed technical flowcharts for the Unity Game Client. |
| [`Doc/Flowchart_WebAdmin.md`](./Doc/Flowchart_WebAdmin.md) | Detailed technical flowcharts for the Web Admin Dashboard. |
| [`Doc/LAPORAN_HKI.md`](./Doc/LAPORAN_HKI.md) | Summary report of source code modules submitted for HKI copyright registration. |
| [`Doc/USER_FLOW.md`](./Doc/USER_FLOW.md) | Comprehensive narrative guide for the user experience (UX Journey). |

---

## ⚖️ 5. License & Intellectual Property (HKI)

This software application and its source code are protected by Copyright Law of the Republic of Indonesia (UU No. 28 Tahun 2014) and are officially registered under **Intellectual Property Rights (HKI - Hak Kekayaan Intelektual)** with the **Directorate General of Intellectual Property (DJKI)**, Ministry of Law and Human Rights RI.

* **Title of Work**: Mathmagic
* **Type of Work**: Video Game (Interactive Mobile Game & Web Admin Ecosystem)
* **Registration Number**: `001400842` (Application No. `EC002026136398`)
* **Copyright Holder**: **UNIVERSITAS TELKOM**
* **Creators (DJKI Team)**: Rikman Aherliwan Rudawan, Rizky Yonanda, et al. (`dkk.`)
* **First Publication**: July 23, 2026 (Bandung, Indonesia)
* **Protection Term**: 50 years from first publication

### 👥 Development & Design Team Credits

| Name | Project Role | Responsibilities |
| :--- | :--- | :--- |
| **Muhamad Sidik** | **Full Developer & Main UI** | Full Unity 6 game development, C# architecture, UI implementation & systems integration |
| **Rizky Yonanda** | **Project Manager** | Project management, scheduling, & team coordination |
| **Zahra Imani** | **UI/UX Design** | UI/UX design, visual assets, & user interface layout design |
| **Dean Erick Adhitia Nugraha** | **Sound Designer** | Audio engineering, sound effects (SFX), & background music (BGM) |
| **Sheilan Mayra** | **QA Testing** | Quality assurance, game testing, & bug reporting |
| **Rikman Aherliwan Rudawan** | **Academic Supervisor** | Project advisor & academic supervision (Telkom University) |

🔗 **Live Application Link**: [https://shorturl.at/cb55f](https://shorturl.at/cb55f)

For details regarding registered core source code modules (`FirebaseAuthController.cs`, `ScoreManager.cs`, etc.), please consult [`Doc/LAPORAN_HKI.md`](./Doc/LAPORAN_HKI.md) or the official [`LICENSE`](./LICENSE) file.

---

<p align="center">
  <i>Copyright &copy; 2026 Universitas Telkom & Authors. Registered Intellectual Property (HKI - DJKI No. 001400842). All Rights Reserved.</i>
</p>



