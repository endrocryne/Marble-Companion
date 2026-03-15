/**
 * tree-canvas.js — Canvas-based companion tree renderer
 * Draws species-specific trees that scale with growth stage and health state.
 */

const SPECIES_CONFIG = {
  Oak: {
    trunkColor: '#5D4037',
    canopyColors: { Healthy: ['#2E7D32','#388E3C','#43A047'], Stressed: ['#8BC34A','#9CCC65','#AED581'], Withering: ['#A5795A','#8D6E63','#795548'], Dormant: ['#78909C','#90A4AE','#B0BEC5'] },
    drawCanopy: drawOakCanopy,
  },
  Maple: {
    trunkColor: '#6D4C41',
    canopyColors: { Healthy: ['#D32F2F','#E53935','#EF5350'], Stressed: ['#EF9A9A','#FFCDD2','#F48FB1'], Withering: ['#A1887F','#BCAAA4','#D7CCC8'], Dormant: ['#78909C','#90A4AE','#B0BEC5'] },
    drawCanopy: drawMapleCanopy,
  },
  Willow: {
    trunkColor: '#4E342E',
    canopyColors: { Healthy: ['#33691E','#558B2F','#689F38'], Stressed: ['#9CCC65','#C5E1A5','#DCEDC8'], Withering: ['#8D6E63','#A1887F','#BCAAA4'], Dormant: ['#90A4AE','#B0BEC5','#CFD8DC'] },
    drawCanopy: drawWillowCanopy,
  },
  Baobab: {
    trunkColor: '#795548',
    canopyColors: { Healthy: ['#388E3C','#2E7D32','#1B5E20'], Stressed: ['#AED581','#C5E1A5','#DCEDC8'], Withering: ['#A1887F','#8D6E63','#795548'], Dormant: ['#90A4AE','#78909C','#607D8B'] },
    drawCanopy: drawBaobabCanopy,
  },
  CherryBlossom: {
    trunkColor: '#5D4037',
    canopyColors: { Healthy: ['#F48FB1','#F06292','#EC407A'], Stressed: ['#F8BBD9','#FCE4EC','#FFDCE6'], Withering: ['#BCAAA4','#D7CCC8','#EFEBE9'], Dormant: ['#90A4AE','#B0BEC5','#CFD8DC'] },
    drawCanopy: drawCherryBlossomCanopy,
  },
  Mangrove: {
    trunkColor: '#3E2723',
    canopyColors: { Healthy: ['#00796B','#00897B','#009688'], Stressed: ['#80CBC4','#B2DFDB','#E0F2F1'], Withering: ['#8D6E63','#A1887F','#BCAAA4'], Dormant: ['#78909C','#90A4AE','#B0BEC5'] },
    drawCanopy: drawMangroveCanopy,
  },
};

// Stage size multipliers: 0=Seed, 1=Seedling, 2=Sapling, 3=Young, 4=Mature, 5=Ancient
const STAGE_SCALE = [0.08, 0.2, 0.38, 0.58, 0.8, 1.0];

// Active animation state per canvas
const _animState = new WeakMap();

/**
 * Main entry point — draws and animates the tree on a canvas element.
 * @param {HTMLCanvasElement} canvas
 * @param {object} treeDto - { species, stage, healthState, totalLP, stageName }
 */
export function drawTree(canvas, treeDto) {
  if (!canvas || !treeDto) return;

  // Stop any previous animation on this canvas
  const prev = _animState.get(canvas);
  if (prev && prev.rafId) cancelAnimationFrame(prev.rafId);

  const state = { phase: 0, running: true, rafId: null };
  _animState.set(canvas, state);

  function loop() {
    if (!state.running) return;
    state.phase += 0.018;
    renderTree(canvas, treeDto, state.phase);
    state.rafId = requestAnimationFrame(loop);
  }

  loop();
}

/** Stop the animation on a canvas (call when page navigates away) */
export function stopTree(canvas) {
  const state = _animState.get(canvas);
  if (state) {
    state.running = false;
    if (state.rafId) cancelAnimationFrame(state.rafId);
  }
}

