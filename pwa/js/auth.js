/**
 * auth.js — Authentication state management for Marble Companion
 */

import * as api from './api.js';

const KEY_TOKEN   = 'mc_token';
const KEY_REFRESH = 'mc_refresh';
const KEY_EXPIRES = 'mc_expires';
const KEY_USER    = 'mc_user';

/** Returns true if a valid (non-expired) token exists */
export function isAuthenticated() {
  const token   = localStorage.getItem(KEY_TOKEN);
  const expires = localStorage.getItem(KEY_EXPIRES);
  if (!token) return false;
  if (expires && Date.now() > new Date(expires).getTime()) return false;
  return true;
}

/** Returns the cached user profile object, or null */
export function getUser() {
  const raw = localStorage.getItem(KEY_USER);
  if (!raw) return null;
  try { return JSON.parse(raw); } catch { return null; }
}

/** Persists auth tokens and user profile to localStorage */
export function saveAuth(token, refreshToken, expiresAt, user) {
  localStorage.setItem(KEY_TOKEN,   token);
  localStorage.setItem(KEY_REFRESH, refreshToken);
  localStorage.setItem(KEY_EXPIRES, expiresAt);
  localStorage.setItem(KEY_USER,    JSON.stringify(user));
}

/** Wipes all auth data from localStorage */
export function clearAuth() {
  [KEY_TOKEN, KEY_REFRESH, KEY_EXPIRES, KEY_USER].forEach((k) =>
    localStorage.removeItem(k)
  );
}

/**
 * Returns true if the authenticated user still needs to complete the
 * onboarding setup flow (displayName not yet set / empty).
 */
export function needsSetup() {
  const user = getUser();
  if (!user) return false;
  return !user.displayName || user.displayName.trim() === '';
}

/**
 * Sign in with a Google ID token obtained from the Google Identity Services SDK.
 * Saves auth data and returns the user profile.
 */
export async function signInWithGoogle(idToken) {
  const data = await api.authGoogle(idToken);
  saveAuth(data.token, data.refreshToken, data.expiresAt, data.user);
  return data.user;
}

/**
 * Sign in with a Microsoft access token obtained from MSAL.
 * Saves auth data and returns the user profile.
 */
export async function signInWithMicrosoft(accessToken) {
  const data = await api.authMicrosoft(accessToken);
  saveAuth(data.token, data.refreshToken, data.expiresAt, data.user);
  return data.user;
}

/**
 * Sign out: calls the API logout endpoint and clears local state.
 */
export async function logout() {
  try {
    await api.authLogout();
  } catch {
    // Ignore API errors — clear locally regardless
  } finally {
    clearAuth();
  }
}
