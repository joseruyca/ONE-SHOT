// Pure domain evaluator. Transport, authentication and DB transactions are NOT implemented.
export const scoringVersion = 'candidate-2';
function integer(x, min, max, label) {
  if (!Number.isSafeInteger(x) || x < min || x > max) throw new Error(`invalid ${label}`);
  return x;
}
export function precisionScore(actual, target, tolerance) {
  integer(actual,0,10000,'actual'); integer(target,0,10000,'target'); integer(tolerance,1,10000,'tolerance');
  const error=Math.min(Math.abs(actual-target),tolerance);
  return Math.floor(((tolerance-error)*10000+Math.floor(tolerance/2))/tolerance);
}
export function evaluateRound(config, events) {
  if (!config || !Array.isArray(events) || events.length>128) throw new Error('invalid evidence');
  if (config.trials !== undefined) {
    if (!Array.isArray(config.trials) || config.trials.length!==3 || events.length!==3) throw new Error('three trials required');
    integer(config.durationMs,1000,25000,'duration');
    if (config.trials.some(t=>t.game!==config.game || t.trials!==undefined) || config.trials.reduce((sum,t)=>sum+t.durationMs,0)!==config.durationMs) throw new Error('invalid trial configuration');
    return Math.floor(config.trials.reduce((sum,t,i)=>sum+evaluateRound(t,events[i]),0)/3);
  }
  const duration=integer(config.durationMs,1000,25000,'duration');
  let previous=-1;
  for (const e of events) {
    if (!e || typeof e!=='object') throw new Error('invalid event');
    integer(e.t,0,duration,'time');
    if(e.t<=previous) throw new Error('unordered events'); previous=e.t;
  }
  switch(config.game) {
    case 'slice': {
      integer(config.target,1,9999,'target'); integer(config.tolerance,1,10000,'tolerance');
      if(events.length===0) return 0;
      if(events.length!==1 || events[0].type!=='cut') throw new Error('invalid cut');
      // Candidate gameplay: vertical cut of constant-width object, x is basis points.
      return precisionScore(events[0].x,config.target,config.tolerance);
    }
    case 'park': {
      integer(config.target,0,10000,'target'); integer(config.tolerance,1,10000,'tolerance');
      integer(config.speedPerSecond,1,10000,'speed');
      if(events.length===0) return 0;
      if(events.length!==1 || events[0].type!=='brake') throw new Error('invalid brake');
      const position=Math.floor(events[0].t*config.speedPerSecond/1000);
      if(position>10000) return 0; // collision boundary
      return precisionScore(position,config.target,config.tolerance);
    }
    case 'mosquito': {
      if(!Array.isArray(config.windows)||config.windows.length<1||config.windows.length>32) throw new Error('invalid windows');
      let end=-1;
      for(const w of config.windows) {
        integer(w.start,0,duration,'window'); integer(w.end,w.start,duration,'window');
        integer(w.x,0,10000,'x');integer(w.y,0,10000,'y');integer(w.radius,1,10000,'radius');
        if(w.start<=end) throw new Error('overlapping windows');end=w.end;
      }
      const hit=new Set();let misses=0;
      for(const e of events) {
        if(e.type!=='tap') throw new Error('invalid tap');
        integer(e.x,0,10000,'x');integer(e.y,0,10000,'y');
        const index=config.windows.findIndex(w=>e.t>=w.start&&e.t<=w.end&&(e.x-w.x)**2+(e.y-w.y)**2<=w.radius**2);
        if(index<0||hit.has(index)) misses++;else hit.add(index);
      }
      return Math.max(0,Math.floor(hit.size*10000/config.windows.length)-misses*500);
    }
    default: throw new Error('unsupported game');
  }
}
export function evaluateShot(configs,evidence) {
  if(!Array.isArray(configs)||configs.length!==3||!Array.isArray(evidence)||evidence.length!==3) throw new Error('three rounds required');
  const scores=configs.map((c,i)=>evaluateRound(c,evidence[i]));
  return {scoringVersion,scores,total:scores.reduce((a,b)=>a+b,0)};
}
export function worldStanding(scores,own) {
  integer(own,0,30000,'score');
  if(!Array.isArray(scores)||scores.length===0) return null;
  scores.forEach(s=>integer(s,0,30000,'score'));
  if(!scores.includes(own)) throw new Error('own result missing');
  // Competition rank; ties share rank. Display as top %, not probability of winning.
  const rank=1+scores.filter(s=>s>own).length;
  return {rank,population:scores.length,topPercent:100*rank/scores.length};
}
