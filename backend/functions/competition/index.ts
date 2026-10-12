// Deploy only after live Auth/RLS/advisors and anti-abuse checks. No external dependencies.
import { evaluateShot } from '../../core/scoring.mjs';
const base=Deno.env.get('SUPABASE_URL');
const key=Deno.env.get('SUPABASE_SERVICE_ROLE_KEY');
const uuid=/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const reply=(status:number,data:unknown)=>new Response(JSON.stringify(data),{status,headers:{'Content-Type':'application/json','Cache-Control':'no-store'}});
async function rpc(name:string,body:unknown) {
 const r=await fetch(`${base}/rest/v1/rpc/${name}`,{method:'POST',headers:{apikey:key!,Authorization:`Bearer ${key}`,'Content-Type':'application/json'},body:JSON.stringify(body),signal:AbortSignal.timeout(10000)});
 if(!r.ok)throw new Error('operation_rejected');return await r.json();
}
Deno.serve(async(req:Request)=>{
 if(!base||!key)return reply(503,{error:'not_configured'});
 if(req.method!=='POST')return reply(405,{error:'method_not_allowed'});
 const auth=req.headers.get('Authorization');if(!auth?.startsWith('Bearer '))return reply(401,{error:'authentication_required'});
 try{
  // Calls Auth server: does not decode unverified JWT or trust body.userId.
  const identity=await fetch(`${base}/auth/v1/user`,{headers:{apikey:key,Authorization:auth},signal:AbortSignal.timeout(10000)});
  if(!identity.ok)return reply(401,{error:'invalid_session'});
  const user=await identity.json();if(!uuid.test(user.id))return reply(401,{error:'invalid_session'});
  const reader=req.body?.getReader();if(!reader)return reply(400,{error:'invalid_body'});
  const chunks:Uint8Array[]=[];let size=0;
  while(true){const {value,done}=await reader.read();if(done)break;size+=value.byteLength;if(size>65536){await reader.cancel();return reply(413,{error:'payload_too_large'});}chunks.push(value);}
  const bytes=new Uint8Array(size);let offset=0;for(const part of chunks){bytes.set(part,offset);offset+=part.length;}
  const body=JSON.parse(new TextDecoder().decode(bytes));
  if(body.action==='today')return reply(200,{shot:await rpc('os_today',{})});
  if(body.action==='start'&&uuid.test(body.shotId)&&uuid.test(body.startKey))return reply(200,await rpc('os_start',{p_user:user.id,p_shot:body.shotId,p_key:body.startKey}));
  if(body.action==='world'&&uuid.test(body.shotId))return reply(200,await rpc('os_world',{p_user:user.id,p_shot:body.shotId}));
  if(body.action==='submit'&&uuid.test(body.attemptId)){
   const context=await rpc('os_context',{p_user:user.id,p_attempt:body.attemptId});
   if(!context||body.configHash!==context.configHash)return reply(409,{error:'attempt_mismatch'});
   if(context.scoringVersion!=='candidate-2')return reply(409,{error:'unsupported_scoring_version'});
   const calculated=evaluateShot(context.rounds,body.evidence); // no client score accepted
   return reply(200,await rpc('os_complete',{p_user:user.id,p_attempt:body.attemptId,p_hash:body.configHash,p_evidence:body.evidence,p_scores:calculated.scores}));
  }
  return reply(400,{error:'invalid_request'});
 }catch{return reply(409,{error:'operation_rejected'});}
});
