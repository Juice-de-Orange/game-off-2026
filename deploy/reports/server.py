#!/usr/bin/env python3
"""In-game bug reports for Kestrel-9 (P84, docs/DEPLOY.md).

POST /api/report with JSON {text, version, day, phase, scene, browser} creates a GitHub issue with the labels "bug" and
"player-report" through a bot token, so testers need no GitHub account. Standard library only.

Privacy: the client IP (X-Real-IP, set by the game's nginx) is only used for the rate limit and kept in memory for at most
ten minutes. Nothing is written to disk and the log lines carry no IP and no report text.

Environment: GITHUB_TOKEN (fine-grained, Issues read/write), GITHUB_REPO, GITHUB_API (tests), PORT.
"""
import ipaddress
import json
import os
import sys
import threading
import time
import urllib.error
import urllib.request
from collections import deque
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

MAX_BODY = 8 * 1024
TEXT_MIN, TEXT_MAX = 3, 1000
FIELD_MAX = 200
PER_IP = (5, 600)       # reports per client per 10 minutes
OVERALL = (60, 3600)    # reports in total per hour
LABELS = ["bug", "player-report"]
MAX_PARALLEL = 8        # GitHub calls at the same time; more → 503 (keeps threads well below the container's pids_limit)
# Browsers may only post from the live site and the itch.io build; the game in the editor sends no Origin at all.
ALLOWED_ORIGINS = ("https://kestrel9.maxlworks.com",)
ALLOWED_ORIGIN_SUFFIXES = (".itch.zone", ".itch.io")


def origin_allowed(origin):
    if not origin:
        return True
    if origin in ALLOWED_ORIGINS:
        return True
    return origin.startswith("https://") and origin.endswith(ALLOWED_ORIGIN_SUFFIXES)


def client_key(ip):
    """IPv6 clients usually own a whole /64, so they count as one; IPv4 as the address. Empty for unknown."""
    try:
        address = ipaddress.ip_address(ip)
    except ValueError:
        return ""
    if address.version == 4:
        return str(address)
    if address.ipv4_mapped:
        return str(address.ipv4_mapped)
    return str(ipaddress.ip_network(f"{address}/64", strict=False))


class Limiter:
    """Sliding windows: per client and in total. Clients without an IP only count toward the total."""

    def __init__(self, per_ip=PER_IP, overall=OVERALL, clock=time.monotonic):
        self._per_ip, self._overall, self._clock = per_ip, overall, clock
        self._lock = threading.Lock()
        self._ips = {}
        self._all = deque()

    def allow(self, ip):
        """Whether one more report fits; doesn't count it (record() does, after it went through)."""
        now = self._clock()
        with self._lock:
            self._prune(now)
            if len(self._all) >= self._overall[0]:
                return False
            hits = self._ips.get(ip) if ip else None
            return hits is None or len(hits) < self._per_ip[0]

    def record(self, ip):
        now = self._clock()
        with self._lock:
            self._all.append(now)
            if ip:
                self._ips.setdefault(ip, deque()).append(now)

    def _prune(self, now):
        while self._all and now - self._all[0] > self._overall[1]:
            self._all.popleft()
        for ip in list(self._ips):
            hits = self._ips[ip]
            while hits and now - hits[0] > self._per_ip[1]:
                hits.popleft()
            if not hits:
                del self._ips[ip]


def one_line(value, limit):
    return " ".join(str(value if value is not None else "").split())[:limit]


def defuse(text):
    """No @mentions: a zero-width space after @ keeps GitHub from notifying anyone."""
    return text.replace("@", "@\u200b")


def clean(value, limit=FIELD_MAX):
    return defuse(one_line(value, limit))


def parse_report(raw):
    """Returns (report, None) or (None, error message)."""
    try:
        data = json.loads(raw.decode("utf-8"))
    except (UnicodeDecodeError, json.JSONDecodeError):
        return None, "not JSON"
    if not isinstance(data, dict):
        return None, "not an object"
    text = str(data.get("text") or "").strip()
    if len(text) < TEXT_MIN:
        return None, "text too short"
    if len(text) > TEXT_MAX:
        return None, "text too long"
    try:
        day = max(0, min(999, int(data.get("day") or 0)))
    except (TypeError, ValueError, OverflowError):  # OverflowError: JSON Infinity / 1e400
        day = 0
    return {
        "text": defuse(text),
        "version": clean(data.get("version")) or "unknown",
        "day": day,
        "phase": clean(data.get("phase")) or "-",
        "scene": clean(data.get("scene")) or "-",
        "browser": clean(data.get("browser")) or "-",
    }, None


