"""Tests for the in-game bug report backend (P84). Run: python -m unittest discover -s deploy/reports"""
import json
import os
import threading
import unittest
import urllib.error
import urllib.request
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

import server


class FakeGitHub:
    """Records issues and answers like the GitHub API; status can be set to simulate failures."""

    def __init__(self):
        self.issues, self.status, self.auth = [], 201, []
        fake = self

        class Handler(BaseHTTPRequestHandler):
            def log_message(self, *args):
                pass

            def do_POST(self):
                body = json.loads(self.rfile.read(int(self.headers["Content-Length"])))
                fake.auth.append(self.headers.get("Authorization"))
                fake.issues.append((self.path, body))
                data = json.dumps({"number": 40 + len(fake.issues)}).encode()
                self.send_response(fake.status)
                self.send_header("Content-Length", str(len(data)))
                self.end_headers()
                self.wfile.write(data)

        self.httpd = ThreadingHTTPServer(("127.0.0.1", 0), Handler)
        threading.Thread(target=self.httpd.serve_forever, daemon=True).start()
        self.url = f"http://127.0.0.1:{self.httpd.server_address[1]}"

    def close(self):
        self.httpd.shutdown()
        self.httpd.server_close()


class ReportServerTests(unittest.TestCase):
    def setUp(self):
        self.github = FakeGitHub()
        self.config = {"token": "secret", "repo": "owner/repo", "api": self.github.url}
        self.httpd = server.make_server(0, self.config)
        self.httpd.server_address = ("127.0.0.1", self.httpd.server_address[1])
        threading.Thread(target=self.httpd.serve_forever, daemon=True).start()
        self.url = f"http://127.0.0.1:{self.httpd.server_address[1]}"

    def tearDown(self):
        self.httpd.shutdown()
        self.httpd.server_close()
        self.github.close()

    def post(self, payload, ip="203.0.113.7", raw=None, origin="https://kestrel9.maxlworks.com"):
        data = raw if raw is not None else json.dumps(payload).encode()
        headers = {"Content-Type": "application/json", "X-Real-IP": ip}
        if origin:
            headers["Origin"] = origin
        request = urllib.request.Request(self.url + "/api/report", data=data, method="POST", headers=headers)
        try:
            with urllib.request.urlopen(request, timeout=5) as response:
                return response.status, json.load(response), dict(response.headers)
        except urllib.error.HTTPError as e:
            return e.code, json.loads(e.read() or b"{}"), dict(e.headers)

    def report(self, **extra):
        payload = {"text": "The door in the hall doesn't open.", "version": "0.1.0+abc1234", "day": 2,
                   "phase": "Shift", "scene": "Main", "browser": "Firefox"}
        payload.update(extra)
        return payload

    def test_creates_issue_with_labels_and_info(self):
        status, body, headers = self.post(self.report())
        self.assertEqual(201, status)
        self.assertEqual({"ok": True, "issue": 41}, body)
        self.assertEqual("https://kestrel9.maxlworks.com", headers["Access-Control-Allow-Origin"])
        path, issue = self.github.issues[0]
        self.assertEqual("/repos/owner/repo/issues", path)
        self.assertEqual(["bug", "player-report"], issue["labels"])
        self.assertEqual("[Player] Day 2, Shift: The door in the hall doesn't open.", issue["title"])
        self.assertIn("Version: 0.1.0+abc1234 (https://github.com/owner/repo/commit/abc1234)", issue["body"])
        self.assertIn("Day: 2 · Phase: Shift", issue["body"])
        self.assertIn("Browser: Firefox", issue["body"])
        self.assertEqual("Bearer secret", self.github.auth[0])

    def test_rejects_bad_input(self):
        self.assertEqual(400, self.post(self.report(text="  "))[0])
        self.assertEqual(400, self.post(self.report(text="x" * 1001))[0])
        self.assertEqual(400, self.post(None, raw=b"not json")[0])
        self.assertEqual(400, self.post(None, raw=b"[1,2]")[0])
        self.assertEqual(413, self.post(None, raw=b"{" + b" " * 9000 + b"}")[0])
        self.assertEqual([], self.github.issues)

    def test_mentions_are_defused_and_fields_shortened(self):
        self.post(self.report(text="ping @Juice-de-Orange please", browser="B" * 500, day="nonsense"))
        _, issue = self.github.issues[0]
        self.assertNotIn("@Juice", issue["body"])
        self.assertIn("@\u200bJuice", issue["body"])
        self.assertIn("B" * 200 + "\n", issue["body"])
        self.assertNotIn("B" * 201, issue["body"])
        self.assertTrue(issue["title"].startswith("[Player] Main: "), "day 0 names the scene")
        self.assertEqual(1, issue["title"].count("\u200b"), "@ is defused exactly once")

    def test_infinite_day_is_clamped_not_a_crash(self):
        status, _, _ = self.post(None, raw=b'{"text": "abc def", "day": 1e400}')
        self.assertEqual(201, status)
        self.assertIn("Day: -", self.github.issues[0][1]["body"])

    def test_no_limit_on_reports(self):
        for _ in range(70):
            self.assertEqual(201, self.post(self.report())[0])
        self.assertEqual(70, len(self.github.issues))

    def test_origins(self):
        self.assertEqual(201, self.post(self.report(), origin="https://html-classic.itch.zone")[0])
        self.assertEqual(201, self.post(self.report(), origin=None)[0], "the editor sends no Origin")
        self.assertEqual(403, self.post(self.report(), origin="https://evil.example")[0])

    def test_github_failure_is_502(self):
        self.github.status = 401
        status, body, _ = self.post(self.report())
        self.assertEqual(502, status)
        self.assertEqual("github 401", body["error"])

    def test_without_token_503(self):
        self.config["token"] = ""
        self.assertEqual(503, self.post(self.report())[0])

    def test_preflight_allows_the_itch_build(self):
        request = urllib.request.Request(self.url + "/api/report", method="OPTIONS",
                                         headers={"Origin": "https://html-classic.itch.zone"})
        with urllib.request.urlopen(request, timeout=5) as response:
            self.assertEqual(204, response.status)
            self.assertEqual("https://html-classic.itch.zone", response.headers["Access-Control-Allow-Origin"])
            self.assertIn("POST", response.headers["Access-Control-Allow-Methods"])


