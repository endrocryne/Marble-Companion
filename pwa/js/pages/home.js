/**
 * home.js — Home page with companion tree, stats, and today's fact
 */

import * as api from '../api.js';
import { drawTree, stopTree } from '../tree-canvas.js';
import { setPageContent, showToast, navigate, escapeHtml } from '../app.js';
import { getUser } from '../auth.js';
import { cacheTree, getCachedTree } from '../db.js';

let _canvas = null;
let _refreshHandler = null;

export async function init() {
  const user = getUser();

  setPageContent(`
    <div class="page" style="padding-top:0">
      <!-- Header -->
      <div style="background:linear-gradient(135deg,#0D7377,#14A085);padding:20px 16px 0;color:#fff;border-radius:0 0 24px 24px;margin-bottom:16px">
        <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:16px">
          <div>
            <div style="font-size:13px;opacity:0.8">Welcome back</div>
            <div style="font-size:20px;font-weight:800">${escapeHtml(user?.displayName || 'Explorer')} 👋</div>
          </div>
          <div class="avatar avatar-${user?.avatarIndex ?? 0}" style="width:44px;height:44px;font-size:18px">
            ${['🌿','🌊','🌻','🦋','🦜','🌙','❄️','🔥'][user?.avatarIndex ?? 0]}
          </div>
        </div>
        <!-- Stat badges -->
        <div style="display:flex;gap:8px;padding-bottom:20px">
          <div style="flex:1;background:rgba(255,255,255,0.15);border-radius:12px;padding:10px;text-align:center">
            <div style="font-size:22px;font-weight:800" id="stat-streak">--</div>
            <div style="font-size:11px;opacity:0.8;font-weight:600">🔥 Day Streak</div>
          </div>
          <div style="flex:1;background:rgba(255,255,255,0.15);border-radius:12px;padding:10px;text-align:center">
            <div style="font-size:22px;font-weight:800" id="stat-lp">--</div>
            <div style="font-size:11px;opacity:0.8;font-weight:600">🍃 Leaf Points</div>
          </div>
          <div style="flex:1;background:rgba(255,255,255,0.15);border-radius:12px;padding:10px;text-align:center">
            <div style="font-size:22px;font-weight:800" id="stat-co2">--</div>
            <div style="font-size:11px;opacity:0.8;font-weight:600">🌍 kg CO₂e saved</div>
          </div>
        </div>
      </div>

      <!-- Tree -->
      <div style="padding:0 16px">
        <div class="section-header">
          <div class="section-title">Your Companion Tree</div>
          <span id="tree-stage-badge" class="badge badge-success">Loading…</span>
        </div>
        <div class="tree-container" style="height:260px;margin-bottom:16px">
          <canvas id="tree-canvas" class="tree-canvas" style="height:260px"></canvas>
          <div class="tree-stats">
            <div class="tree-badge" id="tree-species">🌳 —</div>
            <div class="tree-badge" id="tree-health">💚 Healthy</div>
          </div>
        </div>

        <!-- Today's Fact -->
        <div class="fact-card" id="fact-card">
          <div class="fact-label">💡 Did You Know?</div>
          <div class="fact-text" id="fact-text">Loading today's eco tip…</div>
        </div>

        <!-- Quick Actions -->
        <div class="section-header mt-md">
          <div class="section-title">Quick Actions</div>
        </div>
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;margin-bottom:16px">
          <button class="card" style="text-align:center;cursor:pointer;border:none;padding:16px" onclick="window._app.navigate('habits')">
            <div style="font-size:32px;margin-bottom:8px">✅</div>
            <div style="font-size:13px;font-weight:700;color:var(--color-text-primary)">Check In Habits</div>
          </button>
          <button class="card" style="text-align:center;cursor:pointer;border:none;padding:16px" onclick="window._app.navigate('challenges')">
            <div style="font-size:32px;margin-bottom:8px">🏆</div>
            <div style="font-size:13px;font-weight:700;color:var(--color-text-primary)">Challenges</div>
          </button>
          <button class="card" style="text-align:center;cursor:pointer;border:none;padding:16px" onclick="window._app.navigate('content')">
            <div style="font-size:32px;margin-bottom:8px">📚</div>
            <div style="font-size:13px;font-weight:700;color:var(--color-text-primary)">Learn & Earn</div>
          </button>
          <button class="card" style="text-align:center;cursor:pointer;border:none;padding:16px" onclick="window._app.navigate('offsets')">
            <div style="font-size:32px;margin-bottom:8px">🌱</div>
            <div style="font-size:13px;font-weight:700;color:var(--color-text-primary)">Offset Credits</div>
          </button>
        </div>
      </div>
    </div>

    <!-- FAB -->
    <button class="fab" id="fab-log" aria-label="Log Action" onclick="window._app.navigate('log')">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
        <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
      </svg>
    </button>
  `);

  _canvas = document.getElementById('tree-canvas');
  loadData();
  setupPullToRefresh();

  return cleanup;
}