function renderTree(canvas, dto, phase) {
  const dpr = window.devicePixelRatio || 1;
  const w   = canvas.clientWidth;
  const h   = canvas.clientHeight;

  if (canvas.width !== w * dpr || canvas.height !== h * dpr) {
    canvas.width  = w * dpr;
    canvas.height = h * dpr;
  }

  const ctx = canvas.getContext('2d');
  ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

  const species    = dto.species || 'Oak';
  const stage      = Math.min(Math.max(dto.stage ?? 3, 0), 5);
  const health     = dto.healthState || 'Healthy';
  const scale      = STAGE_SCALE[stage];
  const config     = SPECIES_CONFIG[species] || SPECIES_CONFIG.Oak;
  const colors     = config.canopyColors[health] || config.canopyColors.Healthy;

  drawBackground(ctx, w, h, health);

  const groundY = h * 0.82;
  const cx      = w / 2;

  if (stage === 0) {
    drawSeed(ctx, cx, groundY, colors[0]);
    return;
  }

  const trunkH = h * 0.38 * scale;
  const trunkW = Math.max(6, w * 0.04 * scale);

  drawTrunk(ctx, cx, groundY, trunkH, trunkW, config.trunkColor, health, species);
  config.drawCanopy(ctx, cx, groundY - trunkH, w, h, scale, colors, phase, health);

  if (stage >= 2) {
    drawGrass(ctx, cx, groundY, w, health);
  }
}

function drawBackground(ctx, w, h, health) {
  const skyColors = {
    Healthy:   ['#C8E6F0', '#E0F4F1'],
    Stressed:  ['#D6E4E8', '#E8F0E4'],
    Withering: ['#D4C4B4', '#E8DDD0'],
    Dormant:   ['#C8D0D4', '#D8DDE0'],
  };
  const sky = skyColors[health] || skyColors.Healthy;

  const grad = ctx.createLinearGradient(0, 0, 0, h * 0.75);
  grad.addColorStop(0, sky[0]);
  grad.addColorStop(1, sky[1]);
  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, w, h);

  // Ground
  const groundGrad = ctx.createLinearGradient(0, h * 0.75, 0, h);
  const gc = health === 'Healthy' ? ['#A5D6A7','#81C784'] : health === 'Dormant' ? ['#B0BEC5','#90A4AE'] : ['#BCAAA4','#A1887F'];
  groundGrad.addColorStop(0, gc[0]);
  groundGrad.addColorStop(1, gc[1]);
  ctx.fillStyle = groundGrad;
  ctx.fillRect(0, h * 0.75, w, h * 0.25);
}

function drawSeed(ctx, cx, groundY, color) {
  ctx.save();
  ctx.beginPath();
  ctx.arc(cx, groundY - 8, 8, 0, Math.PI * 2);
  ctx.fillStyle = color;
  ctx.fill();
  ctx.restore();
}

function drawTrunk(ctx, cx, groundY, trunkH, trunkW, trunkColor, health, species) {
  const alpha = health === 'Dormant' ? 0.7 : 1;
  ctx.save();
  ctx.globalAlpha = alpha;

  if (species === 'Baobab') {
    // Baobab has a fat bulging trunk
    ctx.beginPath();
    ctx.moveTo(cx - trunkW * 0.6, groundY);
    ctx.bezierCurveTo(cx - trunkW * 1.5, groundY - trunkH * 0.3, cx - trunkW * 0.5, groundY - trunkH * 0.7, cx, groundY - trunkH);
    ctx.bezierCurveTo(cx + trunkW * 0.5, groundY - trunkH * 0.7, cx + trunkW * 1.5, groundY - trunkH * 0.3, cx + trunkW * 0.6, groundY);
    ctx.fillStyle = trunkColor;
    ctx.fill();
  } else if (species === 'Willow') {
    // Slightly curved trunk
    ctx.beginPath();
    ctx.moveTo(cx - trunkW / 2, groundY);
    ctx.quadraticCurveTo(cx + trunkW * 2, groundY - trunkH * 0.5, cx + trunkW / 2, groundY - trunkH);
    ctx.quadraticCurveTo(cx + trunkW * 2 + trunkW, groundY - trunkH * 0.5, cx + trunkW * 1.5, groundY);
    ctx.fillStyle = trunkColor;
    ctx.fill();
  } else {
    // Standard trunk
    const grad = ctx.createLinearGradient(cx - trunkW, 0, cx + trunkW, 0);
    grad.addColorStop(0, darken(trunkColor, 20));
    grad.addColorStop(0.4, trunkColor);
    grad.addColorStop(1, darken(trunkColor, 10));
    ctx.beginPath();
    ctx.moveTo(cx - trunkW / 2, groundY);
    ctx.lineTo(cx - trunkW / 3, groundY - trunkH);
    ctx.lineTo(cx + trunkW / 3, groundY - trunkH);
    ctx.lineTo(cx + trunkW / 2, groundY);
    ctx.fillStyle = grad;
    ctx.fill();
  }

  ctx.restore();
}

