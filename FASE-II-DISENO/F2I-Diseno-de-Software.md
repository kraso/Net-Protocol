# F2I — Plan Detallado de Diseño y Generación de Software (Fase II)

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase II:** Diseño y generación de software
**Documentos rectores:** `PLANREDES.md` §13–14 y §17 (prompt maestro de Fase II) · `FASE-09-PRODUCTO/F9-Especificacion-de-Producto.md` · Backlog previo `F9-Backlog.json`

| Campo | Valor |
|---|---|
| Documento | F2I-Diseno-de-Software.md |
| Versión | 0.1 (borrador para revisión) |
| Fecha | 26-08-2026 |
| Estado | Pendiente de aprobación |
| Entorno | Ver [`F2I-Entorno-de-Desarrollo.md`](F2I-Entorno-de-Desarrollo.md) (VS 2022 Enterprise 17.14, .NET SDK 9.0.316, git) |

> **Regla de la fase:** este plan entrega **arquitectura, UX/UI, módulos, pruebas, distribución y backlog detallado** listos para ejecutar. **No se escribe el código de la aplicación completa** hasta aprobar este plan y ejecutar la épica D0 (spikes + ADR finales).

---

## A. Arquitectura de software detallada

### A.1. Módulos por capa

| Capa | Módulos | Responsabilidad |
|---|---|---|
| **Presentation** | MOD-01 Shell (navegación, paneles, temas) · MOD-06a vistas de ficha · MOD-06b vistas de grafo/comparador · MOD-07a vista de captura | UI Avalonia (XAML/MVVM); temas claro/oscuro; virtualización |
| **Application** | MOD-08 Casos de uso | buscar · explorar · comparar · visualizar · importar fuente · actualizar catálogo · abrir captura · exportar informe |
| **Domain** | MOD-02 Dominio | Entidades y reglas: `Protocol`, `Standard`, `Version`, `MessageType`, `Field`, `PDU`, `Layer`, `Plane`, `Device`, `NetworkType`, `AddressingScheme`, `Source`, `Implementation`, `Capture`, `Diagram`, `SecurityMechanism`, `Relationship` |
| **Infrastructure** | MOD-03 Persistencia (SQLite/FTS5) · MOD-04 Búsqueda · MOD-05 Pipeline · MOD-07 Capturas (adaptador PCAP) · MOD-09 Configuración/Actualización | Almacén, índices, importadores, adaptadores, artefactos separados |
| **Visualization** | MOD-06 Visualización (renderer desacoplado) | Layouts deterministas, 10 plantillas, exportación SVG/PNG/PDF |
| **Knowledge pipeline** | MOD-05 (ingestion→snapshot) | Datos vivos versionados (IANA/RFC; validado en F8 V-09) |
| **Quality** | MOD-10 Calidad | CI/CD, controles automáticos de datos, métricas de cobertura |
| **Distribution** | MOD-09 (empaquetado) · CI por SO | Instaladores, dataset separado, offline |

### A.2. Contratos entre capas (interfaces públicas resumidas)

| Contrato | Capas | Firma representativa |
|---|---|---|
| `IProtocolRepository` | Infra→Domain | `GetByUrn(urn)`, `Query(spec)`, `Save(entity)`, `History(urn)` |
| `ISearchEngine` | Infra→App/UI | `Search(query, filtros[]) -> ResultSet<Hit>` (FTS5 + filtros por ejes F0) |
| `IDiagramRenderer` | Viz→UI | `Render(diagram:DatoEstructurado, plantilla:tipo) -> SVG` (determinista) |
| `IPcapAdapter` | Infra→App | `Open(path) -> Capture`, `Packets(capture) -> enumerable`, `Dissect(packet) -> campos` |
| `IDatasetPipeline` | Infra→App | `Import(fuente)`, `Snapshot() -> artefacto{hash,diff}`, `Rollback(snapshot)` |
| `IUpdateService` | Infra→App | `CheckUpdates(dataset)`, `ApplyUpdate(snapshot)`, `IsOffline()` |

### A.3. ADR-001 — Stack de aplicación

