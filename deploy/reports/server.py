#!/usr/bin/env python3
"""In-game bug reports for Kestrel-9 (P84, docs/DEPLOY.md).

POST /api/report with JSON {text, version, day, phase, scene, browser} creates a GitHub issue with the labels "bug" and
"player-report" through a bot token, so testers need no GitHub account. Standard library only.

P129: POST /api/metrics with JSON {session, version, events: [...]} appends the game's anonymous play events to one JSONL file
per day in METRICS_DIR (tools/metrics_report.py reads them). Files older than METRICS_DAYS are deleted.

Privacy: the client IP is not used or kept. Reports are not written to disk; metrics carry only what the game sends (a random
session id that lives in the game's memory, no IP) plus the server's time. The log lines carry no IP and no report text.

Environment: GITHUB_TOKEN (fine-grained, Issues read/write), GITHUB_REPO, GITHUB_API (tests), PORT, METRICS_DIR, METRICS_DAYS.
"""
import datetime
import json
import math
import os
import re
import sys
import threading
import urllib.error
import urllib.request
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

MAX_BODY = 8 * 1024
TEXT_MIN, TEXT_MAX = 3, 1000
FIELD_MAX = 200
LABELS = ["bug", "player-report"]
MAX_PARALLEL = 8        # GitHub calls at the same time; more → 503 (keeps threads well below the container's pids_limit)
# Browsers may only post from the live site and the itch.io build; the game in the editor sends no Origin at all.
ALLOWED_ORIGINS = ("https://kestrel9.maxlworks.com",)
ALLOWED_ORIGIN_SUFFIXES = (".itch.zone", ".itch.io")

# P129 play metrics
METRICS_MAX_BODY = 64 * 1024
METRICS_MAX_EVENTS = 100
METRICS_MAX_FIELDS = 24
METRICS_TEXT_MAX = 500
METRICS_LIST_MAX = 12
METRICS_FILE_MAX = 200 * 1024 * 1024  # per day; beyond it events are dropped (a full disk would stop the whole server)
METRICS_EVENTS = {"visit", "session", "day_start", "traveler", "shift_end", "day_end", "run_end", "beat", "error"}
SERVER_FIELDS = ("e", "ts", "s", "v")  # set by the server (MetricsStore.write); an event can't bring its own
KEY = re.compile(r"^[a-z][a-z0-9_]{0,23}$")
SESSION = re.compile(r"^[0-9a-f]{8,32}$")


def origin_allowed(origin):
    if not origin:
        return True
    if origin in ALLOWED_ORIGINS:
        return True
    return origin.startswith("https://") and origin.endswith(ALLOWED_ORIGIN_SUFFIXES)


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


def metric_value(value):
    """A JSON value the metrics keep: bool, finite number, short text or a short list of numbers; anything else → None."""
    if isinstance(value, bool):
        return value
    if isinstance(value, (int, float)):
        return value if math.isfinite(value) else None
    if isinstance(value, str):
        return one_line(value, METRICS_TEXT_MAX)
    if isinstance(value, list) and len(value) <= METRICS_LIST_MAX:
        if all(isinstance(v, (int, float)) and not isinstance(v, bool) and math.isfinite(v) for v in value):
            return value
    return None


def parse_metrics(raw):
    """Returns (session, version, [event, ...], None) or (None, None, None, error). Unknown events and odd fields are dropped."""
    try:
        data = json.loads(raw.decode("utf-8"))
    except (UnicodeDecodeError, json.JSONDecodeError):
        return None, None, None, "not JSON"
    if not isinstance(data, dict):
        return None, None, None, "not an object"
    session = str(data.get("session") or "")
    if not SESSION.match(session):
        return None, None, None, "bad session"
    events = data.get("events")
    if not isinstance(events, list) or not events:
        return None, None, None, "no events"
    if len(events) > METRICS_MAX_EVENTS:
        return None, None, None, "too many events"
    kept = []
    for event in events:
        # "e" is checked as text first: a list or object there is unhashable and would end the request with a TypeError
        if not isinstance(event, dict) or not isinstance(event.get("e"), str) or event["e"] not in METRICS_EVENTS:
            continue
        clean_event = {"e": event["e"]}
        for key, value in list(event.items())[:METRICS_MAX_FIELDS]:
            if key in SERVER_FIELDS or not isinstance(key, str) or not KEY.match(key):
                continue
            value = metric_value(value)
            if value is not None:
                clean_event[key] = value
        kept.append(clean_event)
    return session, one_line(data.get("version"), FIELD_MAX) or "unknown", kept, None


