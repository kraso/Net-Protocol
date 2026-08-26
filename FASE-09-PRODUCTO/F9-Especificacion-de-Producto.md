# F9 — Especificación de Producto

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 9 — Especificación de producto (última fase de investigación)
**Documento rector:** `PLANREDES.md` §8 (F9), §13–14 (arquitectura y hoja de ruta) · Fase II: prompt de `PLANREDES.md` §17

| Campo | Valor |
|---|---|
| Documento | F9-Especificacion-de-Producto.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 |
| Estado | **Aprobada (26-08-2026)** |
| Depende de | F0–F8 (todas aprobadas) |
| Compuerta | **No se programa hasta cerrar F9** (plan S6) y ejecutar la Fase II |

---

## 1. Objetivo y compuertas

Cerrar la **especificación de software** (arquitectura, datos, búsqueda, visualizaciones, capturas, actualización, pruebas, empaquetado) y el **backlog técnico priorizado**, listos para convertirse en backlog ejecutable por otro equipo **sin iniciar la programación**. Al aprobar esta fase, la **Fase I (investigación y documentación) queda cerrada** y se entra en la **Fase II** mediante el prompt maestro de `PLANREDES.md` §17, que elabora el plan detallado de arquitectura, UX/UI, implementación, pruebas y distribución.

## 2. Decisión tecnológica (matriz ponderada confirmada)

| Tecnología | Puntuación ponderada (PLANREDES §12.1) | Veredicto |
|---|---|---|
| **Avalonia + C#/.NET** | **4,40** | ✅ **Base confirmada** |
| Tauri + Rust + web UI | 4,53 | Alternativa registrada (diferencia marginal; misma decisión en el plan) |
| Electron + TypeScript | 4,14 | Candidato fuerte (menor consumo/exceso de memoria) |
| Qt | 4,12 | Alternativa especializada |
| WPF/.NET | 3,95 | Solo Windows |

- **Decisión de partida confirmada:** C#/.NET + **Avalonia** (XAML/MVVM), **SQLite + FTS5**, **renderer de diagramas desacoplado** con exportación SVG/PNG/PDF, **pipeline de importación** de registros oficiales y adaptador **PCAP/PCAPNG** opcional.
- **Spikes de validación (compuerta de la Fase II, épica D0):** (1) UI rica con volúmenes grandes (tablas virtualizadas, paneles, navegación por grafo); (2) renderer de diagramas determinista. El resultado de los spikes es el árbitro final frente a Tauri (ADR D0-3).

## 3. Arquitectura de software (8 capas)

| Capa | Responsabilidad | Módulos |
|---|---|---|
| **Presentation** | Avalonia UI; MVVM; temas claro/oscuro; layouts técnicos | Shell, navegación, paneles, vista de ficha, grafo, comparador |
| **Application** | Casos de uso | buscar · explorar · comparar · visualizar · importar fuente · actualizar catálogo · abrir captura · exportar informe |
| **Domain** | Entidades y reglas de dominio | `Protocol`, `Standard`, `Version`, `MessageType`, `Field`, `PDU`, `Layer`, `Plane`, `Device`, `NetworkType`, `AddressingScheme`, `Source`, `Implementation`, `Capture`, `Diagram`, `SecurityMechanism`, `Relationship` |
| **Infrastructure** | Persistencia y adaptadores | SQLite, FTS5, JSON/YAML, caché, importadores, descarga/actualización de fuentes, adaptador PCAP |
| **Visualization** | Renderer desacoplado | layouts deterministas, plantillas de diagrama (10 tipos), exportación SVG/PNG/PDF, integración Mermaid/Graphviz/Cytoscape según tipo |
| **Knowledge pipeline** | Datos vivos | ingestion → normalization → deduplication → entity linking → validation → indexing → **release snapshot** (diseño F3; IANA validado en F8 V-09) |
| **Quality** | Calidad y CI | unit tests, schema validation, link checking, source freshness, data completeness scoring, regressions, validación de diagramas |
| **Distribution** | Empaquetado y actualización | instaladores Windows/macOS/Linux, dataset versionado independiente del ejecutable, modo offline con documentación embebida |

## 4. Datos y almacenamiento

- **SQLite** como almacén local principal + **FTS5** para búsqueda textual; **JSON/YAML** para fuentes importables y fixtures.
- **Claves URN estables** (`urn:proto:…`), **versionado temporal** (`valid_from`/`valid_to`) y **trazabilidad** a `Source` (esquema F1 §3).
- **Separación de artefactos** (requisito del plan): ejecutable · base de conocimiento · fuentes descargadas/caché · índices de búsqueda · assets de diagramas. El dataset se actualiza **sin recompilar** el ejecutable.
- Los catálogos de investigación (F1–F7) son la **semilla inicial** del dataset: `F3-Protocolos.json` (113), `F2-Catalogo-Dispositivos.json` (22 clases/34 fichas), `F2-Catalogo-Redes.json` (16), `F5-Campos-PDU.json` (51 campos), matrices y registros de seguridad.

## 5. Búsqueda y filtros

Búsqueda avanzada sobre índice FTS5 por: **protocolo · campo · RFC · puerto · capa · dispositivo · mensaje · fabricante · dominio · palabra clave**. Filtros por ejes de clasificación F0 (capas, planos, familia, estado de ciclo de vida, dominio, alcance).

