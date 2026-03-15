/**
 * db.js — IndexedDB wrapper for offline-first storage
 * Provides simple key-value style access with offline action queuing.
 */

const DB_NAME    = 'marble-companion';
const DB_VERSION = 1;

const STORES = ['habits', 'actions', 'tree', 'user', 'content', 'challenges', 'offline-queue'];

let _db = null;

function openDB() {
  if (_db) return Promise.resolve(_db);

  return new Promise((resolve, reject) => {
    const req = indexedDB.open(DB_NAME, DB_VERSION);

    req.onupgradeneeded = (event) => {
      const db = event.target.result;
      STORES.forEach((storeName) => {
        if (!db.objectStoreNames.contains(storeName)) {
          db.createObjectStore(storeName);
        }
      });
    };

    req.onsuccess  = (e) => { _db = e.target.result; resolve(_db); };
    req.onerror    = (e) => reject(e.target.error);
    req.onblocked  = ()  => reject(new Error('IndexedDB blocked'));
  });
}

/** Get a value by key from a store */
export async function dbGet(storeName, key) {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx  = db.transaction(storeName, 'readonly');
    const req = tx.objectStore(storeName).get(key);
    req.onsuccess = () => resolve(req.result);
    req.onerror   = () => reject(req.error);
  });
}

/** Set a value by key in a store */
export async function dbSet(storeName, key, value) {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx  = db.transaction(storeName, 'readwrite');
    const req = tx.objectStore(storeName).put(value, key);
    req.onsuccess = () => resolve();
    req.onerror   = () => reject(req.error);
  });
}

/** Delete a key from a store */
export async function dbDel(storeName, key) {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx  = db.transaction(storeName, 'readwrite');
    const req = tx.objectStore(storeName).delete(key);
    req.onsuccess = () => resolve();
    req.onerror   = () => reject(req.error);
  });
}

/** Get all values from a store */
export async function dbGetAll(storeName) {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx  = db.transaction(storeName, 'readonly');
    const req = tx.objectStore(storeName).getAll();
    req.onsuccess = () => resolve(req.result);
    req.onerror   = () => reject(req.error);
  });
}

/** Get all keys from a store */
export async function dbGetAllKeys(storeName) {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx  = db.transaction(storeName, 'readonly');
    const req = tx.objectStore(storeName).getAllKeys();
    req.onsuccess = () => resolve(req.result);
    req.onerror   = () => reject(req.error);
  });
}

/** Clear all entries from a store */
export async function dbClear(storeName) {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx  = db.transaction(storeName, 'readwrite');
    const req = tx.objectStore(storeName).clear();
    req.onsuccess = () => resolve();
    req.onerror   = () => reject(req.error);
  });
}

// ── Offline Queue ────────────────────────────────────────────────────────────

/**
 * Queue an action for later sync when the device is offline.
 * @param {object} item - { type: 'checkin'|'logAction', payload: {...} }
 */
export async function enqueueOffline(item) {
  const key = `${Date.now()}-${Math.random().toString(36).slice(2)}`;
  await dbSet('offline-queue', key, { ...item, key, queuedAt: new Date().toISOString() });
}

/**
 * Retrieve all queued offline items, ordered by insertion key.
 * @returns {Array<{ key, type, payload, queuedAt }>}
 */
export async function getOfflineQueue() {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx    = db.transaction('offline-queue', 'readonly');
    const store = tx.objectStore('offline-queue');
    const items = [];

    const cursor = store.openCursor();
    cursor.onsuccess = (e) => {
      const c = e.target.result;
      if (c) { items.push({ key: c.key, ...c.value }); c.continue(); }
      else resolve(items.sort((a, b) => a.key.localeCompare(b.key)));
    };
    cursor.onerror = () => reject(cursor.error);
  });
}

/**
 * Remove a processed item from the offline queue.
 */
export async function dequeueOffline(key) {
  await dbDel('offline-queue', key);
}

// ── Helpers: cache API responses locally ─────────────────────────────────────

export async function cacheHabits(habits)     { await dbSet('habits',     'active', habits); }
export async function cacheTree(tree)         { await dbSet('tree',       'current', tree); }
export async function cacheUser(user)         { await dbSet('user',       'profile', user); }
export async function cacheChallenges(data)   { await dbSet('challenges', 'all', data); }
export async function cacheContent(list)      { await dbSet('content',    'list', list); }

export async function getCachedHabits()       { return dbGet('habits',     'active'); }
export async function getCachedTree()         { return dbGet('tree',       'current'); }
export async function getCachedUser()         { return dbGet('user',       'profile'); }
export async function getCachedChallenges()   { return dbGet('challenges', 'all'); }
export async function getCachedContent()      { return dbGet('content',    'list'); }
