"""Tests for the in-game bug report backend (P84). Run: python -m unittest discover -s deploy/reports"""
import json
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


class Clock:
    def __init__(self):
        self.now = 1000.0

    def __call__(self):
        return self.now


class ReportServerTests(unittest.TestCase):
    def setUp(self):
        self.github = FakeGitHub()
        self.clock = Clock()
        self.config = {"token": "secret", "repo": "owner/repo", "api": self.github.url}
        self.httpd = server.make_server(0, self.config, server.Limiter(clock=self.clock))
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

    def test_failed_issue_does_not_use_up_the_limit(self):
        self.github.status = 500
        for _ in range(6):
            self.assertEqual(502, self.post(self.report())[0])
        self.github.status = 201
        self.assertEqual(201, self.post(self.report())[0])

    def test_ipv6_counts_per_64_network(self):
        for i in range(5):
            self.assertEqual(201, self.post(self.report(), ip=f"2001:db8:1:2::{i + 1}")[0])
        self.assertEqual(429, self.post(self.report(), ip="2001:db8:1:2:ffff::9")[0])
        self.assertEqual(201, self.post(self.report(), ip="2001:db8:1:3::1")[0])

    def test_origins(self):
        self.assertEqual(201, self.post(self.report(), origin="https://html-classic.itch.zone")[0])
        self.assertEqual(201, self.post(self.report(), origin=None)[0], "the editor sends no Origin")
        self.assertEqual(403, self.post(self.report(), origin="https://evil.example")[0])

    def test_rate_limit_per_client_and_window(self):
        for _ in range(5):
            self.assertEqual(201, self.post(self.report())[0])
        self.assertEqual(429, self.post(self.report())[0])
        self.assertEqual(201, self.post(self.report(), ip="198.51.100.1")[0], "another client is not blocked")
        self.clock.now += 601
        self.assertEqual(201, self.post(self.report())[0], "the window moves on")

    def test_overall_limit_without_client_ip(self):
        limiter = server.Limiter(per_ip=(5, 600), overall=(3, 3600), clock=self.clock)
        for _ in range(3):
            self.assertTrue(limiter.allow(""), "no IP: only the overall limit counts")
            limiter.record("")
        self.assertFalse(limiter.allow("192.0.2.1"))

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


if __name__ == "__main__":
    unittest.main()