// ── Canopy Renderers ─────────────────────────────────────────────────────────

function drawOakCanopy(ctx, cx, apexY, w, h, scale, colors, phase, health) {
  const r = w * 0.28 * scale;
  const sway = Math.sin(phase) * 2 * scale;

  ctx.save();
  // Shadow blob
  ctx.globalAlpha = 0.15;
  ctx.beginPath();
  ctx.ellipse(cx + sway, apexY + r * 0.15, r * 0.85, r * 0.35, 0, 0, Math.PI * 2);
  ctx.fillStyle = '#000';
  ctx.fill();
  ctx.globalAlpha = 1;

  // Three overlapping circles for oak shape
  const blobs = [
    { dx: -r * 0.3 + sway * 0.5, dy: r * 0.15, r: r * 0.75, c: colors[2] },
    { dx:  r * 0.3 + sway * 0.5, dy: r * 0.1,  r: r * 0.75, c: colors[2] },
    { dx:       sway,             dy: -r * 0.1, r: r * 0.85, c: colors[1] },
  ];

  blobs.forEach(({ dx, dy, r: br, c }) => {
    ctx.beginPath();
    ctx.arc(cx + dx, apexY + dy, br, 0, Math.PI * 2);
    ctx.fillStyle = c;
    ctx.fill();
  });

  // Top highlight
  ctx.beginPath();
  ctx.arc(cx + sway, apexY - r * 0.1, r * 0.65, 0, Math.PI * 2);
  ctx.fillStyle = colors[0];
  ctx.fill();

  if (health === 'Healthy') drawLeafParticles(ctx, cx, apexY, r, phase, colors[0]);

  ctx.restore();
}

function drawMapleCanopy(ctx, cx, apexY, w, h, scale, colors, phase, health) {
  const r = w * 0.24 * scale;
  const sway = Math.sin(phase * 0.9) * 2.5 * scale;

  ctx.save();
  // Layered maple shape (triangular clusters)
  const layers = [
    { dy: r * 0.3,  rx: r * 0.9, ry: r * 0.45, c: colors[2] },
    { dy: 0,        rx: r * 0.8, ry: r * 0.42, c: colors[1] },
    { dy: -r * 0.3, rx: r * 0.6, ry: r * 0.38, c: colors[0] },
  ];

  layers.forEach(({ dy, rx, ry, c }) => {
    ctx.beginPath();
    ctx.ellipse(cx + sway, apexY + dy, rx, ry, 0, 0, Math.PI * 2);
    ctx.fillStyle = c;
    ctx.fill();
  });

  if (health === 'Healthy') drawLeafParticles(ctx, cx, apexY, r, phase, colors[0]);
  ctx.restore();
}

