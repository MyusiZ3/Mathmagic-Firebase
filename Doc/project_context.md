# Mathmagic - Project Context & Requirements

Dokumen ini berfungsi sebagai peta konteks dan spesifikasi kebutuhan proyek **Mathmagic** untuk memandu AI/Developer dalam sesi chat baru tanpa harus membaca ulang seluruh basis kode dari awal.

---

## 1. Ringkasan Proyek

**Mathmagic** adalah aplikasi game edukasi matematika interaktif. Proyek ini terdiri dari dua komponen utama:
1. **Game Client (Unity)**: Klien game multiplatform berbasis Unity untuk pemain menyelesaikan tantangan matematika.
2. **Web Dashboard (Vite + Vanilla JS)**: Antarmuka administrasi web untuk memantau performa pemain dan mengontrol parameter game secara real-time melalui Firestore.

Semua data game, kemajuan level, skor, dan pengaturan keseimbangan permainan (game balance) disimpan dan disinkronkan menggunakan **Firebase (Auth & Firestore)**.

---

## 2. Arsitektur & Teknologi

### A. Game Client (Unity)
*   **Bahasa & Framework**: C#, Unity Engine (mendukung Input System baru & Universal Render Pipeline).
*   **Firebase SDK**: Menggunakan Firebase Auth untuk registrasi/login dan Firebase Firestore untuk penyimpanan data real-time.
*   **Mekanisme Gameplay Utama**:
    *   **Drag & Drop**: Menyusun angka/operator matematika pada slot kosong.
    *   **Virtual Numpad / Direct Input**: Jawaban numerik langsung oleh pemain (dikelola oleh `NumericInputAnswerManager.cs`).
    *   **Equation Balancing Scale**: Konsep timbangan untuk menyamakan nilai ruas kiri dan kanan.
    *   **Scratchpad Canvas**: Canvas coret-coret transparan untuk corat-coret hitungan di layar.
    *   **Matching Pairs (Duolingo-style)**: Menghubungkan tombol soal (kiri) dengan tombol jawaban (kanan) secara dinamis menggunakan visual warna koneksi (`MatchingPairsManager.cs`).
    *   **Tap-to-Fill (Equation Builder)**: Memasukkan angka/operator dari bank kata dengan mengetuk langsung tanpa seret-menyeret (`EquationBuilderManager.cs`).
*   **Sistem Kesehatan & Energi**: Pemain dibatasi oleh `max_health` dengan pemulihan bertahap berdasarkan `health_cooldown_seconds`.
*   **Remote Settings System**: Mengambil konfigurasi game secara real-time dari dokumen `settings/global` di Firestore menggunakan `RemoteSettingsManager.cs` saat game dimulai.

### B. Web Dashboard (Admin Panel)
*   **Teknologi**: Vite, Vanilla JavaScript, Vanilla CSS.
*   **Gaya Desain & Estetika**: Premium Dark Mode dengan tema warna Deep Dark Grey (`#121316` / `#1A1D20`), Neon Lime Green (`#C4F22C` / `#B5E61D`), serta aksen Biru/Ungu. Tampilan menggunakan desain **Bento Grid**, efek **Glassmorphism**, dan ikonografi bergaya iOS (SVG).
*   **Fitur**:
    *   *Overview Stats*: Menampilkan ringkasan total pengguna, rata-rata level tertinggi, dan log pemeliharaan.
    *   *User Management*: Tabel pencarian akun instan, mengubah data skor/level/darah, serta menghapus pengguna.
    *   *Global Settings*: Formulir input real-time untuk mengubah parameter Firestore `settings/global`.
    *   *Leaderboard Monitor*: Klasemen real-time dari seluruh pemain.
    *   *Admin Tools*: Simulasi pembuatan data user dummy, pembersihan akun (account purging), dan log sistem.

---

## 3. Struktur Database Firestore

### A. Collection: `users`
Setiap dokumen merepresentasikan profil pemain.
*   **Document ID**: Menggunakan format custom `user_` + 8 karakter pertama dari Firebase Auth UID (contoh: `user_abc123xy`).
*   **Struktur Dokumen**:
    ```json
    {
      "name": "Nama Lengkap",
      "username": "username_pemain",
      "email": "user@email.com",
      "age": 12,
      "score": 450,
      "LEVEL": 3,
      "LEVEL_COMPLETED": {
        "1": true,
        "2": true
      },
      "BONUS_COMPLETED": {
        "bonus_1": true
      }
    }
    ```

### B. Collection: `settings`
Menyimpan parameter konfigurasi game jarak jauh (Remote Settings).
*   **Document ID**: `global`
*   **Struktur Dokumen**:
    ```json
    {
      "max_health": 5,
      "health_cooldown_seconds": 1800,
      "question_timer_seconds": 30,
      "main_level_score_reward": 100,
      "bonus_level_score_reward": 250,
      "achievement_threshold_a": 30,
      "achievement_threshold_b": 80,
      "achievement_threshold_c": 150,
      "achievement_threshold_d": 200
    }
    ```

