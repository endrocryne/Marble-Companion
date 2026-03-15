/**
 * profile.js — User profile, stats, edit mode, and logout
 */

import * as api from '../api.js';
import { setPageContent, showToast, navigate, escapeHtml } from '../app.js';
import { getUser, saveAuth, logout } from '../auth.js';

const AVATAR_EMOJIS = ['🌿','🌊','🌻','🦋','🦜','🌙','❄️','🔥'];

let _editMode = false;

export async function init() {
  _editMode = false;
  const user = getUser();
  renderProfile(user, false);

  // Fetch fresh user data
  try {
    const fresh = await api.getMe();
    const token   = localStorage.getItem('mc_token');
    const refresh = localStorage.getItem('mc_refresh');
    const expires = localStorage.getItem('mc_expires');
    saveAuth(token, refresh, expires, fresh);
    renderProfile(fresh, _editMode);
  } catch { /* use cached */ }

  return () => {};
}

function renderProfile(user, editMode) {
  setPageContent(`
    <div class="page">
      <!-- Profile Header -->
      <div style="background:linear-gradient(135deg,#0D7377,#14A085);padding:32px 16px 24px;text-align:center;color:#fff;margin:-16px -16px 16px;border-radius:0 0 24px 24px">
        <div class="avatar avatar-lg avatar-${user?.avatarIndex ?? 0}" style="width:72px;height:72px;font-size:32px;margin:0 auto 12px">
          ${AVATAR_EMOJIS[user?.avatarIndex ?? 0]}
        </div>
        <div style="font-size:22px;font-weight:800">${escapeHtml(user?.displayName || 'Explorer')}</div>
        <div style="font-size:13px;opacity:0.8;margin-top:4px">${escapeHtml(user?.email || '')}</div>
        ${user?.regionCountry ? `<div style="font-size:12px;opacity:0.7;margin-top:2px">📍 ${escapeHtml(user.regionCountry)}</div>` : ''}
      </div>

      <!-- Stats -->
      <div class="stats-row mb-md">
        <div class="stat-card">
          <div class="stat-icon">🔥</div>
          <div class="stat-value">${user?.currentStreak ?? 0}</div>
          <div class="stat-label">Day Streak</div>
        </div>
        <div class="stat-card">
          <div class="stat-icon">🍃</div>
          <div class="stat-value">${(user?.totalLP ?? 0).toLocaleString()}</div>
          <div class="stat-label">Leaf Points</div>
        </div>
        <div class="stat-card">
          <div class="stat-icon">🏆</div>
          <div class="stat-value">${user?.longestStreak ?? 0}</div>
          <div class="stat-label">Best Streak</div>
        </div>
      </div>

      <!-- Info Card -->
      <div class="card mb-md" id="info-card">
        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:16px">
          <div class="card-title">Profile Info</div>
          <button class="btn btn-secondary btn-sm" id="btn-edit">${editMode ? 'Cancel' : 'Edit'}</button>
        </div>
        ${editMode ? renderEditForm(user) : renderInfoView(user)}
      </div>

      <!-- Quick Links -->
      <div class="card mb-md">
        <div class="card-title mb-md">More</div>
        <div style="display:flex;flex-direction:column;gap:4px">
          ${menuItem('🏆', 'Achievements', 'achievements')}
          ${menuItem('🏅', 'Challenges',   'challenges')}
          ${menuItem('📚', 'Learn & Earn', 'content')}
          ${menuItem('🌱', 'Offset Credits','offsets')}
          ${menuItem('⚙️', 'Settings',     'settings')}
          ${menuItem('💡', 'Suggestions',  'suggestions')}
        </div>
      </div>

      <!-- Danger Zone -->
      <div class="card mb-lg">
        <div class="card-title mb-md" style="color:var(--color-error)">Account</div>
        <button class="btn btn-ghost btn-full" style="color:var(--color-error);justify-content:flex-start" id="btn-logout">
          Sign Out
        </button>
      </div>

      <div style="text-align:center;font-size:11px;color:var(--color-text-secondary);margin-bottom:32px">
        Joined ${user?.joinedAt ? new Date(user.joinedAt).toLocaleDateString(undefined, { year:'numeric', month:'long' }) : '—'}
      </div>
    </div>
  `);

  document.getElementById('btn-edit')?.addEventListener('click', () => {
    _editMode = !_editMode;
    renderProfile(getUser(), _editMode);
  });

  document.getElementById('btn-logout')?.addEventListener('click', handleLogout);

  if (editMode) {
    document.getElementById('btn-save-profile')?.addEventListener('click', handleSave);
    document.querySelectorAll('.avatar-option-sm').forEach((el) => {
      el.addEventListener('click', () => {
        document.querySelectorAll('.avatar-option-sm').forEach((a) => a.classList.remove('selected'));
        el.classList.add('selected');
      });
    });
  }

  document.querySelectorAll('.menu-link').forEach((link) => {
    link.addEventListener('click', () => navigate(link.dataset.route));
  });
}

