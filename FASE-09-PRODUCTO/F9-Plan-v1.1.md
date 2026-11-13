# v1.1 — Datos y actualizador

**Estado:** alcance aprobado el 13-11-2026. Sin firma de código en esta versión.
**Base publicada:** v1.0.14 · `25b1e87` (P0–P2 cerrados, CI verde, 124 + 55 pruebas).

## Problema que resuelve

El catálogo es el producto, y hoy tiene huecos: de los 28 protocolos con layout F5 solo 10
tienen captura real, la tabla de servicios IANA queda vacía al arrancar y diez clases no llegan
a tres fichas. Además, actualizar exige ir a GitHub, descargar a mano y ejecutar un binario sin
comprobar que es el que se publicó.

v1.1 cierra los huecos de datos y convierte el aviso de actualización en una **descarga
verificada** que el usuario decide instalar.

## Decisiones tomadas

| # | Decisión | Motivo |
|---|---|---|
| D1 | v1.1 = datos + actualizador | Es lo que hace útil el producto sin pedir dinero ni cuentas de desarrollador |
| D2 | Sin firma de código ni notarización | Apple 99 USD/año + certificado Windows 200–400 USD/año; se reevalúa cuando haya usuarios reales |
| D3 | El actualizador descarga, verifica SHA256 y **nunca instala solo** | Sin firma de código, ejecutar automáticamente un binario descargado es peor que no actualizar |

## Especificaciones

### S1 — Checksums publicados en cada release

El job `release` genera `SHA256SUMS` con el hash de todos los instaladores y lo adjunta a la
release, junto a los binarios que ya publica.

**Aceptación:** al crear un tag `v*`, la release contiene `SHA256SUMS`; descargados los assets,
`sha256sum -c SHA256SUMS` devuelve OK en las cinco plataformas.

### S2 — Descarga verificada desde la app

Sobre el aviso que ya existe (`IUpdateChecker` + `UpdateText`): se elige el asset según el RID
en ejecución, se descarga a un directorio temporal, se verifica el SHA256 contra `SHA256SUMS`
y se ofrece «Descargar instalador» y «Abrir release». Nunca se ejecuta nada sin que el usuario
lo pida.

**Aceptación:**
- Con `SHA256SUMS` manipulado, la app rechaza el archivo y muestra el error en `UpdateText`; no deja nada ejecutable en temporal.
- Con el hash correcto, el archivo queda en temporal y la ruta se muestra; el instalador solo se abre si el usuario pulsa.
- Si el RID no tiene asset, el aviso degrada al enlace de la release (comportamiento actual).
- Si la red cae, no se propaga excepción: se informa y se deja el enlace.
- Pruebas: hash válido, hash inválido, asset ausente, SHA256SUMS ausente, red caída.

### S3 — Servicios IANA importados en el arranque

Hoy `Services` queda vacía en una base nueva y la vista de servicio IANA nunca aparece. El
arranque importa el catálogo IANA junto a F3.

**Aceptación:** base nueva → `Servicios.Contar() > 0`; una segunda carga no duplica filas; la
barra de estado muestra el número real; la búsqueda por nombre de servicio sigue funcionando.

### S4 — Capturas reales para los 18 protocolos que solo tienen muestra sintética

**Bloqueada:** falta decidir la fuente de los PCAP. Ver «Decisiones pendientes».

**Aceptación:** cada protocolo pasa de `sintetica` a `real` en el informe F8 con el fichero,
la fuente y la licencia anotados; la prueba de corpus valida el layout F5 contra la captura.

### S5 — Al menos tres fichas por clase en las diez clases no prioritarias (L-002)

**Aceptación:** el informe F8 muestra el conteo por clase con ≥ 3 en las diez; cada ficha
nueva pasa `SourceValidator` con fuente de nivel 1 o 2 declarada.

## Tareas

| ID | Spec | Qué | Ruta | Bloqueo |
|---|---|---|---|---|
| T1 | S1 | Generar y adjuntar `SHA256SUMS` en el job `release` | `.github/workflows/github-actions-ci.yml` | — |
| T2 | S2 | Selección de asset por RID y descarga a temporal | `Infrastructure/Updates/` | T1 |
| T3 | S2 | Verificación SHA256 y aviso en `UpdateText` | `Infrastructure/Updates/` + `MainWindow` | T2 |
| T4 | S3 | Importación IANA en el arranque | `Infrastructure/`, `ArranqueCatalogo` | — |
| T5 | S4 | 18 capturas reales con fuente y licencia | `FASE-08-VALIDACION/` | **fuente sin decidir** |
| T6 | S5 | Fichas por clase | `FASE-04-PROFUNDIZACION/` | alcance de contenido |

T1–T4 no dependen de nadie. T5 y T6 sí.

## Decisiones pendientes

- **S4/T5 — fuente de los PCAP.** Opciones: (a) capturas propias del responsable; (b) capturas
  públicas de terceros con licencia compatible (wiki de Wireshark y similares), anotando fuente
  y licencia por fichero; (c) dejarlo fuera de v1.1 y publicar v1.1 solo con S1–S3.
- **S5/T6 — profundidad de las fichas.** Cuántas clases entran y con qué nivel de detalle.

## Fuera de alcance (v2.0)

- Simulación de protocolos: el producto es referencia y exploración, no simulación.
- Firma de código y notarización (D2).
- Idiomas más allá de es/en.

## Riesgos

- **S2 sin firma de código:** el SHA256 garantiza integridad del transporte, no autoría. Si
  alguien controla la release, controla el hash. Se documenta en la UI y en `SECURITY.md`.
- **S4/S5 dependen de datos y contenido externos:** pueden retrasar la versión; no bloquean S1–S3.
