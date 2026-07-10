# PRD & Implementation Plan: Mathmagic Web Dashboard & Remote Settings

Dokumen ini mendefinisikan kebutuhan fungsional, arsitektur database, desain antarmuka, dan rencana implementasi untuk sistem **Remote Settings (Firestore)** dan **Web Dashboard (Vite + Vanilla JS)** untuk Mathmagic.

---

## 1. Firestore Remote Settings System

Untuk menghindari rebuild game saat ingin mengubah parameter keseimbangan gameplay (game balance), kita akan membuat satu dokumen konfigurasi global di Firestore.

### Struktur Firestore
*   **Collection**: `settings`
*   **Document ID**: `global`
*   **Fields**:
    ```json
    {
      "max_health": 5,
      "health_cooldown_seconds": 1800,
      "question_timer_seconds": 30,
      "main_level_score_reward": 100,
      "bonus_level_score_reward": 250,
      "leaderboard_limit": 50
    }
    ```

### Fitur Parameter Game yang Dikontrol:
1.  **Max Health**: Batas darah maksimal pemain.
2.  **Time Until Next Health**: Waktu tunggu (detik) untuk memulihkan 1 darah.
3.  **Question Timer**: Durasi waktu menjawab per soal (jika ingin menerapkan timer dinamis).
4.  **Main Level Score Reward**: Penambahan skor saat menyelesaikan level utama.
5.  **Bonus Level Score Reward**: Penambahan skor saat menyelesaikan level bonus.

---

## 2. Web Dashboard Requirements

Web Dashboard akan dibangun menggunakan **Vite + Vanilla JS + Vanilla CSS** untuk menjamin performa cepat, tanpa overhead framework berat, dan kemudahan deployment.

### Gaya Desain & Estetika (Premium Dark Mode)
Mengikuti referensi visual yang diberikan:
*   **Warna Utama**: Deep Dark Grey (`#121316` / `#1A1D20`), Neon Lime Green (`#C4F22C` / `#B5E61D`), Accent Blue & Violet.
*   **Gaya UI**: Glassmorphism (blur panel + border tipis), Neumorphism halus pada tombol, Shadow lembut, sudut melengkung (`border-radius: 16px` atau lebih).
*   **Font**: *Outfit* atau *Inter* (diimpor dari Google Fonts).

### Halaman & Fitur Utama Dashboard:
1.  **Overview Stats Dashboard**:
    *   Total Pengguna Terdaftar.
    *   Statistik Level Tertinggi rata-rata pemain.
    *   Ringkasan status server & Firestore.
2.  **User Management / Accounts**:
    *   Tabel daftar akun pengguna dengan pencarian instan (by username/email).
    *   Tombol aksi: **Edit Data** (Skor, Level, Darah) dan **Delete User**.
    *   Konfirmasi aman sebelum menghapus user.
3.  **Global App Settings**:
    *   Formulir input untuk mengedit data `max_health`, `health_cooldown_seconds`, dll.
    *   Tombol "Save Settings" untuk langsung memperbarui dokumen `settings/global` di Firestore secara real-time.
4.  **Leaderboard Monitor**:
    *   Tampilan klasemen skor real-time dari Firestore.

---

## 3. Rencana Langkah Implementasi (Rencana Kerja)

### Tahap 1: Setup Proyek & Konfigurasi Firebase (Web Dashboard)
1.  Inisialisasi proyek Vite Vanilla JS di folder `Web Dashboard`.
2.  Konfigurasikan Firebase SDK untuk Web di dalam proyek Vite.
3.  Buat dokumen awal `settings/global` di Firestore.

### Tahap 2: Integrasi Game Client (Unity) dengan Remote Settings
1.  Buat script `RemoteSettingsManager.cs` di Unity untuk mengunduh dokumen `settings/global` di awal game.
2.  Integrasikan variabel `maxHealth` dan `healthCooldown` pada sistem kesehatan di Unity agar mengambil nilai dari `RemoteSettingsManager` (jika data Firestore berhasil diunduh).

### Tahap 3: Pembuatan Dashboard Web
1.  **Struktur HTML & CSS**: Buat layout premium responsive dengan Sidebar Navigasi dan Panel Konten.
2.  **Integrasi Firebase Web Auth / Login**: Tambahkan proteksi login sederhana (misal menggunakan kredensial admin tertentu atau Firebase Auth) agar dashboard tidak diakses sembarang orang.
3.  **Modul User Management**:
    *   Fetch collection `users` dari Firestore.
    *   Implementasikan fungsi Edit & Delete dokumen user.
4.  **Modul Settings**:
    *   Fetch & Write dokumen `settings/global`.
5.  **Modul Leaderboard**:
    *   Tampilkan list user yang diurutkan berdasarkan `score` tertinggi.

---

## 4. Rekomendasi Tambahan untuk Remote Settings

Berikut adalah beberapa parameter tambahan yang mungkin berguna untuk dikontrol dari jarak jauh:
*   **`maintenance_mode`** (Boolean): Jika `true`, game di Unity akan menampilkan layar pemeliharaan (tidak bisa dimainkan sementara).
*   **`app_version`** (String): Untuk memicu paksaan update (Force Update) jika versi game klien terlalu usang.
*   **`leaderboard_disabled`** (Boolean): Mematikan fitur papan peringkat jika ada masalah teknis sementara.
