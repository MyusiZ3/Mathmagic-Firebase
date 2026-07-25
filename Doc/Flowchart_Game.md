# Flowchart Game Client (Unity - Mathmagic)

## Standar ISO 5807 / ANSI Flowchart Standard

Dokumen ini mendokumentasikan diagram alir (_flowchart_) lengkap untuk aplikasi **Game Client (Unity)** proyek **Mathmagic**. Diagram alir ini disusun secara sistematis mengikuti standar internasional **ISO 5807** mengenai simbol, batasan garis masuk/keluar (_inbound/outbound rules_), serta logika pengkodean pada C# Unity (`FirebaseAuthController.cs`, `LevelManager.cs`, `ScoreManager.cs`, `RemoteSettingsManager.cs`).

---

## 📌 Ringkasan Simbol & Kaidah ISO 5807

```text
+-----------------------+-----------------------+--------------------+--------------------+
| Simbol ISO            | Nama Simbol           | Batasan Masuk (In) | Batasan Keluar(Out)|
+-----------------------+-----------------------+--------------------+--------------------+
| ([Mulai / Selesai])   | Terminator            | Start: 0 / End: 1  | Start: 1 / End: 0  |
| [/ Input / Output /]  | Data Input / Output   | Maksimal 1         | Maksimal 1         |
| [   Proses Data   ]   | Process               | Maksimal 1         | Maksimal 1         |
| <  Keputusan ?  >     | Decision (Kriteria)   | Maksimal 1         | Exactly 2 atau 3   |
| [[  Subrutin / API ]] | Predefined Process    | Maksimal 1         | Maksimal 1         |
| [( Firestore DB / Prefs)] Data Store          | Dibaca / Ditulis oleh Proses               |
| (( Connector (A) ))   | Connector Halaman     | Maksimal 1         | Maksimal 1         |
+-----------------------+-----------------------+--------------------+--------------------+
```

### Rules & Norms:

1. **Arah Utama**: Top-to-Bottom (Atas ke Bawah). Garis alir masuk dari sisi **ATAS** simbol, dan keluar dari sisi **BAWAH** (atau **SAMPING** khusus untuk cabang Decision).
2. **Outdegree Terminator Start**: Tepat 1 garis keluar dari BAWAH. Indegree = 0.
3. **Indegree Terminator End**: Tepat 1 garis masuk dari ATAS. Outdegree = 0.
4. **Outdegree Process & I/O**: Tepat 1 garis keluar dari BAWAH. Indegree = 1 dari ATAS.
5. **Outdegree Decision**: Exactly 2 cabang keluar (misal: `Ya` keluar dari BAWAH, `Tidak` keluar dari SAMPING KANAN/KIRI) yang wajib memiliki label kondisi yang jelas.

---

## 📐 Diagram Alir Utama (Overview Flowchart)

Berikut adalah diagram alir tingkat tinggi (_high-level flowchart_) untuk Game Client Mathmagic:

```mermaid
flowchart TD
    Start Game (Mulai Game Client) --> InitFirebase[[Inisialisasi Firebase App & Dependencies]]
    InitFirebase --> CheckDep{Dependencies Available?}

    CheckDep -- Tidak --> AlertErr[/Tampilkan Alert Error Firebase/]
    AlertErr --> EndApp([Selesai / Keluar Game])

    CheckDep -- Ya --> CheckAuth{Apakah User Sudah Auth?}

    CheckAuth -- Tidak --> UI_Login[/Tampilkan Layar Login / Registrasi/]
    CheckAuth -- Ya --> FetchRemote[[Load PlayerPrefs UserId & Fetch Remote Settings]]

    UI_Login --> SubmitAuth[/Pemain Input Kredensial Login / Registrasi/]
    SubmitAuth --> ProcessAuth[[Proses Authenticasi via Firebase Auth]]
    ProcessAuth --> AuthSuccess{Authenticasi Berhasil?}

    AuthSuccess -- Tidak --> ShowAuthErr[/Tampilkan Feedback Error UI/] --> UI_Login
    AuthSuccess -- Ya --> FetchRemote

    FetchRemote --> LoadMenu[[Load Main Menu & Fetch Level Progress Firestore]]
    LoadMenu --> SelectLevel[/Pemain Memilih Level Utama / Bonus/]

    SelectLevel --> CheckHealth{Apakah Darah Pemain > 0?}
    CheckHealth -- Tidak --> ShowNoHealth[/Tampilkan Warning Cooldown Darah/] --> LoadMenu

    CheckHealth -- Ya --> StartGameplay[[Load Level Gameplay & Timer Soal]]
    StartGameplay --> PlayerInput[/Pemain Memasukkan Jawaban Soal/]

    PlayerInput --> CheckAnswer{Jawaban Benar?}

    CheckAnswer -- Tidak --> DeductHealth[Kurangi Health -1 via HealthManager]
    DeductHealth --> CheckZeroHealth{Apakah Darah == 0?}
    CheckZeroHealth -- Ya --> GameOver[/Tampilkan Overlay Game Over/] --> LoadMenu
    CheckZeroHealth -- Tidak --> PlayerInput

    CheckAnswer -- Ya --> CompleteLvl[[Level Completion & Calc Score Reward]]
    CompleteLvl --> SyncScore[[ScoreManager AddScore & Sync to Firestore]]
    SyncScore --> ShowWin[/Tampilkan Overlay Victory Level / Next Level/]

    ShowWin --> UserOption{Pilihan Pemain?}
    UserOption -- Lanjut Level --> SelectLevel
    UserOption -- Kembali Menu --> LoadMenu
    UserOption -- Logout --> DoLogout[[LogoutController Clear PlayerPrefs & Destroy Singletons]]

    DoLogout --> UI_Login
```

