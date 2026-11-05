// Plays a local WebGL build step by step in its own headless Chrome with real mouse and key events (P57), for playthroughs from a script.
//   node tools/drive-webgl.mjs <build folder> <output folder>       starts the game and waits for steps
//   curl -s localhost:18096 -d '[["click",0.2,0.45],["wait",9000],["shot","start"]]'    runs steps, answers with new console lines
// Steps: click u v | down u v | move u v | up u v | key name | hold name ms | wait ms | shot name | send object method [argument] | eval js | quit
// u, v are 0..1 on the game canvas. Nothing here ships with the game: the hook is injected by this server only.
import { spawn } from 'node:child_process';
import { createServer } from 'node:http';
import { existsSync, mkdirSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import { extname, join, normalize } from 'node:path';

const [buildDir, outDir] = process.argv.slice(2);
if (!buildDir || !outDir) { console.error('usage: drive-webgl.mjs <build> <out>'); process.exit(2); }
mkdirSync(outDir, { recursive: true });

const CHROME = 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const WIDTH = 1600, HEIGHT = 900, FOOTER = 20, HTTP_PORT = 18097, DEBUG_PORT = 18098, CONTROL_PORT = 18096;
const sleep = ms => new Promise(r => setTimeout(r, ms));

const TYPES = { '.html': 'text/html', '.js': 'application/javascript', '.wasm': 'application/wasm', '.unityweb': 'application/octet-stream', '.png': 'image/png', '.css': 'text/css' };
createServer((req, res) => {
  const path = normalize(decodeURIComponent(req.url.split('?')[0])).replace(/^[\\/]+/, '') || 'index.html';
  const file = join(buildDir, path === '.' ? 'index.html' : path);
  if (!existsSync(file) || statSync(file).isDirectory()) { res.writeHead(404); res.end(); return; }
  res.writeHead(200, { 'Content-Type': TYPES[extname(file)] ?? 'application/octet-stream', 'Cache-Control': 'no-store' });
  if (extname(file) !== '.html') { res.end(readFileSync(file)); return; }
  res.end(readFileSync(file, 'utf8').replace('}).then(function () {', '}).then(function (i) { window.k9 = i;'));
}).listen(HTTP_PORT, '127.0.0.1');

const chrome = spawn(CHROME, ['--headless=new', `--remote-debugging-port=${DEBUG_PORT}`, `--user-data-dir=${join(outDir, 'chrome-profile')}`,
  `--window-size=${WIDTH},${HEIGHT}`, '--no-first-run', '--no-default-browser-check', '--mute-audio', '--autoplay-policy=no-user-gesture-required',
  '--disable-background-timer-throttling', '--disable-backgrounding-occluded-windows', '--disable-renderer-backgrounding',
  '--use-angle=d3d11', '--enable-gpu', '--ignore-gpu-blocklist', 'about:blank'], { stdio: 'ignore' });

let ws, seq = 0, lines = [];
const pending = new Map();
for (let i = 0; i < 60 && !ws; i++) {
  try {
    const page = (await (await fetch(`http://127.0.0.1:${DEBUG_PORT}/json`)).json()).find(t => t.type === 'page');
    if (page) {
      ws = new WebSocket(page.webSocketDebuggerUrl);
      await new Promise((ok, fail) => { ws.onopen = ok; ws.onerror = fail; });
    }
  } catch { ws = undefined; await sleep(500); }
}
ws.onmessage = e => {
  const m = JSON.parse(e.data);
  if (m.id && pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); return; }
  if (m.method === 'Runtime.exceptionThrown') lines.push('EXC ' + (m.params.exceptionDetails.exception?.description ?? m.params.exceptionDetails.text));
  if (m.method === 'Runtime.consoleAPICalled') lines.push(m.params.type + ' ' + m.params.args.map(a => a.value ?? a.description ?? '').join(' ').slice(0, 300));
};
const cdp = (method, params = {}) => new Promise(ok => { const id = ++seq; pending.set(id, ok); ws.send(JSON.stringify({ id, method, params })); });
const ev = async expr => (await cdp('Runtime.evaluate', { expression: expr, awaitPromise: true, returnByValue: true })).result?.result?.value;

