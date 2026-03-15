/**
 * onboarding.js — Sign-in / onboarding carousel page
 */

import { signInWithGoogle, signInWithMicrosoft } from '../auth.js';
import { setPageContent, showToast, showLoading } from '../app.js';

const SLIDES = [
  {
    emoji: '🌳',
    title: 'Grow Your Tree',
    desc: 'Every eco-friendly action you take helps your virtual companion tree flourish and level up.',
  },
  {
    emoji: '📊',
    title: 'Track Your Footprint',
    desc: 'Log daily habits and actions across transport, food, energy, and more to see your real-world impact.',
  },
  {
    emoji: '🤝',
    title: 'Challenge Friends',
    desc: 'Join community challenges, compete with friends, and earn Leaf Points for a greener planet.',
  },
];

let currentSlide = 0;
let slideInterval = null;

export async function init() {
  setPageContent(`
    <div class="onboarding-page">
      <div class="carousel">
        <div class="carousel-slides" id="carousel-slides">
          ${SLIDES.map((s, i) => `
            <div class="carousel-slide" style="transform: translateX(${i * 100}%)">
              <div class="carousel-emoji">${s.emoji}</div>
              <h1 class="carousel-title">${s.title}</h1>
              <p class="carousel-desc">${s.desc}</p>
            </div>
          `).join('')}
        </div>
        <div class="carousel-dots" id="carousel-dots">
          ${SLIDES.map((_, i) => `<div class="dot ${i === 0 ? 'active' : ''}" data-index="${i}"></div>`).join('')}
        </div>
      </div>

      <div class="auth-buttons">
        <button class="btn-google" id="btn-google">
          <svg width="20" height="20" viewBox="0 0 24 24">
            <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
            <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
            <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z"/>
            <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
          </svg>
          Continue with Google
        </button>
        <button class="btn-microsoft" id="btn-microsoft">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="white">
            <path d="M11.4 24H0V12.6h11.4V24zM24 24H12.6V12.6H24V24zM11.4 11.4H0V0h11.4v11.4zM24 11.4H12.6V0H24v11.4z"/>
          </svg>
          Continue with Microsoft
        </button>
        <p style="text-align:center;font-size:12px;color:rgba(255,255,255,0.6);margin-top:8px;">
          By continuing you agree to our Terms of Service and Privacy Policy.
        </p>
      </div>
    </div>
  `);

  setupCarousel();
  setupAuthButtons();

  return () => {
    clearInterval(slideInterval);
  };
}

function setupCarousel() {
  currentSlide = 0;
  slideInterval = setInterval(() => goToSlide((currentSlide + 1) % SLIDES.length), 4000);

  document.querySelectorAll('.dot').forEach((dot) => {
    dot.addEventListener('click', () => goToSlide(parseInt(dot.dataset.index)));
  });

  // Touch swipe
  let startX = 0;
  const slides = document.getElementById('carousel-slides');
  slides?.addEventListener('touchstart', (e) => { startX = e.touches[0].clientX; }, { passive: true });
  slides?.addEventListener('touchend', (e) => {
    const diff = startX - e.changedTouches[0].clientX;
    if (Math.abs(diff) > 40) goToSlide(diff > 0
      ? Math.min(currentSlide + 1, SLIDES.length - 1)
      : Math.max(currentSlide - 1, 0));
  }, { passive: true });
}

function goToSlide(index) {
  currentSlide = index;
  document.querySelectorAll('.carousel-slide').forEach((s, i) => {
    s.style.transform = `translateX(${(i - index) * 100}%)`;
  });
  document.querySelectorAll('.dot').forEach((d, i) => {
    d.classList.toggle('active', i === index);
  });
}

function setupAuthButtons() {
  document.getElementById('btn-google')?.addEventListener('click', handleGoogleSignIn);
  document.getElementById('btn-microsoft')?.addEventListener('click', handleMicrosoftSignIn);
}

// Google OAuth client ID - configure in index.html or via window.GOOGLE_CLIENT_ID
// Set window.GOOGLE_CLIENT_ID = 'your-client-id' in a config script before loading the app
const GOOGLE_CLIENT_ID = (typeof window !== 'undefined' && window.GOOGLE_CLIENT_ID) || '';

async function handleGoogleSignIn() {
  if (typeof google === 'undefined') {
    showToast('Google Sign-In is loading, please try again in a moment.', 'warning');
    return;
  }

  if (!GOOGLE_CLIENT_ID) {
    showToast('Google Sign-In is not configured. Please contact the app administrator.', 'error');
    return;
  }

  google.accounts.id.initialize({
    client_id: GOOGLE_CLIENT_ID,
    callback: async (response) => {
      showLoading(true);
      try {
        const user = await signInWithGoogle(response.credential);
        clearInterval(slideInterval);
        const { needsSetup } = await import('../auth.js');
        window.location.hash = needsSetup() ? '#/setup' : '#/home';
      } catch (err) {
        showToast('Sign-in failed: ' + err.message, 'error');
      } finally {
        showLoading(false);
      }
    },
    auto_select: false,
    cancel_on_tap_outside: true,
  });

  google.accounts.id.prompt((notification) => {
    if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
      showToast('Please allow pop-ups to sign in with Google.', 'warning');
    }
  });
}

async function handleMicrosoftSignIn() {
  showToast('Microsoft sign-in: configure MSAL with your tenant ID to enable.', 'info');
}
