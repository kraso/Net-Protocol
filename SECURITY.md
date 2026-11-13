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

Acuse de recibo en un plazo orientativo de 7 días y comunicación del plan de corrección tras la evaluación.

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

Los instaladores Linux van firmados con la clave GPG de releases `CD11DE8033B6E164`; la clave pública (`NetProtocol-gpg-pubkey.asc`) se adjunta a cada release. Instrucciones de verificación en el [README](README.md).
