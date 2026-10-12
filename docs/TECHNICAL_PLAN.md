# Plan de implementación y puertas de calidad
1. Provisionar proyecto separado Supabase tras organización/coste aprobados; crear repo privado GitHub. El conector GitHub presente no expone creación de repositorios.
2. Seleccionar patch Unity6.3 soportado por build runner, crear/importar proyecto y fijar paquetes/locks generados. Configurar license y CI sin enviar secretos al chat.
3. DB iterativa: schema de mínimos, Auth, transacciones start/submit, RLS, tests A/B/anon, advisors; generar migrations con CLI después de validar. No tocar Grupli/GratisCash.
4. Integrar núcleo C# y endpoint de config/evidence. Implementar escenas Home/Results y tres juegos con assets propios, safe area y texto EN/ES. Instrumentar errores desde primera build.
5. Android APK y dispositivos: Shot completo, submit tras reconexión, intento consumido, score oculto antes de jugar, ranking real. No percentil inventado. No iniciar tres SDK simultáneamente antes de build base.
6. Lab separado y ajuste diversión. Mínimo 60fps objetivo con profiling p95; carga/tiempo táctil en gama media; paridad Android/iOS. Definir umbrales tras primeras mediciones.
7. Crews invitaciones, reportar/bloquear/expulsar; world agregado; compartir sin spoiler. Después Reload/SSV/consent según decisión de producto.
8. Pour/Balloon, Runs y skill stats cuando núcleo sea divertido. Beta cerrada y TestFlight; cuenta, privacidad, soporte, fichas y release.
No es cronograma prometido. Cada fase necesita evidencia verificable. No etiquetar listo para producción hasta QA dispositivo, backend adversarial, arte/licencias y distribución real.
