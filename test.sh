#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../app"
flutter pub get
flutter analyze
flutter test
dart ../scripts/engine_checks.dart
dart ../scripts/challenge_checks.dart
dart ../scripts/moving_checks.dart
dart ../scripts/v9_checks.dart
cd ../server
dotnet build -c Release
dotnet run --no-build -c Release -- --self-test

dart ../scripts/replay_fixtures.dart > obj/v11-fixtures.json
dotnet run --no-build -c Release -- --replay-fixtures obj/v11-fixtures.json
