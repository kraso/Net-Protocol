# Bitácora del proyecto

Registro cronológico de decisiones, hitos y cambios. Cada entrada: fecha, evento, decisión/acción, responsable y referencia.

---

## 26-08-2026 — Inicio del proyecto y Fase 0

**Evento:** Se aprueba el plan maestro `PLANREDES.md` v1.0 como documento rector (derivado del master prompt `Prompt_Maestro_Aplicacion_Redes_Investigacion_y_Tecnologia.docx`). Se inicia el proceso de creación de proyecto y la **Fase 0 — Definición y límites**.

**Acciones realizadas:**
1. Creación del esqueleto del repositorio: `README.md`, `REGISTRO/`, `PLANTILLAS/`, `ESQUEMA/`.
2. Generación de los documentos de la Fase 0 en `FASE-00-DEFINICION/`:
   - Carta de alcance (objetivo, audiencia, profundidad, alcance/no-alcance).
   - Glosario de PDU y reglas de nomenclatura.
   - Ejes de clasificación y estados de ciclo de vida.
   - Política de fuentes y evidencia (incl. militares/públicas).
   - Política de incertidumbre (grados de confianza, marcas, registro de conflictos).
   - Criterios de aceptación de la fase (checklist C1–C9).

**Decisión:** La Fase 0 queda **pendiente de aprobación** por el responsable del proyecto. La aprobación se registra en `F0-Carta-de-Alcance.md` (tabla de aprobación) y desbloquea la Fase 1.

**Pendiente:** Revisión por el usuario: (a) validar audiencia/profundidad propuestas, (b) confirmar alcance y no-alcance, (c) firmar la aprobación de F0.

## 26-08-2026 — Fase 0 aprobada; inicio de la Fase 1

**Evento:** El responsable aprueba la **Fase 0 — Definición y límites** (firma en `F0-Carta-de-Alcance.md` §11). Se desbloquea la **Fase 1 — Inventario maestro de autoridades**.

**Acciones:**
1. F0 marcada como **completada** en `REGISTRO/Estado-de-Fases.md` (criterios S1–S7 ✅).
2. Inicio de F1: generación del **registro maestro de autoridades** (16 organismos/registros, AUTH-001…016), del **esquema de datos de la entidad `Source`** y de la **política de sincronización** formalizada.
3. Publicación del catálogo machine-readable `F1-Autoridades.json` (datos estructurados, versionado).

**Pendiente:** Revisión y aprobación de F1 (según sus criterios de aceptación).
## 26-08-2026 — Fase 1 aprobada; inicio de las Fases 2 y 3

**Evento:** El responsable aprueba la **Fase 1 — Inventario maestro de autoridades** (`F1-Registro-de-Autoridades.md` §7). Se desbloquean **F2 — Universo de dispositivos y redes** y **F3 — Inventario de protocolos**.

**Acciones:**
1. F1 marcada como **completada** en `REGISTRO/Estado-de-Fases.md` (criterios F1 ✅).
2. Inicio de F2: taxonomía de **dispositivos** (22 clases con atributos y ejemplos) y de **tipos de red** (16 tipos), con catálogos machine-readable y fichas piloto.
3. Inicio de F3: **inventario maestro de protocolos** (semilla por familias con estados) y **diseño del pipeline de sincronización IANA** (registro oficial como fuente de datos, nunca copia manual), con métricas de cobertura.

**Pendiente:** Revisión y aprobación de F2 y F3 (según sus criterios de aceptación).
## 26-08-2026 — Fases 2 y 3 aprobadas; inicio de las Fases 4 y 5

**Evento:** El responsable aprueba la **Fase 2 — Universo de dispositivos y redes** y la **Fase 3 — Inventario de protocolos** (documentos F2 §7 y F3 §7). Se desbloquean **F4 — Profundización protocolar** y **F5 — Mensajería y PDU**.

**Acciones:**
1. F2 y F3 marcadas como **completadas** en `REGISTRO/Estado-de-Fases.md`. Tareas registradas (no bloqueantes): completar 3+ fichas piloto por clase de dispositivo (F8) y verificación operativa del pipeline IANA (F8).
2. Inicio de F4: plantilla de ficha de protocolo (18 campos), **fichas prioritarias completas** (10 protocolos con fuente primaria) y **matriz de encapsulación/dependencias** machine-readable.
3. Inicio de F5: **unidades de datos por protocolo** (glosario F0), **modelo de campos** (Field), wire format regenerable (layout TCP como referencia), máquinas de estado (TCP/DHCP/BGP) y catálogo `F5-Campos-PDU.json`.

**Pendiente:** Revisión y aprobación de F4 y F5 (según sus criterios de aceptación).
## 26-08-2026 — Fases 4 y 5 aprobadas; inicio de las Fases 6 y 7

**Evento:** El responsable aprueba la **Fase 4 — Profundización protocolar** y la **Fase 5 — Mensajería y PDU** (documentos F4 §6 y F5 §9). Se desbloquean **F6 — Seguridad y operatividad** y **F7 — Dominios profesionales y especiales**.

**Acciones:**
1. F4 y F5 marcadas como **completadas** en `REGISTRO/Estado-de-Fases.md`. Tareas registradas (no bloqueantes): fichas OSPF/Ethernet (F8) y validación de layouts contra capturas (F8).
2. Inicio de F6: modelo de seguridad por protocolo (`SecurityMechanism`), **registro de seguridad de 16 protocolos** y **mapeo a NIST SP 800-207** (R4) con uso complementario de **MITRE ATT&CK** (R5).
3. Inicio de F7: **catálogo de dominios especiales** (OT/ICS, telecom móvil, cloud, data center, IoT, satélite, radio, vehicular, militar público, académico) aplicando la política militar/pública de la F0 (solo material público verificable).

**Pendiente:** Revisión y aprobación de F6 y F7 (según sus criterios de aceptación).
## 26-08-2026 — Fases 6 y 7 aprobadas; inicio de la Fase 8 (Validación)

**Evento:** El responsable aprueba la **Fase 6 — Seguridad y operatividad** y la **Fase 7 — Dominios profesionales y especiales** (documentos F6 §9 y F7 §6). Se inicia la **Fase 8 — Validación** (compuerta de calidad).

**Acciones:**
1. F6 y F7 marcadas como **completadas** en `REGISTRO/Estado-de-Fases.md`.
2. Inicio de F8: ejecución de **verificaciones automatizadas** (parseo JSON, integridad referencial F5/F6/F7→F3, unicidad de IDs, familias/estados, enlaces del repositorio, verificación operativa del registro IANA) y **verificación de IDs de MITRE ATT&CK** contra R5.
3. Cierre de tareas acumuladas: fichas OSPF y Ethernet/802.3 (F4), extensión de fichas piloto de dispositivos (F2), incorporación V2X al inventario F3 v2 (F7), fixture de layouts (F5); registro de lagunas clasificadas.

**Pendiente:** Revisión y aprobación de F8; tras ella, **F9 — Especificación de producto** (que habilita el plan de software de la Fase II).
## 26-08-2026 — Fase 8 aprobada; inicio de la Fase 9 (Especificación de producto)

**Evento:** El responsable aprueba la **Fase 8 — Validación** (`F8-Informe-de-Validacion.md` §9). Compuerta de calidad superada: 11/11 JSON válidos, integridad referencial OK, verificación operativa IANA real (15.401 registros, 7.683 service names), tareas T1–T6 cerradas o clasificadas. Se inicia la **Fase 9 — Especificación de producto**, última fase de investigación.

**Acciones:**
1. F8 marcada como completada en `REGISTRO/Estado-de-Fases.md`.
2. Inicio de F9: especificación de producto (arquitectura de 8 capas, datos SQLite/FTS5, búsqueda, diagramas, capturas, pipeline, distribución, calidad), decisión tecnológica confirmada (Avalonia/.NET; spikes de validación en D0 de la Fase II) y **backlog técnico priorizado** (épicas D0–D7).

**Siguiente:** al aprobar F9, la fase de investigación queda cerrada y se ejecuta la **Fase II** con el prompt maestro de `PLANREDES.md` §17 (plan detallado de arquitectura, UX/UI, implementación, pruebas y distribución).
## 26-08-2026 — Fase 9 aprobada; Fase I cerrada; Fase II en espera

**Evento:** El responsable aprueba la **Fase 9 — Especificación de producto** (`F9-Especificacion-de-Producto.md` §15). Con ello, la **Fase I (investigación y documentación, F0–F9) queda cerrada**.

**Decisión registrada:** La **Fase II — Diseño y generación de software NO se inicia** en este momento; permanece **en espera de instrucción expresa del responsable**. Punto de entrada fijado: prompt maestro de `PLANREDES.md` §17 → épicas D0–D7 de `F9-Backlog.json`.

**Estado del proyecto:** F0–F9 ✅ completadas · 22 documentos de fase · 12 catálogos JSON válidos · registros y bitácora íntegros.
## 26-08-2026 — Inicio de la Fase II (Diseño y generación de software)

**Evento:** El responsable da instrucción de iniciar la **Fase II**. Entrega de los documentos de diseño:
**`F2I-Diseno-de-Software.md`** (arquitectura + ADR-001…005, UX/UI con wireframes, especificación de 10 módulos, plan de pruebas, distribución y mantenimiento, riesgos), **`F2I-Backlog-Detallado.json`** (8 épicas D0–D7, 22 historias con puntos y dependencias) y **`F2I-Entorno-de-Desarrollo.md`**.

**Nota del responsable (registrada):** la **aplicación Visual Studio 2022 está instalada con varios paquetes de herramientas** y podrá ser útil durante el desarrollo. Verificación de entorno realizada: **VS Enterprise 2022 17.14** (workloads ManagedDesktop, NetWeb, Azure, NativeDesktop, Universal, NetCrossPlat, NativeCrossPlat, DataScience, Python; MSVC), **VS Build Tools 2019 16.11**, **.NET SDK 9.0.316** (target net9.0) y **git 2.54.0**.

**Siguiente:** revisión/aprobación del plan de Fase II; tras ella, **épica D0** (spikes de UI y renderer + ADR finales) y luego implementación según backlog.
## 26-08-2026 — Épica D0 completada (spikes y ADR finales)

