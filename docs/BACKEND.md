# Backend y despliegue
Schema privado oneshot, tres tablas mínimas y cinco adaptadores RPC públicos restringidos a service_role. Cliente no tiene grants ni políticas de tabla. RLS activado como defensa adicional.
Endpoint competition exige bearer token y consulta Auth /user. El user_id de cualquier cuerpo se ignora. Cliente nunca envía score aceptado: backend recalcula con config de la base.
Hoy solo devuelve resumen, no parámetros. Start devuelve config tras consumir intento. Submit valida configuración, dueño, duración mínima, deadline y tres rounds. Ranking solo tras resultado válido y una fila por usuario/Shot.

Despliegue pendiente: crear proyecto separado en región UE con coste confirmado cero; activar Auth según estrategia; aplicar SQL iterativamente, advisors y pruebas A/B/anon; generar migrations via CLI; desplegar Edge Function con verify_jwt acorde a publishable keys/JWT actual después de verificar docs. La función hace su propia verificación de usuario, pero no asumir configuración CLI sin comprobarla.

No publicar sin rate limits/CAPTCHA o estrategia anti-abuso, sesión segura, recuperación, borrado/exportación y trazas minimizadas. Payload máximo 64KiB, timeouts 10s fetch. Flags disabled por defecto.
El evaluador verifica plausibilidad y score; no impide input sintético de bots. Profile skill stats y Crews aún no implementados.
