# F2I-D1 — Núcleo de Dominio y Datos

**Fase II — Épica D1 (Núcleo de dominio y datos)**
**Documento rector:** `F2I-Diseno-de-Software.md` §C · `F2I-Backlog-Detallado.json` (D1-1…D1-3) · Resultados **reales** del 26-08-2026.

| Campo | Valor |
|---|---|
| Documento | F2I-D1-Dominio-y-Datos.md |
| Versión | 1.0 |
| Fecha | 26-08-2026 |
| Estado | ✅ Completada (18/18 pruebas) |

---

## 1. Resumen

| Hito | Resultado |
|---|---|
| **D1-1 — Modelo de dominio C#** | ✅ 17 entidades con **URN estable** y **versionado temporal** (`valid_from`/`valid_to`); validaciones de dominio (Protocol, Source) sin dependencias externas |
| **D1-2 — Persistencia SQLite + FTS5** | ✅ Almacén con migraciones versionadas, repositorio con CRUD y **índice FTS5**; búsqueda funcional |
| **D1-3 — Serialización/validación** | ✅ JSON round-trip canónico + **YAML round-trip** (conversor URN); importación real de catálogos F3 (113) y F5 (11 campos TCP) |
| **Calidad** | ✅ **18/18 pruebas superadas** (0 errores, duración 146 ms) |

## 2. Estructura de la solución (`FASE-II-DISENO/src/`)

```
Redes.Knowledge.sln
├── src/Redes.Knowledge.Domain        (clases puras, sin dependencias externas)
│   ├── Urn.cs                         clave estable Y/U RN (urn:proto:tran:tcp)
│   ├── Enums.cs                       LifecycleState · NivelAutoridad · Confianza · RelacionTipo · FamiliaProtocolo
│   ├── EntityBase.cs                  URN + validity (ValidFrom/ValidTo) + EsValidoEn()
│   ├── Entities.cs                    las 17 entidades (Protocol…Relationship)
│   ├── Validation.cs                  ProtocolValidator · SourceValidator · ValidationResult
│   └── Ports.cs                       IProtocolRepository · ISearchEngine · SearchHit
├── src/Redes.Knowledge.Infrastructure
│   ├── SqliteKnowledgeStore.cs        migraciones versionadas + FTS5
│   ├── SqliteProtocolRepository.cs    CRUD + sincronización del índice
│   ├── SqliteSearchEngine.cs          búsqueda FTS5 (MATCH con escapado)
│   ├── CatalogJson.cs                 importación F3/F5 (sin duplicar datos) + round-trip JSON
│   └── SchemaYaml.cs                  YAML con conversor UrnYamlConverter
└── tests/Redes.Knowledge.Tests        UrnTests · ValidationTests · RepositoryTests · SerializationTests
```

**Paquetes reales (NuGet 26-08-2026):** Microsoft.Data.Sqlite **10.0.11** · YamlDotNet **18.1.0** · xunit 2.9.2 · Microsoft.NET.Test.Sdk 17.12.0.

## 3. Modelo de dominio (D1-1)

- **URN estable** separada del nombre mostrado (regla N2 de F0): `Urn.Protocol("TRAN","TCP")` → `urn:proto:tran:tcp`; `Urn.Parse` rechaza vacías.
- **Versionado temporal** en `EntityBase`: `EsValidoEn(DateTime)` verifica vigencia; validación impide `valid_from` posterior a `valid_to`.
- **17 entidades**: Protocol, Standard, Version, MessageType, Field, PDU, Layer, Plane, Device, NetworkType, AddressingScheme, Source, Implementation, Capture, Diagram, SecurityMechanism, Relationship (plan §6.1).
- **Validación de dominio** (reglas de la plantilla F4): campos obligatorios, familia/estado definidos, vigencia coherente, `Source` con versión y **fecha de consulta obligatoria** (política F0).

## 4. Persistencia y búsqueda (D1-2)

- Migraciones versionadas: `Protocols`, `Fields` (con FK a Protocols), `Sources` y **tabla virtual FTS5**.
- `IProtocolRepository`: Save (upsert), GetByUrn, GetAll, GetByFamilia, Delete — con sincronización del índice FTS5 en la misma transacción.
- `ISearchEngine` (SQLite FTS5): búsqueda textual con escapado de términos.

**Incidencias reales resueltas:**
1. **Microsoft.Data.Sqlite** rechaza lotes multi-sentencia con parámetros no usados en todas las sentencias → se ejecutan sentencias separadas dentro de la misma transacción.
2. **Pooling** mantenía el archivo de prueba bloqueado → `Pooling=False` + `ClearAllPools()` en los tests.
3. `IReadOnlyList<string>` no deserializable por YamlDotNet → `string[]` en `Aliases` y `Planos`.

## 5. Serialización (D1-3)

- **JSON canónico**: `Serialize → Deserialize → Serialize` produce el mismo JSON (probado).
- **YAML round-trip**: YamlDotNet 18 cambió la firma de `IYamlTypeConverter` (añade delegados `ObjectDeserializer`/`ObjectSerializer`) → conversor `UrnYamlConverter` adaptado y registrado en `SchemaYaml`.
- **Importación sin duplicar datos** (regla nº 3 del repositorio): los tests leen los catálogos **canónicos de la Fase I**:
  - `F3-Protocolos.json` → **113 protocolos** (estados mapeados, incl. `histórico` acentuado; `military_public` → `Desconocido` hasta ampliar el enum en D3).
  - `F5-Campos-PDU.json` → **11 campos TCP** (Destination Port @ offset 16 bits).

## 6. Resultados de pruebas (reales)

```
dotnet test Redes.Knowledge.Tests → Correctas!  Con error: 0, Superado: 18, Omitido: 0, Total: 18, Duración: 146 ms
```

Cobertura de la carpeta: Urn (3) · Validación (5) · Repositorio/Integración SQLite (6) · Serialización/Importación (4).

## 7. Criterios de salida de D1

- [x] D1-1 modelo de dominio C# con URN y versionado temporal — 17 entidades, validación con tests.
- [x] D1-2 SQLite + FTS5: CRUD, migraciones e integridad referencial (FK) verificada por tests.
- [x] D1-3 serialización JSON/YAML con round-trip fiel e importación de los catálogos reales.
- [x] Compilación de la solución **0 errores** (solo avisos resueltos).
- [ ] *Pendiente de ampliación (en D3, por backlog): mapear `military_public` y `n.d.p.` como estados/notas de incertidumbre en el enum.*

## 8. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de software de escritorio | *(por confirmar)* | | |

> **Siguiente:** épica **D2 — Pipeline de datos** (importador IANA real, normalización/deduplicación, snapshots con hash/diff/rollback).

---
Última actualización: 26-08-2026