**Evento:** Se aprueba el plan de Fase II y se ejecuta la **épica D0**. Resultados **reales** (ver `F2I-D0-Spikes-y-ADR.md`):

1. **D0-1 Entorno:** .NET SDK 9.0.316 · plantillas Avalonia instaladas · **Avalonia 12.1.1** (última estable en NuGet). Incidencias resueltas: TFM de plantilla `net10.0`→`net9.0`; paquete `Avalonia.Diagnostics` discontinuado → `AvaloniaUI.DiagnosticsSupport` 2.2.3.
2. **D0-2 Spike UI:** `SpikeUi` compila **0 errores/0 advertencias** (DockPanel + LastChildFill — API `Dock.Fill` eliminada en Avalonia 12 —, ListBox virtualizado 10.000 filas, temas claro/oscuro). Validación visual queda a cargo del responsable (binario SpikeUi).
3. **D0-3 Spike renderer:** `SpikeDiagramas` → **determinismo OK** (SHA256 idéntico en 2 runs: `467B9761…`), SVG de 2.636 bytes exportado desde los campos TCP reales de `F5-Campos-PDU.json`.
4. **D0-4 ADR:** ADR-001 (Avalonia 12.1.1/net9.0), ADR-002 (datos) y ADR-003 (renderer propio determinista → SVG) **confirmados**.

**Siguiente:** D1 — núcleo de dominio C# (17 entidades) + SQLite/FTS5 + serialización, según `F2I-Backlog-Detallado.json`.
## 26-08-2026 — Épica D1 completada (núcleo de dominio y datos)

**Evento:** Se ejecuta la **épica D1** según `F2I-Backlog-Detallado.json`. Resultados **reales** (ver `F2I-D1-Dominio-y-Datos.md`):

1. **D1-1** Modelo de dominio C#: **17 entidades** con URN estable y versionado temporal; validadores de Protocol y Source.
2. **D1-2** Persistencia **SQLite + FTS5**: migraciones versionadas, repositorio CRUD e índice FTS5 (paquete Microsoft.Data.Sqlite 10.0.11).
3. **D1-3** Serialización: JSON round-trip canónico y **YAML round-trip** (YamlDotNet 18.1.0 con `UrnYamlConverter`); importación real de catálogos F3 (113 protocolos) y F5 (11 campos TCP) **sin duplicar datos**.
4. **Pruebas:** **18/18 superadas** (0 errores, 146 ms). Incidencias resueltas: lotes multi-sentencia de Microsoft.Data.Sqlite (parámetros), pooling que bloqueaba archivos temporales, `IReadOnlyList→string[]`, firma nueva de `IYamlTypeConverter` en YamlDotNet 18.

**Siguiente:** **D2 — Pipeline de datos**: importador del CSV real de IANA (15.401 registros), normalización/deduplicación y snapshots con hash/diff/rollback.
## 26-08-2026 — Épica D2 completada (pipeline de datos, Opción A)

**Evento:** El responsable aprueba la **Opción A** para D2. Resultados **reales** (ver `F2I-D2-Pipeline-de-Datos.md`):

1. **Fixture versionado** del registro oficial de IANA descargado el **26-08-2026** (15.402 filas; `FASE-II-DISENO/data/`).
2. **D2-1 Importador IANA:** parser CSV con comillas, validación de cabecera, **deduplicación (nombre, puerto, transporte)** → **13.141 servicios importados** (1.724 rangos sin nombre, 1.946 sin puerto); persistencia **en lote** en SQLite (tabla `Services`, migración v2); regla **puerto ≠ protocolo** mantenida.
3. **D2-3 Snapshots:** manifiesto con **hash agregado determinista**, diff (añadidos/eliminados/cambiados) y **rollback con verificación de integridad** (rechaza manipulación).
4. **D2-2 deduplicación fina:** **diferida** (entidad-linking con F3) por decisión de la Opción A; se refinará con los datos ya cargados.
5. **Pruebas:** **28/28 superadas** (10 nuevas de D2), 0 errores, 274 ms.

**Siguiente:** **D3 — UI básica** (shell + navegación jerárquica + ficha de protocolo de 18 campos + búsqueda/filtros FTS5).
## 26-08-2026 — Épica D3 completada (UI básica — aplicación real)

**Evento:** Se convierte el spike de UI en la aplicación real. Resultados **reales** (ver `F2I-D3-UI-Basica.md`):

1. **`Redes.Knowledge.App`** (Avalonia 12.1.1, XAML): shell DockPanel con barra de búsqueda/filtros/tema, navegación **por familias** (2 clics a ficha), **ficha de 18 campos** con marcadores honestos `[pendiente]`/`[n.p.d.]`, búsqueda y filtros **FTS5** combinables.
2. **Bootstrap idempotente:** importa F3 (113 protocolos) a `FASE-II-DISENO/run/knowledge.db` solo si está vacío; **13.141 servicios IANA** disponibles vía `Services`.
3. **Hallazgo real (Avalonia 12):** `TextBox.Watermark` obsoleto → **`PlaceholderText`** (build final 0/0).
4. **Pruebas:** **32/32 superadas** (4 nuevas de D3: bootstrap idempotente, búsqueda sobre catálogo real, filtros familia/estado, estado `military_public` tolerable).

**Pendiente (manual, no bloqueante):** validación visual de la UI por el responsable (`dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`).

**Siguiente:** **D4 — diagramas y visualización** (renderer determinista D0-3 → plantillas pila/estado/mensaje/e2e + exportación SVG/PNG/PDF).
## 26-08-2026 — Épica D4 completada (diagramas y visualización)

**Evento:** Se crea el módulo `Redes.Knowledge.Visualization` (sin dependencias externas). Resultados **reales** (ver `F2I-D4-Diagramas-y-Visualizacion.md`):

1. **D4-1** Modelo de diagrama **desacoplado** (`DiagramDocument` + primitivas) con layouts deterministas (ADR-003).
2. **D4-2** **5 plantillas** del plan §11: wire-format bit/byte (datos reales F5/TCP), pila/encapsulación, secuencia (DHCP DORA), máquina de estados (TCP) y ruta e2e con PDU por enlace.
3. **D4-3** Exportación **SVG** (canónico) y **PDF 1.4 mínimo válido y determinista** (sin dependencias; xref con offsets exactos). **PNG** registrado como tarea de integración (D5, rasterización en la app).
4. **Pruebas:** **39/39 superadas** (7 nuevas), incl. determinismo en todas las plantillas y validez estructural del PDF.

**Siguiente:** **D5 — Exploración avanzada** (grafo de relaciones, comparador y fichas detalladas; integración de la vista de diagramas y rasterización PNG).
## 26-08-2026 — Épica D5 completada (exploración avanzada)

**Evento:** Se implementa la exploración avanzada sobre los catálogos reales de la Fase I. Resultados **reales** (ver `F2I-D5-Exploracion-Avanzada.md`):

1. **D5-1 Grafo:** matriz de encapsulación **F4** cargada (20+ relaciones) y **vecinos a 1 salto** (`HTTP/3 → quic`); layout `Layouts.Grafo` (estrella determinista).
2. **D5-2 Comparador:** `ProtocoloComparador` función pura (familia/estado, **PDU F5**, **puertos IANA**, **cifrado F6**); botón "Comparar vs TCP" en la app. BGP → `179/TCP`.
3. **D5-3 Fichas detalladas:** loaders reales (22 dispositivos, 16 redes, campos F5, PDU, seguridad F6); campos y vecinos visibles en la ficha.
4. **Pruebas:** **45/45 superadas** (6 nuevas) · app **0 errores / 0 advertencias**. Incidencias resueltas: URN del grafo normalizadas (expectativas ajustadas), tipo `IReadOnlyList<Field>` en la app.

**Siguiente:** **D6 — Capturas y validación de layouts** (adaptador PCAP/PCAPNG + dissection y correspondencia con F5 → **cierra L-004**).
## 26-08-2026 — Épica D6 completada (capturas y validación de layouts)

**Evento:** Adaptador de capturas **sin dependencias externas** y cierre de la **laguna L-004** de la Fase I. Resultados **reales** (ver `F2I-D6-Capturas-y-Validacion.md`):

1. **D6-1** Lector **PCAP clásico** (ambos endianness) y **PCAPNG** (SHB/IDB/EPB con detección de endianness; bloques desconocidos omitidos); **dissection por capas** Ethernet→IPv4→TCP (IPs y puertos).
2. **D6-2** **Validación de layouts contra `F5-Campos-PDU.json`**: TCP 10/10, IPv4 13/13, Ethernet con semántica de preámbulo (base 64; campos fuera de captura marcados honestamente).
3. Fixtures de captura **generados programáticamente** (paquete 192.0.2.1→203.0.113.2, 49152→80, SYN|ACK) en ambos formatos.
4. **Pruebas:** **52/52 superadas** (7 nuevas). Incidencias resueltas: sobrecarga `Assert.Equal(int, ushort?)`, **BOM del PCAPNG en orden LE** (bytes `4D 3C 2B 1A`).

**Siguiente:** **D7 — Calidad, distribución y Release 1.0** (CI/CD, instaladores por SO, offline y actualización de dataset; vista de captura y PNG en la app).
## 26-08-2026 — Épica D7 completada — Fase II (iteración inicial) cerrada

**Evento:** Última épica de la Fase II. Resultados **reales** (ver `F2I-D7-Calidad-Distribucion-Release.md`):

1. **D7-1 Calidad/CI:** auditoría automática **A01–A07** (incl. **golden-master** determinista) probada sobre datos reales; plantilla **GitHub Actions** (quality/build/package) y README de activación.
2. **D7-2 Distribución:** **publicaciones self-contained reales** win-x64 (231 f.) · linux-x64 (228) · osx-x64 (229); plantilla **Inno Setup**; **dataset versionado** independiente del ejecutable (`DatasetMetadata`).
3. **D7-3 Offline/Release:** bootstrap embebido (offline total); **Release candidato 1.0.0** en `dist/` con metadatos.
4. **Pruebas:** **56/56 en Release** (4 nuevas de D7).