function renderInfoView(user) {
  return `
    <div style="display:flex;flex-direction:column;gap:8px">
      <div class="info-row">
        <span class="info-label">Display Name</span>
        <span class="info-value">${escapeHtml(user?.displayName || '—')}</span>
      </div>
      <div class="info-row">
        <span class="info-label">Email</span>
        <span class="info-value">${escapeHtml(user?.email || '—')}</span>
      </div>
      <div class="info-row">
        <span class="info-label">Region</span>
        <span class="info-value">${escapeHtml([user?.regionContinent, user?.regionCountry].filter(Boolean).join(', ') || '—')}</span>
      </div>
    </div>
  `;
}

function renderEditForm(user) {
  const CONTINENTS = ['Africa','Antarctica','Asia','Europe','North America','Oceania','South America'];
  return `
    <div style="display:flex;flex-direction:column;gap:12px">
      <div class="form-group" style="margin-bottom:0">
        <label class="form-label">Display Name</label>
        <input type="text" class="form-input" id="edit-name" value="${escapeHtml(user?.displayName || '')}" maxlength="50" />
      </div>
      <div class="form-group" style="margin-bottom:0">
        <label class="form-label">Avatar</label>
        <div style="display:flex;gap:8px;flex-wrap:wrap">
          ${AVATAR_EMOJIS.map((emoji, i) => `
            <div class="avatar-option-sm ${i === (user?.avatarIndex ?? 0) ? 'selected' : ''}" data-avatar="${i}" role="button" aria-label="Avatar ${i+1}">
              <div class="avatar avatar-${i}" style="width:36px;height:36px;font-size:16px">${emoji}</div>
            </div>
          `).join('')}
        </div>
      </div>
      <div class="form-group" style="margin-bottom:0">
        <label class="form-label">Continent</label>
        <select class="form-input" id="edit-continent">
          <option value="">— None —</option>
          ${CONTINENTS.map((c) => `<option value="${c}" ${user?.regionContinent === c ? 'selected' : ''}>${c}</option>`).join('')}
        </select>
      </div>
      <div class="form-group" style="margin-bottom:0">
        <label class="form-label">Country</label>
        <input type="text" class="form-input" id="edit-country" value="${escapeHtml(user?.regionCountry || '')}" placeholder="e.g. United Kingdom" />
      </div>
      <button class="btn btn-primary btn-full" id="btn-save-profile">Save Changes</button>
    </div>
  `;
}

async function handleSave() {
  const name      = document.getElementById('edit-name')?.value?.trim();
  const continent = document.getElementById('edit-continent')?.value;
  const country   = document.getElementById('edit-country')?.value?.trim();
  const avatarEl  = document.querySelector('.avatar-option-sm.selected');
  const avatar    = avatarEl ? parseInt(avatarEl.dataset.avatar) : getUser()?.avatarIndex;

  if (!name) { showToast('Display name is required', 'error'); return; }

  const btn = document.getElementById('btn-save-profile');
  if (btn) { btn.disabled = true; btn.textContent = 'Saving…'; }

  try {
    const updated = await api.updateMe({
      displayName:     name,
      avatarIndex:     avatar,
      regionContinent: continent || undefined,
      regionCountry:   country   || undefined,
    });

    const token   = localStorage.getItem('mc_token');
    const refresh = localStorage.getItem('mc_refresh');
    const expires = localStorage.getItem('mc_expires');
    saveAuth(token, refresh, expires, updated);

    showToast('Profile updated! 🌿', 'success');
    _editMode = false;
    renderProfile(updated, false);
  } catch (err) {
    showToast('Failed to save: ' + err.message, 'error');
    if (btn) { btn.disabled = false; btn.textContent = 'Save Changes'; }
  }
}

async function handleLogout() {
  if (!confirm('Are you sure you want to sign out?')) return;
  await logout();
  navigate('onboarding');
}

function menuItem(icon, label, route) {
  return `
    <div class="menu-link" data-route="${route}" role="button" style="display:flex;align-items:center;gap:12px;padding:12px 0;cursor:pointer;border-bottom:1px solid var(--color-divider)">
      <span style="font-size:20px;width:28px">${icon}</span>
      <span style="font-size:15px;font-weight:500;color:var(--color-text-primary);flex:1">${label}</span>
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="var(--color-text-secondary)" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="9 18 15 12 9 6"/>
      </svg>
    </div>
  `;
}
