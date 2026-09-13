# Dokumen Flowchart Sistem Mathmagic (ISO 5807 Standard)

Dokumen ini berisi indeks dan panduan standar internasional (**ISO 5807 / ANSI**) untuk perancangan diagram alir (_flowchart_) pada ekosistem proyek **Mathmagic**.

---

## 📌 Indeks Dokumen Flowchart

| No  | Modul Sistem            | Berkas Dokumentasi                                 | Deskripsi                                                                                                                                 |
| --- | ----------------------- | -------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **User Flow (Ringkas)** | [`USER_FLOW.md`](./USER_FLOW.md)                   | Panduan alur pengalaman pengguna (User Flow / UX Journey) ringkas & naratif dari awal buka aplikasi, login, bermain, hingga selesai. |
| 2   | **Game Flowchart DOCX** | [`Flowchart_Game_Dokumen.docx`](./Archive/Flowchart_Game_Dokumen.docx) | Berkas Word (.docx) resmi memuat diagram alir asli milik pengembang (Arsip). |
| 3   | **Game Client (Unity)** | [`Flowchart_Game.md`](./Flowchart_Game.md)         | Diagram alir teknis (ISO 5807) autentikasi pemain, pemilihan level, gameplay loop, skor, offline sync, dan logout game client Unity.     |
| 4   | **Web Admin Dashboard** | [`Flowchart_WebAdmin.md`](./Flowchart_WebAdmin.md) | Diagram alir teknis (ISO 5807) autentikasi admin, manajemen sesi, remote settings, user management, dan audit log di Web Dashboard.      |
| 5   | **Laporan HKI (DOCX)**  | [`Laporan_HKI_SourceCode.docx`](./Archive/Laporan_HKI_SourceCode.docx) | Berkas Word (.docx) formal berisi potongan kode program utama Game Client Unity (Arsip). |

---

## 📐 Standar ISO 5807 & Kaidah Pembuatan Flowchart

Seluruh diagram alir dalam dokumentasi ini dirancang dengan mematuhi aturan standar internasional **ISO 5807 / ANSI**:

### 1. Simbol Standard & Spesifikasi Alur (Degree Rules)

```text
+-----------------------+-----------------------+--------------------+--------------------+
| Nama Simbol ISO       | Bentuk Visual         | Maks. Garis Masuk  | Maks. Garis Keluar |
+-----------------------+-----------------------+--------------------+--------------------+
| Terminator (Mulai)    | Oval / Rounded Box    | 0                  | 1 (dari BAWAH)     |
| Terminator (Selesai)  | Oval / Rounded Box    | 1 (dari ATAS)      | 0                  |
| Process (Proses)      | Persegi Panjang       | 1 (dari ATAS)      | 1 (dari BAWAH)     |
| Decision (Keputusan)  | Belah Ketupat         | 1 (dari ATAS/SIDE) | 2 - 3 (BAWAH/SIDE) |
| Input / Output (I/O)  | Jajar Genjang         | 1 (dari ATAS)      | 1 (dari BAWAH)     |
| Predefined Process    | Double-Border Box     | 1 (dari ATAS)      | 1 (dari BAWAH)     |
| Database / Data Store | Cylinder / Tabung     | Dibaca / Ditulis oleh Proses               |
| Connector (Konektor)  | Lingkaran dengan Huruf| 1                  | 1                  |
+-----------------------+-----------------------+--------------------+--------------------+
```

### 2. Aturan Garis dan Arah Aliran (Flow Direction Rules)

1. **Arah Utama**: Top-to-Bottom (Atas ke Bawah) dan Left-to-Right (Kiri ke Kanan).
2. **Titik Masuk (Inbound)**: Garis alir ke suatu simbol **wajib masuk dari sisi ATAS** simbol (kecuali garis balik loop yang masuk dari samping).
3. **Titik Keluar (Outbound)**:
   - Untuk **Process / I/O / Subroutine**: Garis **wajib keluar dari sisi BAWAH** simbol (tepat 1 garis keluar).
   - Untuk **Decision (Keputusan)**: Memiliki 2 atau 3 cabang keluar yang keluar dari **BAWAH** (biasanya untuk cabang `Ya` / `True`) dan **SAMPING KIRI/KANAN** (untuk cabang `Tidak` / `False`). Setiap garis keluar wajib diberi label kondisi secara jelas.
4. **Restriksi Terminator**:
   - **Mulai (Start)**: HANYA boleh memiliki **1 garis keluar** (Outbound = 1), **0 garis masuk** (Inbound = 0).
   - **Selesai (End)**: HANYA boleh memiliki **1 garis masuk** (Inbound = 1), **0 garis keluar** (Outbound = 0).
5. **Konektor Halaman (Connector)**: Lingkaran berpola huruf `(A)`, `(B)` digunakan jika alur melompati blok panjang atau pindah halaman untuk mencegah garis bersilangan (_crossing lines_).
