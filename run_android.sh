#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../app"
flutter pub get
flutter emulators
printf '\nAndroid Studio > Device Manager ile emülatörü aç. Sonra:\nflutter devices\nflutter run -d EMULATOR_KIMLIGI\n'
