/**
 * dashboard.js — Carbon footprint charts and stats
 */

import * as api from '../api.js';
import { setPageContent, showToast, escapeHtml } from '../app.js';

const PERIODS = [
  { label: 'Day',   value: 'day' },
  { label: 'Week',  value: 'week' },
  { label: 'Month', value: 'month' },
  { label: 'Year',  value: 'year' },
];

const CAT_COLORS = {
  Transport: '#3B82F6',
  Food:      '#22C55E',
  Energy:    '#F59E0B',
  Shopping:  '#A855F7',
  Travel:    '#EC4899',
  Waste:     '#06B6D4',
};

const CAT_ICONS = {
  Transport: '🚌', Food: '🥦', Energy: '⚡', Shopping: '🛍️', Travel: '✈️', Waste: '♻️',
};

let _period  = 'week';
let _lineChart  = null;
let _donutChart = null;

export async function init() {
  setPageContent(`
    <div class="page">
      <div class="page-header" style="padding:0;margin-bottom:16px">
        <div>
          <div class="page-title">Dashboard</div>
          <div class="page-subtitle">Your carbon footprint overview</div>
        </div>
      </div>

      <!-- Period Selector -->
      <div class="period-selector" id="period-selector">
        ${PERIODS.map((p) => `
          <button class="period-btn ${p.value === _period ? 'active' : ''}" data-period="${p.value}">${p.label}</button>
        `).join('')}
      </div>

      <!-- Summary Cards -->
      <div class="stats-row" id="summary-cards">
        <div class="stat-card skeleton" style="height:90px"></div>
        <div class="stat-card skeleton" style="height:90px"></div>
        <div class="stat-card skeleton" style="height:90px"></div>
        <div class="stat-card skeleton" style="height:90px"></div>
      </div>

      <!-- Line Chart: CO2e Trend -->
      <div class="card card-elevated mb-md">
        <div class="card-header">
          <div class="card-title">CO₂e Trend</div>
          <span class="badge badge-primary" id="trend-badge">kg saved</span>
        </div>
        <div style="position:relative;height:200px">
          <canvas id="line-chart"></canvas>
        </div>
      </div>

      <!-- Donut Chart: Category Breakdown -->
      <div class="card card-elevated mb-md">
        <div class="card-header">
          <div class="card-title">By Category</div>
        </div>
        <div style="display:flex;align-items:center;gap:16px">
          <div style="position:relative;width:160px;height:160px;flex-shrink:0">
            <canvas id="donut-chart"></canvas>
          </div>
          <div id="donut-legend" style="flex:1;display:flex;flex-direction:column;gap:8px"></div>
        </div>
      </div>

      <!-- Category Stats -->
      <div class="card mb-md">
        <div class="card-title mb-md">Actions by Category</div>
        <div id="category-list"></div>
      </div>

      <!-- You vs Average -->
      <div class="card mb-md">
        <div class="card-title mb-md">You vs. Average</div>
        <div id="vs-average"></div>
      </div>
    </div>
  `);

  bindPeriodSelector();
  await loadData();

  return cleanup;
}

function cleanup() {
  if (_lineChart)  { _lineChart.destroy();  _lineChart  = null; }
  if (_donutChart) { _donutChart.destroy(); _donutChart = null; }
}

function bindPeriodSelector() {
  document.getElementById('period-selector')?.addEventListener('click', async (e) => {
    const btn = e.target.closest('.period-btn');
    if (!btn) return;
    _period = btn.dataset.period;
    document.querySelectorAll('.period-btn').forEach((b) => b.classList.toggle('active', b === btn));
    await loadData();
  });
}

async function loadData() {
  try {
    const [summary, actions] = await Promise.all([
      api.getActionSummary(),
      api.getActions(...getDateRange(_period)),
    ]);

    renderSummaryCards(summary);
    renderCharts(actions, summary);
    renderCategoryList(summary.totalsByCategory || {});
    renderVsAverage(summary);
  } catch (err) {
    showToast('Failed to load dashboard data', 'error');
    console.error(err);
  }
}

