import sqlite3,pymysql,re,secrets,json,os,subprocess,tempfile,time,urllib.request,urllib.error
from pathlib import Path
root=Path(__file__).resolve().parent.parent
dotnet=os.environ.get('DOTNET_BIN','dotnet')
dart=os.environ.get('DART_BIN','dart')
dll=os.environ.get('PRISMA_TEST_DLL',str(root/'server/bin/Release/net8.0/Prisma.Server.dll'))
base='http://127.0.0.1:5088'
# Explicit dedicated test credentials. A unique database is created and dropped.
raw=os.environ['PRISMA_MYSQL_TEST']
fields=dict(part.split('=',1) for part in raw.split(';') if '=' in part)
fields={k.strip().lower().replace(' ',''):v.strip() for k,v in fields.items()}
prefix=fields.get('database','')
assert re.fullmatch(r'prisma_test_[a-zA-Z0-9_]{1,24}',prefix), 'Database must begin prisma_test_'
name=prefix+'_'+secrets.token_hex(4)
connection=pymysql.connect(host=fields.get('server','127.0.0.1'),port=int(fields.get('port','3306')),user=fields.get('userid',fields.get('user','root')),password=fields.get('password',''),autocommit=True)
with connection.cursor() as cursor:cursor.execute(f'CREATE DATABASE `{name}` CHARACTER SET utf8mb4')
mysql=';'.join(part for part in raw.split(';') if part and not part.lower().startswith('database='))+';Database='+name
connection.select_db(name)
checks=0
admin_key=secrets.token_hex(32)
with tempfile.TemporaryDirectory() as d:
 datafile=Path(d)/'league.sqlite'
 def start():
  env=os.environ.copy();env['PRISMA_MYSQL']=mysql;env['ASPNETCORE_URLS']=base;env['PRISMA_ADMIN_TOKEN']=admin_key
  p=subprocess.Popen([dotnet,dll],cwd=root/'server',env=env,stdout=subprocess.DEVNULL,stderr=subprocess.PIPE)
  for _ in range(60):
   try:urllib.request.urlopen(base+'/health',timeout=.2);return p
   except Exception:time.sleep(.05)
  p.terminate();_,err=p.communicate(timeout=5)
  raise RuntimeError('Server did not start: '+err.decode())
 def request(path,body=None,token=None,expected=200,method=None,admin=False):
  global checks
  h={'Content-Type':'application/json'}
  if token:h['Authorization']='Bearer '+token
  if admin:h['X-Admin-Token']=admin_key
  q=urllib.request.Request(base+path,data=json.dumps(body).encode() if body is not None else None,headers=h,method=method)
  try:
   with urllib.request.urlopen(q,timeout=15) as r:code=r.status;raw=r.read()
  except urllib.error.HTTPError as e:code=e.code;raw=e.read()
  assert code==expected,(path,code,raw[:200]);checks+=1
  return json.loads(raw) if raw else None
 env=os.environ.copy();env['PRISMA_MYSQL']=mysql
 legacy={'ReplayVersion':4,'Players':[{'Id':'a'*32,'Name':'Deniz A.','TokenHash':__import__('hashlib').sha256(b'legacy-token').hexdigest().upper(),'Score':450,'LegacyScore':0,'RecordAt':'2026-10-01T12:00:00+00:00','Entitlements':[],'Backup':'{"coins":12}'}],'Tickets':[],'Transactions':{}}
 with sqlite3.connect(datafile) as old:
  old.execute('CREATE TABLE league(id INTEGER PRIMARY KEY,payload TEXT NOT NULL)')
  old.execute('INSERT INTO league VALUES(1,?)',(json.dumps(legacy),))
 original=datafile.read_bytes()
 subprocess.run([dotnet,dll,'--import-sqlite',str(datafile)],env=env,check=True,stdout=subprocess.DEVNULL)
 assert datafile.read_bytes()==original;checks+=1
 assert subprocess.run([dotnet,dll,'--import-sqlite',str(datafile)],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL).returncode!=0;checks+=1
 with connection.cursor() as cursor:
  cursor.execute('SELECT name,score,backup FROM players WHERE id=%s',('a'*32,))
  assert cursor.fetchone()==('Deniz A.',450,'{"coins":12}');checks+=1
 p=start()
 try:
  assert subprocess.run([dotnet,dll,'--import-sqlite',str(datafile)],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL).returncode!=0;checks+=1
  assert request('/api/backup',token='legacy-token')['snapshot']=='{"coins":12}'
  assert request('/api/ranks',token='legacy-token')['me']['score']==450
  assert request('/health')['replayVersion']==12
  a=request('/api/players',{'name':'Saitcan Ö.'});b=request('/api/players',{'name':'Elif Y.'})
  request('/api/players',{'name':'<script>'},expected=400)
  request('/api/ranks',expected=401)
  request('/api/backup',expected=401)
  request('/api/backup',{'snapshot':'{}'},a['token'],method='PUT')
  assert request('/api/backup',token=a['token'])['snapshot']=='{}'
  assert request('/api/backup',token=b['token'])['snapshot'] is None
  request('/api/runs',{},expected=401)
  ta=request('/api/runs',{},a['token']);tb=request('/api/runs',{},b['token'])
  request('/api/runs/'+ta['id']+'/finish',{'ticks':1,'events':[]},b['token'],expected=404)
  request('/api/runs/'+ta['id']+'/finish',{'ticks':216001,'events':[]},a['token'],expected=400)
  request('/api/runs/'+ta['id']+'/finish',{'ticks':1,'events':[None]},a['token'],expected=400)
  request('/api/purchases/verify',{'productId':'prisma_remove_ads','purchaseId':'fake','source':'google_play','receipt':'fake'},a['token'],expected=503)
  for product in ['prisma_patterns_pack','prisma_trails_pack']:
   request('/api/purchases/verify',{'productId':product,'purchaseId':'fake','source':'google_play','receipt':'fake'},a['token'],expected=503)
  request('/api/purchases/verify',{'productId':'unknown','source':'google_play','receipt':'fake'},a['token'],expected=400)
  request('/api/admin/state',token=a['token'],expected=401)
  request('/api/wallet',expected=401)
  daily_a=request('/api/runs',{'mode':'daily'},a['token']);daily_b=request('/api/runs',{'mode':'daily'},b['token'])
  assert daily_a['seed']==daily_b['seed'];checks+=1
  request('/api/runs',{'mode':'bad'},a['token'],expected=400)
  request('/api/telemetry',{'kind':'hit','device':'api-test','stage':1,'hits':1,'tick':300},a['token'])
  request('/api/telemetry',{'kind':'bad','device':'api-test','stage':0,'hits':0,'tick':0},a['token'],expected=400)
  config={'prices':{'rose':3},'tasks':[{'id':'api-test','name':'Kristal görevi','target':1,'reward':12}],'events':[{'id':'test-event','name':'Test etkinliği','day':'2026-10-03'}]}
  request('/api/admin/config',config,method='PUT',admin=True)
  assert request('/api/config')['prices']['rose']==3;checks+=1
  proof=json.loads(subprocess.check_output([dart,str(root/'scripts/export_proof.dart'),str(ta['seed'])],text=True))
  p.terminate();p.wait(timeout=5)
  with connection.cursor() as cursor:
   cursor.execute('UPDATE tickets SET created=%s',(__import__('datetime').datetime.fromtimestamp(time.time()-3600,__import__('datetime').timezone.utc).isoformat(),))
   cursor.execute('UPDATE tickets SET seed=%s WHERE id=%s',(ta['seed'],tb['id']))
  p=start()
  payload={'ticks':proof['ticks'],'events':proof['events'],'score':99999999}
  ra=request('/api/runs/'+ta['id']+'/finish',payload,a['token']);assert ra['score']==proof['score'];checks+=1
  assert request('/api/runs/'+ta['id']+'/finish',payload,a['token'])==ra;checks+=1
  rb=request('/api/runs/'+tb['id']+'/finish',payload,b['token']);assert rb['score']==proof['score'];checks+=1
  ranks=request('/api/ranks',token=b['token']);assert ranks['rows'][0]['id']==a['id'] and ranks['me']['position']==2 and ranks['rival']['id']==a['id'];checks+=1
  assert 'token' not in json.dumps(ranks).lower();checks+=1
  p.terminate();p.wait(timeout=5);p=start()
  assert request('/api/ranks',token=a['token'])['me']['score']==proof['score'];checks+=1
  wallet=request('/api/wallet',token=a['token'])
  request('/api/runs/'+ta['id']+'/finish',payload,a['token'])
  assert request('/api/wallet',token=a['token'])==wallet;checks+=1
  request('/api/tasks/api-test/claim',{},a['token']);wallet=request('/api/wallet',token=a['token'])
  request('/api/tasks/api-test/claim',{},a['token']);assert request('/api/wallet',token=a['token'])==wallet;checks+=1
  request('/api/cosmetics/buy',{'id':'rose'},a['token']);wallet=request('/api/wallet',token=a['token'])
  request('/api/cosmetics/buy',{'id':'rose'},a['token']);assert request('/api/wallet',token=a['token'])==wallet;checks+=1
  recovery=request('/api/account/recovery',{},a['token'])['code'];recovered=request('/api/account/recover',{'code':recovery})
  request('/api/wallet',token=a['token'],expected=401);request('/api/account/recover',{'code':recovery},expected=401)
  assert request('/api/wallet',token=recovered['token'])==wallet;checks+=1
  rotated=request('/api/account/rotate',{},recovered['token'])['token'];request('/api/wallet',token=recovered['token'],expected=401)
  request('/api/admin/runs/'+ta['id'],{'status':'rejected'},method='PUT',admin=True)
  assert request('/api/period-ranks?mode=weekly',token=rotated)['me'] is None;checks+=1
  request('/health/ready')
  print(f'PASS: {checks} MySQL migration/HTTP/replay assertions. Dart and C# agree: {proof["score"]} points, {proof["ticks"]} ticks.')
 finally:
  p.terminate();p.wait(timeout=5)
  with connection.cursor() as cursor:cursor.execute(f'DROP DATABASE `{name}`')
  connection.close()
