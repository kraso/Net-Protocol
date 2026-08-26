# F2I-D7 — Calidad, Distribución y Release

**Fase II — Épica D7 (Calidad, distribución y Release 1.0) — última épica**
**Documento rector:** `F2I-Diseno-de-Software.md` §D/§E · `F2I-Backlog-Detallado.json` (D7-1…D7-3) · Resultados **reales** del 26-08-2026.

| Campo | Valor |
|---|---|
| Documento | F2I-D7-Calidad-Distribucion-Release.md |
| Versión | 1.0 |
| Fecha | 26-08-2026 |
| Estado | ✅ Completada (Release candidato 1.0.0 generado) |

---

## 1. Resumen

| Hito | Resultado |
|---|---|
| **D7-1 — Calidad/CI** | ✅ Auditoría automática **A01–A07** (URNs únicas, duplicados, fichas válidas, integridad F5/F6/F7→F3, **golden-master**); plantilla **GitHub Actions** (jobs quality/build/package) y README de activación |
| **D7-2 — Distribución** | ✅ **Publicaciones self-contained reales**: win-x64 (231 f.), linux-x64 (228), osx-x64 (229) · plantilla **Inno Setup** · **dataset versionado** independiente del ejecutable (`DatasetMetadata`) |
| **D7-3 — Offline y Release** | ✅ Bootstrap que embebe el dataset (offline total) · **Release candidato 1.0.0** en `dist/` · metadatos de dataset (versión, hash golden, conteos) |
| **Calidad** | ✅ **56/56 pruebas en Release** (4 nuevas de D7) · publicaciones sin errores |

## 2. Control de calidad automático (`Infrastructure/Quality/`)

| Check | Qué detecta |
|---|---|
| A01 | **URNs duplicadas** (claves estables) |
| A02 | Duplicados por (familia, acrónimo) |
| A03 | Fichas inválidas (esquema F4 / dominio) |
| A04–A06 | **Integridad referencial F5/F6/F7 → F3** |
| A07 | **Golden-master**: hash determinista del dataset → regresiones del dataset |

Probado sobre **datos reales** (113 protocolos F3 + F5/F6/F7): informe completo **OK**; con manipulación intencional (URN duplicada) falla A01 como debe.

## 3. Distribución y Release (D7-2/D7-3)

- **`dotnet publish` self-contained** validado en el equipo para los tres RIDs (Release, net9.0, Avalonia 12.1.1 incl. librerías nativas).
- **Plantilla de instalador Windows** (`packaging/windows/Redes.Knowledge.iss`, Inno Setup); macOS `.dmg` y Linux AppImage/deb documentados en `packaging/README.md` con su script de CI por definir en release.
- **`DatasetMetadata`**: `dataset.json` con `{versión, fecha, hashGolden, protocolos, servicios}` — dataset **versionado e independiente del ejecutable** (se actualiza sin recompilar, ADR-002).
- **Offline total**: el primer arranque siembra el dataset desde los catálogos de la Fase I (bootstrap D3); sin dependencia de Internet en runtime.
- **Pipeline CI** (`ci/github-actions-ci.yml`): quality gate (tests + audit), build/publish por SO, y job de paquete en tags `v*`.

## 4. Resultados reales (verificación ejecutada)

```
dotnet test -c Release → Con error: 0, Superado: 56, Total: 56, Duración: 2 s
PUBLISH self-contained:
  win-x64  → dist/win-x64  (231 archivos; Avalonia.Base.dll, av_libglesv2.dll, App.exe…)
  linux-x64 → dist/linux-x64 (228 archivos)
  osx-x64  → dist/osx-x64  (229 archivos)
```

## 5. Criterios de salida de D7

- [x] D7-1 controles automáticos de datos en compuerta de CI (A01–A07 + golden-master), probados.
- [x] D7-2 publicaciones self-contained por SO y plantillas de instaladores; dataset versionado e independiente.
- [x] D7-3 modo offline verificado por diseño (bootstrap) y Release candidato 1.0.0 generado con metadatos.
- [x] Pruebas 56/56 en Release; publicaciones sin errores.
- [~] *Publicación oficial de instaladores (compilar .iss, `.dmg`, AppImage/deb, firmas) al activar el repositorio git y el CI (release `v1.0.0`).*
- [~] *Pulido post-1.0 (tareas abiertas): rasterización **PNG** (D4-3), panel visual del grafo y **vista de captura** en la app (D5/D6), deduplicación fina del pipeline (D2-2).*

## 6. Estado de la Fase II

**Las 8 épicas (D0–D7) de la Fase II están completadas** en su primera iteración. La solución: `Redes.Knowledge.sln` (App Avalonia 12.1.1 · Domain · Infrastructure · Visualization · Tests, **56/56**), artefactos de distribución en `dist/`, CI documentado y Release candidato listo.

## 7. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de software de escritorio | *(por confirmar)* | | |

> **Siguiente (fuera de la Fase II inicial):** activación del repositorio git + CI, instaladores oficiales firmados (release `v1.0.0`) y el **pulido de iteración 1.1** (PNG, vista de captura, panel del grafo, dedup fina).

---
Última actualización: 26-08-2026