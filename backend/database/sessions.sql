-- Incremental addition for existing ONE SHOT projects; never grant clients Auth access.
begin;
grant usage on schema auth to service_role;
grant select(id,user_id,not_after) on auth.sessions to service_role;
create or replace function public.os_session_active(p_user uuid,p_session uuid) returns boolean
language sql security invoker set search_path='' as $$
 select exists(select 1 from auth.sessions where id=p_session and user_id=p_user and (not_after is null or not_after>now()));
$$;
revoke all on function public.os_session_active(uuid,uuid) from public,anon,authenticated;
grant execute on function public.os_session_active(uuid,uuid) to service_role;
commit;
