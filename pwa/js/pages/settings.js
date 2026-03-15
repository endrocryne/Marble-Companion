/**
 * settings.js — Notification preferences
 */

import * as api from '../api.js';
import { setPageContent, showToast } from '../app.js';

const PREFS = [
  { key: 'habitReminders',    label: 'Habit Reminders',      desc: 'Daily reminders to check in your habits',         icon: '✅' },
  { key: 'challengeUpdates',  label: 'Challenge Updates',    desc: 'Progress updates for your active challenges',      icon: '🏆' },
  { key: 'friendActivity',    label: 'Friend Activity',      desc: 'When friends complete achievements or check in',    icon: '👥' },
  { key: 'achievementAlerts', label: 'Achievement Alerts',   desc: 'Notifications when you unlock new achievements',   icon: '🏅' },
  { key: 'weeklyReport',      label: 'Weekly Report',        desc: 'A weekly summary of your eco-impact',             icon: '📊' },
];

let _prefs = {
  habitReminders: true,
  challengeUpdates: true,
  friendActivity: true,
  achievementAlerts: true,
  weeklyReport: false,
};

export async function init() {
  setPageContent(`
    <div class="page">
      <div style="display:flex;align-items:center;gap:12px;margin-bottom:20px">
        <button class="btn btn-icon btn-ghost" onclick="history.back()" aria-label="Back">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="15,18 9,12 15,6"/>
          </svg>
        </button>
        <div class="page-title">Settings</div>
      </div>

      <!-- Notification Preferences -->
      <div class="card mb-md">
        <div class="card-title mb-md">🔔 Notification Preferences</div>
        <div id="prefs-list">
          ${PREFS.map((p) => `
            <div class="pref-row">
              <span class="pref-icon">${p.icon}</span>
              <div class="pref-info">
                <div class="pref-label">${p.label}</div>
                <div class="pref-desc">${p.desc}</div>
              </div>
              <label class="toggle" aria-label="${p.label}">
                <input type="checkbox" class="toggle-input" data-pref="${p.key}" ${_prefs[p.key] ? 'checked' : ''} />
                <span class="toggle-slider"></span>
              </label>
            </div>
          `).join('')}
        </div>
        <button class="btn btn-primary btn-full mt-md" id="btn-save-prefs">Save Preferences</button>
      </div>

      <!-- App Info -->
      <div class="card mb-md">
        <div class="card-title mb-md">ℹ️ App Info</div>
        <div style="display:flex;flex-direction:column;gap:8px">
          <div class="info-row">
            <span class="info-label">Version</span>
            <span class="info-value">1.0.0 PWA</span>
          </div>
          <div class="info-row">
            <span class="info-label">Offline Mode</span>
            <span class="info-value">${navigator.onLine ? '🟢 Online' : '🔴 Offline'}</span>
          </div>
          <div class="info-row">
            <span class="info-label">Storage</span>
            <span class="info-value" id="storage-info">Calculating…</span>
          </div>
        </div>
      </div>

      <!-- Clear Cache -->
      <div class="card mb-lg">
        <div class="card-title mb-md" style="color:var(--color-warning)">⚠️ Data</div>
        <button class="btn btn-ghost btn-full" style="color:var(--color-warning);justify-content:flex-start" id="btn-clear-cache">
          Clear App Cache
        </button>
        <p style="font-size:12px;color:var(--color-text-secondary);margin-top:8px">
          Clears locally cached data. Your account data is safely stored on the server.
        </p>
      </div>
    </div>
  `);

  document.getElementById('btn-save-prefs')?.addEventListener('click', handleSave);
  document.getElementById('btn-clear-cache')?.addEventListener('click', handleClearCache);

  document.querySelectorAll('.toggle-input').forEach((input) => {
    input.addEventListener('change', (e) => {
      _prefs[e.target.dataset.pref] = e.target.checked;
    });
  });

  showStorageInfo();

  return () => {};
}

async function handleSave() {
  const btn = document.getElementById('btn-save-prefs');
  if (btn) { btn.disabled = true; btn.textContent = 'Saving…'; }

  try {
    await api.updateNotificationPrefs(_prefs);
    showToast('Notification preferences saved ✓', 'success');
  } catch (err) {
    showToast('Failed to save preferences: ' + err.message, 'error');
  } finally {
    if (btn) { btn.disabled = false; btn.textContent = 'Save Preferences'; }
  }
}

async function handleClearCache() {
  if (!confirm('Clear cached app data? This won\'t affect your account data.')) return;

  try {
    const cacheNames = await caches.keys();
    await Promise.all(cacheNames.map((name) => caches.delete(name)));

    // Clear IndexedDB caches
    const { dbClear } = await import('../db.js');
    await Promise.all(['habits','tree','content','challenges'].map((s) => dbClear(s)));

    showToast('Cache cleared! The app will reload.', 'success');
    setTimeout(() => location.reload(), 1500);
  } catch (err) {
    showToast('Failed to clear cache: ' + err.message, 'error');
  }
}

async function showStorageInfo() {
  const el = document.getElementById('storage-info');
  if (!el) return;

  try {
    if ('storage' in navigator && 'estimate' in navigator.storage) {
      const { usage, quota } = await navigator.storage.estimate();
      const usedMB  = (usage  / 1024 / 1024).toFixed(1);
      const quotaMB = (quota  / 1024 / 1024).toFixed(0);
      el.textContent = `${usedMB} MB / ${quotaMB} MB`;
    } else {
      el.textContent = 'Not available';
    }
  } catch {
    el.textContent = 'Not available';
  }
}