## 6. Diagramas y visualización

- Renderer **desacoplado del modelo de grafo**; **layouts deterministas** (mismo dato → mismo diagrama).
- **10 plantillas** del plan (§11): arquitectura · pila/encapsulación · secuencia · estados · mensaje (bit/byte) · decisión · seguridad · ruta e2e · comparativo · captura.
- Exportación **SVG/PNG/PDF**; Mermaid/Graphviz/Cytoscape.js según el tipo de vista (decisión en ADR del spike D0-2).
- Los layouts se generan desde datos estructurados (`F5-Campos-PDU.json`), no a mano.

## 7. Capturas (observabilidad)

- Adaptador **PCAP/PCAPNG** (apertura y exploración); enlace **paquete ↔ ficha de protocolo**.
- Filosofía de **disección por capas** de Wireshark como referencia conceptual (R3); **sin embeder Wireshark**.
- Cierra la laguna L-004 (validación de layouts contra capturas reales) en el hito D6.

## 8. Pipeline de fuentes y actualización

- Diseño operativo de F3 con verificación real de IANA en F8 (V-09): 15.401 registros, 7.683 service names, snapshots versionados con hash/diff/rollback.
- Frecuencias por autoridad (F1 §4) y estados de sincronización (`pendiente/sincronizado/desactualizado/error`).
- Métricas de cobertura (PLANREDES §7.2) computadas en cada release.

## 9. Pruebas y calidad

Unitarias · integración · **snapshot de esquemas de mensajes** · **validación de diagramas regenerables** · pruebas de búsqueda FTS5 · golden-master del pipeline · CI/CD por SO. Controles automáticos de datos (PLANREDES §9.3): esquema, enlaces rotos, fuentes obsoletas, duplicados, contradicciones, integridad referencial, regresiones del dataset.

## 10. Distribución y mantenimiento

Instaladores por SO (Windows/macOS/Linux) · versión de dataset **independiente** de la versión del ejecutable · **modo offline** con documentación embebida · auditorías periódicas del corpus (deprecaciones, extensiones nuevas, freshness de fuentes).

## 11. Backlog técnico priorizado (resumen)

Épicas D0–D7 (detalle machine-readable en [`F9-Backlog.json`](F9-Backlog.json)):

| Épica | Contenido | Prioridad |
|---|---|---|
| **D0** | Decisiones y spikes (UI rica, renderer de diagramas, ADR de stack) | Must |
| **D1** | Núcleo de dominio + SQLite/FTS5 + serialización + validación | Must |
| **D2** | Pipeline de datos (IANA/RFC) + snapshots + índices | Must |
| **D3** | UI básica (navegación, ficha, búsqueda, filtros) | Must |
| **D4** | Diagramas y visualización (renderer, plantillas, exportación) | Should |
| **D5** | Exploración avanzada (grafo, comparador, fichas detalladas) | Should |
| **D6** | Capturas PCAP/PCAPNG y validación de layouts (cierra L-004) | Should |
| **D7** | Calidad, distribución, offline y Release 1.0 | Must |

## 12. Riesgos técnicos de implementación

| Riesgo | Mitigación | Épica |
|---|---|---|
| Rendimiento de UI con grandes volúmenes | Virtualización, índices FTS5, lazy loading | D3 |
| Renderer de diagramas subóptimo para algunos tipos | Spikes D0-2; exportación SVG como formato estable | D0/D4 |
| Carga de los 15.401 registros IANA | Pipeline incremental, snapshot y diff | D2 |
| Regresión del dataset | CI con golden-master y validación de esquema | D2/D7 |
| Complejidad del adaptador PCAP | Alcance acotado (apertura + dissection básica) | D6 |
| Decisión de stack aún abierta (Tauri) | Spikes D0; ADR D0-3 documentado | D0 |

## 13. Transición a la Fase II

La Fase II se inicia entregando el **prompt maestro de `PLANREDES.md` §17** a la sesión de diseño de software. Su resultado (arquitectura detallada, UX/UI, especificación de módulos, plan de pruebas, distribución, backlog refinado) debe resolver la especificación de este documento **antes de escribir el código** de la aplicación completa.

## 14. Criterios de salida / aceptación de F9

- [x] Decisión tecnológica documentada con matriz ponderada (§2).
- [x] Arquitectura de 8 capas y especificación de datos/búsqueda/diagramas/capturas/pipeline/distribución/calidad (§3–§10).
- [x] Backlog técnico priorizado (épicas D0–D7 con historias y criterios) — `F9-Backlog.json`.
- [x] Riesgos técnicos mapeados a épicas (§12).
- [x] Entrada a la Fase II definida (prompt `PLANREDES.md` §17) sin programación previa (§13).
- [x] **Aprobación de la fase (26-08-2026)** → **la Fase I de investigación y documentación queda cerrada**. La **Fase II** permanece **pendiente de inicio**, a la espera de instrucción expresa del responsable.

## 15. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de software de escritorio | *(por confirmar — Fase II)* | | |

> **Estado:** con esta aprobación, la **Fase I (investigación y documentación) queda cerrada**. La **Fase II — Diseño y generación de software permanece pendiente de inicio**, a la espera de instrucción expresa del responsable (punto de entrada: prompt `PLANREDES.md` §17 → épicas D0–D7 de `F9-Backlog.json`).

---
Última actualización: 26-08-2026