def issue_for(report, repo):
    """Title and body of the GitHub issue."""
    where = f"Day {report['day']}, {report['phase']}" if report["day"] > 0 else report["scene"]
    first_line = one_line(report["text"].splitlines()[0], 60)  # the text is defused already
    version = report["version"]
    if "+" in version and not version.endswith("+"):
        version += f" (https://github.com/{repo}/commit/{version.split('+', 1)[1]})"
    body = (
        f"{report['text']}\n\n---\n"
        "_Sent from the in-game bug report (anonymous)._\n"
        f"- Version: {version}\n"
        f"- Day: {report['day'] if report['day'] > 0 else '-'} · Phase: {report['phase'] if report['day'] > 0 else '-'}\n"
        f"- Scene: {report['scene']}\n"
        f"- Browser: {report['browser']}\n"
    )
    return f"[Player] {where}: {first_line}", body


def create_issue(api, repo, token, title, body):
    request = urllib.request.Request(
        f"{api}/repos/{repo}/issues",
        data=json.dumps({"title": title, "body": body, "labels": LABELS}).encode("utf-8"),
        method="POST",
        headers={
            "Authorization": f"Bearer {token}",
            "Accept": "application/vnd.github+json",
            "X-GitHub-Api-Version": "2022-11-28",
            "User-Agent": "kestrel9-reports",
            "Content-Type": "application/json",
        },
    )
    with urllib.request.urlopen(request, timeout=15) as response:
        return json.load(response)["number"]


def make_handler(config, limiter):
    slots = threading.BoundedSemaphore(MAX_PARALLEL)

    class Handler(BaseHTTPRequestHandler):
        server_version = "kestrel9-reports"
        sys_version = ""
        timeout = 20  # a client that stalls mid-request doesn't hold its thread forever

        def log_message(self, fmt, *args):  # the default log line starts with the client IP
            pass

        def _cors(self):
            origin = self.headers.get("Origin") or ""
            if origin and origin_allowed(origin):
                self.send_header("Access-Control-Allow-Origin", origin)
            self.send_header("Vary", "Origin")

        def _send(self, status, payload):
            data = json.dumps(payload).encode("utf-8")
            self.send_response(status)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(data)))
            self._cors()
            self.send_header("Cache-Control", "no-store")
            self.end_headers()
            self.wfile.write(data)
            print(f"report {status} {payload.get('issue', payload.get('error', ''))}", flush=True)

        def do_OPTIONS(self):
            self.send_response(204)
            self._cors()
            self.send_header("Access-Control-Allow-Methods", "POST, OPTIONS")
            self.send_header("Access-Control-Allow-Headers", "Content-Type")
            self.send_header("Access-Control-Max-Age", "86400")
            self.end_headers()

        def do_GET(self):
            if self.path == "/api/health":
                self._send(200, {"ok": True, "configured": bool(config["token"])})
            else:
                self._send(404, {"ok": False, "error": "not found"})

        def do_POST(self):
            if self.path != "/api/report":
                return self._send(404, {"ok": False, "error": "not found"})
            if not origin_allowed(self.headers.get("Origin") or ""):
                return self._send(403, {"ok": False, "error": "origin"})
            try:
                length = int(self.headers.get("Content-Length") or 0)
            except ValueError:
                length = -1
            if length <= 0 or length > MAX_BODY:
                return self._send(413 if length > MAX_BODY else 400, {"ok": False, "error": "bad size"})
            report, error = parse_report(self.rfile.read(length))
            if error:
                return self._send(400, {"ok": False, "error": error})
            if not config["token"]:
                return self._send(503, {"ok": False, "error": "not configured"})
            ip = client_key((self.headers.get("X-Real-IP") or "").split(",")[0].strip())
            if not limiter.allow(ip):
                return self._send(429, {"ok": False, "error": "too many reports"})
            if not slots.acquire(blocking=False):
                return self._send(503, {"ok": False, "error": "busy"})
            try:
                title, body = issue_for(report, config["repo"])
                number = create_issue(config["api"], config["repo"], config["token"], title, body)
            except urllib.error.HTTPError as e:
                return self._send(502, {"ok": False, "error": f"github {e.code}"})
            except (urllib.error.URLError, OSError, KeyError, ValueError):
                return self._send(502, {"ok": False, "error": "github unreachable"})
            finally:
                slots.release()
            limiter.record(ip)  # only reports that became issues count
            self._send(201, {"ok": True, "issue": number})

    return Handler


def make_server(port=8000, config=None, limiter=None):
    config = config or {
        "token": os.environ.get("GITHUB_TOKEN", "").strip(),
        "repo": os.environ.get("GITHUB_REPO", "Juice-de-Orange/game-off-2026").strip(),
        "api": os.environ.get("GITHUB_API", "https://api.github.com").rstrip("/"),
    }
    return ThreadingHTTPServer(("0.0.0.0", port), make_handler(config, limiter or Limiter()))


if __name__ == "__main__":
    server = make_server(int(os.environ.get("PORT", "8000")))
    print(f"kestrel9-reports on :{server.server_address[1]}, token {'set' if os.environ.get('GITHUB_TOKEN') else 'MISSING'}", flush=True)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        sys.exit(0)
