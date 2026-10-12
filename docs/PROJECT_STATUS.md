# Estado real — 2026-10-12 — 0.2.0
Stable build: ninguna. APK/AAB/IPA: ninguno.

## Creado
- Proyecto fuente Unity 6000.3.26f1 con inicialización/editor build entrypoints, recursos y metadatos estables.
- Cliente UI Toolkit: Home, tres micropruebas (Slice, Park, Mosquito), tres ejercicios por Round, práctica, acceso email/password a Supabase, arranque competitivo, envío de evidencias, reintento de submit y resultados/world.
- ScriptableObject ArtTheme, recursos sustituibles, copy EN/ES en JSON, fuente del concepto visual SVG y PNG.
- Backend schema PostgreSQL: Shots, attempts, flags, inicio/submit idempotentes, ventana temporal, ocultación de ranking, RPC restringidas a servicio. Sin SECURITY DEFINER ni acceso directo de cliente.
- Edge API: verificación de identidad contra Auth, límite de cuerpo, evaluador de evidencia y RPC de servicio. Secretos solo en entorno.
- Evaluador candidate-2: tres ejercicios por prueba, media igualitaria por Round, suma 0–30.000.

## Verificado
16 pruebas Node/PGlite superadas. Prueban schema, permisos por roles simulados, inicio/reenvío, manipulación, invalidación, ventanas temporales y evaluación. PGlite ejecuta PostgreSQL WASM; no sustituye una prueba Supabase real, ni demuestra concurrencia entre conexiones.
Typecheck del endpoint sin errores con TypeScript; no se ha ejecutado el endpoint en Deno ni desplegado.
Los nueve controles C# previos siguen preservados; cubren el núcleo independiente, no los archivos nuevos con UnityEngine.
Concepto artístico renderizado e inspeccionado, no captura de la app.

## No completado
Cliente nuevo sin importación/compilación Unity ni prueba de dispositivos. No se garantiza que importe sin cambios.
Backend no desplegado, Auth sin configurar, sin proyecto separado . No hay ninguna competición online activa.
Crews, invitaciones, Pour/Balloon, Runs, OAuth móvil, account creation/deletion/export, report/block, ads/SSV/consent, push/crash, settings, persistencia segura/recovery, profiling, publicación y revisión de marca pendientes.
No publicar esta entrega como versión comercial. Default competition/ads/reload=false; sin claves ni scores/rankings falsos.

## Bloqueos concretos
Conector Supabase permite listar pero get_cost retorna UNAVAILABLE. Organización elegida: joseruyca's projects. No se ha obtenido coste ni creado proyecto.
Repositorio creado por propietario: joseruyca/ONE-SHOT, público. Permiso de integración habilitado 2026-10-12. Fuente subida y verificada en main (04abec429bee1c318272b7bd9319bc51fce02b0d); rama develop creada. CI añadido y verificado: backend y csharp-domain SUCCESS en https://github.com/joseruyca/ONE-SHOT/actions/runs/38255169864 (código 7fa74c0c3877650f1ccbc0cd1b29c586fcfe2714). Documento original OWNER_BRIEF.txt excluido del repositorio público.
Navegador alternativo requiere login del propietario, que usa contraseñas guardadas en su Chrome. No pedir contraseña por chat ni asumir sesión local disponible.
No hay Unity/licencia/Android toolchain en este entorno. Compilación con cuenta y entorno autorizado pendiente; no activar servicios de pago ni pruebas con facturación.

## Presupuesto
Cero costes autorizados. Ningún plan de pago, cargo, dominio ni contratación activados. Antes de recursos cloud o CI, comprobar plan/cuotas y bloquear gasto adicional.