const canvasH = Math.min(HEIGHT - FOOTER, WIDTH * 9 / 16), canvasW = canvasH * 16 / 9;
const canvasX = (WIDTH - canvasW) / 2, canvasY = (HEIGHT - FOOTER - canvasH) / 2;
const at = (u, v) => ({ x: Math.round(canvasX + u * canvasW), y: Math.round(canvasY + v * canvasH) });
const mouse = (type, u, v, extra = {}) => cdp('Input.dispatchMouseEvent', { type, ...at(u, v), ...extra });
const KEY = name => name.length === 1
  ? { code: /[a-z]/.test(name) ? 'Key' + name.toUpperCase() : 'Digit' + name, key: name, windowsVirtualKeyCode: name.toUpperCase().charCodeAt(0), text: name }
  : { space: { code: 'Space', key: ' ', windowsVirtualKeyCode: 32, text: ' ' }, escape: { code: 'Escape', key: 'Escape', windowsVirtualKeyCode: 27 }, tab: { code: 'Tab', key: 'Tab', windowsVirtualKeyCode: 9 } }[name];

async function step([kind, a, b, c]) {
  switch (kind) {
    case 'click':
      await mouse('mouseMoved', a, b); await sleep(120);
      await mouse('mousePressed', a, b, { button: 'left', buttons: 1, clickCount: 1 }); await sleep(60);
      await mouse('mouseReleased', a, b, { button: 'left', buttons: 0, clickCount: 1 }); await sleep(250);
      return;
    case 'move': await mouse('mouseMoved', a, b, { buttons: c ? 1 : 0 }); await sleep(60); return;
    case 'down': await mouse('mousePressed', a, b, { button: 'left', buttons: 1, clickCount: 1 }); await sleep(60); return;
    case 'up': await mouse('mouseReleased', a, b, { button: 'left', buttons: 0, clickCount: 1 }); await sleep(120); return;
    case 'hold':
      await cdp('Input.dispatchKeyEvent', { type: 'keyDown', ...KEY(a) }); await sleep(b);
      await cdp('Input.dispatchKeyEvent', { type: 'keyUp', ...KEY(a), text: undefined }); await sleep(150);
      return;
    case 'key': return step(['hold', a, 120]);
    case 'wait': await sleep(a); return;
    case 'shot': {
      const r = await cdp('Page.captureScreenshot', { format: 'png' });
      writeFileSync(join(outDir, a + '.png'), Buffer.from(r.result.data, 'base64'));
      return;
    }
    case 'send': await ev(`k9.SendMessage(${JSON.stringify(a)}, ${JSON.stringify(b)}${c === undefined ? '' : ', ' + JSON.stringify(c)})`); return;
    case 'eval': lines.push('eval ' + JSON.stringify(await ev(a))); return;
    case 'quit': await cdp('Browser.close').catch(() => {}); await sleep(500); chrome.kill(); process.exit(0);
  }
}

await cdp('Page.enable'); await cdp('Runtime.enable');
await cdp('Emulation.setDeviceMetricsOverride', { width: WIDTH, height: HEIGHT, deviceScaleFactor: 1, mobile: false });
await cdp('Page.navigate', { url: `http://127.0.0.1:${HTTP_PORT}/` });

createServer((req, res) => {
  let body = '';
  req.on('data', d => { body += d; });
  req.on('end', async () => {
    try { for (const s of JSON.parse(body || '[]')) await step(s); } catch (e) { lines.push('DRIVER ' + e); }
    res.end(lines.join('\n') + '\n');
    lines = [];
  });
}).listen(CONTROL_PORT, '127.0.0.1');
console.log('ready');
