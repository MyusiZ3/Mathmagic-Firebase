# LAPORAN POTONGAN KODE PROGRAM (SOURCE CODE) UTAMA UNTUK HAK CIPTA (HKI)
### DIREKTORAT JENDERAL KEKAYAAN INTELEKTUAL (DJKI) - KEMENKUMHAM RI

---

## 📌 Identitas Ciptaan
* **Judul Ciptaan**: MATHMAGIC: MEDIA PEMBELAJARAN INTERAKTIF MATEMATIKA BERBASIS GAME
* **Jenis Ciptaan**: Program Komputer (Software Aplikasi Interactive Game Mobile)
* **Tahun Pembuatan**: 2026
* **Bahasa Pemrograman**: C# (Unity Engine Framework)
* **Berkas DOCX Asli**: [`Doc/Laporan_HKI_SourceCode.docx`](./Laporan_HKI_SourceCode.docx)

---

## 💻 1. Modul Autentikasi & Isolasi Akun: `FirebaseAuthController.cs`
* **Lokasi Berkas**: `Assets/1 Script/Firebase/FirebaseAuthController.cs`
* **Deskripsi Fungsi**: Modul utama autentikasi dan isolasi data pengguna pada Game Client Unity. Mengelola registrasi akun, pengecekan keunikan username secara real-time pada Cloud Firestore, validasi RegEx email, enkripsi login Firebase Auth, auto-login berbasis sesi token lokal, serta mekanisme pembersihan cache `PlayerPrefs` saat logout untuk mencegah kebocoran data antar akun (*cross-account data isolation*).

```csharp
// Modul 1: FirebaseAuthController.cs (Unity C#)
// (Kode program lengkap tersimpan pada berkas Laporan_HKI_SourceCode.docx)
```

---

## 💻 2. Modul Manajemen Skor & Sinkronisasi Firestore: `ScoreManager.cs`
* **Lokasi Berkas**: `Assets/1 Script/Score/ScoreManager.cs`
* **Deskripsi Fungsi**: Modul inti pencatatan skor pemain, akumulasi nilai permainan, dan sinkronisasi data real-time ke Cloud Firestore. Dilengkapi dengan sistem *offline-fallback* (menyimpan skor ke cache PlayerPrefs lokal jika koneksi terputus) yang secara otomatis akan dikirimkan ke cloud saat koneksi internet pulih, serta fungsi pembersihan memori lokal (`ClearLocalUserData`) saat terjadi sesi logout.

```csharp
// Modul 2: ScoreManager.cs (Unity C#)
// (Kode program lengkap tersimpan pada berkas Laporan_HKI_SourceCode.docx)
```
