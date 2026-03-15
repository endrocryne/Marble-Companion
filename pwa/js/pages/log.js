/**
 * log.js — Log eco actions (Quick + Detailed modes)
 */

import * as api from '../api.js';
import { setPageContent, showToast, showLPPopup, escapeHtml } from '../app.js';
import { enqueueOffline } from '../db.js';

const CATEGORIES = [
  { id: 'Transport', label: 'Transport', icon: '🚌', color: '#3B82F6' },
  { id: 'Food',      label: 'Food',      icon: '🥦', color: '#22C55E' },
  { id: 'Energy',    label: 'Energy',    icon: '⚡', color: '#F59E0B' },
  { id: 'Shopping',  label: 'Shopping',  icon: '🛍️', color: '#A855F7' },
  { id: 'Travel',    label: 'Travel',    icon: '✈️', color: '#EC4899' },
  { id: 'Waste',     label: 'Waste',     icon: '♻️', color: '#06B6D4' },
];

// Action templates per category
const ACTION_TEMPLATES = {
  Transport: [
    { id: 'walked',        label: 'Walked instead of drove', icon: '🚶', co2e: 2.5 },
    { id: 'cycled',        label: 'Cycled to work/school',   icon: '🚴', co2e: 2.8 },
    { id: 'public-transit',label: 'Used public transit',     icon: '🚌', co2e: 1.8 },
    { id: 'carpool',       label: 'Carpooled',               icon: '🚗', co2e: 1.2 },
    { id: 'ev',            label: 'Drove electric/hybrid',   icon: '⚡', co2e: 0.9 },
    { id: 'wfh',           label: 'Worked from home',        icon: '🏠', co2e: 3.5 },
  ],
  Food: [
    { id: 'vegan-meal',    label: 'Ate a vegan meal',        icon: '🥗', co2e: 1.5 },
    { id: 'vegetarian',    label: 'Vegetarian meal',          icon: '🥕', co2e: 0.8 },
    { id: 'local-produce', label: 'Bought local produce',    icon: '🧺', co2e: 0.5 },
    { id: 'no-food-waste', label: 'No food wasted today',    icon: '♻️', co2e: 0.7 },
    { id: 'plant-based',   label: 'Plant-based protein',     icon: '🫘', co2e: 1.2 },
    { id: 'meatless-day',  label: 'Meatless day',            icon: '🌿', co2e: 2.5 },
  ],
  Energy: [
    { id: 'short-shower',  label: 'Took a short shower',     icon: '🚿', co2e: 0.6 },
    { id: 'lights-off',    label: 'Turned off lights/standby',icon:'💡', co2e: 0.3 },
    { id: 'eco-wash',      label: 'Eco laundry wash',        icon: '🫧', co2e: 0.5 },
    { id: 'air-dry',       label: 'Air-dried clothes',       icon: '👕', co2e: 1.0 },
    { id: 'thermostat',    label: 'Lowered thermostat 1°C',  icon: '🌡️', co2e: 0.8 },
    { id: 'renewable',     label: 'Used renewable energy',   icon: '☀️', co2e: 2.0 },
  ],
  Shopping: [
    { id: 'secondhand',    label: 'Bought secondhand',       icon: '♻️', co2e: 3.0 },
    { id: 'reusable-bag',  label: 'Used reusable bags',      icon: '🛍️', co2e: 0.2 },
    { id: 'repair',        label: 'Repaired instead of replaced',icon:'🔧',co2e:4.0},
    { id: 'borrowed',      label: 'Borrowed instead of bought',icon:'🤝', co2e: 3.5 },
    { id: 'no-plastic',    label: 'Avoided single-use plastic',icon:'🚫', co2e: 0.5 },
    { id: 'local-brand',   label: 'Bought from local brand', icon: '🏪', co2e: 1.0 },
  ],
  Travel: [
    { id: 'train-not-fly', label: 'Chose train over flight', icon: '🚆', co2e:150.0},
    { id: 'staycation',    label: 'Staycation instead of flying',icon:'🏡',co2e:200.0},
    { id: 'offset-flight', label: 'Offset a flight',         icon: '✈️', co2e: 50.0 },
    { id: 'eco-hotel',     label: 'Stayed in eco hotel',     icon: '🌿', co2e: 5.0  },
  ],
  Waste: [
    { id: 'composted',     label: 'Composted food scraps',   icon: '🌱', co2e: 0.8 },
    { id: 'recycled',      label: 'Recycled properly',       icon: '♻️', co2e: 0.5 },
    { id: 'zero-waste',    label: 'Zero waste day',          icon: '🗑️', co2e: 1.5 },
    { id: 'refill',        label: 'Used refill station',     icon: '💧', co2e: 0.3 },
  ],
};

let _mode     = 'quick';
let _category = 'Transport';
let _selected = null;

