# Mathmagic: Interactive Educational Math Game & Web Admin Dashboard

[![Unity](https://img.shields.io/badge/Unity-6000.0.35f1_LTS-blue.svg?logo=unity)](https://unity.com/)
[![Firebase](https://img.shields.io/badge/Firebase-Firestore_%26_Auth-FFCA28.svg?logo=firebase)](https://firebase.google.com/)
[![Vite](https://img.shields.io/badge/Vite-Web_Dashboard-646CFF.svg?logo=vite)](https://vitejs.dev/)
[![HKI Registered](https://img.shields.io/badge/HKI_DJKI-Registered-blueviolet.svg)](Doc/HKI_REPORT.md)
[![Live App](https://img.shields.io/badge/Live_App-Play_Now-brightgreen.svg?logo=googleplay)](https://shorturl.at/cb55f)

Mathmagic is an interactive educational math game ecosystem designed for elementary and secondary school students. The system integrates a mobile Unity game client with real-time cloud backend synchronization and a web-based admin dashboard for monitoring student performance and adjusting game parameters dynamically.

Live Application Link: [https://shorturl.at/cb55f](https://shorturl.at/cb55f)

---

## System Ecosystem Overview

The Mathmagic ecosystem consists of two core components:

1. **Unity Game Client (Mobile Application)**: Developed using Unity 6 (v6000.0.35f1 LTS) to provide high-performance, interactive mathematics gameplay with offline fallback caching and real-time cloud synchronization.
2. **Web Admin Dashboard**: Built with Vite, vanilla JavaScript (ES6), and the Firebase Web SDK. Features a Bento Grid dashboard for telemetry analytics, real-time leaderboards, and remote game balancing without needing client app updates.

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

## 1. Unity Game Client (Mobile Application)

### Technical Specifications
* **Engine Version**: Unity 6 (`6000.0.35f1 LTS`)
* **Programming Language**: C# (.NET Standard 2.1)
* **Local Storage**: `PlayerPrefs` (offline caching and account session isolation)
* **Cloud Infrastructure**: Firebase Authentication & Firestore NoSQL Database
* **UI Performance**: Sinusoidal math wave calculations (`Mathf.Sin`) in `HealthUIUpdater.cs` for UI heartbeat animations with minimal GPU/CPU overhead.

### Key Client Directory Structure (`/Assets`)
```
Assets/
├── 1 Script/
│   ├── Level Script/          # Level progression, timers, and UI overlays
│   │   ├── LevelManager.cs
│   │   ├── Timer.cs
│   │   └── OverlayManager.cs
│   ├── Login Register Script/ # Authentication and user session management
│   │   ├── FirebaseAuthController.cs
│   │   └── DoLogout.cs
│   ├── Score/                 # Score tracking, HP mechanics, and Firestore sync
│   │   ├── ScoreManager.cs
│   │   ├── ProfileManager.cs
│   │   └── New/
│   │       ├── HealthManager.cs
│   │       └── HealthUIUpdater.cs
│   └── Web Admin Interop/    # Remote configuration and live game balance fetcher
│       └── RemoteSettingsManager.cs
```

---

## 2. Web Admin Dashboard

### Technical Specifications
* **Bundler / Framework**: Vite (Vanilla ES6 JavaScript)
* **Styling**: Vanilla CSS with Bento Grid layout
* **Database & Auth**: Firebase Web SDK v10+ (Firestore & Authentication)
* **Core Capabilities**: Student analytics telemetry, real-time leaderboards, remote game balance editor.

### Key Web Dashboard Directory Structure (`/Web Dashboard`)
```
Web Dashboard/
├── index.html              # Main HTML structure and Bento Grid layout
├── main.js                 # App controller and Firestore communication logic
├── styles.css              # Glassmorphism styling and responsive grid rules
├── package.json            # npm package manifest and dependencies
└── vite.config.js          # Vite bundler setup
```

---

## 3. Installation & Setup

### Prerequisites
* Unity Hub with Unity 6 (`6000.0.35f1 LTS`) installed.
* Node.js (v18+) and npm installed.

### A. Running the Unity Game Client
1. Open Unity Hub and click **Add project from disk**.
2. Select the repository root directory (`Mathmagic-New`).
3. Ensure the project version is set to Unity **`6000.0.35f1 LTS`**.
4. Open `Assets/Scenes/MainLoginScene.unity`.
5. Press **Play** in Unity Editor to run the game simulation.

### B. Running the Web Admin Dashboard
1. Open terminal and navigate to the `Web Dashboard` folder:
   ```bash
   cd "Web Dashboard"
   ```
2. Install npm dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm run dev
   ```
4. Open the local address displayed in terminal (typically `http://localhost:5173`).

---

## 4. Project Documentation

Technical documentation, ISO 5807 flowchart specifications, and narrative user guides are located in the `Doc/` directory:

| Document File | Description |
| :--- | :--- |
| [`Doc/Manual Book Mathmagic.pdf`](./Doc/Manual%20Book%20Mathmagic.pdf) | Application user manual and operational guide (PDF). |
| [`Doc/SURAT PENGAKUAN INDUSTRI.pdf`](./Doc/SURAT%20PENGAKUAN%20INDUSTRI.pdf) | Official industry recognition and validation letter (PDF). |
| [`Doc/HKI_REPORT.md`](./Doc/HKI_REPORT.md) | Summary report of source code modules submitted for HKI copyright registration. |
| [`Doc/FLOWCHART_INDEX.md`](./Doc/FLOWCHART_INDEX.md) | Master index for system flowcharts and ISO 5807 standards. |
| [`Doc/GAME_FLOWCHART.md`](./Doc/GAME_FLOWCHART.md) | Detailed technical flowcharts for the Unity Game Client. |
| [`Doc/WEB_ADMIN_FLOWCHART.md`](./Doc/WEB_ADMIN_FLOWCHART.md) | Detailed technical flowcharts for the Web Admin Dashboard. |
| [`Doc/USER_FLOW.md`](./Doc/USER_FLOW.md) | Narrative user experience journey guide (UX Journey). |

---

## 5. License & Intellectual Property (HKI)

This application and its source code are protected under Copyright Law of the Republic of Indonesia (UU No. 28 Tahun 2014) and registered under Intellectual Property Rights (HKI) with the Directorate General of Intellectual Property (DJKI), Ministry of Law and Human Rights RI.

* **Title of Work**: Mathmagic
* **Type of Work**: Video Game (Interactive Mobile Game & Web Admin Ecosystem)
* **Registration Number**: `001400842` (Application No. `EC002026136398`)
* **Copyright Holder**: Universitas Telkom
* **Creators (DJKI Registration)**: Rikman Aherliwan Rudawan, Rizky Yonanda, et al.
* **First Publication**: July 23, 2026 (Bandung, Indonesia)
* **Protection Term**: 50 years from first publication

### Development & Design Team Credits

| Name | Project Role | Responsibilities |
| :--- | :--- | :--- |
| **Muhamad Sidik** | **Project Creator, Full Developer & Main UI Designer** | Original Project Creator, full Unity 6 game development, C# core architecture, Main UI/UX design and implementation, and Firebase systems integration |
| **Rizky Yonanda** | **Project Manager** | Project management, scheduling, and team coordination |
| **Zahra Imani** | **UI/UX Design** | UI/UX design, visual assets, and user interface layout design |
| **Dean Erick Adhitia Nugraha** | **Sound Designer** | Audio engineering, sound effects (SFX), and background music (BGM) |
| **Sheilan Mayra** | **QA Testing** | Quality assurance, game testing, and bug reporting |
| **Rikman Aherliwan Rudawan** | **Academic Supervisor** | Project advisor and academic supervision (Telkom University) |

Live Application Link: [https://shorturl.at/cb55f](https://shorturl.at/cb55f)

For details regarding registered core source code modules (`FirebaseAuthController.cs`, `ScoreManager.cs`), see [`Doc/HKI_REPORT.md`](./Doc/HKI_REPORT.md) or the [`LICENSE`](./LICENSE) file.

---

<p align="center">
  <i>Copyright &copy; 2026 Universitas Telkom & Authors. Registered Intellectual Property (HKI - DJKI No. 001400842). All Rights Reserved.</i>
</p>
