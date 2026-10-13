# ONE SHOT — 0.6 Motion Garden — estado real

El propietario ha rechazado la dirección Studio Toys hiperrealista. Se sustituye el arte por una dirección 2D original dibujada y animada en UI Toolkit. No se considera que el producto esté terminado ni se han verificado las animaciones de esta revisión en un dispositivo.

## Cambios de esta revisión

Portada animada con personajes vectoriales; pan con ojos/parpadeo y separación reactiva al corte; coche con ruedas animadas, vibración de marcha y rebote de frenada; BUZZ con alas animadas, objetivos que desaparecen al acertar y partículas de impacto. Paleta clara de coral, azul, lavanda y amarillo. Catálogo con escenas animadas, resultados con personajes y transiciones de entrada de páginas. Reducir movimiento desactiva animaciones decorativas y transiciones; el movimiento que forma parte de una prueba sigue funcionando.

La app no carga las imágenes hiperrealistas como arte por defecto. La validación comprueba el tema procedural en lugar de exigir PNG que ya no se utilizan. Los campos opcionales del ArtTheme permiten reemplazar personajes y portada por diseños propios mediante useCustomArtwork.

## Juego

Nuevo modo de práctica Flow: un corte/frenada válido, o completar todos los objetivos BUZZ, permite avanzar después del feedback sin esperar el resto de los cinco segundos. La simulación de Shot y la competición mantienen el tiempo completo. Una única arena persiste durante las tres acciones; se eliminan las pantallas intermedias de puntuación. Frenar y atrapar responden al contacto inicial del dedo; cortar mantiene el gesto hasta levantarlo. Los eventos siguen registrando tiempos relativos a cada configuración del servidor.

Los nombres visibles son SLICE/BRAKE/BUZZ; los identificadores internos slice/park/mosquito se conservan para el historial y el servidor. El evaluador de puntuación y las reglas competitivas no se han modificado.

## Conservado

Registro/login, cierre de sesión, historial, ajustes, ES/EN, recuperación duradera de resultados completos y código de Crews/Run/borrado. No se guardan contraseñas ni tokens en disco. El intento competitivo no se recupera al salir; un resultado guardado no amplía el plazo del servidor.

## Validación y bloqueos

El último log del propietario confirmó compilación nativa de la revisión anterior en Unity 6000.3.26f1. Esta revisión cambia render e interacción: necesita importación, Play y revisión de gestos, ritmo, animaciones, accesibilidad y rendimiento en Unity/móvil. CI comprueba dominio y sintaxis; no sustituye una prueba nativa de UI Toolkit.

Supabase conserva el endpoint anterior: la conexión rechaza las escrituras con Invalid or expired requestState. Crews, Runs, revocación estricta y borrado están implementados en fuentes, pero NO desplegados. Competition/ads/reload apagados, sin Shot activo. Ningún servicio de pago contratado.

## Pendiente del producto completo

Validar esta dirección con la app funcionando; QA autenticado en móvil; despliegue nuevo backend; builds Android/iOS; recuperación de cuenta; OAuth/deep links/almacenamiento seguro; exportación; moderación/report/block; privacidad y datos legales reales del responsable; operación de Shots; notificaciones; nuevos juegos; decisiones de Reload/monetización; publicación en tiendas. No declarar acabado o listo para publicar por tener CI verde.