export async function init() {
  setPageContent(`
    <div class="page">
      <div style="display:flex;align-items:center;gap:12px;margin-bottom:16px">
        <button class="btn btn-icon btn-ghost" onclick="history.back()" aria-label="Back">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="15,18 9,12 15,6"/>
          </svg>
        </button>
        <div class="page-title">Log Action</div>
      </div>

      <!-- Mode Toggle -->
      <div class="tab-bar mb-md">
        <button class="tab-btn active" id="mode-quick">Quick Log</button>
        <button class="tab-btn" id="mode-detailed">Detailed</button>
      </div>

      <!-- Category Selector -->
      <div class="category-grid" id="cat-grid">
        ${CATEGORIES.map((c) => `
          <div class="category-option ${c.id === _category ? 'selected' : ''} cat-${c.id.toLowerCase()}" data-cat="${c.id}" style="color:${c.color}">
            <div class="category-option-icon">${c.icon}</div>
            <div class="category-option-label">${c.label}</div>
          </div>
        `).join('')}
      </div>

      <!-- Quick Log Panel -->
      <div id="panel-quick">
        <div class="section-title mb-md" id="action-section-title">Select Transport Action</div>
        <div id="action-list"></div>
      </div>

      <!-- Detailed Log Panel -->
      <div id="panel-detailed" style="display:none">
        <div class="card">
          <div class="form-group">
            <label class="form-label">Description</label>
            <input type="text" class="form-input" id="detail-desc" placeholder="What did you do?" />
          </div>
          <div class="form-group">
            <label class="form-label">CO₂e Saved (kg)</label>
            <input type="number" class="form-input" id="detail-co2e" placeholder="e.g. 2.5" step="0.01" min="0" />
          </div>
          <div class="form-group">
            <label class="form-label">Notes (optional)</label>
            <textarea class="form-input" id="detail-notes" rows="2" placeholder="Any extra details…"></textarea>
          </div>
          <button class="btn btn-primary btn-full" id="btn-log-detailed">Log Action 🌿</button>
        </div>
      </div>
    </div>
  `);

  renderActions();
  bindEvents();

  return () => {};
}

function bindEvents() {
  document.getElementById('mode-quick')?.addEventListener('click', () => switchMode('quick'));
  document.getElementById('mode-detailed')?.addEventListener('click', () => switchMode('detailed'));

  document.getElementById('cat-grid')?.addEventListener('click', (e) => {
    const option = e.target.closest('.category-option');
    if (!option) return;
    _category = option.dataset.cat;
    _selected = null;
    document.querySelectorAll('.category-option').forEach((o) => o.classList.toggle('selected', o === option));
    document.getElementById('action-section-title').textContent = `Select ${_category} Action`;
    renderActions();
  });

  document.getElementById('btn-log-detailed')?.addEventListener('click', submitDetailed);
}

function switchMode(mode) {
  _mode = mode;
  document.getElementById('mode-quick').classList.toggle('active', mode === 'quick');
  document.getElementById('mode-detailed').classList.toggle('active', mode === 'detailed');
  document.getElementById('panel-quick').style.display  = mode === 'quick'    ? '' : 'none';
  document.getElementById('panel-detailed').style.display = mode === 'detailed' ? '' : 'none';
}

function renderActions() {
  const list = document.getElementById('action-list');
  if (!list) return;
  const templates = ACTION_TEMPLATES[_category] || [];

  list.innerHTML = templates.map((t) => `
    <div class="action-item ${_selected?.id === t.id ? 'selected' : ''}" data-action="${t.id}" style="cursor:pointer;${_selected?.id === t.id ? 'border:2px solid var(--color-primary);' : ''}">
      <div class="action-item-icon">${t.icon}</div>
      <div class="action-item-info">
        <div class="action-item-name">${t.label}</div>
        <div class="action-item-co2">~${t.co2e} kg CO₂e saved</div>
      </div>
      <button class="btn btn-sm btn-primary" onclick="void(0)">Log</button>
    </div>
  `).join('');

  list.querySelectorAll('.action-item').forEach((el) => {
    el.addEventListener('click', () => {
      const t = templates.find((a) => a.id === el.dataset.action);
      if (t) submitQuick(t);
    });
  });
}

async function submitQuick(template) {
  try {
    const result = await api.logAction({
      category: _category,
      actionTemplateId: template.id,
      isDetailed: false,
    });
    showLPPopup(result.lpAwarded, result.co2eSaved);
  } catch (err) {
    if (!navigator.onLine) {
      await enqueueOffline({ type: 'logAction', payload: { category: _category, actionTemplateId: template.id, isDetailed: false } });
      showToast('Offline — action queued for sync', 'warning');
      showLPPopup(10, template.co2e); // Optimistic LP
    } else {
      showToast('Failed to log action: ' + err.message, 'error');
    }
  }
}

async function submitDetailed() {
  const desc  = document.getElementById('detail-desc')?.value?.trim();
  const co2eVal = parseFloat(document.getElementById('detail-co2e')?.value);
  const notes = document.getElementById('detail-notes')?.value?.trim();

  if (!desc) { showToast('Please enter a description', 'error'); return; }
  if (isNaN(co2eVal) || co2eVal < 0) { showToast('Please enter a valid CO₂e amount', 'error'); return; }

  try {
    const result = await api.logAction({
      category:   _category,
      isDetailed: true,
      detailedData: { description: desc, co2eSaved: co2eVal, notes },
    });
    showLPPopup(result.lpAwarded, result.co2eSaved);
    document.getElementById('detail-desc').value  = '';
    document.getElementById('detail-co2e').value  = '';
    document.getElementById('detail-notes').value = '';
  } catch (err) {
    if (!navigator.onLine) {
      await enqueueOffline({ type: 'logAction', payload: { category: _category, isDetailed: true, detailedData: { description: desc, co2eSaved: co2eVal } } });
      showToast('Offline — action queued for sync', 'warning');
    } else {
      showToast('Failed to log action: ' + err.message, 'error');
    }
  }
}
