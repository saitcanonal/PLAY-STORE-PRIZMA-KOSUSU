"""V4 rule reference model: planning/consistency check only.
This is a Python port; it does NOT compile or execute the Dart/C# production code.
Run Flutter tests and C# self-tests to validate the actual implementations.
"""
import math
from dataclasses import dataclass

def lanes(z):
    if z < 450: return [0,1,2,3,4]
    cycle=math.floor((z-450)/180); phase=(z-450)%180; side=1 if cycle%2==0 else 2
    if phase<16:return [side,side+1]
    if phase<56:return [2]
    if phase<72:return [side,side+1]
    return [0,1,2,3,4]
@dataclass
class Item:
    z:float;lane:int;kind:int;resolved:bool=False
    @property
    def x(self):return (self.lane-2)*.48
class Model:
    def __init__(self,seed):
        self.random=seed;self.row=0;self.safe=2;self.nextz=14.;self.distance=0.;self.x=0.;self.target=0.;self.invul=0.;self.recovery=0.;self.hits=0;self.combo=0;self.crystals=0;self.score=0;self.ticks=0;self.items=[];self.done=False
        self.generate()
    def next(self):
        self.random=(self.random*1103515245+12345)&0x7fffffff
        return self.random/2147483648
    def generate(self):
        while self.nextz<self.distance+100:
            ls=lanes(self.nextz)
            safels=[l for l in ls if l in lanes(self.nextz-8) and l in lanes(self.nextz+8)]
            if self.row>2 and self.next()<.58:self.safe=max(0,min(4,self.safe+(1 if self.next()<.5 else -1)))
            if self.safe not in safels:self.safe=min(safels,key=lambda l:abs(l-self.safe))
            self.items.append(Item(self.nextz,self.safe,0))
            d=min(.75,.30+self.nextz/6500)
            transition=len(lanes(self.nextz-12))!=len(ls) or len(lanes(self.nextz+12))!=len(ls)
            for lane in range(5):
                if lane!=self.safe and lane in ls and self.row>4 and len(ls)==5 and not transition and self.next()<d:self.items.append(Item(self.nextz,lane,1))
            self.row+=1;self.nextz+=8
    def on(self,x):return any(abs(x-(l-2)*.48)<=.24 for l in lanes(self.distance))
    def damage(self,fall):
        if self.invul>0 or self.done:return
        self.hits+=1;self.combo=0;self.invul=1.8;self.recovery=.65 if fall else .18
        if self.hits>=3:self.done=True
    def step(self):
        if self.done:return
        dt=1/60;before=self.distance
        elapsed=self.ticks/60
        speed=10.5+elapsed*.24 if elapsed<=56.25 else 24+2*math.log(1+(elapsed-56.25)/60)
        self.distance+=speed*dt
        self.ticks+=1;self.invul=max(0,self.invul-dt);recovering=self.recovery>0
        if recovering:
            self.recovery=max(0,self.recovery-dt)
            if self.recovery==0:
                self.x=(min(lanes(self.distance),key=lambda l:abs(self.x-(l-2)*.48))-2)*.48;self.target=self.x
        else:
            self.x+=(self.target-self.x)*(1-math.exp(-17*dt))
            if not self.on(self.x):self.damage(True)
        for i in self.items:
            if self.done:break
            if i.resolved or i.z>self.distance or i.z<before:continue
            i.resolved=True
            if recovering or self.recovery>0 or not self.on(self.x):continue
            touching=abs(self.x-i.x)<.18
            if i.kind==0:
                if touching:self.crystals+=1;self.combo+=1;self.score+=100+min(self.combo,10)*10
                else:self.combo=0
            elif i.kind==1 and touching:self.damage(False)
        self.items=[i for i in self.items if i.z>=self.distance-8]
        self.generate()

for seed in range(1,41):
    g=Model(seed)
    while not g.done and g.ticks<216000:g.step()
    assert g.done,(seed,'stationary did not die')
print('PASS reference model: 40 stationary seeds terminate')
for seed in [1,6,67,81,99]:
    g=Model(seed);max_items=0
    for tick in range(60*60*15):
        nxt=next(i for i in g.items if i.kind==0 and i.z>g.distance)
        g.target=nxt.x
        g.step();max_items=max(max_items,len(g.items))
        assert not g.done,(seed,tick,g.distance,g.hits)
        assert max_items<90,(seed,max_items)
    print(f'PASS reference safe path seed {seed}: {g.distance:.1f}m, {g.hits} hits, {max_items} objects max')
g=Model(1);g.items=[Item(.01,0,0)];g.step();assert g.score==0 and g.distance>0
g.items=[Item(g.distance+.01,2,0)];g.step();assert g.score==110
for mode in [True,False,True]:
    g.invul=0;g.recovery=0
    if mode:g.x=g.target=1.4;g.items=[]
    else:g.x=g.target=0;g.items=[Item(g.distance+.01,2,1)]
    g.step()
assert g.done and g.hits==3
print('PASS reference score-only-on-pickup and fall/barrier/fall shared lives')