---

## 🔍 Detail Modul System Flowchart

---

### 1. Modul Autentikasi & Registrasi (`FirebaseAuthController.cs`)

Modul ini menangani registrasi akun baru (dengan validasi username unik di Firestore) dan login menggunakan email maupun username.

#### Diagram ISO Standar (ASCII Representation):

```text
               +-----------------------------------+
               |        (START: Mulai App)         |
               +-----------------------------------+
                                 | (1 out)
                                 v
               +-----------------------------------+
               |  [[ FirebaseApp.CheckDependencies ]] |
               +-----------------------------------+
                                 | (1 out)
                                 v
               +-----------------------------------+
               | < Dependencies Available? >       |
               +-----------------------------------+
                 | (Ya - bawah)             | (Tidak - samping)
                 v                          v
   +---------------------------+   +-------------------------------+
   | < User Current User !=    |   | [/ Show Alert: Firebase Error/]|
   |   null (Auto-Login)? >    |   +-------------------------------+
   +---------------------------+                   |
     | (Ya)            | (Tidak)                   v
     |                 v               +-----------------------+
     |   +---------------------------+ | (END: Keluar App)     |
     |   | [/ Tampilkan Menu Login  /] | +-----------------------+
     |   +---------------------------+
     |                 |
     |                 v
     |   +---------------------------+
     |   | [/ Input Mode Select:    /]
     |   |    Registrasi / Login     |
     |   +---------------------------+
     |                 |
     |                 v
     |   +---------------------------+
     |   | < Mode Pilihan Pemain? >  |
     |   +---------------------------+
     |     | (Registrasi)            | (Login)
     |     v                         v
     |  +---------------------+   +---------------------+
     |  | [/ Input: Name,    /]   | [/ Input: Username  /]
     |  |    Username, Email, |   |    / Email, Pass    |
     |  |    Password, Conf  |    +---------------------+
     |  +---------------------+              |
     |             |                         v
     |             v              +---------------------+
     |  +---------------------+   | < Format Input      |
     |  | < Form Valid?      |   |   Valid Email? >    |
     |  |   (Length/Match) >  |   +---------------------+
     |  +---------------------+     | (Ya)         | (Tidak)
     |    | (Ya)       | (Tidak)    v              v
     |    v            v         +-----------+  +-------------------+
     | +------------+ +--------+ | [Perform  |  | [( Firestore DB:  |
     | | [(Firestore| | [/Show | |  Login    |  |    Query Username)|]
     | |   Check    | |  Error | |  by Email |  +-------------------+
     | |  Username] | |  Alert | |  Direct]  |            |
     | +------------+ +--------+ +-----------+            v
     |    |                        |            +-------------------+
     |    v                        v            | < Username Exists |
     | +------------+              |            |   in Firestore? > |
     | | < Unique? >|              |            +-------------------+
     | +------------+              |              | (Ya)       | (Tidak)
     |   | (Ya) |(Tidak)           |              v            v
     |   v      v                  |      +---------------+ +-------+
     | +------+ +-----------+      |      | [Get Email &  | | [/Show|
     | |[Auth | |[/Show Err |      |      |  PerformAuth] | |  Err/] |
     | | Reg] | |  Alert/]  |      |      +---------------+ +-------+
     | +------+ +-----------+      |              |
     |   |                         v              v
     |   +-------------------> +--------------------+
     |                         | < Auth Success? >  |
     |                         +--------------------+
     |                           | (Ya)        | (Tidak)
     |                           v             v
     |                         +------------+ +-----------------+
     |                         |[Save Prefs | | [/ Show Error   |
     |                         | UserId]    | |    Alert UI /]  |
     |                         +------------+ +-----------------+
     |                               |                 |
     +-------------------------------+                 v
                                     |         (Back to Login UI)
                                     v
                       +---------------------------+
                       | (( A: Go to Main Menu ))  |
                       +---------------------------+
```