- **Contexto:** app de escritorio multiplataforma (Win/macOS/Linux), local-first, con volúmenes grandes de datos y diagramas técnicas; equipo con dominio C#.
- **Alternativas:** Avalonia/.NET (4,40) · Tauri/Rust (4,53) · Electron/TS (4,14) · Qt (4,12) · WPF (3,95) — matriz ponderada `PLANREDES.md` §12.1.
- **Decisión:** **Avalonia (XAML/MVVM) sobre .NET 9** como base; SQLite + FTS5; renderer de diagramas desacoplado con SVG.
- **Justificación:** diferencia marginal con Tauri en la matriz; ventaja real en dominio C#, reutilización de modelos/serialización/validación .NET y menor coste de cambio para el equipo; ecosistema de datos robusto (SQLite/EF opcional o Dapper — a decidir en D1).
- **Consecuencias:** spikes D0-1 (UI rica) y D0-2 (renderer) son el **árbitro final** frente a Tauri; si un spike no alcanza el umbral, se reactiva la alternativa con ADR de revisión.
- **Estado: ✅ CONFIRMADO en D0 (26-08-2026)** — **Avalonia 12.1.1** sobre **net9.0** (SDK 9.0.316 instalado); spike UI y plantilla oficial compilados sin errores (ver [`F2I-D0-Spikes-y-ADR.md`](F2I-D0-Spikes-y-ADR.md)). Nota de entorno: la plantilla oficial genera `net10.0` por defecto y el paquete `Avalonia.Diagnostics` está discontinuado (→ `AvaloniaUI.DiagnosticsSupport`).

### A.4. ADR-002 — Datos y persistencia

- Claves **URN estables** separadas del nombre; **versionado temporal** (`valid_from`/`valid_to`) en todas las entidades.
- **SQLite** (Single-file, offline) + **FTS5**; contrato de esquema: `Source` (F1 §3), `Field` (F5 §3), `SecurityMechanism` (F6 §2), plantilla de ficha (F4).
- **Artefactos separados** (regla del plan): ejecutable · dataset · fuentes/caché · índices · assets de diagramas → actualización sin recompilar.
- Catálogos F1–F7 como **semilla inicial** del dataset (113 protocolos, 22 clases, 16 redes, 51 campos, matrices y seguridad).

### A.5. ADR-003 — Visualización

- **Renderer desacoplado** del modelo de grafo; **layouts deterministas** (mismo dato → mismo diagrama; prueba automatizada).
- **SVG** como formato vectorial de intercambio; exportación PNG/PDF.
- Motores por tipo de vista (evaluación confirmada en D0 y a ejecutar en D4): **renderer propio determinista → SVG** para wire formats/mensajes (demostrado en D0-3); **Graphviz** (estados/flujo) y **Mermaid** (pila/encapsulación, secuencia) como motores de apoyo según el tipo; **Cytoscape.js o canvas** para el grafo navegable.
- **Estado: ✅ CONFIRMADO en D0** — enfoque "datos estructurados → SVG determinista" validado sobre el wire format de TCP (ver [`F2I-D0-Spikes-y-ADR.md`](F2I-D0-Spikes-y-ADR.md)).
- Plantillas: las 10 del plan §11; layouts regenerables desde `F5-Campos-PDU.json` y catálogo de diagramas.

### A.6. ADR-004 — Pipeline de datos

- Ciclo fijado en F3: ingestion → normalization → deduplication → entity linking → validation → indexing → **release snapshot**.
- **IANA CSV oficial** como primera fuente (validada en F8 V-09: 15.401 registros, 7.683 service names); normalización clave (service name, transporte); fecha de consulta obligatoria.
- Snapshot inmutable `{fecha, hash, procedencia, diff}` con rollback; **índices FTS reconstruibles**.

### A.7. ADR-005 — Capturas (observabilidad)

- Adaptador propio **PCAP/PCAPNG** (apertura, listado de paquetes, dissection por capas básica); sin embeder Wireshark; filosofía de disección conceptual (R3).
- Enlace paquete ↔ ficha de protocolo contra `F5-Campos-PDU.json`; cierra la **laguna L-004**.

### A.8. Decisiones diferidas (se cierran en D0/D1)

Empaquetado exacto por SO (D7) · motor de diagramas definitivo (D0-2) · ORM vs. SQL crudo en SQLite (D1) · runner CI multiplataforma (D7) · granularidad del adaptador PCAP (D6).

---

## B. Diseño de UX/UI

### B.1. Perfiles y principios (heredados de F0)

- Perfiles P1–P5 (NOC/SOC, arquitecto, seguridad, investigador, desarrollador) con profundidad N0–N3 (F0-Carta §3–4).
- Principios: local-first · trazabilidad visible (fuente/confianza por ficha) · incertidumbre explícita (`[n.p.d.]` visible) · datos regenerables · temas claro/oscuro de serie.

### B.2. Mapa de navegación (destino)

```
Shell
├── Panel lateral (exploración por ejes F0: familias · capas · planos · dominios · estado)
├── Búsqueda global (FTS5 + filtros combinables)
├── Área central
│   ├── Vista Ficha (protocolo/dispositivo/red/mensaje/campo — plantillas F4)
│   ├── Vista Grafo (relaciones + matrices de encapsulación)
│   ├── Vista Comparador (2+ protocolos)
│   └── Vista Captura (paquete ↔ campos documentados)
├── Barra de estado (dataset/versión, última sincronización, métricas de cobertura)
└── Menú de sistema (importar fuente · actualizar catálogo · exportar informe · ajustes)
```

