# F1 — Registro Maestro de Autoridades y Fuentes

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 1 — Inventario maestro de autoridades
**Documento rector:** `PLANREDES.md` §4, §7.3, §8 (F1) y `F0-Politica-de-Fuentes.md`

| Campo | Valor |
|---|---|
| Documento | F1-Registro-de-Autoridades.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 |
| Estado | **Aprobada (26-08-2026)** |
| Depende de | F0 (aprobada el 26-08-2026) |
| Fase siguiente | F2 — Universo de dispositivos y redes · F3 — Inventario de protocolos (iniciadas) |

---

## 1. Objetivo de la fase

Construir el **catálogo de organizaciones y registros** que define *de dónde sale el universo a investigar*; formalizar el **esquema de datos de la entidad `Source`** (registro de fuente) y la **política de sincronización** de registros vivos.

Resultados esperados según `PLANREDES.md` §8 (F1): documento, catálogo, esquema de datos y/o criterios de aceptación.

## 2. Catálogo maestro de autoridades (AUTH-001…016)

Catálogo con 16 entradas. La **fuente de datos** es el archivo machine-readable [`F1-Autoridades.json`](F1-Autoridades.json) (versionado, validable y consumible por el pipeline de la Fase 3); esta tabla es su vista legible.

| ID | Organismo | Ámbito | Registros clave | Método de acceso | Frecuencia sugerida | Nivel de autoridad |
|---|---|---|---|---|---|---|
| AUTH-001 | IETF / RFC Editor | Estándares de Internet | RFC, Internet Drafts, grupos de trabajo | Datatracker + RFC Editor; descarga de índices y documentos (.txt/.xml) | Mensual | Primaria normativa |
| AUTH-002 | IANA | Registros centrales de Internet | Service Name & Transport Protocol Port Number Registry, Protocol Numbers, EtherTypes, MIME types, AS numbers | Descarga oficial; **sincronizar como datos, no copiar a mano** | Mensual | Primaria normativa (registro oficial) |
| AUTH-003 | IEEE | LAN/MAN y estándares industriales | 802.1, 802.3, 802.11, P802.15 | Get IEEE / IEEE SA | Trimestral | Primaria normativa |
| AUTH-004 | ISO/IEC | Estándares internacionales | ISO/IEC 7498, 11801 | Catálogo ISO | Bajo demanda | Primaria normativa |
| AUTH-005 | ITU-T | Telecomunicaciones | Recomendaciones X/Y/G; señalización | Catálogo ITU-T | Bajo demanda | Primaria normativa |
| AUTH-006 | 3GPP | Redes móviles | TS 23.x, 24.x, 38.x; GSM/UMTS/LTE/5G | Portal 3GPP | Mensual | Primaria normativa |
| AUTH-007 | ETSI | Telecomunicaciones europeas | GSM, NFV, MEC | Catálogo ETSI | Trimestral | Primaria normativa |
| AUTH-008 | W3C | Web (cuando corresponda) | Recomendaciones (HTTP/HTML/URL…) | Recomendaciones W3C | Trimestral | Primaria normativa |
| AUTH-009 | ICANN / registries | Dominios y direccionamiento global | Registries de TLD, policy | Páginas oficiales | Trimestral | Registro |
| AUTH-010 | Organismos industriales | Perfiles de interoperabilidad | MEF, ONF, OASIS | Sitios oficiales | Trimestral | Estándar industrial |
| AUTH-011 | NIST | Ciberseguridad y guías | SP 800-series (800-207), NIST CSF | csrc.nist.gov | Trimestral | Guía normativa de referencia |
| AUTH-012 | MITRE | Amenazas y defensa | ATT&CK, CWE, CVE | attack.mitre.org | Mensual | Base de conocimiento defensiva (complementaria) |
| AUTH-013 | DLA ASSIST / QuickSearch | Estándares militares públicos (EE. UU.) | MIL-STD-188, MIL-STD-2045, MIL-STD-6020 | quicksearch.dla.mil | Trimestral / bajo demanda | Military/Public Standard |
| AUTH-014 | Proyectos open source | Implementaciones y observabilidad | Wireshark (wsdg), libpcap/tcpdump, FRRouting | Repositorios y documentación oficiales | Trimestral | Primaria de implementación |
| AUTH-015 | Fabricantes | Documentación de implementación | Manuales, white papers, datasheets oficiales | Sitios oficiales | Bajo demanda | Primaria de implementación |
| AUTH-016 | Repositorios académicos | Investigación y estándares emergentes | IEEE Xplore, ACM DL, arXiv | Bibliotecas digitales | Trimestral | Secundaria especializada |

> **Regla de sincronización:** los registros vivos se consumen mediante un **pipeline de importación/normalización** (F3) que reconstruye el índice de forma reproducible. Nunca se copian manualmente y no se fijan en el ejecutable.

## 3. Esquema de datos de la entidad `Source` (registro de fuente)

Campos obligatorios según `F0-Politica-de-Fuentes.md` §2; este esquema es el contrato del pipeline:

