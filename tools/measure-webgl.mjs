// Measures a local WebGL build in its own headless Chrome (P57): frame times without the vsync cap, memory, console errors,
// screenshots, driven by real mouse and key events, the same scenario every run. Independent of the desktop session.
//   node tools/measure-webgl.mjs <build folder> <output folder> [scenario: full|quick]
// Env: K9_CPU=4 slows the CPU down fourfold (a weaker device); K9_NOTRAFFIC=1 stops the ships in Main before measuring.
// Output: <output>/result.json and screenshots. Nothing here ships with the game: the hook is injected by this server only.
import { spawn } from 'node:child_process';
import { createServer } from 'node:http';
import { existsSync, mkdirSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import { extname, join, normalize } from 'node:path';

const [buildDir, outDir, scenario = 'full'] = process.argv.slice(2);
if (!buildDir || !outDir) { console.error('usage: measure-webgl.mjs <build> <out> [full|quick]'); process.exit(2); }
mkdirSync(outDir, { recursive: true });

const CHROME = 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const WIDTH = 1600, HEIGHT = 900, FOOTER = 20, HTTP_PORT = 18094, DEBUG_PORT = 18095;
const sleep = ms => new Promise(r => setTimeout(r, ms));

// ---------- page hook ----------
const HOOK = `<script>
window.k9f={t:[],stamp:[],last:0,on:false,start:0};
(function loop(n){var f=window.k9f;if(f.on&&f.last){f.t.push(n-f.last);f.stamp.push(n-f.start);}f.last=n;requestAnimationFrame(loop);})(0);
window.k9start=function(){var f=k9f;f.t=[];f.stamp=[];f.start=performance.now();f.on=true;};
window.k9stop=function(){var f=k9f;f.on=false;var a=f.t.slice().sort(function(x,y){return x-y});var n=a.length;var s=a.reduce(function(p,c){return p+c},0);
 var pc=function(q){return n?+a[Math.min(n-1,Math.floor(n*q))].toFixed(2):0};var over=function(ms){return f.t.filter(function(x){return x>ms}).length};
 var spikes=f.t.map(function(v,i){return [+v.toFixed(1),Math.round(f.stamp[i])]}).sort(function(x,y){return y[0]-x[0]}).slice(0,8);
 return {frames:n,seconds:+(s/1000).toFixed(1),avgMs:+(s/Math.max(1,n)).toFixed(2),fps:+(1000*n/Math.max(1,s)).toFixed(1),p50:pc(.5),p95:pc(.95),p99:pc(.99),max:n?+a[n-1].toFixed(1):0,over16:over(16.7),over33:over(33.4),over100:over(100),spikes:spikes};};
window.k9mem=function(){var m=window.k9&&k9.GetMetricsInfo?k9.GetMetricsInfo():{};return {wasmTotalMB:Math.round((m.totalWASMHeapSize||0)/1048576),wasmUsedMB:Math.round((m.usedWASMHeapSize||0)/1048576),jsUsedMB:Math.round((m.usedJSHeapSize||0)/1048576),pageLoad:m.pageLoadTime,firstFrame:m.pageLoadTimeToFrame1,janked:m.numJankedFrames};};
</script></head>`;

// ---------- static server ----------
const TYPES = { '.html': 'text/html', '.js': 'application/javascript', '.wasm': 'application/wasm', '.unityweb': 'application/octet-stream', '.png': 'image/png', '.css': 'text/css' };
const server = createServer((req, res) => {
  const path = normalize(decodeURIComponent(req.url.split('?')[0])).replace(/^[\\/]+/, '') || 'index.html';
  const file = join(buildDir, path === '.' ? 'index.html' : path);
  if (!existsSync(file) || statSync(file).isDirectory()) { res.writeHead(404); res.end(); return; }
  res.writeHead(200, { 'Content-Type': TYPES[extname(file)] ?? 'application/octet-stream', 'Cache-Control': 'no-store' });
  if (extname(file) !== '.html') { res.end(readFileSync(file)); return; }
  res.end(readFileSync(file, 'utf8').replace('</head>', HOOK).replace('}).then(function () {', '}).then(function (i) { window.k9 = i;'));
}).listen(HTTP_PORT, '127.0.0.1');

// ---------- Chrome + CDP ----------
const chrome = spawn(CHROME, ['--headless=new', `--remote-debugging-port=${DEBUG_PORT}`, `--user-data-dir=${join(outDir, 'chrome-profile')}`,
  `--window-size=${WIDTH},${HEIGHT}`, '--no-first-run', '--no-default-browser-check', '--mute-audio', '--autoplay-policy=no-user-gesture-required',
  '--disable-background-timer-throttling', '--disable-backgrounding-occluded-windows', '--disable-renderer-backgrounding',
  '--disable-frame-rate-limit', '--disable-gpu-vsync', '--use-angle=d3d11', '--enable-gpu', '--ignore-gpu-blocklist', 'about:blank'], { stdio: 'ignore' });

let ws, seq = 0;
const pending = new Map(), consoleLines = [];
async function connect() {
  for (let i = 0; i < 60; i++) {
    try {
      const tabs = await (await fetch(`http://127.0.0.1:${DEBUG_PORT}/json`)).json();
      const page = tabs.find(t => t.type === 'page');
      if (page) {
        ws = new WebSocket(page.webSocketDebuggerUrl);
        await new Promise((ok, fail) => { ws.onopen = ok; ws.onerror = fail; });
        ws.onmessage = e => {
          const m = JSON.parse(e.data);
          if (m.id && pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); return; }
          if (m.method === 'Runtime.exceptionThrown') consoleLines.push('EXC ' + (m.params.exceptionDetails.exception?.description ?? m.params.exceptionDetails.text));
          if (m.method === 'Runtime.consoleAPICalled') {
            const text = m.params.args.map(a => a.value ?? a.description ?? '').join(' ');
            if (m.params.type === 'error' || /exception|error|nullreference|missing/i.test(text)) consoleLines.push(m.params.type + ' ' + text.slice(0, 300));
          }
        };
        return;
      }
    } catch { /* not up yet */ }
    await sleep(500);
  }
  throw new Error('no Chrome');
}
const cdp = (method, params = {}) => new Promise(ok => { const id = ++seq; pending.set(id, ok); ws.send(JSON.stringify({ id, method, params })); });
const ev = async expr => {
  const r = await cdp('Runtime.evaluate', { expression: expr, awaitPromise: true, returnByValue: true });
  if (r.result?.exceptionDetails) throw new Error(r.result.exceptionDetails.exception?.description ?? 'eval failed');
  return r.result?.result?.value;
};

