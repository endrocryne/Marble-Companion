// CACHE_VERSION controls cache invalidation. Bump this string on every deployment.
// In a CI/CD pipeline you can replace it automatically:
//   sed -i "s/mc-v1/mc-$(git rev-parse --short HEAD)/" sw.js
// or inject it as a build-time environment variable.
const CACHE_VERSION = 'mc-v1';
const STATIC_CACHE  = `${CACHE_VERSION}-static`;
const API_CACHE     = `${CACHE_VERSION}-api`;

const STATIC_ASSETS = [
  './index.html',
  './css/app.css',
  './js/app.js',
  './js/api.js',
  './js/auth.js',
  './js/db.js',
  './js/tree-canvas.js',
  './js/pages/onboarding.js',
  './js/pages/setup.js',
  './js/pages/home.js',
  './js/pages/dashboard.js',
  './js/pages/habits.js',
  './js/pages/habit-library.js',
  './js/pages/log.js',
  './js/pages/social.js',
  './js/pages/challenges.js',
  './js/pages/achievements.js',
  './js/pages/content.js',
  './js/pages/content-detail.js',
  './js/pages/offsets.js',
  './js/pages/profile.js',
  './js/pages/settings.js',
  './js/pages/suggestions.js',
  './manifest.json',
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(STATIC_CACHE).then((cache) => {
      return cache.addAll(STATIC_ASSETS).catch((err) => {
        console.warn('[SW] Failed to cache some assets:', err);
      });
    }).then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys
          .filter((key) => key.startsWith('mc-') && key !== STATIC_CACHE && key !== API_CACHE)
          .map((key) => caches.delete(key))
      );
    }).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  // Skip non-GET requests and browser-extension requests
  if (request.method !== 'GET') return;
  if (!url.protocol.startsWith('http')) return;

  // API calls: network-first strategy
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(networkFirst(request));
    return;
  }

  // CDN resources: cache-first
  if (url.hostname !== self.location.hostname) {
    event.respondWith(cacheFirst(request));
    return;
  }

  // Static assets: cache-first with offline fallback
  event.respondWith(cacheFirstWithFallback(request));
});

async function networkFirst(request) {
  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(API_CACHE);
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    const cached = await caches.match(request);
    if (cached) return cached;
    return new Response(JSON.stringify({ error: 'Offline', offline: true }), {
      status: 503,
      headers: { 'Content-Type': 'application/json' },
    });
  }
}

async function cacheFirst(request) {
  const cached = await caches.match(request);
  if (cached) return cached;
  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(STATIC_CACHE);
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    return new Response('Offline', { status: 503 });
  }
}

async function cacheFirstWithFallback(request) {
  const cached = await caches.match(request);
  if (cached) return cached;
  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(STATIC_CACHE);
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    // Fall back to cached index.html for navigation requests
    if (request.mode === 'navigate') {
      const fallback = await caches.match('./index.html');
      if (fallback) return fallback;
    }
    return new Response('Offline', { status: 503 });
  }
}

// Background sync for queued actions
self.addEventListener('sync', (event) => {
  if (event.tag === 'sync-actions') {
    event.waitUntil(syncQueuedActions());
  }
});

async function syncQueuedActions() {
  // Notify all clients to flush their offline queues
  const clients = await self.clients.matchAll();
  clients.forEach((client) => client.postMessage({ type: 'SYNC_QUEUE' }));
}
