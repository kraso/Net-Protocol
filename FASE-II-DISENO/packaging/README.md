# Empaquetado y Distribución (D7-2)

Estrategia aprobada en el plan de Fase II (§E) y materiales de esta carpeta:

| SO | Formato objetivo | Herramienta | Estado |
|---|---|---|---|
| Windows | Instalador `.exe` | **Inno Setup** (script: [`windows/Redes.Knowledge.iss`](windows/Redes.Knowledge.iss)) | ✅ En CI (tag `v*`) |
| macOS | `.dmg` + `.app` bundle | `sips` + `iconutil` (`.icns`) + `hdiutil` (en runner macOS) | ✅ En CI (tag `v*`) |
| Linux | `.deb` + `.rpm` + `.AppImage` | `dpkg-deb` / `rpmbuild` / `appimagetool` (en runner Ubuntu) | ✅ En CI (tag `v*`) |

Los tres instaladores se generan en CI al empujar un tag `v*` y se publican como
**GitHub Release** (job `release` del workflow `github-actions-ci.yml`).

## Patrón unificado de nombres

Todos los instaladores comparten el formato **`NetProtocol-<versión>-<SO>-<arquitectura>.<extensión>`**
y cada instalador Linux lleva su firma adjunta como **`<instalador>.asc`**:

| Instalador | Archivo | Firma adjunta |
|---|---|---|
| Windows (Inno Setup) | `NetProtocol-<ver>-win-x64.exe` | — (sin firma de código) |
| Debian/Ubuntu | `NetProtocol-<ver>-linux-amd64.deb` | `…-linux-amd64.deb.asc` |
| Fedora/RHEL/openSUSE | `NetProtocol-<ver>-linux-x86_64.rpm` | `…-linux-x86_64.rpm.asc` + firma integrada `rpmsign` |
| Linux portable | `NetProtocol-<ver>-linux-x86_64.AppImage` | `…-linux-x86_64.AppImage.asc` |
| macOS | `NetProtocol-<ver>-macos-x64.dmg` | — (sin firma de código) |

> El token de arquitectura usa la convención nativa de cada ecosistema
> (`amd64` para .deb, `x86_64` para .rpm/.AppImage, `x64` para exe/dmg).
> `NetProtocol-gpg-pubkey.asc` **no** sigue el patrón de versión: es la clave
> pública de releases, la misma en todas las versiones, con nombre estable para
> que los comandos de importación no cambien entre releases.

**Entrada de menú e icono (Linux):** tanto el `.deb`, `.rpm` como `.AppImage` instalan la
entrada de menú (`/usr/share/applications/netprotocol.desktop`), el icono
(`/usr/share/pixmaps/netprotocol.png`, desde `src/Redes.Knowledge.App/Assets/Logo_NetProtocol.png` — PNG 256×256,
estándar freedesktop) y un comando en el PATH: **`netprotocol`** (un **wrapper** que
lanza `/usr/lib/netprotocol/NetProtocol` con `DOTNET_EnableDiagnostics=0` — sin duplicar
binarios). El `Exec` del menú usa la ruta absoluta `/usr/bin/netprotocol`. La AppImage
lleva su propio icono (`usr/share/icons/hicolor/256x256/apps/netprotocol.png`) y un
`AppRun` equivalente en la raíz del AppDir.

> **Ojo con el bit de ejecución:** `actions/download-artifact@v4` pierde el modo
> de ejecución al descargar el binario (problema conocido del action). Por eso
> el empaquetado **fija `0755` explícitamente** en los apphosts ELF
> (`NetProtocol`, `createdump`) en el `%install`/`debroot` — nunca dependas del
> mode del artefacto. El `.rpm` además refresca el menú en `%post`
> (`update-desktop-database` + `kbuildsycoca6`, best-effort) para que la entrada
> aparezca sin re-login en KDE.

**URL ascendente en la metainformación de los paquetes:** el `.rpm`
(`URL:` del spec), el `.deb` (`Homepage:` del control) y el instalador Windows
(`AppPublisherURL` de Inno Setup) publican la URL del repositorio actual. Se
deriva en CI de `GITHUB_REPOSITORY` (`https://github.com/${GITHUB_REPOSITORY}`),
que GitHub Actions mantiene siempre a `owner/nombre` actuales: **si se renombra
el repositorio, los próximos paquetes llevan la URL correcta sin tocar nada**
(no hay URLs hardcodeadas; el `.iss` conserva el valor actual solo como
`#define` por defecto para builds locales).

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

## Runtime sin dependencias nativas (Linux)

El runtime .NET self-contained carga `lttng-ust`/`urcu` para diagnósticos/trazado
(EventPipe) **solo de forma perezosa**, pero eso obligaba a declarar
`liblttng-ust0`/`liburcu6` como dependencias del paquete — incómodo en distros
con versiones antiguas (p. ej. Ubuntu 20.04 tiene `liblttng-ust0` 2.11.0,
inferior al mínimo exigido). Para **no** depender de esas librerías:

