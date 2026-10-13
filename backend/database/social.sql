begin;
create table oneshot.crews (
 id uuid primary key default gen_random_uuid(),owner_id uuid not null references auth.users(id) on delete cascade,
 name text not null check(char_length(name) between 3 and 30),
 invite_code text not null unique default upper(substr(replace(gen_random_uuid()::text,'-',''),1,16)),created_at timestamptz not null default now()
);
create table oneshot.crew_members (
 crew_id uuid not null references oneshot.crews(id) on delete cascade,user_id uuid not null references auth.users(id) on delete cascade,
 nickname text not null check(char_length(nickname) between 2 and 20),joined_at timestamptz not null default now(),primary key(crew_id,user_id)
);
create index crew_members_user on oneshot.crew_members(user_id);
alter table oneshot.crews enable row level security;
alter table oneshot.crew_members enable row level security;
revoke all on oneshot.crews,oneshot.crew_members from public,anon,authenticated;
grant select,insert,update,delete on oneshot.crews,oneshot.crew_members to service_role;
create table oneshot.social_limits(user_id uuid primary key references auth.users(id) on delete cascade,day date not null default (now() at time zone 'UTC')::date,hits integer not null default 0);
alter table oneshot.social_limits enable row level security;
revoke all on oneshot.social_limits from public,anon,authenticated;
grant select,insert,update,delete on oneshot.social_limits to service_role;
create function oneshot.social_budget(p_user uuid) returns void language plpgsql security invoker set search_path='' as $$
declare n integer;
begin
 insert into oneshot.social_limits(user_id,hits) values(p_user,1) on conflict(user_id) do update set hits=case when oneshot.social_limits.day=(now() at time zone 'UTC')::date then oneshot.social_limits.hits+1 else 1 end,day=(now() at time zone 'UTC')::date returning hits into n;
 if n>30 then raise exception 'daily social limit';end if;
end $$;
revoke all on function oneshot.social_budget(uuid) from public,anon,authenticated;
grant execute on function oneshot.social_budget(uuid) to service_role;
create function public.os_social_budget(p_user uuid) returns boolean language plpgsql security invoker set search_path='' as $$ begin perform oneshot.social_budget(p_user);return true;end $$;
revoke all on function public.os_social_budget(uuid) from public,anon,authenticated;
grant execute on function public.os_social_budget(uuid) to service_role;
create function public.os_crews(p_user uuid) returns jsonb language sql security invoker set search_path='' as $$
 select jsonb_build_object('crews',coalesce(jsonb_agg(jsonb_build_object('id',c.id,'name',c.name,'owner',c.owner_id=p_user,'code',case when c.owner_id=p_user then c.invite_code else null end,'members',(select count(*) from oneshot.crew_members cm where cm.crew_id=c.id)) order by c.created_at),'[]'::jsonb))
 from oneshot.crews c join oneshot.crew_members m on m.crew_id=c.id and m.user_id=p_user;
$$;
create function public.os_crew_create(p_user uuid,p_name text,p_nickname text) returns jsonb language plpgsql security invoker set search_path='' as $$
declare c oneshot.crews;
begin
 perform pg_advisory_xact_lock(hashtextextended(p_user::text,0));
 if (select count(*) from oneshot.crew_members where user_id=p_user)>=3 then raise exception 'crew limit';end if;
 if p_name is null or p_name !~ '^[[:alnum:] _.-]{3,30}$' or p_nickname is null or p_nickname !~ '^[[:alnum:] _.-]{2,20}$' then raise exception 'invalid name';end if;
 insert into oneshot.crews(owner_id,name) values(p_user,p_name) returning * into c;
 insert into oneshot.crew_members(crew_id,user_id,nickname) values(c.id,p_user,p_nickname);
 return jsonb_build_object('id',c.id);
