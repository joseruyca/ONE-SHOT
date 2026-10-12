# Live backend QA — 2026-10-12

PASS Supabase migration competition_foundations and competition v1 ACTIVE with verify_jwt=true.
PASS transactional PostgreSQL start/idempotency/attempt consumption/hidden world/submit/ranking. Fixtures rolled back.
PASS HTTP no session 401; publishable key without session 401; privileged RPC 401.
INFO RLS without policies deliberately denies clients; unused index on empty database. See https://supabase.com/docs/guides/database/database-linter?lint=0008_rls_enabled_no_policy and https://supabase.com/docs/guides/database/database-linter?lint=0005_unused_index
NOT verified: real user JWT lifecycle, Unity import/build, mobile devices. Competition/reload/ads still disabled. Server keys not retrieved or committed.
