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
Backend desplegado en proyecto separado ONE-SHOT. Auth/configuración de cuentas aún pendiente de validar. No hay ninguna competición online activa.
Crews, invitaciones, Pour/Balloon, Runs, OAuth móvil, account creation/deletion/export, report/block, ads/SSV/consent, push/crash, settings, persistencia segura/recovery, profiling, publicación y revisión de marca pendientes.
No publicar esta entrega como versión comercial. Default competition/ads/reload=false; sin claves ni scores/rankings falsos.

## Bloqueos concretos
Proyecto ONE-SHOT creado por propietario y localizado (xdxcllpiblwiauxakwcv, eu-west-1). Schema competition_foundations aplicado y función competition v1 ACTIVE con verify_jwt=true. No se han obtenido claves secretas del servidor.
Repositorio creado por propietario: joseruyca/ONE-SHOT, público. Permiso de integración habilitado 2026-10-12. Fuente subida y verificada en main (04abec429bee1c318272b7bd9319bc51fce02b0d); rama develop creada. CI añadido y verificado: backend y csharp-domain SUCCESS en https://github.com/joseruyca/ONE-SHOT/actions/runs/38255169864 (código 7fa74c0c3877650f1ccbc0cd1b29c586fcfe2714). Documento original OWNER_BRIEF.txt excluido del repositorio público.
Navegador alternativo requiere login del propietario, que usa contraseñas guardadas en su Chrome. No pedir contraseña por chat ni asumir sesión local disponible.
No hay Unity/licencia/Android toolchain en este entorno. Compilación con cuenta y entorno autorizado pendiente; no activar servicios de pago ni pruebas con facturación.

## Presupuesto
Cero costes autorizados. Ningún plan de pago, cargo, dominio ni contratación activados. Antes de recursos cloud o CI, comprobar plan/cuotas y bloquear gasto adicional.

## Backend desplegado y verificado — actualización
Schema aplicado en Supabase real, API competition activa, configuración pública Unity conectada. QA transaccional real superado (fixtures rollback). Tres checks HTTP reales: sin sesión 401; publishable key sin sesión 401; RPC privilegiada 401. No se ha probado flujo con JWT de usuario real ni cliente móvil.
Security advisors: solo INFO por RLS sin políticas (deny por defecto, tablas privadas sin acceso de clientes). Performance: índice nuevo todavía sin uso. Competition/ads/reload desactivados hasta QA autenticado y primera build. No hay Shot activo ni usuarios finales creados.
CI tras conectar backend: SUCCESS https://github.com/joseruyca/ONE-SHOT/actions/runs/38256022409. Compilación Unity sigue pendiente de licencia/entorno autorizado.

## 0.2.1 — preparación Windows
Propietario ha activado Unity Personal en su ordenador y está instalando 6000.3.26f1 + Android/OpenJDK/SDK/NDK. Este entorno del agente sigue sin Unity.
Script Windows creado para descargar un commit concreto, preservar carpetas existentes, abrir proyecto o generar APK y mostrar log si falla. No ejecutado en Windows todavía.
Corregidos inputs simultáneos/cancelados, gestos fuera del objeto Slice y tamaños mínimos de controles. Pruebas de geometría añadidas al CI; ejecución completada con éxito en main/develop para 11d28184f8518727ae8ccd1628643727cd005283. Backend y núcleo C# verdes. Script Windows/cliente Unity todavía no ejecutados.

## 0.3.0 — revisión visual a partir de capturas reales
Usuario ha abierto y jugado el cliente 0.2.1 en Unity; capturas confirman importación previa y muestran interfaz deficiente.
Añadida cámara de fondo para eliminar aviso No cameras rendering. UI centrada con límite de anchura, tipografía en negrita, tarjetas con objetos reconocibles. Renderer ToyCanvas separado; pan/coche/mosquito dibujados con formas propias y texturas sustituibles. Feedback de puntos por acción añadido.
Updater Windows crea copia de archivos reemplazados, conserva recursos locales no distribuidos y abre el editor. Requiere cerrar Unity. No ejecutado en Windows aquí.
Nueva revisión aún pendiente de verificar visualmente en Unity; no se declara APK ni acabado de producción.

## 0.4.0 — experiencia completa de práctica
Navegación Hoy/Practicar/Tu espacio, catálogo de práctica, guía y cuenta atrás por juego, práctica individual o completa, desglose de resultados, mejores marcas locales, sonidos originales sintetizados con volumen configurable y ajuste de sonido/reduced motion.
Registro por email y contraseña añadido al cliente; requiere verificar configuración/delivery/redirects de Auth antes de release. No se han enviado correos de prueba ni creado usuarios desde el agente. Contraseñas y tokens no se persisten en PlayerPrefs.
Validación de desglose server-side recibido en cliente añadida al dominio; pruebas CI pendientes de resultado.
Cliente Unity de esta revisión todavía sin prueba visual/importación. No prometer perfección ni publicación. Crews, account recovery/deletion/export, push, OAuth y competición completa siguen pendientes.

Añadido parser Roslyn de todos los .cs Unity al CI para detectar sintaxis antes de la importación. Este chequeo no valida tipos/API Unity, render ni build móvil.