class MetricsTests(unittest.TestCase):
    """P129: play metrics are stored as JSONL per day, cleaned, and old days are deleted."""

    def setUp(self):
        import datetime
        import tempfile
        self.datetime = datetime
        self.dir = tempfile.TemporaryDirectory()
        self.now = datetime.datetime(2026, 11, 20, 18, 30, 5, tzinfo=datetime.timezone.utc)
        self.store = server.MetricsStore(self.dir.name, days=90, now=lambda: self.now)
        config = {"token": "", "repo": "owner/repo", "api": "http://127.0.0.1:9", "metrics": self.store}
        self.httpd = server.make_server(0, config)
        self.httpd.server_address = ("127.0.0.1", self.httpd.server_address[1])
        threading.Thread(target=self.httpd.serve_forever, daemon=True).start()
        self.url = f"http://127.0.0.1:{self.httpd.server_address[1]}"

    def tearDown(self):
        self.httpd.shutdown()
        self.httpd.server_close()
        self.dir.cleanup()

    def post(self, payload=None, raw=None, origin="https://kestrel9.maxlworks.com", url=None):
        data = raw if raw is not None else json.dumps(payload).encode()
        headers = {"Content-Type": "application/json"}
        if origin:
            headers["Origin"] = origin
        request = urllib.request.Request((url or self.url) + "/api/metrics", data=data, method="POST", headers=headers)
        try:
            with urllib.request.urlopen(request, timeout=5) as response:
                return response.status, json.load(response)
        except urllib.error.HTTPError as e:
            return e.code, json.loads(e.read() or b"{}")

    def batch(self, *events, session="a1b2c3d4e5f6"):
        return {"session": session, "version": "0.1.0+abc1234", "events": list(events)}

    def lines(self, day="2026-11-20"):
        path = os.path.join(self.dir.name, f"metrics-{day}.jsonl")
        with open(path, encoding="utf-8") as f:
            return [json.loads(line) for line in f]

    def test_stores_events_with_session_version_and_time(self):
        status, body = self.post(self.batch(
            {"e": "day_start", "t": 12.5, "day": 1, "money": 40, "family": [0, 0, 1]},
            {"e": "traveler", "t": 30.25, "day": 1, "id": "T_Story_Shifter", "chosen": "Admit", "ok": False, "sec": 17.5}))
        self.assertEqual((200, {"ok": True, "stored": 2}), (status, body))
        first, second = self.lines()
        self.assertEqual({"ts": "2026-11-20T18:30:05Z", "s": "a1b2c3d4e5f6", "v": "0.1.0+abc1234", "e": "day_start", "t": 12.5,
                          "day": 1, "money": 40, "family": [0, 0, 1]}, first)
        self.assertEqual(False, second["ok"])
        self.assertEqual("T_Story_Shifter", second["id"])

    def test_drops_unknown_events_and_odd_fields(self):
        status, body = self.post(self.batch(
            {"e": "hack", "t": 1},
            {"e": "beat", "t": 2, "fps": 58.2, "Bad-Key": 1, "nested": {"a": 1}, "inf": 1e400, "msg": "x" * 900, "list": [1, "a"]}))
        self.assertEqual((200, {"ok": True, "stored": 1}), (status, body))
        (beat,) = self.lines()
        self.assertEqual(58.2, beat["fps"])
        for key in ("Bad-Key", "nested", "inf", "list"):
            self.assertNotIn(key, beat)
        self.assertEqual(500, len(beat["msg"]))

    def test_rejects_bad_batches(self):
        self.assertEqual(400, self.post(self.batch({"e": "beat"}, session="../../etc"))[0])
        self.assertEqual(400, self.post(self.batch())[0], "no events")
        self.assertEqual(400, self.post(self.batch(*[{"e": "beat"}] * 101))[0])
        self.assertEqual(400, self.post(raw=b"nope")[0])
        self.assertEqual(413, self.post(raw=b"{" + b" " * (65 * 1024) + b"}")[0])
        self.assertEqual(403, self.post(self.batch({"e": "beat"}), origin="https://evil.example")[0])
        self.assertEqual([], os.listdir(self.dir.name))

    def test_old_days_are_deleted(self):
        for day in ("2026-08-21", "2026-08-22"):
            open(os.path.join(self.dir.name, f"metrics-{day}.jsonl"), "w").close()
        self.post(self.batch({"e": "beat"}))
        self.assertEqual(["metrics-2026-08-22.jsonl", "metrics-2026-11-20.jsonl"], sorted(os.listdir(self.dir.name)),
                         "90 days back from 20.11. is 22.08.: kept, the day before goes")

    def test_full_day_refuses_instead_of_filling_the_disk(self):
        with open(os.path.join(self.dir.name, "metrics-2026-11-20.jsonl"), "w") as f:
            f.truncate(server.METRICS_FILE_MAX)
        self.assertEqual(503, self.post(self.batch({"e": "beat"}))[0])

    def test_unhashable_event_name_is_dropped_not_a_crash(self):
        status, body = self.post(self.batch({"e": ["beat"]}, {"e": {"x": 1}}, {"e": 5}, {"e": "beat", "fps": 60}))
        self.assertEqual((200, {"ok": True, "stored": 1}), (status, body))
        self.assertEqual([60], [line["fps"] for line in self.lines()])

    def test_server_fields_cannot_be_overridden(self):
        self.post(self.batch({"e": "beat", "ts": "1999-01-01T00:00:00Z", "s": "forged", "v": "x", "fps": 30}))
        (beat,) = self.lines()
        self.assertEqual(("2026-11-20T18:30:05Z", "a1b2c3d4e5f6", "0.1.0+abc1234"), (beat["ts"], beat["s"], beat["v"]))

    def test_day_limit_counts_utf8_bytes_not_characters(self):
        event = {"e": "error", "msg": "ä" * 400}  # 400 characters, 800 bytes in UTF-8
        line = json.dumps({"ts": "2026-11-20T18:30:05Z", "s": "a1b2c3d4e5f6", "v": "0.1.0+abc1234", **event}, ensure_ascii=False) + "\n"
        chars, size = len(line), len(line.encode("utf-8"))
        self.assertGreater(size, chars)
        path = os.path.join(self.dir.name, "metrics-2026-11-20.jsonl")
        with open(path, "wb") as f:
            f.truncate(server.METRICS_FILE_MAX - chars)  # fits by characters, not by bytes
        self.assertEqual(503, self.post(self.batch(event))[0])
        self.assertEqual(server.METRICS_FILE_MAX - chars, os.path.getsize(path))
        with open(path, "wb") as f:
            f.truncate(server.METRICS_FILE_MAX - size)  # fits exactly
        self.assertEqual(200, self.post(self.batch(event))[0])
        self.assertEqual(server.METRICS_FILE_MAX, os.path.getsize(path))

    def test_off_without_store_and_reports_unaffected(self):
        off = server.make_server(0, {"token": "", "repo": "owner/repo", "api": "http://127.0.0.1:9", "metrics": None})
        off.server_address = ("127.0.0.1", off.server_address[1])
        threading.Thread(target=off.serve_forever, daemon=True).start()
        try:
            status, _ = self.post(self.batch({"e": "beat"}), url=f"http://127.0.0.1:{off.server_address[1]}")
            self.assertEqual(404, status, "404 makes the game stop sending")
        finally:
            off.shutdown()
            off.server_close()


if __name__ == "__main__":
    unittest.main()
