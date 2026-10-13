// Only invoked after Auth verification and live-session validation in index.ts.
export async function deleteOwnAccount(base,key,jwt,userId,fetcher=fetch){
 if(!/^[0-9a-f-]{36}$/i.test(userId))throw new Error('invalid_identity');
 const logout=await fetcher(`${base}/auth/v1/logout?scope=global`,{method:'POST',headers:{apikey:key,Authorization:jwt},signal:AbortSignal.timeout(10000)});
 if(!logout.ok)throw new Error('revocation_failed');
 const deleted=await fetcher(`${base}/auth/v1/admin/users/${userId}`,{method:'DELETE',headers:{apikey:key,Authorization:`Bearer ${key}`},signal:AbortSignal.timeout(10000)});
 if(!deleted.ok)throw new Error('deletion_failed');
 return {deleted:true};
}

export function hasRecentPassword(claims,now=Date.now()/1000){
 return Array.isArray(claims?.amr)&&claims.amr.some(item=>item?.method==='password'&&Number.isSafeInteger(item.timestamp)&&item.timestamp<=now+30&&now-item.timestamp<=300);
}
