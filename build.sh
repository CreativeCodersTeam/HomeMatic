#!/usr/bin/env bash

bash --version 2>&1 | head -n 1

set -e
SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)

###########################################################################
# CONFIGURATION
###########################################################################

BUILD_PROJECT_FILE="$SCRIPT_DIR/build/Build.csproj"
DOTNET_GLOBAL_FILE="$SCRIPT_DIR//global.json"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

###########################################################################
# EXECUTION
###########################################################################

# If dotnet CLI is installed globally and it matches requested version, use for execution
if [ -x "$(command -v dotnet)" ] && dotnet --version &>/dev/null; then
    export DOTNET_EXE="$(command -v dotnet)"
else
    echo "No matching dotnet version found"
    exit 1
fi

echo "Microsoft (R) .NET SDK version $("$DOTNET_EXE" --version)"

"$DOTNET_EXE" build "$BUILD_PROJECT_FILE" /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet
"$DOTNET_EXE" run --project "$BUILD_PROJECT_FILE" --no-build -- "$@"