**Estado de la Fase II:** **D0–D7 completadas** (26-08-2026). Solución `Redes.Knowledge.sln` (App Avalonia 12.1.1 · Domain · Infrastructure · Visualization · Tests). **Pendientes no bloqueantes:** activar repositorio git + CI, instaladores oficiales firmados (`v1.0.0`), pulido **1.1** (PNG, vista de captura, panel del grafo, dedup fina D2-2). Fases I y II del plan maestro, completadas en su entrega inicial.
## 26-08-2026 — Ajustes de UI solicitados por el responsable (sidebar + zoom)

**Solicitud:** (1) los desplegables de la izquierda con tamaños distintos → **sidebar lateral unificada con paneles plegables**; (2) **tamaño de letra ajustable con Ctrl+Scroll** en toda la interfaz.

**Cambios aplicados** (`Redes.Knowledge.App`, compilación **0 errores / 0 advertencias**):

1. **Sidebar unificada:** borde fijo de 340 px con separador; paneles (Expander) con **altura uniforme** (240 px), márgenes/padding consistentes y comportamiento **acordeón** (solo uno abierto a la vez).
2. **Zoom global Ctrl+Scroll:** manejador global (burbuja+túnel, `handledEventsToo`), rango **70–250 %**; se aplica `ScaleTransform` al contenido de la ventana (origen arriba-izquierda) y el porcentaje se muestra en la barra de estado.
   - *Nota técnica:* Avalonia 12 no expone `LayoutTransform`; el zoom actual es de **render** (escala visual). Si se desea que el texto **refluya** con el zoom (escala de layout), se refina en la iteración 1.1 con un `LayoutTransformControl`/fuente global.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App` (o el ejecutable de `dist\win-x64`).
## 26-08-2026 — Sidebar definitiva (filtros dentro de la sidebar + colapso)

**Seguimiento de la solicitud de UI:** el responsable confirmó que el zoom funciona; los dropdowns debían vivir **dentro de la sidebar**. Reestructura aplicada (`MainWindow.axaml`, build **0/0**):

- **Sidebar explícita y colapsable** (340 px, borde separador): contiene (1) **búsqueda**, (2) los **dropdowns de filtros Familia y Estado** (ancho completo y uniforme), (3) separador y (4) la **navegación por familias** en acordeón de altura uniforme.
- **Botón colapsable** en la barra superior: "⬅ Ocultar sidebar" / "➡ Mostrar sidebar".
- La barra superior queda solo con: sidebar toggle · tema · comparador.

**Relanzar:** cerrar la instancia anterior y `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Sidebar corregida: grupos de familias uniformes (interpretación confirmada)

**Corrección de la solicitud de UI:** los "dropdowns de los filtros" eran los **grupos de familias** (ACEL (10), ADCONF (9)…), no los desplegables Familia/Estado. Se deshizo el cambio anterior (Búsqueda/Familia/Estado vuelven a la barra superior) y la sidebar queda así:

- **Barra superior:** búsqueda global, filtros **Familia** y **Estado** (ComboBox), tema, comparador.
- **Sidebar (340 px):** **solo los grupos de familias**, cada panel con **anchura idéntica garantizada** (306 px fijos) y **altura uniforme** (240 px) en acordeón; **filtro rápido** ("Filtrar familias…") para localizar familias/protocolos sin desplegar.
- Ctrl+Scroll (zoom) intacto.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Diagnóstico y corrección: "pendiente de pipeline" y relaciones invisibles

**Consulta del responsable:** "F4/F5/F6, ¿qué significan? La app parece disfuncional: no salen relaciones catalogadas y casi todo indica pendiente de pipeline."

**Aclaración:** F4/F5/F6 son **identificadores de fase del dataset** (F4-Matriz-Encapsulacion.json, F5-Campos-PDU.json, F6-Seguridad-Protocolos.json), no teclas de función ni atajos: la app los usa como etiquetas de procedencia del dato (`[F4]`, `[F5]`, `[F6]`).

**Causas reales encontradas (no había error de ejecución):**
1. La ficha de 18 campos mostraba varios bloques con el literal `[pendiente de pipeline]` (texto fijo del código): daba la impresión de que la app estaba rota cuando en realidad esos campos no tienen aún datos estructurados.
2. **Emparejamiento F4↔F3**: la matriz F4 nombra la entidad "Ethernet (802.3)", pero F3 la cataloga como `ETH`; la normalización producía `ethernet8023` ≠ `eth`, así que **las relaciones de Ethernet nunca aparecían** (IPv4/IPv6/ARP → ETH invisibles).
3. La ficha inicial elegía el primer protocolo por acrónimo (`5G NR`, sin relaciones F4): al abrir la app "no salían relaciones".
4. Los vecinos del grafo se mostraban con la clave normalizada (`ipv4`, `quic`) en lugar del nombre legible.

**Correcciones aplicadas (build 0/0, tests 58/58 ✅):**
- `CatalogoExploracion`: alias `Ethernet (802.3)` → `ETH` al cargar relaciones F4; el grafo ahora resuelve ETH (vecinos IPv4/IPv6/ARP).
- `CatalogJson`: nuevo `CargarNotasFuenteF3` (nota y fuente reales del catálogo, sin duplicar datos).
- `MainWindow`: (a) carga de notas/fuentes F3; (b) la ficha inicial **selecciona TCP** (primer protocolo con relaciones) o el de más vecinos; (c) `RenderFicha` muestra datos reales (estado + fecha catálogo, descripción F3, encapsulación F4, PDU F5, seguridad F6, puertos IANA) e indica `[n.p.d.]` solo donde no hay dato estructurado (fuente primaria: pipeline R1-R11 pendiente, plano funcional, vínculo, ficha completa D4 en iteración 1.1); (d) vecinos legibles "ACR · Nombre (tipo)".
- Tests nuevos: `Grafo_Alias_F4_Resuelve_ETH` y `Notas_Y_Fuentes_F3_Disponibles`.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Ficha sin ruido [n.p.d.] + diagramas de arquitectura integrados

**Feedback del responsable:** "Demasiados campos [n.p.d.] en casi todos los protocolos; en NetBIOS aparece 'pipeline R1-R11 pendiente'; la app no muestra diagramas gráficos de la arquitectura de cada protocolo."

**Causas:** (1) `RenderFicha` imprimía todas las líneas numeradas aunque no hubiera dato; (2) el estado mostraba el texto interno del pipeline como si fuera un dato; (3) la librería `Redes.Knowledge.Visualization` (Layouts: pila/grafo/wire format) existía y estaba testeada, pero la App **no la referenciaba ni la dibujaba**.

**Correcciones (build 0/0, tests 60/60 ✅):**
1. **Ficha de 18 campos reales:** nuevo `F4-Fichas-Prioritarias.json` **derivado** de `F4-Fichas-Prioritarias.md` vía `generar-fichas-json.ps1` (regenerable, 12 fichas × 18 campos). La ficha de un protocolo con ficha F4 muestra **los 18 campos completos** (finalidad, mensajes, secuencia, addressing, routing, QoS, observabilidad, interoperabilidad, implementaciones, fuentes con RFC); el resto se completa con F3/F5/F6/IANA. **Solo se imprime una línea si hay dato**; lo que falta se resume en una ÚNICA línea final de pendientes (épica D4/iteración 1.1) — se acabó el spam `[n.p.d.]` por línea.
2. **Sin "pipeline R1-R11" en las fichas:** ese texto interno desaparece; las fuentes reales (RFC nivel 1) vienen del campo 18 de la ficha F4 o del campo `fuente` de F3.
3. **Diagramas de arquitectura por protocolo** (integración real de `Redes.Knowledge.Visualization`):
   - **Pila de encapsulación** (F4): cadena "X corre sobre Y…" resuelta desde el protocolo hacia el medio (p. ej. TCP → IPv4 → ETH → Cobre/Fibra).
   - **Grafo de vecinos a 1 salto** (F4) para protocolos con relaciones.
   - **Wire format de la cabecera** (F5) para los protocolos con campos catalogados (TCP/UDP/IPv4/IPv6/ETH/DNS).
   - Nuevo control `DiagramView` (DrawingContext, ADR-003: el modelo no conoce el renderer). Panel bajo la ficha, escalable con el zoom Ctrl+Scroll.
4. **Puertos IANA con prefijo:** `PorNombre` usaba coincidencia exacta; ahora `LIKE prefijo%` → NetBIOS muestra 137–139 (netbios-ns/dgm/ssn), HTTP 80, SSH 22, etc.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Texto seleccionable, pendientes fuera de la UI y diagramas a color

**Feedback del responsable:** (1) NetBIOS mostraba al final "Pendiente de integración (épica D4/iteración 1.1)"; (2) el texto debe ser seleccionable en toda la aplicación; (3) el título "Pila de encapsulación (F4)" apenas se veía (azul oscuro fijo); (4) los diagramas "solo muestran recuadros con texto", sin color funcional.

**Correcciones (build 0/0, tests 60/60 ✅):**
1. **Pendientes fuera de la UI:** eliminada la línea "Pendiente de integración (épica…)" de la ficha; las lagunas se siguen registrando en `REGISTRO/Bitacora.md` y `F2I-Backlog-Detallado.json`, no en cada ficha. La ficha solo imprime líneas con dato (campos sin dato simplemente no se muestran).
2. **Texto seleccionable:** `DetailText`, `StatusText`, `DiagramTitle`, etiquetas `Familia:`/`Estado:` y títulos de los diagramas convertidos a `SelectableTextBlock` (Avalonia no permite selección en `TextBlock`).
3. **Títulos legibles:** los títulos de los diagramas heredan el color del tema (se eliminó el `Foreground` fijo azul oscuro `#334155`).
4. **Diagramas a color reales:** `DiagramView` ignoraba el `Fill/Stroke` de cada primitiva y pintaba todo con un único color. Ahora respeta los colores del modelo y aplica paleta por tipo: **pila** con paleta por capa (ámbar→verde→azul→violeta…), **grafo** con la semilla amarilla del layout y vecinos en azul claro + **aristas coloreadas por tipo de relación** (corre sobre=verde, encapsula=azul, depende de=naranja, sustituye a=rojo…), **wire format** en azul claro. Fondo blanco fijo del lienzo para legibilidad en tema claro y oscuro.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Wire format: etiquetas dentro de sus casillas

