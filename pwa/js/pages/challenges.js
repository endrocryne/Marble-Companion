/**
 * challenges.js — Curated challenges list + my challenges
 */

import * as api from '../api.js';
import { setPageContent, showToast, escapeHtml, navigate } from '../app.js';

const CAT_ICONS = { Transport:'🚌', Food:'🥦', Energy:'⚡', Shopping:'🛍️', Travel:'✈️', Waste:'♻️' };
const DIFF_CLASS = { Easy:'badge-easy', Medium:'badge-medium', Hard:'badge-hard' };
const TYPE_ICONS = { Individual:'🧍', Group:'👥', Competitive:'🏆' };

let _activeTab = 'available';

export async function init(params = {}) {
  setPageContent(`
    <div class="page">
      <div class="page-header" style="padding:0;margin-bottom:16px">
        <div class="page-title">Challenges</div>
      </div>
      <div class="tab-bar">
        <button class="tab-btn active" id="tab-available">Available</button>
        <button class="tab-btn" id="tab-mine">My Challenges</button>
      </div>
      <div id="panel-available" class="tab-panel active">
        ${skeleton(3)}
      </div>
      <div id="panel-mine" class="tab-panel">
        ${skeleton(2)}
      </div>
    </div>
  `);

  document.getElementById('tab-available')?.addEventListener('click', () => switchTab('available'));
  document.getElementById('tab-mine')?.addEventListener('click',      () => switchTab('mine'));

  await Promise.all([loadAvailable(), loadMine()]);

  return () => {};
}

function switchTab(tab) {
  _activeTab = tab;
  document.getElementById('tab-available').classList.toggle('active', tab === 'available');
  document.getElementById('tab-mine').classList.toggle('active', tab === 'mine');
  document.getElementById('panel-available').classList.toggle('active', tab === 'available');
  document.getElementById('panel-mine').classList.toggle('active', tab === 'mine');
}

async function loadAvailable() {
  try {
    const challenges = await api.getCuratedChallenges();
    renderChallenges('panel-available', challenges, false);
  } catch {
    document.getElementById('panel-available').innerHTML = emptyState('No challenges available right now. Check back soon!');
  }
}

async function loadMine() {
  try {
    const challenges = await api.getMyChallenges();
    renderChallenges('panel-mine', challenges, true);
  } catch {
    document.getElementById('panel-mine').innerHTML = emptyState("You haven't joined any challenges yet.", 'available');
  }
}

function renderChallenges(panelId, challenges, showProgress) {
  const el = document.getElementById(panelId);
  if (!el) return;

  if (!challenges?.length) {
    el.innerHTML = emptyState(showProgress
      ? "You haven't joined any challenges yet."
      : 'No challenges found.');
    return;
  }

  el.innerHTML = challenges.map((c) => `
    <div class="challenge-card">
      <div class="challenge-header">
        <div class="challenge-icon">${CAT_ICONS[c.category] || '🌿'}</div>
        <div style="flex:1">
          <div class="challenge-title">${escapeHtml(c.title)}</div>
          <div style="display:flex;gap:6px;margin-top:4px;flex-wrap:wrap">
            <span class="badge ${DIFF_CLASS[c.difficulty] || 'badge-primary'}">${c.difficulty}</span>
            <span class="badge badge-primary">${TYPE_ICONS[c.type] || ''} ${c.type}</span>
            <span class="badge badge-accent">🍃 ${c.lpReward} LP</span>
          </div>
        </div>
      </div>
      <div class="challenge-desc">${escapeHtml(c.description || '')}</div>
      <div class="challenge-meta">
        ${c.participantCount != null ? `<span style="font-size:12px;color:var(--color-text-secondary)">👥 ${c.participantCount} participants</span>` : ''}
        ${c.endsAt ? `<span style="font-size:12px;color:var(--color-text-secondary)">⏰ Ends ${formatDate(c.endsAt)}</span>` : ''}
      </div>
      ${showProgress && c.progress != null ? `
        <div style="margin-bottom:12px">
          <div style="display:flex;justify-content:space-between;font-size:12px;color:var(--color-text-secondary);margin-bottom:4px">
            <span>Progress</span><span>${c.progress ?? 0} / ${c.targetValue}</span>
          </div>
          <div class="progress-bar">
            <div class="progress-fill" style="width:${Math.min(100,(c.progress/c.targetValue)*100)}%"></div>
          </div>
        </div>
        <button class="btn btn-secondary btn-sm btn-full update-progress-btn" data-id="${c.id}">Update Progress</button>
      ` : `
        <button class="btn btn-primary btn-sm btn-full join-btn" data-id="${c.id}">Join Challenge</button>
      `}
    </div>
  `).join('');

  el.querySelectorAll('.join-btn').forEach((btn) => {
    btn.addEventListener('click', () => handleJoin(btn.dataset.id, btn));
  });

  el.querySelectorAll('.update-progress-btn').forEach((btn) => {
    btn.addEventListener('click', () => handleUpdateProgress(btn.dataset.id));
  });
}

async function handleJoin(id, btn) {
  btn.disabled = true;
  btn.textContent = 'Joining…';
  try {
    await api.joinChallenge(id);
    showToast('Joined challenge! 🎯', 'success');
    btn.textContent = 'Joined ✓';
    await loadMine();
  } catch (err) {
    btn.disabled = false;
    btn.textContent = 'Join Challenge';
    showToast('Failed to join: ' + err.message, 'error');
  }
}

async function handleUpdateProgress(id) {
  const val = prompt('Enter your current progress value:');
  if (val === null) return;
  const num = parseFloat(val);
  if (isNaN(num)) { showToast('Please enter a valid number', 'error'); return; }

  try {
    await api.updateChallengeProgress(id, num);
    showToast('Progress updated! 💪', 'success');
    await loadMine();
  } catch (err) {
    showToast('Failed to update: ' + err.message, 'error');
  }
}

function emptyState(msg, btn) {
  return `
    <div class="empty-state">
      <div class="empty-emoji">🏆</div>
      <div class="empty-title">No challenges</div>
      <div class="empty-desc">${msg}</div>
      ${btn === 'available' ? '<button class="btn btn-primary mt-md" onclick="void(0)">Browse Available</button>' : ''}
    </div>
  `;
}

function skeleton(n) {
  return Array(n).fill('<div class="challenge-card skeleton mb-md" style="height:180px"></div>').join('');
}

function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
}
