# Política de seguridad

## Versiones soportadas

Solo la última versión publicada recibe correcciones de seguridad.

| Versión | Soportada |
|---|---|
| 1.0.14 (última) | ✅ |
| anteriores | ❌ |

## Cómo reportar una vulnerabilidad

**No abras un issue público.** Usa el reporte privado de GitHub:

1. Entra en <https://github.com/kraso/Net-Protocol/security/advisories/new>.
2. Describe el impacto, los pasos de reproducción y la versión afectada.
3. Adjunta una captura o un PCAP si aplica (sin datos personales ni credenciales).

Si esa página no está disponible (reporte privado deshabilitado en el repositorio), abre un issue indicando solo que necesitas un canal privado, **sin detalles del fallo**, y se habilitará uno.

Este es un proyecto mantenido por una sola persona: los reportes se atienden por orden de llegada y sin plazo de respuesta garantizado. La evaluación y el plan de corrección se comunican cuando estén listos.

## Alcance

El proyecto es una aplicación de escritorio **local-first**: no hay servidor, ni cuentas, ni telemetría, ni envío de datos a terceros. Los datos de usuario viven en `%LOCALAPPDATA%\NetProtocol` (Windows) o el directorio equivalente del sistema.

Quedan dentro del alcance:

- Ejecución de código o escalada de privilegios a través de los instaladores (`.exe`, `.deb`, `.rpm`, `.AppImage`, `.dmg`).
- Fallos de robustez del lector PCAP/PCAPNG ante ficheros malformados (desbordamientos, bucles infinitos, agotamiento de memoria).
- Rutas de resolución de rutas y apertura de ficheros (path traversal, ejecución de comandos al abrir el explorador).
- Integridad de la cadena de suministro: firma GPG de los paquetes Linux y verificación de los artefactos de release.

Quedan fuera del alcance:

- La exactitud técnica del contenido documental (RFC, IEEE, IANA…). Para eso, abre un issue normal.
- Avisos de SmartScreen o Gatekeeper por ausencia de firma de código en Windows y macOS: es una decisión consciente del proyecto, documentada en el README.
- Vulnerabilidades en dependencias de terceros sin una ruta de explotación demostrable en esta aplicación.

## Verificación de los artefactos

Los instaladores Linux se firman con la clave GPG de releases `CD11DE8033B6E164`; la clave pública (`NetProtocol-gpg-pubkey.asc`) se adjunta a cada release. Instrucciones de verificación en el [README](README.md).

La firma depende de que los secretos `GPG_PRIVATE_KEY` y `GPG_KEY_ID` estén configurados en el repositorio. Si faltan, el workflow emite un aviso y publica los instaladores **sin firmar**: en ese caso el release no incluye `.asc` ni `NetProtocol-gpg-pubkey.asc`, y esa ausencia es la señal de que no hay firma que verificar.

Desde v1.1, cada release publica además `SHA256SUMS` con el hash de todos los instaladores. La aplicación lo usa antes de ofrecer una actualización: descarga el instalador a una carpeta temporal, comprueba su SHA256 y, si no coincide, lo borra y avisa.

**Qué garantiza y qué no:** el SHA256 garantiza que el archivo descargado es el que la release publica, no quién lo publicó. Sin firma de código, quien controle la release controla también el hash. Por eso la aplicación **no instala ni ejecuta nada automáticamente**: deja el instalador verificado en disco y el usuario decide cuándo abrirlo.
