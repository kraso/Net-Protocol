# F2I-D0 — Spikes y ADR Finales

**Fase II — Épica D0 (Decisiones y spikes)**
**Documento rector:** `F2I-Diseno-de-Software.md` · Resultados **reales** obtenidos el 26-08-2026 en este equipo.

| Campo | Valor |
|---|---|
| Documento | F2I-D0-Spikes-y-ADR.md |
| Versión | 1.0 |
| Fecha | 26-08-2026 |
| Estado | ✅ Completada (verificada) |

---

## 1. Resumen

| Hito | Resultado |
|---|---|
| **D0-1 — Entorno y plantillas** | ✅ .NET SDK 9.0.316 (win-x64) · plantillas Avalonia instaladas (`avalonia.app`, `avalonia.mvvm`, `avalonia.xplat`) · **Avalonia 12.1.1** (última estable en NuGet) · plantilla oficial compilada en **net9.0** con 0 errores |
| **D0-2 — Spike UI rica** | ✅ `SpikeUi` **compila (0 errores / 0 advertencias)**: DockPanel (sin Dock.Fill → **LastChildFill**, cambio de API en Avalonia 12), **ListBox virtualizado con 10.000 filas**, tema claro/oscuro por `RequestedThemeVariant` |
| **D0-3 — Spike renderer determinista** | ✅ `SpikeDiagramas` build 0 errores y ejecución OK: **campos TCP reales** del catálogo `F5-Campos-PDU.json` (11 campos, 10 con longitud) → **DETERMINISMO OK** (SHA256 idéntico en dos ejecuciones) → SVG de **2.636 bytes** exportado |
| **D0-4 — ADR finales** | ✅ ADR-001, ADR-002 y ADR-003 **CONFIRMADOS** (sección 4) |

## 2. D0-1 — Entorno (datos verificados)

- `dotnet --version` → **9.0.316** · RID `win-x64` · Windows 10.0.26200.
- Plantillas Avalonia previamente no instaladas → `dotnet new install Avalonia.Templates` **OK**.
- NuGet (consulta real): **Avalonia 12.1.1**, Avalonia.Desktop 12.1.1, Avalonia.Themes.Fluent 12.1.1.
- **Incidencias resueltas** (registradas, no inventadas):
  1. La plantilla oficial genera **`net10.0`** por defecto → ajustado a **`net9.0`** (SDK instalado). Avalonia 12.1.1 contiene `lib/net8.0` y `lib/net10.0` → compatible con net9.0 vía assets net8.0.
  2. `Avalonia.Diagnostics` **ya no existe** ≥ 12 (última 11.3.20) → se usa `AvaloniaUI.DiagnosticsSupport` 2.2.3 (patrón de la plantilla oficial).
- Entorno de VS 2022 Enterprise 17.14 documentado en `F2I-Entorno-de-Desarrollo.md`.

## 3. D0-2 — Spike de UI rica (`FASE-II-DISENO/spikes/SpikeUi/`)

Aplicación Avalonia **sin XAML** (UI en código) que valida los tres requisitos del spike:

- **DockPanel** con barra superior, panel lateral (260 px, exploración por ejes F0) y área central.
- **ListBox virtualizado** (`VirtualizingStackPanel` por defecto) con **10.000 filas** generadas en memoria (medición de generación visible en la barra de estado al ejecutar).
- **Tema claro/oscuro** por `Application.RequestedThemeVariant`.

**Hallazgo de API (Avalonia 12):** el enum `Dock` ya no incluye `Fill` (`Dock = {Left, Bottom, Right, Top}` — verificado por reflexión sobre el ensamblado 12.1.1); el relleno del área central se obtiene como **último hijo de DockPanel (LastChildFill)**.

**Limitación honesta:** la validación interactiva (fluidez de scroll, sensación de "sin degradación") es **manual** — se ejecuta el binario `SpikeUi` desde VS 2022 o `dotnet run --project FASE-II-DISENO\spikes\SpikeUi`. El criterio programático comprobable (compilación + 10.000 filas + virtualización) está verde.

## 4. D0-3 — Spike de renderer determinista (`FASE-II-DISENO/spikes/SpikeDiagramas/`)

Procesa el **catálogo canónico** `F5-Campos-PDU.json` (sin duplicar datos) y genera un **layout SVG bit/byte estilo RFC** a partir de offsets/longitudes en bits.

Resultado de la ejecución real:

```
Campos TCP catalogados en F5: 11 (con longitud: 10)
Run1 SHA256: 467B9761A4510CBC4183A930C36775325B5BF130AC8E99A8CE318C96F32B84F5
Run2 SHA256: 467B9761A4510CBC4183A930C36775325B5BF130AC8E99A8CE318C96F32B84F5
DETERMINISMO: OK (contenidos idénticos)
SVG exportado: <repo>\FASE-II-DISENO\spikes\out\tcp-header.svg (2636 bytes)
```

**Conclusión:** el principio "mismo dato → mismo diagrama" queda **demostrado** sobre el wire format de TCP; la arquitectura del renderer (desacoplado, SVG, basado en datos de F5) es viable para la épica D4.

## 5. D0-4 — ADR finales

| ADR | Decisión (confirmada en D0) | Evidencia |
|---|---|---|
| **ADR-001 · Stack** | **Avalonia 12.1.1 sobre .NET 9** (SDK 9.0.316), UI XAML/MVVM en producto, spike en código puro; SQLite + FTS5. Alternativa Tauri/Rust documentada y **descartada en esta iteración** (diferencia marginal en la matriz, sin ventaja demostrable en los spikes) | Spike UI compila; plantilla oficial compila |
| **ADR-002 · Datos** | Sin cambios sobre el diseño: URN estables, versionado temporal, SQLite+FTS5, artefactos separados, catálogos F1–F7 como semilla | — (diseño ya validado en F8) |
| **ADR-003 · Visualización** | **Renderer propio determinista → SVG** para wire formats/mensajes (demostrado en D0-3); **Graphviz/Mermaid** como motores de apoyo para flujo/estados (evaluación en D4); exportación PNG/PDF | Spike de determinismo OK |
| **ADR-004 · Pipeline** | Sin cambios (F3 + validación IANA real en F8 V-09) | — |
| **ADR-005 · Capturas** | Sin cambios (adaptador propio PCAP/PCAPNG, hito D6) | — |

## 6. Decisiones diferidas (confirmadas para D1/D4)

- TFM consolidado: **net9.0** (documentado en `F2I-Entorno-de-Desarrollo.md`).
- ORM vs. SQL crudo para SQLite → se decide en **D1-2**.
- Motor de apoyo para diagramas de flujo/estados → se evalúa en **D4**.
- Empaquetado por SO → **D7**.

## 7. Criterios de salida de D0

- [x] D0-1 entorno y plantillas validado (build real 0 errores).
- [x] D0-2 spike UI rica compilado (10k filas virtualizadas, dock, temas).
- [x] D0-3 renderer determinista demostrado (SHA256 idéntico + SVG exportado).
- [x] D0-4 ADR-001..003 confirmados y documentados.
- [ ] Validación interactiva del spike UI por el responsable (binario `SpikeUi` en VS 2022) — **pendiente visual**, no bloqueante para D1.

## 8. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de software de escritorio | *(por confirmar)* | | |

> **Siguiente:** épica **D1 — Núcleo de dominio y datos** (D1-1 modelo de dominio C#, D1-2 SQLite+FTS5, D1-3 serialización/validación).

---
Última actualización: 26-08-2026