### B.3. Wireframes (ASCII)

**Ficha de protocolo:** `[Encabezado: nombre · acrónimo · estado · fuente/confianza] [Pestañas: Vista | Campos | Mensajes | FSM | Seguridad | Diagramas] [Columna: identidad, encapsulación, capas; panel de enlaces/sources]`

**Comparador:** `[A] protocolo 1 [B] protocolo 2 [tabla: capa · PDU · puertos · seguridad · casos de uso]` con filas regenerables de los catálogos.

**Grafo:** lienzo con nodos (entidades) y aristas tipadas (encapsula/corre_sobre/depende_de); zoom + panel de inspección a 1 salto.

**Captura:** lista de paquetes (timestamps, IPs, puertos) + panel de dissection (árbol por capas) + panel de ficha vinculada.

### B.4. Temas y tokens (propuesta no finalista)

Neutros de fondo con acento técnico (azul), semántica de estado (vigente/obsoleto/…), marcas `[n.p.d.]` con estilo diferenciado; contraste objetivo AA (WCAG 2.2); tokens en recursos Avalonia (claro/oscuro) sin valores por defecto hardcodeados.

### B.5. Patrones para grandes volúmenes

Virtualización de listas/tablas (10k+ filas) · índice FTS5 + autocompletado · lazy-load de fichas y diagramas · caché de layouts calculados · páginas/ventana de resultados.

---

## C. Especificación de módulos

| ID | Módulo | Capa | Responsabilidad | Interfaz pública (resumen) | Dependencias | Criterios de aceptación técnicos |
|---|---|---|---|---|---|---|
| MOD-01 | Shell (UI) | Presentation | Navegación, paneles, temas, estado | `MainWindow`, `NavigationService`, `ThemeManager` | MOD-08 | 9 ejes navegables; temas claro/oscuro; virtualización |
| MOD-02 | Dominio | Domain | Entidades, reglas, versionado | `Protocol`, `Version`, `Field`, `Relationship`… | — | URN estables; valid_from/valid_to; validación |
| MOD-03 | Persistencia | Infrastructure | SQLite + FTS5, migraciones | `IProtocolRepository` | MOD-02 | CRUD; migraciones versionadas; integridad referencial |
| MOD-04 | Búsqueda | Infrastructure | FTS5 + filtros por ejes | `ISearchEngine` | MOD-03 | Búsqueda por protocolo/campo/RFC/puerto/capa/dispositivo/fabricante/dominio; filtros combinables |
| MOD-05 | Pipeline | Infrastructure | Importadores IANA/RFC, normalización, snapshot | `IDatasetPipeline` | MOD-02, MOD-03 | CSV IANA completo; dedup; snapshot+rollback |
| MOD-06 | Visualización | Visualization | Renderer desacoplado, layouts, exportación | `IDiagramRenderer`, `IDiagramLayout` | MOD-02 | Determinismo; 10 plantillas; SVG/PNG/PDF |
| MOD-07 | Capturas | Infrastructure | PCAP/PCAPNG + dissection | `IPcapAdapter` | MOD-03, MOD-02 | Apertura PCAP/NG; dissection por capas; enlace a campos F5 |
| MOD-08 | Casos de uso | Application | Orquestación de acciones | `BuscarUseCase`, `CompararUseCase`, `ImportarUseCase`… | MOD-04..07 | Cubren los 8 casos de uso del plan |
| MOD-09 | Config./Actualización | Infrastructure | Artefactos separados, update sin recompilar, offline | `IUpdateService` | MOD-05 | Dataset actualizable en runtime; offline completo |
| MOD-10 | Calidad | Quality | CI, controles datos, métricas | `QualityPipelines`, `CoverageDashboard` | TODO | Controles de la §9.3 del plan; golden-master |

DAG de dependencias: MOD-02 → MOD-03 → {MOD-04, MOD-05}; MOD-02+03 → MOD-07; MOD-06 ← MOD-02; MOD-08 ← {04,05,06,07}; MOD-09 ← MOD-05; MOD-10 ← todos.

---

## D. Plan de pruebas

| Nivel | Alcance | Automatización |
|---|---|---|
| **Unitarias** | Dominio (URN, versionado temporal, validación) · normalización CSV IANA con fixtures · serialización JSON/YAML round-trip | CI, cada PR |
| **Integración** | SQLite (migraciones, integridad referencial) · pipeline end-to-end con CSV real fijado (golden file) · búsqueda FTS5 con corpus de prueba | CI, cada PR |
| **Snapshot de esquemas de mensajes** | Layouts golden derivados de `F5-Campos-PDU.json` (TCP, IPv4, IPv6, UDP, Ethernet, DNS) | CI; regresión de catálogo |
| **Diagramas** | Determinismo (mismo input → mismo SVG) · exportación SVG/PNG/PDF válida | CI |
| **UI** | Pruebas de vista por contrato (Avalonia.Headless o patrón definido en spike D0-1) | CI (+ humana en D0) |
| **Datos/CI** | Controles automáticos de la §9.3: esquema, enlaces rotos, fuentes obsoletas, duplicados, contradicciones, integridad referencial, regresiones | Pipeline de calidad por SO |
| **Empaquetado** | Instalador limpio en Windows/macOS/Linux (CI multiplataforma) | Por release |