function getDateRange(period) {
  const now  = new Date();
  const from = new Date(now);
  if (period === 'day')   from.setDate(now.getDate() - 1);
  if (period === 'week')  from.setDate(now.getDate() - 7);
  if (period === 'month') from.setMonth(now.getMonth() - 1);
  if (period === 'year')  from.setFullYear(now.getFullYear() - 1);
  return [from.toISOString(), now.toISOString(), undefined];
}

function renderSummaryCards(summary) {
  const cats   = summary.totalsByCategory || {};
  const catArr = Object.values(cats);
  const totalCO2  = (summary.totalCO2eSaved ?? 0).toFixed(1);
  const totalLP   = (summary.totalLP ?? 0).toLocaleString();
  const totalActs = catArr.reduce((a, c) => a + (c.actionCount || 0), 0);
  const bestCat   = catArr.sort((a, b) => b.co2eSaved - a.co2eSaved)[0];

  document.getElementById('summary-cards').innerHTML = `
    <div class="stat-card">
      <div class="stat-icon">🌍</div>
      <div class="stat-value">${totalCO2}</div>
      <div class="stat-label">kg CO₂e Saved</div>
    </div>
    <div class="stat-card">
      <div class="stat-icon">🍃</div>
      <div class="stat-value">${totalLP}</div>
      <div class="stat-label">Leaf Points</div>
    </div>
    <div class="stat-card">
      <div class="stat-icon">✅</div>
      <div class="stat-value">${totalActs}</div>
      <div class="stat-label">Actions Logged</div>
    </div>
    <div class="stat-card">
      <div class="stat-icon">${bestCat ? CAT_ICONS[Object.keys(summary.totalsByCategory || {}).find(k => summary.totalsByCategory[k] === bestCat) || ''] || '🏆' : '🏆'}</div>
      <div class="stat-value" style="font-size:13px">${bestCat ? (Object.keys(summary.totalsByCategory || {}).find(k => summary.totalsByCategory[k] === bestCat) || '—') : '—'}</div>
      <div class="stat-label">Top Category</div>
    </div>
  `;
}

