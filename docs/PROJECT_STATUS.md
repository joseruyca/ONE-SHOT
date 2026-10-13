# ONE SHOT — estado verificable — 0.5.0 — 2026-10-13

No hay una build móvil validada ni una versión comercial terminada. La exigencia de calidad del propietario no se ha alcanzado todavía. No se considera suficiente una ilustración, un mockup o un conjunto de tests para declarar el producto completo.

## Implementado

Cliente Unity 6000.3.26f1: práctica de Slice/Park/Mosquito, tres ejercicios por prueba, guías específicas, cuenta atrás, feedback de audio y puntos, resultados y récords locales, historial de 50 sesiones, ES/EN seleccionable, audio y reducción de movimiento, navegación fija y contenido desplazable. Salida con confirmación; abandonar el intento competitivo no lo recupera. Las interrupciones no conservan una partida a medias. Un resultado completo se guarda atómicamente antes del envío y se puede reenviar después de cerrar la app e identificarse con la misma cuenta. El archivo no incluye contraseña ni tokens; no permite reiniciar ni repetir pruebas. La copia solo se elimina tras confirmación del servidor o descarte explícito, que no devuelve el intento. Persistir el archivo no amplía el plazo del servidor: un submit nuevo fuera de plazo se rechaza; recuperar un submit ya confirmado sigue siendo idempotente.

Dirección Studio Toys: arte original generado, portada editorial y atlas transparente de los tres objetos jugables. Integrados en UI Toolkit; fuentes USS y ArtTheme permiten sustituir arte y paleta sin alterar el sistema de puntuación.

Cuenta: registro/login email, tokens y refresh solo en memoria, cierre de sesión remoto, borrado con reidentificación y confirmación explícita. Backend comprueba identidad en Auth y existencia de sesión. No se ejecutaron altas, correos ni borrados contra usuarios reales.

Crews: creación, invitación privada, unión idempotente, salida, borrado por propietario, máximo tres grupos/persona y 32 miembros/grupo. Código solo visible al propietario; no se exponen IDs de miembros en la clasificación. Resultados del grupo ocultos hasta completar el Shot propio. Presupuesto de 30 acciones de creación/unión por día contra abuso de códigos; salir no está sujeto a ese límite.

Runs: ciclos UTC de 14 días anclados al 2026-01-05, diez mejores resultados diarios, mínimo diez días para clasificar. Un solo resultado por día; resultados invalidados excluidos. Clasificación actual provisional. Esta definición concreta requiere evaluación de producto antes del lanzamiento.

Competición: servidor calcula las puntuaciones desde evidencias; inicio/envío idempotentes, consumición única, límites de tiempo, invalidación y ranking real. Flags competition/ads/reload apagados; no hay Shot publicado.

## Validación

26 pruebas Node/PGlite pasan en CI: puntuación, permisos, intentos, sesiones, revocación antes de borrado (HTTP simulado), Crews y Runs. PGlite no prueba carreras entre conexiones. El CI verifica núcleo C# y sintaxis Unity; no verifica tipos de las API Unity ni genera APK.

Arte inspeccionado: Hero 1024×1536 RGB; Toys 2172×724 RGBA con transparencia. No son capturas de la interfaz corriendo en Unity.

CI verde de la recuperación: https://github.com/joseruyca/ONE-SHOT/actions/runs/38386293959 (6e809eb8fb033f20858b2e3cee8cc0f7d3be3ca5). Backend, núcleo C# y sintaxis de fuentes Unity pasan.

La comprobación `ONE SHOT/Validate project` o `-executeMethod OneShot.Editor.Validation.Check` permite comprobar importación real y recursos en el editor instalado. El actualizador Windows ejecuta esa validación antes de abrir el editor. Todavía no ejecutado en este entorno, que carece de Unity y Windows.

## Pendiente para producto completo

Importación y verificación visual de esta revisión en Unity; build Android/iOS; flujo autenticado completo desde dispositivo; revisión de gestos, diversión y legibilidad; pruebas de rendimiento/accesibilidad; OAuth/deep links y almacenamiento seguro de sesión; recuperación de cuenta; exportación de datos; moderación/report/block; política de privacidad y datos legales del responsable; publicación de Shots y operación diaria; push; Pour/Balloon; catálogo/cosméticos; decisiones de Reload y anuncios. No activar monetización ni servicios facturados.

## Restricción económica

No se ha contratado ningún plan, servicio, dominio ni compra. Usar infraestructura existente dentro del plan gratuito, sin activar facturación adicional. La publicación en tiendas no está resuelta; no asumir que puede garantizarse coste cero.

## Despliegue bloqueado — 2026-10-13

La conexión Supabase permite lectura, pero las escrituras fallan con `McpServerError: Invalid or expired requestState` (dos intentos con el paquete completo, uno de execute_sql y un intento aislado de Crews/Runs sin modificar permisos de Auth). Se comprobó después del primer fallo que `oneshot.crews` y `public.os_session_active` no existían. No se desplegó una nueva Edge Function que dependiera de esas tablas inexistentes. El servidor conserva la versión anterior. Por tanto Crews, Runs y borrado de cuenta están implementados y probados en código, pero NO disponibles en el servidor real todavía.

No confundir un cierre de sesión Auth del cliente con la comprobación adicional de sesión en el servidor, que también espera despliegue. Scripts SQL preparados en backend/database/sessions.sql y social.sql; módulo account.mjs preparado para desplegar junto al endpoint. Antes de desplegar, resolver el error de escritura, aplicar ambas ampliaciones, verificar fixtures transaccionales, desplegar el endpoint y ejecutar QA autenticado.

## Corrección de importación — 2026-10-13

El propietario ejecutó el actualizador y Unity rechazó com.unity.modules.inputlegacy@1.0.0, que no es un paquete disponible. Se elimina esa dependencia errónea y el sondeo de UnityEngine.Input.GetKeyDown. Cancelar/Atrás se maneja mediante NavigationCancelEvent de UI Toolkit, con foco en el contenedor persistente de la interfaz. La comprobación de importación detuvo correctamente la apertura en lugar de anunciar éxito. Nueva importación en el Unity del propietario pendiente.

El segundo log de importación identifica cuatro CS0029 (Color32 a StyleColor) en AppScreens, OneShotApp, SocialScreens y StudioScreens. Se convierte explícitamente Color32 a Color antes de asignarlo al estilo; se corrigen también las ramas de los operadores condicionales para que su tipo sea Color. Son errores de tipos de la API Unity que el parser de sintaxis de CI no podía detectar. Nueva importación real pendiente.

El tercer log confirma compilación C# real exitosa en Unity 6000.3.26f1. La validación falla al cargar el arte, aunque los tres PNG se importan. Se configuran explícitamente TextureImporter/Texture2D, dimensiones máximas, NPOT y transparencia, con normalización y reimportación desde Editor. La validación identifica el archivo y tipo exactos y comprueba carga por AssetDatabase y Resources. El actualizador deja de usar -nographics para esta comprobación de texturas; debe volver a descargarse el script. No se declara resuelto en el dispositivo hasta repetir esa comprobación. Se corrigen además los avisos de scale en USS y uvRegion obsoleto (Unity remapea las UV automáticamente).
