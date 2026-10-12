# Build
Unity pin 6000.3.26f1, release oficial 08/10/2026: https://unity.com/releases/editor/whats-new/6000.3.26f1
Se usa UI Toolkit integrado y módulos built-in, sin packages terceros de runtime. Manifest escrito, aún no resuelto por editor; packages-lock pendiente de generar al importar.

ProjectSetup genera escena y recursos; Build.Android exporta APK development IL2CPP ARM64 target36. Build.IOS exporta proyecto Xcode; firma/TestFlight requiere cuenta y toolchain apropiados. Identificador com.oneshot.daily es provisional, no reservado.
No hay SDK/toolchain Unity instalado ni licencia habilitada aquí. No se han ejecutado estos entrypoints. No hay CI remoto ni auto-build de pago.
Tras acceso a entorno gratuito autorizado: importar, resolver fallos, fijar lockfile, probar Home/las 3 pruebas/login/submit, APK y dispositivos; después AAB/firma y TestFlight. Verificar costes y cuotas antes de cualquier runner cloud.
