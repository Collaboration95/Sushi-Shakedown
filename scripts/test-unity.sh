#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.1f1/Unity.app/Contents/MacOS/Unity}"
results_dir="${TEST_RESULTS_DIR:-$project_dir/Logs/Tests}"
if [[ ! -x "$unity_editor" ]]; then
  echo "Set UNITY_EDITOR to the Unity 6000.6.1f1 executable." >&2
  exit 1
fi
mkdir -p "$results_dir"
for platform in EditMode PlayMode; do
  "$unity_editor" -batchmode -nographics -projectPath "$project_dir" \
    -runTests -testPlatform "$platform" \
    -testResults "$results_dir/$platform.xml" -logFile "$results_dir/$platform.log"
  python3 - "$results_dir/$platform.xml" <<'PY'
import sys
import xml.etree.ElementTree as ET
report = ET.parse(sys.argv[1]).getroot()
if int(report.get('total', 0)) == 0 or int(report.get('failed', 0)) or report.get('result') != 'Passed':
    raise SystemExit('Unity suite failed or discovered no tests: ' + sys.argv[1])
print(f"{sys.argv[1]}: {report.get('passed')} passed")
PY
done
