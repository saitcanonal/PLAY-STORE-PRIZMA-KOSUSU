#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../app"
if [[ "$(uname -s)" != "Darwin" ]]; then
  printf 'Bu komutu Xcode kurulu Mac üzerinde çalıştır.\n' >&2
  exit 1
fi
flutter --version
# pubspec requires Flutter 3.41 or newer for the UIScene engine callback.
flutter clean
flutter pub get
(
  cd ios
  pod install
)
flutter build ios --simulator --debug
open ios/Runner.xcworkspace
