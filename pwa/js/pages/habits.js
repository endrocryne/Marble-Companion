/**
 * habits.js — Active habits list with check-in functionality
 */

import * as api from '../api.js';
import { setPageContent, showToast, navigate, escapeHtml } from '../app.js';
import { cacheHabits, getCachedHabits, enqueueOffline } from '../db.js';

const CAT_ICONS  = { Transport:'🚌', Food:'🥦', Energy:'⚡', Shopping:'🛍️', Travel:'✈️', Waste:'♻️' };
const CAT_COLORS = { Transport:'#3B82F6', Food:'#22C55E', Energy:'#F59E0B', Shopping:'#A855F7', Travel:'#EC4899', Waste:'#06B6D4' };

let _habits = [];

export async function init() {
  setPageContent(`
    <div class="page">
      <div class="page-header" style="padding:0;margin-bottom:16px">
        <div class="page-title">My Habits</div>
        <button class="btn btn-secondary btn-sm" onclick="window._app.navigate('habit-library')">+ Browse</button>
      </div>
      <div id="habits-container">
        ${skeletonList(4)}
      </div>
    </div>
    <button class="fab" aria-label="Log Action" onclick="window._app.navigate('log')">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
        <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
      </svg>
    </button>
  `);

  // Show cache while loading
  const cached = await getCachedHabits();
  if (cached?.length) { _habits = cached; renderHabits(); }

  await loadHabits();

  return () => {};
}

async function loadHabits() {
  try {
    _habits = await api.getActiveHabits();
    cacheHabits(_habits);
    renderHabits();
  } catch (err) {
    if (!_habits.length) {
      document.getElementById('habits-container').innerHTML = `
        <div class="empty-state">
          <div class="empty-emoji">🌱</div>
          <div class="empty-title">No habits yet</div>
          <div class="empty-desc">Browse the habit library to start building eco-friendly habits.</div>
          <button class="btn btn-primary mt-md" onclick="window._app.navigate('habit-library')">Browse Habits</button>
        </div>
      `;
    }
  }
}

function renderHabits() {
  const container = document.getElementById('habits-container');
  if (!container) return;

  if (!_habits.length) {
    container.innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">🌱</div>
        <div class="empty-title">No habits yet</div>
        <div class="empty-desc">Start building eco-friendly habits by browsing the library.</div>
        <button class="btn btn-primary mt-md" onclick="window._app.navigate('habit-library')">Browse Habits</button>
      </div>
    `;
    return;
  }

  container.innerHTML = `
    <div class="habit-grid">
      ${_habits.map((h) => renderHabitCard(h)).join('')}
    </div>
  `;

  container.querySelectorAll('.checkin-btn').forEach((btn) => {
    btn.addEventListener('click', (e) => {
      e.stopPropagation();
      handleCheckin(btn.dataset.id);
    });
  });

  container.querySelectorAll('.remove-btn').forEach((btn) => {
    btn.addEventListener('click', (e) => {
      e.stopPropagation();
      handleRemove(btn.dataset.id);
    });
  });
}

function renderHabitCard(h) {
  const color   = CAT_COLORS[h.category] || '#0D7377';
  const icon    = CAT_ICONS[h.category] || '✅';
  const checked = h.isCheckedInToday;

  return `
    <div class="habit-card ${checked ? 'checked-in' : ''}">
      <div class="habit-icon cat-bg-${h.category?.toLowerCase()}" style="background:${color}22">
        <span style="font-size:22px">${icon}</span>
      </div>
      <div class="habit-info">
        <div class="habit-name">${escapeHtml(h.name)}</div>
        <div class="habit-meta">
          🔥 ${h.currentStreak} day streak · ${h.frequency}
        </div>
        ${h.lastCheckinAt ? `<div class="habit-meta">Last: ${formatDate(h.lastCheckinAt)}</div>` : ''}
      </div>
      <div style="display:flex;flex-direction:column;gap:6px;align-items:flex-end">
        <button class="btn btn-sm ${checked ? 'btn-ghost' : 'btn-success'} checkin-btn" data-id="${h.id}" ${checked ? 'disabled' : ''}>
          ${checked ? '✓ Done' : 'Check In'}
        </button>
        <button class="btn btn-sm btn-ghost remove-btn" data-id="${h.id}" style="font-size:12px;color:var(--color-text-secondary)">Remove</button>
      </div>
    </div>
  `;
}

async function handleCheckin(id) {
  const habit = _habits.find((h) => h.id === id);
  if (!habit || habit.isCheckedInToday) return;

  // Optimistic update
  habit.isCheckedInToday = true;
  habit.currentStreak += 1;
  renderHabits();

  try {
    const result = await api.checkinHabit(id);
    showCheckinPopup(result.lpAwarded, result.co2eSaved, result.newStreak);
  } catch (err) {
    if (!navigator.onLine) {
      await enqueueOffline({ type: 'checkin', payload: { habitId: id } });
      showToast('Offline — check-in will sync when you\'re back online', 'warning');
    } else {
      // Revert optimistic update
      habit.isCheckedInToday = false;
      habit.currentStreak -= 1;
      renderHabits();
      showToast('Check-in failed: ' + err.message, 'error');
    }
  }
}

function showCheckinPopup(lp, co2e, streak) {
  const backdrop = document.createElement('div');
  backdrop.className = 'modal-backdrop center';
  backdrop.innerHTML = `
    <div class="lp-popup" id="checkin-popup">
      <div class="lp-popup-emoji">✅</div>
      <div class="lp-popup-title">Habit Complete!</div>
      <div class="lp-popup-lp">+${lp} LP</div>
      <div class="lp-popup-co2">${co2e > 0 ? `${co2e.toFixed(2)} kg CO₂e saved` : ''}</div>
      <div style="font-size:14px;color:var(--color-text-secondary);margin-top:4px">🔥 ${streak} day streak!</div>
      <button class="btn btn-primary mt-md" id="checkin-close">Great!</button>
    </div>
  `;
  document.body.appendChild(backdrop);
  requestAnimationFrame(() => document.getElementById('checkin-popup')?.classList.add('show'));
  document.getElementById('checkin-close')?.addEventListener('click', () => backdrop.remove());
  backdrop.addEventListener('click', (e) => { if (e.target === backdrop) backdrop.remove(); });
}

async function handleRemove(id) {
  if (!confirm('Remove this habit from your list?')) return;
  try {
    await api.removeHabit(id);
    _habits = _habits.filter((h) => h.id !== id);
    cacheHabits(_habits);
    renderHabits();
    showToast('Habit removed', 'info');
  } catch (err) {
    showToast('Failed to remove habit: ' + err.message, 'error');
  }
}

function skeletonList(n) {
  return `<div class="habit-grid">${Array(n).fill('<div class="habit-card skeleton" style="height:88px"></div>').join('')}</div>`;
}

function formatDate(iso) {
  const d = new Date(iso);
  const now = new Date();
  const diff = Math.floor((now - d) / 86400000);
  if (diff === 0) return 'Today';
  if (diff === 1) return 'Yesterday';
  return `${diff}d ago`;
}