class MetricsStore:
    """Appends events to <dir>/metrics-YYYY-MM-DD.jsonl (UTC) and deletes days older than `days` once per day."""

    def __init__(self, directory, days=90, now=None):
        self.directory, self.days = directory, days
        self.now = now or (lambda: datetime.datetime.now(datetime.timezone.utc))
        self.lock = threading.Lock()
        self.pruned = None

    def path(self, day):
        return os.path.join(self.directory, f"metrics-{day.isoformat()}.jsonl")

    def write(self, session, version, events):
        now = self.now()
        stamp = now.strftime("%Y-%m-%dT%H:%M:%SZ")
        # Encoded once: the limit counts bytes on disk, and non-ASCII text (ensure_ascii=False) takes more bytes than characters.
        data = "".join(json.dumps({"ts": stamp, "s": session, "v": version, **event}, ensure_ascii=False) + "\n"
                       for event in events).encode("utf-8")
        with self.lock:
            self.prune(now.date())
            path = self.path(now.date())
            try:
                size = os.path.getsize(path)
            except OSError:
                size = 0
            if size + len(data) > METRICS_FILE_MAX:
                return False
            with open(path, "ab") as f:
                f.write(data)
        return True

    def prune(self, today):
        if self.pruned == today:
            return
        self.pruned = today
        oldest = today - datetime.timedelta(days=self.days)
        for name in os.listdir(self.directory):
            match = re.match(r"^metrics-(\d{4}-\d{2}-\d{2})\.jsonl$", name)
            if match and datetime.date.fromisoformat(match.group(1)) < oldest:
                os.remove(os.path.join(self.directory, name))


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


def make_handler(config):
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

        def _send(self, status, payload, quiet=False):
            data = json.dumps(payload).encode("utf-8")
            self.send_response(status)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(data)))
            self._cors()
            self.send_header("Cache-Control", "no-store")
            self.end_headers()
            self.wfile.write(data)
            if quiet:  # metrics come every 30 s per player; only their failures are worth a log line
                return
            print(f"{'metrics' if self.path == '/api/metrics' else 'report'} {status} {payload.get('issue', payload.get('error', ''))}", flush=True)

        def do_OPTIONS(self):
            self.send_response(204)
            self._cors()
            self.send_header("Access-Control-Allow-Methods", "POST, OPTIONS")
            self.send_header("Access-Control-Allow-Headers", "Content-Type")
            self.send_header("Access-Control-Max-Age", "86400")
            self.end_headers()

        def do_GET(self):
            if self.path == "/api/health":
                self._send(200, {"ok": True, "configured": bool(config["token"]), "metrics": config.get("metrics") is not None})
            else:
                self._send(404, {"ok": False, "error": "not found"})

        def _body(self, limit):
            """The request body, or None after an error answer."""
            if not origin_allowed(self.headers.get("Origin") or ""):
                self._send(403, {"ok": False, "error": "origin"})
                return None
            try:
                length = int(self.headers.get("Content-Length") or 0)
            except ValueError:
                length = -1
            if length <= 0 or length > limit:
                self._send(413 if length > limit else 400, {"ok": False, "error": "bad size"})
                return None
            return self.rfile.read(length)

        def do_POST(self):
            if self.path == "/api/metrics":
                return self._metrics()
            if self.path != "/api/report":
                return self._send(404, {"ok": False, "error": "not found"})
            raw = self._body(MAX_BODY)
            if raw is None:
                return
            report, error = parse_report(raw)
            if error:
                return self._send(400, {"ok": False, "error": error})
            if not config["token"]:
                return self._send(503, {"ok": False, "error": "not configured"})
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
            self._send(201, {"ok": True, "issue": number})

        def _metrics(self):
            store = config.get("metrics")
            if store is None:  # the game stops sending for this session on 404
                return self._send(404, {"ok": False, "error": "metrics off"})
            raw = self._body(METRICS_MAX_BODY)
            if raw is None:
                return
            session, version, events, error = parse_metrics(raw)
            if error:
                return self._send(400, {"ok": False, "error": error})
            try:
                stored = store.write(session, version, events) if events else True
            except OSError:
                return self._send(503, {"ok": False, "error": "storage"})
            if not stored:
                return self._send(503, {"ok": False, "error": "full"})
            self._send(200, {"ok": True, "stored": len(events)}, quiet=True)

    return Handler


def metrics_store():
    """The store in METRICS_DIR, or None (metrics off) when the folder is missing or not writable."""
    directory = os.environ.get("METRICS_DIR", "/data").strip()
    if not directory or not os.path.isdir(directory) or not os.access(directory, os.W_OK):
        return None
    return MetricsStore(directory, int(os.environ.get("METRICS_DAYS", "90")))


def make_server(port=8000, config=None):
    config = config or {
        "token": os.environ.get("GITHUB_TOKEN", "").strip(),
        "repo": os.environ.get("GITHUB_REPO", "Juice-de-Orange/game-off-2026").strip(),
        "api": os.environ.get("GITHUB_API", "https://api.github.com").rstrip("/"),
        "metrics": metrics_store(),
    }
    return ThreadingHTTPServer(("0.0.0.0", port), make_handler(config))


if __name__ == "__main__":
    server = make_server(int(os.environ.get("PORT", "8000")))
    metrics = "on" if metrics_store() is not None else "OFF (no writable METRICS_DIR)"
    print(f"kestrel9-reports on :{server.server_address[1]}, token {'set' if os.environ.get('GITHUB_TOKEN') else 'MISSING'}, metrics {metrics}", flush=True)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        sys.exit(0)
