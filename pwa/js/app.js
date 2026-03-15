/**
 * app.js — Main application entry point
 * Registers service worker, implements hash router, manages navigation.
 */

import * as auth from './auth.js';

// ── Service Worker Registration ───────────────────────────────────────────────
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('./sw.js').then((reg) => {
      console.log('[SW] Registered:', reg.scope);
    }).catch((err) => {
      console.warn('[SW] Registration failed:', err);
    });
  });

  // Listen for sync messages from SW
  navigator.serviceWorker.addEventListener('message', (event) => {
    if (event.data?.type === 'SYNC_QUEUE') flushOfflineQueue();
  });
}

// ── Online/Offline Detection ──────────────────────────────────────────────────
window.addEventListener('online',  () => { showToast('Back online 🌐', 'success'); flushOfflineQueue(); });
window.addEventListener('offline', () => { showToast('You\'re offline — changes will sync later', 'warning'); });

// ── Auth Expiry ───────────────────────────────────────────────────────────────
window.addEventListener('mc:auth-expired', () => navigate('onboarding'));

// ── Page Registry ─────────────────────────────────────────────────────────────
const PAGE_MODULES = {
  onboarding:       () => import('./pages/onboarding.js'),
  setup:            () => import('./pages/setup.js'),
  home:             () => import('./pages/home.js'),
  dashboard:        () => import('./pages/dashboard.js'),
  habits:           () => import('./pages/habits.js'),
  'habit-library':  () => import('./pages/habit-library.js'),
  log:              () => import('./pages/log.js'),
  social:           () => import('./pages/social.js'),
  feed:             () => import('./pages/social.js'),
  friends:          () => import('./pages/social.js'),
  challenges:       () => import('./pages/challenges.js'),
  'challenge-detail': () => import('./pages/challenges.js'),
  achievements:     () => import('./pages/achievements.js'),
  content:          () => import('./pages/content.js'),
  'content-detail': () => import('./pages/content-detail.js'),
  offsets:          () => import('./pages/offsets.js'),
  profile:          () => import('./pages/profile.js'),
  settings:         () => import('./pages/settings.js'),
  suggestions:      () => import('./pages/suggestions.js'),
};

// Tab routes that are top-level nav tabs
const NAV_ROUTES = ['home', 'dashboard', 'habits', 'social', 'profile'];

// Routes that require auth
const AUTH_ROUTES = new Set([
  'home','dashboard','habits','habit-library','log','social','feed','friends',
  'challenges','challenge-detail','achievements','content','content-detail',
  'offsets','profile','settings','suggestions'
]);

// Routes that should hide the bottom nav
const HIDE_NAV_ROUTES = new Set(['onboarding', 'setup']);

let _currentPage = null;
let _currentCleanup = null;

// ── Navigation ────────────────────────────────────────────────────────────────

/** Navigate to a route: navigate('home') or navigate('content-detail', { id: '123' }) */
export function navigate(route, params = {}) {
  const qs = new URLSearchParams(params).toString();
  window.location.hash = qs ? `#/${route}?${qs}` : `#/${route}`;
}

function parseHash() {
  const hash = window.location.hash || '#/onboarding';
  const [pathPart, qsPart] = hash.slice(2).split('?');
  const route  = pathPart || 'onboarding';
  const params = Object.fromEntries(new URLSearchParams(qsPart || ''));
  return { route, params };
}

async function handleRoute() {
  const { route, params } = parseHash();

  // Auth guard
  if (AUTH_ROUTES.has(route) && !auth.isAuthenticated()) {
    window.location.hash = '#/onboarding';
    return;
  }

  // Redirect authenticated users away from onboarding
  if (route === 'onboarding' && auth.isAuthenticated()) {
    window.location.hash = auth.needsSetup() ? '#/setup' : '#/home';
    return;
  }

  // Redirect to setup if needed
  if (AUTH_ROUTES.has(route) && route !== 'setup' && auth.isAuthenticated() && auth.needsSetup()) {
    window.location.hash = '#/setup';
    return;
  }

  // Update bottom nav visibility
  const nav = document.getElementById('bottom-nav');
  if (HIDE_NAV_ROUTES.has(route)) {
    nav?.classList.add('hidden');
    document.getElementById('page-container')?.classList.add('no-nav');
  } else {
    nav?.classList.remove('hidden');
    document.getElementById('page-container')?.classList.remove('no-nav');
  }

  // Update active nav tab
  updateNavTab(route);

  // Run cleanup of previous page
  if (typeof _currentCleanup === 'function') {
    try { _currentCleanup(); } catch { /* ignore */ }
    _currentCleanup = null;
  }

  // Load and render the new page
  const loader = PAGE_MODULES[route];
  if (!loader) {
    renderNotFound(route);
    return;
  }

  showLoading(true);
  try {
    const mod = await loader();
    const pageParams = { ...params, route };

    // Handle social tab switching
    if (route === 'feed')    pageParams.tab = 'feed';
    if (route === 'friends') pageParams.tab = 'friends';
    if (route === 'challenge-detail') pageParams.id = params.id;
    if (route === 'content-detail')   pageParams.id = params.id;

    if (typeof mod.init === 'function') {
      _currentCleanup = await mod.init(pageParams) || null;
    }
    _currentPage = route;
  } catch (err) {
    console.error('[Router] Page load error:', err);
    renderError(err.message);
  } finally {
    showLoading(false);
  }
}

