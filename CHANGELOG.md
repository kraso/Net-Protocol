# Changelog

Todos los cambios relevantes de este proyecto. El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el versionado [SemVer](https://semver.org/lang/es/). El historial detallado, con causa raíz de cada fallo,
está en [`REGISTRO/Bitacora.md`](REGISTRO/Bitacora.md).

## [Sin publicar]

### Añadido
- `LICENSE` (MIT), `SECURITY.md` y este `CHANGELOG.md`.
- Soporte **arm64**: `linux-arm64` y `osx-arm64` en la matriz de build, y empaquetado `.deb` `arm64`,
  `.rpm` `aarch64`, AppImage `aarch64` y `.dmg` `arm64` en los jobs de release.

### Cambiado
- `on.push` acotado a `main` y tags `v*`: la matriz ya no se lanza en cada rama.
- Sincronización documental con la realidad publicada: README, `REGISTRO/Estado-de-Fases.md` y
  `FASE-II-DISENO/F2I-Diseno-de-Software.md` reflejan la v1.0.14 y las 78/78 pruebas.
- `FASE-II-DISENO/ci/` deja de duplicar el workflow: apunta a `.github/workflows/github-actions-ci.yml`
  como fuente de verdad única.

### Corregido
- `SECURITY.md` ya no compromete un plazo de acuse de recibo que el proyecto no puede garantizar, y
  ofrece una ruta alternativa si el reporte privado de GitHub no está disponible.
- `SECURITY.md` y README advierten de que la firma GPG de los instaladores Linux es opcional: si faltan
  los secretos del repositorio, el release se publica sin `.asc` ni clave pública.
- `FASE-II-DISENO/ci/README.md` documenta los 7 jobs reales (incluido `diagnostico-gpg`) y la cadena `needs`.
- `LICENSE` identifica al titular con nombre y apellidos.
- La tabla de releases del README marca v1.0.0 como retirada y v1.0.1/v1.0.2 como superseded.

### Eliminado
- Plantilla obsoleta `FASE-II-DISENO/ci/github-actions-ci.yml` (51 líneas frente a las 535 del workflow real).

## [1.0.14] — 2026-09-10

### Corregido
- Cierre determinista de los popups de información: un supervisor global (`DispatcherTimer` de 500 ms)
  cierra cada popup al cumplirse su deadline de 5 s desde que se abrió. La deadline solo se fija en la
  transición cerrado→abierto, de modo que los re-enters espurios de X11 ya no pueden reiniciarla; al
  cerrar se aplica una gracia de 1,2 s antes de permitir reabrir.

## [1.0.13] — 2026-09-10

### Corregido
- Cierre de tooltips incondicional por tiempo (6 s), sin depender de flags ni de la geometría del puntero:
  en X11 los eventos del overlay no llegan de forma fiable.

## [1.0.12] — 2026-09-10

### Añadido
- Auto-cierre de tooltips por tiempo con gracia anti-reabrir, como respaldo de X11.

### Corregido
- El RPM excluye los auto-Requires `liblttng-ust`/`liburcu`, que rompían la instalación en Fedora.

## [1.0.11] — 2026-09-10

### Cambiado
- Instaladores sin dependencias nativas del runtime de diagnóstico (`DOTNET_EnableDiagnostics=0`).
- Patrón de nombres unificado: `NetProtocol-<versión>-<SO>-<arquitectura>.<extensión>`, con `<instalador>.asc`
  para las firmas Linux.

## [1.0.10] — 2026-08-30

### Añadido
- Sistema de tokens de color por tema (`ThemeDictionaries`) y gradiente "cristal pizarra" en la barra de título.
- Selección y foco de `ListBox`/`ComboBox`/`TextBox` con la identidad de la app; divisores dinámicos.
- Diagramas adaptativos al modo oscuro.

## [1.0.9] — 2026-08-28

### Añadido
- Entrada de menú, icono y comando `netprotocol` en el RPM; `.desktop` e icono también en el `.deb`.
- Tema oscuro por defecto al arrancar, independiente del tema del escritorio.
- Documentación de instalación Linux con verificación GPG de la clave `CD11DE8033B6E164`.

### Corregido
- URL ascendente de los paquetes derivada de `GITHUB_REPOSITORY` (resistente a renombrados del repositorio).
- Permisos `0755` explícitos en los apphosts ELF: `download-artifact` perdía el bit de ejecución y KDE
  ocultaba la entrada de menú.

## [1.0.8] — 2026-08-28

### Corregido
- Tooltip de "Muestra de prueba" en una sola línea con la ruta completa de capturas (`ToolTip.SetTipTemplate`).

## [1.0.7] — 2026-08-28

### Añadido
- Tooltips (retardo 600 ms) en Comparar, Exportar, Abrir captura y Muestra de prueba.

### Cambiado
- El botón "Abrir captura..." pasa a "Abrir captura", sin puntos suspensivos.

## [1.0.6] — 2026-08-28

### Añadido
- Selector de protocolo en la barra superior, junto a la búsqueda.
- Botón "✕ Volver a la ficha" como salida explícita de la comparación.

### Cambiado
- Reordenación de la barra (Familia/Estado tras "Comparar con:").
- Mínimo de redimensión dinámico según el diagrama más ancho de la ficha.

## [1.0.5] — 2026-08-28

### Añadido
- Zoom por reflow: escala el tamaño de fuente real y los diagramas como unidad, con scroll horizontal propio.
- Redimensión por bordes de ventana (`BeginResizeDrag`), mínimo 960×600.

## [1.0.4] — 2026-08-28

### Corregido
- Instalador Windows robusto: `AppMutex` + `CloseApplications=force` cierra la app abierta antes de actualizar,
  y `[InstallDelete]` evita huérfanos de versiones previas. Actualización en el mismo directorio sin desinstalar;
  los datos de usuario en `%LOCALAPPDATA%\NetProtocol` se conservan.

## [1.0.3] — 2026-08-28

### Corregido
- Instalador construido desde el tag correcto: la v1.0.2 se había generado desde un tag anterior al grafo navegable.

## [1.0.2] — 2026-08-27

### Añadido
- Grafo navegable, exportación SVG/PNG/PDF con PNG 2×, vista de capturas y deduplicación fina con
  entity-linking IANA.

> Sustituida por la v1.0.3: el instalador no incluía el grafo navegable.

## [1.0.1] — 2026-08-26

### Corregido
- Bug de arranque de la v1.0.0.

> Sustituida por la v1.0.2.

## [1.0.0] — 2026-08-26

### Añadido
- Primera publicación: aplicación Avalonia 12.1.1 sobre .NET 9, catálogo de 113 protocolos, fichas,
  comparador, diagramas, capturas PCAP/PCAPNG e instaladores por SO.

> Retirada por un bug de arranque.
