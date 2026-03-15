/**
 * setup.js — New user onboarding setup page
 */

import * as api from '../api.js';
import { setPageContent, showToast, showLoading, navigate } from '../app.js';
import { saveAuth, getUser } from '../auth.js';

const CONTINENTS = ['Africa','Antarctica','Asia','Europe','North America','Oceania','South America'];

const QUIZ_QUESTIONS = [
  {
    key: 'diet',
    label: 'Diet',
    emoji: '🍽️',
    options: [
      { value: 'vegan', label: 'Vegan' },
      { value: 'vegetarian', label: 'Vegetarian' },
      { value: 'pescatarian', label: 'Pescatarian' },
      { value: 'flexitarian', label: 'Flexitarian' },
      { value: 'omnivore', label: 'Omnivore' },
      { value: 'high-meat', label: 'High Meat' },
    ],
  },
  {
    key: 'transport',
    label: 'Primary Transport',
    emoji: '🚌',
    options: [
      { value: 'walking', label: '🚶 Walking' },
      { value: 'cycling', label: '🚴 Cycling' },
      { value: 'public', label: '🚌 Public Transit' },
      { value: 'ev', label: '⚡ Electric Car' },
      { value: 'hybrid', label: '🔋 Hybrid Car' },
      { value: 'petrol', label: '🚗 Petrol Car' },
    ],
  },
  {
    key: 'energy',
    label: 'Home Energy',
    emoji: '⚡',
    options: [
      { value: 'renewables', label: '☀️ Renewables' },
      { value: 'partial', label: '⚡ Partial Green' },
      { value: 'grid', label: '🔌 Grid Mix' },
      { value: 'coal', label: '🏭 Coal Heavy' },
    ],
  },
  {
    key: 'flights',
    label: 'Flights per Year',
    emoji: '✈️',
    options: [
      { value: 'zero', label: '0 Flights' },
      { value: 'one-two', label: '1-2 Flights' },
      { value: 'three-five', label: '3-5 Flights' },
      { value: 'six-plus', label: '6+ Flights' },
    ],
  },
  {
    key: 'shopping',
    label: 'Shopping Habits',
    emoji: '🛍️',
    options: [
      { value: 'minimal', label: '📦 Minimal' },
      { value: 'secondhand', label: '♻️ Secondhand' },
      { value: 'moderate', label: '🛒 Moderate' },
      { value: 'frequent', label: '🏪 Frequent' },
      { value: 'heavy', label: '💳 Heavy' },
    ],
  },
];

let formData = {
  displayName: '',
  avatarIndex: 0,
  regionContinent: '',
  regionCountry: '',
  baselineQuizAnswers: {},
};

export async function init() {
  const user = getUser();
  formData.displayName = user?.displayName || '';

  setPageContent(`
    <div class="setup-page">
      <div class="setup-logo">🌿</div>
      <h1 class="setup-title">Set Up Your Profile</h1>
      <p class="setup-subtitle">Let's personalize your Marble experience</p>

      <!-- Name -->
      <div class="setup-section">
        <div class="setup-section-title">Display Name</div>
        <div class="form-group" style="margin-bottom:0">
          <input type="text" class="form-input" id="setup-name" placeholder="Your name" maxlength="50" value="${formData.displayName}" />
        </div>
      </div>

      <!-- Avatar -->
      <div class="setup-section">
        <div class="setup-section-title">Choose Avatar</div>
        <div class="avatar-picker" id="avatar-picker">
          ${[0,1,2,3,4,5,6,7].map((i) => `
            <div class="avatar-option ${i === 0 ? 'selected' : ''}" data-avatar="${i}" role="button" aria-label="Avatar ${i+1}">
              <div class="avatar avatar-lg avatar-${i}">${['🌿','🌊','🌻','🦋','🦜','🌙','❄️','🔥'][i]}</div>
            </div>
          `).join('')}
        </div>
      </div>

      <!-- Region -->
      <div class="setup-section">
        <div class="setup-section-title">Region (Optional)</div>
        <div class="form-group">
          <label class="form-label">Continent</label>
          <select class="form-input" id="setup-continent">
            <option value="">Select continent…</option>
            ${CONTINENTS.map((c) => `<option value="${c}">${c}</option>`).join('')}
          </select>
        </div>
        <div class="form-group" style="margin-bottom:0">
          <label class="form-label">Country</label>
          <input type="text" class="form-input" id="setup-country" placeholder="e.g. United Kingdom" />
        </div>
      </div>

      <!-- Baseline Quiz -->
      ${QUIZ_QUESTIONS.map((q) => `
        <div class="setup-section">
          <div class="setup-section-title">${q.emoji} ${q.label}</div>
          <div class="chip-group" id="quiz-${q.key}">
            ${q.options.map((opt) => `
              <div class="chip" data-quiz="${q.key}" data-value="${opt.value}">${opt.label}</div>
            `).join('')}
          </div>
        </div>
      `).join('')}

      <button class="btn btn-primary btn-full btn-lg mt-lg" id="btn-setup-submit">
        Start My Journey 🌱
      </button>
    </div>
  `);

  bindEvents();

  return () => {};
}

function bindEvents() {
  document.getElementById('setup-name')?.addEventListener('input', (e) => {
    formData.displayName = e.target.value;
  });

  document.getElementById('avatar-picker')?.addEventListener('click', (e) => {
    const option = e.target.closest('.avatar-option');
    if (!option) return;
    document.querySelectorAll('.avatar-option').forEach((o) => o.classList.remove('selected'));
    option.classList.add('selected');
    formData.avatarIndex = parseInt(option.dataset.avatar);
  });

  document.getElementById('setup-continent')?.addEventListener('change', (e) => {
    formData.regionContinent = e.target.value;
  });

  document.getElementById('setup-country')?.addEventListener('input', (e) => {
    formData.regionCountry = e.target.value;
  });

  document.querySelectorAll('.chip[data-quiz]').forEach((chip) => {
    chip.addEventListener('click', () => {
      const quiz = chip.dataset.quiz;
      // Single-select per quiz question
      document.querySelectorAll(`.chip[data-quiz="${quiz}"]`).forEach((c) => c.classList.remove('active'));
      chip.classList.add('active');
      formData.baselineQuizAnswers[quiz] = chip.dataset.value;
    });
  });

  document.getElementById('btn-setup-submit')?.addEventListener('click', handleSubmit);
}

async function handleSubmit() {
  if (!formData.displayName.trim()) {
    showToast('Please enter a display name', 'error');
    return;
  }

  showLoading(true);
  try {
    const dto = {
      displayName: formData.displayName.trim(),
      avatarIndex: formData.avatarIndex,
      regionContinent: formData.regionContinent || undefined,
      regionCountry:   formData.regionCountry   || undefined,
      baselineQuizAnswers: formData.baselineQuizAnswers,
    };

    await api.setupUser(dto);

    // Refresh user profile
    const updated = await api.getMe();
    const token   = localStorage.getItem('mc_token');
    const refresh = localStorage.getItem('mc_refresh');
    const expires = localStorage.getItem('mc_expires');
    saveAuth(token, refresh, expires, updated);

    showToast('Profile set up! Welcome to Marble 🌿', 'success');
    navigate('home');
  } catch (err) {
    showToast('Setup failed: ' + err.message, 'error');
  } finally {
    showLoading(false);
  }
}
