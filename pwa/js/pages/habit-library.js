/**
 * habit-library.js — Searchable library of all available habits
 */

import * as api from '../api.js';
import { setPageContent, showToast, escapeHtml } from '../app.js';

const CAT_ICONS  = { Transport:'🚌', Food:'🥦', Energy:'⚡', Shopping:'🛍️', Travel:'✈️', Waste:'♻️' };
const CAT_COLORS = { Transport:'#3B82F6', Food:'#22C55E', Energy:'#F59E0B', Shopping:'#A855F7', Travel:'#EC4899', Waste:'#06B6D4' };
const CATEGORIES = ['All', 'Transport', 'Food', 'Energy', 'Shopping', 'Travel', 'Waste'];

let _library  = [];
let _filter   = 'All';
let _query    = '';
let _adding   = new Set();

export async function init() {
  setPageContent(`
    <div class="page">
      <div style="display:flex;align-items:center;gap:12px;margin-bottom:16px">
        <button class="btn btn-icon btn-ghost" onclick="history.back()" aria-label="Back">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="15,18 9,12 15,6"/>
          </svg>
        </button>
        <div class="page-title">Habit Library</div>
      </div>

      <!-- Search -->
      <div class="search-bar">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
        </svg>
        <input type="text" class="search-input" id="search-input" placeholder="Search habits…" />
      </div>

      <!-- Category Filter -->
      <div class="chip-group" id="cat-filter" style="margin-bottom:16px">
        ${CATEGORIES.map((c) => `
          <div class="chip ${c === _filter ? 'active' : ''}" data-cat="${c}">${c === 'All' ? 'All' : (CAT_ICONS[c] || '') + ' ' + c}</div>
        `).join('')}
      </div>

      <!-- Library List -->
      <div id="library-list">
        ${skeletonList(8)}
      </div>
    </div>
  `);

  bindSearch();
  bindFilter();
  await loadLibrary();

  return () => {};
}

async function loadLibrary() {
  try {
    _library = await api.getHabitLibrary();
    renderLibrary();
  } catch (err) {
    document.getElementById('library-list').innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">📚</div>
        <div class="empty-title">Couldn't load library</div>
        <div class="empty-desc">${escapeHtml(err.message)}</div>
      </div>
    `;
  }
}

function bindSearch() {
  document.getElementById('search-input')?.addEventListener('input', (e) => {
    _query = e.target.value.toLowerCase();
    renderLibrary();
  });
}

function bindFilter() {
  document.getElementById('cat-filter')?.addEventListener('click', (e) => {
    const chip = e.target.closest('.chip');
    if (!chip) return;
    _filter = chip.dataset.cat;
    document.querySelectorAll('#cat-filter .chip').forEach((c) => c.classList.toggle('active', c === chip));
    renderLibrary();
  });
}

function getFiltered() {
  return _library.filter((h) => {
    const matchCat   = _filter === 'All' || h.category === _filter;
    const matchQuery = !_query || h.name.toLowerCase().includes(_query) || h.description?.toLowerCase().includes(_query);
    return matchCat && matchQuery;
  });
}

function renderLibrary() {
  const list = document.getElementById('library-list');
  if (!list) return;
  const items = getFiltered();

  if (!items.length) {
    list.innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">🔍</div>
        <div class="empty-title">No results</div>
        <div class="empty-desc">Try a different search or category filter.</div>
      </div>
    `;
    return;
  }

  // Group by category
  const groups = {};
  items.forEach((h) => {
    if (!groups[h.category]) groups[h.category] = [];
    groups[h.category].push(h);
  });

  list.innerHTML = Object.entries(groups).map(([cat, habits]) => `
    <div style="margin-bottom:20px">
      <div class="section-title mb-md" style="display:flex;align-items:center;gap:8px">
        <span>${CAT_ICONS[cat] || '🌿'}</span> ${cat}
      </div>
      ${habits.map((h) => renderHabitItem(h)).join('')}
    </div>
  `).join('');

  list.querySelectorAll('.add-habit-btn').forEach((btn) => {
    btn.addEventListener('click', () => handleAdd(btn.dataset.id));
  });
}

function renderHabitItem(h) {
  const color   = CAT_COLORS[h.category] || '#0D7377';
  const isAdding = _adding.has(h.id);

  return `
    <div class="action-item" style="margin-bottom:8px">
      <div class="action-item-icon" style="background:${color}18;border-radius:10px;width:44px;height:44px;display:flex;align-items:center;justify-content:center">
        ${CAT_ICONS[h.category] || '✅'}
      </div>
      <div class="action-item-info">
        <div class="action-item-name">${escapeHtml(h.name)}</div>
        <div class="action-item-co2">${escapeHtml(h.description || '')} · ${h.frequency}</div>
        <div class="action-item-co2">~${h.estimatedCO2ePerAction?.toFixed(2) || '0'} kg CO₂e saved</div>
      </div>
      <button class="btn btn-sm btn-primary add-habit-btn" data-id="${h.id}" ${isAdding ? 'disabled' : ''}>
        ${isAdding ? '…' : '+ Add'}
      </button>
    </div>
  `;
}

async function handleAdd(id) {
  if (_adding.has(id)) return;
  _adding.add(id);
  renderLibrary();

  try {
    await api.addHabit({ habitLibraryItemId: id });
    showToast('Habit added to your list! 🌿', 'success');
  } catch (err) {
    showToast('Failed to add habit: ' + err.message, 'error');
  } finally {
    _adding.delete(id);
    renderLibrary();
  }
}

function skeletonList(n) {
  return Array(n).fill(`
    <div class="action-item skeleton mb-sm" style="height:72px;margin-bottom:8px"></div>
  `).join('');
}