function drawWillowCanopy(ctx, cx, apexY, w, h, scale, colors, phase, health) {
  const r = w * 0.22 * scale;
  const sway = Math.sin(phase * 0.7) * 3 * scale;

  ctx.save();
  // Main canopy dome
  ctx.beginPath();
  ctx.arc(cx + sway * 0.3, apexY, r, 0, Math.PI * 2);
  ctx.fillStyle = colors[1];
  ctx.fill();

  ctx.beginPath();
  ctx.arc(cx + sway * 0.3, apexY - r * 0.2, r * 0.7, 0, Math.PI * 2);
  ctx.fillStyle = colors[0];
  ctx.fill();

  // Drooping branches
  const branchCount = Math.max(6, Math.round(10 * scale));
  for (let i = 0; i < branchCount; i++) {
    const angle = (i / branchCount) * Math.PI * 2;
    const bx    = cx + Math.cos(angle) * r * 0.75 + sway;
    const by    = apexY + Math.sin(angle) * r * 0.4;
    const dropY = by + r * (0.5 + 0.3 * Math.abs(Math.cos(angle)));
    const swing = Math.sin(phase + i) * 4 * scale;

    ctx.beginPath();
    ctx.moveTo(bx, by);
    ctx.quadraticCurveTo(bx + swing, by + (dropY - by) * 0.5, bx + swing * 1.5, dropY);
    ctx.strokeStyle = colors[0];
    ctx.lineWidth = 1.5 * scale;
    ctx.globalAlpha = 0.7;
    ctx.stroke();
  }
  ctx.globalAlpha = 1;
  ctx.restore();
}

function drawBaobabCanopy(ctx, cx, apexY, w, h, scale, colors, phase, health) {
  const r = w * 0.2 * scale;
  const sway = Math.sin(phase * 0.6) * 1.5 * scale;

  ctx.save();
  // Baobab: wide flat canopy with gnarly branch tips
  ctx.beginPath();
  ctx.ellipse(cx + sway, apexY + r * 0.1, r * 1.3, r * 0.55, 0, 0, Math.PI * 2);
  ctx.fillStyle = colors[2];
  ctx.fill();

  // Add irregular blobs at branch tips
  const tips = [-1.1, -0.55, 0, 0.55, 1.1];
  tips.forEach((t, i) => {
    const blobSway = Math.sin(phase + i * 0.8) * 2 * scale;
    ctx.beginPath();
    ctx.arc(cx + t * r + blobSway, apexY - r * 0.15, r * 0.32, 0, Math.PI * 2);
    ctx.fillStyle = i % 2 === 0 ? colors[0] : colors[1];
    ctx.fill();
  });

  ctx.restore();
}

function drawCherryBlossomCanopy(ctx, cx, apexY, w, h, scale, colors, phase, health) {
  const r = w * 0.26 * scale;
  const sway = Math.sin(phase * 0.85) * 2.5 * scale;

  ctx.save();
  // Base cloud shape
  const blobs = [
    { dx: -r * 0.4, dy: r * 0.15, r: r * 0.65 },
    { dx:  r * 0.4, dy: r * 0.12, r: r * 0.65 },
    { dx: 0,        dy: -r * 0.1, r: r * 0.75 },
    { dx: -r * 0.7, dy: -r * 0.1, r: r * 0.5 },
    { dx:  r * 0.7, dy: -r * 0.1, r: r * 0.5 },
  ];

  blobs.forEach(({ dx, dy, r: br }, i) => {
    ctx.beginPath();
    ctx.arc(cx + dx + sway, apexY + dy, br, 0, Math.PI * 2);
    ctx.fillStyle = i < 2 ? colors[2] : i === 2 ? colors[1] : colors[0];
    ctx.fill();
  });

  // Floating petal particles
  if (health === 'Healthy' || health === 'Stressed') {
    drawBlossomPetals(ctx, cx, apexY, r, phase, colors[0]);
  }

  ctx.restore();
}

