/**
 * content-detail.js — Full article view with bookmark and LP reward
 */

import * as api from '../api.js';
import { setPageContent, showToast, showLPPopup, escapeHtml } from '../app.js';

const CAT_ICONS = { Transport:'🚌', Food:'🥦', Energy:'⚡', Shopping:'🛍️', Travel:'✈️', Waste:'♻️', General:'🌍' };
const DIFF_CLASS = { Beginner:'badge-easy', Intermediate:'badge-medium', Advanced:'badge-hard' };

export async function init(params = {}) {
  const { id } = params;

  if (!id) {
    setPageContent(`
      <div class="page">
        <div class="empty-state">
          <div class="empty-emoji">❓</div>
          <div class="empty-title">No article selected</div>
          <button class="btn btn-primary mt-md" onclick="window._app.navigate('content')">Browse Articles</button>
        </div>
      </div>
    `);
    return;
  }

  setPageContent(`
    <div class="page" style="padding-top:0">
      <!-- Header -->
      <div style="background:var(--color-primary);padding:16px;display:flex;align-items:center;gap:12px;color:#fff">
        <button class="btn btn-icon" style="color:#fff;background:rgba(255,255,255,0.15)" onclick="history.back()" aria-label="Back">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="15,18 9,12 15,6"/>
          </svg>
        </button>
        <div style="font-size:16px;font-weight:700;flex:1">Article</div>
        <button class="btn btn-icon" id="btn-bookmark" style="color:#fff;background:rgba(255,255,255,0.15)" aria-label="Bookmark">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M19 21l-7-5-7 5V5a2 2 0 012-2h10a2 2 0 012 2z"/>
          </svg>
        </button>
      </div>

      <div id="article-content" style="padding:16px">
        <div class="skeleton" style="height:32px;width:80%;margin-bottom:12px;border-radius:8px"></div>
        <div class="skeleton" style="height:200px;margin-bottom:16px;border-radius:12px"></div>
        ${Array(4).fill('<div class="skeleton" style="height:16px;margin-bottom:8px;border-radius:4px"></div>').join('')}
      </div>
    </div>
  `);

  await loadArticle(id);

  return () => {};
}

async function loadArticle(id) {
  try {
    const article = await api.getContentDetail(id);
    renderArticle(article);
  } catch (err) {
    document.getElementById('article-content').innerHTML = `
      <div class="empty-state">
        <div class="empty-emoji">📭</div>
        <div class="empty-title">Couldn't load article</div>
        <div class="empty-desc">${escapeHtml(err.message)}</div>
      </div>
    `;
  }
}

function renderArticle(a) {
  const el = document.getElementById('article-content');
  if (!el) return;

  const published = a.publishedAt ? new Date(a.publishedAt).toLocaleDateString(undefined, { year:'numeric', month:'long', day:'numeric' }) : '';

  el.innerHTML = `
    <!-- Meta -->
    <div style="display:flex;align-items:center;gap:8px;margin-bottom:12px;flex-wrap:wrap">
      <span style="font-size:20px">${CAT_ICONS[a.category] || '📄'}</span>
      <span class="badge ${DIFF_CLASS[a.difficulty] || 'badge-primary'}">${a.difficulty || 'Beginner'}</span>
      ${a.lpReward ? `<span class="badge badge-accent">🍃 +${a.lpReward} LP</span>` : ''}
      <span style="font-size:12px;color:var(--color-text-secondary);margin-left:auto">${published}</span>
    </div>

    <!-- Title -->
    <h1 style="font-size:22px;font-weight:800;color:var(--color-text-primary);line-height:1.3;margin-bottom:12px">
      ${escapeHtml(a.title)}
    </h1>

    <!-- Summary -->
    ${a.summary ? `
      <div style="background:rgba(13,115,119,0.06);border-left:3px solid var(--color-primary);padding:12px;border-radius:0 8px 8px 0;font-size:14px;color:var(--color-text-secondary);margin-bottom:16px;line-height:1.5">
        ${escapeHtml(a.summary)}
      </div>
    ` : ''}

    <!-- Hero Image -->
    ${a.imageUrl ? `<img src="${escapeHtml(a.imageUrl)}" alt="${escapeHtml(a.title)}" style="width:100%;border-radius:12px;margin-bottom:16px;max-height:220px;object-fit:cover" loading="lazy" />` : ''}

    <!-- Body -->
    <div style="font-size:15px;color:var(--color-text-primary);line-height:1.7" id="article-body">
      ${renderBody(a.body || '')}
    </div>

    <!-- Category Info -->
    <div class="fact-card mt-lg" style="margin-bottom:16px">
      <div class="fact-label">${CAT_ICONS[a.category] || '🌍'} Category: ${a.category || 'General'}</div>
      <div class="fact-text">Every small action in the ${a.category || 'general'} category adds up to meaningful change for our planet.</div>
    </div>

    <!-- LP Earn Button -->
    ${a.lpReward ? `
      <button class="btn btn-primary btn-full btn-lg" id="btn-mark-read">
        ✅ Mark as Read — Earn ${a.lpReward} LP
      </button>
    ` : ''}
  `;

  document.getElementById('btn-bookmark')?.addEventListener('click', () => handleBookmark(a.id));
  document.getElementById('btn-mark-read')?.addEventListener('click', () => handleMarkRead(a));
}

function renderBody(body) {
  if (!body) return '<p style="color:var(--color-text-secondary)">No content available.</p>';
  // Convert plain text paragraphs or basic markdown-ish formatting
  return body
    .split('\n\n')
    .filter(Boolean)
    .map((para) => {
      if (para.startsWith('## ')) return `<h2 style="font-size:18px;font-weight:700;margin:20px 0 8px">${escapeHtml(para.slice(3))}</h2>`;
      if (para.startsWith('# '))  return `<h2 style="font-size:20px;font-weight:800;margin:20px 0 8px">${escapeHtml(para.slice(2))}</h2>`;
      if (para.startsWith('- '))  {
        const items = para.split('\n').filter(l => l.startsWith('- '));
        return `<ul style="padding-left:20px;margin:8px 0">${items.map(l => `<li style="margin-bottom:4px">${escapeHtml(l.slice(2))}</li>`).join('')}</ul>`;
      }
      return `<p style="margin-bottom:12px">${escapeHtml(para)}</p>`;
    })
    .join('');
}

async function handleBookmark(id) {
  try {
    await api.bookmarkContent(id);
    const btn = document.getElementById('btn-bookmark');
    if (btn) {
      btn.style.background = 'rgba(50,224,196,0.3)';
    }
    showToast('Bookmarked! 🔖', 'success');
  } catch (err) {
    showToast('Failed to bookmark: ' + err.message, 'error');
  }
}

async function handleMarkRead(article) {
  const btn = document.getElementById('btn-mark-read');
  if (btn) { btn.disabled = true; btn.textContent = 'Saved!'; }
  showLPPopup(article.lpReward, 0);
}