**Feedback del responsable:** "Todos los campos del wire format salen fuera de sus respectivas casillas."

**Causa:** en `Layouts.WireFormat` la etiqueta del campo se dibujaba desde `x+3` **sin límite de ancho**; en casillas estrechas (campos de 4–8 bits ≈ 80–160 px) el texto `Version (0-4)` se desbordaba sobre las casillas vecinas.

**Corrección (build 0/0, tests 60/60 ✅):**
1. `Layouts.EtiquetaCampo`: la etiqueta se **ajusta al ancho de la casilla** — texto completo `Nombre (a-b)` si cabe; si no, solo `Nombre`; si tampoco, nombre truncado con "…". Estimación determinista (~6,5 px/carácter a 13px): mismo input → mismo diagrama.
2. `DiagramView`: red de seguridad — si el layout fija un ancho máximo (`W>0`), el texto se **mide y se trunca con "…"** para no desbordar la caja.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Grafo de vecinos: sin texto superpuesto en aristas

**Feedback del responsable:** "En el grafo de vecinos a un salto sobran las indicaciones 'corre sobre' en cada arista, porque se superponen con el texto y queda feo y disfuncional."

**Causa:** `Layouts.Grafo` dibujaba el tipo de relación en el punto medio de cada arista; en el grafo estrella las líneas pasan por el centro (donde está la semilla) y los textos se superponían entre sí y con los nodos.

**Corrección (build 0/0, tests 60/60 ✅):**
1. `Layouts.Grafo` nuevo parámetro `mostrarEtiquetasAristas` (por defecto `true` para no romper SVG/PDF): cuando es `false` **no se dibuja texto en las aristas**, pero la línea conserva su etiqueta para que el renderer la coloree por tipo.
2. La app llama el grafo con `mostrarEtiquetasAristas: false`.
3. **Leyenda al pie del diagrama**: cada tipo de relación presente se muestra como una caja de color + su nombre (corre sobre=verde, encapsula=azul, depende de=naranja…), sin solaparse con los nodos (el lienzo del grafo creció a 600×400). El color de las aristas sigue comunicando el tipo.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Pila sin texto redundante y wire format con campos en sus casillas

**Feedback del responsable:** (1) aplicar a la pila de encapsulación el mismo tratamiento que al grafo (quitar texto redundante en aristas y en la parte superior); (2) el wire format tenía el texto de todos sus campos desplazado de sus casillas.

**Causas:**
1. `Layouts.Pila` dibujaba la etiqueta "encapsulación" sobre cada línea de conexión entre capas y un título interno que duplicaba el del panel → texto redundante.
2. `Layouts.WireFormat` colocaba el texto del campo en `y + RowH/2 + 2` interpretado como esquina superior; la casilla mide 30 px, así que el texto de 13 px quedaba ~10 px desplazado hacia abajo y se salía de su casilla (además el `DiagramView` sí lo dibuja top-left, mientras el `SvgRenderer` lo trata como baseline de SVG).

**Corrección (build 0/0, tests 60/60 ✅):**
1. **Pila:** nuevos parámetros `mostrarTitulo` y `mostrarEtiquetasEnlace` (por defecto `true`, para no alterar SVG/PDF); la app los usa en `false` → solo las cajas de capas con sus líneas de conexión (el título de la sección ya lo pone el panel). El texto de capa ahora está **centrado verticalmente** en su caja.
2. **Wire format:** el texto del campo se dibuja **centrado verticalmente dentro de la casilla** (`y + (RowH - 4 - 13) / 2`, top-left del texto) y el número de fila se centra respecto a su fila; se conserva el ajuste de ancho (texto completo / nombre / nombre truncado con "…").

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Comparador con TCP: sin diagramas y con tabla rica por aspecto

**Feedback del responsable:** (1) el comparador con TCP mostraba los diagramas individuales del protocolo a comparar (no debería mostrar ninguno); (2) indicaba "(datos F3/F5/F6/IANA)" pero la comparación no mostraba nada interesante (la mayoría de columnas salían "—").

**Causas:** `CompararConTcp` solo cambiaba el texto del detalle, **no limpiaba `DiagramPanel`** → quedaban los diagramas del último protocolo; y la tabla solo comparaba Familia/Estado/PDU/Puertos/Cifrado (datos vacíos para la mayoría).

**Corrección (build 0/0, tests 61/61 ✅):**
1. **Sin diagramas en la comparación:** `CompararConTcp` limpia `DiagramPanel` y oculta `DiagramTitle`.
2. **Comparador enriquecido** (`ProtocoloComparador` + parámetros opcionales `fichas` y `relaciones`): nuevas columnas **Capas** (F3 o campo 5 de la ficha F4), **Finalidad** (ficha F4) y **Encapsulación** (ficha F4 o grafo de relaciones). Retrocompatible: los tests antiguos siguen pasando con valores por defecto.
3. **Tabla transpuesta legible:** filas = aspectos (Familia, Estado, Capas, Finalidad, Encapsulación, PDU, Puertos IANA, Cifrado F6), columnas = los dos protocolos; marca `(= igual)` cuando coinciden y acorta textos largos; pie con la procedencia de cada dato.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Tabla de comparación: ancho fijo, sin desbordes

**Feedback del responsable:** "Los datos correspondientes a las tablas de comparación sobresalen y descolocan toda la pantalla. Debes ajustar la justificación para que no sobresalga."

**Causa:** las celdas de la tabla transpuesta usaban padding de 44 caracteres y valores de hasta ~160 caracteres → las líneas superaban el ancho de la ventana; con `TextWrapping` el texto se rompía a mitad del relleno y descuadraba la alineación.

**Corrección (build 0/0, tests 61/61 ✅):**
1. **Celdas de ancho fijo (36 caracteres)** con truncado y "…": la tabla nunca desborda la ventana.
2. **Separación por tipo:** los aspectos cortos (Familia, Estado, Capas, PDU, Puertos, Cifrado) van en la tabla; los largos (**Finalidad** y **Encapsulación**) salen de la tabla y se muestran por protocolo en líneas envueltas (`Texto — Protocolo: …`), seleccionables y con salto de línea natural.
3. Marcador `(= igual)` solo cuando el valor coincide y no es "—".

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Ampliación de cobertura del dataset (iteración 1.1, lote 1)

**Feedback del responsable:** "La mayoría de los protocolos solo indican Identidad, Estado y Capas. eso es muy poca información, ¿Está bien?" → se confirma que era el estado planificado de la Fase I (solo 12 fichas prioritarias); el responsable aprueba **ampliar fichas F4, campos F5, seguridad F6 y grafo F4**.

**Ampliación aplicada (build 0/0, tests 61/61 ✅):**
- **Fichas F4: 12 → 17** (nuevas: SSH, SMTP, FTP, SNMP, NTP; 18 campos cada una con RFC nivel 1) — JSON regenerado con `generar-fichas-json.ps1` (regenerable).
- **Campos de PDU (F5): 6 → 12** (nuevos: ICMP, ICMPv6, DHCP, TLS record, NTP, BGP; offsets en bits reales).
- **Seguridad (F6): 15 → 21** (nuevos: HTTP/1.1, HTTP/2, SMTP, FTP, SNMP, NTP).
- **Grafo F4: 14 → 21 protocolos, 20 → 29 relaciones** (SSH/SMTP/FTP/HTTP→TCP, SNMP/NTP→UDP, ICMP/ICMPv6 encap, etc.).
- **App:** `MainWindow` ahora carga campos F5 para **cualquier** protocolo del catálogo que los tenga, no solo los 6 iniciales → wire format y PDU aparecen en más fichas.

**Nota honesta:** la ampliación es por lotes; quedan ~80 protocolos con solo inventario F3 hasta su ficha (pendiente: SNMP/SSH/etc. ya cubiertos; resto en lotes posteriores con fuentes verificables). El `Estado-de-Fases.md` y el backlog registran el lote 1.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Ampliación de cobertura (iteración 1.1, lote 2)

**Continuación de la ampliación aprobada** (tras el lote 1: SSH, SMTP, FTP, SNMP, NTP):

- **Fichas F4: 17 → 29** (nuevas: HTTP/1.1, HTTP/2, RIP, IS-IS, SCTP, DTLS, IPsec/ESP, IKE, Kerberos, RADIUS, MQTT, CoAP; 18 campos con RFC nivel 1). JSON regenerado con `generar-fichas-json.ps1` (corregido el parser para "IS-IS — …" y alias "IPsec (ESP)"→"IPsec").
- **Campos de PDU (F5): 12 → 17** (nuevos: RIP, SCTP, HTTP/2 frame, MQTT, CoAP; offsets reales).
- **Seguridad (F6): 21 → 32** (nuevos: DTLS, RIP, ICMPv6, SCTP, RADIUS, OSPF, QUIC, MQTT, CoAP, PPP, PIM).
- **Grafo F4: 21 → 30 protocolos, 29 → 44 relaciones** (HTTP→TLS, RIP→UDP, IS-IS→Ethernet, SCTP→IPv4/IPv6, DTLS→UDP, ESP→IP, IKE→UDP, Kerberos→UDP/TCP, RADIUS→UDP, MQTT→TCP, CoAP→UDP…). Alias nuevo "IPsec (ESP)"→"IPsec" en `CatalogoExploracion`.

**Cobertura total (113): fichas 29 · PDU 17 · seguridad 32 · grafo 30.** Quedan ~75 con solo inventario F3 (lotes posteriores). Build 0/0 · tests 61/61 ✅.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Ampliación de cobertura (iteración 1.1, lote 3)

**Continuación de la ampliación aprobada:**

- **Fichas F4: 29 → 45** (nuevas: QUIC, RTP, RTCP, SIP, XMPP, NFS, SMB, iSCSI, MPLS, GRE, VXLAN, WireGuard, GTP, Modbus, DNP3, OPC UA; 18 campos con RFC/norma nivel 1). JSON regenerado con `generar-fichas-json.ps1`.
- **Campos de PDU (F5): 17 → 23** (nuevos: QUIC, RTP, MPLS shim, VXLAN, GRE, GTP; offsets reales).
- **Seguridad (F6): 32 → 45** (nuevos: RTP/SRTP, SIP, XMPP, NFS, SMB, iSCSI, MPLS, GRE, VXLAN, GTP, Modbus, DNP3, OPC UA).
- **Grafo F4: 30 → 47 protocolos, 44 → 61 relaciones** (RTP/RTCP/SIP→UDP, XMPP/NFS/SMB/iSCSI/Modbus/DNP3/OPC UA→TCP, GRE→IPv4/IPv6, VXLAN/WireGuard/GTP→UDP, MPLS→Ethernet…).

