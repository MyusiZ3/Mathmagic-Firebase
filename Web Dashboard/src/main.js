import './style.css';
import { initializeApp } from 'firebase/app';
import { 
  getFirestore, 
  collection, 
  doc, 
  getDoc, 
  getDocs, 
  setDoc, 
  updateDoc, 
  deleteDoc, 
  onSnapshot, 
  query, 
  orderBy, 
  limit 
} from 'firebase/firestore';

// Firebase configuration
const firebaseConfig = {
  apiKey: "AIzaSyBCJ-4_2We_oBvgbXd1qqE7lTuat_DVGn8",
  authDomain: "mathmagic-df71a.firebaseapp.com",
  databaseURL: "https://mathmagic-df71a-default-rtdb.firebaseio.com",
  projectId: "mathmagic-df71a",
  storageBucket: "mathmagic-df71a.firebasestorage.app",
  messagingSenderId: "953317182090",
  appId: "1:953317182090:web:1115cdc38b29b610ebbb73",
  measurementId: "G-XSQVYYT94Z"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const db = getFirestore(app);

// State Management
let currentTab = 'dashboard';
let users = [];
let globalSettings = {};
let selectedUser = null;
let isLoggedIn = sessionStorage.getItem('mm_admin_logged') === 'true';

// SVG Icons
const icons = {
  dashboard: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="9" rx="1"/><rect x="14" y="3" width="7" height="5" rx="1"/><rect x="14" y="12" width="7" height="9" rx="1"/><rect x="3" y="16" width="7" height="5" rx="1"/></svg>`,
  users: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`,
  settings: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>`,
  leaderboard: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/></svg>`,
  search: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>`,
  edit: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 1 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>`,
  delete: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/></svg>`,
  database: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><ellipse cx="12" cy="5" rx="9" ry="3"/><path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5"/><path d="M3 12c0 1.66 4 3 9 3s9-1.34 9-3"/></svg>`,
  logout: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>`
};

// UI Rendering Utilities
function showToast(message, type = 'success') {
  const container = document.getElementById('toast-container');
  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  toast.innerText = message;
  container.appendChild(toast);
  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transform = 'translateY(10px)';
    toast.style.transition = 'all 0.3s ease';
    setTimeout(() => toast.remove(), 300);
  }, 3000);
}

// Initial HTML Layout Setup
function renderAppStructure() {
  const appEl = document.getElementById('app');

  if (!isLoggedIn) {
    appEl.innerHTML = `
      <div class="login-container">
        <div class="login-card">
          <div class="login-header">
            <h1>Mathmagic <span>Admin</span></h1>
            <p>Enter administrative passcode to continue</p>
          </div>
          <form id="login-form">
            <div class="form-group">
              <label for="passcode">Admin Passcode</label>
              <input type="password" id="passcode" class="form-control" placeholder="••••••••" required autofocus />
            </div>
            <button type="submit" class="btn btn-primary" style="width: 100%; height: 48px; margin-top: 1rem;">
              Sign In
            </button>
          </form>
        </div>
      </div>
      <div id="toast-container" class="toast-container"></div>
    `;

    document.getElementById('login-form').addEventListener('submit', (e) => {
      e.preventDefault();
      const pin = document.getElementById('passcode').value;
      if (pin === 'admin123' || pin === 'adminmathmagic') {
        sessionStorage.setItem('mm_admin_logged', 'true');
        isLoggedIn = true;
        showToast('Successfully authenticated!');
        setTimeout(() => renderAppStructure(), 500);
      } else {
        showToast('Invalid passcode. Access Denied.', 'error');
      }
    });
    return;
  }

  // Dashboard structure
  appEl.innerHTML = `
    <!-- Sidebar -->
    <aside class="sidebar">
      <div class="sidebar-logo">
        Mathmagic <span>Console</span>
      </div>
      
      <nav style="flex: 1;">
        <ul class="nav-links">
          <li class="nav-item">
            <button class="nav-btn ${currentTab === 'dashboard' ? 'active' : ''}" data-tab="dashboard">
              ${icons.dashboard} Dashboard Overview
            </button>
          </li>
          <li class="nav-item">
            <button class="nav-btn ${currentTab === 'users' ? 'active' : ''}" data-tab="users">
              ${icons.users} User Accounts
            </button>
          </li>
          <li class="nav-item">
            <button class="nav-btn ${currentTab === 'settings' ? 'active' : ''}" data-tab="settings">
              ${icons.settings} Global Settings
            </button>
          </li>
          <li class="nav-item">
            <button class="nav-btn ${currentTab === 'leaderboard' ? 'active' : ''}" data-tab="leaderboard">
              ${icons.leaderboard} Leaderboard
            </button>
          </li>
        </ul>
      </nav>

      <div class="sidebar-footer">
        <div class="admin-profile">
          <span class="admin-name">Administrator</span>
          <span class="admin-role">Super Admin</span>
        </div>
        <button id="logout-btn" class="logout-btn" title="Logout">
          ${icons.logout}
        </button>
      </div>
    </aside>

    <!-- Main Content Area -->
    <main class="main-content">
      <header class="navbar">
        <div class="navbar-title">
          <h2 id="navbar-title-text">Dashboard Overview</h2>
        </div>
        <div class="navbar-actions">
          <div class="sync-status">
            <div class="sync-indicator"></div>
            <span>Firestore Sync Active</span>
          </div>
        </div>
      </header>

      <!-- Panel: Dashboard Overview -->
      <section id="panel-dashboard" class="page-panel ${currentTab === 'dashboard' ? 'active' : ''}">
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
        </div>

        <div class="dashboard-grid">
          <div class="dashboard-box">
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

          <div class="dashboard-box">
            <h3>${icons.database} Firebase Services</h3>
            <div class="server-status-list">
              <div class="status-item">
                <span class="status-name">Cloud Firestore</span>
                <span class="status-badge status-online">Connected</span>
              </div>
              <div class="status-item">
                <span class="status-name">App Hosting</span>
                <span class="status-badge status-online">Online</span>
              </div>
              <div class="status-item">
                <span class="status-name">Authentication</span>
                <span class="status-badge status-online">Connected</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Panel: Users -->
      <section id="panel-users" class="page-panel ${currentTab === 'users' ? 'active' : ''}">
        <div class="table-card">
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
                  <th>Username</th>
                  <th>Email</th>
                  <th>Score</th>
                  <th>Level</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody id="users-tbody">
                <tr><td colspan="5" style="text-align: center; color: var(--text-muted);">Loading user directory...</td></tr>
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <!-- Panel: Global Settings -->
      <section id="panel-settings" class="page-panel ${currentTab === 'settings' ? 'active' : ''}">
        <div class="dashboard-box" style="max-width: 800px; margin: 0 auto;">
          <h3 style="border-bottom: 1px solid var(--border-color); padding-bottom: 1rem;">
            ${icons.settings} Game Balance & Configuration
          </h3>
          
          <form id="global-settings-form" style="margin-top: 1.5rem;">
            <div class="settings-form-grid">
              <div class="form-group">
                <label for="input-max-health">Max Health</label>
                <input type="number" id="input-max-health" class="form-control" min="1" max="100" required />
              </div>
              <div class="form-group">
                <label for="input-health-cooldown">Health Cooldown (Seconds)</label>
                <input type="number" id="input-health-cooldown" class="form-control" min="10" required />
              </div>
              <div class="form-group">
                <label for="input-timer">Question Timer (Seconds)</label>
                <input type="number" id="input-timer" class="form-control" min="5" required />
              </div>
              <div class="form-group">
                <label for="input-leaderboard-limit">Leaderboard Show Limit</label>
                <input type="number" id="input-leaderboard-limit" class="form-control" min="1" max="100" required />
              </div>
              <div class="form-group">
                <label for="input-main-reward">Main Level Score Reward</label>
                <input type="number" id="input-main-reward" class="form-control" min="1" required />
              </div>
              <div class="form-group">
                <label for="input-bonus-reward">Bonus Level Score Reward</label>
                <input type="number" id="input-bonus-reward" class="form-control" min="1" required />
              </div>
            </div>

            <div class="form-group">
              <label for="input-app-version">Target App Version</label>
              <input type="text" id="input-app-version" class="form-control" placeholder="e.g. 1.0.0" required />
            </div>

            <div class="switch-group">
              <div class="switch-label">
                <span class="switch-title">Maintenance Mode</span>
                <span class="switch-desc">Block access to the game for maintenance</span>
              </div>
              <label class="switch">
                <input type="checkbox" id="check-maintenance">
                <span class="slider"></span>
              </label>
            </div>

            <div class="switch-group" style="margin-bottom: 2rem; border-bottom: none;">
              <div class="switch-label">
                <span class="switch-title">Disable Leaderboards</span>
                <span class="switch-desc">Temporarily freeze or hide all player leaderboards</span>
              </div>
              <label class="switch">
                <input type="checkbox" id="check-leaderboard-disabled">
                <span class="slider"></span>
              </label>
            </div>

            <div style="display: flex; gap: 1rem; justify-content: flex-end;">
              <button type="button" id="btn-reset-settings" class="btn btn-secondary">Reset to Default</button>
              <button type="submit" class="btn btn-primary">Save Remote Configuration</button>
            </div>
          </form>
        </div>
      </section>

      <!-- Panel: Leaderboard -->
      <section id="panel-leaderboard" class="page-panel ${currentTab === 'leaderboard' ? 'active' : ''}">
        <div class="table-card" style="max-width: 900px; margin: 0 auto;">
          <div class="table-header">
            <h3 style="font-family: var(--font-title); font-size: 1.25rem; font-weight: 700; color: var(--color-primary);">
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
                  <th>Max Level reached</th>
                  <th style="text-align: right;">Total Score</th>
                </tr>
              </thead>
              <tbody id="leaderboard-tbody">
                <tr><td colspan="4" style="text-align: center; color: var(--text-muted);">Calculating scores...</td></tr>
              </tbody>
            </table>
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
            <label>Username</label>
            <input type="text" id="edit-username" class="form-control" readonly style="opacity: 0.6; cursor: not-allowed;" />
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

    <div id="toast-container" class="toast-container"></div>
  `;

  // Attach navigation events
  document.querySelectorAll('.nav-btn').forEach(btn => {
    btn.addEventListener('click', (e) => {
      const tab = e.currentTarget.getAttribute('data-tab');
      switchTab(tab);
    });
  });

  // Attach logout event
  document.getElementById('logout-btn').addEventListener('click', () => {
    sessionStorage.removeItem('mm_admin_logged');
    isLoggedIn = false;
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
    dashboard: 'Dashboard Overview',
    users: 'User Accounts Directory',
    settings: 'Global App Settings',
    leaderboard: 'Live Leaderboard Monitor'
  };
  document.getElementById('navbar-title-text').innerText = titleText[tabId];

  // Toggle active button
  document.querySelectorAll('.nav-btn').forEach(btn => {
    if (btn.getAttribute('data-tab') === tabId) {
      btn.classList.add('active');
    } else {
      btn.classList.remove('active');
    }
  });

  // Toggle active panel
  document.querySelectorAll('.page-panel').forEach(panel => {
    if (panel.id === `panel-${tabId}`) {
      panel.classList.add('active');
    } else {
      panel.classList.remove('active');
    }
  });
}

// Start watching Firestore data
function startFirestoreListeners() {
  // Listen for settings changes
  const settingsDocRef = doc(db, 'settings', 'global');
  onSnapshot(settingsDocRef, (docSnap) => {
    if (docSnap.exists()) {
      globalSettings = docSnap.data();
      populateSettingsForm(globalSettings);
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
        app_version: "1.0.0"
      };
      setDoc(settingsDocRef, defaultSettings).then(() => {
        showToast('Initialized default global settings in Firestore.');
      });
    }
  });

  // Listen for users collection changes
  const usersColRef = collection(db, 'users');
  onSnapshot(usersColRef, (querySnap) => {
    users = [];
    querySnap.forEach((docSnap) => {
      const data = docSnap.data();
      users.push({
        id: docSnap.id,
        ...data
      });
    });
    updateStats();
    renderUsersTable();
    renderLeaderboard();
  });
}

function populateSettingsForm(settings) {
  if (currentTab === 'settings') {
    document.getElementById('input-max-health').value = settings.max_health || 5;
    document.getElementById('input-health-cooldown').value = settings.health_cooldown_seconds || 1800;
    document.getElementById('input-timer').value = settings.question_timer_seconds || 30;
    document.getElementById('input-leaderboard-limit').value = settings.leaderboard_limit || 50;
    document.getElementById('input-main-reward').value = settings.main_level_score_reward || 100;
    document.getElementById('input-bonus-reward').value = settings.bonus_level_score_reward || 250;
    document.getElementById('input-app-version').value = settings.app_version || "1.0.0";
    document.getElementById('check-maintenance').checked = !!settings.maintenance_mode;
    document.getElementById('check-leaderboard-disabled').checked = !!settings.leaderboard_disabled;
  }
}

function updateStats() {
  if (!document.getElementById('stat-total-users')) return;

  const total = users.length;
  document.getElementById('stat-total-users').innerText = total;

  if (total > 0) {
    const sumScore = users.reduce((acc, u) => acc + (parseInt(u.score) || 0), 0);
    const avgScore = Math.round(sumScore / total);
    document.getElementById('stat-avg-score').innerText = avgScore;

    const sumLevel = users.reduce((acc, u) => acc + (parseInt(u.LEVEL) || 0), 0);
    const avgLevel = (sumLevel / total).toFixed(1);
    document.getElementById('stat-avg-level').innerText = avgLevel;

    // Top 5 Active Users dashboard box
    const sorted = [...users].sort((a,b) => (b.score || 0) - (a.score || 0)).slice(0, 5);
    const tbody = document.getElementById('top-users-tbody');
    tbody.innerHTML = sorted.map(u => `
      <tr>
        <td>
          <div class="user-info-td">
            <div class="user-avatar">${(u.username || 'U').charAt(0).toUpperCase()}</div>
            <div>
              <div style="font-weight: 600; color: #fff;">${u.username || 'Anonymous'}</div>
              <div style="font-size: 0.75rem; color: var(--text-muted);">${u.email || 'No Email'}</div>
            </div>
          </div>
        </td>
        <td style="font-weight: 700; color: var(--color-primary);">${u.score || 0}</td>
        <td>Level ${u.LEVEL || 1}</td>
      </tr>
    `).join('');
  } else {
    document.getElementById('stat-avg-score').innerText = '0';
    document.getElementById('stat-avg-level').innerText = '0.0';
    document.getElementById('top-users-tbody').innerHTML = `
      <tr><td colspan="3" style="text-align: center; color: var(--text-muted);">No users registered yet.</td></tr>
    `;
  }
}

function renderUsersTable(filterText = '') {
  const tbody = document.getElementById('users-tbody');
  if (!tbody) return;

  const queryStr = filterText.toLowerCase().trim();
  const filtered = users.filter(u => {
    const name = (u.username || '').toLowerCase();
    const mail = (u.email || '').toLowerCase();
    return name.includes(queryStr) || mail.includes(queryStr);
  });

  document.getElementById('user-count-display').innerText = `${filtered.length} users found`;

  if (filtered.length === 0) {
    tbody.innerHTML = `
      <tr><td colspan="5" style="text-align: center; color: var(--text-muted);">No users match search criteria.</td></tr>
    `;
    return;
  }

  tbody.innerHTML = filtered.map(u => `
    <tr>
      <td>
        <div class="user-info-td">
          <div class="user-avatar" style="background: linear-gradient(135deg, var(--color-blue) 0%, var(--color-violet) 100%);">
            ${(u.username || 'U').charAt(0).toUpperCase()}
          </div>
          <span style="font-weight: 600;">${u.username || 'Anonymous'}</span>
        </div>
      </td>
      <td style="color: var(--text-muted); font-size: 0.9rem;">${u.email || 'No email registered'}</td>
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
  `).join('');

  // Attach button actions dynamically
  tbody.querySelectorAll('.edit-user-btn').forEach(btn => {
    btn.addEventListener('click', (e) => {
      const uId = e.currentTarget.getAttribute('data-id');
      openEditModal(uId);
    });
  });

  tbody.querySelectorAll('.delete-user-btn').forEach(btn => {
    btn.addEventListener('click', (e) => {
      const uId = e.currentTarget.getAttribute('data-id');
      openDeleteModal(uId);
    });
  });
}

function renderLeaderboard() {
  const tbody = document.getElementById('leaderboard-tbody');
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

  tbody.innerHTML = boardUsers.map((u, index) => {
    const rank = index + 1;
    let rankBadgeClass = '';
    let rankText = rank;

    if (rank === 1) rankText = '🥇';
    else if (rank === 2) rankText = '🥈';
    else if (rank === 3) rankText = '🥉';

    return `
      <tr>
        <td style="text-align: center; font-weight: 700; font-size: 1.1rem; color: #fff;">${rankText}</td>
        <td>
          <div class="user-info-td">
            <div class="user-avatar" style="background: ${rank === 1 ? 'var(--color-primary)' : 'rgba(255,255,255,0.05)'}; color: ${rank === 1 ? 'var(--bg-deep)' : '#fff'}">
              ${(u.username || 'U').charAt(0).toUpperCase()}
            </div>
            <div>
              <div style="font-weight: 600; color: #fff;">${u.username || 'Anonymous'}</div>
              <div style="font-size: 0.75rem; color: var(--text-muted);">${u.email || 'No email'}</div>
            </div>
          </div>
        </td>
        <td>Level ${u.LEVEL || 1}</td>
        <td style="text-align: right; font-weight: 800; font-size: 1.05rem; color: var(--color-primary);">${u.score || 0}</td>
      </tr>
    `;
  }).join('');
}

function openEditModal(userId) {
  selectedUser = users.find(u => u.id === userId);
  if (!selectedUser) return;

  document.getElementById('edit-username').value = selectedUser.username || 'Anonymous';
  document.getElementById('edit-score').value = selectedUser.score || 0;
  document.getElementById('edit-level').value = selectedUser.LEVEL !== undefined ? selectedUser.LEVEL : 1;
  document.getElementById('edit-hp').value = selectedUser.Hp !== undefined ? selectedUser.Hp : 5;

  document.getElementById('edit-user-modal').classList.add('active');
}

function openDeleteModal(userId) {
  selectedUser = users.find(u => u.id === userId);
  if (!selectedUser) return;

  document.getElementById('delete-username-text').innerText = selectedUser.username || 'Anonymous';
  document.getElementById('delete-user-modal').classList.add('active');
}

function closeModals() {
  document.querySelectorAll('.modal-overlay').forEach(modal => {
    modal.classList.remove('active');
  });
  selectedUser = null;
}

function setupTabFunctionality() {
  // Search bar functionality
  const searchInput = document.getElementById('user-search-input');
  if (searchInput) {
    searchInput.addEventListener('input', (e) => {
      renderUsersTable(e.target.value);
    });
  }

  // Edit user form submission
  const editForm = document.getElementById('edit-user-form');
  if (editForm) {
    editForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      if (!selectedUser) return;

      const scoreVal = parseInt(document.getElementById('edit-score').value);
      const levelVal = parseInt(document.getElementById('edit-level').value);
      const hpVal = parseInt(document.getElementById('edit-hp').value);

      try {
        const userDocRef = doc(db, 'users', selectedUser.id);
        await updateDoc(userDocRef, {
          score: scoreVal,
          LEVEL: levelVal,
          Hp: hpVal
        });
        showToast(`Successfully updated credentials for ${selectedUser.username}`);
        closeModals();
      } catch (err) {
        showToast(`Error updating user: ${err.message}`, 'error');
      }
    });
  }

  // Delete user confirm
  const confirmDeleteBtn = document.getElementById('confirm-delete-btn');
  if (confirmDeleteBtn) {
    confirmDeleteBtn.addEventListener('click', async () => {
      if (!selectedUser) return;

      try {
        const userDocRef = doc(db, 'users', selectedUser.id);
        await deleteDoc(userDocRef);
        showToast(`User ${selectedUser.username} has been permanently deleted.`);
        closeModals();
      } catch (err) {
        showToast(`Error deleting user: ${err.message}`, 'error');
      }
    });
  }

  // Modal closes
  document.querySelectorAll('.modal-close-btn').forEach(btn => {
    btn.addEventListener('click', closeModals);
  });

  // Settings form submission
  const settingsForm = document.getElementById('global-settings-form');
  if (settingsForm) {
    settingsForm.addEventListener('submit', async (e) => {
      e.preventDefault();

      const updatedSettings = {
        max_health: parseInt(document.getElementById('input-max-health').value),
        health_cooldown_seconds: parseInt(document.getElementById('input-health-cooldown').value),
        question_timer_seconds: parseInt(document.getElementById('input-timer').value),
        leaderboard_limit: parseInt(document.getElementById('input-leaderboard-limit').value),
        main_level_score_reward: parseInt(document.getElementById('input-main-reward').value),
        bonus_level_score_reward: parseInt(document.getElementById('input-bonus-reward').value),
        app_version: document.getElementById('input-app-version').value,
        maintenance_mode: document.getElementById('check-maintenance').checked,
        leaderboard_disabled: document.getElementById('check-leaderboard-disabled').checked
      };

      try {
        const settingsDocRef = doc(db, 'settings', 'global');
        await setDoc(settingsDocRef, updatedSettings);
        showToast('Global settings updated and synchronized successfully!');
      } catch (err) {
        showToast(`Failed to update settings: ${err.message}`, 'error');
      }
    });

    // Reset settings button
    document.getElementById('btn-reset-settings').addEventListener('click', () => {
      document.getElementById('input-max-health').value = 5;
      document.getElementById('input-health-cooldown').value = 1800;
      document.getElementById('input-timer').value = 30;
      document.getElementById('input-leaderboard-limit').value = 50;
      document.getElementById('input-main-reward').value = 100;
      document.getElementById('input-bonus-reward').value = 250;
      document.getElementById('input-app-version').value = "1.0.0";
      document.getElementById('check-maintenance').checked = false;
      document.getElementById('check-leaderboard-disabled').checked = false;
      showToast('Form reset to system defaults. Click Save to publish.');
    });
  }
}

// Initialize application
renderAppStructure();
// If logged in, trigger settings loaded populate
if (isLoggedIn) {
  // Let listeners run
}
