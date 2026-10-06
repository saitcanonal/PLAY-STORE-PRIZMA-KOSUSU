#!/usr/bin/env python3
"""Validated store build. No credentials are printed or uploaded."""
import argparse
import base64
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
from urllib.parse import urlsplit

ROOT = Path(__file__).resolve().parents[1]
TEST_PUBLISHER = '3940256099942544'

def enabled(c, k):
    return str(c.get(k, '')).lower() == 'true'

def validate(c, platform):
    errors = []
    if not enabled(c, 'STORE_READY'):
        errors.append('STORE_READY must be true after the store checklist is completed.')
    for key in ('PRIVACY_URL', 'SUPPORT_URL'):
        u = urlsplit(str(c.get(key, '')))
        if u.scheme != 'https' or not u.hostname or u.username or u.password or u.hostname in ('localhost', '127.0.0.1') or u.hostname.endswith(('.example', '.invalid')) or u.hostname == 'example.com':
            errors.append(key + ': enter your public HTTPS page.')
    if not re.fullmatch(r'[^\s@]+@[^\s@]+\.[^\s@]+', str(c.get('SUPPORT_EMAIL', ''))):
        errors.append('SUPPORT_EMAIL: enter a monitored support email.')
    # Initial store release is local-first: the supplied prototype API and
    # external receipt validator have not passed production integration tests.
    if c.get('PRISMA_API'):
        errors.append('Initial store release requires PRISMA_API empty. Production backend validation is pending.')
    if enabled(c, 'IAP_ENABLED'):
        errors.append('IAP is blocked: a deployed and tested receipt validator is required.')
    if enabled(c, 'ADS_ENABLED'):
        if not enabled(c, 'LIVE_ADS'):
            errors.append('ADS_ENABLED requires LIVE_ADS=true. Never publish test ads.')
        prefix = 'ANDROID' if platform == 'android' else 'IOS'
        for suffix, separator in [('ADMOB_APP_ID', '~'), ('REWARDED_ID', '/')]:
            key = prefix + '_' + suffix
            value = str(c.get(key, ''))
            if not re.fullmatch(r'ca-app-pub-\d{16}' + re.escape(separator) + r'\d{10}', value) or TEST_PUBLISHER in value:
                errors.append(key + ': enter a real AdMob identifier, not a test identifier.')
    return errors

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('platform', choices=['android', 'ios'])
    parser.add_argument('config', nargs='?')
    parser.add_argument('--check', action='store_true')
    parser.add_argument('--native', action='store_true')
    args = parser.parse_args()
    if args.native:
        if os.environ.get('CONFIGURATION') != 'Release':
            return
        c = dict(base64.b64decode(v).decode().split('=', 1) for v in os.environ.get('DART_DEFINES', '').split(',') if v)
    else:
        if not args.config:
            parser.error('config file required')
        config = Path(args.config).resolve()
        c = json.loads(config.read_text())
    errors = validate(c, args.platform)
    if errors:
        for error in errors:
            print('BLOCKED: ' + error, file=sys.stderr)
        sys.exit(2)
    if args.native:
        if args.platform == 'ios' and enabled(c, 'ADS_ENABLED') and os.environ.get('ADMOB_APP_ID') != c.get('IOS_ADMOB_APP_ID'):
            sys.exit('BLOCKED: AdMob iOS native ID differs. Build with scripts/release.py.')
        return
    if args.check:
        print('Configuration structure OK. Verify public pages, AdMob, signing and devices before submission.')
        return
    flutter = shutil.which('flutter')
    if not flutter:
        sys.exit('Flutter not found in PATH.')
    if args.platform == 'ios':
        if sys.platform != 'darwin':
            sys.exit('iOS archive requires a Mac and Xcode 26+.')
        result = subprocess.check_output(['xcodebuild', '-version'], text=True)
        if int(re.search(r'Xcode (\d+)', result)[1]) < 26:
            sys.exit('Xcode 26+ is required.')
        native_id = c.get('IOS_ADMOB_APP_ID') or 'ca-app-pub-3940256099942544~1458002511'
        (ROOT / 'app/ios/Flutter/Ads.xcconfig').write_text('ADMOB_APP_ID=' + native_id + '\n')
    elif not (ROOT / 'app/android/key.properties').exists():
        sys.exit('Create app/android/key.properties using key.properties.example. Keep the key private.')
    app = ROOT / 'app'
    subprocess.run([flutter, 'pub', 'get'], cwd=app, check=True)
    subprocess.run([flutter, 'analyze'], cwd=app, check=True)
    subprocess.run([flutter, 'test'], cwd=app, check=True)
    subprocess.run([flutter, 'build', 'appbundle' if args.platform == 'android' else 'ipa',
        '--release', '--dart-define-from-file=' + str(config)], cwd=app, check=True)
    print('Build complete. Upload manually to the internal test track / TestFlight first.')

if __name__ == '__main__':
    main()
