# Stack: verificado y pendiente — 2026-10-12
| Área | Decisión candidata | Estado |
|---|---|---|
| Motor | Unity 6.3 LTS | Familia con soporte hasta dic.2027; patch/build matrix pendiente |
| Android | IL2CPP ARM64, target SDK36, APK QA/AAB release | Google exige API36 desde 31/08/2026; sin build |
| iOS | IL2CPP, Xcode26+/SDK26+, TestFlight | Requisito vigente Apple; sin runner/certificados |
| Backend | Supabase separado, región UE | Cuenta conectada; proyecto ONE SHOT inexistente |
| C# | Evaluar SDK documentado por Supabase | No asumir «oficial» o Unity-ready; IL2CPP/pinning pendientes |
| Builds | Unity Build Automation preferida | Soporta Android/iOS/Git; coste/login/config pendientes |
| Ads | AdMob rewarded + UMP + SSV | Docs revisadas; SDK/cuenta/consent no integrados |
| Crash/analytics | Firebase Crashlytics/Analytics | Documentación Unity disponible; configurar consentimiento antes de SDK |
| Push | FCM/APNs | Docs Unity verificadas; cuentas/push keys pendientes |
| Links | App Links + Universal Links | No garantizan recuperación de invitación tras instalar |
| Deferred links | Landing + código recuperable, evaluar proveedor si preciso | No usar Firebase Dynamic Links, retirado |
| Privacidad | GDPR, borrado/exportación, mínimos datos | Política final depende de edad, proveedores y titular |
| Admin | Herramienta privada posterior | No tablero público con service_role |
No se han contratado servicios ni estimado costes de cuenta: el proyecto nuevo requiere organización elegida y confirmación del presupuesto devuelto por Supabase.
Changelog Supabase consultado: breaking change 25/09/2026 Postgres15.19/17.11 afecta ltree, cifrados pgcrypto antiguos, btree_gist con NaN y operadores personalizados. El diseño inicial no los necesita. Volver a revisar al provisionar.
Fuentes primarias:
- https://unity.com/releases/unity-6/support
- https://support.google.com/googleplay/android-developer/answer/11926878
- https://developer.apple.com/news/upcoming-requirements/
- https://supabase.com/docs/reference/csharp/introduction
- https://supabase.com/docs/reference/csharp/auth-signinanonymously
- https://supabase.com/changelog.md
- https://docs.unity.com/en-us/build-automation
- https://developers.google.com/admob/unity/rewarded
- https://developers.google.com/admob/unity/ssv
- https://developers.google.com/admob/unity/privacy
- https://firebase.google.com/docs/unity/setup
- https://firebase.google.com/docs/crashlytics/unity/get-started
- https://firebase.google.com/docs/cloud-messaging/unity/get-started
- https://firebase.google.com/support/dynamic-links-faq
- https://developer.android.com/training/app-links
- https://developer.apple.com/documentation/xcode/supporting-universal-links-in-your-app
- https://developer.apple.com/app-store/review/guidelines/
- https://eur-lex.europa.eu/eli/reg/2016/679/oj

Actualización 0.2.0: patch elegido 6000.3.26f1 (release oficial 08/10/2026), no validado en Build Automation. No contratar Build Automation sin comprobar gratuidad.
