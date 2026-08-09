#!/usr/bin/env bash
# PreToolUse hook: hard-block destructive commands regardless of how they are phrased.
# Exit 2 = block the tool call and show stderr to the model.
set -euo pipefail

# Prefer jq for precise extraction; fall back to matching the raw JSON when jq is absent
# (the raw match is coarser but fails safe rather than erroring out).
if command -v jq >/dev/null 2>&1; then
  command_text=$(jq -r '.tool_input.command // empty')
else
  command_text=$(cat)
fi

blocklist='database +drop|drop +database|rm +-rf +/|git +push +.*--force|git +reset +--hard +origin'

if echo "$command_text" | grep -Eiq "$blocklist"; then
  echo "Blocked by policy (block-dangerous.sh): this command class is never run by the agent." >&2
  echo "If a human really intends this, they run it themselves in a terminal." >&2
  exit 2
fi

exit 0
