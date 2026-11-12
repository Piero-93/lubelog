#!/usr/bin/env bash
# Runs the image under /lubelogger and checks that pages, assets and redirects keep the prefix.
# Usage: smoke.sh <image>
set -euo pipefail
image="$1"
base="http://localhost:18080"
name="lubelogger-smoke"

docker run -d --name "$name" -p 18080:8080 -e LUBELOGGER_BASE_PATH=/lubelogger "$image" >/dev/null
trap 'docker logs "$name" | tail -20; docker rm -f "$name" >/dev/null' EXIT

for _ in $(seq 30); do
    curl -fs -o /dev/null "$base/lubelogger/" && break
    sleep 1
done

fail=0
check() { if [ "$2" = "$3" ]; then echo "ok   $1"; else echo "FAIL $1: expected '$3', got '$2'"; fail=1; fi; }

html=$(curl -fs "$base/lubelogger/")
unprefixed=$(grep -oE '(src|href)="/[^"]*"' <<<"$html" | grep -v '="/lubelogger/' || true)
check "no unprefixed src/href on the garage page" "$unprefixed" ""
check "shared.js served under the prefix" "$(curl -s -o /dev/null -w '%{http_code}' "$base/lubelogger/js/shared.js")" "200"
check "default images served under the prefix" "$(curl -s -o /dev/null -w '%{http_code}' "$base/lubelogger/defaults/noimage.png")" "200"
check "redirects keep the prefix" "$(curl -s -o /dev/null -w '%{redirect_url}' "$base/lubelogger/Admin")" "$base/lubelogger/Error/403"
check "root path still works" "$(curl -s -o /dev/null -w '%{http_code}' "$base/Home/Index")" "200"

exit $fail