| Campo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `id` | string (URN) | ✅ | `urn:source:<autoridad>.<identificador>` |
| `autoridad_id` | ref AUTH-xxx | ✅ | Organismo del catálogo §2 |
| `titulo` | string | ✅ | Título del documento |
| `url` / `uri` | string | ✅ | |
| `version` | string | ✅ | Nº de RFC/norma/revisión (número concreto, nunca "reciente") |
| `organismo` | string | ✅ | |
| `fecha_publicacion` | date | ✅ | Fecha absoluta |
| `fecha_consulta` | date | ✅ | Distinta de la publicación |
| `seccion` | string | — | Sección/página cuando sea posible |
| `nivel_autoridad` | enum 1–4 | ✅ | Jerarquía F0 |
| `confianza` | enum ALTO/MEDIO/BAJO/DESCONOCIDO | ✅ | Política F0 |
| `estado` | enum vigente/obsoleto/… | ✅ | Ciclo de vida de la fuente |
| `notas` | string | — | Observaciones |

**Ejemplo (fuente semilla R2):**

```json
{
  "id": "urn:source:ietf.rfc9114",
  "autoridad_id": "AUTH-001",
  "titulo": "HTTP/3",
  "url": "https://www.rfc-editor.org/info/rfc9114/",
  "version": "RFC 9114",
  "organismo": "IETF",
  "fecha_publicacion": "2022-06-06",
  "fecha_consulta": "2026-08-26",
  "seccion": null,
  "nivel_autoridad": 1,
  "confianza": "ALTO",
  "estado": "vigente",
  "notas": "Ejemplo de estándar IETF actual: HTTP/3 sobre QUIC"
}
```

## 4. Política de sincronización formalizada

| Elemento | Definición |
|---|---|
| **Modos de sincronización** | `manual` (documentos bajo demanda), `programada` (mensual/trimestral según §2), `pipeline` (automatizado, F3) |
| **Ciclo** | Ingestion → normalization → deduplication → entity linking → validation → indexing → **release snapshot** |
| **Snapshot** | Cada sincronización genera un artefacto inmutable: `{fecha, hash, procedencia, diff vs. previo}` |
| **Validación** | Esquema `Source` (§3), enlaces vivos, deduplicación por URN, integridad referencial |
| **Rollback** | Posible contra el snapshot anterior (los registros vivos no se pierden al actualizar) |
| **Estado por registro** | `pendiente` · `sincronizado` · `desactualizado` · `error` (registrado en el pipeline) |
| **Regla fija** | Los registros vivos (IANA, RFC, MIL-STD…) son **datos versionables**; fecha de consulta obligatoria en cada uso |

## 5. Fuentes semilla verificadas (R1–R11)

Fuentes ya verificadas en el plan (`PLANREDES.md` §18) con fecha de consulta 26-08-2026; serán el **primer lote** del registro de fuentes del pipeline (F3):

| ID | Fuente | URL | Nivel | Confianza |
|---|---|---|---|---|
| R1 | IANA — Service Name and Transport Protocol Port Number Registry | https://www.iana.org/assignments/service-names-port-numbers | 1 | ALTO |
| R2 | RFC Editor — RFC 9114 HTTP/3 | https://www.rfc-editor.org/info/rfc9114/ | 1 | ALTO |
| R3 | Wireshark Developer’s Guide | https://www.wireshark.org/docs/wsdg_html/ | 2 | ALTO |
| R4 | NIST SP 800-207 — Zero Trust Architecture | https://csrc.nist.gov/pubs/sp/800/207/final | 1 | ALTO |
| R5 | MITRE ATT&CK | https://attack.mitre.org/ | 3 (complementaria) | ALTO |
| R6 | DLA ASSIST — MIL-STD-188 | https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=35582 | 1 (military/public) | ALTO |
| R7 | DLA ASSIST — MIL-STD-2045 | https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=117743 | 1 (military/public) | ALTO |
| R8 | DLA ASSIST — MIL-STD-6020 | https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=215906 | 1 (military/public) | ALTO |
| R9 | Avalonia Docs | https://docs.avaloniaui.net/docs/get-started/ | 2 | ALTO |
| R10 | Avalonia Supported Platforms | https://docs.avaloniaui.net/docs/supported-platforms | 2 | ALTO |
| R11 | Electron Docs | https://www.electronjs.org/docs/latest/ | 2 | ALTO |

## 6. Criterios de salida / aceptación de F1

- [x] Catálogo de autoridades completo con URLs (AUTH-001…016) y fecha de consulta.
- [x] Esquema de la entidad `Source` (sección 3) aprobado y validable.
- [x] Política de sincronización definida (modos, snapshot, validación, rollback).
- [x] Fuentes semilla R1–R11 incorporadas al registro.
- [x] Catálogo machine-readable `F1-Autoridades.json` versionado y JSON válido.

## 7. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Especialista en estándares | *(por confirmar)* | | |

> La aprobación de F1 desbloquea **F2 — Universo de dispositivos y redes** y **F3 — Inventario de protocolos** (ambas iniciadas el 26-08-2026).

---
Última actualización: 26-08-2026