**Cobertura total (113): fichas 45 · PDU 23 · seguridad 45 · grafo 47.** Quedan ~55 con solo inventario F3 (lotes posteriores por familias). Build 0/0 · tests 61/61 ✅.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Ampliación de cobertura (iteración 1.1, lote 4)

**Continuación de la ampliación aprobada:**

- **Fichas F4: 45 → 69** (nuevas: WIFI, PPP, 802.1Q, STP, RSTP, MSTP, LACP, NDP, mDNS, IGMP, PIM, VRRP, OSPFv3, DCCP, Telnet, NETCONF, Syslog, TACACS+, PTP, FC, EAP, DNSSEC, PROFINET, DVB-S2; 18 campos con RFC/norma nivel 1). JSON regenerado con `generar-fichas-json.ps1`.
- **Campos de PDU (F5): 23 → 28** (nuevos: IGMP, VRRP, STP BPDU, Syslog, Telnet).
- **Seguridad (F6): 45 → 61** (nuevos: WIFI/WPA3, 802.1Q, STP, NDP, mDNS, IS-IS, OSPFv3, Telnet, NETCONF, Syslog, TACACS+, PTP, FC, EAP, PROFINET, DVB-S2).
- **Grafo F4: 47 → 67 protocolos, 61 → 82 relaciones** (802.1Q/STP/RSTP/MSTP/LACP/PROFINET→Ethernet, NDP/OSPFv3→IPv6, IGMP/PIM/VRRP/DCCP→IPv4, mDNS/Syslog/PTP→UDP, Telnet/TACACS+→TCP, NETCONF→SSH, EAP→802.1X, DNSSEC→DNS…).

**Cobertura total (113): fichas 69 · PDU 28 · seguridad 61 · grafo 67.** Quedan ~25 con solo inventario F3: móviles (GSM/UMTS/LTE/5G NR/TETRA/DMR/Link16/Link11), históricos (X.25, FR, ATM, Token Ring, FDDI, NetBEUI, IPX/SPX, AppleTalk, ARCNET, SONET/SDH, ISDN), IoT restantes (EtherCAT, LoRaWAN, Zigbee, BACnet, NVMe-oF/FCoE), gest (RESTCONF, gRPC, IPFIX, NetFlow), SEG (802.1X, GRE ya), MOV (MIP/MIPv6/LISP), SR/MPLS, y vehiculares ITS-G5/C-V2X. Build 0/0 · tests 61/61 ✅.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Ampliación de cobertura (iteración 1.1, lote 5) — catálogo completo al 100 %

**Lote final de la ampliación aprobada** (69 → **113 fichas**: L2TP, CSMA/CD, LLMNR, NetBIOS, RIPv2, SR, MIP, MIPv6, LISP, RESTCONF, gRPC, IPFIX, NetFlow, ICMP, ICMPv6, FCoE, NVMe-oF, 802.1X, EtherCAT, LoRaWAN, Zigbee, BACnet, GSM, UMTS, LTE, 5G NR, TETRA, DMR, Link 16, Link 11, ITS-G5, C-V2X, X.25, FR, ATM, Token Ring, FDDI, NetBEUI, IPX/SPX, AppleTalk, ARCNET, SONET/SDH, ISDN, EIGRP).

- **Fichas F4: 69 → 113 (100 % del catálogo F3)**, todas con los 18 campos y fuentes (RFC/norma nivel 1; propietarios/militares marcados como tales). JSON regenerado con `generar-fichas-json.ps1`.
- Test de fichas endurece: exige **113 fichas** y que **toda** ficha tenga campo 18.

**Cobertura final del dataset (113 protocolos): fichas F4 113/113 · PDU F5 28 · seguridad F6 61 · grafo F4 67.** Build 0/0 · tests 61/61 ✅.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Leyenda de familias, verificación ATT&CK (L-003) y git+CI

**Petición del responsable:** (1) sección "Leyenda" que explique escuetamente cada familia de protocolos; (2) verificar IDs ATT&CK (L-003); (3) activar git + CI.

**1. Leyenda de familias (app):** botón **"Leyenda"** en la barra superior; muestra las 13 familias con su descripción (del campo `familias` de F3, sin duplicar datos) y 5 ejemplos de acrónimos de cada una. Cierra el "no sé a qué categoría pertenece" sin memorizar abreviaturas.

**2. IDs ATT&CK (L-003 → cerrado):** el servicio de búsqueda web sigue sin saldo, por lo que se aplicó el método documentado en la propia laguna (conocimiento experto de la taxonomía, confianza ALTA). Auditoría de todos los IDs presentes en F6:
- Correctos: T1046, T1071.001/.003/.004, T1090.001, T1110, T1557.002, T1558, T1021.002, T0866.
- **Corregidos 4 erróneos en F6:** RADIUS `T1021.006` (era WinRM) → **T1110**; NTP `T1070.008` (era borrado de buzón) → **T1557**; RTP `T1573.002` (era cifrado del adversario) → **T1040/T1557**; RIP `T1553` retirado (no aplica a inyección de rutas).
- T1078/T1040 confirmados por taxonomía aunque no se usan en F6. Actualización de `F8-Lagunas.json` (L-003: `pendiente` → `cerrado`; confirmación online formal queda como mantenimiento de pipeline).

**3. git + CI activados:**
- `git init` (rama `main`), `.gitignore` (excluye bin/obj, dist/, run/, config del entorno), **commit inicial `0b94429`** (126 archivos, working tree limpio).
- CI movido de `ci/github-actions-ci.yml` a **`.github/workflows/github-actions-ci.yml`**; job `package-win` ahora instala Inno Setup (choco) para no fallar en el runner. Pipeline: `quality` (tests Release 61/61) + `build` (win-x64/linux-x64/osx-x64 self-contained) + `package-win` solo en tags `v*`.
- No hay remoto GitHub configurado (no se pidió): para activar, `git remote add origin <url>` + `git push -u origin main`.

**Build 0/0 · tests 61/61 ✅**

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Repositorio publicado en GitHub (privado)

**Petición del responsable:** "hay GitHub CLI instalado, hazlo desde línea de comando."

**Hecho tras reautenticación (cuenta `kraso`, token con scopes `repo` + `workflow`):**
- `gh repo create redes-knowledge --private --source . --remote origin --push`
- Repositorio: **https://github.com/kraso/redes-knowledge** (privado, rama por defecto `main`).
- `origin` configurado (fetch/push HTTPS); `main` trackea `origin/main`; 2 commits publicados.

**CI:** los jobs de Actions (`quality`: 61 tests; `build`: win/linux/osx; `package-win` en tags `v*`) quedan activos automáticamente al publicar. (Pendiente no bloqueante: pasar el repo a público cuando se decida.)

### CI verificado y en verde ✅ (26-08-2026)

- Primer run falló por un **bug real de portabilidad que detectó CI**: `ExploracionTests.Grafo_Carga_Relaciones_Reales` construía la ruta con separador manual (`@"\"`), válido en Windows pero roto en Linux (`FileNotFoundException`). Corregido al helper portable `R(...)` (la suite local de Windows no lo veía; el runner ubuntu sí).
- Añadido trigger `workflow_dispatch` (el evento `push` no disparó en el repo recién creado; dispatch permite ejecución manual de diagnóstico).
- **Run `32987043202` → success**: quality 61/61 + build win-x64 + linux-x64 + osx-x64 (self-contained); `package-win` skippeado (sin tag `v*`).
- Trabajo de CI completo; el push de la rama `main` debería seguir disparando runs, y tags `v*` activan el instalador Inno.

---
Última actualización: 26-08-2026
## 26-08-2026 — Tipografía global: JetBrainsMono Nerd Font Mono (SemiBold)

**Petición del responsable:** cambiar la tipografía de toda la UI/UX a `JetBrainsMonoNerdFontMono-SemiBold.ttf` (fuente localizada en `F:\Fuentes Tipográficas\Jetbrains Mono\JetBrainsMonoNerdFonts`).

**Implementación (portátil, no depende del sistema):**
1. TTF copiado a `src/Redes.Knowledge.App/Assets/Fonts/JetBrainsMonoNerdFontMono-SemiBold.ttf` e incluido como `AvaloniaResource` (csproj) → queda **embebido en el binario** (App.dll ~2,3 MB), por lo que los builds de CI en Linux/macOS también la usan.
2. `App.axaml`: recurso global `FuenteUi` apuntando a `avares://…#JetBrainsMono NFM` (nombre real de familia del TTF, verificado en el registro de Windows).
3. `MainWindow.axaml`: `FontFamily="{DynamicResource FuenteUi}"` en la ventana raíz → **herencia global** a todos los controles (botones, combos, listas, texto).
4. `DiagramView.cs`: los diagramas (pila/grafo/wire format) usan la misma familia vía `Typeface` embebida (antes `Typeface.Default`). Se eliminó el `FontFamily="Consolas, monospace"` explícito de la ficha.

**Notas:** se descartó el selector `:root` (AVLN2200/AVLN3000 en Avalonia 12) y `x:Shared` (no soportado); la herencia va desde el `Window`. Build 0/0 · tests 61/61 ✅.
## 26-08-2026 — ComboBox ampliados (Todas las familias / Todos los estados)

**Feedback del responsable:** las casillas "Todas las familias" / "Todos los estados" (menús desplegables) eran demasiado estrechas y no se veía el texto completo con la nueva fuente monospaciada.

**Corrección (build 0/0):** `FilterFamilia` y `FilterEstado` pasaron de **150 → 210 px**; se rebalanceó la barra superior para que todo siga cabiendo (búsqueda 300 → 170 px); se mantienen las etiquetas "Familia:"/"Estado:".
## 26-08-2026 — Renombrado a "Net Protocol", barra de título gradiente, logo y desplegables colapsados

