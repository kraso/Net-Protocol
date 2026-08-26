# F2I-D3 — UI Básica (Aplicación Real)

**Fase II — Épica D3 (UI básica)**
**Documento rector:** `F2I-Diseno-de-Software.md` §B · `F2I-Backlog-Detallado.json` (D3-1…D3-3) · Resultados **reales** del 26-08-2026.

| Campo | Valor |
|---|---|
| Documento | F2I-D3-UI-Basica.md |
| Versión | 1.0 |
| Fecha | 26-08-2026 |
| Estado | ✅ Completada |

---

## 1. Resumen

| Hito | Resultado |
|---|---|
| **D3-1 — Shell + navegación jerárquica** | ✅ Aplicación **`Redes.Knowledge.App`** (Avalonia 12.1.1, XAML): DockPanel con barra superior, navegación por **familias** (Expander + lista, acceso a ficha en 2 clics) y **filtros familia/estado** |
| **D3-2 — Ficha de protocolo (18 campos)** | ✅ Vista renderizada con los datos del dataset cargado + **marcadores honestos** `[pendiente]`/`[n.p.d.]` para lo aún no cableado (F4/F5 → pipeline) |
| **D3-3 — Búsqueda/filtros FTS5** | ✅ Buscador con resultados navegables y filtros combinables por familia/estado; motor FTS5 probado sobre el catálogo real |
| **Calidad** | ✅ **32/32 pruebas** (4 nuevas) · App compila **0 errores / 0 advertencias** |

## 2. La aplicación (`src/Redes.Knowledge.App/`)

- **Arquitectura de la ventana:** barra superior (búsqueda + filtros + tema) · navegación lateral (ScrollViewer + Expander por familia) · ficha central (monospace) · barra de estado.
- **Bootstrap (D3):** `DatasetBootstrap.EnsureProtocolos` importa **F3-Protocolos.json (113 protocolos)** al almacén SQLite local (`FASE-II-DISENO/run/knowledge.db`) **solo si está vacío** (idempotente); los **13.141 servicios IANA** del fixture quedan disponibles vía `Services`.
- **Navegación jerárquica:** grupos por `FamiliaProtocolo` (13), cada protocolo a 1 clic dentro del grupo → acceso en 2 clics; filtros `Familia` y `Estado` combinables (reconstruyen la navegación).
- **Búsqueda:** FTS5 (`SqliteSearchEngine`) con Enter/botón; los resultados se muestran como lista navegable y la ficha del primero se renderiza.
- **Ficha de 18 campos:** muestran Identidad, Estado, Capas/Familia desde el dataset y `[pendiente]`/`[n.p.d.]` explícitos para los bloques que se cablearán en D4–D6 (encapsulación, PDU, campos, seguridad, fuentes). Nota dentro de la vista: la ficha canónica vive en `F4-Fichas-Prioritarias.md`.
- **Tema claro/oscuro** por `Application.RequestedThemeVariant` (spike D0-2 consolidado).

## 3. Hallazgos reales (Avalonia 12)

1. `TextBox.Watermark` es **obsoleto** en Avalonia 12 → **`PlaceholderText`** (aviso AVLN5001 resuelto; build final 0/0).
2. Se evitó `TreeView` (riesgo de plantillas) en beneficio de `Expander` + `ListBox` en código — mismo patrón probado en el spike D0-2.

## 4. Resultados de pruebas (reales)

```
dotnet test → Con error: 0, Superado: 32, Total: 32, Duración: 1 s
dotnet build Redes.Knowledge.App → 0 Advertencia(s), 0 Errores
```

Nuevos de D3 (`DatasetBootstrapTests`): bootstrap **113 idempotente** · búsqueda FTS sobre catálogo real ("tcp"/"Transmission"/"border") · filtros por familia/estado (TRAN→TCP/QUIC; Histórico→X.25/Token Ring, ≥10) · estado `military_public` → `Desconocido` sin romper el import.

## 5. Criterios de salida de D3

- [x] D3-1 shell con navegación jerárquica por ejes (familias) y acceso en 2 clics.
- [x] D3-2 ficha de protocolo con marcadores de pendiente explícitos.
- [x] D3-3 búsqueda FTS5 y filtros combinables (familia/estado), lógica cubierta por tests.
- [x] App real compilada (0/0) y solución con 32/32 pruebas.
- [ ] **Validación visual interactiva por el responsable** (ejecutar `Redes.Knowledge.App` desde VS 2022 o `dotnet run --project FASE-II-DISENO\src\Redes.Knowledge.App`) — pendiente manual, no bloqueante.

## 6. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de software de escritorio | *(por confirmar)* | | |

> **Siguiente:** épica **D4 — Diagramas y visualización** (renderer determinista validado en D0-3 → plantillas de pila/encapsulación, estado, mensaje bit/byte y ruta e2e; exportación SVG/PNG/PDF) — o, si prefieres validar primero la UI, ejecutar la app con VS 2022.

---
Última actualización: 26-08-2026