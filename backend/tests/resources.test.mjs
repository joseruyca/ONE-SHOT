import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
const base=new URL('../../client/Assets/',import.meta.url);
async function files(url){const found=[];for(const entry of await fs.readdir(url,{withFileTypes:true})){const child=new URL(entry.name+(entry.isDirectory()?'/':''),url);if(entry.isDirectory())found.push(...await files(child));else found.push(child);}return found;}
test('all literal UI copy keys and game guides have Spanish and English text',async()=>{
 const catalog=JSON.parse(await fs.readFile(new URL('Resources/Copy.json',base),'utf8'));const keys=new Set();
 for(const e of catalog.entries){assert.ok(!keys.has(e.key),'duplicate key '+e.key);keys.add(e.key);for(const language of ['es','en'])assert.ok(typeof e[language]==='string'&&e[language].trim().length,'empty '+e.key+'/'+language);}
 for(const file of await files(base))if(file.pathname.endsWith('.cs')){const source=await fs.readFile(file,'utf8');for(const match of source.matchAll(/\bL\("([^"]+)"\)/g))assert.ok(keys.has(match[1]),'missing UI key '+match[1]);}
 for(const game of ['slice','park','mosquito'])assert.ok(keys.has(game+'_guide'),'missing game guide');
});
