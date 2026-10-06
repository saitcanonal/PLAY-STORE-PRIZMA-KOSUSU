public static class AdminHtml {
 public const string Page = """
<!doctype html><html lang="tr"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Prizma Yönetim</title>
<style>body{font:16px system-ui;background:#0e1830;color:#edf8ff;max-width:1100px;margin:32px auto;padding:20px}button,input,select{font:inherit;border-radius:10px;padding:10px;background:#1a2c46;color:white;border:1px solid #567}button{cursor:pointer}pre{overflow:auto}label{display:block;margin:16px 0}table{width:100%;border-collapse:collapse}td,th{text-align:left;padding:10px;border-bottom:1px solid #345}.row{display:flex;gap:8px;flex-wrap:wrap;margin:8px 0}.row input{min-width:80px;max-width:240px}section{background:#15223b;padding:18px;border-radius:16px;margin:18px 0}#status{color:#80f5d0}</style>
<h1>Prizma Koşusu · Yönetim</h1><p>Görevler, kozmetik fiyatları, etkinlikler ve doğrulanmış koşu incelemesi.</p>
<label>Yönetici anahtarı <input id="key" type="password" autocomplete="off"></label><button id="load">Yükle</button><p id="status" role="status"></p>
<section><h2>Top fiyatları</h2><div id="prices"></div><p>Yalnız çevrimiçi cüzdan fiyatlarını değiştirir.</p></section>
<section><h2>Günlük kristal görevleri</h2><div id="tasks"></div><button id="addTask">Görev ekle</button><p>Ad · kristal hedefi · jeton ödülü. Gün içinde aynı görev ödülü bir kez alınır.</p></section>
<section><h2>Etkinlik takvimi</h2><div id="calendar"></div><button id="addEvent">Etkinlik ekle</button></section>
<button id="save">Değişiklikleri kaydet</button>
<section><h2>Koşu incelemesi</h2><table><thead><tr><th>Koşu / Mod</th><th>Puan</th><th>İnceleme</th></tr></thead><tbody id="runs"></tbody></table></section><h2>Son oyun olayları</h2><pre id="events"></pre>
<script>
const $=id=>document.getElementById(id);let config=null;
async function api(path,method='GET',body){const r=await fetch(path,{method,headers:{'X-Admin-Token':$('key').value,'Content-Type':'application/json'},body:body===undefined?undefined:JSON.stringify(body)});if(!r.ok)throw Error('İşlem başarısız: '+r.status);return r.json()}
function input(value,label,type,change){let el=document.createElement('input');el.value=value;el.type=type;el.setAttribute('aria-label',label);el.title=label;if(type==='number'){el.min=0;el.max=100000}el.onchange=()=>change(type==='number'?Number(el.value):el.value);return el}
function removeButton(action){let b=document.createElement('button');b.textContent='Kaldır';b.onclick=action;return b}
function render(){for(const id of ['prices','tasks','calendar'])$(id).replaceChildren();const names={rose:'Gül',solar:'Güneş',violet:'Ametist',ice:'Buz'};
 for(const [id,price] of Object.entries(config.prices)){let row=document.createElement('label');row.textContent=(names[id]||id)+' ';row.append(input(price,'Jeton fiyatı','number',v=>config.prices[id]=v));$('prices').append(row)}
 config.tasks.forEach((t,i)=>{let row=document.createElement('div');row.className='row';row.append(input(t.name,'Görev adı','text',v=>t.name=v),input(t.target,'Kristal hedefi','number',v=>t.target=v),input(t.reward,'Jeton ödülü','number',v=>t.reward=v),removeButton(()=>{config.tasks.splice(i,1);render()}));$('tasks').append(row)});
 config.events.forEach((e,i)=>{let row=document.createElement('div');row.className='row';row.append(input(e.name,'Etkinlik adı','text',v=>e.name=v),input(e.day,'Etkinlik tarihi','date',v=>e.day=v),removeButton(()=>{config.events.splice(i,1);render()}));$('calendar').append(row)})}
async function load(){try{const s=await api('/api/admin/state');config={prices:s.prices,tasks:s.tasks,events:s.events};render();$('events').textContent=JSON.stringify({summary:s.telemetrySummary,recent:s.telemetry},null,2);$('runs').replaceChildren();for(const r of s.runs){let tr=document.createElement('tr');for(const v of [r.runId+' / '+r.mode,r.score]){let td=document.createElement('td');td.textContent=v;tr.append(td)}let td=document.createElement('td');for(const [v,label] of [['approved','Onayla'],['rejected','Reddet'],['pending','Beklet']]){let b=document.createElement('button');b.textContent=label;b.onclick=async()=>{try{await api('/api/admin/runs/'+encodeURIComponent(r.runId),'PUT',{status:v});$('status').textContent='İnceleme kaydedildi'}catch(e){$('status').textContent=e.message}};td.append(b)}tr.append(td);$('runs').append(tr)}$('status').textContent='Güncel'}catch(e){$('status').textContent=e.message}}
$('load').onclick=load;$('save').onclick=async()=>{try{if(!config)throw Error('Önce ayarları yükle');await api('/api/admin/config','PUT',config);$('status').textContent='Kaydedildi'}catch(e){$('status').textContent=e.message}};
$('addTask').onclick=()=>{if(!config)return;config.tasks.push({id:crypto.randomUUID().replaceAll('-',''),name:'Yeni görev',target:40,reward:80});render()};$('addEvent').onclick=()=>{if(!config)return;config.events.push({id:crypto.randomUUID().replaceAll('-',''),name:'Yeni etkinlik',day:new Date().toLocaleDateString('sv-SE',{timeZone:'Europe/Istanbul'})});render()};
</script></html>
""";
}
