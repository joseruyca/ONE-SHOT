# Studio Toys — dirección artística original

Objetivo: un pequeño mundo tangible de destreza, editorial y juguetón: crema cálida, azul cobalto, naranja y amarillo mantequilla. Pan, coche y mosquito comparten luz de estudio y materiales de resina/clay. Sin armas ni referencias copiadas de otro juego.

## Activos de producción

- `client/Assets/Resources/Studio/Hero.png`: master vertical/promocional, RGB, 1024×1536.
- `client/Assets/Resources/Studio/HeroWide.png`: portada de la tarjeta horizontal de la app, RGB, 1536×1024. Los tres objetos quedan dentro del recorte de la tarjeta.
- `client/Assets/Resources/Studio/Toys.png`: atlas transparente, RGBA, 2172×724. Tercios horizontales iguales: pan / coche / mosquito. Muestreo mediante UV en ToyCanvas; archivo original intacto.
- `client/Assets/Resources/Studio/Studio.uss`: estilos de portada, tarjetas, menú, timer, historial y diálogos.

Modo: imagegen nativo, dos generaciones nuevas sin referencia y una edición horizontal del master Hero. No assets de pago ni APIs contratadas. Los archivos se incorporan al proyecto; no son simples imágenes de presentación.

Prompt Hero: premium original mobile game artwork for ONE SHOT, no weapons, no text or UI. Portrait 2:3. Tactile miniature diorama, collectible designer toys, cream plaster studio, cobalt and tangerine, butter yellow. Rounded blue toy car on terracotta curved track, sculptural golden bread on a cream pedestal, whimsical violet mosquito with translucent wings, oversized orange sculptural ring, soft clay/satin resin materials and professional warm lighting. Calm top negative space, objects in center/lower two thirds.

Prompt Toys: production mobile game sprite sheet, transparent background, one horizontal row with isolated objects and padding: golden rectangular bread directly from above, cobalt designer toy car directly from above pointing right, friendly violet mosquito with expressive eyes and translucent wings. Equal thirds, completely visible. Cohesive satin resin/clay, warm studio light, no floor/pedestals/text/labels. Rectangular bread footprint for vertical cuts; butter yellow wheels.

## Sustituir con diseños propios

El asset `Resources/OneShotArt` tiene campos Hero, Sprite Atlas, Bread, Car y Mosquito Artwork. Los campos específicos prevalecen sobre el atlas. Puede conservarse la lógica y cambiar los cuatro diseños; `useStudioPalette=false` preserva la paleta personalizada. Los archivos de arte personalizados no incluidos en el repositorio no son sobrescritos por el actualizador Windows.

Pan: diseño desde arriba, centro visual alineado con el rectángulo de interacción x15–85% / y32–68%. Coche: morro hacia la derecha. Mosquito: centro visual alineado con el círculo de toque. No colocar etiquetas ni instrucciones dentro de los PNG; la interfaz las traduce.

## Criterios visuales aún pendientes de comprobar en Unity

Portada sin zonas vacías accidentales; contraste legible; textura nítida sin sangrado entre tercios; objetos y zonas de toque alineados; menú fijo; controles cómodos con una mano; flujo completo en pantallas pequeñas; animaciones sin interferir con precisión; sonidos agradables; arte sustituto validado con las mismas zonas de interacción.

Prompt HeroWide (edición, referencia Hero.png): recompose the exact Studio Toys artwork into landscape 3:2 for a mobile app card. Preserve designs, materials and lighting. Bread on pedestal left, cobalt car center-right foreground, violet mosquito floating center-left, orange arch behind. Keep all three objects in central 75% height / 90% width to survive card cropping. Reduce empty sky; no words, UI or labels.