function cleanup() {
  if (_canvas) stopTree(_canvas);
  if (_refreshHandler) {
    document.getElementById('page-container')?.removeEventListener('touchstart', _refreshHandler);
  }
}

async function loadData() {
  // Load from cache first for instant display
  const cached = await getCachedTree();
  if (cached) renderTree(cached);

  try {
    const [tree, user, fact, summary] = await Promise.allSettled([
      api.getTree(),
      api.getMe(),
      api.getTodayContent(),
      api.getActionSummary(),
    ]);

    if (tree.status === 'fulfilled') {
      renderTree(tree.value);
      cacheTree(tree.value);
    }

    if (user.status === 'fulfilled') {
      const u = user.value;
      const streakEl = document.getElementById('stat-streak');
      const lpEl     = document.getElementById('stat-lp');
      if (streakEl) streakEl.textContent = u.currentStreak ?? 0;
      if (lpEl)     lpEl.textContent     = u.totalLP?.toLocaleString() ?? 0;
    }

    if (summary.status === 'fulfilled') {
      const co2El = document.getElementById('stat-co2');
      if (co2El) co2El.textContent = (summary.value.totalCO2eSaved ?? 0).toFixed(1);
    }

    if (fact.status === 'fulfilled' && fact.value) {
      const factEl = document.getElementById('fact-text');
      if (factEl) factEl.textContent = fact.value.summary || fact.value.title;
    } else {
      const factEl = document.getElementById('fact-text');
      if (factEl) factEl.textContent = 'Walking or cycling instead of driving just once a week can save over 100 kg CO₂e per year!';
    }
  } catch (err) {
    console.warn('[home] Data load error:', err);
  }
}

function renderTree(treeDto) {
  const stageBadge  = document.getElementById('tree-stage-badge');
  const speciesBadge = document.getElementById('tree-species');
  const healthBadge  = document.getElementById('tree-health');

  if (stageBadge)  stageBadge.textContent  = treeDto.stageName || `Stage ${treeDto.stage}`;
  if (speciesBadge) speciesBadge.textContent = `🌳 ${treeDto.species || 'Oak'}`;
  if (healthBadge)  healthBadge.textContent  = `${healthEmoji(treeDto.healthState)} ${treeDto.healthState || 'Healthy'}`;

  if (_canvas) drawTree(_canvas, treeDto);
}

function healthEmoji(state) {
  return { Healthy: '💚', Stressed: '💛', Withering: '🟠', Dormant: '❄️' }[state] || '💚';
}

function setupPullToRefresh() {
  let startY = 0;
  const container = document.getElementById('page-container');
  if (!container) return;

  const touchStart = (e) => { startY = e.touches[0].clientY; };
  const touchEnd   = (e) => {
    if (container.scrollTop === 0 && e.changedTouches[0].clientY - startY > 60) {
      loadData();
      showToast('Refreshing…', 'info');
    }
  };

  container.addEventListener('touchstart', touchStart, { passive: true });
  container.addEventListener('touchend',   touchEnd,   { passive: true });
  _refreshHandler = touchStart;
}
