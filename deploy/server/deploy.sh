#!/bin/bash
# Forced command of the kestrel9 deploy key on Geoffrey (P49, docs/DEPLOY.md). Installed as /opt/kestrel9/deploy.sh.
# Usage over SSH: docker save kestrel9:<sha> | ssh geoffrey "deploy <sha>"
# Accepts one image named exactly kestrel9:<sha>, starts it and rolls back if it doesn't answer with that version.
set -euo pipefail

BASE=/opt/kestrel9
URL=http://127.0.0.1:18092
cd "$BASE"

read -r verb sha extra <<<"${SSH_ORIGINAL_COMMAND:-}"
if [[ "$verb" != deploy || ! "$sha" =~ ^[0-9a-f]{7,40}$ || -n "${extra:-}" ]]; then
  echo "usage: deploy <git sha>, image tar on stdin" >&2
  exit 2
fi

exec 9>"$BASE/.lock"
flock -n 9 || { echo "another deploy is running" >&2; exit 3; }

tar_file=$(mktemp -p "$BASE" upload.XXXXXX)
trap 'rm -f "$tar_file"' EXIT
# A build is well below this; the limit keeps a broken upload from filling the shared disk.
head -c 2147483648 > "$tar_file"

# docker load would also overwrite tags of other services: only this one image name may go through,
# wherever the archive names an image (manifest.json, repositories, index.json annotations).
if ! python3 - "$tar_file" "$sha" <<'PY'
import json, sys, tarfile

path, sha = sys.argv[1:]
allowed = {f"kestrel9:{sha}", f"docker.io/library/kestrel9:{sha}"}
names = []
with tarfile.open(path) as tar:
    def read(name):
        try:
            return json.load(tar.extractfile(name))
        except KeyError:
            return None
    for entry in read("manifest.json") or []:
        names += entry.get("RepoTags") or []
    for repo, tags in (read("repositories") or {}).items():
        names += [f"{repo}:{tag}" for tag in tags]
    for entry in (read("index.json") or {}).get("manifests", []):
        annotations = entry.get("annotations") or {}
        names += [annotations[k] for k in ("io.containerd.image.name",) if k in annotations]
        ref = annotations.get("org.opencontainers.image.ref.name")
        if ref and ref != sha:
            names.append(ref)
if f"kestrel9:{sha}" not in names or not set(names) <= allowed:
    sys.exit(f"found: {sorted(set(names)) or 'none'}")
PY
then
  echo "refused: image must be named exactly kestrel9:$sha" >&2
  exit 4
fi

previous=$(sed -n 's/^KESTREL9_TAG=//p' "$BASE/.env" 2>/dev/null || true)
# A second deploy of the same version replaces its tag: this one keeps the running image for the way back.
if [[ -n "$previous" ]] && docker image inspect "kestrel9:$previous" >/dev/null 2>&1; then
  docker tag "kestrel9:$previous" kestrel9:rollback
fi
docker load -i "$tar_file" >/dev/null

start() {
  echo "KESTREL9_TAG=$1" > "$BASE/.env"
  docker compose -f "$BASE/compose.yml" up -d --no-build --pull never --remove-orphans
}

answers() {
  local i
  for i in $(seq 1 20); do
    if [[ "$(curl -fsS --max-time 3 "$URL/version.txt" 2>/dev/null)" == "$1" ]] && curl -fsS --max-time 3 -o /dev/null "$URL/"; then
      return 0
    fi
    sleep 1
  done
  return 1
}

if start "$sha" && answers "$sha"; then
  # Keep the running and the previous version for a manual rollback, remove older ones.
  docker images kestrel9 --format '{{.Tag}}' | while read -r tag; do
    if [[ "$tag" != "$sha" && "$tag" != "$previous" && "$tag" != rollback ]]; then docker rmi -f "kestrel9:$tag" >/dev/null || true; fi
  done
  echo "live: $sha"
  exit 0
fi

echo "new version $sha did not answer" >&2
if [[ -z "$previous" ]]; then
  docker rmi -f "kestrel9:$sha" >/dev/null 2>&1 || true
  exit 1
fi
if [[ "$previous" != "$sha" ]]; then docker rmi -f "kestrel9:$sha" >/dev/null 2>&1 || true; fi
if docker tag kestrel9:rollback "kestrel9:$previous" && start "$previous" && answers "$previous"; then
  echo "rolled back to $previous" >&2
else
  echo "ROLLBACK FAILED: $previous does not answer either" >&2
fi
exit 1