**Cobertura objetivo:** dominio ≥ 80 % · infraestructura crítica ≥ 70 % · UI core ≥ 40 % · **golden-master del pipeline** (hash de snapshot vs. esperado) siempre verde.

---

## E. Distribución y mantenimiento

- **Instaladores:** Windows (Inno Setup/MSIX — decisión en D7) · macOS (.dmg) · Linux (AppImage/deb); soportados por CI multiplataforma.
- **Dataset versionado con semver propio**, independiente del ejecutable; **actualización en runtime** desde caché/repositorio con rollback a snapshot previo; **offline** total con dataset embebido.
- **Mantenimiento del corpus:** auditorías trimestrales de fuentes (freshness), aplicación de estados de ciclo de vida (F0), incorporación de extensiones/deprecaciones nuevas; métricas de cobertura publicadas en cada release (panel MOD-10).

---

## F. Backlog técnico detallado

Ver [`F2I-Backlog-Detallado.json`](F2I-Backlog-Detallado.json): 8 épicas (D0–D7), **24 historias** con prioridad, **puntos de estimación relativa**, dependencias y criterios de aceptación. Orden de ejecución objetivo: D0 → D1 → D2 → D3 → (D4 ∥ D5) → D6 → D7.

---

## G. Riesgos técnicos de implementación

| Riesgo | Mitigación | Épica |
|---|---|---|
| Ajuste Avalonia/.NET 9 (versiones, templates, tooling) | Fijar versión en D0 (`dotnet --info` + templates); spike D0-1 | D0 |
| Renderer de diagramas por debajo del umbral | Spike D0-2; SVG como formato estable; motores por tipo | D0/D4 |
| Rendimiento con 15.401 registros IANA + catálogos | Virtualización, FTS5, lazy-load, caché | D3 |
| Regresión del dataset | Golden-master + controles automáticos en CI | D2/D7 |
| CI multiplataforma (macOS/Linux) sin runners | Seleccionar proveedor/self-hosted en D7; compilación reproducible por CLI `dotnet` | D7 |
| Complejidad del adaptador PCAP | Alcance mínimo (apertura + dissection básica), granularidad decidida en D6 | D6 |
| Dependencias nativas (si se requieren para PCAP) | MSVC disponible (VS2022); preferir soluciones 100 % managed | D6 |
| Mantenibilidad a largo plazo del corpus | Pipeline + snapshots + auditorías; documentación embebida | D2/D7 |

---

## H. Entorno de desarrollo (resumen)

Detalle completo en [`F2I-Entorno-de-Desarrollo.md`](F2I-Entorno-de-Desarrollo.md). Datos detectados (26-08-2026):

- **Visual Studio Enterprise 2022 17.14** — workloads: ManagedDesktop (.NET desktop), NetWeb, Azure, NativeDesktop (C++/MSVC), Universal, NetCrossPlat, NativeCrossPlat, DataScience, Python · componentes MSVC.
- **Visual Studio Build Tools 2019 16.11** (MSBuild/VCTools) — disponible como respaldo de builds.
- **.NET SDK 9.0.316** — target del proyecto: **net9.0** (Avalonia compatible; fijar versión exacta en D0).
- **git 2.54.0.windows.1** — control de versiones (convención Conventional Commits propuesta).

El IDE principal será VS 2022 Enterprise; **las builds reproducibles se definen por CLI `dotnet`** (independientes del IDE) para CI.

---

## Criterios de aceptación del plan de Fase II

- [x] Arquitectura detallada con ADR-001…005 (A).
- [x] Diseño de UX/UI: mapa de navegación, wireframes, temas, patrones de volumen (B).
- [x] Especificación de módulos con contratos y criterios técnicos (C).
- [x] Plan de pruebas (unitarias→CI, snapshot, golden-master, empaquetado) (D).
- [x] Distribución y mantenimiento (E).
- [x] Backlog detallado con estimaciones y dependencias (F).
- [x] Riesgos técnicos mapeados (G) y entorno de desarrollo documentado (H).
- [ ] Aprobación del plan → autoriza la épica **D0 (spikes)** y, tras sus ADR, la implementación.

---

## Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | *(pendiente)* | | |
| Arquitecto de software de escritorio | *(por confirmar)* | | |

---
Última actualización: 26-08-2026