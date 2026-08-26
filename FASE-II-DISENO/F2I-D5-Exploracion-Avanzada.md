# F2I-D5 — Exploración Avanzada

**Fase II — Épica D5 (Exploración avanzada)**
**Documento rector:** `F2I-Diseno-de-Software.md` §B/§C · `F2I-Backlog-Detallado.json` (D5-1…D5-3) · Resultados **reales** del 26-08-2026.

| Campo | Valor |
|---|---|
| Documento | F2I-D5-Exploracion-Avanzada.md |
| Versión | 1.0 |
| Fecha | 26-08-2026 |
| Estado | ✅ Completada |

---

## 1. Resumen

| Hito | Resultado |
|---|---|
| **D5-1 — Grafo de relaciones** | ✅ Matriz de encapsulación **F4 cargada (20+ relaciones)**; **vecinos a 1 salto** por entidad; layout **estrella determinista** (`Layouts.Grafo`); vecinos visibles en la ficha de la app |
| **D5-2 — Comparador** | ✅ `ProtocoloComparador` (función pura): familia/estado (dominio), **PDU (F5)**, **puertos (registro IANA)**, **cifrado (F6)**; botón **"Comparar vs TCP"** en la app |
| **D5-3 — Fichas detalladas** | ✅ Loaders de los catálogos reales: **dispositivos F2 (22)**, **redes F2 (16)**, **campos F5** (por protocolo), **PDU F5**, **seguridad F6** (16 entradas); ficha de la app ampliada con **Campos** y **Vecinos** |
| **Calidad** | ✅ **45/45 pruebas** (6 nuevas) · app compila **0 errores / 0 advertencias** |

## 2. Nuevos componentes

| Componente | Responsabilidad |
|---|---|
| `CatalogoExploracion` (Infrastructure) | Carga relaciones F4, dispositivos F2, redes F2, PDU F5, cifrado F6 (siempre desde los JSON canónicos de la Fase I) |
| `GrafoRelaciones` (Infrastructure) | `Vecinos1Salto(entidad, relaciones)` con normalización; `EntidadDe(urn)` |
| `ProtocoloComparador` (Infrastructure) | `Comparar(...)` función pura → tabla `FilaComparacion` (regenerable) |
| `Layouts.Grafo` (Visualization) | Estrella determinista semilla + vecinos (ángulos por índice) |
| App (`MainWindow`) | Ficha ampliada, botón comparador, carga de catálogos D5 al arrancar |

## 3. Detalles verificados con datos reales

- **Grafo:** `Vecinos1Salto("HTTP/3")` → `quic (CorreSobre)`; relaciones: 20+ (HTTP/3→QUIC, QUIC→UDP, TCP→IPv4…).
  - *Nota de representación:* las URN del grafo usan **nombres normalizados** (`urn:entidad:http3`); el display con nombres originales se refina al cablear el panel visual del grafo en la UI (queda en tareas de pulido D5/D7).
- **Comparador:** BGP → puertos `179/TCP` (IANA) · PDU `—` · TCP → PDU `segmento` (F5) · familia `TRAN`; TLS → cifrado contiene `AEAD` (F6).
- **Fichas:** 22 dispositivos (Router presente), 16 redes (WAN presente), `ObtenerPduF5("TCP")` = `segmento`, `ObtenerPduF5("UDP")` = `datagrama`.

## 4. Incidencias reales resueltas

1. Las URN del grafo normalizan nombres → expectativas de tests ajustadas a `http3`/`quic`.
2. `IReadOnlyList<Field>` vs `List<Field>` en el diccionario de la app → tipo del diccionario corregido.

## 5. Resultados de pruebas (reales)

```
dotnet test → Con error: 0, Superado: 45, Total: 45, Duración: 1 s
dotnet build Redes.Knowledge.App → 0 Advertencia(s), 0 Errores
```

Nuevos de D5 (`ExploracionTests`, 6): relaciones reales F4 · vecinos HTTP/3 · dispositivos/redes reales · PDU y seguridad reales · comparador con BGP→179/TCP y PDU F5 · layout de grafo determinista.

## 6. Criterios de salida de D5

- [x] D5-1 grafo de relaciones con vecinos a 1 salto y matriz de encapsulación cargada (visibles en la app).
- [x] D5-2 comparador de protocolos regenerable (capa/familia, estado, PDU, puertos IANA, cifrado F6).
- [x] D5-3 fichas detalladas (dispositivo/red/mensaje/campo) con loaders sobre los catálogos reales y campos visibles en la ficha.
- [x] Pruebas 45/45 y app compilada 0/0.
- [~] Panel visual del grafo (render SVG en la app) y display de nombres originales: **tareas de pulido** en la integración D5/D7 (hoy el grafo se consulta vía servicios y tests; la vista interactiva se suma con el canvas).

## 7. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de software de escritorio | *(por confirmar)* | | |

> **Siguiente:** épica **D6 — Capturas y validación de layouts** (adaptador PCAP/PCAPNG + dissection por capas y correspondencia con `F5-Campos-PDU.json`, cerrando la **laguna L-004**).

---
Última actualización: 26-08-2026