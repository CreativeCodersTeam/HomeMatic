#!/usr/bin/env bash
# collect-diff.sh — collect a unified diff in uncommitted or branch mode.
# Output JSON: {loc, files, file_list, diff}
# Exit codes: 0 ok (incl. empty diff), 1 usage, 2 not git, 3 baseline missing.

set -u

usage() {
  cat <<EOF
collect-diff.sh — collect a unified diff with exclusions applied

Usage:
  collect-diff.sh --repo-root <path> --mode uncommitted|branch [--baseline main]
  collect-diff.sh --help

Exclusions:
  - .gitignore (implicit via git diff)
  - *.min.js
  - wwwroot/lib/**

Exit codes:
  0  success (empty diff is success)
  1  usage error
  2  not a git repository
  3  baseline branch not found (branch mode only)
EOF
}

REPO_ROOT="" MODE="" BASELINE="main"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --repo-root) REPO_ROOT=${2:-}; shift 2 ;;
    --mode)      MODE=${2:-};      shift 2 ;;
    --baseline)  BASELINE=${2:-};  shift 2 ;;
    --help|-h)   usage; exit 0 ;;
    *) echo "unknown arg: $1" >&2; usage >&2; exit 1 ;;
  esac
done

[[ -n "$REPO_ROOT" && -n "$MODE" ]] || { usage >&2; exit 1; }
case "$MODE" in uncommitted|branch) ;; *) echo "invalid --mode: $MODE" >&2; exit 1 ;; esac
[[ -d "$REPO_ROOT/.git" ]] || { echo "not a git repository: $REPO_ROOT" >&2; exit 2; }

cd "$REPO_ROOT"

if [[ "$MODE" == "branch" ]]; then
  if ! git rev-parse --verify --quiet "$BASELINE" -- >/dev/null; then
    echo "baseline branch not found: $BASELINE" >&2
    exit 3
  fi
fi

# Guard: uncommitted mode against an empty repo (no HEAD yet) — abort cleanly.
if [[ "$MODE" == "uncommitted" ]] && ! git rev-parse --verify --quiet HEAD -- >/dev/null; then
  echo "repository has no commits yet (HEAD missing)" >&2
  exit 3
fi

# pathspec exclusions on top of .gitignore
EXCLUDES=(
  ':(exclude)*.min.js'
  ':(exclude,glob)wwwroot/lib/**'
)

if [[ "$MODE" == "uncommitted" ]]; then
  # working-tree changes vs HEAD (staged + unstaged + untracked)
  diff_payload=$(git diff HEAD --no-renames -- . "${EXCLUDES[@]}")
  # Append untracked files as added-from-empty diffs
  while IFS= read -r f; do
    [[ -z "$f" ]] && continue
    diff_payload+=$'\n'$(git diff --no-index --no-renames /dev/null "$f" 2>/dev/null || true)
  done < <(git ls-files --others --exclude-standard -- . "${EXCLUDES[@]}")
else
  diff_payload=$(git diff "$BASELINE"...HEAD --no-renames -- . "${EXCLUDES[@]}")
fi

# Count files & LOC from the diff
files=0
loc=0
file_list=()
while IFS= read -r line; do
  case "$line" in
    "+++ b/"*) f=${line#+++ b/}; [[ "$f" != "/dev/null" ]] && { file_list+=("$f"); files=$((files+1)); } ;;
    "+"*|"-"*)
      [[ "$line" == "+++ "* || "$line" == "--- "* ]] || loc=$((loc+1))
      ;;
  esac
done <<< "$diff_payload"

# emit JSON (escape diff payload for JSON string)
json_escape() {
  printf '%s' "$1" | python3 -c 'import json,sys; print(json.dumps(sys.stdin.read()), end="")'
}

list_json() {
  local first=1
  printf '['
  for v in "$@"; do
    [[ $first -eq 0 ]] && printf ','
    first=0
    printf '"%s"' "${v//\"/\\\"}"
  done
  printf ']'
}

diff_json=$(json_escape "$diff_payload")
fl_json=$(list_json ${file_list[@]+"${file_list[@]}"})

printf '{"loc":%d,"files":%d,"file_list":%s,"diff":%s}\n' \
  "$loc" "$files" "$fl_json" "$diff_json"
