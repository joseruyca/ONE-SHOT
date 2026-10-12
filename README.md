# ONE SHOT — 0.2.0
Proyecto fuente Unity y backend de competición. No contiene APK ni es una app comercial terminada.

Cliente: client/ (Unity 6000.3.26f1). Código de UI/juegos y tema artístico sustituible. Backend: backend/database/schema.sql, backend/functions/competition/index.ts, evaluador y pruebas.

Estado verificable y pendientes: docs/PROJECT_STATUS.md. Concepto visual: art/direction.png y direction.svg.

Comandos para el agente, no para el propietario:
- QA backend: `cd backend && npm ci --ignore-scripts && npm test`.
- Unity import con licencia vigente y módulos Android/iOS; entrypoints `OneShot.Editor.Build.Android` / `OneShot.Editor.Build.IOS`.

La primera importación crea Main.unity y recursos si faltan. Sin backend configurado, la competición informa de que está pendiente; la práctica tiene lógica local. Nunca usar service_role en OneShotBackend.asset.

No ejecutar provisioning SQL contra Grupli o GratisCash. Antes de desplegar: proyecto separado gratuito, Auth, advisors, rate limiting, tests de integración, sesiones/borrado, build Unity y QA dispositivo. El propietario solo debe intervenir para acciones de cuenta/autorización que no se puedan automatizar.
