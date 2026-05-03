#!/usr/bin/env bash
# run-checks.sh — run optional dotnet build|format|test, emit structured JSON.
# Output JSON: {build:{ok,warnings[],errors[]}|null, format:{ok,violations[]}|null, test:{ok,failed[],duration}|null}
# Always exits 0 (tool failures are reported in JSON, not via exit code) unless usage error.
# DOTNET_BIN env overrides `dotnet` (used by tests).

set -u

usage() {
  cat <<EOF
run-checks.sh — run dotnet build/format/test and emit structured JSON

Usage:
  run-checks.sh --repo-root <path> [--build] [--format] [--test]
  run-checks.sh --help

Each requested check runs independently; failures are reported in JSON, not via exit code.
Set DOTNET_BIN to override the dotnet binary path (used by tests).
EOF
}

REPO_ROOT=""; DO_BUILD=0; DO_FORMAT=0; DO_TEST=0
while [[ $# -gt 0 ]]; do
  case "$1" in
    --repo-root) REPO_ROOT=${2:-}; shift 2 ;;
    --build)  DO_BUILD=1; shift ;;
    --format) DO_FORMAT=1; shift ;;
    --test)   DO_TEST=1; shift ;;
    --help|-h) usage; exit 0 ;;
    *) echo "unknown arg: $1" >&2; usage >&2; exit 1 ;;
  esac
done

[[ -n "$REPO_ROOT" && -d "$REPO_ROOT" ]] || { usage >&2; exit 1; }

DOTNET=${DOTNET_BIN:-dotnet}

run_dotnet() {
  # captures stdout+stderr into the named variable, returns the exit code
  local _out_var=$1; shift
  local _tmp; _tmp=$(mktemp)
  local _rc=0
  # shellcheck disable=SC2086
  ( cd "$REPO_ROOT" && $DOTNET "$@" ) >"$_tmp" 2>&1 || _rc=$?
  printf -v "$_out_var" '%s' "$(cat "$_tmp")"
  rm -f "$_tmp"
  return "$_rc"
}

json_string() {
  python3 -c 'import json,sys; print(json.dumps(sys.stdin.read()), end="")' <<<"$1"
}

json_string_array() {
  local first=1
  printf '['
  while IFS= read -r line; do
    [[ -z "$line" ]] && continue
    [[ $first -eq 0 ]] && printf ','
    first=0
    printf '%s' "$(json_string "$line")"
  done <<<"$1"
  printf ']'
}

BUILD_JSON="null"
if [[ $DO_BUILD -eq 1 ]]; then
  out=""
  run_dotnet out build --nologo --verbosity minimal || true
  warnings=$(grep -E ': warning [A-Z]+[0-9]+' <<<"$out" || true)
  errors=$(grep -E ': error [A-Z]+[0-9]+' <<<"$out" || true)
  ok="true"; [[ -n "$errors" ]] && ok="false"
  BUILD_JSON=$(printf '{"ok":%s,"warnings":%s,"errors":%s}' \
    "$ok" \
    "$(json_string_array "$warnings")" \
    "$(json_string_array "$errors")")
fi

FORMAT_JSON="null"
if [[ $DO_FORMAT -eq 1 ]]; then
  out=""
  rc=0
  run_dotnet out format --verify-no-changes --no-restore || rc=$?
  violations=$(grep -E '\([0-9]+,[0-9]+\)' <<<"$out" || true)
  ok="true"; [[ $rc -ne 0 ]] && ok="false"
  FORMAT_JSON=$(printf '{"ok":%s,"violations":%s}' \
    "$ok" "$(json_string_array "$violations")")
fi

TEST_JSON="null"
if [[ $DO_TEST -eq 1 ]]; then
  out=""
  rc=0
  start=$(date +%s)
  run_dotnet out test --nologo --verbosity minimal || rc=$?
  end=$(date +%s)
  failed=$(grep -E '^Failed:' <<<"$out" || true)
  ok="true"; [[ $rc -ne 0 ]] && ok="false"
  TEST_JSON=$(printf '{"ok":%s,"failed":%s,"duration":%d}' \
    "$ok" "$(json_string_array "$failed")" "$((end - start))")
fi

printf '{"build":%s,"format":%s,"test":%s}\n' "$BUILD_JSON" "$FORMAT_JSON" "$TEST_JSON"