#### Langkah Logika & Validasi:

1. **Pemeriksaan Sesi**: Jika Firebase Auth menyimpan token aktif (`CurrentUser != null`), sistem langsung menyimpan `UserId` ke `PlayerPrefs` dan melompati form login.
2. **Validasi Registrasi**:
   - Kolom tidak boleh kosong.
   - Format email diverifikasi dengan RegEx `^[^@\s]+@[^@\s]+\.[^@\s]+$`.
   - Panjang password minimal 6 karakter dan cocok dengan konfirmasi.
   - Mengecek ketersediaan `username` di collection Firestore `users`.
3. **Penyimpanan Profil Firestore**:
   - ID dokumen menggunakan format kustom `user_` + 8 karakter awal Firebase Auth UID.
   - Menyimpan field bawaan: `name`, `username`, `email`, `score: 0`, `age: 12`.

---

### 2. Modul Remote Settings & Level Management (`RemoteSettingsManager.cs` & `LevelManager.cs`)

Modul ini bertanggung jawab mengambil konfigurasi keseimbangan game secara terpusat dari Firestore (`settings/global`) serta mengatur pembukaan level pemain.

#### Diagram ISO Standar (ASCII Representation):

```text
                       +---------------------------+
                       |  (( A: Enter Main Menu )) |
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [[ RemoteSettingsManager. |
                       |    FetchSettings()      ]]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [( Firestore: Fetch       |
                       |    settings/global      )]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | < Firestore Connected? >  |
                       +---------------------------+
                         | (Ya)               | (Tidak/Offline)
                         v                    v
           +--------------------------+ +---------------------------+
           | [Update Game Balance:    | | [Gunakan Default Values:  |
           |  max_health, cooldown,   | |  max_health = 5,          |
           |  question_timer, reward] | |  cooldown = 1800, timer=30|
           +--------------------------+ +---------------------------+
                         |                    |
                         +--------+-----------+
                                  |
                                  v
                       +---------------------------+
                       | [[ LevelManager.Check     |
                       |    LevelProgress()      ]]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [( Firestore: Get User    |
                       |    LEVEL & COMPLETED    )]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [Update State Tombol UI:  |
                       |  Level 1 = Interaktif,    |
                       |  Level N = Unlock jika    |
                       |  Level N-1 Selesai]       |
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [/ Pemain Memilih Level /]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | (( B: Enter Level Scene ))|
                       +---------------------------+
```

#### Komponen Remote Settings:

- `max_health`: Batas jumlah nyawa maksimal.
- `health_cooldown_seconds`: Durasi pemulihan 1 nyawa (default: 1800 detik).
- `question_timer_seconds`: Timer batas waktu per soal.
- `main_level_score_reward`: Hadiah skor level utama.
- `bonus_level_score_reward`: Hadiah skor level bonus.

---

### 3. Modul Gameplay & Mekanik Jawaban Dinamis

Mekanik gameplay mencakup 5 tipe tantangan interaktif (Matching Pairs, Numpad Direct Input, Drag & Drop, Equation Builder, Equation Scale).

#### Diagram ISO Standar (ASCII Representation):

```text
                       +---------------------------+
                       | (( B: Start Level Scene ))|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [Inisialisasi Level:      |
                       |  Setup Timer & Soal]      |
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [/ Pemain Input Jawaban /]|
                       |    (Tap/Numpad/Drag/Match)|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | < Apakah Jawaban Benar    |
                       |   atau Waktu Habis? >     |
                       +---------------------------+
                         | (Salah / Timeout)  | (Benar)
                         v                    v
           +--------------------------+ +---------------------------+
           | [[ HealthManager.        | | [/ Tampilkan UI Correct /]|
           |    DeductHealth(1)     ]]| +---------------------------+
           +--------------------------+               |
                         |                            v
                         v              +---------------------------+
           +--------------------------+ | [[ LevelCompletion.       |
           | < Apakah Health == 0? >  | |    CompleteLevel()      ]]|
           +--------------------------+ +---------------------------+
             | (Ya)            | (Tidak)              |
             v                 v                      v
     +---------------+ +---------------+ +---------------------------+
     | [/ Overlay    | | [/ Sound FX   | | (( C: Process Score Sync))|
     |   Game Over /]| |    Wrong UI /]| +---------------------------+
     +---------------+ +---------------+
             |                 |
             v                 v
     +---------------+ (Kembali Input)
     | (( Go Menu )) |
     +---------------+
```

