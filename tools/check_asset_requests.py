"""Validate every YAML block in docs/ASSET-REQUESTS.md. Run before pushing: python tools/check_asset_requests.py"""
import re
import sys

import yaml

text = open("docs/ASSET-REQUESTS.md", encoding="utf-8").read()
blocks = re.findall(r"### (\S+)\n(?:\*[^\n]*\n)?```yaml\n(.*?)```", text, re.S)
broken = 0
for name, block in blocks:
    try:
        yaml.safe_load(block)
    except yaml.YAMLError as e:
        broken += 1
        print(f"BROKEN {name}: {str(e).splitlines()[0]}")
print(f"{len(blocks)} entries checked, {broken} broken")
sys.exit(1 if broken else 0)
