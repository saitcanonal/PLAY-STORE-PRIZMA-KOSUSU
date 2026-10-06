#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../app"
flutter pub get
open -a Simulator
flutter devices
printf '\nListeden iPhone simülatör kimliğini seçip şu komutu çalıştır:\nflutter run -d SIMULATOR_KIMLIGI\n'