**Peticiones del responsable:**
1. Iniciar con **todos los desplegables colapsados** (antes el primero se abría).
2. Ajuste de línea / ventana suficiente para que ningún texto quede oculto.
3. **Renombrar la app a "Net Protocol"**.
4. **Barra de título con gradiente negro→blanco horizontal, cristalizado** (transparencia parcial), predominio del negro, **permanente** entre temas claro/oscuro.
5. **Logo** `data/Logo_NetProtocol.png` como icono de aplicación.

**Implementación (build 0/0 · tests 61/61 ✅):**
1. `MainWindow.axaml.cs`: `IsExpanded = false` en la navegación → todos los grupos de familias inician colapsados.
2. Ventana **1400×820** (anterior 1200×760) y sidebar 360 px → el texto de la barra superior y los filtros quedan completos; `StatusText` y ficha ya usan `TextWrapping`.
3. Renombrado completo: `AssemblyName=NetProtocol` → exe **NetProtocol.exe**; `app.manifest` → `NetProtocol`; `Title="Net Protocol"`; instalador Inno → AppName/DefaultDirName/grupo/`OutputBaseFilename=NetProtocol-Setup-1.0.0` + `SetupIconFile=NetProtocol.ico`; `avares://NetProtocol/…` en App.axaml y DiagramView.
4. **Barra de título propia** (44 px) con `LinearGradientBrush` horizontal negro→blanco (offsets 0,0.55,0.80 negro; 0.97/1.0 blanco; alphas F2/E6/D9/B3 → **cristalizado**, colores fijos → **no cambia con el tema**). Ventana con `ExtendClientAreaToDecorationsHint="True"` + `ExtendClientAreaTitleBarHeightHint="44"` (API de Avalonia 12; `ExtendClientAreaChromeHints` ya no existe). Logo en la barra (26×26) + título "Net Protocol".
5. **Logo**: PNG original 1254×1254 (2 MB) derivado a **256×256** (`Assets/Logo_NetProtocol.png`) + `.ico` (41 KB) para el ejecutable (`ApplicationIcon`); icono de ventana/taskbar cargado vía `AssetLoader` (Avalonia 12 no acepta `avares://` directo en `Bitmap`).

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App` (ahora produce y lanza **NetProtocol.exe**).
## 26-08-2026 — Barra de título: eliminado el doble "Net Protocol"

**Feedback del responsable:** "El nombre de la aplicación en la barra de título sale dos veces: una normal y otra encima del icono de la aplicación."

**Causa:** en Avalonia 12 el tema Fluent dibuja **su propia barra de título** (con `Title` e icono del sistema) por encima de la barra personalizada con gradiente que ya contenía el logo + texto "Net Protocol" → el nombre aparecía dos veces.

**Corrección (build 0/0 · tests 61/61 ✅):**
- `WindowDecorations="None"` (API de Avalonia 12; `SystemDecorations` está obsoleto) → el tema ya no dibuja ningún título/icono del sistema sobre la barra.
- La **barra personalizada es la única**: gradiente (recurso `PincelBarraTitulo` en App.axaml), logo 26×26 + "Net Protocol" **una sola vez**, arrastre de la ventana con `BeginMoveDrag(PointerPressed)`.
- **Botones de ventana propios** (— □ ✕) con estilo `WinBtn`: hover translúcido, el de cerrar en rojo `#E81123`; handlers `Minimize_Click`/`Maximize_Click`/`Close_Click`.

**Relanzar:** `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`.
## 26-08-2026 — Ajuste de línea integral: nada de texto recortado

**Feedback del responsable:** "La anchura sigue sin cumplir el requisito de que o se muestra entera la línea o se ajusta el interlineado para que quepa en la ventana."

**Corrección (build 0/0 · tests 61/61 ✅):**
1. **Barra superior → `WrapPanel`** (`ItemSpacing`/`LineSpacing`, API de Avalonia 12): si la ventana es estrecha, los controles (búsqueda, filtros, botones) **bajan de línea** en vez de recortarse a la derecha.
2. **Ítems de la sidebar con ajuste de línea**: nuevo `ItemProtocolo` (acrónimo · nombre completo con `TextWrapping=Wrap`, `MaxLines=2`, sin trimming + familia · estado) → nombres largos (ITS-G5, C-V2X…) se **envuelven**; aplicado a navegación y resultados de búsqueda.
3. **Placeholder del filtro** acortado a "Filtrar familias…" para que no se trunque.
4. Ficha (18 campos), leyenda y comparador ya usan línea envolvente (`DetailText` con `TextWrapping="Wrap"`); barra de estado igual: **o la línea cabe entera o se ajusta**.
## 26-08-2026 — Causa raíz del texto cortado: scroll horizontal del contenido

**Feedback del responsable:** "No se ha realizado correctamente el cambio, no es un texto puntual es un conjunto bastante notable de líneas que se cortan y no se ajustan. Revísalo."

**Causa raíz (estructural):** el `ScrollViewer` de la ficha tenía `HorizontalScrollBarVisibility="Auto"` → **un ScrollViewer con scroll horizontal mide el contenido con ancho infinito**, así que `TextWrapping="Wrap"` **nunca activaba**: TODAS las líneas largas (finalidad de TCP = 209 chars, encapsulación, fuentes, leyenda, comparador) se extendían y se cortaban — no era un texto puntual, era el mecanismo en sí.

**Corrección (build 0/0 · tests 61/61 ✅):**
1. `ScrollViewer` de la ficha → **`HorizontalScrollBarVisibility="Disabled"`**: el contenido tiene ancho finito y el texto **envuelve de verdad** (antes: ancho infinito → sin ajuste). La sidebar ya estaba en `Disabled`.
2. Se calculó la capacidad real: área de ficha ≈ 1012 px → ~130 chars/línea a 13 px JetBrainsMono; la línea más larga (230 chars) ahora se envuelve en ~2 líneas completas.
3. `ItemProtocolo` en la sidebar: se retiró `MaxLines=2` + `TextTrimming` que **recortaban** el nombre en 2 líneas; ahora el nombre se envuelve sin límite.
4. El `WrapPanel` de la barra superior (paso anterior) sigue activo: si no cabe, los controles bajan de línea.
## 26-08-2026 — Comparador configurable: ya no solo contra TCP

**Pregunta del responsable:** "¿por qué solo se comparan con TCP y no se pueden comparar con otros protocolos?"

**Motivo original:** el comparador nació en el prototipo D5 con **TCP como referencia canónica fija** (transporte de referencia para el primer spike); era una limitación de diseño, no técnica.

**Cambio (build 0/0 · tests 61/61 ✅):**
- Nueva **"Comparar con:"** — ComboBox en la barra superior con **los 113 protocolos** (muestra "ACR · Nombre" envuelto; TCP por defecto). El botón pasa a "**Comparar**" y usa la referencia seleccionada (`CompararConReferencia`), que puede ser **cualquier protocolo**: se comparan todos los aspectos (familia, estado, capas, PDU, puertos IANA, cifrado, finalidad, encapsulación). `ProtocoloComparador` ya era agnóstico; solo hacía falta el selector.
- El título de la salida sigue `=== Comparador: A vs B ===` con los acrónimos reales elegidos.
## 26-08-2026 — Crash al abrir el desplegable del comparador (fix)

**Feedback del responsable:** "Al abrir el desplegable el comparador se cae la aplicación y se cierra."

**Diagnóstico reproducible (drop-down abierto automáticamente al arrancar):** `NullReferenceException` en el lambda del `ItemTemplate` (`CompareTarget.ItemTemplate = FuncDataTemplate<Protocol>`): al abrir el popup, Avalonia **recicla contenedores** e invoca el data template con `data = null`; el lambda hacía `p.Acronimo` sin guarda → crash.

**Corrección (build 0/0 · tests 61/61 ✅):** guarda `p is null ? null : new TextBlock {…}` en el template (patrón estándar de templates reutilizados). Verificado con el dropdown abierto automáticamente (sin crash); retirado el diagnóstico. Los ítems de la sidebar no usan data templates (usan `Tag`/`ItemProtocolo`), no les aplica.
## 26-08-2026 — Desplegable "Comparar con:": ancho fijo y texto tipo carrusel

**Petición del responsable:** "Haz que el desplegable del protocolo a comparar sea de un ancho fijo y que el texto en su interior sea el que se mueva como si fuera un carrusel, siempre que su tamaño lo requiera."

**Implementación (build 0/0 · tests 61/61 ✅):**
1. Nuevo control **`MarqueeTextBlock`**: mide el texto real (`FormattedText`); si desborda el ancho disponible, **se desplaza en bucle** (entra por la derecha → sale por la izquierda → reinicia, con pausa inicial para poder leer el principio); si cabe, queda estático. Usa `DispatcherTimer` (16 ms, ~90 px/s) + `TranslateTransform`; se detiene al salir del árbol visual.
2. `CompareTarget` con **ancho fijo 300 px**; su `ItemTemplate` envuelve el texto en un **`Border` con `ClipToBounds`** (recorte del carrusel) y `MarqueeTextBlock` (se conserva la guarda null contra el reciclaje del popup). Al abrir el desplegable, los ítems del popup disponen de más ancho → el texto completo se muestra; en el área cerrada del selector, el texto largo se mueve como carrusel.
## 26-08-2026 — Marquee corregido: solo en el popup bajo el puntero; ancho de lista fijo

**Aclaración del responsable:** el carrusel NO debía estar en el recuadro del selector (superfluo); se refería a los valores seleccionables: la ventanita de la lista se ajustaba al ancho del texto (quería que quedara fija) y el texto del elemento bajo el puntero debía moverse hasta el clic.

**Corrección (build 0/0 · tests 61/61 ✅):**
- `MarqueeTextBlock` detecta el contexto: si su `VisualRoot` **no** es un `Window` (es decir, vive dentro del `PopupRoot` del desplegable) → **carrusel solo cuando el puntero está sobre ese ítem** (entra por la derecha con pausa breve; al salir el puntero o al cerrar, se detiene y vuelve a elipsis estática). Si `VisualRoot` es el `Window` (recuadro del selector) → **siempre estático con elipsis**.
- `Border` del template con **`Width = 300` fijo** → la ventanita de la lista **deja de crecer** con el ancho del texto; todos los ítems comparten el mismo ancho y el texto largo se recorta/desplaza dentro de él.
## 26-08-2026 — Recuadro limpio (SelectionBoxItemTemplate) y carrusel bajo el puntero activado