function renderCharts(actions, summary) {
  // Build time-series data
  const buckets = buildTimeBuckets(_period, actions);

  // Line Chart
  const lineCtx = document.getElementById('line-chart')?.getContext('2d');
  if (lineCtx) {
    if (_lineChart) _lineChart.destroy();
    _lineChart = new Chart(lineCtx, {
      type: 'line',
      data: {
        labels:   buckets.labels,
        datasets: [{
          label: 'CO₂e saved (kg)',
          data:  buckets.values,
          borderColor:     '#0D7377',
          backgroundColor: 'rgba(13,115,119,0.1)',
          fill: true,
          tension: 0.4,
          pointBackgroundColor: '#32E0C4',
          pointRadius: 4,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { font: { size: 11 } } },
          x: { grid: { display: false },                               ticks: { font: { size: 11 } } },
        },
      },
    });
  }

  // Donut Chart
  const cats   = summary.totalsByCategory || {};
  const catKeys = Object.keys(cats).filter((k) => cats[k].co2eSaved > 0);
  const donutCtx = document.getElementById('donut-chart')?.getContext('2d');
  if (donutCtx && catKeys.length > 0) {
    if (_donutChart) _donutChart.destroy();
    _donutChart = new Chart(donutCtx, {
      type: 'doughnut',
      data: {
        labels:   catKeys,
        datasets: [{
          data: catKeys.map((k) => cats[k].co2eSaved.toFixed(2)),
          backgroundColor: catKeys.map((k) => CAT_COLORS[k] || '#999'),
          borderWidth: 2,
          borderColor: '#fff',
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '65%',
        plugins: { legend: { display: false } },
      },
    });
  }

  // Legend
  const legend = document.getElementById('donut-legend');
  if (legend) {
    legend.innerHTML = catKeys.map((k) => `
      <div style="display:flex;align-items:center;gap:8px">
        <div style="width:10px;height:10px;border-radius:50%;background:${CAT_COLORS[k]};flex-shrink:0"></div>
        <span style="font-size:12px;color:var(--color-text-secondary)">${CAT_ICONS[k] || ''} ${k}</span>
        <span style="font-size:12px;font-weight:700;color:var(--color-text-primary);margin-left:auto">${cats[k].co2eSaved.toFixed(1)}kg</span>
      </div>
    `).join('');
  }
}

function buildTimeBuckets(period, actions) {
  const now     = new Date();
  const labels  = [];
  const values  = [];
  const bucketMap = {};

  let count, formatFn, keyFn;

  if (period === 'day') {
    count = 24;
    formatFn = (i) => `${i}:00`;
    keyFn    = (d) => new Date(d).getHours();
  } else if (period === 'week') {
    count = 7;
    const days = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];
    formatFn = (i) => days[(now.getDay() - (count - 1 - i) + 7) % 7];
    keyFn    = (d) => {
      const diff = Math.floor((now - new Date(d)) / 86400000);
      return count - 1 - diff;
    };
  } else if (period === 'month') {
    count = 4;
    formatFn = (i) => `Wk ${i + 1}`;
    keyFn    = (d) => Math.floor((now - new Date(d)) / (86400000 * 7));
  } else {
    count = 12;
    const months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    formatFn = (i) => months[(now.getMonth() - (count - 1 - i) + 12) % 12];
    keyFn    = (d) => {
      const m = (now.getMonth() - new Date(d).getMonth() + 12) % 12;
      return count - 1 - m;
    };
  }

  for (let i = 0; i < count; i++) {
    labels.push(formatFn(i));
    bucketMap[i] = 0;
  }

  (actions || []).forEach((a) => {
    const k = keyFn(a.loggedAt);
    if (k >= 0 && k < count) bucketMap[k] += (a.co2eSaved || 0);
  });

  for (let i = 0; i < count; i++) values.push(+bucketMap[i].toFixed(2));

  return { labels, values };
}

function renderCategoryList(cats) {
  const list = document.getElementById('category-list');
  if (!list) return;
  const entries = Object.entries(cats);
  if (!entries.length) { list.innerHTML = '<div class="text-secondary" style="font-size:14px">No actions logged yet.</div>'; return; }

  list.innerHTML = entries.map(([cat, data]) => {
    const pct = Math.min(100, (data.co2eSaved / Math.max(...Object.values(cats).map(d => d.co2eSaved))) * 100);
    return `
      <div style="margin-bottom:12px">
        <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:4px">
          <span style="font-size:14px;font-weight:600">${CAT_ICONS[cat] || ''} ${cat}</span>
          <span style="font-size:13px;color:var(--color-text-secondary)">${data.actionCount} actions · ${data.co2eSaved.toFixed(2)}kg</span>
        </div>
        <div class="progress-bar">
          <div class="progress-fill" style="width:${pct}%;background:${CAT_COLORS[cat]}"></div>
        </div>
      </div>
    `;
  }).join('');
}

function renderVsAverage(summary) {
  const el = document.getElementById('vs-average');
  if (!el) return;
  const yours   = summary.totalCO2eSaved ?? 0;
  const avg      = 5.5; // kg/week global average approximation
  const better   = yours >= avg;

  el.innerHTML = `
    <div style="display:flex;align-items:center;gap:16px;padding:12px;background:${better?'rgba(58,183,149,0.08)':'rgba(224,122,95,0.08)'};border-radius:12px">
      <div style="font-size:36px">${better ? '🌟' : '💪'}</div>
      <div>
        <div style="font-size:15px;font-weight:700;color:${better?'var(--color-success)':'var(--color-warning)'}">
          ${better ? `${(yours - avg).toFixed(1)} kg better than average!` : `${(avg - yours).toFixed(1)} kg below target`}
        </div>
        <div style="font-size:13px;color:var(--color-text-secondary)">
          You saved ${yours.toFixed(1)} kg · Community avg: ${avg} kg
        </div>
      </div>
    </div>
  `;
}
