# CI/CD — Control de calidad y releases (D7-1)

> **Fuente de verdad única:** [`.github/workflows/github-actions-ci.yml`](../../.github/workflows/github-actions-ci.yml).
> La antigua plantilla `github-actions-ci.yml` de este directorio se retiró el 13-11-2026: había
> divergido del workflow real (51 líneas frente a 535) y inducía a error. No se duplica el YAML aquí.

## Jobs del workflow real

| Job | Disparador | Plataforma | Qué hace |
|---|---|---|---|
| `quality` | push, pull_request | ubuntu-latest | `dotnet test` en Release con la **auditoría automática** (A01–A07 incl. golden-master) como compuerta |
| `build` | push, pull_request | matriz win-x64 / linux-x64 / osx-x64 | `dotnet publish` self-contained por RID → artifact con retención de 2 días |
| `package-win` | tags `v*` | windows-latest | instalador Inno Setup (`packaging/windows/Redes.Knowledge.iss`) |
| `package-linux` | tags `v*` | ubuntu-latest | `.deb` + `.rpm` + AppImage, firmados con GPG, con `.desktop`, icono y comando `netprotocol` |
| `package-macos` | tags `v*` | macos-latest | `.dmg` |
| `release` | tags `v*` | ubuntu-latest | publica los assets y las notas de instalación por SO |

## Controles automáticos de datos (compuerta de calidad §9.3 del plan, en `DatasetQuality`)

- A01 URNs únicas · A02 sin duplicados (familia, acrónimo) · A03 fichas válidas (esquema F4) ·
  A04/A05/A06 integridad referencial F5/F6/F7 → F3 · **A07 golden-master** (hash determinista del dataset; regresiones detectadas).

## Convenciones de release

1. Bump de `<Version>` en `Redes.Knowledge.App.csproj` y en `packaging/windows/Redes.Knowledge.iss`.
2. Commit `vX.Y.Z: <resumen>` y tag anotado `vX.Y.Z` sobre ese commit.
3. Push del tag → el workflow construye y publica los instaladores; verificar el run en verde.
4. Registrar la publicación en `REGISTRO/Bitacora.md` y en `CHANGELOG.md`.
