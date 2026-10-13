# Motion Garden — dirección 2D y sistema de movimiento

Se retira Studio Toys como estética de la aplicación. La nueva identidad nace del gesto y del personaje, sin iluminación fotográfica, dioramas ni texturas realistas.

## Lenguaje

Contornos azul tinta, siluetas redondeadas, expresiones simples, grandes zonas claras y acentos coral, azul, lavanda y amarillo. La escena central es jugable o animada: una foto ya no sustituye al contenido del juego. Tipografía grande y pocas instrucciones simultáneas.

## Movimiento implementado

- Ambiente: flotación ligera en portada, parpadeo, ruedas, aleteo y escenas de catálogo.
- Acción: trazo al deslizar, separación del pan, rebote al frenar y partículas en la posición real del toque.
- Objetivos: BUZZ desaparece al acertar; no permanece como señuelo puntuable.
- Interfaz: entrada breve de páginas, respuesta de botones, contador de resultado.
- Accesibilidad: reducción de movimiento elimina flotación/parpadeo/ruedas decorativas/aleteo/partículas y transiciones; desplazamiento real del coche y aparición de objetivos conservan las reglas de juego.

## Coherencia de interacción

Las animaciones decorativas no desplazan el centro de un objetivo ni cambian el rectángulo válido del corte. El coche sigue la posición derivada del tiempo del servidor, con movimiento visual vertical suave. Los efectos se dibujan alrededor del toque, no alrededor del centro de pantalla. Al cambiar de acción se liberan los punteros para impedir que un gesto de la acción anterior puntúe en la nueva.

Flow acelera la práctica y conserva el feedback. Simulación y competición mantienen el tiempo completo. No se cambian tolerancias ni el evaluador online para hacer coincidir una animación.

## Sustitución por diseños propios

`Resources/OneShotArt`: activar useCustomArtwork y asignar Bread/Car/Mosquito Artwork/Hero para usar ilustraciones propias. Sin texturas asignadas se utiliza el dibujo procedural. Las animaciones de las piezas vectoriales no se transfieren automáticamente a una imagen fija; para personajes propios animados hay que proporcionar rig o frames y conectar su reproducción. La paleta sigue siendo configurable desactivando useStudioPalette, conservado por compatibilidad con el asset anterior.

## Criterios de aceptación pendientes de prueba nativa

Siluetas legibles en móvil pequeño, ritmo comprensible, feedback sin bloquear el siguiente gesto, tacto alineado con lo visible, 60 FPS sostenidos en gama media, ausencia de recortes, modo de movimiento reducido comprobado y valoración del propietario sobre la identidad visual. Ningún mockup sustituye esa comprobación.
