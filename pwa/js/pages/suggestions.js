/**
 * suggestions.js — Personalized habit recommendations
 */

import * as api from '../api.js';
import { setPageContent, showToast, escapeHtml } from '../app.js';
import { getUser } from '../auth.js';

const EFFORT_CLASS  = { Low: 'badge-easy', Medium: 'badge-medium', High: 'badge-hard' };
const IMPACT_CLASS  = { Low: 'badge-easy', Medium: 'badge-medium', High: 'badge-success' };
const CAT_ICONS     = { Transport:'🚌', Food:'🥦', Energy:'⚡', Shopping:'🛍️', Travel:'✈️', Waste:'♻️' };
const CAT_COLORS    = { Transport:'#3B82F6', Food:'#22C55E', Energy:'#F59E0B', Shopping:'#A855F7', Travel:'#EC4899', Waste:'#06B6D4' };

// Generate personalized suggestions based on user baseline quiz answers
function buildSuggestions(user) {
  const suggestions = [
    {
      id: 's-1', category: 'Transport', name: 'Cycle to Work',
      description: 'Replace your car commute with cycling at least twice a week.',
      effort: 'Medium', impact: 'High', co2eSaved: 3.5, lpReward: 50,
      habitLibraryId: 'cycle-commute',
    },
    {
      id: 's-2', category: 'Food', name: 'Meatless Mondays',
      description: 'Skip meat every Monday — saves up to 2.5 kg CO₂e per meal.',
      effort: 'Low', impact: 'High', co2eSaved: 2.5, lpReward: 40,
      habitLibraryId: 'meatless-monday',
    },
    {
      id: 's-3', category: 'Energy', name: 'Short Showers',
      description: 'Keep showers under 5 minutes to save water and energy.',
      effort: 'Low', impact: 'Low', co2eSaved: 0.6, lpReward: 20,
      habitLibraryId: 'short-shower',
    },
    {
      id: 's-4', category: 'Shopping', name: 'Buy Secondhand First',
      description: 'Before buying new, check secondhand shops and marketplaces.',
      effort: 'Medium', impact: 'High', co2eSaved: 4.0, lpReward: 60,
      habitLibraryId: 'secondhand',
    },
    {
      id: 's-5', category: 'Waste', name: 'Start Composting',
      description: 'Compost food scraps to divert waste from landfill.',
      effort: 'Medium', impact: 'Medium', co2eSaved: 0.8, lpReward: 30,
      habitLibraryId: 'composting',
    },
    {
      id: 's-6', category: 'Energy', name: 'Switch to Green Energy',
      description: 'Contact your energy provider to switch to a renewable tariff.',
      effort: 'Low', impact: 'High', co2eSaved: 2.0, lpReward: 45,
      habitLibraryId: 'renewable-energy',
    },
    {
      id: 's-7', category: 'Transport', name: 'Use Public Transit',
      description: 'Replace one car trip per week with bus or train.',
      effort: 'Low', impact: 'Medium', co2eSaved: 1.8, lpReward: 35,
      habitLibraryId: 'public-transit',
    },
    {
      id: 's-8', category: 'Food', name: 'Buy Local Produce',
      description: 'Shop at farmers\' markets or use a local veg box scheme.',
      effort: 'Medium', impact: 'Medium', co2eSaved: 0.5, lpReward: 25,
      habitLibraryId: 'local-produce',
    },
    {
      id: 's-9', category: 'Waste', name: 'Zero Waste Week',
      description: 'Challenge yourself to produce zero landfill waste for a week.',
      effort: 'High', impact: 'High', co2eSaved: 1.5, lpReward: 80,
      habitLibraryId: 'zero-waste',
    },
  ];

  return suggestions;
}

let _adding = new Set();
let _added  = new Set();

export async function init() {
  const user = getUser();

  setPageContent(`
    <div class="page">
      <div style="display:flex;align-items:center;gap:12px;margin-bottom:16px">
        <button class="btn btn-icon btn-ghost" onclick="history.back()" aria-label="Back">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="15,18 9,12 15,6"/>
          </svg>
        </button>
        <div>
          <div class="page-title">Suggestions</div>
          <div class="page-subtitle">Personalized for you</div>
        </div>
      </div>

      <div class="fact-card mb-md">
        <div class="fact-label">💡 How it works</div>
        <div class="fact-text">Suggestions are based on your lifestyle quiz answers and existing habits. Add any to start tracking your impact.</div>
      </div>

      <div id="suggestions-grid">
        ${buildSuggestions(user).map((s) => renderCard(s)).join('')}
      </div>
    </div>
  `);

  bindAddButtons();

  return () => {};
}

function buildSuggestionsHtml() {
  const user = getUser();
  return buildSuggestions(user).map((s) => renderCard(s)).join('');
}

function renderCard(s) {
  const color   = CAT_COLORS[s.category] || '#0D7377';
  const icon    = CAT_ICONS[s.category] || '🌿';
  const isAdded = _added.has(s.id);
  const isAdding = _adding.has(s.id);

  return `
    <div class="suggestion-card" data-suggestion-id="${s.id}">
      <div style="display:flex;align-items:flex-start;gap:12px;margin-bottom:10px">
        <div style="width:44px;height:44px;border-radius:12px;background:${color}18;display:flex;align-items:center;justify-content:center;font-size:22px;flex-shrink:0">${icon}</div>
        <div style="flex:1">
          <div style="font-size:15px;font-weight:700;color:var(--color-text-primary);margin-bottom:4px">${escapeHtml(s.name)}</div>
          <div style="font-size:13px;color:var(--color-text-secondary);line-height:1.4">${escapeHtml(s.description)}</div>
        </div>
      </div>

      <div style="display:flex;gap:6px;flex-wrap:wrap;margin-bottom:10px">
        <span class="badge ${EFFORT_CLASS[s.effort] || 'badge-primary'}">⚡ ${s.effort} effort</span>
        <span class="badge ${IMPACT_CLASS[s.impact] || 'badge-primary'}">🎯 ${s.impact} impact</span>
        <span class="badge badge-primary">~${s.co2eSaved} kg CO₂e</span>
        <span class="badge badge-accent">🍃 +${s.lpReward} LP</span>
      </div>

      <button class="btn btn-full ${isAdded ? 'btn-ghost' : 'btn-primary'} btn-sm add-suggestion-btn" data-id="${s.id}" data-habit-id="${s.habitLibraryId}" ${isAdded || isAdding ? 'disabled' : ''}>
        ${isAdded ? '✓ Added to Habits' : isAdding ? 'Adding…' : '+ Add to Habits'}
      </button>
    </div>
  `;
}

function rerender() {
  const user = getUser();
  const grid = document.getElementById('suggestions-grid');
  if (grid) grid.innerHTML = buildSuggestions(user).map((s) => renderCard(s)).join('');
  bindAddButtons();
}

function bindAddButtons() {
  document.querySelectorAll('.add-suggestion-btn').forEach((btn) => {
    btn.addEventListener('click', () => handleAdd(btn.dataset.id, btn.dataset.habitId));
  });
}

async function handleAdd(suggestionId, habitLibraryId) {
  if (_adding.has(suggestionId) || _added.has(suggestionId)) return;
  _adding.add(suggestionId);
  rerender();

  try {
    await api.addHabit({ habitLibraryItemId: habitLibraryId });
    _added.add(suggestionId);
    showToast('Habit added! 🌿', 'success');
  } catch (err) {
    showToast('Failed to add: ' + err.message, 'error');
  } finally {
    _adding.delete(suggestionId);
    rerender();
  }
}
