# Game Client Flowchart Documentation (Unity - Mathmagic)

## International ISO 5807 / ANSI Flowchart Standard

This document provides complete flowchart documentation for the **Mathmagic Unity Game Client**. The flowcharts are systematically structured following the **ISO 5807** international standard regarding symbol shapes, inbound/outbound line degree constraints, and C# source code logic (`FirebaseAuthController.cs`, `LevelManager.cs`, `ScoreManager.cs`, `RemoteSettingsManager.cs`).

---

## ISO 5807 Symbol Specifications & Conventions

```text
+-----------------------+-----------------------+--------------------+--------------------+
| ISO Symbol            | Symbol Name           | Inbound Limit (In) | Outbound Limit(Out)|
+-----------------------+-----------------------+--------------------+--------------------+
| ([Start / End])       | Terminator            | Start: 0 / End: 1  | Start: 1 / End: 0  |
| [/ Input / Output /]  | Data Input / Output   | Max 1              | Max 1              |
| [   Process Data  ]   | Process               | Max 1              | Max 1              |
| <  Decision ?   >     | Decision (Criteria)   | Max 1              | Exactly 2 or 3     |
| [[  Subroutine / API]]| Predefined Process    | Max 1              | Max 1              |
| [( Firestore DB / Prefs)] Data Store          | Read / Written by Process               |
| (( Connector (A) ))   | Page Connector        | Max 1              | Max 1              |
+-----------------------+-----------------------+--------------------+--------------------+
```

### Rules & Conventions:

1. **Primary Direction**: Top-to-Bottom. Inbound lines enter from the **TOP** of symbols, and outbound lines exit from the **BOTTOM** (or **SIDES** specifically for Decision branches).
2. **Start Terminator Outdegree**: Exactly 1 outbound line from the BOTTOM. Indegree = 0.
3. **End Terminator Indegree**: Exactly 1 inbound line from the TOP. Outdegree = 0.
4. **Process & I/O Outdegree**: Exactly 1 outbound line from the BOTTOM. Indegree = 1 from the TOP.
5. **Decision Outdegree**: Exactly 2 exit branches (e.g., `Yes` exits from BOTTOM, `No` exits from RIGHT/LEFT) with clear condition labels.

---

## Overview Flowchart

The following high-level flowchart outlines the full Unity Game Client lifecycle:

```mermaid
flowchart TD
    Start Game (Launch Game Client) --> InitFirebase[[Initialize Firebase App & Dependencies]]
    InitFirebase --> CheckDep{Dependencies Available?}

    CheckDep -- No --> AlertErr[/Display Firebase Error Alert/]
    AlertErr --> EndApp([End / Quit Application])

    CheckDep -- Yes --> CheckAuth{Is User Authenticated?}

    CheckAuth -- No --> UI_Login[/Display Login / Registration Screen/]
    CheckAuth -- Yes --> FetchRemote[[Load PlayerPrefs UserId & Fetch Remote Settings]]

    UI_Login --> SubmitAuth[/Player Inputs Credentials/]
    SubmitAuth --> ProcessAuth[[Authenticate via Firebase Auth SDK]]
    ProcessAuth --> AuthSuccess{Authentication Successful?}

    AuthSuccess -- No --> ShowAuthErr[/Display Error UI Feedback/] --> UI_Login
    AuthSuccess -- Yes --> FetchRemote

    FetchRemote --> LoadMenu[[Load Main Menu & Fetch Firestore Level Progress]]
    LoadMenu --> SelectLevel[/Player Selects Main / Bonus Level/]

    SelectLevel --> CheckHealth{Is Player Health > 0?}
    CheckHealth -- No --> ShowNoHealth[/Display HP Cooldown Warning/] --> LoadMenu

    CheckHealth -- Yes --> StartGameplay[[Load Level Gameplay & Problem Timer]]
    StartGameplay --> PlayerInput[/Player Submits Problem Answer/]

    PlayerInput --> CheckAnswer{Is Answer Correct?}

    CheckAnswer -- No --> DeductHealth[Deduct Health -1 via HealthManager]
    DeductHealth --> CheckZeroHealth{Is Health == 0?}
    CheckZeroHealth -- Yes --> GameOver[/Display Game Over Overlay/] --> LoadMenu
    CheckZeroHealth -- No --> PlayerInput

    CheckAnswer -- Yes --> CompleteLvl[[Level Completion & Calculate Score Reward]]
    CompleteLvl --> SyncScore[[ScoreManager AddScore & Sync to Firestore]]
    SyncScore --> ShowWin[/Display Level Victory / Next Level Overlay/]

    ShowWin --> UserOption{Player Choice?}
    UserOption -- Next Level --> SelectLevel
    UserOption -- Main Menu --> LoadMenu
    UserOption -- Logout --> DoLogout[[LogoutController Clear PlayerPrefs & Destroy Singletons]]

    DoLogout --> UI_Login
```

