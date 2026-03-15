/**
 * achievements.js — Achievement grid (locked + unlocked)
 */

import * as api from '../api.js';
import { setPageContent, showToast, escapeHtml } from '../app.js';

const CAT_ICONS = { Transport:'🚌', Food:'🥦', Energy:'⚡', Shopping:'🛍️', Travel:'✈️', Waste:'♻️', General:'🌍', Streak:'🔥', Social:'👥' };

export async function init() {
  setPageContent(`
    <div class="page">
      <div class="page-header" style="padding:0;margin-bottom:16px">
        <div class="page-title">Achievements</div>
      </div>
      <div id="achievements-content">
        <div class="achievement-grid">
          ${Array(8).fill('<div class="achievement-card skeleton" style="height:140px"></div>').join('')}
        </div>
      </div>
    </div>
  `);

  await loadAchievements();

  return () => {};
}

async function loadAchievements() {
  try {
    const [all, unlocked] = await Promise.all([
      api.getAllAchievements(),
      api.getUnlockedAchievements(),
    ]);

    const unlockedMap = {};
    (unlocked || []).forEach((u) => { unlockedMap[u.achievement.id] = u; });

    renderAchievements(all || [], unlockedMap);
  } catch (err) {
    document.getElementById('achievements-content').innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">🏆</div>
        <div class="empty-title">Achievements unavailable</div>
        <div class="empty-desc">${escapeHtml(err.message)}</div>
      </div>
    `;
  }
}

function renderAchievements(all, unlockedMap) {
  const el = document.getElementById('achievements-content');
  if (!el) return;

  const unlocked = all.filter((a) => unlockedMap[a.id]);
  const locked   = all.filter((a) => !unlockedMap[a.id]);

  let html = '';

  if (unlocked.length) {
    html += `
      <div class="section-header">
        <div class="section-title">Unlocked (${unlocked.length})</div>
        <span class="badge badge-success">🍃 ${unlocked.reduce((s, a) => s + (a.lpReward || 0), 0)} LP earned</span>
      </div>
      <div class="achievement-grid mb-lg">
        ${unlocked.map((a) => renderCard(a, unlockedMap[a.id], true)).join('')}
      </div>
    `;
  }

  if (locked.length) {
    html += `
      <div class="section-header">
        <div class="section-title">Locked (${locked.length})</div>
      </div>
      <div class="achievement-grid">
        ${locked.map((a) => renderCard(a, null, false)).join('')}
      </div>
    `;
  }

  if (!all.length) {
    html = `
      <div class="empty-state">
        <div class="empty-emoji">🏅</div>
        <div class="empty-title">No achievements yet</div>
        <div class="empty-desc">Complete eco-actions and habits to unlock achievements.</div>
      </div>
    `;
  }

  el.innerHTML = html;
}

function renderCard(achievement, unlock, isUnlocked) {
  const icon    = achievement.iconUrl || CAT_ICONS[achievement.category] || '🏅';
  const isEmoji = !achievement.iconUrl;
  const isNew   = unlock?.isNew;

  return `
    <div class="achievement-card ${isUnlocked ? '' : 'locked'}" ${isNew ? 'style="border:2px solid var(--color-accent)"' : ''}>
      ${isNew ? '<div class="badge badge-accent" style="margin-bottom:6px">New!</div>' : ''}
      <div class="achievement-icon">
        ${isEmoji ? icon : `<img src="${escapeHtml(icon)}" alt="" style="width:40px;height:40px" />`}
      </div>
      <div class="achievement-name">${escapeHtml(achievement.name)}</div>
      <div class="achievement-desc">${escapeHtml(achievement.description || '')}</div>
      <div style="margin-top:6px">
        <span class="badge badge-primary">🍃 ${achievement.lpReward} LP</span>
      </div>
      ${isUnlocked && unlock?.unlockedAt ? `
        <div style="font-size:10px;color:var(--color-text-secondary);margin-top:4px">${formatDate(unlock.unlockedAt)}</div>
      ` : ''}
    </div>
  `;
}

function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
}
