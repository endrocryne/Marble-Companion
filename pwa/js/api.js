/**
 * api.js — Marble Companion API Client
 * Handles all HTTP communication with the backend, JWT auth, and token refresh.
 */

// BASE_URL can be configured via window.API_BASE_URL (set in index.html config block)
// Defaults to empty string for same-origin deployments.
export const BASE_URL = (typeof window !== 'undefined' && window.API_BASE_URL) || '';

let _refreshing = null; // Singleton promise for in-flight refresh

function getToken() {
  return localStorage.getItem('mc_token');
}

function getRefreshToken() {
  return localStorage.getItem('mc_refresh');
}

function saveTokens(token, refreshToken, expiresAt) {
  localStorage.setItem('mc_token', token);
  localStorage.setItem('mc_refresh', refreshToken);
  localStorage.setItem('mc_expires', expiresAt);
}

function getHeaders(extra = {}) {
  const token = getToken();
  const headers = { 'Content-Type': 'application/json', ...extra };
  if (token) headers['Authorization'] = `Bearer ${token}`;
  return headers;
}

async function doRefresh() {
  const refreshToken = getRefreshToken();
  if (!refreshToken) throw new Error('No refresh token available');

  const res = await fetch(`${BASE_URL}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  });

  if (!res.ok) throw new Error('Token refresh failed');
  const data = await res.json();
  saveTokens(data.token, data.refreshToken, data.expiresAt);
  return data.token;
}

async function refreshTokenOnce() {
  if (!_refreshing) {
    _refreshing = doRefresh().finally(() => { _refreshing = null; });
  }
  return _refreshing;
}

async function request(method, path, body, opts = {}) {
  const url = `${BASE_URL}${path}`;
  const init = {
    method,
    headers: getHeaders(opts.extraHeaders || {}),
  };
  if (body !== undefined) init.body = JSON.stringify(body);

  let res = await fetch(url, init);

  // Auto-refresh on 401
  if (res.status === 401 && !opts._retried) {
    try {
      await refreshTokenOnce();
      init.headers = getHeaders(opts.extraHeaders || {});
      res = await fetch(url, init);
    } catch {
      // Refresh failed — clear auth and throw
      localStorage.removeItem('mc_token');
      localStorage.removeItem('mc_refresh');
      localStorage.removeItem('mc_user');
      window.dispatchEvent(new CustomEvent('mc:auth-expired'));
      throw new Error('Session expired. Please sign in again.');
    }
  }

  if (!res.ok) {
    let msg = `HTTP ${res.status}`;
    try {
      const err = await res.json();
      msg = err.message || err.title || JSON.stringify(err);
    } catch { /* ignore */ }
    throw new Error(msg);
  }

  if (res.status === 204) return null;
  return res.json();
}

const get  = (path, opts) => request('GET',    path, undefined, opts);
const post = (path, body, opts) => request('POST',   path, body, opts);
const put  = (path, body, opts) => request('PUT',    path, body, opts);
const del  = (path, opts) => request('DELETE', path, undefined, opts);

// ── Auth ────────────────────────────────────────────────────────────────────
export const authGoogle    = (idToken)      => post('/api/auth/google', { idToken });
export const authMicrosoft = (accessToken)  => post('/api/auth/microsoft', { accessToken });
export const authRefresh   = (refreshToken) => post('/api/auth/refresh', { refreshToken });
export const authLogout    = ()             => del('/api/auth/logout');

// ── Users ───────────────────────────────────────────────────────────────────
export const getMe         = ()     => get('/api/users/me');
export const updateMe      = (dto)  => put('/api/users/me', dto);
export const setupUser     = (dto)  => post('/api/users/me/setup', dto);
export const searchUsers   = (q)    => get(`/api/users/search?q=${encodeURIComponent(q)}`);

// ── Habits ──────────────────────────────────────────────────────────────────
export const getHabitLibrary  = ()     => get('/api/habits/library');
export const getActiveHabits  = ()     => get('/api/habits/active');
export const addHabit         = (dto)  => post('/api/habits', dto);
export const removeHabit      = (id)   => del(`/api/habits/${id}`);
export const checkinHabit     = (id)   => post(`/api/habits/${id}/checkin`);

// ── Actions ─────────────────────────────────────────────────────────────────
export const logAction        = (dto)           => post('/api/actions', dto);
export const getActions       = (from, to, cat) => {
  const p = new URLSearchParams();
  if (from) p.set('from', from);
  if (to)   p.set('to', to);
  if (cat)  p.set('category', cat);
  return get(`/api/actions?${p.toString()}`);
};
export const deleteAction     = (id)  => del(`/api/actions/${id}`);
export const getActionSummary = ()    => get('/api/actions/summary');

// ── Tree ────────────────────────────────────────────────────────────────────
export const getTree          = ()    => get('/api/tree');

// ── Friends ─────────────────────────────────────────────────────────────────
export const getFriends       = ()          => get('/api/friends');
export const sendFriendRequest = (username) => post('/api/friends/request', { username });
export const acceptFriendRequest = (id)     => put(`/api/friends/request/${id}/accept`);
export const removeFriend     = (id)        => del(`/api/friends/${id}`);
export const getFeed          = ()          => get('/api/friends/feed');
export const reactToFeed      = (eventId, reactionType) => post(`/api/friends/feed/${eventId}/react`, { reactionType });

// ── Challenges ──────────────────────────────────────────────────────────────
export const getCuratedChallenges = ()    => get('/api/challenges/curated');
export const getMyChallenges      = ()    => get('/api/challenges/mine');
export const createChallenge      = (dto) => post('/api/challenges', dto);
export const joinChallenge        = (id)  => post(`/api/challenges/${id}/join`);
export const updateChallengeProgress = (id, progress) => post(`/api/challenges/${id}/progress`, { progress });

// ── Achievements ─────────────────────────────────────────────────────────────
export const getAllAchievements     = () => get('/api/achievements');
export const getUnlockedAchievements = () => get('/api/achievements/unlocked');

// ── Content ─────────────────────────────────────────────────────────────────
export const getContentList   = ()   => get('/api/content');
export const getTodayContent  = ()   => get('/api/content/today');
export const getContentDetail = (id) => get(`/api/content/${id}`);
export const bookmarkContent  = (id) => post(`/api/content/${id}/bookmark`);

// ── Offsets ──────────────────────────────────────────────────────────────────
export const getOffsetCredits  = ()    => get('/api/offsets/credits');
export const redeemOffset      = (dto) => post('/api/offsets/redeem', dto);
export const getOffsetHistory  = ()    => get('/api/offsets/history');

// ── Notifications ─────────────────────────────────────────────────────────────
export const updateNotificationPrefs = (dto) => put('/api/notifications/preferences', dto);
