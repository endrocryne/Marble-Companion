/**
 * social.js — Feed + Friends combined social page
 */

import * as api from '../api.js';
import { setPageContent, showToast, escapeHtml, timeAgo } from '../app.js';

const REACTIONS = [
  { type: 'Inspired', emoji: '💪', label: 'Inspired' },
  { type: 'Strong',   emoji: '💚', label: 'Strong'   },
  { type: 'Together', emoji: '🤝', label: 'Together'  },
  { type: 'Fire',     emoji: '🔥', label: 'Fire'      },
];

const EVENT_ICONS = {
  MilestoneAchievement: '🏆',
  ChallengeCompletion:  '🎯',
  StreakMilestone:      '🔥',
  TreeAdvancement:      '🌳',
};

const AVATAR_EMOJIS = ['🌿','🌊','🌻','🦋','🦜','🌙','❄️','🔥'];

let _activeTab = 'feed';

export async function init(params = {}) {
  if (params.tab) _activeTab = params.tab;

  setPageContent(`
    <div class="page">
      <div class="page-title mb-md">Social</div>
      <div class="tab-bar">
        <button class="tab-btn ${_activeTab === 'feed' ? 'active' : ''}" id="tab-feed">Activity Feed</button>
        <button class="tab-btn ${_activeTab === 'friends' ? 'active' : ''}" id="tab-friends">Friends</button>
      </div>
      <div id="tab-panel-feed" class="tab-panel ${_activeTab === 'feed' ? 'active' : ''}">
        <div id="feed-content">${skeleton(3, 120)}</div>
      </div>
      <div id="tab-panel-friends" class="tab-panel ${_activeTab === 'friends' ? 'active' : ''}">
        <div id="friends-content">${skeleton(4, 80)}</div>
      </div>
    </div>
  `);

  document.getElementById('tab-feed')?.addEventListener('click',    () => switchTab('feed'));
  document.getElementById('tab-friends')?.addEventListener('click', () => switchTab('friends'));

  loadCurrentTab();

  return () => {};
}

function switchTab(tab) {
  _activeTab = tab;
  document.getElementById('tab-feed').classList.toggle('active', tab === 'feed');
  document.getElementById('tab-friends').classList.toggle('active', tab === 'friends');
  document.getElementById('tab-panel-feed').classList.toggle('active', tab === 'feed');
  document.getElementById('tab-panel-friends').classList.toggle('active', tab === 'friends');
  loadCurrentTab();
}

function loadCurrentTab() {
  if (_activeTab === 'feed')    loadFeed();
  if (_activeTab === 'friends') loadFriends();
}

// ── Feed ──────────────────────────────────────────────────────────────────────
async function loadFeed() {
  try {
    const feed = await api.getFeed();
    renderFeed(feed);
  } catch (err) {
    document.getElementById('feed-content').innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">📭</div>
        <div class="empty-title">No feed yet</div>
        <div class="empty-desc">Add friends and start completing eco-actions to see activity here.</div>
      </div>
    `;
  }
}

function renderFeed(items) {
  const el = document.getElementById('feed-content');
  if (!el) return;

  if (!items?.length) {
    el.innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">🌱</div>
        <div class="empty-title">Feed is empty</div>
        <div class="empty-desc">Your friends' eco-actions will appear here once you connect.</div>
      </div>
    `;
    return;
  }

  el.innerHTML = items.map((event) => `
    <div class="feed-item" data-event-id="${event.id}">
      <div class="feed-header">
        <div class="avatar avatar-sm avatar-${event.avatarIndex ?? 0}">
          ${AVATAR_EMOJIS[event.avatarIndex ?? 0]}
        </div>
        <div class="feed-meta">
          <div class="feed-name">${escapeHtml(event.displayName)}</div>
          <div class="feed-time">${timeAgo(event.occurredAt)}</div>
        </div>
        <span style="font-size:24px">${EVENT_ICONS[event.eventType] || '🌿'}</span>
      </div>
      <div class="feed-title">${escapeHtml(event.title)}</div>
      <div class="feed-content">${escapeHtml(event.description || '')}</div>
      <div class="reactions-row">
        ${REACTIONS.map((r) => {
          const count  = event.reactions?.[r.type] ?? 0;
          const active = event.userReaction === r.type;
          return `
            <button class="reaction-btn ${active ? 'active' : ''}" data-event="${event.id}" data-reaction="${r.type}">
              ${r.emoji} ${count > 0 ? count : ''}
            </button>
          `;
        }).join('')}
      </div>
    </div>
  `).join('');

  el.querySelectorAll('.reaction-btn').forEach((btn) => {
    btn.addEventListener('click', () => handleReact(btn.dataset.event, btn.dataset.reaction));
  });
}

async function handleReact(eventId, reactionType) {
  try {
    await api.reactToFeed(eventId, reactionType);
    // Refresh feed after reacting
    await loadFeed();
  } catch (err) {
    showToast('Could not react: ' + err.message, 'error');
  }
}

