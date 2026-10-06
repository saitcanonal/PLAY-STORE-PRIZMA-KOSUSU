#!/usr/bin/env python3
"""Run from your monitoring service. Nonzero exit signals an unavailable API/DB."""
import os,sys,json,urllib.request
base=os.environ.get('PRISMA_MONITOR_URL','http://127.0.0.1:5080').rstrip('/')
try:
    for route in ('/health','/health/ready'):
        with urllib.request.urlopen(base+route,timeout=6) as response:
            if response.status!=200:raise RuntimeError('Unhealthy service')
            json.load(response)
    print('API and database healthy')
except Exception:
    print('API/database health check failed',file=sys.stderr);sys.exit(1)
