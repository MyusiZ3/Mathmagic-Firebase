# Mathmagic System Flowchart Documentation (ISO 5807 Standard)

This document contains the index and international standard (**ISO 5807 / ANSI**) guidelines for system flowchart design across the **Mathmagic** project ecosystem.

---

## 📌 Flowchart Documentation Index

| No  | System Module           | Documentation File                                 | Description                                                                                                                              |
| --- | ----------------------- | -------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **User Flow (Summary)** | [`USER_FLOW.md`](./USER_FLOW.md)                   | Concise narrative guide for the user experience (User Flow / UX Journey) from app open, login, gameplay, to exit.                        |
| 2   | **Game Flowchart DOCX** | [`Flowchart_Game_Dokumen.docx`](./Archive/Flowchart_Game_Dokumen.docx) | Official Word document (.docx) containing original developer flowcharts (Archived).                                                       |
| 3   | **Game Client (Unity)** | [`Flowchart_Game.md`](./Flowchart_Game.md)         | Technical flowcharts (ISO 5807) covering player auth, level selection, gameplay loop, scoring, offline sync, & logout in Unity.       |
| 4   | **Web Admin Dashboard** | [`Flowchart_WebAdmin.md`](./Flowchart_WebAdmin.md) | Technical flowcharts (ISO 5807) covering admin auth, session management, remote settings, user management, & audit logs in Web Dashboard. |
| 5   | **HKI Report (DOCX)**   | [`Laporan_HKI_SourceCode.docx`](./Archive/Laporan_HKI_SourceCode.docx) | Formal Word document (.docx) containing primary source code snippets for HKI copyright registration (Archived).                          |

---

## 📐 ISO 5807 Standard & Flowchart Conventions

All flowcharts in this documentation are designed in full compliance with **ISO 5807 / ANSI** international standards:

### 1. Standard Symbols & Flow Specifications (Degree Rules)

```text
+-----------------------+-----------------------+--------------------+--------------------+
| ISO Symbol Name       | Visual Shape          | Max Inbound Lines  | Max Outbound Lines |
+-----------------------+-----------------------+--------------------+--------------------+
| Terminator (Start)    | Oval / Rounded Box    | 0                  | 1 (from BOTTOM)    |
| Terminator (End)      | Oval / Rounded Box    | 1 (from TOP)       | 0                  |
| Process               | Rectangle             | 1 (from TOP)       | 1 (from BOTTOM)    |
| Decision              | Diamond               | 1 (from TOP/SIDE)  | 2 - 3 (BOTTOM/SIDE)|
| Input / Output (I/O)  | Parallelogram         | 1 (from TOP)       | 1 (from BOTTOM)    |
| Predefined Process    | Double-Border Box     | 1 (from TOP)       | 1 (from BOTTOM)    |
| Database / Data Store | Cylinder / Tank       | Read / Written by Process               |
| Connector             | Circle with Letter    | 1                  | 1                  |
+-----------------------+-----------------------+--------------------+--------------------+
```

### 2. Flow Line Rules & Directionality (Flow Direction Rules)

1. **Primary Flow Axis**: Top-to-Bottom and Left-to-Right.
2. **Inbound Points**: Flow lines connecting to a symbol **must enter from the TOP** (except return loop lines entering from the side).
3. **Outbound Points**:
   - For **Process / I/O / Subroutine**: Lines **must exit from the BOTTOM** (exactly 1 outbound line).
   - For **Decision**: Must have 2 or 3 outbound lines exiting from the **BOTTOM** (typically `Yes` / `True`) and **SIDES** (typically `No` / `False`). Every exit line must be clearly labeled with its evaluation condition.
4. **Terminator Constraints**:
   - **Start**: MUST only have **1 outbound line** (Outbound = 1), **0 inbound lines** (Inbound = 0).
   - **End**: MUST only have **1 inbound line** (Inbound = 1), **0 outbound lines** (Outbound = 0).
5. **Page Connectors**: Lettered connector circles `(A)`, `(B)` are used when flow crosses long blocks or pages to prevent crossing lines.