function updateNavTab(route) {
  document.querySelectorAll('.nav-tab').forEach((tab) => {
    const tabRoute = tab.dataset.route;
    const isActive = tabRoute === route ||
      (route === 'feed' && tabRoute === 'social') ||
      (route === 'friends' && tabRoute === 'social');
    tab.classList.toggle('active', isActive);
  });
}

function renderNotFound(route) {
  setPageContent(`
    <div class="page">
      <div class="empty-state">
        <div class="empty-emoji">🗺️</div>
        <div class="empty-title">Page not found</div>
        <div class="empty-desc">Route "/${route}" doesn't exist.</div>
        <button class="btn btn-primary mt-lg" onclick="window.location.hash='#/home'">Go Home</button>
      </div>
    </div>
  `);
}

function renderError(message) {
  setPageContent(`
    <div class="page">
      <div class="empty-state">
        <div class="empty-emoji">⚠️</div>
        <div class="empty-title">Something went wrong</div>
        <div class="empty-desc">${escapeHtml(message)}</div>
        <button class="btn btn-primary mt-lg" onclick="window.location.hash='#/home'">Go Home</button>
      </div>
    </div>
  `);
}

// ── Public Helpers ────────────────────────────────────────────────────────────

/** Replace the content of #page-content */
export function setPageContent(html) {
  const el = document.getElementById('page-content');
  if (el) el.innerHTML = html;
}

/** Show or hide the full-screen loading overlay */
export function showLoading(visible) {
  const el = document.getElementById('loading-overlay');
  if (el) el.classList.toggle('hidden', !visible);
}

/** Show a toast notification */
export function showToast(message, type = 'info') {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  toast.textContent = message;
  container.appendChild(toast);

  setTimeout(() => toast.remove(), 3200);
}

/** Show a full-page LP reward popup */
export function showLPPopup(lp, co2e) {
  const backdrop = document.createElement('div');
  backdrop.className = 'modal-backdrop center';
  backdrop.innerHTML = `
    <div class="lp-popup" id="lp-popup-inner">
      <div class="lp-popup-emoji">🌿</div>
      <div class="lp-popup-title">Action Logged!</div>
      <div class="lp-popup-lp">+${lp} LP</div>
      <div class="lp-popup-co2">${co2e > 0 ? `${co2e.toFixed(2)} kg CO₂e saved` : ''}</div>
      <button class="btn btn-primary mt-md" id="lp-popup-close">Awesome!</button>
    </div>
  `;
  document.body.appendChild(backdrop);
  requestAnimationFrame(() => document.getElementById('lp-popup-inner')?.classList.add('show'));
  backdrop.querySelector('#lp-popup-close').addEventListener('click', () => backdrop.remove());
  backdrop.addEventListener('click', (e) => { if (e.target === backdrop) backdrop.remove(); });
}

/** Escape HTML to prevent XSS when setting innerHTML */
export function escapeHtml(str) {
  return String(str ?? '').replace(/[&<>"']/g, (c) => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;',
  }[c]));
}

/** Format ISO date to relative time string */
export function timeAgo(iso) {
  const diff = Date.now() - new Date(iso).getTime();
  const m = Math.floor(diff / 60000);
  if (m < 1)   return 'just now';
  if (m < 60)  return `${m}m ago`;
  const h = Math.floor(m / 60);
  if (h < 24)  return `${h}h ago`;
  const d = Math.floor(h / 24);
  return `${d}d ago`;
}

// ── Offline Queue Flush ───────────────────────────────────────────────────────
async function flushOfflineQueue() {
  try {
    const { getOfflineQueue, dequeueOffline } = await import('./db.js');
    const { checkinHabit, logAction } = await import('./api.js');
    const queue = await getOfflineQueue();
    for (const item of queue) {
      try {
        if (item.type === 'checkin') await checkinHabit(item.payload.habitId);
        if (item.type === 'logAction') await logAction(item.payload);
        await dequeueOffline(item.key);
      } catch { /* keep in queue if it fails */ }
    }
    if (queue.length > 0) showToast(`Synced ${queue.length} offline action(s)`, 'success');
  } catch { /* ignore if db not available */ }
}

// ── Nav Tabs Click ────────────────────────────────────────────────────────────
function bindNavTabs() {
  document.querySelectorAll('.nav-tab').forEach((tab) => {
    tab.addEventListener('click', () => navigate(tab.dataset.route));
  });
}

// ── Init ──────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
  bindNavTabs();
  window.addEventListener('hashchange', handleRoute);
  handleRoute();
});

// Expose to window for inline onclick handlers in page HTML
window._app = { navigate, showToast, showLPPopup, escapeHtml, timeAgo };
