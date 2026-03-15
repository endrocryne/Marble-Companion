/**
 * offsets.js — Carbon offset credits redemption
 */

import * as api from '../api.js';
import { setPageContent, showToast, escapeHtml } from '../app.js';
import { getUser } from '../auth.js';

const TIER_INFO = {
  PlantTree:    { icon: '🌱', color: '#22C55E', label: 'Plant a Tree',       badge: 'badge-success' },
  OffsetCarbon: { icon: '💨', color: '#0D7377', label: 'Offset Carbon',      badge: 'badge-primary' },
  FundWind:     { icon: '🌬️', color: '#3B82F6', label: 'Fund Wind Energy',   badge: 'badge-accent'  },
};

export async function init() {
  const user = getUser();

  setPageContent(`
    <div class="page">
      <div class="page-header" style="padding:0;margin-bottom:16px">
        <div>
          <div class="page-title">Carbon Offsets</div>
          <div class="page-subtitle">Redeem Leaf Points to offset your footprint</div>
        </div>
      </div>

      <!-- LP Balance -->
      <div class="fact-card mb-md">
        <div class="fact-label">Your Balance</div>
        <div style="font-size:28px;font-weight:900">🍃 ${(user?.totalLP ?? 0).toLocaleString()} LP</div>
      </div>

      <!-- Tab -->
      <div class="tab-bar mb-md">
        <button class="tab-btn active" id="tab-credits">Available</button>
        <button class="tab-btn" id="tab-history">History</button>
      </div>

      <div id="panel-credits" class="tab-panel active">
        ${skeleton(3)}
      </div>
      <div id="panel-history" class="tab-panel">
        ${skeleton(3)}
      </div>
    </div>
  `);

  document.getElementById('tab-credits')?.addEventListener('click', () => switchTab('credits'));
  document.getElementById('tab-history')?.addEventListener('click', () => switchTab('history'));

  await Promise.all([loadCredits(), loadHistory()]);

  return () => {};
}

function switchTab(tab) {
  document.getElementById('tab-credits').classList.toggle('active', tab === 'credits');
  document.getElementById('tab-history').classList.toggle('active', tab === 'history');
  document.getElementById('panel-credits').classList.toggle('active', tab === 'credits');
  document.getElementById('panel-history').classList.toggle('active', tab === 'history');
}

async function loadCredits() {
  try {
    const credits = await api.getOffsetCredits();
    renderCredits(credits || []);
  } catch (err) {
    document.getElementById('panel-credits').innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">❌</div>
        <div class="empty-title">Couldn't load credits</div>
        <div class="empty-desc">${escapeHtml(err.message)}</div>
      </div>
    `;
  }
}

function renderCredits(credits) {
  const el = document.getElementById('panel-credits');
  if (!el) return;

  if (!credits.length) {
    el.innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">🌿</div>
        <div class="empty-title">No credits available</div>
        <div class="empty-desc">Check back soon for offset opportunities.</div>
      </div>
    `;
    return;
  }

  // Group by tier
  const byTier = {};
  credits.forEach((c) => {
    if (!byTier[c.tier]) byTier[c.tier] = [];
    byTier[c.tier].push(c);
  });

  el.innerHTML = Object.entries(byTier).map(([tier, items]) => {
    const info = TIER_INFO[tier] || { icon: '🌍', color: '#0D7377', label: tier, badge: 'badge-primary' };
    return `
      <div style="margin-bottom:20px">
        <div class="section-header">
          <div class="section-title">${info.icon} ${info.label}</div>
          <span class="badge ${info.badge}">${tier}</span>
        </div>
        ${items.map((credit) => `
          <div class="offset-card">
            <div class="offset-icon">${info.icon}</div>
            <div class="offset-info">
              <div class="offset-name">${escapeHtml(credit.name)}</div>
              <div class="offset-desc">${escapeHtml(credit.description || '')}</div>
              <div class="offset-desc">🌍 ${credit.co2eOffsetKg?.toFixed(1)} kg CO₂e offset</div>
              <div class="offset-cost">🍃 ${credit.lpCost?.toLocaleString()} LP</div>
            </div>
            <button class="btn btn-primary btn-sm redeem-btn" data-id="${credit.id}" data-cost="${credit.lpCost}">
              Redeem
            </button>
          </div>
        `).join('')}
      </div>
    `;
  }).join('');

  el.querySelectorAll('.redeem-btn').forEach((btn) => {
    btn.addEventListener('click', () => handleRedeem(btn.dataset.id, parseInt(btn.dataset.cost), btn));
  });
}

async function handleRedeem(id, cost, btn) {
  const user = getUser();
  if ((user?.totalLP ?? 0) < cost) {
    showToast(`You need ${cost} LP but only have ${user?.totalLP ?? 0} LP`, 'error');
    return;
  }
  if (!confirm(`Redeem this offset for ${cost} LP?`)) return;

  btn.disabled = true;
  btn.textContent = '…';

  try {
    await api.redeemOffset({ offsetCreditId: id });
    showToast('Offset redeemed! 🌍 You\'re making a difference.', 'success');
    await Promise.all([loadCredits(), loadHistory()]);
  } catch (err) {
    btn.disabled = false;
    btn.textContent = 'Redeem';
    showToast('Redemption failed: ' + err.message, 'error');
  }
}

async function loadHistory() {
  try {
    const history = await api.getOffsetHistory();
    renderHistory(history || []);
  } catch (err) {
    document.getElementById('panel-history').innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">📋</div>
        <div class="empty-title">No history available</div>
      </div>
    `;
  }
}

function renderHistory(history) {
  const el = document.getElementById('panel-history');
  if (!el) return;

  if (!history.length) {
    el.innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">📋</div>
        <div class="empty-title">No redemptions yet</div>
        <div class="empty-desc">Redeem your first offset credit to see history here.</div>
      </div>
    `;
    return;
  }

  const totalKg = history.reduce((s, h) => s + (h.co2eOffsetKg || 0), 0);

  el.innerHTML = `
    <div class="card mb-md" style="background:linear-gradient(135deg,rgba(13,115,119,0.08),rgba(50,224,196,0.08));border:1px solid rgba(13,115,119,0.15)">
      <div style="display:flex;gap:24px;justify-content:center;text-align:center">
        <div>
          <div style="font-size:24px;font-weight:800;color:var(--color-primary)">${history.length}</div>
          <div style="font-size:12px;color:var(--color-text-secondary)">Offsets Redeemed</div>
        </div>
        <div>
          <div style="font-size:24px;font-weight:800;color:var(--color-primary)">${totalKg.toFixed(1)}</div>
          <div style="font-size:12px;color:var(--color-text-secondary)">kg CO₂e offset</div>
        </div>
      </div>
    </div>
    ${history.map((h) => {
      const info = TIER_INFO[h.tier] || { icon: '🌍' };
      return `
        <div class="offset-card">
          <div class="offset-icon">${info.icon}</div>
          <div class="offset-info">
            <div class="offset-name">${escapeHtml(h.offsetCreditName)}</div>
            <div class="offset-desc">🌍 ${h.co2eOffsetKg?.toFixed(1)} kg CO₂e offset</div>
            <div class="offset-cost">🍃 ${h.lpSpent?.toLocaleString()} LP spent</div>
          </div>
          <div style="font-size:11px;color:var(--color-text-secondary);text-align:right;align-self:flex-start">
            ${formatDate(h.redeemedAt)}
          </div>
        </div>
      `;
    }).join('')}
  `;
}

function skeleton(n) {
  return Array(n).fill('<div class="offset-card skeleton mb-sm" style="height:88px"></div>').join('');
}

function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
}