// Canvas is 16:9, centered in the area above the footer (template Kestrel9)
const canvasH = Math.min(HEIGHT - FOOTER, WIDTH * 9 / 16), canvasW = canvasH * 16 / 9;
const canvasX = (WIDTH - canvasW) / 2, canvasY = (HEIGHT - FOOTER - canvasH) / 2;
const at = (u, v) => ({ x: Math.round(canvasX + u * canvasW), y: Math.round(canvasY + v * canvasH) });
async function click(u, v) {
  const p = at(u, v);
  await cdp('Input.dispatchMouseEvent', { type: 'mouseMoved', ...p });
  await sleep(120);
  await cdp('Input.dispatchMouseEvent', { type: 'mousePressed', ...p, button: 'left', buttons: 1, clickCount: 1 });
  await sleep(60);
  await cdp('Input.dispatchMouseEvent', { type: 'mouseReleased', ...p, button: 'left', buttons: 0, clickCount: 1 });
  await sleep(250);
}
const KEYS = { a: ['KeyA', 65, 'a'], d: ['KeyD', 68, 'd'], p: ['KeyP', 80, 'p'], q: ['KeyQ', 81, 'q'], w: ['KeyW', 87, 'w'], s: ['KeyS', 83, 's'], space: ['Space', 32, ' '] };
async function hold(key, ms) {
  const [code, vk, text] = KEYS[key];
  await cdp('Input.dispatchKeyEvent', { type: 'keyDown', code, key: text, windowsVirtualKeyCode: vk, text });
  await sleep(ms);
  await cdp('Input.dispatchKeyEvent', { type: 'keyUp', code, key: text, windowsVirtualKeyCode: vk });
  await sleep(200);
}
const send = (obj, method) => ev(`k9.SendMessage(${JSON.stringify(obj)}, ${JSON.stringify(method)})`);
async function shot(name) {
  const r = await cdp('Page.captureScreenshot', { format: 'png' });
  if (r.result?.data) writeFileSync(join(outDir, name + '.png'), Buffer.from(r.result.data, 'base64'));
}

