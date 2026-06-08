#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LFS_BIN="$ROOT_DIR/.tools/bin/git-lfs"

if [[ ! -x "$LFS_BIN" ]]; then
  echo "Missing local git-lfs at $LFS_BIN"
  echo "Install Git LFS globally on your development PC, or ask the tooling owner to place a local binary here."
  exit 1
fi

git -C "$ROOT_DIR" config filter.lfs.clean "$LFS_BIN clean -- %f"
git -C "$ROOT_DIR" config filter.lfs.smudge "$LFS_BIN smudge -- %f"
git -C "$ROOT_DIR" config filter.lfs.process "$LFS_BIN filter-process"
git -C "$ROOT_DIR" config filter.lfs.required true

mkdir -p "$ROOT_DIR/.git/hooks"
cat > "$ROOT_DIR/.git/hooks/pre-push" <<HOOK
#!/bin/sh
"$LFS_BIN" pre-push "\$@"
HOOK
chmod +x "$ROOT_DIR/.git/hooks/pre-push"

"$LFS_BIN" version
