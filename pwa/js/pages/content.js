/**
 * content.js — Eco-content library list with search and filters
 */

import * as api from '../api.js';
import { setPageContent, showToast, escapeHtml, navigate } from '../app.js';
import { cacheContent, getCachedContent } from '../db.js';

const DIFF_CLASS  = { Beginner:'badge-easy', Intermediate:'badge-medium', Advanced:'badge-hard' };
const CAT_ICONS   = { Transport:'🚌', Food:'🥦', Energy:'⚡', Shopping:'🛍️', Travel:'✈️', Waste:'♻️', General:'🌍' };
const FILTERS     = ['All', 'Transport', 'Food', 'Energy', 'Shopping', 'Travel', 'Waste'];

let _all    = [];
let _filter = 'All';
let _query  = '';

export async function init() {
  setPageContent(`
    <div class="page">
      <div class="page-header" style="padding:0;margin-bottom:16px">
        <div class="page-title">Learn & Earn</div>
      </div>

      <div class="search-bar">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
        </svg>
        <input type="text" class="search-input" id="content-search" placeholder="Search articles…" />
      </div>

      <div class="chip-group mb-md">
        ${FILTERS.map((f) => `
          <div class="chip ${f === _filter ? 'active' : ''}" data-filter="${f}">
            ${f === 'All' ? 'All' : (CAT_ICONS[f] || '') + ' ' + f}
          </div>
        `).join('')}
      </div>

      <!-- Today's Feature -->
      <div id="today-feature"></div>

      <!-- List -->
      <div id="content-list">
        ${skeleton(5)}
      </div>
    </div>
  `);

  // Show cache immediately
  const cached = await getCachedContent();
  if (cached?.length) { _all = cached; renderList(); }

  bindSearch();
  bindFilters();
  await loadContent();

  return () => {};
}

async function loadContent() {
  try {
    const [list, today] = await Promise.all([api.getContentList(), api.getTodayContent()]);
    _all = list || [];
    cacheContent(_all);
    renderToday(today);
    renderList();
  } catch (err) {
    if (!_all.length) {
      document.getElementById('content-list').innerHTML = `
        <div class="empty-state">
          <div class="empty-emoji">📚</div>
          <div class="empty-title">Couldn't load content</div>
          <div class="empty-desc">${escapeHtml(err.message)}</div>
        </div>
      `;
    }
  }
}

function renderToday(today) {
  const el = document.getElementById('today-feature');
  if (!el || !today) return;

  el.innerHTML = `
    <div class="card-elevated" style="background:linear-gradient(135deg,#0D7377,#14A085);color:#fff;border-radius:16px;padding:16px;margin-bottom:16px;cursor:pointer" data-id="${today.id}" id="today-card">
      <div style="font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;opacity:0.8;margin-bottom:8px">📰 Today's Article</div>
      <div style="font-size:17px;font-weight:800;margin-bottom:8px;line-height:1.3">${escapeHtml(today.title)}</div>
      <div style="font-size:13px;opacity:0.85;line-height:1.4;margin-bottom:12px">${escapeHtml(today.summary || '')}</div>
      <div style="display:flex;gap:8px;align-items:center">
        <span style="background:rgba(255,255,255,0.2);border-radius:9999px;padding:3px 10px;font-size:11px;font-weight:700">${today.difficulty || ''}</span>
        ${today.lpReward ? `<span style="background:rgba(255,255,255,0.2);border-radius:9999px;padding:3px 10px;font-size:11px;font-weight:700">🍃 +${today.lpReward} LP</span>` : ''}
      </div>
    </div>
  `;

  document.getElementById('today-card')?.addEventListener('click', () => navigate('content-detail', { id: today.id }));
}

function renderList() {
  const el = document.getElementById('content-list');
  if (!el) return;

  const filtered = _all.filter((c) => {
    const matchCat   = _filter === 'All' || c.category === _filter;
    const matchQuery = !_query || c.title.toLowerCase().includes(_query) || c.summary?.toLowerCase().includes(_query);
    return matchCat && matchQuery;
  });

  if (!filtered.length) {
    el.innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">🔍</div>
        <div class="empty-title">No results</div>
        <div class="empty-desc">Try adjusting your search or filters.</div>
      </div>
    `;
    return;
  }

  el.innerHTML = filtered.map((c) => `
    <div class="content-card" data-id="${c.id}">
      <div style="display:flex;align-items:center;gap:8px;margin-bottom:6px">
        <span style="font-size:18px">${CAT_ICONS[c.category] || '📄'}</span>
        <div class="read-dot ${c.isRead ? 'read' : ''}"></div>
        <span class="badge ${DIFF_CLASS[c.difficulty] || 'badge-primary'}">${c.difficulty || 'Beginner'}</span>
        <span style="font-size:11px;color:var(--color-text-secondary);margin-left:auto">${c.category || ''}</span>
      </div>
      <div class="content-title">${escapeHtml(c.title)}</div>
      <div class="content-summary">${escapeHtml(c.summary || '')}</div>
    </div>
  `).join('');

  el.querySelectorAll('.content-card').forEach((card) => {
    card.addEventListener('click', () => navigate('content-detail', { id: card.dataset.id }));
  });
}

function bindSearch() {
  document.getElementById('content-search')?.addEventListener('input', (e) => {
    _query = e.target.value.toLowerCase();
    renderList();
  });
}

function bindFilters() {
  document.querySelectorAll('.chip[data-filter]').forEach((chip) => {
    chip.addEventListener('click', () => {
      _filter = chip.dataset.filter;
      document.querySelectorAll('.chip[data-filter]').forEach((c) => c.classList.toggle('active', c === chip));
      renderList();
    });
  });
}

function skeleton(n) {
  return Array(n).fill('<div class="content-card skeleton mb-md" style="height:100px"></div>').join('');
}