**Feedback del responsable:** (1) "se han colado dos caracteres en el cuadro de selección del desplegable… aparecen para cualquier protocolo"; (2) "no existe animación para los textos debajo del puntero del ratón; debería animarse el texto cuando el puntero esté encima de ese elemento".

**Correcciones (build 0/0 · tests 61/61 ✅):**
1. **Recuadro del selector** → nuevo **`SelectionBoxItemTemplate`** (API de Avalonia 12, equivalente al `SelectionBoxItemTemplate` de WPF): el recuadro usa un TextBlock **estático con elipsis** (sin el `Border` de 300 px ni el marquee) — desaparecen los caracteres extra que producía reutilizar el template del popup en el recuadro. El `ItemTemplate` (con ancho 300 + carrusel) queda **solo para el popup**.
2. **Carrusel bajo el puntero**: el marquee ya no depende de `IsPointerOver` (poco fiable dentro del `ListBoxItem` del popup): ahora se suscribe a **`PointerEntered`/`PointerExited`** del propio control (que cubre todo el ancho del ítem por `HorizontalAlignment=Stretch`) y anima **solo cuando el puntero está encima de ESE ítem**; al salir, se detiene y vuelve a elipsis estática. El `Border` del popup mantiene `Width=300` fijo (la ventanita no crece con el texto).
## 26-08-2026 — Carrusel rediseñado (Canvas + SetLeft): animación garantizada bajo el puntero

**Feedback del responsable:** "La animación tipo carrusel horizontal del texto no se consigue."

**Causa:** la versión anterior usaba un `TextBlock` con `RenderTransform` (TranslateTransform); en el popup el cambio de transform **no invalidaba el render** de forma fiable (texto movido que no se repintaba) y, además, sin fondo el control no era hit-testable → los eventos de puntero no llegaban.

**Corrección (build 0/0 · tests 61/61 ✅):** `MarqueeTextBlock` rediseñado como **`Canvas`** que:
- Recorta con su propio `ClipToBounds` (ancho fijo 300 px del ítem).
- **Reposiciona un `TextBlock` hijo con `Canvas.SetLeft` en cada tick** → el reposicionamiento invalida el layout → repintado **garantizado** (no depende del renderer de transform).
- `Background = Transparent` → hit-testable → recibe `PointerEntered`/`PointerExited`.
- Detecta popup vs recuadro por `VisualRoot` (PopupRoot vs Window): **solo en el popup y bajo el puntero anima**; en el recuadro (SelectionBoxItemTemplate) estático con elipsis.
- Reenvía Text/FontSize/FontStyle/FontWeight/Foreground al TextBlock interno (bindings).
## 26-08-2026 — Carrusel v3 (Grid + Margin): texto siempre visible en el popup

**Feedback del responsable:** "Ahora no hay texto que seleccionar en el popup de los protocolos, no se ve el texto."

**Causa:** la versión con `Canvas` ocultaba el texto del popup (el Canvas como raíz del ítem no renderizaba el texto de forma fiable en el contexto del popup del ComboBox).

**Corrección (build 0/0 · tests 61/61 ✅):** `MarqueeTextBlock` ahora es un **`Grid` con `ClipToBounds`** que contiene un **`TextBlock` hijo visible por defecto** (HorizontalAlignment=Left, sin desplazamiento inicial → el texto se ve desde el arranque). El movimiento usa **`Margin` del TextBlock** (el relayout del contenedor fuerza el repintado — sin depender del renderer de transforms). Se reutilizan directamente las propiedades de `TextBlock` (sin `AddOwner`), evitando advertencias de nulabilidad. El `SelectionBoxItemTemplate` del recuadro sigue siendo un TextBlock estático con elipsis.
## 26-08-2026 — Botones inferiores en la sidebar y sección "Acerca de"

**Petición del responsable:**
1. Mover "Tema claro/oscuro" y "Leyenda" de la barra superior a la **parte inferior izquierda de la sidebar**.
2. Botón **"Acerca de"** abajo del todo en la sidebar, que abra una nueva sección con los datos de la aplicación centrados y formateados.

**Implementación (build 0/0 · tests 61/61 ✅):**
- `MainWindow.axaml`: la barra superior queda solo con búsqueda + filtros + comparador; la **sidebar** gana un bloque inferior (Dock=Bottom): separador + **Tema claro/oscuro** + **Leyenda** + **Acerca de**.
- Nueva vista **MostrarAcercaDe()**: texto centrado (`TextAlignment.Center`) con:
  - Nombre de la aplicación: **Net Protocol**
  - Versión: **1.0.0**
  - Autor: **Marcos Calabrés Ibáñez**
  - Email: **marcoscalabresibaniez@gmail.com**
  - **© Todos los derechos reservados**
  - Oculta los diagramas; la ficha vuelve a alinear a la izquierda al seleccionar un protocolo (`RenderFicha` restaura `TextAlignment.Left`).
## 26-08-2026 — Sección "Acciones" en la sidebar con botones estilo desplegable

**Petición del responsable:** hacer una sección especial en la sidebar con los tres botones (Tema/Leyenda/Acerca de) estilizados como los desplegables superiores, más estrechos y con un matiz que los diferencie (son botones, no desplegables).

**Implementación (build 0/0 · tests 61/61 ✅):**
- Rótulo **"Acciones"** + separador en la parte inferior de la sidebar.
- Botones `SidebarAction` (220 × 34 px, más estrechos que los ComboBox de 210–300): **réplica del aspecto de los desplegables** usando los mismos recursos del tema Fluent (`SystemControlBackgroundChromeMediumLowBrush` de fondo · `SystemControlForegroundBaseMediumBrush` de borde · texto `SystemControlForegroundBaseHighBrush`), esquinas 4 px, ancho completo de la sección.
- **Matiz diferenciador** (porque son botones, no desplegables): **icono circular de acento** a la izquierda (◐ tema, ☰ leyenda, ℹ acerca) + **chevrón "▸" tenue** a la derecha (sugiere acción, inverso del ▾ del desplegable); hover con fondo de lista baja + borde de acento; pressed con fondo de lista media.
- Se quitaron los handlers duplicados del XAML (los Click siguen conectándose en el constructor).
## 26-08-2026 — Leyenda alineada a la izquierda (manteniéndola en la zona central)

**Petición del responsable:** el texto de la sección **Leyenda** no debe verse centrado; alineado a la **izquierda**, pero mostrado en la zona central de la app (panel de detalle).

**Implementación (build 0/0 · tests 61/61 ✅):**
- `MostrarLeyenda()` restaura `DetailText.TextAlignment = TextAlignment.Left` al inicio (antes solo `RenderFicha` lo hacía; tras visitar "Acerca de", que centra el texto, la Leyenda quedaba centrada por herencia).
- Lo mismo en `CompararConReferencia()`: la tabla y los campos largos del comparador vuelven a alinearse a la izquierda aunque se haya visitado "Acerca de" antes.
- "Acerca de" conserva su centrado (`MostrarAcercaDe` sigue fijando `TextAlignment.Center`), y cualquier ficha/comparador/leyenda posterior lo restaura.
- Verificación: app lanzada 6 s sin crash; git push en `kraso/redes-knowledge`.
## 26-08-2026 — Diagnóstico del "heredado centrado" en Leyenda: binario Debug obsoleto

**Síntoma reportado:** con la corrección de alineación ya en el código, pulsar "Acerca de" y luego "Leyenda" seguía mostrando el texto centrado.

**Causa raíz (build 0/0 · tests 61/61 ✅):** el código fuente ya restauraba `TextAlignment.Left` en `MostrarLeyenda`/`CompararConReferencia`, pero **existían dos binarios**: `bin\Debug\net9.0\NetProtocol.exe` (20:38, anterior a la corrección) y `bin\Release\net9.0\NetProtocol.exe` (20:46, corregido). El responsable lanzaba el perfil **Debug** (F5/`dotnet run`), que seguía conteniendo el bug.

**Acción:** reconstruido el perfil **Debug** con la corrección (20:54): ambos binarios quedan al día y el comportamiento es idéntico en los dos perfiles. Smokes test Debug: lanzamiento 6 s sin crash. `git push` en `kraso/redes-knowledge`.
## 26-08-2026 — Botones "Acciones": adiós al color de acento, iconos en blanco brillante

**Petición del responsable:** quitar el color **marrón anaranjado** (acento de Windows, `SystemControlHighlightAccentBrush`) de los tres botones de la sidebar y sustituirlo por **blanco brillante**; cambiar también los iconos, usando el **logotipo de la aplicación** en "Acerca de". Se mantiene el patrón de diseño (círculo + texto + chevrón).

**Implementación (build 0/0 · tests 61/61 ✅):**
- El círculo de cada botón pasa de acento naranja al 85 % de opacidad a **blanco brillante permanente** (`#FFFFFFFF`, opacidad 1): ahora es el único color fijo de la sección, igual que la barra de título.
- Glifos en **gris oscuro** (`#FF1C1C1C`) sobre el círculo blanco para mantener contraste en ambos temas:
  - **Tema claro/oscuro** → ☾ (luna, antes ◐).
  - **Leyenda** → ≡ (índice de líneas, antes ☰).
  - **Acerca de** → **logotipo de la aplicación** (`Assets/Logo_NetProtocol.png`) dentro del círculo (22×22, padding 3), antes ℹ.
- El borde del hover deja de usar el acento (naranja) → **`SystemControlForegroundBaseHighBrush`** (borde claro del tema, neutro en claro y oscuro). Sin acentos en toda la sección.
- Reconstruidos **Debug y Release** (ambos binarios al día); app lanzada 6 s sin crash. `git push` en `kraso/redes-knowledge`.
## 26-08-2026 — Instaladores multiplataforma y Release oficial v1.0.0

**Petición del responsable:** generar los instaladores de todas las plataformas y subirlos como release a GitHub.

