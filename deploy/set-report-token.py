"""Stores the GitHub token of the bug report backend (P84) on Geoffrey and restarts it. Run it in your own terminal:

    python deploy/set-report-token.py

Token: github.com → Settings → Developer settings → Fine-grained tokens → Generate. Repository access: only
Juice-de-Orange/game-off-2026; Permissions → Repository → Issues: Read and write (nothing else). The token is read hidden,
sent over SSH to /opt/kestrel9/reports.env (mode 600) and never written to a local file or the shell history.
"""
import getpass
import os
import shutil
import subprocess
import sys

REPO = "Juice-de-Orange/game-off-2026"
REMOTE = ("umask 077 && cat > /opt/kestrel9/reports.env.new && mv /opt/kestrel9/reports.env.new /opt/kestrel9/reports.env"
          " && docker compose -f /opt/kestrel9/compose.yml up -d --no-build --pull never --force-recreate reports"
          " && sleep 2 && docker exec kestrel9-reports wget -qO- http://127.0.0.1:8000/api/health")


def ssh_binary():
    # The Windows OpenSSH client uses the ssh-agent service that holds the Geoffrey key; Git Bash's ssh does not.
    windows = r"C:\Windows\System32\OpenSSH\ssh.exe"
    return windows if os.path.exists(windows) else (shutil.which("ssh") or "ssh")


def main():
    token = getpass.getpass(f"GitHub token for {REPO} (Issues: read and write), hidden: ").strip()
    if not token.startswith("github_pat_") or any(c.isspace() for c in token):
        sys.exit("Please use a fine-grained token (github_pat_...) limited to Issues on this one repo. Nothing was changed.")
    env = f"GITHUB_TOKEN={token}\nGITHUB_REPO={REPO}\n"
    result = subprocess.run([ssh_binary(), "geoffrey", REMOTE], input=env.encode(), capture_output=True)
    out = (result.stdout + result.stderr).decode(errors="replace").replace(token, "***")
    print(out.strip())
    if result.returncode != 0:
        sys.exit(f"Failed (exit {result.returncode}).")
    print("Token stored. The backend reports \"configured\": true above if it took the token.")


if __name__ == "__main__":
    main()