// ── Friends ───────────────────────────────────────────────────────────────────
async function loadFriends() {
  try {
    const friends = await api.getFriends();
    renderFriends(friends);
  } catch (err) {
    document.getElementById('friends-content').innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">👥</div>
        <div class="empty-title">No friends yet</div>
        <div class="empty-desc">Search for people to connect with.</div>
      </div>
    `;
  }
}

function renderFriends(friends) {
  const el = document.getElementById('friends-content');
  if (!el) return;

  el.innerHTML = `
    <!-- Add Friend Search -->
    <div class="card mb-md">
      <div class="card-title mb-md">Add a Friend</div>
      <div style="display:flex;gap:8px">
        <input type="text" class="form-input" id="friend-search" placeholder="Search by username…" style="flex:1" />
        <button class="btn btn-primary" id="btn-friend-search">Search</button>
      </div>
      <div id="friend-search-results" style="margin-top:12px"></div>
    </div>

    <!-- Friends List -->
    <div class="section-title mb-md">Your Friends (${friends.length})</div>
    ${friends.length === 0 ? `
      <div class="empty-state" style="padding:24px">
        <div class="empty-emoji">👋</div>
        <div class="empty-title">No friends yet</div>
        <div class="empty-desc">Search above to find and add friends!</div>
      </div>
    ` : friends.map((f) => `
      <div class="friend-card">
        <div class="avatar avatar-${f.avatarIndex ?? 0}">${AVATAR_EMOJIS[f.avatarIndex ?? 0]}</div>
        <div class="friend-info">
          <div class="friend-name">${escapeHtml(f.displayName)}</div>
          <div class="friend-stats">🔥 ${f.currentStreak} streak · 🍃 ${f.totalLP?.toLocaleString()} LP</div>
          <div class="friend-stats" style="font-size:11px">Friends since ${formatDate(f.friendsSince)}</div>
        </div>
        <button class="btn btn-sm btn-ghost" style="color:var(--color-error)" data-friend-id="${f.userId}" onclick="void(0)">Remove</button>
      </div>
    `).join('')}
  `;

  document.getElementById('btn-friend-search')?.addEventListener('click', handleFriendSearch);
  document.getElementById('friend-search')?.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') handleFriendSearch();
  });

  el.querySelectorAll('[data-friend-id]').forEach((btn) => {
    btn.addEventListener('click', () => handleRemoveFriend(btn.dataset.friendId));
  });
}

async function handleFriendSearch() {
  const q = document.getElementById('friend-search')?.value?.trim();
  if (!q) return;

  const resultsEl = document.getElementById('friend-search-results');
  if (resultsEl) resultsEl.innerHTML = '<div class="spinner spinner-sm"></div>';

  try {
    const results = await api.searchUsers(q);
    if (!results?.length) {
      resultsEl.innerHTML = `<div style="font-size:14px;color:var(--color-text-secondary)">No users found for "${escapeHtml(q)}"</div>`;
      return;
    }
    resultsEl.innerHTML = results.map((u) => `
      <div class="friend-card" style="margin-bottom:8px">
        <div class="avatar avatar-sm avatar-${u.avatarIndex ?? 0}">${AVATAR_EMOJIS[u.avatarIndex ?? 0]}</div>
        <div class="friend-info">
          <div class="friend-name">${escapeHtml(u.displayName)}</div>
          <div class="friend-stats">🍃 ${u.totalLP?.toLocaleString()} LP</div>
        </div>
        <button class="btn btn-sm btn-primary add-friend-btn" data-username="${escapeHtml(u.displayName)}">Add</button>
      </div>
    `).join('');

    resultsEl.querySelectorAll('.add-friend-btn').forEach((btn) => {
      btn.addEventListener('click', () => handleSendRequest(btn.dataset.username, btn));
    });
  } catch (err) {
    resultsEl.innerHTML = `<div style="font-size:14px;color:var(--color-error)">${escapeHtml(err.message)}</div>`;
  }
}

async function handleSendRequest(username, btn) {
  btn.disabled = true;
  btn.textContent = '…';
  try {
    await api.sendFriendRequest(username);
    btn.textContent = 'Sent ✓';
    showToast('Friend request sent!', 'success');
  } catch (err) {
    btn.disabled = false;
    btn.textContent = 'Add';
    showToast('Failed: ' + err.message, 'error');
  }
}

async function handleRemoveFriend(id) {
  if (!confirm('Remove this friend?')) return;
  try {
    await api.removeFriend(id);
    showToast('Friend removed', 'info');
    await loadFriends();
  } catch (err) {
    showToast('Failed: ' + err.message, 'error');
  }
}

function skeleton(n, h) {
  return Array(n).fill(`<div class="skeleton mb-md" style="height:${h}px;border-radius:12px"></div>`).join('');
}

function formatDate(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short' });
}
