#!/usr/bin/env bash
# PreToolUse hook: merging a PR may be a production deploy. Force an explicit human step.
set -euo pipefail

if command -v jq >/dev/null 2>&1; then
  command_text=$(jq -r '.tool_input.command // empty')
else
  command_text=$(cat)
fi

if echo "$command_text" | grep -Eq 'gh +pr +merge'; then
  echo "Blocked (confirm-merge.sh): merging can deploy to production." >&2
  echo "Policy: the human runs the merge themselves after reviewing checks." >&2
  exit 2
fi

exit 0
