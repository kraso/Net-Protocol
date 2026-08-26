# Empaquetado y Distribución (D7-2)

Estrategia aprobada en el plan de Fase II (§E) y materiales de esta carpeta:

| SO | Formato objetivo | Herramienta | Estado |
|---|---|---|---|
| Windows | Instalador `.exe` | **Inno Setup** (script: [`windows/Redes.Knowledge.iss`](windows/Redes.Knowledge.iss)) | Plantilla lista; ejecutar en CI/release |
| macOS | `.dmg` | `hdiutil` (en runner macOS) | Script CI por definir en release |
| Linux | AppImage / `.deb` | AppImageKit / dpkg-deb | Plantilla por definir en release |

## Reglas vinculantes (del plan)

1. **Dataset versionado con semver propio** e independiente del ejecutable (`dataset.json` junto a la base de datos local; ver `Quality/DatasetMetadata`).
2. **Actualización sin recompilar**: el ejecutable lee el dataset desde datos versionables; el pipeline (D2) regenera snapshots.
3. **Modo offline completo**: el dataset se embebe en el primer arranque desde los catálogos de la Fase I (bootstrap D3).
4. Publicación **self-contained por RID** (win-x64, linux-x64, osx-x64) validada localmente en D7 (carpeta `dist/`).

## Pasos de release (checklist)

1. `dotnet test` verde (quality gate con golden-master A07).
2. `dotnet publish -c Release -r <rid> --self-contained true -o dist/<rid>`.
3. Firma/código y firma del instalador (firmas requeridas por SO — configurar en CI).
4. Publicar instaladores; adjuntar `dataset.json` (versión, hash golden, conteos).