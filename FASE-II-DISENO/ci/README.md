# CI/CD — Control de calidad y releases (D7-1)

Configuración de referencia: [`github-actions-ci.yml`](github-actions-ci.yml) (plantilla para GitHub Actions).

## Jobs

| Job | Plataforma | Qué hace |
|---|---|---|
| `quality` | ubuntu-latest | `dotnet test` con la **auditoría automática** (A01–A07 incl. golden-master) como compuerta |
| `build` | matriz win/linux/macos | restaura, compila y **publica self-contained por RID** (`dist/<rid>`) → artifact |
| `package` | windows-latest (release) | compila el instalador con **Inno Setup** (script `packaging/windows/`) |

## Controles automáticos de datos (compuerta de calidad §9.3 del plan, en `DatasetQuality`)

- A01 URNs únicas · A02 sin duplicados (familia, acrónimo) · A03 fichas válidas (esquema F4) ·
  A04/A05/A06 integridad referencial F5/F6/F7 → F3 · **A07 golden-master** (hash determinista del dataset; regresiones detectadas).

## Activación

1. Inicializar el repositorio git y publicar (la carpeta `FASE-II-DISENO/ci/` se mueve a `.github/workflows/ci.yml`).
2. Fijar la versión del dataset (`dataset.json`) y los runners (win/linux/macos).
3. En releases: firmas de ejecutable e instalador por SO.