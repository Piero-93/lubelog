#!/usr/bin/env bash
# Fails when the code has client-side navigations that bypass withBasePath().
# jQuery requests are covered by the ajaxPrefilter in shared.js and server-rendered
# src/href attributes by BasePathTagHelper, so only these few patterns need a look.
set -euo pipefail
cd "$(dirname "$0")/../.."

pattern='window\.location(\.href)?\s*=(?!=)(?!\s*withBasePath\()'
pattern+='|location\.(assign|replace)\('
pattern+='|window\.open\((?!\s*withBasePath\()'
pattern+='|fetch\('
pattern+='|\.withUrl\((?!\s*eventHubUrl\))'
pattern+='|new (WebSocket|EventSource)\('

if grep -rnP "$pattern" wwwroot/js Views --include='*.js' --include='*.cshtml'; then
    echo "::error::navigations above do not go through withBasePath()"
    exit 1
fi
echo "base path check: ok"
