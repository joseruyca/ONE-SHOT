-- Provisioning SQL, not a generated migration. Apply only to a dedicated ONE SHOT project.
begin;
create schema if not exists oneshot;
revoke all on schema oneshot from public, anon, authenticated;
grant usage on schema oneshot to service_role;
create table oneshot.shots (
 id uuid primary key default gen_random_uuid(), label text not null,
 starts_at timestamptz not null, ends_at timestamptz not null,
 config jsonb not null check (jsonb_typeof(config)='array' and jsonb_array_length(config)=3),
 config_hash text not null, scoring_version text not null,
 enabled boolean not null default false,
 check (ends_at>starts_at), unique(starts_at)
);
create function oneshot.freeze_shot() returns trigger language plpgsql security invoker set search_path='' as $$
begin
 if now() >= old.starts_at and (new.config is distinct from old.config or new.config_hash is distinct from old.config_hash or new.scoring_version is distinct from old.scoring_version or new.starts_at is distinct from old.starts_at or new.ends_at is distinct from old.ends_at) then raise exception 'active shot is immutable'; end if;
 return new;
end $$;
create trigger freeze_active_shot before update on oneshot.shots for each row execute function oneshot.freeze_shot();
create table oneshot.attempts (
 id uuid primary key default gen_random_uuid(), user_id uuid not null references auth.users(id) on delete cascade,
 shot_id uuid not null references oneshot.shots(id), start_key uuid not null,
 started_at timestamptz not null default now(), deadline_at timestamptz not null,
 status text not null default 'started' check(status in ('started','completed','invalidated')),
 evidence jsonb, scores jsonb, total integer check(total between 0 and 30000),
 completed_at timestamptz,
 unique(user_id,shot_id), unique(user_id,start_key)
);
create index attempts_ranking on oneshot.attempts(shot_id,total desc) where status='completed';
create table oneshot.flags (name text primary key, enabled boolean not null default false);
insert into oneshot.flags values ('competition',false),('reload',false),('ads',false);
alter table oneshot.shots enable row level security;
alter table oneshot.attempts enable row level security;
alter table oneshot.flags enable row level security;
revoke all on all tables in schema oneshot from public,anon,authenticated;
grant select,insert,update,delete on all tables in schema oneshot to service_role;
-- No client table policies: client uses authenticated Edge API; privileged identity is checked there.
create function oneshot.start_attempt(p_user uuid,p_shot uuid,p_key uuid) returns jsonb
language plpgsql security invoker set search_path='' as $$
declare a oneshot.attempts; s oneshot.shots;
begin
 if p_user is null or p_key is null then raise exception 'identity required'; end if;
 perform pg_advisory_xact_lock(hashtextextended(p_user::text,0));
 select * into a from oneshot.attempts where user_id=p_user and start_key=p_key;
 if found then
  if a.shot_id<>p_shot then raise exception 'idempotency conflict'; end if;
  select * into strict s from oneshot.shots where id=a.shot_id;
  return jsonb_build_object('attemptId',a.id,'configHash',s.config_hash,'deadlineAt',a.deadline_at,'status',a.status,'rounds',s.config);
 end if;
 if not coalesce((select enabled from oneshot.flags where name='competition'),false) then raise exception 'competition disabled'; end if;
 select * into s from oneshot.shots where id=p_shot and enabled and now()>=starts_at and now()<ends_at;
 if not found then raise exception 'shot unavailable'; end if;
 if s.ends_at-now() < (select (sum((r->>'durationMs')::integer)+5000)*interval '1 millisecond' from jsonb_array_elements(s.config) r) then raise exception 'not enough time remaining'; end if;
 if exists(select 1 from oneshot.attempts where user_id=p_user and shot_id=p_shot) then raise exception 'attempt consumed'; end if;
 insert into oneshot.attempts(user_id,shot_id,start_key,deadline_at)
 values(p_user,p_shot,p_key,least(s.ends_at,now()+interval '120 seconds')) returning * into a;
 return jsonb_build_object('attemptId',a.id,'configHash',s.config_hash,'deadlineAt',a.deadline_at,'status',a.status,'rounds',s.config);
end $$;
create function oneshot.attempt_context(p_user uuid,p_attempt uuid) returns jsonb
language sql security invoker set search_path='' as $$
 select jsonb_build_object('configHash',s.config_hash,'scoringVersion',s.scoring_version,'rounds',s.config,'status',a.status,'total',a.total,'deadlineAt',a.deadline_at)
 from oneshot.attempts a join oneshot.shots s on s.id=a.shot_id where a.id=p_attempt and a.user_id=p_user;
