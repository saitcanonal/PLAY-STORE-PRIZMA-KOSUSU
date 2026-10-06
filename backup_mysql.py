#!/usr/bin/env python3
"""Consistent JSON SQL snapshot and guarded restoration to an EMPTY database.
Requires: python3 -m pip install pymysql
PRISMA_MYSQL stays in environment; never prints credentials.
Usage: backup_mysql.py export backup.json | restore backup.json
"""
import os,sys,json,hashlib,re
from pathlib import Path
import pymysql
TABLES=('players','tickets','purchases','progression')
def connect():
    fields={k.strip().lower().replace(' ',''):v.strip() for k,v in (part.split('=',1) for part in os.environ['PRISMA_MYSQL'].split(';') if '=' in part)}
    db=fields['database']
    if not re.fullmatch(r'[a-zA-Z0-9_]{1,64}',db):raise ValueError('Invalid database')
    ssl_mode=fields.get('sslmode','required').lower()
    # Use the same verified TLS policy as your API for remote database backups.
    if ssl_mode=='none' and fields.get('server','127.0.0.1') not in ('127.0.0.1','localhost','mysql'):raise ValueError('Remote backups require TLS')
    return pymysql.connect(host=fields.get('server','127.0.0.1'),port=int(fields.get('port','3306')),database=db,user=fields.get('userid',fields.get('user','root')),password=fields.get('password',''),charset='utf8mb4',ssl=None if ssl_mode=='none' else {'check_hostname':True},autocommit=False)
def main():
    action,path=sys.argv[1:];path=Path(path);checksum=path.with_suffix(path.suffix+'.sha256')
    with connect() as db:
        if action=='export':
            tables=[]
            with db.cursor() as c:
                c.execute('SET TRANSACTION ISOLATION LEVEL REPEATABLE READ');c.execute('START TRANSACTION WITH CONSISTENT SNAPSHOT')
                for table in TABLES:
                    c.execute('SHOW CREATE TABLE `'+table+'`');create=c.fetchone()[1]
                    c.execute('SELECT * FROM `'+table+'`');rows=c.fetchall();columns=[x[0] for x in c.description]
                    tables.append({'name':table,'create':create,'columns':columns,'rows':rows})
            db.commit();raw=json.dumps({'version':1,'tables':tables},ensure_ascii=False).encode()
            tmp=path.with_suffix(path.suffix+'.tmp');tmp.write_bytes(raw);tmp.replace(path)
            os.chmod(path,0o600);checksum.write_text(hashlib.sha256(raw).hexdigest());os.chmod(checksum,0o600)
            print('Snapshot exported:',sum(len(t['rows']) for t in tables),'rows')
        elif action=='restore':
            raw=path.read_bytes()
            if hashlib.sha256(raw).hexdigest()!=checksum.read_text().strip():raise ValueError('Snapshot checksum mismatch')
            data=json.loads(raw)
            if data.get('version')!=1 or [t['name'] for t in data['tables']]!=list(TABLES):raise ValueError('Invalid snapshot')
            with db.cursor() as c:
                c.execute('SHOW TABLES')
                if c.fetchall():raise ValueError('Restoration requires an EMPTY target database')
                for table in data['tables']:
                    if not table['create'].startswith('CREATE TABLE `'+table['name']+'`'):raise ValueError('Invalid table DDL')
                    c.execute(table['create'])
                    for col in table['columns']:
                        if not re.fullmatch(r'[a-zA-Z0-9_]+',col):raise ValueError('Invalid column')
                    if table['rows']:
                        cols=','.join('`'+x+'`' for x in table['columns']);params=','.join(['%s']*len(table['columns']))
                        c.executemany('INSERT INTO `'+table['name']+'` ('+cols+') VALUES ('+params+')',table['rows'])
                    c.execute('SELECT COUNT(*) FROM `'+table['name']+'`')
                    if c.fetchone()[0]!=len(table['rows']):raise ValueError('Restored row count mismatch')
            db.commit();print('Snapshot restored; row counts verified. Start API and verify identity, wallet and rankings.')
        else:raise ValueError('Use export or restore')
if __name__=='__main__':main()