---

## Modular System Flowchart Details

---

### 1. Authentication & Registration Module (`FirebaseAuthController.cs`)

Manages new account registration (with username uniqueness validation on Firestore) and login using email or username.

#### ISO Standard Diagram (ASCII Representation):

```text
               +-----------------------------------+
               |       (START: App Launch)         |
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
                 | (Yes - bottom)           | (No - side)
                 v                          v
   +---------------------------+   +-------------------------------+
   | < User CurrentUser !=     |   | [/ Show Alert: Firebase Error/]|
   |   null (Auto-Login)? >    |   +-------------------------------+
   +---------------------------+                   |
     | (Yes)           | (No)                      v
     |                 v               +-----------------------+
     |   +---------------------------+ | (END: Exit App)       |
     |   | [/ Display Login UI      /] | +-----------------------+
     |   +---------------------------+
     |                 |
     |                 v
     |   +---------------------------+
     |   | [/ Select Auth Mode:     /]
     |   |    Register / Login       |
     |   +---------------------------+
     |                 |
     |                 v
     |   +---------------------------+
     |   | < Selected Mode? >        |
     |   +---------------------------+
     |     | (Register)              | (Login)
     |     v                         v
     |  +---------------------+   +---------------------+
     |  | [/ Input: Name,    /]   | [/ Input: Username  /]
     |  |    Username, Email, |   |    / Email, Pass    |
     |  |    Password, Conf  |    +---------------------+
     |  +---------------------+              |
     |             |                         v
     |             v              +---------------------+
     |  +---------------------+   | < Format Valid      |
     |  | < Form Valid?      |   |   Email? >          |
     |  |   (Length/Match) >  |   +---------------------+
     |  +---------------------+     | (Yes)        | (No)
     |    | (Yes)      | (No)       v              v
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
     | +------------+              |              | (Yes)      | (No)
     |   | (Yes)|(No)              |              v            v
     |   v      v                  |      +---------------+ +-------+
     | +------+ +-----------+      |      | [Get Email &  | | [/Show|
     | |[Auth | |[/Show Err |      |      |  PerformAuth] | |  Err/] |
     | | Reg] | |  Alert/]  |      |      +---------------+ +-------+
     | +------+ +-----------+      |              |
     |   |                         v              v
     |   +-------------------> +--------------------+
     |                         | < Auth Success? >  |
     |                         +--------------------+
     |                           | (Yes)       | (No)
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

---

### 2. Remote Settings & Level Management Module (`RemoteSettingsManager.cs` & `LevelManager.cs`)

Responsible for fetching centralized game balance configurations from Firestore (`settings/global`) and managing player level unlocks.

#### ISO Standard Diagram (ASCII Representation):

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
                         | (Yes)              | (No / Offline)
                         v                    v
           +--------------------------+ +---------------------------+
           | [Update Game Balance:    | | [Use Fallback Defaults:   |
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
                       | [Update UI Button State:  |
                       |  Level 1 = Interactive,   |
                       |  Level N = Unlock if      |
                       |  Level N-1 Complete]      |
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [/ Player Selects Level /]|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | (( B: Enter Level Scene ))|
                       +---------------------------+
```

---

### 3. Gameplay Module & Dynamic Answer Mechanics

Interactive gameplay covers 5 challenge types (Matching Pairs, Numpad Direct Input, Drag & Drop, Equation Builder, Equation Scale).

#### ISO Standard Diagram (ASCII Representation):