$$;
create function oneshot.complete_attempt(p_user uuid,p_attempt uuid,p_hash text,p_evidence jsonb,p_scores jsonb) returns jsonb
language plpgsql security invoker set search_path='' as $$
declare a oneshot.attempts; s oneshot.shots; v_total integer; min_elapsed integer;
begin
 select * into a from oneshot.attempts where id=p_attempt and user_id=p_user for update;
 if not found then raise exception 'attempt unavailable'; end if;
 if a.status='invalidated' then raise exception 'attempt invalidated'; end if;
 if a.status='completed' then
  if a.evidence is distinct from p_evidence then raise exception 'submit conflict'; end if;
  return jsonb_build_object('total',a.total,'scores',a.scores,'status','completed');
 end if;
 select * into strict s from oneshot.shots where id=a.shot_id;
 if not s.enabled or not coalesce((select enabled from oneshot.flags where name='competition'),false) then raise exception 'competition disabled'; end if;
 if s.config_hash<>p_hash then raise exception 'config mismatch'; end if;
 if now()>a.deadline_at then raise exception 'attempt expired'; end if;
 select sum((r->>'durationMs')::integer) into min_elapsed from jsonb_array_elements(s.config) r;
 if extract(epoch from (now()-a.started_at))*1000 < min_elapsed then raise exception 'submitted too early'; end if;
 if p_evidence is null or jsonb_typeof(p_evidence)<>'array' or jsonb_array_length(p_evidence)<>3 then raise exception 'three rounds required'; end if;
 if p_scores is null or jsonb_typeof(p_scores)<>'array' or jsonb_array_length(p_scores)<>3 then raise exception 'three scores required'; end if;
 if exists(select 1 from jsonb_array_elements(p_scores) v where jsonb_typeof(v)<>'number' or v::text !~ '^[0-9]+$' or (v::text)::integer not between 0 and 10000) then raise exception 'invalid score'; end if;
 select sum((v::text)::integer) into v_total from jsonb_array_elements(p_scores) v;
 update oneshot.attempts set status='completed',evidence=p_evidence,scores=p_scores,total=v_total,completed_at=now() where id=a.id;
 return jsonb_build_object('total',v_total,'scores',p_scores,'status','completed');
end $$;
create function oneshot.today() returns jsonb language sql security invoker set search_path='' as $$
 select jsonb_build_object('shotId',id,'label',label,'endsAt',ends_at,'durationMs',(select sum((r->>'durationMs')::integer) from jsonb_array_elements(config) r))
 from oneshot.shots where enabled and now()>=starts_at and now()<ends_at order by starts_at desc limit 1;
$$;
create function oneshot.world_result(p_user uuid,p_shot uuid) returns jsonb
language plpgsql security invoker set search_path='' as $$
declare own_score integer; n bigint; ranking bigint;
begin
 select total into own_score from oneshot.attempts where user_id=p_user and shot_id=p_shot and status='completed';
 if own_score is null then raise exception 'complete shot first'; end if;
 select count(*),1+count(*) filter(where total>own_score) into n,ranking from oneshot.attempts where shot_id=p_shot and status='completed';
 return jsonb_build_object('total',own_score,'rank',ranking,'population',n,'topPercent',100.0*ranking/nullif(n,0),'provisional',now()<(select ends_at from oneshot.shots where id=p_shot));
end $$;
-- Public RPC adapters are INVOKER and callable only by backend service identity.
create function public.os_start(p_user uuid,p_shot uuid,p_key uuid) returns jsonb language sql security invoker set search_path='' as $$ select oneshot.start_attempt(p_user,p_shot,p_key) $$;
create function public.os_context(p_user uuid,p_attempt uuid) returns jsonb language sql security invoker set search_path='' as $$ select oneshot.attempt_context(p_user,p_attempt) $$;
create function public.os_complete(p_user uuid,p_attempt uuid,p_hash text,p_evidence jsonb,p_scores jsonb) returns jsonb language sql security invoker set search_path='' as $$ select oneshot.complete_attempt(p_user,p_attempt,p_hash,p_evidence,p_scores) $$;
create function public.os_today() returns jsonb language sql security invoker set search_path='' as $$ select oneshot.today() $$;
create function public.os_world(p_user uuid,p_shot uuid) returns jsonb language sql security invoker set search_path='' as $$ select oneshot.world_result(p_user,p_shot) $$;
revoke all on all functions in schema oneshot from public,anon,authenticated;
grant execute on all functions in schema oneshot to service_role;
revoke all on function public.os_start(uuid,uuid,uuid),public.os_context(uuid,uuid),public.os_complete(uuid,uuid,text,jsonb,jsonb),public.os_today(),public.os_world(uuid,uuid) from public,anon,authenticated;
grant execute on function public.os_start(uuid,uuid,uuid),public.os_context(uuid,uuid),public.os_complete(uuid,uuid,text,jsonb,jsonb),public.os_today(),public.os_world(uuid,uuid) to service_role;
commit;
