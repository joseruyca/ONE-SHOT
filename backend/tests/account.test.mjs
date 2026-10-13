import test from 'node:test';
import assert from 'node:assert/strict';
import {deleteOwnAccount,hasRecentPassword} from '../functions/competition/account.mjs';
const id='10000000-0000-0000-0000-000000000001';
test('account deletion revokes all sessions before deleting only the verified owner',async()=>{
 const requests=[];const result=await deleteOwnAccount('https://project.example','server-secret','Bearer user-jwt',id,async(url,options)=>{requests.push({url,options});return {ok:true};});
 assert.deepEqual(result,{deleted:true});assert.equal(requests[0].url,'https://project.example/auth/v1/logout?scope=global');assert.equal(requests[0].options.headers.Authorization,'Bearer user-jwt');assert.equal(requests[1].url,`https://project.example/auth/v1/admin/users/${id}`);assert.equal(requests[1].options.method,'DELETE');assert.equal(requests[1].options.headers.Authorization,'Bearer server-secret');
});
test('failed revocation never proceeds to deletion; failed delete never reports success',async()=>{
 let calls=0;await assert.rejects(()=>deleteOwnAccount('https://project.example','key','Bearer jwt',id,async()=>{calls++;return {ok:false};}),/revocation_failed/);assert.equal(calls,1);
 calls=0;await assert.rejects(()=>deleteOwnAccount('https://project.example','key','Bearer jwt',id,async()=>({ok:++calls===1})),/deletion_failed/);
});
test('malformed identity cannot reach an admin endpoint',async()=>{await assert.rejects(()=>deleteOwnAccount('https://project.example','key','Bearer jwt','../other',async()=>assert.fail('unexpected network call')),/invalid_identity/);});

test('deletion requires fresh password authentication, not merely a refreshed JWT',()=>{const now=10000;assert.equal(hasRecentPassword({amr:[{method:'password',timestamp:9900}]},now),true);assert.equal(hasRecentPassword({iat:9999,amr:[{method:'password',timestamp:9000},{method:'token_refresh',timestamp:9999}]},now),false);assert.equal(hasRecentPassword({amr:[{method:'password',timestamp:11000}]},now),false);assert.equal(hasRecentPassword({amr:[{method:'oauth',timestamp:9999}]},now),false);assert.equal(hasRecentPassword({},now),false);});
