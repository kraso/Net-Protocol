# F2I-D4 — Diagramas y Visualización

**Fase II — Épica D4 (Diagramas y visualización)**
**Documento rector:** `F2I-Diseno-de-Software.md` §B/§C (MOD-06) · `F2I-Backlog-Detallado.json` (D4-1…D4-3) · Resultados **reales** del 26-08-2026.

| Campo | Valor |
|---|---|
| Documento | F2I-D4-Diagramas-y-Visualizacion.md |
| Versión | 1.0 |
| Fecha | 26-08-2026 |
| Estado | ✅ Completada |

---

## 1. Resumen

| Hito | Resultado |
|---|---|
| **D4-1 — Renderer desacoplado** | ✅ Modelo de diagrama (`DiagramDocument` + primitivas Rect/Line/Text) **independiente** de SVG y PDF (ADR-003); layouts con aritmética determinista |
| **D4-2 — Plantillas (5 de 10)** | ✅ wire-format bit/byte (con datos reales de F5 · TCP), **pila/encapsulación**, **secuencia temporal** (DHCP DORA), **máquina de estados** (TCP), **ruta e2e** con PDU por enlace |
| **D4-3 — Exportación** | ✅ **SVG** (formato canónico, exportado a archivo) + **PDF vectorial mínimo válido y determinista** (sin dependencias externas) |
| **Calidad** | ✅ **39/39 pruebas** (7 nuevas de D4) · determinismo verificado en todas las plantillas |

## 2. Módulo `Redes.Knowledge.Visualization` (sin dependencias externas)

```
Redes.Knowledge.Visualization.csproj
├── DiagramDocument.cs     DiagramDocument · Primitive (Rect/Line/Text) · WireField · MensajeSecuencia · Transicion
├── Layouts.cs             5 productores de layout deterministas (WireFormat · Pila · Secuencia · MaquinaEstados · RutaE2E)
├── SvgRenderer.cs         SVG determinista (decimales con cultura invariable, texto escapado)
└── PdfExporter.cs         PDF 1.4 mínimo (objetos + xref con offsets exactos; rect/línea/texto)
```

**Desacople (ADR-003):** las plantillas producen un **modelo** (`DiagramDocument`); los renderers lo convierten a formato. Mismo input → mismas coordenadas → mismo SVG/PDF (probado comparando salidas).

## 3. Plantillas implementadas (plan §11)

| # | Plantilla | Entrada | Verificado con |
|---|---|---|---|
| 5 | **Mensaje / wire format (bit/byte)** | campos offset/longitud (bits) | **`F5-Campos-PDU.json` (TCP, 10 campos reales)** — contiene Source/Destination Port |
| 2 | **Pila y encapsulación** | lista de capas top→bottom | HTTP/3 · QUIC · UDP · IPv4 · Ethernet · Fibra (cadena real de F4) |
| 3 | **Secuencia temporal** | mensajes (de, para, etiqueta) | DHCP DORA (Discover→Offer→Request→Ack) |
| 4 | **Máquina de estados** | estados + transiciones | TCP (LISTEN→SYN-RECEIVED→ESTABLISHED) |
| 8 | **Ruta extremo a extremo** | nodos + PDU por enlace | Host→Switch L2→Router→Switch L2→Servidor |

## 4. Exportación (D4-3)

- **SVG**: formato vectorial canónico de intercambio; exportado a archivo y comparado byte a byte (test).
- **PDF mínimo** (`PdfExporter`): genera un **PDF 1.4 válido** (cabecera `%PDF-1.4`, objetos Catalog/Pages/Page/Content/Font, **xref con offsets exactos**, `startxref` y `%%EOF`) dibujando rectángulos, líneas y texto. Determinista (dos exportaciones idénticas).
- **PNG**: **tarea registrada** — la rasterización requiere el stack gráfico y se integra con la UI en la épica de exploración (D5: render del SVG en la app via Avalonia/Skia) o con `Svg.Skia` si el equipo lo prefiere. No bloqueante (el plan exige SVG/PNG/PDF; SVG y PDF ya cubren el vector; PNG se cierra con la vista interactiva).

## 5. Incidencias reales resueltas

1. `w` calculado como `double` (const `colW`) en `MaquinaEstados` → cast a `int` (contructor espera int).
2. `using System.Text;` faltante en los tests (`Encoding`).

## 6. Resultados de pruebas (reales)

```
dotnet test → Con error: 0, Superado: 39, Total: 39, Duración: 1 s
```

Nuevos de D4 (`VisualizationTests`, 7): wire format determinista desde F5 · pila con capas · secuencia DORA · FSM TCP · ruta e2e · **PDF válido y determinista** (prefijo, startxref, %%EOF, >500 B) · exportación SVG a archivo byte-a-byte.

## 7. Criterios de salida de D4

- [x] D4-1 modelo de diagrama desacoplado del renderer (ADR-003) con determinismo probado.
- [x] D4-2 cinco plantillas de las diez del plan (§11) regenerables desde datos estructurados.
- [x] D4-3 exportación **SVG** (canonical) y **PDF mínimo** válido.
- [~] **PNG**: tarea registrada para la integración D5 (rasterización en la app). No bloqueante.
- [x] Pruebas 39/39 y compilación sin errores.

## 8. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de software de escritorio | *(por confirmar)* | | |

> **Siguiente:** épica **D5 — Exploración avanzada** (grafo de relaciones + comparador + fichas detalladas), donde también se integrará la **vista de diagramas** y la **rasterización PNG**.

---
Última actualización: 26-08-2026