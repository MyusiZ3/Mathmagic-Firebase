# Mathmagic - Rekomendasi Mekanik Gameplay Matematika Universal

Dokumen ini berisi beberapa ide mekanik gameplay matematika yang interaktif, disesuaikan untuk game matematika **universal/umum** (aljabar, aritmatika dasar, persamaan, geometri, dsb), menggunakan sistem UI Unity.

---

## 1. Mekanik Drag & Drop (Menyeret & Meletakkan)

### Deskripsi
Pemain menyeret objek angka, variabel, operator, atau bentuk geometri ke posisi yang kosong untuk melengkapi pernyataan matematika.

---

## 2. Virtual Numpad / Input Angka Langsung

### Deskripsi
Pemain menyelesaikan hitungan sendiri (perkalian, pembagian, aljabar) tanpa opsi jawaban bantuan. Ini sangat efektif untuk menguji pemahaman matematika murni secara mandiri.

---

## 3. Timbangan Persamaan (Equation Balancing Scale)

### Deskripsi
Mekanik visual interaktif untuk mengajarkan konsep keseimbangan persamaan matematika (dasar aljabar). Sisi kiri dan kanan timbangan harus memiliki nilai matematis yang sama.

---

## 4. Papan Coret-Coret Layar (Scratchpad Canvas)

### Deskripsi
Fitur coret-coret transparan yang sangat penting untuk matematika umum, terutama ketika soal membutuhkan perhitungan bertahap (seperti perkalian bersusun, pembagian porogapit, atau pencarian FPB/KPK).

---

## 5. Mekanik Ala Duolingo (Tap-to-Arrange & Matching)

Mekanik ala Duolingo sebenarnya **jauh lebih mudah dibuat di Unity** dibanding Drag & Drop, karena tidak membutuhkan perhitungan fisika seret-menyereit. Semua interaksi hanya menggunakan tombol klik biasa!

### A. Cocokkan Pasangan (Matching Pairs)
* **Deskripsi**: Terdapat dua kolom kartu. Kolom kiri berisi pertanyaan/operasi matematika, kolom kanan berisi hasil angka acak. Pemain mengetuk satu kartu di kiri, lalu mengetuk pasangannya di kanan.
* **Contoh Penerapan**:
  * Kiri: `[ 5 x 4 ]`, `[ 12 - 3 ]`, `[ 16 / 4 ]`
  * Kanan: `[ 9 ]`, `[ 20 ]`, `[ 4 ]`

### B. Susun Persamaan / Blok Klik (Tap-to-Fill)
* **Deskripsi**: Soal memiliki beberapa slot kosong. Di bagian bawah layar terdapat "bank kata/angka". Pemain tidak menyeret, melainkan **cukup mengetuk** angka di bank kata tersebut untuk memasukkannya ke slot kosong di atas secara berurutan.

---

## 6. Desain Sistem Scoring & Penalti Baru

Untuk mekanik seperti **Matching Pairs** atau **Tap-to-Arrange**, ada 2 metode scoring yang sangat cocok:

### Metode A: Scoring Langsung Per Tindakan (Immediate Reward)
Setiap pemain mencocokkan satu pasang, langsung diproses:
* **Jika Benar**:
  * Skor bertambah kecil secara instan (misal: `+5` atau `+10` skor).
  * Kartu yang cocok terkunci/menghilang dengan efek suara sukses.
* **Jika Salah**:
  * Nyawa (HP) langsung berkurang `1`.
  * Kartu berubah warna merah sebentar, lalu kembali ke posisi semula.
* **Akhir Level**:
  * Ketika seluruh pasangan sudah habis tercocokkan, panggil `NextQuestion()` untuk membuka soal/panel berikutnya.

> [!NOTE]
> *Kelebihan*: Mudah dipahami pemain karena umpan balik (feedback) instan. Cocok untuk anak-anak atau pemain kasual.

---

### Metode B: Scoring Akumulasi di Akhir Panel (Performance-Based)
Skor tidak bertambah per pasang, melainkan dihitung sekaligus setelah panel selesai dibersihkan:
* **Selama Permainan**:
  * Pemain mencocokkan pasangan. Jika salah, HP berkurang seperti biasa (agar ada tantangan bertahan hidup).
  * Kita mencatat jumlah kesalahan (*mistake counter*) secara sembunyi-sembunyi di kode.
* **Ketika Panel Selesai**:
  * Pemain mendapatkan **Skor Dasar**: `+30` poin.
  * Dikurangi **Penalti Kesalahan**: `-5` poin untuk setiap kali salah tebak.
  * Ditambah **Bonus Waktu**: Sisa detik di Timer dikalikan pengali (misal: `sisaDetik * 2` poin).
  * Total Skor Akhir = `Skor Dasar - Penalti Kesalahan + Bonus Waktu`.

> [!TIP]
> *Kelebihan*: Lebih menantang dan terasa premium karena pemain yang berpikir cepat dan teliti akan mendapatkan skor jauh lebih tinggi (karena Bonus Waktu dan minim Penalti).
