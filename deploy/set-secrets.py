"""Sets the GitHub secrets for the live deployment (P49, docs/DEPLOY.md). Run in your own terminal; nothing is printed or logged."""
import getpass
import os
import subprocess
import sys

REPO = "Juice-de-Orange/game-off-2026"
KEY = os.path.expanduser("~/.ssh/kestrel9_deploy_key")
LICENSE = r"C:\ProgramData\Unity\Unity_lic.ulf"


def put(name, value):
    subprocess.run(["gh", "secret", "set", name, "-R", REPO], input=value.encode("utf-8"), check=True)


if not os.path.exists(KEY):
    sys.exit(f"{KEY} is missing: the deploy key from docs/DEPLOY.md.")
if not os.path.exists(LICENSE):
    sys.exit(f"{LICENSE} is missing. Unity Hub -> Preferences -> Licenses -> Add -> Get a free personal license, then run this again.")

email = input("Unity account e-mail: ").strip()
password = getpass.getpass("Unity account password (not shown): ")
if not email or not password:
    sys.exit("E-mail and password are required.")

with open(KEY, encoding="ascii") as f:
    put("DEPLOY_KEY", f.read())
with open(LICENSE, encoding="utf-8") as f:
    put("UNITY_LICENSE", f.read())
put("UNITY_EMAIL", email)
put("UNITY_PASSWORD", password)
print("All four secrets are set.")
