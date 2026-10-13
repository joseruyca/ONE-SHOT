# ONE SHOT — estado verificable — 0.5.0 — 2026-10-13

No hay una build móvil validada ni una versión comercial terminada. La exigencia de calidad del propietario no se ha alcanzado todavía. No se considera suficiente una ilustración, un mockup o un conjunto de tests para declarar el producto completo.

## Implementado

Cliente Unity 6000.3.26f1: práctica de Slice/Park/Mosquito, tres ejercicios por prueba, guías específicas, cuenta atrás, feedback de audio y puntos, resultados y récords locales, historial de 50 sesiones, ES/EN seleccionable, audio y reducción de movimiento, navegación fija y contenido desplazable. Salida con confirmación; abandonar el intento competitivo no lo recupera. Las interrupciones no conservan una partida a medias.

Dirección Studio Toys: arte original generado, portada editorial y atlas transparente de los tres objetos jugables. Integrados en UI Toolkit; fuentes USS y ArtTheme permiten sustituir arte y paleta sin alterar el sistema de puntuación.

Cuenta: registro/login email, tokens y refresh solo en memoria, cierre de sesión remoto, borrado con reidentificación y confirmación explícita. Backend comprueba identidad en Auth y existencia de sesión. No se ejecutaron altas, correos ni borrados contra usuarios reales.

Crews: creación, invitación privada, unión idempotente, salida, borrado por propietario, máximo tres grupos/persona y 32 miembros/grupo. Código solo visible al propietario; no se exponen IDs de miembros en la clasificación. Resultados del grupo ocultos hasta completar el Shot propio. Presupuesto de 30 acciones de creación/unión por día contra abuso de códigos; salir no está sujeto a ese límite.

Runs: ciclos UTC de 14 días anclados al 2026-01-05, diez mejores resultados diarios, mínimo diez días para clasificar. Un solo resultado por día; resultados invalidados excluidos. Clasificación actual provisional. Esta definición concreta requiere evaluación de producto antes del lanzamiento.

Competición: servidor calcula las puntuaciones desde evidencias; inicio/envío idempotentes, consumición única, límites de tiempo, invalidación y ranking real. Flags competition/ads/reload apagados; no hay Shot publicado.

## Validación

25 pruebas locales Node/PGlite pasan: puntuación, permisos, intentos, sesiones, revocación antes de borrado (HTTP simulado), Crews y Runs. PGlite no prueba carreras entre conexiones. El CI verifica núcleo C# y sintaxis Unity; no verifica tipos de las API Unity ni genera APK.

Arte inspeccionado: Hero 1024×1536 RGB; Toys 2172×724 RGBA con transparencia. No son capturas de la interfaz corriendo en Unity.

La comprobación `ONE SHOT/Validate project` o `-executeMethod OneShot.Editor.Validation.Check` permite comprobar importación real y recursos en el editor instalado. Todavía no ejecutada en este entorno, que carece de Unity.

## Pendiente para producto completo

Importación y verificación visual de esta revisión en Unity; build Android/iOS; flujo autenticado completo desde dispositivo; revisión de gestos, diversión y legibilidad; pruebas de rendimiento/accesibilidad; recuperación duradera de envíos pendientes; OAuth/deep links y almacenamiento seguro de sesión; recuperación de cuenta; exportación de datos; moderación/report/block; política de privacidad y datos legales del responsable; publicación de Shots y operación diaria; push; Pour/Balloon; catálogo/cosméticos; decisiones de Reload y anuncios. No activar monetización ni servicios facturados.

## Restricción económica

No se ha contratado ningún plan, servicio, dominio ni compra. Usar infraestructura existente dentro del plan gratuito, sin activar facturación adicional. La publicación en tiendas no está resuelta; no asumir que puede garantizarse coste cero.