---

### 4. Modul Penyimpanan Skor & Sinkronisasi Offline (`ScoreManager.cs`)

Game Mathmagic menjamin data tidak hilang saat koneksi terputus dengan mekanisme caching dua tingkat (`local_score` & `pending_score`).

#### Diagram ISO Standar (ASCII Representation):

```text
                       +---------------------------+
                       | (( C: Process Score Sync))|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [ScoreManager.AddScore(): |
                       |  currentScore += reward]  |
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [( PlayerPrefs: Simpan    |
                       |    local_score = score  )]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [[ TryUpdateFirestore() ]]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | < Firestore Online? >     |
                       +---------------------------+
                         | (Ya)               | (Tidak / Connection Fail)
                         v                    v
           +--------------------------+ +---------------------------+
           | [( Firestore DB: Update  | | [( PlayerPrefs: Simpan    |
           |    users.score = val   )]| |    pending_score = val  )]|
           +--------------------------+ +---------------------------+
                         |                    |
                         v                    v
           +--------------------------+ +---------------------------+
           | [( PlayerPrefs: Clear    | | [[ Background Coroutine:  |
           |    pending_score = 0   )]| |    SyncPendingScore()   |
           +--------------------------+ |    Retry per 5 detik  ]]|
                         |              +---------------------------+
                         +--------+-----------+
                                  |
                                  v
                       +---------------------------+
                       | [/ Overlay Victory Level /|
                       |    Opsi: Next / Menu     /]
                       +---------------------------+
```

---

### 5. Modul Logout & Keamanan Isolasi Akun (`LogoutController.cs`)

Untuk mencegah kebocoran data antar akun (_cross-account data bleeding_) saat berganti pemain di perangkat yang sama.

#### Diagram ISO Standar (ASCII Representation):

```text
                       +---------------------------+
                       | [/ Pemain Klik Logout /]  |
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [[ FirebaseAuth.SignOut()]]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [[ ScoreManager.          |
                       |    ClearLocalUserData() ]]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [( PlayerPrefs: Delete    |
                       |    UserId, local_score,   |
                       |    pending_score, Name  )]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [Destroy Singletons:      |
                       |  ScoreManager,            |
                       |  LevelManager,            |
                       |  HealthManager]           |
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [Load Scene: LoginScene]  |
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | (END: Layar Auth Siap)    |
                       +---------------------------+
```

---

## 📑 Tabel Matriks Verifikasi Standar ISO 5807

| No  | Elemen Logic               | Simbol ISO         | Aturan Garis Masuk (Inbound) | Aturan Garis Keluar (Outbound) | Keterangan Standar        |
| --- | -------------------------- | ------------------ | ---------------------------- | ------------------------------ | ------------------------- |
| 1   | Mulai Game Client          | Terminator Oval    | 0 (None)                     | 1 (Ke BAWAH)                   | Sesuai ISO                |
| 2   | Selesai / Quit Game        | Terminator Oval    | 1 (Dari ATAS)                | 0 (None)                       | Sesuai ISO                |
| 3   | Input User (Login/Form)    | Parallelogram      | 1 (Dari ATAS)                | 1 (Ke BAWAH)                   | Sesuai ISO                |
| 4   | Panggilan Firebase Auth    | Predefined Process | 1 (Dari ATAS)                | 1 (Ke BAWAH)                   | Sesuai ISO                |
| 5   | Evaluasi Password / Health | Decision (Diamond) | 1 (Dari ATAS)                | 2 Cabang (BAWAH & SAMPING)     | Sesuai ISO                |
| 6   | Firestore & PlayerPrefs    | Database / Storage | Dibaca/Ditulis               | Dibaca/Ditulis                 | Sesuai ISO                |
| 7   | Connector Halaman `((A))`  | Connector Circle   | 1                            | 1                              | Memutus penyilangan garis |