end $$;
create function public.os_crew_join(p_user uuid,p_code text,p_nickname text) returns jsonb language plpgsql security invoker set search_path='' as $$
declare c oneshot.crews;
begin
 perform pg_advisory_xact_lock(hashtextextended(p_user::text,0));
 if p_code is null or p_code !~ '^[0-9A-F]{16}$' or p_nickname is null or p_nickname !~ '^[[:alnum:] _.-]{2,20}$' then raise exception 'invalid input';end if;
 select * into c from oneshot.crews where invite_code=p_code for update;
 if not found then raise exception 'crew unavailable';end if;
 if exists(select 1 from oneshot.crew_members where crew_id=c.id and user_id=p_user) then return jsonb_build_object('id',c.id);end if;
 if (select count(*) from oneshot.crew_members where user_id=p_user)>=3 or (select count(*) from oneshot.crew_members where crew_id=c.id)>=32 then raise exception 'crew full';end if;
 insert into oneshot.crew_members(crew_id,user_id,nickname) values(c.id,p_user,p_nickname);
 return jsonb_build_object('id',c.id);
end $$;
create function public.os_crew_remove(p_user uuid,p_crew uuid,p_delete boolean) returns jsonb language plpgsql security invoker set search_path='' as $$
declare c oneshot.crews;
begin
 select * into c from oneshot.crews where id=p_crew for update;
 if not found or not exists(select 1 from oneshot.crew_members where crew_id=p_crew and user_id=p_user) then raise exception 'crew unavailable';end if;
 if p_delete then
  if c.owner_id<>p_user then raise exception 'owner required';end if;
  delete from oneshot.crews where id=p_crew;
 else
  if c.owner_id=p_user then raise exception 'owner must delete crew explicitly';end if;
  delete from oneshot.crew_members where crew_id=p_crew and user_id=p_user;
 end if;
 return jsonb_build_object('removed',true);
end $$;
create function public.os_crew_result(p_user uuid,p_crew uuid,p_shot uuid) returns jsonb language plpgsql security invoker set search_path='' as $$
begin
 if not exists(select 1 from oneshot.crew_members where crew_id=p_crew and user_id=p_user) then raise exception 'membership required';end if;
 if not exists(select 1 from oneshot.attempts where user_id=p_user and shot_id=p_shot and status='completed') then raise exception 'complete shot first';end if;
 return (select jsonb_build_object('results',coalesce(jsonb_agg(jsonb_build_object('nickname',nickname,'total',total,'rank',ranking,'you',user_id=p_user) order by total desc,nickname),'[]'::jsonb)) from (
  select m.nickname,m.user_id,a.total,rank() over(order by a.total desc) as ranking from oneshot.crew_members m join oneshot.attempts a on a.user_id=m.user_id and a.shot_id=p_shot and a.status='completed' where m.crew_id=p_crew
 ) standings);
end $$;
create function public.os_run(p_user uuid) returns jsonb language sql security invoker set search_path='' as $$
 with bounds as (select (date '2026-01-05'+((((now() at time zone 'UTC')::date-date '2026-01-05')/14)*14)) as start_day),
 daily as(select distinct on(a.user_id,(s.starts_at at time zone 'UTC')::date) a.user_id,a.total,s.starts_at from oneshot.attempts a join oneshot.shots s on s.id=a.shot_id cross join bounds b where a.status='completed' and s.starts_at>=b.start_day::timestamp at time zone 'UTC' and s.starts_at<(b.start_day+14)::timestamp at time zone 'UTC' order by a.user_id,(s.starts_at at time zone 'UTC')::date,a.total desc),
 results as(select user_id,total,row_number() over(partition by user_id order by total desc,starts_at) as position from daily),
 totals as(select user_id,count(*) as played,sum(total) filter(where position<=10) as total from results group by user_id),
 own as(select * from totals where user_id=p_user)
 select jsonb_build_object('startsAt',b.start_day,'endsAt',b.start_day+14,'played',coalesce(o.played,0),'total',coalesce(o.total,0),'qualified',coalesce(o.played>=10,false),'population',(select count(*) from totals where played>=10),'rank',case when o.played>=10 then (select 1+count(*) from totals where played>=10 and total>o.total) else 0 end,'provisional',true) from bounds b left join own o on true;
$$;
revoke all on function public.os_crews(uuid),public.os_crew_create(uuid,text,text),public.os_crew_join(uuid,text,text),public.os_crew_remove(uuid,uuid,boolean),public.os_crew_result(uuid,uuid,uuid),public.os_run(uuid) from public,anon,authenticated;
grant execute on function public.os_crews(uuid),public.os_crew_create(uuid,text,text),public.os_crew_join(uuid,text,text),public.os_crew_remove(uuid,uuid,boolean),public.os_crew_result(uuid,uuid,uuid),public.os_run(uuid) to service_role;
commit;
