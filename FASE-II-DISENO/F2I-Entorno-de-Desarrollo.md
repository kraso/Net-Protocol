# F2I — Entorno de Desarrollo (Fase II)

**Documento:** F2I-Entorno-de-Desarrollo.md · **Fecha:** 26-08-2026 · **Estado:** verificado (datos detectados en el equipo)

## 1. Herramientas instaladas (detectadas el 26-08-2026)

| Herramienta | Detectada | Notas |
|---|---|---|
| **Visual Studio Enterprise 2022** | ✅ 17.14.37502.11 — `C:\Program Files\Microsoft Visual Studio\2022\Enterprise` | IDE principal del proyecto |
| **VS Build Tools 2019** | ✅ 16.11.37507.1 — `...\2019\BuildTools` | Respaldo de builds MSBuild/VCTools |
| **.NET SDK** | ✅ 9.0.316 — `C:\Program Files\dotnet\sdk` | Target base: **net9.0** |
| **git** | ✅ 2.54.0.windows.1 | Control de versiones |

## 2. Workloads y componentes de VS 2022 (vswhere + state.json)

| Workload (ID) | Estado | Utilidad para el proyecto |
|---|---|---|
| Microsoft.VisualStudio.Workload.ManagedDesktop | ✅ Instalada | Desarrollo .NET de escritorio (base de la app) |
| Microsoft.VisualStudio.Workload.NetWeb | ✅ Instalada | Herramientas web/.NET (soporte para RESTCONF/gRPC si se usan) |
| Microsoft.VisualStudio.Workload.Azure | ✅ Instalada | Utilidad marginal (no previsto) |
| Microsoft.VisualStudio.Workload.NativeDesktop (C++/MSVC) | ✅ Instalada | Soporte para dependencias nativas si el adaptador PCAP lo requiere |
| Microsoft.VisualStudio.Workload.Universal | ✅ Instalada | No previsto en el producto |
| Microsoft.VisualStudio.Workload.NetCrossPlat / NativeCrossPlat | ✅ Instaladas | Soporte cross-platform |
| Microsoft.VisualStudio.Workload.DataScience / Python | ✅ Instaladas | Utilidad marginal (análisis auxiliar) |
| Componente MSVC x86/x64 | ✅ Instalado | Herramientas de C++ disponibles |

## 3. Decisiones de tooling para el proyecto

| Tema | Decisión propuesta | Verificación en |
|---|---|---|
| Target framework | **net9.0** (SDK 9.0.316 instalado) | D0 (`dotnet --info`, `dotnet new list`) |
| Framework UI | **Avalonia** (XAML/MVVM) — fijar versión estable compatible en D0 | D0 (spike D0-1) |
| Persistencia | SQLite + FTS5 (Paquete NuGet a fijar en D1; Dapper o SQL crudo a decidir en D1) | D1 |
| Build reproducible | **CLI `dotnet`** como fuente de verdad para CI (independiente del IDE) | D1/D7 |
| IDE | VS 2022 Enterprise (IntelliSense, depurador, perfiles) — complementario al CLI | Todo |
| Control de versiones | git + Conventional Commits (sugerido) · ramas por épica (D0…D7) | D0 |
| Tests | xUnit o MS Test vía templates del SDK; Avalonia.Headless evaluado en D0-1 | D3/D7 |

## 4. Notas

1. La **nota del responsable** sobre VS 2022 queda registrada: el IDE con sus paquetes de herramientas está disponible y se usará como entorno principal durante el desarrollo (épicas D1–D7).
2. No se asume ningún componente **no** detectado: si una dependencia real lo exige (p. ej. librería nativa para PCAP), se valida su instalación con la herramienta correspondiente (Visual Studio Installer / `winget`) en la épica que la necesite.
3. Cualquier requisito de tooling adicional se registra en la bitácora antes de instalarse (trazabilidad).

---
Última actualización: 26-08-2026