```text
                       +---------------------------+
                       | (( B: Start Level Scene ))|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [Initialize Level:        |
                       |  Setup Timer & Problems]  |
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | [/ Player Inputs Answer /]|
                       |    (Tap/Numpad/Drag/Match)|
                       +---------------------------+
                                     |
                                     v
                       +---------------------------+
                       | < Is Answer Correct       |
                       |   or Time Expired? >      |
                       +---------------------------+
                         | (Incorrect / Timeout) | (Correct)
                         v                       v
           +--------------------------+ +---------------------------+
           | [[ HealthManager.        | | [/ Display Correct UI /]  |
           |    DeductHealth(1)     ]]| +---------------------------+
           +--------------------------+               |
                         |                            v
                         v              +---------------------------+
           +--------------------------+ | [[ LevelCompletion.       |
           | < Is Health == 0? >      | |    CompleteLevel()      ]]|
           +--------------------------+ +---------------------------+
             | (Yes)           | (No)                 |
             v                 v                      v
     +---------------+ +---------------+ +---------------------------+
     | [/ Game Over  | | [/ Wrong Sound| | (( C: Process Score Sync))|
     |   Overlay /]  | |    & Visual /]| +---------------------------+
     +---------------+ +---------------+
             |                 |
             v                 v
     +---------------+ (Back to Input)
     | (( Go Menu )) |
     +---------------+
```

---

### 4. Score Persistence & Offline Sync Module (`ScoreManager.cs`)

Guarantees no data loss during network disconnections using a two-tier caching mechanism (`local_score` & `pending_score`).

#### ISO Standard Diagram (ASCII Representation):

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
                       | [( PlayerPrefs: Save      |
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
                         | (Yes)              | (No / Connection Fail)
                         v                    v
           +--------------------------+ +---------------------------+
           | [( Firestore DB: Update  | | [( PlayerPrefs: Save      |
           |    users.score = val   )]| |    pending_score = val  )]|
           +--------------------------+ +---------------------------+
                         |                    |
                         v                    v
           +--------------------------+ +---------------------------+
           | [( PlayerPrefs: Clear    | | [[ Background Coroutine:  |
           |    pending_score = 0   )]| |    SyncPendingScore()   |
           +--------------------------+ |    Retry per 5 seconds ]]|
                         |              +---------------------------+
                         +--------+-----------+
                                  |
                                  v
                       +---------------------------+
                       | [/ Victory Level Overlay /|
                       |    Options: Next / Menu  /]
                       +---------------------------+
```

---

### 5. Logout & Account Isolation Security Module (`LogoutController.cs`)

Prevents cross-account data contamination when changing player sessions on shared devices.

#### ISO Standard Diagram (ASCII Representation):

```text
                       +---------------------------+
                       | [/ Player Clicks Logout /]|
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
                       | (END: Auth Screen Ready)  |
                       +---------------------------+
```

---

## ISO 5807 Compliance Verification Matrix

| No  | Logic Element               | ISO Symbol         | Inbound Line Rule | Outbound Line Rule | Compliance Status |
| --- | --------------------------- | ------------------ | ----------------- | ------------------ | ----------------- |
| 1   | Launch Game Client          | Oval Terminator    | 0 (None)          | 1 (to BOTTOM)      | ISO Compliant     |
| 2   | End / Quit Game             | Oval Terminator    | 1 (from TOP)      | 0 (None)           | ISO Compliant     |
| 3   | User Input (Login / Form)   | Parallelogram      | 1 (from TOP)      | 1 (to BOTTOM)      | ISO Compliant     |
| 4   | Firebase Auth SDK Call      | Predefined Process | 1 (from TOP)      | 1 (to BOTTOM)      | ISO Compliant     |
| 5   | Password / HP Evaluation    | Decision Diamond   | 1 (from TOP)      | 2 Branches (BOTTOM & SIDE) | ISO Compliant |
| 6   | Firestore & PlayerPrefs     | Database Cylinder  | Read / Written    | Read / Written     | ISO Compliant     |
| 7   | Page Connector `((A))`      | Connector Circle   | 1                 | 1                  | Prevents Crossing Lines |
