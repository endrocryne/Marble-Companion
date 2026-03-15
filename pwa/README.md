# Marble Companion — Progressive Web App

A fully functional PWA version of the Marble Companion eco-habit tracking app. All features of the original .NET MAUI app are retained and accessible from any modern browser.

## Features

- 🌳 **Companion Tree** — Canvas-rendered tree that grows with your eco actions (6 species × 6 stages × 4 health states)
- ✅ **Habit Tracking** — Active habits with daily/weekly check-ins, streaks, and Leaf Points rewards
- 📚 **Habit Library** — Browse 80+ pre-authored sustainable habits, searchable and filterable by category
- 📊 **Dashboard** — CO₂e trends (line chart), category breakdown (donut chart), period selectors
- 📝 **Action Logging** — Quick log or detailed log for eco-friendly actions (transport, food, energy, waste, shopping, travel)
- 🏆 **Challenges** — Curated and custom challenges with progress tracking
- 🥇 **Achievements** — Milestone achievements with LP rewards
- 👥 **Social Feed** — Activity feed with reactions (Inspired 💪, Strong 💚, Together 🤝, Fire 🔥)
- 🤝 **Friends** — Search users, send/accept friend requests
- 📖 **Content Library** — Educational articles with bookmarking and LP rewards for reading
- 🌱 **Carbon Offsets** — Redeem Leaf Points for symbolic carbon offsets (Plant Tree, Offset Carbon, Fund Wind)
- 💡 **Suggestions** — AI-powered personalized habit recommendations based on your activity
- ⚙️ **Settings** — Notification preferences (habit reminders, challenge updates, friend activity, etc.)

## PWA Capabilities

- **Installable** — Add to home screen on Android/iOS/desktop
- **Offline Support** — Cache-first for static assets, offline queue for check-ins and action logs
- **Background Sync** — Offline actions sync automatically when connection is restored
- **Web App Manifest** — Full PWA manifest with shortcuts for quick access to Log and Habits

## Setup

### Prerequisites

- The **Marble Companion API** running (see `MarbleCompanion.API/` for setup instructions)
- A **Google Cloud OAuth 2.0 Client ID** for authentication (create one at [console.cloud.google.com](https://console.cloud.google.com))

### Configuration

Edit the configuration block in `index.html`:

```html
<script>
  // Your Google OAuth 2.0 Client ID
  window.GOOGLE_CLIENT_ID = 'YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com';

  // API base URL — leave empty for same-origin, or set to your API server URL
  window.API_BASE_URL = 'https://api.marblecompanion.com';
</script>
```

### Serving

Any static file server works. Examples:

```bash
# Python (quick local test)
cd pwa && python3 -m http.server 8000

# Node.js with serve
npx serve pwa

# nginx / Apache — point document root to the pwa/ directory
```

> **Note:** Service workers require HTTPS in production (or `localhost` for development).

### Docker (with the API)

The existing `docker-compose.yml` starts the API. Serve the `pwa/` folder via nginx on port 80 alongside it for a full local stack.

## File Structure

```
pwa/
├── index.html              # App shell (SPA entry point)
├── manifest.json           # PWA web app manifest
├── sw.js                   # Service worker (cache strategies + background sync)
├── css/
│   └── app.css             # Complete design system (CSS custom properties, components)
├── js/
│   ├── app.js              # Router, navigation, toasts, LP popup
│   ├── api.js              # Typed API client (all 40+ endpoints, auto-refresh on 401)
│   ├── auth.js             # JWT storage, Google/Microsoft sign-in helpers
│   ├── db.js               # IndexedDB wrapper (offline queue, tree/habit cache)
│   ├── tree-canvas.js      # Canvas 2D tree renderer with animation
│   └── pages/
│       ├── onboarding.js   # 3-slide carousel + OAuth sign-in
│       ├── setup.js        # Profile setup + baseline quiz
│       ├── home.js         # Tree + stats + quick actions
│       ├── dashboard.js    # Charts (line/donut) + period selector
│       ├── habits.js       # Active habits + check-in
│       ├── habit-library.js# Searchable habit library
│       ├── log.js          # Quick + detailed action log
│       ├── social.js       # Feed + Friends (two-tab page)
│       ├── challenges.js   # Curated + my challenges
│       ├── achievements.js # Achievement grid
│       ├── content.js      # Content library list
│       ├── content-detail.js # Article reader + bookmark
│       ├── offsets.js      # Offset credits + history
│       ├── profile.js      # User profile + edit
│       ├── settings.js     # Notification preferences
│       └── suggestions.js  # Personalized habit suggestions
└── icons/
    ├── icon-192.png
    └── icon-512.png
```

## Authentication

The PWA uses **OAuth 2.0** (same as the mobile app):

- **Google Sign-In** — Uses Google Identity Services (`accounts.google.com/gsi/client`). Requires `GOOGLE_CLIENT_ID` to be configured.
- **Microsoft Sign-In** — Redirects to Microsoft OAuth (requires `MICROSOFT_CLIENT_ID` configuration in a future update).

After sign-in, the API returns a JWT access token and refresh token. These are stored in `localStorage` and automatically refreshed when they expire.

## Technology Stack

| Layer | Technology |
|-------|-----------|
| UI Framework | Vanilla HTML/CSS/JS (no build step required) |
| Charts | [Chart.js 4.4](https://www.chartjs.org/) via CDN |
| Tree Rendering | Canvas 2D API (custom renderer) |
| Offline Storage | IndexedDB (native browser API) |
| Auth | Google Identity Services + JWT |
| Service Worker | Workbox-compatible patterns (hand-written) |
| API Client | Fetch API with auto-refresh |