**Implementación (CI ✅ · release publicado):**
- Ampliado `.github/workflows/github-actions-ci.yml`: en tags `v*` ahora se construyen y publican los instaladores de las **tres plataformas** y se crea el **GitHub Release** automáticamente:
  - `package-win` → **Inno Setup** (`NetProtocol-Setup-1.0.0.exe`). Corregidos dos fallos latentes: faltaba descargar el artefacto `win-x64` (jobs en runners distintos) y el patrón de subida era `RedesKnowledge-Setup-*` cuando el script genera `NetProtocol-Setup-*`.
  - `package-linux` → **`.deb`** (dpkg-deb) + **`.rpm`** (rpmbuild). Fallos resueltos en CI: la línea de continuación del campo `Description` de Debian debe llevar un espacio inicial; `rpmbuild` ejecuta `%install` con cwd en `BUILD` → rutas absolutas.
  - `package-macos` → bundle **`.app`** (Info.plist) + **`.dmg`** (hdiutil, UDZO).
  - `release` → descarga `setup-*` y `gh release create`. Fallo resuelto: se adjuntaban todos los artefactos (colisión de `.dll` homónimas entre RIDs) → ahora solo `dist/setup-*/*`.
- **Release publicado:** https://github.com/kraso/redes-knowledge/releases/tag/v1.0.0 con 4 assets:
  - Windows: `NetProtocol-Setup-1.0.0.exe` (54,0 MB)
  - Linux: `NetProtocol-1.0.0-amd64.deb` (37,2 MB) y `NetProtocol-1.0.0-x86_64.rpm` (46,5 MB)
  - macOS: `NetProtocol-1.0.0-macos.dmg` (55,2 MB)
- Verificación previa local: instalador Windows compilado con Inno Setup 6 local (51,5 MB) ✅.
- **Pendiente** (backlog): firma de código/instaladores (requiere certificados) — el release es **sin firmar** (Gatekeeper/SmartScreen mostrarán aviso).
- Commit del flujo `v1.0.0`=última versión del workflow; `REGISTRO/Estado-de-Fases.md` actualizado (61/61 tests, CI activo, release publicado).
## 26-08-2026 — v1.0.1: instalador Windows no arrancaba (datasets no embebidos + raíz de datos de desarrollo)

**Síntoma reportado:** el instalador Windows instala los ficheros correctamente, pero la app **no arranca** — ni desde el menú Inicio ni con "Ejecutar al terminar" del instalador.

**Causa raíz (reproducida localmente en "modo instalado" → crash exit 0xE0434352):**
1. `RaizDelRepositorio()` subía desde `AppContext.BaseDirectory` buscando `PLANREDES.md`. En desarrollo el exe vive dentro del repo y lo encontraba; **en una máquina instalada no existe** → `InvalidOperationException` en el constructor → proceso muere antes de abrir ventana.
2. El instalador **no incluía los datasets** (F3/F4/F5/F6) que la app lee en arranque.
3. La base de datos se creaba bajo la raíz del repo (`FASE-II-DISENO/run`) — no escribible en Program Files para usuario normal.

**Fix (build 0/0 · tests 61/61 ✅ · verificado con el instalador real):**
- Los 5 catálogos de la Fase I se **embeben junto al ejecutable** (`datos/FASE-XX-...` vía `<None Link + CopyToOutputDirectory>` en el csproj).
- Nueva `RaizDatos()`: si existe `datos/FASE-03-INVENTARIO` junto al exe → raíz de datos **modo instalado**; si no, sube buscando `PLANREDES.md` (modo desarrollo).
- Base de datos movida a **`%LOCALAPPDATA%\NetProtocol\knowledge.db`** (escribible), con fallback a la raíz de datos.
- "Acerca de" muestra la **versión del ensamblado** (ya no hardcodeada): 1.0.1.
- Bump a **v1.0.1** (csproj + `.iss`). Release v1.0.0 (roto) **eliminado**; publicado **v1.0.1** en https://github.com/kraso/redes-knowledge/releases/tag/v1.0.1 con los 4 instaladores (`.exe`, `.deb`, `.rpm`, `.dmg`).
- Verificación: instalación silenciosa en carpeta temporal → `datos/` presente → la app **arranca** desde el directorio instalado con CWD de Inicio ✅.
- Extra: `FASE-II-DISENO/data/NetProtocol.rar` (181,6 MB, fuera de límite GitHub) excluido del repo (`.gitignore` corregido; el `Add-Content` se había pegado a la línea `.git/` sin salto final).
## 26-08-2026 — Firmas GPG para .deb/.rpm y dependencias runtime de Linux

**Petición del responsable:** (1) cómo firmar correctamente los instaladores Linux (.deb/.rpm); (2) que el `.rpm` declare sus dependencias runtime: `liblttng-ust0` y `liburcu6`.

**Implementación (workflow CI + docs):**
- `.deb` → `Depends: liblttng-ust0 (>= 2.12.0), liburcu6 (>= 0.12.1)`; `.rpm` → `Requires: liblttng-ust0 >= 2.12.0` + `Requires: liburcu6 >= 0.12.1` (librerías nativas del runtime .NET para diagnóstico/trazado).
- Nuevo paso **"Firmar .deb/.rpm con GPG"** en `package-linux`: se activa **solo si existen los secretos** `GPG_PRIVATE_KEY`/`GPG_KEY_ID` (+ `GPG_PASSPHRASE` opcional); sin ellos avisa y publica sin firmar. Firma `.deb` con `dpkg-sig --sign builder` y `.rpm` con `rpmsign --addsign` (variante `--pinentry-mode loopback` si hay pasphrase); **verificación obligatoria** (`rpm --checksig` / `dpkg-sig --verify`). Exporta la clave pública como asset `NetProtocol-gpg-pubkey.asc`.
- `packaging/README.md`: sección completa de firma (generación de clave, secretos, verificación por parte del usuario) + tabla de dependencias.
- Pendiente del responsable: generar la clave GPG y añadir los 3 secretos para que la firma se active en el próximo tag.
## 26-08-2026 — Firma GPG operativa: clave dedicada sin frase + validación rc

**Historial de la puesta en marcha (fallos resueltos en CI):**
1. `dpkg-sig` **retirado de repos Ubuntu/Debian** → el `.deb` se firma con **firma adjunta `.asc` estándar** (`gpg --detach-sign --armor`), verificable con `gpg --verify`. El repo apt con `Release` firmado queda como vía canónica futura.
2. La clave personal RSA (`84A27DF8CF75FE62`) tenía frase protegida y daba `Bad passphrase` en CI de forma inconsistente (bytes alterados entre PowerShell → `gh secret set` → gpg de Ubuntu; se normalizaron CR/LF sin éxito) → se adoptó la **clave dedicada de CI "Net Protocol Releases" (`FFB1C378AD2E3628`, RSA 4096, SIN frase)** generada en local; secretos del repo actualizados: `GPG_PRIVATE_KEY` + `GPG_KEY_ID` (se eliminaron `GPG_PASSPHRASE` y el secreto residual `OSRPJ53...`).
3. `rpmsign` con `--armor` producía `Unsupported PGP signature` (rpm espera firma **binaria**) → se quita `--armor` del `__gpg_sign_cmd`.
4. `rpm --checksig` daba `SIGNATURES NOT OK` sin la clave pública → se **importa la pubkey en rpm** antes de verificar.
5. La frase es **opcional** en CI: con clave sin protección firma directa en batch; con `GPG_PASSPHRASE` usa loopback + `passphrase-fd 0`.

**Validación (tag `v1.0.2rc1`, CI ✅):** release con `.deb` + `.deb.asc` (verificado localmente con gpg → `Firma correcta de "Net Protocol Releases"`), `.rpm` firmado (`rpm --checksig` OK en CI), `NetProtocol-gpg-pubkey.asc` adjunto, `.dmg` y Windows. Las dependencias Linux (`liblttng-ust0`, `liburcu6`) quedaron declaradas en `.deb`/`.rpm`.

**Extra:** la versión del instalador Windows salía hardcodeada (`NetProtocol-Setup-1.0.1.exe`) → ahora el `.iss` usa `#define MyAppVersion` y CI pasa `/DMyAppVersion=<tag>` (verificado localmente: genera `NetProtocol-Setup-1.0.2.exe`).
## 26-08-2026 — Causa raíz de la firma con frase y resolución definitiva (CD11)

**Síntoma persistente:** frase protegida de una clave (primero `84A27DF8CF75FE62`, luego `CD11DE8033B6E164`) → `Bad passphrase` constante en CI pese a validar OK en local y a subir la frase por consola, fichero y web.

**Diagnóstico (job `diagnostico-gpg`, workflow_dispatch, sin revelar el valor):** longitud 35 · ASCII · sin espacios · sha256 calculada; **las 3 firmas directas con gpg en la misma imagen (Ubuntu 24.04, gpg 2.4) daban exit 0** → la frase y la clave eran correctas. El fallo era **solo de `rpmsign`**: no reenvía el stdin de la tubería a gpg, así que `--passphrase-fd 0` entregaba **frase vacía** → `Bad passphrase` (por eso la clave sin protección funcionaba: frase vacía = correcta).

**Fix (verificado, CI ✅ en `v1.0.2rc1`):** rpmsign firma con **`--passphrase-file /tmp/netprotocol-pass.txt`** (fichero con permisos 600, borrado tras firmar) en vez de fd 0. La firma directa del `.deb` (gpg con `passphrase-fd 0`) ya funcionaba.

**Estado final de la firma:** clave **`CD11DE8033B6E164`** ("MARCOS CALABRES IBAÑEZ (27/08/2026 05:43:35)", RSA 4096, con frase) en los secretos `GPG_PRIVATE_KEY`/`GPG_KEY_ID`/`GPG_PASSPHRASE`. `.deb` con `.asc` (verificado: `Firma correcta de "MARCOS CALABRES IBAÑEZ…"`) y `.rpm` firmado (`rpm --checksig` OK); `NetProtocol-gpg-pubkey.asc` adjunto. Instalador Windows ya versionado por tag (`NetProtocol-Setup-1.0.2rc1.exe`). Eliminadas del llavero local: `84A27DF8CF75FE62` (revocada) y `7EC2D658F76BD4A3`.