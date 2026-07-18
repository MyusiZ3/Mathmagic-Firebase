import "./style.css";
import logoMagicSlogan from "./assets/logomagicslogan.png";
import rizkyPp from "./assets/Profile/rizky_pp.jpeg";
import sidikPp from "./assets/Profile/sidik_pp.jpeg";
import zahraPp from "./assets/Profile/zahra_pp.jpg";
import sheilanPp from "./assets/Profile/sheilan_pp.jpeg";
import erikPp from "./assets/Profile/erik_pp.jpeg";
import { initializeApp } from "firebase/app";
import {
  getFirestore,
  collection,
  doc,
  getDoc,
  getDocs,
  setDoc,
  addDoc,
  updateDoc,
  deleteDoc,
  onSnapshot,
  query,
  orderBy,
  limit,
} from "firebase/firestore";

// Firebase configuration
const firebaseConfig = {
  apiKey: "AIzaSyBCJ-4_2We_oBvgbXd1qqE7lTuat_DVGn8",
  authDomain: "mathmagic-df71a.firebaseapp.com",
  databaseURL: "https://mathmagic-df71a-default-rtdb.firebaseio.com",
  projectId: "mathmagic-df71a",
  storageBucket: "mathmagic-df71a.firebasestorage.app",
  messagingSenderId: "953317182090",
  appId: "1:953317182090:web:1115cdc38b29b610ebbb73",
  measurementId: "G-XSQVYYT94Z",
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const db = getFirestore(app);

// State Management
let currentTab = "dashboard";
let users = [];
let usersCurrentPage = 1;
let usersSortKey = localStorage.getItem("mm_users_sort_key") || "username";
let usersSortOrder = localStorage.getItem("mm_users_sort_order") || "asc";
let admins = [];
let globalSettings = {};
let selectedUser = null;
let selectedAdmin = null;
let isLoggedIn = sessionStorage.getItem("mm_admin_logged") === "true";
let loggedInUsername = sessionStorage.getItem("mm_admin_username") || "";
let loggedInRole = sessionStorage.getItem("mm_admin_role") || "";

// Telemetry State
let concurrencyRange = "daily";
let heatmapRange = "weekly";

// iOS-style Outline SVG Icons (SF Symbols Inspired)
const icons = {
  dashboard: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/></svg>`,
  users: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`,
  settings: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>`,
  leaderboard: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 4l3 12h14l3-12-6 7-4-7-4 7-6-7z"/><path d="M3 20h18"/></svg>`,
  search: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>`,
  edit: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>`,
  delete: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>`,
  database: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><ellipse cx="12" cy="5" rx="9" ry="3"/><path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5"/><path d="M3 12c0 1.66 4 3 9 3s9-1.34 9-3"/></svg>`,
  logout: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>`,
  admins: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>`,
  medal: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="15" r="6"/><path d="M8.56 2.9A7 7 0 0 1 19 9v1H5V9a7 7 0 0 1 1.56-4.38L7 4"/><line x1="12" y1="9" x2="12" y2="21"/></svg>`,
  trophy: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="8 21 12 17 16 21"/><path d="M17 4H7v8a5 5 0 0 0 10 0V4"/><path d="M3 4h2v4a2 2 0 0 0 4 0V4"/><path d="M21 4h-2v4a2 2 0 0 0-4 0V4"/><line x1="12" y1="17" x2="12" y2="12"/></svg>`,
  barChart: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/><line x1="2" y1="20" x2="22" y2="20"/></svg>`,
  lock: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>`,
  key: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 2l-2 2m-7.61 7.61a5.5 5.5 0 1 1-7.778 7.778 5.5 5.5 0 0 1 7.777-7.777zm0 0L15.5 7.5m0 0l3 3L22 7l-3-3m-3.5 3.5L19 4"/></svg>`,
  bolt: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/></svg>`,
  crown: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 19h20M2 19l2-9 5 4 3-7 3 7 5-4 2 9"/><circle cx="12" cy="5" r="1" fill="currentColor"/></svg>`,
  rank1: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><text x="12" y="17" text-anchor="middle" font-size="11" font-weight="700" fill="currentColor" stroke="none">1</text></svg>`,
  rank2: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><text x="12" y="17" text-anchor="middle" font-size="11" font-weight="700" fill="currentColor" stroke="none">2</text></svg>`,
  rank3: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><text x="12" y="17" text-anchor="middle" font-size="11" font-weight="700" fill="currentColor" stroke="none">3</text></svg>`,
  info: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>`,
  linkedin: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z"/><rect x="2" y="9" width="4" height="12"/><circle cx="4" cy="4" r="2"/></svg>`,
  github: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 0 0-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0 0 20 4.77 5.07 5.07 0 0 0 19.91 1S18.73.65 16 2.48a13.38 13.38 0 0 0-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 0 0 5 4.77a5.44 5.44 0 0 0-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 0 0 9 18.13V22"/></svg>`,
  mail: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>`,
  globe: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/></svg>`,
  calendar: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>`,
  palette: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22C17.5228 22 22 17.5228 22 12C22 6.5 17.5 2 12 2S2 6.5 2 12c0 1 .8 1.8 1.8 1.8h1.4c1 0 1.8.8 1.8 1.8v1.4c0 1 .8 1.8 1.8 1.8H12z"/><circle cx="7.5" cy="10.5" r="1"/><circle cx="11.5" cy="7.5" r="1"/><circle cx="16.5" cy="9.5" r="1"/><circle cx="15.5" cy="14.5" r="1"/></svg>`,
  shield: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>`,
  volume: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/><path d="M19.07 4.93a10 10 0 0 1 0 14.14M15.54 8.46a5 5 0 0 1 0 7.07"/></svg>`,
};

// UI Rendering Utilities
function showToast(message, type = "success") {
  const container = document.getElementById("toast-container");
  const toast = document.createElement("div");
  toast.className = `toast toast-${type}`;
  toast.innerText = message;
  container.appendChild(toast);
  setTimeout(() => {
    toast.style.opacity = "0";
    toast.style.transform = "translateY(10px)";
    toast.style.transition = "all 0.3s ease";
    setTimeout(() => toast.remove(), 300);
  }, 3000);
}

// Initial HTML Layout Setup
function renderAppStructure() {
  const appEl = document.getElementById("app");

  if (!isLoggedIn) {
    appEl.innerHTML = `
      <div class="login-container">
        <div class="login-card">
          <div class="login-header">
            <img src="${logoMagicSlogan}" alt="Mathmagic Logo" class="login-logo">
            <p>Enter administrative credentials to continue</p>
          </div>
          <form id="login-form">
            <div class="form-group">
              <label for="login-username">Username</label>
              <input type="text" id="login-username" class="form-control" placeholder="e.g. superadmin" required autofocus />
            </div>
            <div class="form-group">
              <label for="login-password">Password</label>
              <input type="password" id="login-password" class="form-control" placeholder="••••••••" required />
            </div>
            <button type="submit" class="btn btn-primary" style="width: 100%; height: 48px; margin-top: 1rem;">
              Sign In
            </button>
          </form>
        </div>
      </div>
      <div id="toast-container" class="toast-container"></div>
    `;

    document
      .getElementById("login-form")
      .addEventListener("submit", async (e) => {
        e.preventDefault();
        const usernameVal = document
          .getElementById("login-username")
          .value.trim()
          .toLowerCase();
        const passwordVal = document.getElementById("login-password").value;

        try {
          const adminDocRef = doc(db, "admins", usernameVal);
          let adminDoc = await getDoc(adminDocRef);

          if (
            !adminDoc.exists() &&
            usernameVal === "superadmin" &&
            passwordVal === "admin123"
          ) {
            const adminsSnapshot = await getDocs(collection(db, "admins"));
            if (adminsSnapshot.empty) {
              await setDoc(adminDocRef, {
                username: "superadmin",
                password: "admin123",
                role: "superadmin",
                createdAt: new Date().toISOString(),
              });
              adminDoc = await getDoc(adminDocRef);
              showToast("Initialized default superadmin account.", "info");
            }
          }

          if (adminDoc.exists() && adminDoc.data().password === passwordVal) {
            const data = adminDoc.data();
            sessionStorage.setItem("mm_admin_logged", "true");
            sessionStorage.setItem("mm_admin_username", data.username);
            sessionStorage.setItem("mm_admin_role", data.role);
            isLoggedIn = true;
            loggedInUsername = data.username;
            loggedInRole = data.role;
            showToast("Successfully authenticated!");
            setTimeout(() => renderAppStructure(), 500);
          } else {
            showToast("Invalid username or password.", "error");
          }
        } catch (err) {
          showToast(`Login failed: ${err.message}`, "error");
        }
      });
    return;
  }

  // Dashboard structure
  appEl.innerHTML = `
    <!-- Sidebar Navigation -->
    <aside class="sidebar">
      <div>
        <div class="sidebar-brand">
          <img src="${logoMagicSlogan}" alt="Mathmagic Logo" class="sidebar-logo">
        </div>
        
        <nav class="sidebar-nav">
          <button class="nav-btn ${currentTab === "dashboard" ? "active" : ""}" data-tab="dashboard">
            ${icons.dashboard} <span>Dashboard</span>
          </button>
          <button class="nav-btn ${currentTab === "users" ? "active" : ""}" data-tab="users">
            ${icons.users} <span>Users</span>
          </button>
          <button class="nav-btn ${currentTab === "settings" ? "active" : ""}" data-tab="settings">
            ${icons.settings} <span>Settings</span>
          </button>
          <button class="nav-btn ${currentTab === "leaderboard" ? "active" : ""}" data-tab="leaderboard">
            ${icons.leaderboard} <span>Leaderboard</span>
          </button>
          <button class="nav-btn ${currentTab === "admins" ? "active" : ""}" data-tab="admins">
            ${icons.admins} <span>Admins</span>
          </button>
          <button class="nav-btn ${currentTab === "about" ? "active" : ""}" data-tab="about">
            ${icons.info} <span>About Dev</span>
          </button>
        </nav>
      </div>

      <div class="sidebar-footer">
        <div class="admin-profile">
          <div class="admin-profile-avatar">${(loggedInUsername || "A")[0].toUpperCase()}</div>
          <div class="admin-profile-details">
            <span class="admin-name">${loggedInUsername || "Admin"}</span>
            <span class="admin-role">${loggedInRole || "admin"}</span>
          </div>
        </div>
        <button id="logout-btn" class="logout-btn" title="Logout">
          ${icons.logout} <span>Logout</span>
        </button>
      </div>
    </aside>

    <!-- Main Content Area -->
    <main class="main-content">
      <!-- Greeting and Header Block -->
      <div class="content-header-bar">
        <div>
          <h1 class="content-title" id="navbar-title-text">Dashboard Overview</h1>
          <p class="content-subtitle" id="navbar-subtitle-text">Welcome back, ${loggedInUsername || "Administrator"}! Real-time control center for game balance and telemetry.</p>
        </div>
        <div class="header-actions" id="header-actions"></div>
      </div>

      <!-- Panel: Dashboard Overview -->
      <section id="panel-dashboard" class="page-panel ${currentTab === "dashboard" ? "active" : ""}">
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-info">
              <h3>Total Registered Users</h3>
              <div id="stat-total-users" class="stat-value">-</div>
            </div>
            <div class="stat-icon-wrapper">${icons.users}</div>
          </div>
          <div class="stat-card">
            <div class="stat-info">
              <h3>Average User Score</h3>
              <div id="stat-avg-score" class="stat-value">-</div>
            </div>
            <div class="stat-icon-wrapper">${icons.leaderboard}</div>
          </div>
          <div class="stat-card">
            <div class="stat-info">
              <h3>Average Max Level</h3>
              <div id="stat-avg-level" class="stat-value">-</div>
            </div>
            <div class="stat-icon-wrapper">${icons.database}</div>
          </div>
          <div class="stat-card">
            <div class="stat-info">
              <h3>System Status</h3>
              <div id="stat-sys-status" class="stat-value">v1.0.0</div>
            </div>
            <div class="stat-icon-wrapper">${icons.settings}</div>
          </div>
        </div>

        <div class="dashboard-grid">
          <!-- Concurrency Line Chart -->
          <div class="dashboard-box telemetry-box bento-col-2">
            <div class="telemetry-header">
              <h3>${icons.dashboard} Peak Concurrency</h3>
              <div class="segmented-control" id="concurrency-range-control">
                <button class="${concurrencyRange === "daily" ? "active" : ""}" data-range="daily">Daily</button>
                <button class="${concurrencyRange === "weekly" ? "active" : ""}" data-range="weekly">Weekly</button>
                <button class="${concurrencyRange === "monthly" ? "active" : ""}" data-range="monthly">Monthly</button>
              </div>
            </div>
            <div id="concurrency-chart-container">
              <!-- Rendered dynamically -->
            </div>
          </div>
          <!-- Row 1 Right: Gameplay Balance (span 1) -->
          <div class="dashboard-box balance-bento-card">
            <div class="settings-card-header">
              <div class="settings-card-icon-wrap" style="background: rgba(181,155,235,0.12); border: 1px solid rgba(181,155,235,0.2); color: var(--color-primary);">${icons.settings}</div>
              <div>
                <h3 style="margin:0;font-size:0.9rem;font-weight:700;color:#fff;">Gameplay Balance</h3>
                <p style="margin:0;font-size:0.72rem;color:var(--text-muted);">Health &amp; timer config</p>
              </div>
            </div>
            <div class="settings-divider" style="margin: 0.75rem 0;"></div>
            <div class="balance-preview-list" id="balance-settings-preview-gameplay">
              <div class="preview-item-loading">Loading...</div>
            </div>
          </div>

          <!-- Row 2 Left: Player Activity Heatmap (span 2) -->
          <div class="dashboard-box telemetry-box bento-col-2">
            <div class="telemetry-header">
              <h3>${icons.users} Player Activity Heatmap</h3>
              <div class="segmented-control" id="heatmap-range-control">
                <button class="${heatmapRange === "daily" ? "active" : ""}" data-range="daily">Daily</button>
                <button class="${heatmapRange === "weekly" ? "active" : ""}" data-range="weekly">Weekly</button>
                <button class="${heatmapRange === "monthly" ? "active" : ""}" data-range="monthly">Monthly</button>
              </div>
            </div>
            <div id="heatmap-chart-container">
              <!-- Rendered dynamically -->
            </div>
            <div class="heatmap-legend" style="margin-top: 12px;">
              <span>Low Activity</span>
              <div class="legend-scale">
                <span class="heatmap-cell" style="opacity: 0.15"></span>
                <span class="heatmap-cell" style="opacity: 0.4"></span>
                <span class="heatmap-cell" style="opacity: 0.7"></span>
                <span class="heatmap-cell" style="opacity: 1"></span>
              </div>
              <span>Peak Load</span>
            </div>
          </div>

          <!-- Row 2 Right: Achievement Ranks (span 1) -->
          <div class="dashboard-box balance-bento-card">
            <div class="settings-card-header">
              <div class="settings-card-icon-wrap" style="background:rgba(255,224,130,0.1);border:1px solid rgba(255,224,130,0.2);color:#ffe082;">${icons.medal}</div>
              <div>
                <h3 style="margin:0;font-size:0.9rem;font-weight:700;color:#fff;">Achievement Ranks</h3>
                <p style="margin:0;font-size:0.72rem;color:var(--text-muted);">Score unlock thresholds</p>
              </div>
            </div>
            <div class="settings-divider" style="margin: 0.75rem 0;"></div>
            <div class="balance-preview-list" id="balance-settings-preview-achievements">
              <div class="preview-item-loading">Loading...</div>
            </div>
          </div>

          <!-- Row 3 Left: Top 5 Active Users (span 2) -->
          <div class="dashboard-box top-users-box bento-col-2">
            <h3>${icons.leaderboard} Top 5 Active Users</h3>
            <div class="table-container">
              <table>
                <thead>
                  <tr>
                    <th>User</th>
                    <th>Score</th>
                    <th>Max Level</th>
                  </tr>
                </thead>
                <tbody id="top-users-tbody">
                  <tr><td colspan="3" style="text-align: center; color: var(--text-muted);">Loading active users...</td></tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Row 3 Right: System Status (span 1) -->
          <div class="dashboard-box balance-bento-card">
            <div class="settings-card-header">
              <div class="settings-card-icon-wrap" style="background:rgba(239,154,154,0.1);border-color:rgba(239,154,154,0.2);color:#ef9a9a;">${icons.lock}</div>
              <div>
                <h3 style="margin:0;font-size:0.9rem;font-weight:700;color:#fff;">System Status</h3>
                <p style="margin:0;font-size:0.72rem;color:var(--text-muted);">Maintenance &amp; access</p>
              </div>
            </div>
            <div class="settings-divider" style="margin: 0.75rem 0;"></div>
            <div class="balance-preview-list" id="balance-settings-preview-status">
              <div class="preview-item-loading">Loading...</div>
            </div>
          </div>

          <!-- Row 4: Firebase Services (span 3) -->
          <div class="dashboard-box server-services-box bento-col-3">
            <h3>${icons.database} Firebase Services</h3>
            <div class="server-status-list services-status-grid">
              <div class="status-item" style="margin:0;">
                <span class="status-name">Cloud Firestore</span>
                <span class="status-badge status-online">Connected</span>
              </div>
              <div class="status-item" style="margin:0;">
                <span class="status-name">Realtime Database</span>
                <span class="status-badge status-online">Connected</span>
              </div>
              <div class="status-item" style="margin:0;">
                <span class="status-name">Authentication</span>
                <span class="status-badge status-online">Online</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Panel: Users -->
      <section id="panel-users" class="page-panel ${currentTab === "users" ? "active" : ""}">
        <div class="dashboard-grid">
          <!-- Main user table spans 2 columns -->
          <div class="table-card bento-col-2">
            <div class="table-header">
              <div class="search-bar">
                ${icons.search}
                <input type="text" id="user-search-input" placeholder="Search by username or email..." />
              </div>
              <div style="color: var(--text-muted); font-size: 0.9rem;" id="user-count-display">
                0 users found
              </div>
            </div>
            <div class="table-container">
              <table>
                <thead>
                  <tr>
                    <th class="sortable-header" data-sort-key="username">
                      <div style="display: inline-flex; align-items: center; gap: 0.35rem;">
                        Username <span class="sort-indicator"></span>
                      </div>
                    </th>
                    <th class="sortable-header" data-sort-key="email">
                      <div style="display: inline-flex; align-items: center; gap: 0.35rem;">
                        Email <span class="sort-indicator"></span>
                      </div>
                    </th>
                    <th class="sortable-header" data-sort-key="score">
                      <div style="display: inline-flex; align-items: center; gap: 0.35rem;">
                        Score <span class="sort-indicator"></span>
                      </div>
                    </th>
                    <th class="sortable-header" data-sort-key="level">
                      <div style="display: inline-flex; align-items: center; gap: 0.35rem;">
                        Level <span class="sort-indicator"></span>
                      </div>
                    </th>
                    <th style="text-align: right;">Actions</th>
                  </tr>
                </thead>
                <tbody id="users-tbody">
                  <tr><td colspan="5" style="text-align: center; color: var(--text-muted);">Loading user directory...</td></tr>
                </tbody>
              </table>
            </div>
            <div id="users-pagination-container"></div>
          </div>

          <!-- Side Bento Panel for User Diagnostics & Simulation -->
          <div class="dashboard-box">
            <h3>${icons.users} Users Distribution</h3>
            <div class="user-distribution-metrics" id="users-distribution-metrics" style="display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.25rem;">
              <!-- Rendered dynamically -->
              <div class="preview-item-loading">Analyzing distribution...</div>
            </div>
            
            <h3 style="margin-top: 1.75rem;">${icons.bolt} User Simulation</h3>
            <div class="form-group" style="margin-top: 0.5rem; margin-bottom: 0.5rem;">
              <label for="input-simulate-name" style="font-size: 0.8rem; color: var(--text-muted); margin-bottom: 0.35rem; display: block;">Player Name (Optional)</label>
              <input type="text" id="input-simulate-name" class="form-control" placeholder="Enter name..." style="width: 100%;" />
            </div>
            <div class="form-group" style="margin-bottom: 0.5rem;">
              <label for="input-simulate-username" style="font-size: 0.8rem; color: var(--text-muted); margin-bottom: 0.35rem; display: block;">Username (Optional)</label>
              <input type="text" id="input-simulate-username" class="form-control" placeholder="Enter username..." style="width: 100%;" />
            </div>
            <div class="form-group" style="margin-bottom: 0.75rem;">
              <label for="input-simulate-age" style="font-size: 0.8rem; color: var(--text-muted); margin-bottom: 0.35rem; display: block;">Age (Optional)</label>
              <input type="number" id="input-simulate-age" class="form-control" placeholder="Enter age..." style="width: 100%;" />
            </div>
            <div class="server-status-list" style="margin-top: 0.25rem;">
              <button class="btn btn-secondary" id="btn-simulate-user" style="width: 100%; justify-content: flex-start; text-align: left; padding: 0.75rem 1rem;">
                + Simulate New Player
              </button>
              <button class="btn btn-secondary" id="btn-purge-simulated" style="width: 100%; justify-content: flex-start; text-align: left; padding: 0.75rem 1rem; color: var(--color-red); border-color: rgba(255, 69, 58, 0.15);">
                Clear Simulated Users
              </button>
              <button class="btn btn-secondary" id="btn-purge-lowscore" style="width: 100%; justify-content: flex-start; text-align: left; padding: 0.75rem 1rem; color: var(--color-red); border-color: rgba(255, 69, 58, 0.15);">
                Clear Zero Score Accounts
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- Panel: Global Settings -->
      <section id="panel-settings" class="page-panel ${currentTab === "settings" ? "active" : ""}">
        <div class="settings-bento-grid">

          <!-- CARD 1: Game Balance -->
          <div class="dashboard-box settings-card-game-balance">
            <div class="settings-card-header">
              <div class="settings-card-icon-wrap" style="background: rgba(181,155,235,0.12); border: 1px solid rgba(181,155,235,0.2);">${icons.settings}</div>
              <div>
                <h3 style="margin: 0; font-size: 1rem; font-weight: 700; color: #fff;">Game Balance &amp; Configuration</h3>
                <p style="margin: 0; font-size: 0.78rem; color: var(--text-muted);">Core gameplay parameters synced to Unity client</p>
              </div>
            </div>
            <div class="settings-divider"></div>
            <div class="settings-form-grid">
              <div class="form-group">
                <label for="input-max-health">Max Health</label>
                <input type="number" id="input-max-health" class="form-control" min="1" max="100" />
              </div>
              <div class="form-group">
                <label for="input-health-cooldown">Health Cooldown (Seconds)</label>
                <input type="number" id="input-health-cooldown" class="form-control" min="10" />
              </div>
              <div class="form-group">
                <label for="input-timer">Question Timer (Seconds)</label>
                <input type="number" id="input-timer" class="form-control" min="5" />
              </div>
              <div class="form-group">
                <label for="input-leaderboard-limit">Leaderboard Show Limit</label>
                <input type="number" id="input-leaderboard-limit" class="form-control" min="1" max="100" />
              </div>
              <div class="form-group">
                <label for="input-main-reward">Main Level Score Reward</label>
                <input type="number" id="input-main-reward" class="form-control" min="1" />
              </div>
              <div class="form-group">
                <label for="input-bonus-reward">Bonus Level Score Reward</label>
                <input type="number" id="input-bonus-reward" class="form-control" min="1" />
              </div>
            </div>
            <div style="display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 1.25rem;">
              <button type="button" id="btn-reset-balance" class="btn btn-secondary">Reset</button>
              <button type="button" id="btn-save-balance" class="btn btn-primary">Save Balance</button>
            </div>
          </div>

          <!-- CARD 2: Achievement Thresholds -->
          <div class="dashboard-box settings-card-achievements">
            <div class="settings-card-header">
              <div class="settings-card-icon-wrap" style="background: rgba(255,224,130,0.1); border: 1px solid rgba(255,224,130,0.2);">${icons.medal}</div>
              <div>
                <h3 style="margin: 0; font-size: 1rem; font-weight: 700; color: #fff;">Achievement Thresholds</h3>
                <p style="margin: 0; font-size: 0.78rem; color: var(--text-muted);">Score needed to unlock each achievement rank</p>
              </div>
            </div>
            <div class="settings-divider"></div>
            <div style="display: flex; flex-direction: column; gap: 0.85rem; flex: 1;">
              <div class="achievement-threshold-row">
                <div class="achievement-rank-badge" style="background: linear-gradient(135deg, rgba(255,224,130,0.15), rgba(255,171,118,0.1)); border-color: rgba(255,224,130,0.25); color: #ffe082;">${icons.rank1}<span>A</span></div>
                <div style="flex: 1;"><label for="input-ach-a" style="font-size: 0.75rem; color: var(--text-muted); display: block; margin-bottom: 0.3rem;">Achievement A</label>
                <input type="number" id="input-ach-a" class="form-control" min="0" placeholder="0" /></div>
              </div>
              <div class="achievement-threshold-row">
                <div class="achievement-rank-badge" style="background: linear-gradient(135deg, rgba(144,202,249,0.15), rgba(144,202,249,0.08)); border-color: rgba(144,202,249,0.25); color: #90caf9;">${icons.rank2}<span>B</span></div>
                <div style="flex: 1;"><label for="input-ach-b" style="font-size: 0.75rem; color: var(--text-muted); display: block; margin-bottom: 0.3rem;">Achievement B</label>
                <input type="number" id="input-ach-b" class="form-control" min="0" placeholder="0" /></div>
              </div>
              <div class="achievement-threshold-row">
                <div class="achievement-rank-badge" style="background: linear-gradient(135deg, rgba(165,214,167,0.15), rgba(165,214,167,0.08)); border-color: rgba(165,214,167,0.25); color: #a5d6a7;">${icons.rank3}<span>C</span></div>
                <div style="flex: 1;"><label for="input-ach-c" style="font-size: 0.75rem; color: var(--text-muted); display: block; margin-bottom: 0.3rem;">Achievement C</label>
                <input type="number" id="input-ach-c" class="form-control" min="0" placeholder="0" /></div>
              </div>
              <div class="achievement-threshold-row">
                <div class="achievement-rank-badge" style="background: linear-gradient(135deg, rgba(181,155,235,0.15), rgba(181,155,235,0.08)); border-color: rgba(181,155,235,0.25); color: var(--color-primary);">${icons.database}<span>D</span></div>
                <div style="flex: 1;"><label for="input-ach-d" style="font-size: 0.75rem; color: var(--text-muted); display: block; margin-bottom: 0.3rem;">Achievement D</label>
                <input type="number" id="input-ach-d" class="form-control" min="0" placeholder="0" /></div>
              </div>
            </div>
            <div style="display: flex; justify-content: flex-end; margin-top: 1.25rem;">
              <button type="button" id="btn-save-achievements" class="btn btn-primary" style="background: linear-gradient(135deg, rgba(255,200,80,0.85), rgba(255,171,118,0.75)); border: 1px solid rgba(255,224,130,0.3); color: #1a1a1a;">Save Thresholds</button>
            </div>
          </div>

          <!-- CARD 3: Server Log -->
          <div class="dashboard-box settings-card-server-logs">
            <div class="settings-card-header">
              <div class="settings-card-icon-wrap" style="background: rgba(144,202,249,0.12); border: 1px solid rgba(144,202,249,0.2); color: var(--color-blue);">${icons.dashboard}</div>
              <div>
                <h3 style="margin: 0; font-size: 1rem; font-weight: 700; color: #fff;">Server Log</h3>
                <p style="margin: 0; font-size: 0.78rem; color: var(--text-muted);">Real-time console feed for configuration syncs and administrative operations</p>
              </div>
            </div>
            <div class="settings-divider"></div>
            <div class="telemetry-console" id="settings-console" style="height: 220px; margin-top: 0.25rem;">
              <div class="console-line"><span class="console-timestamp">[SYSTEM]</span> Remote settings listener active. Ready for updates.</div>
            </div>
          </div>

          <!-- CARD 4: Engine Configuration -->
          <div class="dashboard-box settings-card-logs">
            <div class="settings-card-header">
              <div class="settings-card-icon-wrap" style="background: rgba(165,214,167,0.1); border: 1px solid rgba(165,214,167,0.2); color: var(--color-green);">${icons.database}</div>
              <div>
                <h3 style="margin: 0; font-size: 1rem; font-weight: 700; color: #fff;">Engine Configuration</h3>
                <p style="margin: 0; font-size: 0.78rem; color: var(--text-muted);">Live values synced from Firestore</p>
              </div>
            </div>
            <div class="settings-divider"></div>
            <div class="balance-preview-list" id="settings-metadata-box">
              <div class="preview-item-loading">Retrieving balance settings...</div>
            </div>
          </div>

        </div>
      </section>

      <!-- Panel: Leaderboard -->
      <section id="panel-leaderboard" class="page-panel ${currentTab === "leaderboard" ? "active" : ""}">
        <div class="dashboard-grid">
          <!-- Leaderboard Table spans 2 columns -->
          <div class="table-card bento-col-2">
            <div class="table-header">
              <h3 style="font-family: var(--font-title); font-size: 1.1rem; font-weight: 700; color: #fff; margin: 0;">
                Global Player Standings
              </h3>
              <span style="font-size: 0.85rem; color: var(--text-muted);">Real-time rankings based on high score</span>
            </div>
            <div class="table-container">
              <table>
                <thead>
                  <tr>
                    <th style="width: 80px; text-align: center;">Rank</th>
                    <th>Player</th>
                    <th>Max Level</th>
                    <th style="text-align: right;">Total Score</th>
                  </tr>
                </thead>
                <tbody id="leaderboard-tbody">
                  <tr><td colspan="4" style="text-align: center; color: var(--text-muted);">Calculating scores...</td></tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Side Bento Panel for Hall of Fame & Tier Breakdown -->
          <div class="dashboard-box">
            <h3>${icons.trophy} Hall of Fame</h3>
            <div class="top-player-highlight-card" id="top-player-highlight" style="background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.05); border-radius: var(--radius-ios-md); padding: 1.25rem; display: flex; flex-direction: column; align-items: center; text-align: center; gap: 0.35rem; margin-top: 0.25rem;">
              <!-- Rendered dynamically -->
              <div class="preview-item-loading">Retrieving top player...</div>
            </div>

            <h3 style="margin-top: 1.75rem;">${icons.barChart} Score Tier Distribution</h3>
            <div class="tier-distribution-list" id="tier-distribution" style="display: flex; flex-direction: column; gap: 0.85rem; margin-top: 0.25rem;">
              <!-- Rendered dynamically -->
              <div class="preview-item-loading">Analyzing tiers...</div>
            </div>
          </div>
        </div>
      </section>

      <!-- Panel: Admin Management -->
      <section id="panel-admins" class="page-panel ${currentTab === "admins" ? "active" : ""}">
        <div class="dashboard-grid">
          <!-- Admins table spans 2 columns -->
          <div class="table-card bento-col-2">
            <div class="table-header">
              <div>
                <h3 style="font-family: var(--font-title); font-size: 1.1rem; font-weight: 700; color: #fff; margin: 0;">
                  Console Administrators
                </h3>
                <span id="admin-count-display" style="font-size: 0.85rem; color: var(--text-muted);">0 administrators registered</span>
              </div>
              ${
                loggedInRole === "superadmin"
                  ? `
              <button id="add-admin-btn" class="btn btn-primary">
                + Add Admin
              </button>
              `
                  : ""
              }
            </div>
            <div class="table-container">
              <table>
                <thead>
                  <tr>
                    <th>Username</th>
                    <th>Role</th>
                    <th>Created At</th>
                    ${loggedInRole === "superadmin" ? `<th style="text-align: right; width: 100px;">Actions</th>` : ""}
                  </tr>
                </thead>
                <tbody id="admins-tbody">
                  <tr><td colspan="4" style="text-align: center; color: var(--text-muted);">Loading admin accounts...</td></tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Side Bento Panel for Console Access & Security Logs -->
          <div class="dashboard-box">
            <h3>${icons.lock} Security Center</h3>
            <div class="server-status-list" style="margin-top: 0.25rem;">
              <div class="status-item">
                <span class="status-name">Console Shield</span>
                <span class="status-badge status-online">Secure</span>
              </div>
              <div class="status-item">
                <span class="status-name">SSL/TLS Mode</span>
                <span class="status-badge status-online">HTTPS</span>
              </div>
            </div>

            <h3 style="margin-top: 1.5rem;">${icons.settings} Access Statistics</h3>
            <div class="balance-preview-list" id="admins-statistics-box" style="margin-top: 0.25rem;">
              <!-- Rendered dynamically -->
              <div class="preview-item-loading">Analyzing admins...</div>
            </div>

            <h3 style="margin-top: 1.5rem;">${icons.key} Authentication Log</h3>
            <div class="telemetry-console" id="admins-console" style="height: 120px; margin-top: 0.25rem; font-size: 0.7rem;">
              <div class="console-line"><span class="console-timestamp">[SYSTEM]</span> Console shield active.</div>
            </div>
          </div>
        </div>
      </section>

      <!-- Panel: About Dev -->
      <section id="panel-about" class="page-panel ${currentTab === "about" ? "active" : ""}">
        <div class="about-hero">
          <h2 class="about-hero-title">Meet the Creators</h2>
          <p class="about-hero-subtitle">The creative minds behind the Mathmagic ecosystem, coordinating to craft the ultimate educational gaming experience.</p>
        </div>

        <div class="dev-flow-container">
          <!-- Central Connecting SVG Curve Line (Visible on Desktop) -->
          <div class="flow-svg-container">
            <svg class="flow-svg-line" viewBox="0 0 100 1200" preserveAspectRatio="none">
              <path d="M 50,0 Q 15,150 50,300 T 50,600 T 50,900 T 50,1200" fill="none" stroke="var(--color-primary)" stroke-width="2" stroke-dasharray="8 6" opacity="0.3"/>
            </svg>
          </div>

          <!-- Section 1: Rizky (PM) - Text Left, Image Right -->
          <div class="flow-section type-left">
            <div class="flow-content-wrapper">
              <div class="flow-text-block">
                <div class="dev-role">Project Manager</div>
                <h3 class="dev-name">Rizky Yonanda</h3>
                <p class="dev-bio">Manages task tracking, schedules releases, coordinates cross-functional communication, and ensures the team aligns with the game's core educational objectives.</p>
                <div class="dev-card-footer">
                  <div class="dev-links">
                    <a href="https://www.linkedin.com/in/rizkyyonanda/" target="_blank" class="dev-link-btn" title="LinkedIn">${icons.linkedin}</a>
                  </div>
                  <div class="dev-badge">Project Coordinator</div>
                </div>
              </div>
              <div class="flow-media-block">
                <div class="flow-avatar-frame">
                  <img src="${rizkyPp}" alt="Rizky Yonanda" class="flow-avatar-img">
                  <!-- Floating Badge -->
                  <div class="floating-game-badge badge-pm">
                    <span class="badge-icon">${icons.calendar}</span>
                    <span class="badge-text">Quest Started: Level 1</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Section 2: Muhamad Sidik (Dev) - Image Left, Text Right -->
          <div class="flow-section type-right">
            <div class="flow-content-wrapper">
              <div class="flow-media-block">
                <div class="flow-avatar-frame frame-highlight">
                  <img src="${sidikPp}" alt="Muhamad Sidik" class="flow-avatar-img">
                  <!-- Floating Badge -->
                  <div class="floating-game-badge badge-dev">
                    <span class="badge-icon">${icons.bolt}</span>
                    <span class="badge-text">Firebase Connected!</span>
                  </div>
                </div>
              </div>
              <div class="flow-text-block">
                <div class="dev-role">APP & Web Developer, UI</div>
                <h3 class="dev-name">Muhamad Sidik</h3>
                <p class="dev-bio">Core programmer responsible for building the game client in Unity, integrating Firebase SDKs, designing the database architecture, and constructing the administrative web dashboard.</p>
                <div class="dev-card-footer">
                  <div class="dev-links">
                    <a href="https://id.linkedin.com/in/muhamad-sidik-a6757b25b" target="_blank" class="dev-link-btn" title="LinkedIn">${icons.linkedin}</a>
                    <a href="https://github.com/MyusiZ3" target="_blank" class="dev-link-btn" title="GitHub">${icons.github}</a>
                    <a href="https://creative-portfolio-theta-rosy.vercel.app/" target="_blank" class="dev-link-btn" title="Portfolio">${icons.globe}</a>
                    <a href="mailto:muhamadsidik.imy@gmail.com" class="dev-link-btn" title="Email">${icons.mail}</a>
                  </div>
                  <div class="dev-badge">Lead Developer</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Section 3: Zahra Imani (UI) - Text Left, Image Right -->
          <div class="flow-section type-left">
            <div class="flow-content-wrapper">
              <div class="flow-text-block">
                <div class="dev-role">UI Design</div>
                <h3 class="dev-name">Zahra Imani</h3>
                <p class="dev-bio">Creates visual assets, UI layouts, icons, and menus, ensuring a consistent brand experience that keeps young players engaged.</p>
                <div class="dev-card-footer">
                  <div class="dev-links">
                    <a href="https://www.linkedin.com/in/zahraimani/" target="_blank" class="dev-link-btn" title="LinkedIn">${icons.linkedin}</a>
                  </div>
                  <div class="dev-badge">Visual Designer</div>
                </div>
              </div>
              <div class="flow-media-block">
                <div class="flow-avatar-frame">
                  <img src="${zahraPp}" alt="Zahra Imani" class="flow-avatar-img">
                  <!-- Floating Badge -->
                  <div class="floating-game-badge badge-ui">
                    <span class="badge-icon">${icons.palette}</span>
                    <span class="badge-text">UI Style Guidelines Set</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Section 4: Sheilan Mayra (QA) - Image Left, Text Right -->
          <div class="flow-section type-right">
            <div class="flow-content-wrapper">
              <div class="flow-media-block">
                <div class="flow-avatar-frame">
                  <img src="${sheilanPp}" alt="Sheilan Mayra" class="flow-avatar-img">
                  <!-- Floating Badge -->
                  <div class="floating-game-badge badge-qa">
                    <span class="badge-icon">${icons.shield}</span>
                    <span class="badge-text">0 Bugs: Build Approved</span>
                  </div>
                </div>
              </div>
              <div class="flow-text-block">
                <div class="dev-role">QA Testing</div>
                <h3 class="dev-name">Sheilan Mayra</h3>
                <p class="dev-bio">Performs comprehensive game build checks, designs bug-reporting systems, tracks telemetry issues, and optimizes user experience across multiple target devices.</p>
                <div class="dev-card-footer">
                  <div class="dev-links">
                    <a href="https://www.linkedin.com/in/sheilan-mayra-369124332/" target="_blank" class="dev-link-btn" title="LinkedIn">${icons.linkedin}</a>
                  </div>
                  <div class="dev-badge">Quality Assurance</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Section 5: Dean Erick A.N (Sound) - Text Left, Image Right -->
          <div class="flow-section type-left">
            <div class="flow-content-wrapper">
              <div class="flow-text-block">
                <div class="dev-role">Sound Designer</div>
                <h3 class="dev-name">Dean Erick A.N</h3>
                <p class="dev-bio">Crafts the auditory identity of Mathmagic, including rewarding score-unlock sound effects, immersive background music tracks, and level ambient audio.</p>
                <div class="dev-card-footer">
                  <div class="dev-links">
                    <a href="https://www.linkedin.com/in/deanerick/?locale=en" target="_blank" class="dev-link-btn" title="LinkedIn">${icons.linkedin}</a>
                  </div>
                  <div class="dev-badge">Audio Specialist</div>
                </div>
              </div>
              <div class="flow-media-block">
                <div class="flow-avatar-frame">
                  <img src="${erikPp}" alt="Dean Erick A.N" class="flow-avatar-img">
                  <!-- Floating Badge -->
                  <div class="floating-game-badge badge-sound">
                    <span class="badge-icon">${icons.volume}</span>
                    <span class="badge-text">Soundtracks Mixed 100%</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

        </div>
      </section>
    </main>

    <!-- Modal: Edit User -->
    <div id="edit-user-modal" class="modal-overlay">
      <div class="modal">
        <h3 class="modal-title">Edit User Progress</h3>
        <form id="edit-user-form">
          <div class="form-group">
            <label for="edit-name">Name</label>
            <input type="text" id="edit-name" class="form-control" required />
          </div>
          <div class="form-group">
            <label for="edit-username">Username</label>
            <input type="text" id="edit-username" class="form-control" required />
          </div>
          <div class="form-group">
            <label for="edit-score">Score</label>
            <input type="number" id="edit-score" class="form-control" min="0" required />
          </div>
          <div class="form-group">
            <label for="edit-level">Completed Level</label>
            <input type="number" id="edit-level" class="form-control" min="0" required />
          </div>
          <div class="form-group">
            <label for="edit-hp">Health (HP)</label>
            <input type="number" id="edit-hp" class="form-control" min="0" required />
          </div>
          <div class="form-group">
            <label for="edit-age">Age</label>
            <input type="number" id="edit-age" class="form-control" min="0" required />
          </div>
          
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary modal-close-btn">Cancel</button>
            <button type="submit" class="btn btn-primary">Save Changes</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal: Delete Confirmation -->
    <div id="delete-user-modal" class="modal-overlay">
      <div class="modal" style="max-width: 400px;">
        <h3 class="modal-title" style="color: var(--color-danger);">Delete User Account</h3>
        <p style="font-size: 0.95rem; color: var(--text-main); margin-bottom: 1.5rem;">
          Are you sure you want to permanently delete user <strong id="delete-username-text" style="color: var(--color-danger);"></strong>? This action is irreversible.
        </p>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary modal-close-btn">Cancel</button>
          <button type="button" id="confirm-delete-btn" class="btn btn-danger">Yes, Delete Account</button>
        </div>
      </div>
    </div>

    <!-- Modal: Add Admin -->
    <div id="add-admin-modal" class="modal-overlay">
      <div class="modal">
        <h3 class="modal-title">Register New Administrator</h3>
        <form id="add-admin-form">
          <div class="form-group">
            <label for="admin-username">Username</label>
            <input type="text" id="admin-username" class="form-control" placeholder="e.g. admin2" required autocomplete="off" />
          </div>
          <div class="form-group">
            <label for="admin-password">Password</label>
            <input type="password" id="admin-password" class="form-control" placeholder="••••••••" required autocomplete="new-password" />
          </div>
          <div class="form-group">
            <label for="admin-role">Account Role</label>
            <select id="admin-role" class="form-control" required style="background-color: var(--bg-input); color: #fff;">
              <option value="admin">Admin</option>
              <option value="superadmin">Super Admin</option>
            </select>
          </div>
          
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary modal-close-btn">Cancel</button>
            <button type="submit" class="btn btn-primary">Register Admin</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal: Delete Admin Confirmation -->
    <div id="delete-admin-modal" class="modal-overlay">
      <div class="modal" style="max-width: 400px;">
        <h3 class="modal-title" style="color: var(--color-danger);">Delete Admin Account</h3>
        <p style="font-size: 0.95rem; color: var(--text-main); margin-bottom: 1.5rem;">
          Are you sure you want to permanently delete administrator <strong id="delete-admin-username-text" style="color: var(--color-danger);"></strong>? They will immediately lose access to the console.
        </p>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary modal-close-btn">Cancel</button>
          <button type="button" id="confirm-delete-admin-btn" class="btn btn-danger">Yes, Delete Admin</button>
        </div>
      </div>
    </div>

    <!-- Modal: Purge Simulated Users Confirmation -->
    <div id="purge-simulated-modal" class="modal-overlay">
      <div class="modal" style="max-width: 400px;">
        <h3 class="modal-title" style="color: var(--color-danger);">Purge Simulated Users</h3>
        <p style="font-size: 0.95rem; color: var(--text-main); margin-bottom: 1.5rem;">
          Are you sure you want to permanently delete all <strong id="purge-simulated-count" style="color: var(--color-danger);">0</strong> simulated users? This action cannot be undone.
        </p>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary modal-close-btn">Cancel</button>
          <button type="button" id="confirm-purge-simulated-btn" class="btn btn-danger">Yes, Purge Users</button>
        </div>
      </div>
    </div>

    <!-- Modal: Purge Zero Score Users Confirmation -->
    <div id="purge-lowscore-modal" class="modal-overlay">
      <div class="modal" style="max-width: 400px;">
        <h3 class="modal-title" style="color: var(--color-danger);">Purge Zero Score Accounts</h3>
        <p style="font-size: 0.95rem; color: var(--text-main); margin-bottom: 1.5rem;">
          Are you sure you want to permanently delete all <strong id="purge-lowscore-count" style="color: var(--color-danger);">0</strong> accounts with a score of 0? This action cannot be undone.
        </p>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary modal-close-btn">Cancel</button>
          <button type="button" id="confirm-purge-lowscore-btn" class="btn btn-danger">Yes, Purge Accounts</button>
        </div>
      </div>
    </div>

    <!-- Modal: Logout Confirmation -->
    <div id="logout-confirm-modal" class="modal-overlay">
      <div class="modal" style="max-width: 400px;">
        <h3 class="modal-title" style="color: var(--color-primary);">Confirm Logout</h3>
        <p style="font-size: 0.95rem; color: var(--text-main); margin-bottom: 1.5rem;">
          Are you sure you want to log out of the Mathmagic Web Dashboard?
        </p>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary modal-close-btn">Cancel</button>
          <button type="button" id="confirm-logout-btn" class="btn btn-primary" style="background: linear-gradient(135deg, var(--color-primary), #704dff); color: #fff;">Log Out</button>
        </div>
      </div>
    </div>

    <div id="toast-container" class="toast-container"></div>
  `;

  // Attach navigation events
  document.querySelectorAll(".nav-btn").forEach((btn) => {
    btn.addEventListener("click", (e) => {
      const tab = e.currentTarget.getAttribute("data-tab");
      switchTab(tab);
    });
  });

  // Attach logout event
  document.getElementById("logout-btn").addEventListener("click", () => {
    document.getElementById("logout-confirm-modal").classList.add("active");
  });

  document
    .getElementById("confirm-logout-btn")
    .addEventListener("click", () => {
      sessionStorage.removeItem("mm_admin_logged");
      isLoggedIn = false;
      closeModals();
      renderAppStructure();
    });

  // Attach search and form events
  setupTabFunctionality();

  // Start real-time Firestore listeners
  startFirestoreListeners();
}

function switchTab(tabId) {
  currentTab = tabId;

  // Update Title text
  const titleText = {
    dashboard: "Dashboard Overview",
    users: "User Accounts Directory",
    settings: "Global App Settings",
    leaderboard: "Live Leaderboard Monitor",
    admins: "Admin Management",
    about: "About Developers",
  };
  const titleEl = document.getElementById("navbar-title-text");
  if (titleEl) {
    titleEl.innerText = titleText[tabId] || "Console";
  }

  // Toggle active button
  document.querySelectorAll(".nav-btn").forEach((btn) => {
    if (btn.getAttribute("data-tab") === tabId) {
      btn.classList.add("active");
    } else {
      btn.classList.remove("active");
    }
  });

  // Toggle active panel
  document.querySelectorAll(".page-panel").forEach((panel) => {
    if (panel.id === `panel-${tabId}`) {
      panel.classList.add("active");
    } else {
      panel.classList.remove("active");
    }
  });

  if (tabId === "dashboard") {
    updateStats();
    updateBalanceSettingsPreview();
    renderConcurrencyChart();
    renderHeatmap();
    setupTelemetryControls();
  } else if (tabId === "users") {
    renderUsersTable();
    renderUsersDistribution();
  } else if (tabId === "settings") {
    populateSettingsForm(globalSettings);
    renderSettingsMetadata();
    updateBalanceSettingsPreview();
  } else if (tabId === "leaderboard") {
    renderLeaderboard();
    renderLeaderboardSidebar();
  } else if (tabId === "admins") {
    renderAdminsTable();
    renderAdminsSidebar();
  }
}

// Start watching Firestore data
function startFirestoreListeners() {
  // Listen for settings changes
  const settingsDocRef = doc(db, "settings", "global");
  onSnapshot(settingsDocRef, (docSnap) => {
    if (docSnap.exists()) {
      globalSettings = docSnap.data();
      populateSettingsForm(globalSettings);
      updateBalanceSettingsPreview();
      renderSettingsMetadata();
      logSettingsActivity("Global configuration loaded / synced.");

      const sysStatusVal = document.getElementById("stat-sys-status");
      if (sysStatusVal) {
        let text = globalSettings.app_version || "v1.0.0";
        if (globalSettings.maintenance_mode) {
          text += " (Maint)";
        }
        sysStatusVal.innerText = text;
      }
    } else {
      // Document settings/global doesn't exist, create it with default config
      const defaultSettings = {
        max_health: 5,
        health_cooldown_seconds: 1800,
        question_timer_seconds: 30,
        main_level_score_reward: 100,
        bonus_level_score_reward: 250,
        leaderboard_limit: 50,
        maintenance_mode: false,
        leaderboard_disabled: false,
        app_version: "1.0.0",
        achievement_threshold_a: 30,
        achievement_threshold_b: 80,
        achievement_threshold_c: 150,
        achievement_threshold_d: 200,
      };
      setDoc(settingsDocRef, defaultSettings).then(() => {
        showToast("Initialized default global settings in Firestore.");
      });
    }
  });

  // Listen for users collection changes
  const usersColRef = collection(db, "users");
  onSnapshot(usersColRef, (querySnap) => {
    users = [];
    querySnap.forEach((docSnap) => {
      const data = docSnap.data();
      users.push({
        id: docSnap.id,
        ...data,
      });
    });
    updateStats();
    renderUsersTable();
    renderUsersDistribution();
    renderLeaderboard();
    renderLeaderboardSidebar();
    renderConcurrencyChart();
    renderHeatmap();
    setupTelemetryControls();
  });

  // Listen for admins collection changes
  const adminsColRef = collection(db, "admins");
  onSnapshot(adminsColRef, (querySnap) => {
    admins = [];
    querySnap.forEach((docSnap) => {
      const data = docSnap.data();
      admins.push({
        id: docSnap.id,
        ...data,
      });
    });
    renderAdminsTable();
    renderAdminsSidebar();
    logAdminActivity("Administrator directory updated.");
  });
}

function populateSettingsForm(settings) {
  if (currentTab === "settings") {
    document.getElementById("input-max-health").value =
      settings.max_health || 5;
    document.getElementById("input-health-cooldown").value =
      settings.health_cooldown_seconds || 1800;
    document.getElementById("input-timer").value =
      settings.question_timer_seconds || 30;
    document.getElementById("input-leaderboard-limit").value =
      settings.leaderboard_limit || 50;
    document.getElementById("input-main-reward").value =
      settings.main_level_score_reward || 100;
    document.getElementById("input-bonus-reward").value =
      settings.bonus_level_score_reward || 250;

    document.getElementById("input-ach-a").value =
      settings.achievement_threshold_a !== undefined
        ? settings.achievement_threshold_a
        : 30;
    document.getElementById("input-ach-b").value =
      settings.achievement_threshold_b !== undefined
        ? settings.achievement_threshold_b
        : 80;
    document.getElementById("input-ach-c").value =
      settings.achievement_threshold_c !== undefined
        ? settings.achievement_threshold_c
        : 150;
    document.getElementById("input-ach-d").value =
      settings.achievement_threshold_d !== undefined
        ? settings.achievement_threshold_d
        : 200;
  }
}

function updateStats() {
  if (!document.getElementById("stat-total-users")) return;

  const total = users.length;
  document.getElementById("stat-total-users").innerText = total;

  if (total > 0) {
    const sumScore = users.reduce(
      (acc, u) => acc + (parseInt(u.score) || 0),
      0,
    );
    const avgScore = Math.round(sumScore / total);
    document.getElementById("stat-avg-score").innerText = avgScore;

    const sumLevel = users.reduce(
      (acc, u) => acc + (parseInt(u.LEVEL) || 0),
      0,
    );
    const avgLevel = (sumLevel / total).toFixed(1);
    document.getElementById("stat-avg-level").innerText = avgLevel;

    // Top 5 Active Users dashboard box
    const sorted = [...users]
      .sort((a, b) => (b.score || 0) - (a.score || 0))
      .slice(0, 5);
    const tbody = document.getElementById("top-users-tbody");
    tbody.innerHTML = sorted
      .map(
        (u) => `
      <tr>
        <td>
          <div class="user-info-td">
            <div class="user-avatar">${(u.username || "U").charAt(0).toUpperCase()}</div>
            <div>
              <div style="font-weight: 600; color: #fff;">${u.username || "Anonymous"}</div>
              <div style="font-size: 0.75rem; color: var(--text-muted);">${u.email || "No Email"}</div>
            </div>
          </div>
        </td>
        <td style="font-weight: 700; color: var(--color-primary);">${u.score || 0}</td>
        <td>Level ${u.LEVEL || 1}</td>
      </tr>
    `,
      )
      .join("");
  } else {
    document.getElementById("stat-avg-score").innerText = "0";
    document.getElementById("stat-avg-level").innerText = "0.0";
    document.getElementById("top-users-tbody").innerHTML = `
      <tr><td colspan="3" style="text-align: center; color: var(--text-muted);">No users registered yet.</td></tr>
    `;
  }
}

function updateUsersPagination(currentPage, totalPages, totalItems) {
  const container = document.getElementById("users-pagination-container");
  if (!container) return;

  if (totalItems <= 10) {
    container.innerHTML = "";
    return;
  }

  const startIdx = (currentPage - 1) * 10;
  const endIdx = startIdx + 10;

  container.innerHTML = `
    <div style="display: flex; align-items: center; justify-content: space-between; padding: 0.75rem 1.25rem; border-top: 1px solid var(--border-color); background: rgba(255,255,255,0.01); border-bottom-left-radius: var(--radius-ios-lg); border-bottom-right-radius: var(--radius-ios-lg);">
      <div style="font-size: 0.8rem; color: var(--text-muted);">
        Showing <span style="color: var(--text-main); font-weight: 500;">${startIdx + 1}-${Math.min(endIdx, totalItems)}</span> of <span style="color: var(--text-main); font-weight: 500;">${totalItems}</span> players
      </div>
      <div style="display: flex; align-items: center; gap: 0.5rem;">
        <button class="btn btn-secondary" id="btn-users-prev" style="padding: 0.4rem 0.8rem; font-size: 0.8rem; min-height: unset; border-radius: 6px;" ${currentPage === 1 ? "disabled" : ""}>
          Previous
        </button>
        <span style="font-size: 0.8rem; color: var(--text-secondary); min-width: 80px; text-align: center;">
          Page ${currentPage} of ${totalPages}
        </span>
        <button class="btn btn-secondary" id="btn-users-next" style="padding: 0.4rem 0.8rem; font-size: 0.8rem; min-height: unset; border-radius: 6px;" ${currentPage === totalPages ? "disabled" : ""}>
          Next
        </button>
      </div>
    </div>
  `;

  // Attach event listeners
  const btnPrev = document.getElementById("btn-users-prev");
  const btnNext = document.getElementById("btn-users-next");

  if (btnPrev && currentPage > 1) {
    btnPrev.addEventListener("click", () => {
      usersCurrentPage--;
      renderUsersTable();
    });
  }

  if (btnNext && currentPage < totalPages) {
    btnNext.addEventListener("click", () => {
      usersCurrentPage++;
      renderUsersTable();
    });
  }
}

function renderUsersTable(filterText = null) {
  const tbody = document.getElementById("users-tbody");
  if (!tbody) return;

  const searchInput = document.getElementById("user-search-input");
  const queryStr = (
    filterText !== null ? filterText : searchInput ? searchInput.value : ""
  )
    .toLowerCase()
    .trim();

  const filtered = users.filter((u) => {
    const name = (u.username || "").toLowerCase();
    const mail = (u.email || "").toLowerCase();
    return name.includes(queryStr) || mail.includes(queryStr);
  });

  // Sort the filtered list
  filtered.sort((a, b) => {
    let valA, valB;
    if (usersSortKey === "username") {
      valA = (a.username || "").toLowerCase();
      valB = (b.username || "").toLowerCase();
    } else if (usersSortKey === "email") {
      valA = (a.email || "").toLowerCase();
      valB = (b.email || "").toLowerCase();
    } else if (usersSortKey === "score") {
      valA = Number(a.score) || 0;
      valB = Number(b.score) || 0;
    } else if (usersSortKey === "level") {
      valA = Number(a.LEVEL != null ? a.LEVEL : a.level != null ? a.level : 1);
      valB = Number(b.LEVEL != null ? b.LEVEL : b.level != null ? b.level : 1);
    } else {
      return 0;
    }

    if (valA < valB) return usersSortOrder === "asc" ? -1 : 1;
    if (valA > valB) return usersSortOrder === "asc" ? 1 : -1;
    return 0;
  });

  // Update sort header indicators UI
  const headers = document.querySelectorAll(".sortable-header");
  headers.forEach((h) => {
    const key = h.getAttribute("data-sort-key");
    const indicator = h.querySelector(".sort-indicator");
    if (!indicator) return;

    if (key === usersSortKey) {
      h.classList.add("active");
      if (usersSortOrder === "asc") {
        indicator.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="width: 12px; height: 12px; margin-left: 2px;"><line x1="12" y1="19" x2="12" y2="5"/><polyline points="5 12 12 5 19 12"/></svg>`;
      } else {
        indicator.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="width: 12px; height: 12px; margin-left: 2px;"><line x1="12" y1="5" x2="12" y2="19"/><polyline points="19 12 12 19 5 12"/></svg>`;
      }
    } else {
      h.classList.remove("active");
      indicator.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="width: 12px; height: 12px; opacity: 0.25; margin-left: 2px;"><polyline points="7 15 12 20 17 15"/><polyline points="7 9 12 4 17 9"/></svg>`;
    }
  });

  document.getElementById("user-count-display").innerText =
    `${filtered.length} users found`;

  const totalPages = Math.ceil(filtered.length / 10) || 1;
  if (usersCurrentPage > totalPages) {
    usersCurrentPage = totalPages;
  }
  if (usersCurrentPage < 1) {
    usersCurrentPage = 1;
  }

  const startIdx = (usersCurrentPage - 1) * 10;
  const endIdx = startIdx + 10;
  const paginated = filtered.slice(startIdx, endIdx);

  if (filtered.length === 0) {
    tbody.innerHTML = `
      <tr><td colspan="5" style="text-align: center; color: var(--text-muted);">No users match search criteria.</td></tr>
    `;
    updateUsersPagination(1, 1, 0);
    return;
  }

  tbody.innerHTML = paginated
    .map(
      (u) => `
    <tr>
      <td>
        <div class="user-info-td">
          <div class="user-avatar" style="background: linear-gradient(135deg, var(--color-blue) 0%, var(--color-violet) 100%);">
            ${(u.username || "U").charAt(0).toUpperCase()}
          </div>
          <span style="font-weight: 600;">${u.username || "Anonymous"}</span>
        </div>
      </td>
      <td style="color: var(--text-muted); font-size: 0.9rem;">${u.email || "No email registered"}</td>
      <td style="font-weight: 700; color: var(--color-primary);">${u.score || 0}</td>
      <td>Level ${u.LEVEL || 1}</td>
      <td>
        <div class="actions-cell">
          <button class="btn btn-secondary btn-icon-only edit-user-btn" data-id="${u.id}" title="Edit User">
            ${icons.edit}
          </button>
          <button class="btn btn-danger btn-icon-only delete-user-btn" data-id="${u.id}" title="Delete User">
            ${icons.delete}
          </button>
        </div>
      </td>
    </tr>
  `,
    )
    .join("");

  // Attach button actions dynamically
  tbody.querySelectorAll(".edit-user-btn").forEach((btn) => {
    btn.addEventListener("click", (e) => {
      const uId = e.currentTarget.getAttribute("data-id");
      openEditModal(uId);
    });
  });

  tbody.querySelectorAll(".delete-user-btn").forEach((btn) => {
    btn.addEventListener("click", (e) => {
      const uId = e.currentTarget.getAttribute("data-id");
      openDeleteModal(uId);
    });
  });

  updateUsersPagination(usersCurrentPage, totalPages, filtered.length);
}

function renderLeaderboard() {
  const tbody = document.getElementById("leaderboard-tbody");
  if (!tbody) return;

  const sorted = [...users].sort((a, b) => (b.score || 0) - (a.score || 0));
  const limitCount = globalSettings.leaderboard_limit || 50;
  const boardUsers = sorted.slice(0, limitCount);

  if (boardUsers.length === 0) {
    tbody.innerHTML = `
      <tr><td colspan="4" style="text-align: center; color: var(--text-muted);">No player records found.</td></tr>
    `;
    return;
  }

  tbody.innerHTML = boardUsers
    .map((u, index) => {
      const rank = index + 1;
      let rankBadgeClass = "";
      let rankText = rank;

      if (rank === 1)
        rankText = `<span class="rank-badge rank-gold">${icons.rank1}</span>`;
      else if (rank === 2)
        rankText = `<span class="rank-badge rank-silver">${icons.rank2}</span>`;
      else if (rank === 3)
        rankText = `<span class="rank-badge rank-bronze">${icons.rank3}</span>`;

      return `
      <tr>
        <td style="text-align: center; font-weight: 700; font-size: 1.1rem; color: #fff;">${rankText}</td>
        <td>
          <div class="user-info-td">
            <div class="user-avatar" style="background: ${rank === 1 ? "var(--color-primary)" : "rgba(255,255,255,0.05)"}; color: ${rank === 1 ? "var(--bg-deep)" : "#fff"}">
              ${(u.username || "U").charAt(0).toUpperCase()}
            </div>
            <div>
              <div style="font-weight: 600; color: #fff;">${u.username || "Anonymous"}</div>
              <div style="font-size: 0.75rem; color: var(--text-muted);">${u.email || "No email"}</div>
            </div>
          </div>
        </td>
        <td>Level ${u.LEVEL || 1}</td>
        <td style="text-align: right; font-weight: 800; font-size: 1.05rem; color: var(--color-primary);">${u.score || 0}</td>
      </tr>
    `;
    })
    .join("");
}

function openEditModal(userId) {
  selectedUser = users.find((u) => u.id === userId);
  if (!selectedUser) return;

  document.getElementById("edit-name").value = selectedUser.name || "";
  document.getElementById("edit-username").value = selectedUser.username || "";
  document.getElementById("edit-score").value = selectedUser.score || 0;
  document.getElementById("edit-level").value =
    selectedUser.LEVEL !== undefined ? selectedUser.LEVEL : 1;
  document.getElementById("edit-hp").value =
    selectedUser.Hp !== undefined ? selectedUser.Hp : 5;
  document.getElementById("edit-age").value =
    selectedUser.age !== undefined ? selectedUser.age : 12;

  document.getElementById("edit-user-modal").classList.add("active");
}

function openDeleteModal(userId) {
  selectedUser = users.find((u) => u.id === userId);
  if (!selectedUser) return;

  document.getElementById("delete-username-text").innerText =
    selectedUser.username || "Anonymous";
  document.getElementById("delete-user-modal").classList.add("active");
}

function openAddAdminModal() {
  const userEl = document.getElementById("admin-username");
  const passEl = document.getElementById("admin-password");
  const roleEl = document.getElementById("admin-role");
  if (userEl) userEl.value = "";
  if (passEl) passEl.value = "";
  if (roleEl) roleEl.value = "admin";

  const modal = document.getElementById("add-admin-modal");
  if (modal) modal.classList.add("active");
}

function openDeleteAdminModal(adminId) {
  selectedAdmin = admins.find((a) => a.id === adminId);
  if (!selectedAdmin) return;

  const txtEl = document.getElementById("delete-admin-username-text");
  if (txtEl) txtEl.innerText = selectedAdmin.username || "Anonymous";

  const modal = document.getElementById("delete-admin-modal");
  if (modal) modal.classList.add("active");
}

function renderAdminsTable() {
  const tbody = document.getElementById("admins-tbody");
  if (!tbody) return;

  const countDisplay = document.getElementById("admin-count-display");
  if (countDisplay) {
    countDisplay.innerText = `${admins.length} administrators registered`;
  }

  if (admins.length === 0) {
    tbody.innerHTML = `
      <tr><td colspan="${loggedInRole === "superadmin" ? "4" : "3"}" style="text-align: center; color: var(--text-muted);">No admin accounts found.</td></tr>
    `;
    return;
  }

  tbody.innerHTML = admins
    .map((admin) => {
      const isCurrentUser = admin.username === loggedInUsername;
      const canDelete = loggedInRole === "superadmin" && !isCurrentUser;

      let actionBtn = "";
      if (canDelete) {
        actionBtn = `
        <button class="btn btn-danger btn-icon-only delete-admin-btn" data-id="${admin.id}" title="Delete Admin">
          ${icons.delete}
        </button>
      `;
      } else if (isCurrentUser) {
        actionBtn = `<span style="font-size: 0.85rem; color: var(--text-muted); font-style: italic;">You</span>`;
      }

      let createdAtText = "-";
      if (admin.createdAt) {
        if (typeof admin.createdAt.toDate === "function") {
          createdAtText = admin.createdAt.toDate().toLocaleString();
        } else if (admin.createdAt.seconds) {
          createdAtText = new Date(
            admin.createdAt.seconds * 1000,
          ).toLocaleString();
        } else {
          createdAtText = new Date(admin.createdAt).toLocaleString();
        }
      }

      return `
      <tr>
        <td>
          <div class="user-info-td">
            <div class="user-avatar" style="background: linear-gradient(135deg, var(--color-violet) 0%, var(--color-primary) 100%);">
              ${(admin.username || "A").charAt(0).toUpperCase()}
            </div>
            <span style="font-weight: 600;">${admin.username}</span>
          </div>
        </td>
        <td>
          <span style="display: inline-block; padding: 0.25rem 0.5rem; border-radius: 6px; font-size: 0.75rem; font-weight: 600; text-transform: capitalize; background: ${admin.role === "superadmin" ? "rgba(167, 139, 250, 0.15)" : "rgba(255, 255, 255, 0.05)"}; color: ${admin.role === "superadmin" ? "var(--color-primary)" : "var(--text-muted)"}; border: 1px solid ${admin.role === "superadmin" ? "rgba(167, 139, 250, 0.3)" : "rgba(255,255,255,0.1)"};">
            ${admin.role}
          </span>
        </td>
        <td style="color: var(--text-muted); font-size: 0.9rem;">${createdAtText}</td>
        ${
          loggedInRole === "superadmin"
            ? `
        <td style="text-align: right;">
          <div class="actions-cell" style="justify-content: flex-end;">
            ${actionBtn}
          </div>
        </td>
        `
            : ""
        }
      </tr>
    `;
    })
    .join("");

  // Attach delete buttons events
  tbody.querySelectorAll(".delete-admin-btn").forEach((btn) => {
    btn.addEventListener("click", (e) => {
      const adminId = e.currentTarget.getAttribute("data-id");
      openDeleteAdminModal(adminId);
    });
  });
}

function closeModals() {
  document.querySelectorAll(".modal-overlay").forEach((modal) => {
    modal.classList.remove("active");
  });
  selectedUser = null;
  selectedAdmin = null;
}

function renderBalanceItems(fields) {
  return fields
    .map((f) => {
      let valClass = "";
      if (f.isStatus)
        valClass = f.statusVal
          ? "status-val status-on"
          : "status-val status-off";
      return (
        '<div class="balance-preview-item"><span class="preview-label">' +
        f.label +
        '</span><span class="preview-value ' +
        valClass +
        '">' +
        f.value +
        "</span></div>"
      );
    })
    .join("");
}

function updateBalanceSettingsPreview() {
  const cgp = document.getElementById("balance-settings-preview-gameplay");
  const cach = document.getElementById("balance-settings-preview-achievements");
  const cst = document.getElementById("balance-settings-preview-status");
  const loading = '<div class="preview-item-loading">No data available.</div>';

  if (!globalSettings) {
    if (cgp) cgp.innerHTML = loading;
    if (cach) cach.innerHTML = loading;
    if (cst) cst.innerHTML = loading;
    return;
  }

  const s = globalSettings;

  if (cgp)
    cgp.innerHTML = renderBalanceItems([
      { label: "Max Health Pool", value: (s.max_health || 5) + " HP" },
      {
        label: "Health Cooldown",
        value: ((s.health_cooldown_seconds || 1800) / 60).toFixed(0) + " min",
      },
      {
        label: "Question Timer",
        value: (s.question_timer_seconds || 30) + " sec",
      },
      {
        label: "Main Level Reward",
        value: "+" + (s.main_level_score_reward || 100) + " pts",
      },
      {
        label: "Bonus Level Reward",
        value: "+" + (s.bonus_level_score_reward || 250) + " pts",
      },
    ]);

  if (cach)
    cach.innerHTML = renderBalanceItems([
      {
        label: "Achievement A",
        value:
          (s.achievement_threshold_a != null ? s.achievement_threshold_a : 30) +
          " pts",
      },
      {
        label: "Achievement B",
        value:
          (s.achievement_threshold_b != null ? s.achievement_threshold_b : 80) +
          " pts",
      },
      {
        label: "Achievement C",
        value:
          (s.achievement_threshold_c != null
            ? s.achievement_threshold_c
            : 150) + " pts",
      },
      {
        label: "Achievement D",
        value:
          (s.achievement_threshold_d != null
            ? s.achievement_threshold_d
            : 200) + " pts",
      },
    ]);

  if (cst)
    cst.innerHTML = renderBalanceItems([
      { label: "App Version", value: s.app_version || "1.0.0" },
      {
        label: "Maintenance Mode",
        value: s.maintenance_mode ? "Active" : "Disabled",
        isStatus: true,
        statusVal: s.maintenance_mode,
      },
      {
        label: "Leaderboard",
        value: s.leaderboard_disabled ? "Frozen" : "Live",
        isStatus: true,
        statusVal: s.leaderboard_disabled,
      },
    ]);
}

function getDeterministicHash(str) {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  return Math.abs(hash);
}

function renderConcurrencyChart() {
  const container = document.getElementById("concurrency-chart-container");
  if (!container) return;

  const N = users.length;
  let points = [];
  let labels = [];
  const now = Date.now();

  if (concurrencyRange === "daily") {
    // 24 slots: Hour 0 to 23
    points = Array(24).fill(0);
    labels = Array.from(
      { length: 24 },
      (_, h) => `${String(h).padStart(2, "0")}:00`,
    );

    users.forEach((u) => {
      if (u.LastHpUpdateTime) {
        const timeDiffMs = now - u.LastHpUpdateTime * 1000;
        if (timeDiffMs >= 0 && timeDiffMs < 24 * 60 * 60 * 1000) {
          const hour = new Date(u.LastHpUpdateTime * 1000).getHours();
          points[hour]++;
          return;
        }
      }
      // Fallback distribution
      const hash = getDeterministicHash(u.username || u.id || "anonymous");
      const hour = hash % 24;
      points[hour]++;
    });
  } else if (concurrencyRange === "weekly") {
    // 7 slots: Mon to Sun
    points = Array(7).fill(0);
    const days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
    labels = days;

    users.forEach((u) => {
      if (u.LastHpUpdateTime) {
        const timeDiffMs = now - u.LastHpUpdateTime * 1000;
        if (timeDiffMs >= 0 && timeDiffMs < 7 * 24 * 60 * 60 * 1000) {
          const day = new Date(u.LastHpUpdateTime * 1000).getDay(); // 0 = Sun, 1 = Mon...
          const adjustedDayIndex = day === 0 ? 6 : day - 1; // Map Sun to 6, Mon to 0
          points[adjustedDayIndex]++;
          return;
        }
      }
      // Fallback distribution
      const hash = getDeterministicHash(u.username || u.id || "anonymous");
      const day = hash % 7;
      points[day]++;
    });
  } else {
    // Monthly (30 slots: D1 to D30)
    points = Array(30).fill(0);
    labels = Array.from({ length: 30 }, (_, d) => `D${d + 1}`);

    users.forEach((u) => {
      if (u.LastHpUpdateTime) {
        const timeDiffMs = now - u.LastHpUpdateTime * 1000;
        if (timeDiffMs >= 0 && timeDiffMs < 30 * 24 * 60 * 60 * 1000) {
          const dayOfMonth = new Date(u.LastHpUpdateTime * 1000).getDate(); // 1-31
          const idx = Math.min(dayOfMonth - 1, 29);
          points[idx]++;
          return;
        }
      }
      // Fallback distribution
      const hash = getDeterministicHash(u.username || u.id || "anonymous");
      const day = hash % 30;
      points[day]++;
    });
  }

  const maxVal = Math.max(...points, 1);
  const K = points.length;

  let pathD = "";
  let fillD = "";
  let circleHtml = "";

  const xMin = 15;
  const xMax = 485;
  const yMin = 20;
  const yMax = 110;

  for (let i = 0; i < K; i++) {
    const x = xMin + (i / (K - 1)) * (xMax - xMin);
    const y = yMax - (points[i] / maxVal) * (yMax - yMin);

    if (i === 0) {
      pathD = `M ${x},${y}`;
      fillD = `M ${x},${yMax} L ${x},${y}`;
    } else {
      if (K <= 7) {
        const prevX = xMin + ((i - 1) / (K - 1)) * (xMax - xMin);
        const prevY = yMax - (points[i - 1] / maxVal) * (yMax - yMin);
        const cpX = (prevX + x) / 2;
        pathD += ` C ${cpX},${prevY} ${cpX},${y} ${x},${y}`;
      } else {
        pathD += ` L ${x},${y}`;
      }
    }
    fillD += ` L ${x},${y}`;

    circleHtml += `
      <circle class="chart-point" cx="${x}" cy="${y}" r="4" fill="var(--bg-deep)" stroke="var(--color-primary)" stroke-width="2">
        <title>${labels[i]} - Peak Players: ${points[i]}</title>
      </circle>
    `;
  }

  const lastX = xMin + (K - 1) * ((xMax - xMin) / (K - 1));
  fillD += ` L ${lastX},${yMax} Z`;

  let xLabelsHtml = "";
  const labelStep = Math.max(1, Math.floor(K / 5));
  for (let i = 0; i < K; i += labelStep) {
    xLabelsHtml += `<span>${labels[i]}</span>`;
  }
  if ((K - 1) % labelStep !== 0) {
    xLabelsHtml += `<span>${labels[K - 1]}</span>`;
  }

  container.innerHTML = `
    <div class="chart-container">
      <svg viewBox="0 0 500 130" class="trend-chart-svg">
        <defs>
          <linearGradient id="dyn-chart-grad" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stop-color="var(--color-primary)" stop-opacity="0.35"></stop>
            <stop offset="100%" stop-color="var(--color-primary)" stop-opacity="0.0"></stop>
          </linearGradient>
        </defs>
        
        <line x1="10" y1="20" x2="490" y2="20" stroke="rgba(255,255,255,0.03)" stroke-dasharray="3"></line>
        <line x1="10" y1="65" x2="490" y2="65" stroke="rgba(255,255,255,0.03)" stroke-dasharray="3"></line>
        <line x1="10" y1="110" x2="490" y2="110" stroke="rgba(255,255,255,0.05)"></line>

        <path d="${fillD}" fill="url(#dyn-chart-grad)"></path>
        <path d="${pathD}" fill="none" stroke="var(--color-primary)" stroke-width="2.5" stroke-linecap="round"></path>
        ${circleHtml}
      </svg>
    </div>
    <div class="chart-labels">
      ${xLabelsHtml}
    </div>
  `;
}

function renderHeatmap() {
  const container = document.getElementById("heatmap-chart-container");
  if (!container) return;

  const N = users.length;
  let html = "";
  const now = Date.now();

  if (heatmapRange === "daily") {
    html = `
      <div class="heatmap-container">
        <div class="heatmap-days" style="height: 48px;">
          <span>AM</span>
          <span>PM</span>
        </div>
        <div class="heatmap-grid" style="grid-template-rows: repeat(2, 1fr); grid-template-columns: repeat(12, 1fr); height: 48px;">
    `;

    let cellCounts = Array(24).fill(0);
    users.forEach((u) => {
      if (u.LastHpUpdateTime) {
        const timeDiffMs = now - u.LastHpUpdateTime * 1000;
        if (timeDiffMs >= 0 && timeDiffMs < 24 * 60 * 60 * 1000) {
          const hour = new Date(u.LastHpUpdateTime * 1000).getHours();
          cellCounts[hour]++;
          return;
        }
      }
      const hash = getDeterministicHash(u.username || u.id || "anonymous");
      const hour = hash % 24;
      cellCounts[hour]++;
    });

    for (let row = 0; row < 2; row++) {
      for (let col = 0; col < 12; col++) {
        const hour = row * 12 + col;
        const count = cellCounts[hour];
        let opacity = 0.08;
        if (count > 0 && N > 0) {
          opacity = Math.min(0.95, 0.15 + (count / N) * 0.8);
        }

        const actPct = Math.round(opacity * 100);
        html += `<div class="heatmap-cell" style="opacity: ${opacity};" title="${String(hour).padStart(2, "0")}:00 - Activity: ${actPct}% (${count} players)"></div>`;
      }
    }

    html += `
        </div>
      </div>
    `;
  } else if (heatmapRange === "weekly") {
    html = `
      <div class="heatmap-container">
        <div class="heatmap-days" style="height: 110px;">
          <span>Mon</span>
          <span>Wed</span>
          <span>Fri</span>
          <span>Sun</span>
        </div>
        <div class="heatmap-grid" style="grid-template-rows: repeat(7, 1fr); grid-template-columns: repeat(18, 1fr); height: 110px;">
    `;

    let cellCounts = Array(7)
      .fill()
      .map(() => Array(18).fill(0));
    users.forEach((u) => {
      let dayIdx = 0;
      let periodIdx = 0;
      if (u.LastHpUpdateTime) {
        const timeDiffMs = now - u.LastHpUpdateTime * 1000;
        if (timeDiffMs >= 0 && timeDiffMs < 7 * 24 * 60 * 60 * 1000) {
          const day = new Date(u.LastHpUpdateTime * 1000).getDay();
          dayIdx = day === 0 ? 6 : day - 1;
          const hour = new Date(u.LastHpUpdateTime * 1000).getHours();
          periodIdx = Math.floor((hour / 24) * 18);
          cellCounts[dayIdx][periodIdx]++;
          return;
        }
      }
      const hash = getDeterministicHash(u.username || u.id || "anonymous");
      dayIdx = hash % 7;
      periodIdx = (hash + 3) % 18;
      cellCounts[dayIdx][periodIdx]++;
    });

    const days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
    for (let day = 0; day < 7; day++) {
      for (let col = 0; col < 18; col++) {
        const count = cellCounts[day][col];
        let opacity = 0.08;
        if (count > 0 && N > 0) {
          opacity = Math.min(0.95, 0.15 + (count / N) * 0.8);
        }

        const actPct = Math.round(opacity * 100);
        html += `<div class="heatmap-cell" style="opacity: ${opacity};" title="${days[day]}, Period ${col + 1} - Activity: ${actPct}% (${count} players)"></div>`;
      }
    }

    html += `
        </div>
      </div>
    `;
  } else {
    html = `
      <div class="heatmap-container">
        <div class="heatmap-days" style="height: 90px;">
          <span>Wk 1</span>
          <span>Wk 3</span>
          <span>Wk 5</span>
        </div>
        <div class="heatmap-grid" style="grid-template-rows: repeat(5, 1fr); grid-template-columns: repeat(7, 1fr); height: 90px; max-width: 280px;">
    `;

    let cellCounts = Array(30).fill(0);
    users.forEach((u) => {
      let dayOfMonthIdx = 0;
      if (u.LastHpUpdateTime) {
        const timeDiffMs = now - u.LastHpUpdateTime * 1000;
        if (timeDiffMs >= 0 && timeDiffMs < 30 * 24 * 60 * 60 * 1000) {
          const dayOfMonth = new Date(u.LastHpUpdateTime * 1000).getDate();
          dayOfMonthIdx = Math.min(dayOfMonth - 1, 29);
          cellCounts[dayOfMonthIdx]++;
          return;
        }
      }
      const hash = getDeterministicHash(u.username || u.id || "anonymous");
      dayOfMonthIdx = hash % 30;
      cellCounts[dayOfMonthIdx]++;
    });

    for (let wk = 0; wk < 5; wk++) {
      for (let d = 0; d < 7; d++) {
        const dayOfMonth = wk * 7 + d + 1;
        let opacity = 0.08;
        let count = 0;
        if (dayOfMonth <= 30) {
          count = cellCounts[dayOfMonth - 1];
          if (count > 0 && N > 0) {
            opacity = Math.min(0.95, 0.15 + (count / N) * 0.8);
          }
        } else {
          opacity = 0;
        }

        const actPct = Math.round(opacity * 100);
        const titleStr =
          dayOfMonth <= 30
            ? `Day ${dayOfMonth} - Activity: ${actPct}% (${count} players)`
            : "";
        html += `<div class="heatmap-cell" style="opacity: ${opacity}; cursor: ${opacity > 0 ? "pointer" : "default"};" title="${titleStr}"></div>`;
      }
    }

    html += `
        </div>
      </div>
    `;
  }

  container.innerHTML = html;
}

function setupTelemetryControls() {
  const ccRange = document.getElementById("concurrency-range-control");
  if (ccRange) {
    ccRange.querySelectorAll("button").forEach((btn) => {
      btn.addEventListener("click", (e) => {
        ccRange
          .querySelectorAll("button")
          .forEach((b) => b.classList.remove("active"));
        e.target.classList.add("active");
        concurrencyRange = e.target.getAttribute("data-range");
        renderConcurrencyChart();
      });
    });
  }

  const hmRange = document.getElementById("heatmap-range-control");
  if (hmRange) {
    hmRange.querySelectorAll("button").forEach((btn) => {
      btn.addEventListener("click", (e) => {
        hmRange
          .querySelectorAll("button")
          .forEach((b) => b.classList.remove("active"));
        e.target.classList.add("active");
        heatmapRange = e.target.getAttribute("data-range");
        renderHeatmap();
      });
    });
  }
}

function setupTabFunctionality() {
  // Search bar functionality
  const searchInput = document.getElementById("user-search-input");
  if (searchInput) {
    searchInput.addEventListener("input", (e) => {
      usersCurrentPage = 1;
      renderUsersTable(e.target.value);
    });
  }

  document.querySelectorAll(".sortable-header").forEach((header) => {
    header.addEventListener("click", (e) => {
      const key = e.currentTarget.getAttribute("data-sort-key");
      if (usersSortKey === key) {
        usersSortOrder = usersSortOrder === "asc" ? "desc" : "asc";
      } else {
        usersSortKey = key;
        usersSortOrder = "asc";
      }
      localStorage.setItem("mm_users_sort_key", usersSortKey);
      localStorage.setItem("mm_users_sort_order", usersSortOrder);
      usersCurrentPage = 1;
      renderUsersTable();
    });
  });

  // Edit user form submission
  const editForm = document.getElementById("edit-user-form");
  if (editForm) {
    editForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      if (!selectedUser) return;

      const nameVal = document.getElementById("edit-name").value.trim();
      const usernameVal = document.getElementById("edit-username").value.trim();
      const scoreVal = parseInt(document.getElementById("edit-score").value);
      const levelVal = parseInt(document.getElementById("edit-level").value);
      const hpVal = parseInt(document.getElementById("edit-hp").value);
      const ageVal = parseInt(document.getElementById("edit-age").value);

      try {
        const userDocRef = doc(db, "users", selectedUser.id);
        const updateData = {
          name: nameVal,
          username: usernameVal,
          score: scoreVal,
          LEVEL: levelVal,
          Hp: hpVal,
          age: ageVal,
        };
        if (selectedUser.Hp !== hpVal) {
          updateData.LastHpUpdateTime = Math.floor(Date.now() / 1000);
        }
        await updateDoc(userDocRef, updateData);
        showToast(`Successfully updated credentials for ${usernameVal}`);
        closeModals();
      } catch (err) {
        showToast(`Error updating user: ${err.message}`, "error");
      }
    });
  }

  // Delete user confirm
  const confirmDeleteBtn = document.getElementById("confirm-delete-btn");
  if (confirmDeleteBtn) {
    confirmDeleteBtn.addEventListener("click", async () => {
      if (!selectedUser) return;

      try {
        const userDocRef = doc(db, "users", selectedUser.id);
        await deleteDoc(userDocRef);
        showToast(
          `User ${selectedUser.username} has been permanently deleted.`,
        );
        closeModals();
      } catch (err) {
        showToast(`Error deleting user: ${err.message}`, "error");
      }
    });
  }

  // Modal closes
  document.querySelectorAll(".modal-close-btn").forEach((btn) => {
    btn.addEventListener("click", closeModals);
  });

  // === Per-section Settings Handlers ===
  const settingsRef = () => doc(db, "settings", "global");

  // Helper: log to settings console
  function logToSettingsConsole(msg) {
    const el = document.getElementById("settings-console");
    if (!el) return;
    const time = new Date().toLocaleTimeString();
    const line = document.createElement("div");
    line.className = "console-line";
    line.innerHTML = `<span class="console-timestamp">[${time}]</span> ${msg}`;
    el.appendChild(line);
    el.scrollTop = el.scrollHeight;
  }

  // SAVE: Game Balance
  const btnSaveBalance = document.getElementById("btn-save-balance");
  if (btnSaveBalance) {
    btnSaveBalance.addEventListener("click", async () => {
      try {
        btnSaveBalance.disabled = true;
        btnSaveBalance.textContent = "Saving...";
        await updateDoc(settingsRef(), {
          max_health:
            parseInt(document.getElementById("input-max-health").value) || 5,
          health_cooldown_seconds:
            parseInt(document.getElementById("input-health-cooldown").value) ||
            1800,
          question_timer_seconds:
            parseInt(document.getElementById("input-timer").value) || 30,
          leaderboard_limit:
            parseInt(
              document.getElementById("input-leaderboard-limit").value,
            ) || 50,
          main_level_score_reward:
            parseInt(document.getElementById("input-main-reward").value) || 100,
          bonus_level_score_reward:
            parseInt(document.getElementById("input-bonus-reward").value) ||
            250,
        });
        showToast("Game Balance saved!");
        logToSettingsConsole("Game Balance updated successfully.");
      } catch (err) {
        showToast(`Failed: ${err.message}`, "error");
      } finally {
        btnSaveBalance.disabled = false;
        btnSaveBalance.textContent = "Save Balance";
      }
    });
  }

  // RESET: Game Balance
  const btnResetBalance = document.getElementById("btn-reset-balance");
  if (btnResetBalance) {
    btnResetBalance.addEventListener("click", () => {
      document.getElementById("input-max-health").value = 5;
      document.getElementById("input-health-cooldown").value = 1800;
      document.getElementById("input-timer").value = 30;
      document.getElementById("input-leaderboard-limit").value = 50;
      document.getElementById("input-main-reward").value = 100;
      document.getElementById("input-bonus-reward").value = 250;
      showToast("Balance reset to defaults. Click Save to publish.", "info");
    });
  }

  // SAVE: Achievement Thresholds
  const btnSaveAch = document.getElementById("btn-save-achievements");
  if (btnSaveAch) {
    btnSaveAch.addEventListener("click", async () => {
      try {
        btnSaveAch.disabled = true;
        btnSaveAch.textContent = "Saving...";
        await updateDoc(settingsRef(), {
          achievement_threshold_a:
            parseInt(document.getElementById("input-ach-a").value) || 0,
          achievement_threshold_b:
            parseInt(document.getElementById("input-ach-b").value) || 0,
          achievement_threshold_c:
            parseInt(document.getElementById("input-ach-c").value) || 0,
          achievement_threshold_d:
            parseInt(document.getElementById("input-ach-d").value) || 0,
        });
        showToast("Achievement Thresholds saved!");
        logToSettingsConsole("Achievement thresholds updated.");
      } catch (err) {
        showToast(`Failed: ${err.message}`, "error");
      } finally {
        btnSaveAch.disabled = false;
        btnSaveAch.textContent = "Save Thresholds";
      }
    });
  }

  // Add Admin modal triggering
  const addAdminBtn = document.getElementById("add-admin-btn");
  if (addAdminBtn) {
    addAdminBtn.addEventListener("click", openAddAdminModal);
  }

  // Add Admin form submission
  const addAdminForm = document.getElementById("add-admin-form");
  if (addAdminForm) {
    addAdminForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      const usernameVal = document
        .getElementById("admin-username")
        .value.trim();
      const passwordVal = document.getElementById("admin-password").value;
      const roleVal = document.getElementById("admin-role").value;

      if (!usernameVal || !passwordVal) {
        showToast("Please fill in all fields.", "error");
        return;
      }

      try {
        const adminDocRef = doc(db, "admins", usernameVal);
        const adminDoc = await getDoc(adminDocRef);
        if (adminDoc.exists()) {
          showToast(`Admin username "${usernameVal}" already exists.`, "error");
          return;
        }

        await setDoc(adminDocRef, {
          username: usernameVal,
          password: passwordVal,
          role: roleVal,
          createdAt: new Date().toISOString(),
        });

        showToast(`Administrator ${usernameVal} registered successfully!`);
        closeModals();
      } catch (err) {
        showToast(`Failed to register admin: ${err.message}`, "error");
      }
    });
  }

  // Confirm delete admin
  const confirmDeleteAdminBtn = document.getElementById(
    "confirm-delete-admin-btn",
  );
  if (confirmDeleteAdminBtn) {
    confirmDeleteAdminBtn.addEventListener("click", async () => {
      if (!selectedAdmin) return;

      try {
        const adminDocRef = doc(db, "admins", selectedAdmin.id);
        await deleteDoc(adminDocRef);
        showToast(
          `Admin ${selectedAdmin.username} has been successfully deleted.`,
        );
        closeModals();
      } catch (err) {
        showToast(`Failed to delete admin: ${err.message}`, "error");
      }
    });
  }

  // User Simulation button
  const btnSimulate = document.getElementById("btn-simulate-user");
  if (btnSimulate) {
    btnSimulate.addEventListener("click", async () => {
      try {
        const inputSimulateName = document.getElementById(
          "input-simulate-name",
        );
        const nameValue = inputSimulateName
          ? inputSimulateName.value.trim()
          : "";

        const inputSimulateUsername = document.getElementById(
          "input-simulate-username",
        );
        const usernameValue = inputSimulateUsername
          ? inputSimulateUsername.value.trim()
          : "";

        const inputSimulateAge = document.getElementById("input-simulate-age");
        const ageValue = inputSimulateAge
          ? parseInt(inputSimulateAge.value.trim())
          : NaN;

        let finalName = "";
        let finalUsername = "";

        if (nameValue) {
          finalName = nameValue;
        } else {
          const names = [
            "Ahmad",
            "Budi",
            "Chandra",
            "Dewi",
            "Eko",
            "Fitri",
            "Gita",
            "Hadi",
            "Indah",
            "Joko",
            "Kartika",
            "Lani",
            "Mawan",
            "Ningsih",
            "Oki",
            "Putra",
            "Rini",
            "Siti",
            "Tono",
            "Utami",
            "Wawan",
            "Yanti",
          ];
          const baseName = names[Math.floor(Math.random() * names.length)];
          finalName = baseName;
        }

        if (usernameValue) {
          finalUsername = usernameValue;
        } else {
          // Create username by stripping non-alphanumeric characters, then append random suffix
          const baseUser = finalName.replace(/[^a-zA-Z0-9]/g, "");
          finalUsername = baseUser
            ? baseUser + Math.floor(Math.random() * 900 + 100)
            : "user" + Math.floor(Math.random() * 9000 + 1000);
        }

        const randomEmail = `${finalUsername.toLowerCase()}@mathmagic.com`;
        const randomScore = Math.floor(Math.random() * 25000);
        const randomLevel = Math.floor(randomScore / 800) + 1;
        const randomHp = Math.floor(Math.random() * 5) + 1;
        const finalAge = !isNaN(ageValue)
          ? ageValue
          : Math.floor(Math.random() * 10) + 8;

        await addDoc(collection(db, "users"), {
          name: finalName,
          username: finalUsername,
          email: randomEmail,
          score: randomScore,
          LEVEL: randomLevel,
          Hp: randomHp,
          age: finalAge,
          isSimulated: true,
        });

        if (inputSimulateName) inputSimulateName.value = "";
        if (inputSimulateUsername) inputSimulateUsername.value = "";
        if (inputSimulateAge) inputSimulateAge.value = "";

        showToast(`Simulated user "${finalName}" (${finalUsername}) added!`);
      } catch (err) {
        showToast(`Failed to simulate user: ${err.message}`, "error");
      }
    });
  }

  // Purge Simulated Users button
  const btnPurgeSimulated = document.getElementById("btn-purge-simulated");
  if (btnPurgeSimulated) {
    btnPurgeSimulated.addEventListener("click", () => {
      const simulatedUsers = users.filter(
        (u) =>
          u.isSimulated === true ||
          (u.email &&
            (u.email.toLowerCase().endsWith("@mathmagic.com") ||
              u.email.toLowerCase().endsWith("mathmagic.com"))),
      );
      if (simulatedUsers.length === 0) {
        showToast("No simulated user accounts found.", "info");
        return;
      }
      const countEl = document.getElementById("purge-simulated-count");
      if (countEl) countEl.innerText = simulatedUsers.length;

      const modal = document.getElementById("purge-simulated-modal");
      if (modal) modal.classList.add("active");
    });
  }

  // Confirm Purge Simulated Users
  const confirmPurgeSimulatedBtn = document.getElementById(
    "confirm-purge-simulated-btn",
  );
  if (confirmPurgeSimulatedBtn) {
    confirmPurgeSimulatedBtn.addEventListener("click", async () => {
      const simulatedUsers = users.filter(
        (u) =>
          u.isSimulated === true ||
          (u.email &&
            (u.email.toLowerCase().endsWith("@mathmagic.com") ||
              u.email.toLowerCase().endsWith("mathmagic.com"))),
      );
      if (simulatedUsers.length === 0) {
        closeModals();
        return;
      }

      confirmPurgeSimulatedBtn.disabled = true;
      confirmPurgeSimulatedBtn.innerText = "Purging...";

      let count = 0;
      for (const u of simulatedUsers) {
        try {
          await deleteDoc(doc(db, "users", u.id));
          count++;
        } catch (e) {
          console.error(e);
        }
      }

      confirmPurgeSimulatedBtn.disabled = false;
      confirmPurgeSimulatedBtn.innerText = "Yes, Purge Users";

      showToast(`Successfully deleted ${count} simulated accounts.`);
      closeModals();
    });
  }

  // Purge Zero Score Users button
  const btnPurge = document.getElementById("btn-purge-lowscore");
  if (btnPurge) {
    btnPurge.addEventListener("click", () => {
      const zeroUsers = users.filter((u) => (u.score || 0) === 0);
      if (zeroUsers.length === 0) {
        showToast("No user accounts with a score of 0 found.", "info");
        return;
      }
      const countEl = document.getElementById("purge-lowscore-count");
      if (countEl) countEl.innerText = zeroUsers.length;

      const modal = document.getElementById("purge-lowscore-modal");
      if (modal) modal.classList.add("active");
    });
  }

  // Confirm Purge Zero Score Users
  const confirmPurgeLowscoreBtn = document.getElementById(
    "confirm-purge-lowscore-btn",
  );
  if (confirmPurgeLowscoreBtn) {
    confirmPurgeLowscoreBtn.addEventListener("click", async () => {
      const zeroUsers = users.filter((u) => (u.score || 0) === 0);
      if (zeroUsers.length === 0) {
        closeModals();
        return;
      }

      confirmPurgeLowscoreBtn.disabled = true;
      confirmPurgeLowscoreBtn.innerText = "Purging...";

      let count = 0;
      for (const u of zeroUsers) {
        try {
          await deleteDoc(doc(db, "users", u.id));
          count++;
        } catch (e) {
          console.error(e);
        }
      }

      confirmPurgeLowscoreBtn.disabled = false;
      confirmPurgeLowscoreBtn.innerText = "Yes, Purge Accounts";

      showToast(`Successfully purged ${count} zero-score accounts.`);
      closeModals();
    });
  }
}

// Render distribution list in Users panel side bento card
function renderUsersDistribution() {
  const container = document.getElementById("users-distribution-metrics");
  if (!container) return;
  if (!users || users.length === 0) {
    container.innerHTML =
      '<div style="color: var(--text-muted); text-align: center; font-size: 0.85rem; padding: 1rem 0;">No user data to analyze.</div>';
    return;
  }

  // Calculate bracket sizes
  let bronzeCount = 0; // score < 1000
  let silverCount = 0; // score 1000-5000
  let goldCount = 0; // score 5000-15000
  let diamondCount = 0; // score > 15000

  users.forEach((u) => {
    const s = u.score || 0;
    if (s < 1000) bronzeCount++;
    else if (s <= 5000) silverCount++;
    else if (s <= 15000) goldCount++;
    else diamondCount++;
  });

  const total = users.length;
  const groups = [
    {
      name: "Bronze (Score < 1k)",
      count: bronzeCount,
      color: "var(--color-danger)",
    },
    {
      name: "Silver (1k - 5k)",
      count: silverCount,
      color: "var(--color-orange)",
    },
    { name: "Gold (5k - 15k)", count: goldCount, color: "var(--color-blue)" },
    {
      name: "Diamond (Score > 15k)",
      count: diamondCount,
      color: "var(--color-green)",
    },
  ];

  let html = "";
  groups.forEach((g) => {
    const pct = total > 0 ? Math.round((g.count / total) * 100) : 0;
    html += `
      <div style="margin-bottom: 0.5rem;">
        <div style="display: flex; justify-content: space-between; font-size: 0.8rem; margin-bottom: 0.2rem;">
          <span style="color: var(--text-secondary);">${g.name}</span>
          <span style="color: var(--text-main); font-weight: 600;">${g.count} (${pct}%)</span>
        </div>
        <div style="background: rgba(255,255,255,0.05); height: 6px; border-radius: var(--radius-pill); overflow: hidden;">
          <div style="background: ${g.color}; height: 100%; width: ${pct}%; border-radius: var(--radius-pill);"></div>
        </div>
      </div>
    `;
  });

  container.innerHTML = html;
}

// Render dynamic Hall of Fame and Tier breakdown in Leaderboard panel
function renderLeaderboardSidebar() {
  // 1. Top player highlight
  const highlightEl = document.getElementById("top-player-highlight");
  if (highlightEl) {
    if (!users || users.length === 0) {
      highlightEl.innerHTML =
        '<div style="color: var(--text-muted); font-size: 0.85rem;">No players.</div>';
    } else {
      // Find player with highest score
      const sorted = [...users].sort((a, b) => (b.score || 0) - (a.score || 0));
      const topPlayer = sorted[0];
      highlightEl.innerHTML = `
        <div style="display: flex; align-items: center; justify-content: center; width: 52px; height: 52px; border-radius: 50%; background: linear-gradient(135deg, rgba(255,215,0,0.18) 0%, rgba(181,155,235,0.18) 100%); margin-bottom: 0.25rem;">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="#ffe082" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="width:28px;height:28px;"><path d="M2 19h20M2 19l2-9 5 4 3-7 3 7 5-4 2 9"/><circle cx="12" cy="5" r="1" fill="#ffe082"/></svg>
        </div>
        <div style="font-family: var(--font-title); font-weight: 700; color: #fff; font-size: 1.15rem;">
          ${topPlayer.username || "Anonymous"}
        </div>
        <div style="font-size: 0.8rem; color: var(--text-secondary); margin-bottom: 0.75rem;">
          ${topPlayer.email || "no-email@mathmagic.com"}
        </div>
        <div style="display: flex; gap: 1.5rem; justify-content: center; width: 100%; border-top: 1px solid var(--border-color); padding-top: 0.75rem; margin-top: 0.25rem;">
          <div>
            <div style="font-size: 0.7rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em;">Score</div>
            <div style="font-size: 1.05rem; font-weight: 700; color: var(--color-orange);">${(topPlayer.score || 0).toLocaleString()}</div>
          </div>
          <div>
            <div style="font-size: 0.7rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em;">Level</div>
            <div style="font-size: 1.05rem; font-weight: 700; color: var(--color-blue);">${topPlayer.LEVEL || 1}</div>
          </div>
        </div>
      `;
    }
  }

  // 2. Score Tier Distribution
  const tierEl = document.getElementById("tier-distribution");
  if (tierEl) {
    if (!users || users.length === 0) {
      tierEl.innerHTML =
        '<div style="color: var(--text-muted); font-size: 0.85rem;">No players.</div>';
      return;
    }

    let counts = [0, 0, 0, 0];
    users.forEach((u) => {
      const s = u.score || 0;
      if (s >= 20000) counts[0]++;
      else if (s >= 10000) counts[1]++;
      else if (s >= 5000) counts[2]++;
      else counts[3]++;
    });

    const total = users.length;
    const tiers = [
      {
        name: "Grandmaster (20k+)",
        count: counts[0],
        color: "var(--color-primary)",
      },
      {
        name: "Master (10k - 20k)",
        count: counts[1],
        color: "var(--color-blue)",
      },
      {
        name: "Elite (5k - 10k)",
        count: counts[2],
        color: "var(--color-green)",
      },
      { name: "Novice (< 5k)", count: counts[3], color: "var(--text-muted)" },
    ];

    let html = "";
    tiers.forEach((t) => {
      const pct = total > 0 ? Math.round((t.count / total) * 100) : 0;
      html += `
        <div style="margin-bottom: 0.4rem;">
          <div style="display: flex; justify-content: space-between; font-size: 0.8rem; margin-bottom: 0.2rem;">
            <span style="color: var(--text-secondary); display: flex; align-items: center; gap: 0.35rem;">
              <span style="display: inline-block; width: 8px; height: 8px; border-radius: 50%; background: ${t.color};"></span>
              ${t.name}
            </span>
            <span style="color: var(--text-main); font-weight: 600;">${t.count} (${pct}%)</span>
          </div>
        </div>
      `;
    });
    tierEl.innerHTML = html;
  }
}

// Render Engine Configuration Metadata in Settings panel
function renderSettingsMetadata() {
  const container = document.getElementById("settings-metadata-box");
  if (!container) return;

  const data = globalSettings || {};
  const items = [
    {
      label: "Default HP Cooldown",
      val: `${Math.round((data.health_cooldown_seconds || 1800) / 60)} mins`,
      desc: "Time taken to regenerate 1 health unit",
    },
    {
      label: "Time Allowed per Q",
      val: `${data.question_timer_seconds || 30}s`,
      desc: "Max seconds timer for game questions",
    },
    {
      label: "Standard Level reward",
      val: `+${data.main_level_score_reward || 100} pts`,
      desc: "Score rewarded upon completing main levels",
    },
    {
      label: "System status indicator",
      val: data.maintenance_mode ? "Maintenance" : "Operational",
      color: data.maintenance_mode
        ? "var(--color-danger)"
        : "var(--color-green)",
      desc: "Current client access gateway",
    },
  ];

  let html = "";
  items.forEach((item) => {
    html += `
      <div style="display: flex; justify-content: space-between; align-items: flex-start; padding: 0.65rem 0; border-bottom: 1px solid var(--border-color); font-size: 0.85rem;">
        <div>
          <div style="font-weight: 600; color: var(--text-main);">${item.label}</div>
          <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.1rem;">${item.desc}</div>
        </div>
        <div style="font-weight: bold; color: ${item.color || "var(--color-blue)"}; text-align: right;">
          ${item.val}
        </div>
      </div>
    `;
  });

  container.innerHTML = html;
}

// Render dynamic administrative statistics sidebar
function renderAdminsSidebar() {
  const container = document.getElementById("admins-statistics-box");
  if (!container) return;

  const total = admins ? admins.length : 0;
  const superadmins = admins
    ? admins.filter((a) => a.role === "superadmin").length
    : 0;
  const standard = total - superadmins;

  const items = [
    {
      label: "Total Operators",
      val: total,
      desc: "Registered accounts with console access",
    },
    {
      label: "Super Administrators",
      val: superadmins,
      desc: "Full authority including admin registration",
    },
    {
      label: "Standard Operators",
      val: standard,
      desc: "Can adjust configurations and view tables",
    },
  ];

  let html = "";
  items.forEach((item) => {
    html += `
      <div style="display: flex; justify-content: space-between; align-items: center; padding: 0.65rem 0; border-bottom: 1px solid var(--border-color); font-size: 0.85rem;">
        <div>
          <div style="font-weight: 600; color: var(--text-main);">${item.label}</div>
          <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.1rem;">${item.desc}</div>
        </div>
        <div style="font-weight: 700; color: var(--color-primary); font-size: 1rem;">
          ${item.val}
        </div>
      </div>
    `;
  });

  container.innerHTML = html;
}

// Settings changes logging console
function logSettingsActivity(msg) {
  const consoleEl = document.getElementById("settings-console");
  if (!consoleEl) return;
  const time = new Date().toLocaleTimeString();
  const line = document.createElement("div");
  line.className = "console-line";
  line.innerHTML = `<span class="console-timestamp">[${time}]</span> ${msg}`;
  consoleEl.appendChild(line);
  consoleEl.scrollTop = consoleEl.scrollHeight;
}

// Security audit logging console
function logAdminActivity(msg) {
  const consoleEl = document.getElementById("admins-console");
  if (!consoleEl) return;
  const time = new Date().toLocaleTimeString();
  const line = document.createElement("div");
  line.className = "console-line";
  line.innerHTML = `<span class="console-timestamp">[${time}]</span> ${msg}`;
  consoleEl.appendChild(line);
  consoleEl.scrollTop = consoleEl.scrollHeight;
}

// Initialize application
renderAppStructure();
// If logged in, trigger settings loaded populate
if (isLoggedIn) {
  // Let listeners run
}