const result = { build: buildDir, scenario, cpu: process.env.K9_CPU ?? 1, noTraffic: !!process.env.K9_NOTRAFFIC, when: new Date().toISOString(), phases: {}, memory: {} };
async function measure(name, seconds) {
  await ev('k9start()');
  await sleep(seconds * 1000);
  result.phases[name] = await ev('k9stop()');
  result.memory[name] = await ev('k9mem()');
  await shot(name);
  console.log(name, JSON.stringify({ ...result.phases[name], spikes: undefined }), JSON.stringify(result.memory[name]));
}

const START = [0.206, 0.446], MENU_IN_PAUSE = [0.5, 0.684];
async function intoShift(firstRun) {
  await click(...START);
  await sleep(firstRun ? 9000 : 7000);
  await hold('space', 150); // closes the morning letter
  await sleep(2500);
}

try {
  await connect();
  await cdp('Page.enable'); await cdp('Runtime.enable');
  if (process.env.K9_CPU) await cdp('Emulation.setCPUThrottlingRate', { rate: Number(process.env.K9_CPU) });
  await cdp('Emulation.setDeviceMetricsOverride', { width: WIDTH, height: HEIGHT, deviceScaleFactor: 1, mobile: false });
  await cdp('Page.navigate', { url: `http://127.0.0.1:${HTTP_PORT}/` });
  for (let i = 0; i < 240 && !(await ev('!!window.k9').catch(() => false)); i++) await sleep(500);
  await sleep(6000); // boot splash into the menu
  result.memory.loaded = await ev('k9mem()');
  result.renderer = await ev(`(function(){var g=document.createElement('canvas').getContext('webgl2');var e=g&&g.getExtension('WEBGL_debug_renderer_info');return e?g.getParameter(e.UNMASKED_RENDERER_WEBGL):'?'})()`);
  await ev('k9start()'); await sleep(200); await ev('k9stop()');
  console.log('renderer', result.renderer, JSON.stringify(result.memory.loaded));

  const long = scenario === 'full' ? 1 : 0.5;
  await measure('menu', 30 * long);
  await intoShift(true);
  if (process.env.K9_NOTRAFFIC) await send('PF_Traffic', 'OnDisable');
  await measure('room', 15 * long);
  await send('PF_CounterStation', 'Sit');
  await sleep(2000);
  await measure('desk', 45 * long); // ships pass the windows behind the lock; travelers talk
  await hold('a', 2400);
  await measure('board', 10 * long);
  await hold('d', 1300);
  await send('PF_CounterStation', 'Leave');
  await sleep(1500);
  await measure('standing', 15 * long);

  if (scenario === 'full') {
    // Days and scene changes: does memory keep growing?
    for (let day = 2; day <= 3; day++) {
      await send('PF_DayFlow', 'DevEndDay');
      await sleep(6000);
      result.memory['day' + day] = await ev('k9mem()');
      await hold('space', 150);
      await sleep(1500);
    }
    for (let cycle = 1; cycle <= 3; cycle++) {
      await hold('p', 150);
      await sleep(400);
      await click(...MENU_IN_PAUSE);
      await sleep(7000);
      result.memory['menu' + cycle] = await ev('k9mem()');
      await intoShift(false);
      result.memory['main' + cycle] = await ev('k9mem()');
    }
    await shot('after_cycles');
  }
} catch (e) {
  result.error = String(e);
  console.error(e);
} finally {
  result.console = [...new Set(consoleLines)].slice(0, 40);
  writeFileSync(join(outDir, 'result.json'), JSON.stringify(result, null, 1));
  console.log('memory', JSON.stringify(result.memory));
  console.log('console', JSON.stringify(result.console).slice(0, 1200));
  try { await cdp('Browser.close'); } catch { /* already gone */ }
  await sleep(800);
  chrome.kill();
  server.close();
  process.exit(result.error ? 1 : 0);
}