- El `.deb` y el `.rpm` instalan un **wrapper** en `/usr/bin/netprotocol` que
  exporta `DOTNET_EnableDiagnostics=0` y ejecuta el binario con `exec`.
- La `.AppImage` lleva un **`AppRun`** equivalente en la raíz del AppDir.

Consecuencia: **los paquetes ya no declaran `Depends`/`Requires` nativos** y se
instalan en cualquier distro sin librerías adicionales. La traza de diagnóstico
queda desactivada (no afecta a la funcionalidad de la app).

> Para recuperar la traza: lanzar el binario directamente
> (`/usr/lib/netprotocol/NetProtocol`) o quitar la variable del wrapper.

## Firma GPG de los instaladores Linux

La firma se hace en CI (job `package-linux`) **solo si existen los secretos**;
sin ellos el job avisa (`::warning::`) y publica sin firmar. La misma clave GPG
firma el `.rpm` (firma integrada con `rpmsign` **más** firma adjunta `.asc`) y los
`.deb`/`.AppImage` con firma adjunta `.asc` estándar (`gpg --detach-sign --armor`,
siguiendo el patrón `<instalador>.asc`). `dpkg-sig`, el firmador clásico del `.deb`,
fue **retirado de los repos de Ubuntu/Debian**; la forma canónica de apt sigue
siendo un repositorio con `Release` firmado, ver abajo).

### 1. Generar la clave (una sola vez, en tu equipo)

```bash
gpg --full-generate-key        # RSA 4096, cédula 3 años, id. "Net Protocol Releases <email>"
gpg --list-secret-keys --keyid-format=long   # anota el KEY_ID (ej. 9E3F...)
gpg --armor --export-secret-keys KEY_ID > netprotocol-signing.asc   # PRIVADA: no compartir
```

Para CI lo más simple es una clave **sin frase de contraseña** (el runner es
efímero). Si la clave tiene frase, añade el secreto `GPG_PASSPHRASE` y el
workflow la pre-carga en el agente (automático).

### 2. Configurar secretos del repositorio

Settings → Secrets and variables → Actions (o `gh secret set`):

| Secreto | Valor |
|---|---|
| `GPG_PRIVATE_KEY` | contenido de `netprotocol-signing.asc` (armored) |
| `GPG_KEY_ID` | el KEY_ID largo (ej. `9E3F4A2B...`) |
| `GPG_PASSPHRASE` | **obligatorio** si la clave tiene frase; si falta, el job falla con error claro |

### 3. Qué produce CI cuando la clave está configurada

- `.rpm` firmado (`rpmsign --addsign`) + **verificación obligatoria** (`rpm --checksig`); si falla, el job falla.
- `.deb`, `.rpm` y `.AppImage` con firma adjunta `<instalador>.asc` (`gpg --detach-sign --armor`) + verificación (`gpg --verify`).
- Se adjunta al release la clave pública `NetProtocol-gpg-pubkey.asc` (misma clave en todas las versiones).

### 4. Verificación por parte de quien instala

```bash
# RPM (Fedora/RHEL/openSUSE) — importar la clave ANTES de instalar:
wget -O NetProtocol-gpg-pubkey.asc \
  https://github.com/kraso/Net-Protocol/releases/download/v1.0.13/NetProtocol-gpg-pubkey.asc
gpg --import NetProtocol-gpg-pubkey.asc      # comprobación manual
sudo rpm --import NetProtocol-gpg-pubkey.asc # la importa para rpm/zypper/dnf
rpm -Kv NetProtocol-1.0.13-linux-x86_64.rpm        # -> "digests signatures OK"

sudo zypper install ./NetProtocol-1.0.13-linux-x86_64.rpm   # openSUSE
# sudo dnf install ./NetProtocol-1.0.13-linux-x86_64.rpm    # Fedora/RHEL
```

> **NOKEY / "la clave pública de firma no está disponible"** al primer `zypper install`
> es normal: la máquina aún no conoce la clave del firmante. No es un paquete
> dañado → importar la clave como arriba (una sola vez por equipo; las versiones
> siguientes con la misma clave se instalan sin preguntar). Atajo:
> `sudo zypper --gpg-auto-import-keys install ./…rpm` (zypper pregunta y la importa).

```bash
# DEB (Debian/Ubuntu): firma adjunta .asc
gpg --verify NetProtocol-1.0.13-linux-amd64.deb.asc NetProtocol-1.0.13-linux-amd64.deb

# AppImage (portable): misma verificación con .asc adjunta
gpg --verify NetProtocol-1.0.13-linux-x86_64.AppImage.asc NetProtocol-1.0.13-linux-x86_64.AppImage

# Forma canónica completa para apt (repositorio firmado, pendiente)
# Publicar en un repo apt con Release.gpg firmado (apt-ftparchive/reprepro) e
# instalar el .deb desde él: apt verifica la cadena de confianza automáticamente.
```