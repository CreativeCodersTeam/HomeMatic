#!/usr/bin/env bash
# detect-dotnet-version.sh — Detect .NET SDK and target frameworks in a repo.
# Outputs JSON: {sdk, target_frameworks[], project_files[]}
# Exit codes: 0 ok, 1 usage, 4 SDK<10, 5 malformed project file.

set -u

usage() {
  cat <<EOF
detect-dotnet-version.sh — detect .NET SDK and target frameworks

Usage: detect-dotnet-version.sh --repo-root <path>
       detect-dotnet-version.sh --help

Exit codes:
  0  success
  1  usage error
  4  SDK below 10 or no .NET version detected
  5  malformed global.json or *.csproj
EOF
}

REPO_ROOT=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --repo-root) REPO_ROOT=${2:-}; shift 2 ;;
    --help|-h)   usage; exit 0 ;;
    *) echo "unknown arg: $1" >&2; usage >&2; exit 1 ;;
  esac
done

[[ -n "$REPO_ROOT" ]] || { echo "missing --repo-root" >&2; usage >&2; exit 1; }
[[ -d "$REPO_ROOT" ]] || { echo "not a directory: $REPO_ROOT" >&2; exit 1; }

# --- parse global.json (optional) ---
SDK="unknown"
if [[ -f "$REPO_ROOT/global.json" ]]; then
  SDK=$(grep -oE '"version"[[:space:]]*:[[:space:]]*"[^"]+"' "$REPO_ROOT/global.json" \
        | head -n1 | sed -E 's/.*"version"[[:space:]]*:[[:space:]]*"([^"]+)".*/\1/')
  if [[ -z "$SDK" ]]; then
    echo "malformed global.json: $REPO_ROOT/global.json" >&2
    exit 5
  fi
  sdk_major=${SDK%%.*}
  if [[ "$sdk_major" =~ ^[0-9]+$ ]] && [[ "$sdk_major" -lt 10 ]]; then
    echo "global.json pins SDK $SDK; this skill targets .NET 10+" >&2
    exit 4
  fi
fi

# --- find all *.csproj and extract <TargetFramework(s)> ---
project_files_json="[]"
tfms_json="[]"
# bash 3.2 compatible: read into array via while loop
projects=()
while IFS= read -r line; do
  projects+=("$line")
done < <(find "$REPO_ROOT" -type f -name '*.csproj' -not -path '*/bin/*' -not -path '*/obj/*' 2>/dev/null | sort)

if [[ ${#projects[@]} -eq 0 ]]; then
  echo "no *.csproj found under $REPO_ROOT" >&2
  exit 4
fi

all_tfms=()
rel_paths=()

for p in "${projects[@]}"; do
  rel=${p#"$REPO_ROOT/"}
  rel_paths+=("$rel")
  # crude check for malformed XML: must contain </Project>
  if ! grep -q '</Project>' "$p"; then
    echo "malformed csproj: $p" >&2
    exit 5
  fi
  # extract single or plural element
  tfm_line=$(grep -oE '<TargetFramework[s]?>[^<]+</TargetFramework[s]?>' "$p" | head -n1 || true)
  if [[ -z "$tfm_line" ]]; then
    echo "no TargetFramework in: $p" >&2
    exit 5
  fi
  tfm_value=$(printf '%s' "$tfm_line" | sed -E 's|<TargetFrameworks?>([^<]+)</TargetFrameworks?>|\1|')
  IFS=';' read -r -a parts <<<"$tfm_value"
  for t in "${parts[@]}"; do
    [[ -n "$t" ]] && all_tfms+=("$t")
  done
done

# --- enforce .NET 10+ ---
ok=0
for t in "${all_tfms[@]}"; do
  if [[ "$t" =~ ^net([0-9]+)\.[0-9]+$ ]]; then
    major=${BASH_REMATCH[1]}
    if [[ $major -ge 10 ]]; then ok=1; break; fi
  fi
done

if [[ $ok -eq 0 ]]; then
  echo "no target framework >= net10.0 detected; found: ${all_tfms[*]}" >&2
  exit 4
fi

# --- emit JSON (manual, jq-free) ---
json_array() {
  local first=1
  printf '['
  for v in "$@"; do
    [[ $first -eq 0 ]] && printf ','
    first=0
    printf '"%s"' "${v//\"/\\\"}"
  done
  printf ']'
}

tfms_json=$(json_array "${all_tfms[@]}")
project_files_json=$(json_array "${rel_paths[@]}")

printf '{"sdk":"%s","target_frameworks":%s,"project_files":%s}\n' \
  "$SDK" "$tfms_json" "$project_files_json"
