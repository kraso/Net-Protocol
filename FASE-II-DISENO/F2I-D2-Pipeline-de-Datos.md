# F2I-D2 — Pipeline de Datos (Opción A)

**Fase II — Épica D2 (Pipeline de datos) — Opción A aprobada: D2-1 + D2-3; D2-2 diferido**
**Documento rector:** `F2I-Backlog-Detallado.json` (D2-1…D2-3) · Resultados **reales** del 26-08-2026.

| Campo | Valor |
|---|---|
| Documento | F2I-D2-Pipeline-de-Datos.md |
| Versión | 1.0 |
| Fecha | 26-08-2026 |
| Estado | ✅ Completada (Opción A) |

---

## 1. Resumen

| Hito | Resultado |
|---|---|
| **D2-1 — Importador IANA completo** | ✅ Parseo del **registro real** (fixture versionado 26-08-2026): **15.402 filas → 13.141 servicios importados** tras deduplicación; persistencia en lote en SQLite (tabla `Services`, migración v2) |
| **D2-3 — Snapshots (hash/diff/rollback)** | ✅ Manifiesto inmutable con **hash agregado determinista**, copia de contenidos, **diff** y **rollback con verificación de integridad** |
| **D2-2 — Deduplicación fina** | ⏳ **Diferido (decisión de la Opción A)**: se refina en la siguiente iteración D2 con los datos ya cargados (entity-linking con el inventario F3; tarea registrada) |
| **Calidad** | ✅ **28/28 pruebas** (10 nuevas de D2), 0 errores, 274 ms |

## 2. Fixture y datos reales

- **Fixture versionado:** `FASE-II-DISENO/data/iana-service-names-port-numbers-2026-08-26.csv` (descarga oficial con fecha de consulta **26-08-2026**; 1,15 MB, 15.402 líneas).
- Conteos reales del importador (misma lógica que los tests):

| Métrica | Valor |
|---|---|
| TotalFilas (incl. cabecera) | 15.402 |
| SinNombre (rangos no asignados) | 1.724 |
| SinPuerto (entrada sin puerto numérico) | 1.946 |
| **Importados tras deduplicación (nombre, puerto, transporte)** | **13.141** |
| Muestras verificadas | `ssh/22/tcp`, `domain/53/udp`, `https/443/tcp` |

## 3. Importador IANA (`Infrastructure/Iana/`)

- `CsvReader` — parser CSV con soporte de comillas escapadas (`""`).
- `IanaServiceImporter` — reglas de la Fase 3: el registro es **fuente de datos**; se registra **fecha de consulta**; **puerto ≠ protocolo** (no se crean protocolos a partir de puertos); deduplicación por `(nombre, puerto, transporte)`; cabecera validada (lanza `InvalidDataException` si no es el registro IANA).
- `SqliteServiceRepository` — inserción **en lote en una única transacción** (13.141 filas), consultas por nombre/transporte/puerto y por puerto; URN por servicio con fecha de consulta.

## 4. Snapshots (`Infrastructure/Snapshot/`)

- `DatasetSnapshotService.Crear` — manifiesto `{version, fecha, procedencia, hashAgregado, archivos[{ruta,bytes,sha256}], bytesTotales}` + copia de contenidos bajo `files/`.
- **Hash agregado determinista**: NO incluye fecha/procedencia → dos directorios idénticos producen el mismo hash (probado).
- `Diff` — añadidos/eliminados/cambiados entre dos manifiestos.
- `Restaurar` — rollback **con verificación de integridad** (rechaza contenidos manipulados) y recreación de estructura de directorios.

## 5. Resultados de pruebas (reales)

```
dotnet test Redes.Knowledge.Tests → Con error: 0, Superado: 28, Total: 28, Duración: 274 ms
```

Nuevos de D2: **IanaImporterTests (5)** — registro real completo (>15k filas; >12k importados), servicios conocidos, deduplicación, cabecera inválida, persistencia en lote + consultas · **SnapshotTests (5)** — manifiesto/contenido, determinismo del hash, diff, rollback verificado, rechazo de snapshot manipulado.

## 6. Criterios de D2 (Opción A)

- [x] D2-1 importador IANA completo sobre el CSV real (fixture versionado) + persistencia en lote.
- [x] D2-3 snapshots: manifiesto con hash agregado determinista, diff y rollback con verificación.
- [x] Migración v2 del almacén (tabla `Services`).
- [ ] D2-2 deduplicación fina / entity-linking con el inventario F3: **registrado como diferido** (siguiente iteración D2, al refinar el pipeline con los datos ya cargados).
- [x] Pruebas 28/28 y compilación de la solución sin errores.

## 7. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de software de escritorio | *(por confirmar)* | | |

> **Siguiente:** épica **D3 — UI básica** (shell, navegación jerárquica, ficha de protocolo de 18 campos, búsqueda/filtros FTS5) — y en paralelo queda registrada la **deduplicación fina (D2-2 continuación)** para cuando se carguen las fichas.

---
Última actualización: 26-08-2026