---

## 4. Alur Manajemen State & Caching

1.  **Login & Autentikasi (`FirebaseAuthController.cs`)**:
    *   Saat login berhasil, UID disimpan di `PlayerPrefs` dengan key `UserId`.
    *   Sistem langsung memuat scene tujuan (`targetSceneName`).
2.  **Penyimpanan Skor & Offline Sync (`ScoreManager.cs`)**:
    *   Skor disimpan secara lokal menggunakan `PlayerPrefs` (`local_score`).
    *   Perubahan skor dikirim ke Firestore secara asinkron.
    *   Jika pengiriman gagal (offline), nilai disimpan di `pending_score` dan dicoba ulang secara otomatis setiap 5 detik melalui rutin `SyncPendingScore()`.
3.  **Progres Level Klien (`LevelManager.cs`)**:
    *   Mengatur kelayakan akses level (Level 1 selalu terbuka, Level $N$ terbuka jika Level $N-1$ selesai).
    *   Mencocokkan progres lokal klien dengan record Firestore secara berkala.
4.  **Keamanan Logout & Isolasi Akun**:
    *   Saat logout dilakukan, cache `PlayerPrefs` lokal berikut wajib dihapus bersih: `UserId`, `local_score`, `pending_score`, `PlayerName`, `HasSeenWelcome`, dan `profileImageName`.
    *   Semua instance persistent manager (`ScoreManager`, `LevelManager`, `HealthManager`) dihancurkan (`Destroy`) agar re-inisialisasi berjalan bersih untuk akun baru tanpa terjadi kebocoran data (cross-account data bleeding).

---

## 5. Struktur Direktori Repositori

```text
Mathmagic-New/
├── Assets/
│   ├── 0 Scenes/              # Adegan permainan Unity (Login, Menu, Level, dll)
│   ├── 1 Script/              # Basis kode C# game
│   │   ├── Firebase/          # Auth, Firestore Sync, Leaderboard, Remote Settings
│   │   ├── Level Script/      # Logika Level, Input Jawaban, Timer, Shuffler
│   │   ├── Score/             # Management Skor, Achievements, Progres
│   │   └── UI Etc/            # Musik, Blur, Efek UI
│   ├── 2 UI/                  # Aset gambar, ikon, dan Canvas UI Unity
│   ├── 3 Font/                # Font yang digunakan dalam proyek
│   ├── 4 Prefabs/             # Prefab tombol, overlay, dan komponen reusable
│   └── TextMesh Pro/          # Paket TMP
├── Doc/
│   ├── User_Manual.docx       # Panduan pengguna klien
│   └── project_context.md     # File konteks ini
├── Plan/
│   ├── Dashboard_PRD_Plan.md  # Dokumen PRD dan Rencana Kerja Web Dashboard
│   └── GameplaySuggestions.md # Saran mekanik gameplay matematika
└── Web Dashboard/             # Proyek Dashboard Admin (Vite + HTML/CSS/JS)
    ├── src/
    │   ├── main.js            # Seluruh logika dashboard (100k+ baris, Firebase integrations)
    │   └── style.css          # Desain styles premium dark mode & bento grid
    ├── index.html             # Entry point web dashboard
    └── firestore.rules        # Aturan keamanan database Firestore
```

---

## 6. Aturan Penting & Best Practices untuk Pengembangan

1.  **Format ID Dokumen Pengguna**:
    *   *PENTING*: Selalu buat ID dokumen Firestore pengguna menggunakan format `user_` + 8 karakter UID (diambil dari UID Auth).
    *   *Contoh C#*: `string shortId = "user_" + (userId.Length >= 8 ? userId.Substring(0, 8) : userId);`
    *   *Jangan* langsung menggunakan UID Firebase Auth penuh sebagai Document ID di Firestore, karena klien Unity dikonfigurasi untuk membaca short ID ini.
2.  **Manajemen Cache & Memory**:
    *   Apabila memodifikasi fitur login/logout, selalu panggil pembersihan data yang didefinisikan dalam `ScoreManager.ClearLocalUserData()`.
3.  **Pengembangan Web Dashboard**:
    *   Hindari penggunaan framework (seperti React/Vue/Angular) kecuali diminta secara eksplisit. Dashboard harus tetap modular dalam Vanilla JS (`main.js`) dan Vanilla CSS (`style.css`).
    *   Pastikan visual selalu selaras dengan skema warna premium dark mode, layout bento grid responsif, dan animasi mikro transisi yang halus.
4.  **Menambahkan Parameter Baru**:
    *   Jika menambahkan parameter keseimbangan game baru, daftarkan parameter tersebut di Firestore dokumen `settings/global`, perbarui `RemoteSettingsManager.cs` di Unity untuk memuat nilainya, dan tambahkan field input terkait di tab "Global Settings" pada Web Dashboard.