function drawMangroveCanopy(ctx, cx, apexY, w, h, scale, colors, phase, health) {
  const r = w * 0.22 * scale;
  const groundY = h * 0.82;
  const sway = Math.sin(phase * 0.8) * 2 * scale;

  ctx.save();
  // Aerial prop roots below canopy
  const rootCount = Math.max(3, Math.round(5 * scale));
  for (let i = 0; i < rootCount; i++) {
    const rx   = cx + (i - rootCount / 2) * r * 0.4 + sway * 0.3;
    const ry   = apexY + r * 0.3;
    const rxEnd = rx + (i - rootCount / 2) * r * 0.2;

    ctx.beginPath();
    ctx.moveTo(rx, ry);
    ctx.quadraticCurveTo(rxEnd, ry + (groundY - ry) * 0.5, rxEnd, groundY * 0.98);
    ctx.strokeStyle = SPECIES_CONFIG.Mangrove.trunkColor;
    ctx.lineWidth = 2 * scale;
    ctx.globalAlpha = 0.6;
    ctx.stroke();
  }
  ctx.globalAlpha = 1;

  // Main dome canopy
  ctx.beginPath();
  ctx.arc(cx + sway * 0.5, apexY, r, 0, Math.PI * 2);
  ctx.fillStyle = colors[1];
  ctx.fill();

  ctx.beginPath();
  ctx.arc(cx + sway * 0.5, apexY - r * 0.2, r * 0.7, 0, Math.PI * 2);
  ctx.fillStyle = colors[0];
  ctx.fill();

  if (health === 'Healthy') drawLeafParticles(ctx, cx, apexY, r, phase, colors[0]);

  ctx.restore();
}

// ── Particle Effects ──────────────────────────────────────────────────────────

function drawLeafParticles(ctx, cx, cy, r, phase, color) {
  const count = 8;
  ctx.save();
  for (let i = 0; i < count; i++) {
    const angle = (i / count) * Math.PI * 2 + phase * 0.5;
    const dist  = r * (0.7 + 0.3 * Math.sin(phase + i));
    const x     = cx + Math.cos(angle) * dist;
    const y     = cy + Math.sin(angle) * dist * 0.6;
    const size  = 4 + 2 * Math.sin(phase * 1.2 + i);

    ctx.globalAlpha = 0.5 + 0.3 * Math.sin(phase + i * 1.3);
    ctx.beginPath();
    ctx.ellipse(x, y, size, size * 0.6, angle, 0, Math.PI * 2);
    ctx.fillStyle = color;
    ctx.fill();
  }
  ctx.globalAlpha = 1;
  ctx.restore();
}

function drawBlossomPetals(ctx, cx, cy, r, phase, color) {
  const count = 12;
  ctx.save();
  for (let i = 0; i < count; i++) {
    const t     = (phase * 0.3 + i / count) % 1;
    const angle = (i / count) * Math.PI * 2 + phase * 0.2;
    const x     = cx + Math.cos(angle) * r * (0.5 + t * 0.8);
    const y     = cy + t * r * 0.6 + Math.sin(phase + i) * 6;
    const size  = 5 * (1 - t * 0.5);

    ctx.globalAlpha = (1 - t) * 0.7;
    ctx.beginPath();
    ctx.arc(x, y, size, 0, Math.PI * 2);
    ctx.fillStyle = color;
    ctx.fill();
  }
  ctx.globalAlpha = 1;
  ctx.restore();
}

function drawGrass(ctx, cx, groundY, w, health) {
  const grassColor = health === 'Healthy' ? '#4CAF50' : health === 'Dormant' ? '#90A4AE' : '#A1887F';
  const count = 20;
  ctx.save();
  ctx.strokeStyle = grassColor;
  ctx.lineWidth = 1.5;
  ctx.globalAlpha = 0.6;

  for (let i = 0; i < count; i++) {
    const x    = (cx - w * 0.35) + (i / count) * w * 0.7;
    const gH   = 8 + (i % 3) * 4;
    const lean = (i % 2 === 0 ? 1 : -1) * 3;

    ctx.beginPath();
    ctx.moveTo(x, groundY);
    ctx.quadraticCurveTo(x + lean, groundY - gH * 0.5, x + lean * 1.5, groundY - gH);
    ctx.stroke();
  }
  ctx.restore();
}

// ── Utility ───────────────────────────────────────────────────────────────────

function darken(hex, pct) {
  const n = parseInt(hex.slice(1), 16);
  const r = Math.max(0, ((n >> 16) & 0xff) - pct);
  const g = Math.max(0, ((n >>  8) & 0xff) - pct);
  const b = Math.max(0, ( n        & 0xff) - pct);
  return `#${((r << 16) | (g << 8) | b).toString(16).padStart(6, '0')}`;
}
