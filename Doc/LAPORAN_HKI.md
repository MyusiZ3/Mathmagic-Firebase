# LAPORAN CIPTAAN & POTONGAN KODE PROGRAM (SOURCE CODE) HAK CIPTA (HKI)
### DIREKTORAT JENDERAL KEKAYAAN INTELEKTUAL (DJKI) - KEMENKUMHAM RI

---

## 📌 Identitas Resmi Surat Pencatatan Ciptaan (UU No. 28 Tahun 2014)

* **Judul Ciptaan**: Mathmagic (Media Pembelajaran Interaktif Matematika Berbasis Game)
* **Jenis Ciptaan**: Permainan Video (Interactive Game Mobile & Web Admin Ecosystem)
* **Nomor Pencatatan**: 001400842
* **Nomor & Tanggal Permohonan**: EC002026136398 (7 Agustus 2026)
* **Pemegang Hak Cipta**: UNIVERSITAS TELKOM
* **Tautan Aplikasi**: [https://shorturl.at/cb55f](https://shorturl.at/cb55f)

---

## 👥 Tim Pengembang & Peran Proyek (Development Team)

| Nama Lengkap | Peran Proyek | Deskripsi Kontribusi |
| :--- | :--- | :--- |
| **Muhamad Sidik** | **Full Developer & Main UI** | Pengembang Utama aplikasi game Unity 6, Pemrogram Logika C#, UI Implementation, & Integrasi Firebase |
| **Rizky Yonanda** | **Project Manager** | Manajer Proyek, Penjadwalan, & Koordinasi Tim |
| **Zahra Imani** | **UI/UX Design** | Perancang Antarmuka (UI/UX) & Aset Visual Game |
| **Dean Erick Adhitia Nugraha** | **Sound Designer** | Desain Efek Suara (SFX) & Musik Latar (BGM) |
| **Sheilan Mayra** | **QA Testing** | Pengujian Kualitas Aplikasi (Quality Assurance) & Pengujian Sistem |
| **Rikman Aherliwan Rudawan** | **Dosen Pembimbing** | Pembimbing Akademik (Universitas Telkom) |

* **Tanggal & Tempat Pengumuman Pertama**: 23 Juli 2026, di Kota Bandung
* **Jangka Waktu Pelindungan**: 50 (lima puluh) tahun sejak pertama kali diumumkan
* **Bahasa Pemrograman**: C# (Unity Engine Framework) & JavaScript (Vite Web Dashboard)
* **Dokumen Panduan Resmi**: [`Manual Book Mathmagic.pdf`](./Manual%20Book%20Mathmagic.pdf)
* **Surat Pengakuan Industri**: [`SURAT PENGAKUAN INDUSTRI.pdf`](./SURAT%20PENGAKUAN%20INDUSTRI.pdf)
* **Berkas Sumber Kode HKI**: [`Laporan_HKI_SourceCode.docx`](./Archive/Laporan_HKI_SourceCode.docx)

> [!NOTE]
> Salinan resmi fisik Surat Pencatatan Ciptaan (Sertifikat HKI) disimpan oleh Pemegang Hak Cipta (Universitas Telkom) dan Tim Pencipta untuk menjaga kerahasiaan data krusial institusi. Identitas resmi pencatatan disajikan lengkap pada dokumen ini.


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
