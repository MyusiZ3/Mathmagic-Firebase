# Intellectual Property (HKI) Copyright & Source Code Report
### Directorate General of Intellectual Property (DJKI) - Ministry of Law and Human Rights RI

---

## Official Copyright Registration Identity (Law No. 28 of 2014)

* **Title of Work**: Mathmagic (Interactive Math Learning Media Based on Educational Game)
* **Type of Work**: Video Game (Interactive Mobile Game & Web Admin Ecosystem)
* **Registration Number**: 001400842
* **Application Number & Date**: EC002026136398 (August 7, 2026)
* **Copyright Holder**: Universitas Telkom
* **Live Application Link**: [https://shorturl.at/cb55f](https://shorturl.at/cb55f)

---

## Development & Design Team Credits

| Full Name | Project Role | Contribution Description |
| :--- | :--- | :--- |
| **Muhamad Sidik** | **Project Creator, Full Developer & Main UI Designer** | Original Project Creator, Lead Unity 6 game application developer, C# logic programmer, Main UI/UX Designer & implementer, and Cloud Firebase systems integrator |
| **Rizky Yonanda** | **Project Manager** | Project Manager, scheduling, and team coordination |
| **Zahra Imani** | **UI/UX Design** | UI/UX Designer and game visual asset designer |
| **Dean Erick Adhitia Nugraha** | **Sound Designer** | Audio engineering, sound effects (SFX), and background music (BGM) |
| **Sheilan Mayra** | **QA Testing** | Quality Assurance testing and system stability verification |
| **Rikman Aherliwan Rudawan** | **Academic Supervisor** | Academic Project Advisor & Supervisor (Telkom University) |

* **First Publication Date & Place**: July 23, 2026, in Bandung, Indonesia
* **Protection Term**: 50 years from first publication
* **Programming Languages**: C# (Unity Engine Framework) & JavaScript (Vite Web Dashboard)
* **Official User Manual**: [`Manual Book Mathmagic.pdf`](./Manual%20Book%20Mathmagic.pdf)
* **Industry Recognition Letter**: [`SURAT PENGAKUAN INDUSTRI.pdf`](./SURAT%20PENGAKUAN%20INDUSTRI.pdf)
* **Source Code HKI Archive**: [`Laporan_HKI_SourceCode.docx`](./Archive/Laporan_HKI_SourceCode.docx)

> [!NOTE]
> Physical copies of the official Copyright Certificate (Surat Pencatatan Ciptaan) are retained by the Copyright Holder (Telkom University) and the Authors for institutional data privacy. Complete registration metadata is presented in full above.

---

## 1. Authentication & Account Isolation Module: `FirebaseAuthController.cs`
* **File Location**: `Assets/1 Script/Firebase/FirebaseAuthController.cs`
* **Functional Description**: Primary authentication and user data isolation module for the Unity Game Client. Manages account registration, real-time username uniqueness validation on Cloud Firestore, email RegEx validation, Firebase Auth login encryption, local token-based auto-login, and `PlayerPrefs` cache clearing upon logout to prevent cross-account data leakage.

```csharp
// Module 1: FirebaseAuthController.cs (Unity C#)
// (Complete program code archived in Laporan_HKI_SourceCode.docx)
```

---

## 2. Score Management & Firestore Synchronization Module: `ScoreManager.cs`
* **File Location**: `Assets/1 Script/Score/ScoreManager.cs`
* **Functional Description**: Core module for recording player scores, game score accumulation, and real-time cloud synchronization to Firestore. Equipped with an offline-fallback caching system (`PlayerPrefs`) that automatically syncs to the cloud once internet connectivity is restored, as well as local memory cleanup (`ClearLocalUserData`) during logout sessions.

```csharp
// Module 2: ScoreManager.cs (Unity C#)
// (Complete program code archived in Laporan_HKI_SourceCode.docx)
```
