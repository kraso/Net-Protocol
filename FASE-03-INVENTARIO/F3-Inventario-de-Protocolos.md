# F3 — Inventario Maestro de Protocolos

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 3 — Inventario de protocolos
**Documento rector:** `PLANREDES.md` §7, §8 (F3), §9.2 · `F1-Registro-de-Autoridades.md` · `F0-Ejes-de-Clasificacion.md`

| Campo | Valor |
|---|---|
| Documento | F3-Inventario-de-Protocolos.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 |
| Estado | **Aprobada (26-08-2026)** |
| Depende de | F0 (aprobada), F1 (aprobada), F2 (aprobada) |
| Fase siguiente | F4 — Profundización protocolar (en curso) · F5 — Mensajería y PDU (en curso) |

---

## 1. Objetivo de la fase

Recolectar, normalizar, deduplicar y versionar los protocolos por familia y dominio (plan §8 F3). **El registro de IANA de nombres de servicio y puertos se sincroniza como fuente de datos, nunca se copia a mano**. Esta fase produce: inventario maestro (semilla), diseño operativo del pipeline IANA y métricas de cobertura base.

## 2. Inventario maestro de protocolos

### 2.1. Proceso del inventario

| Etapa | Regla |
|---|---|
| **Recolección** | Desde el catálogo de autoridades (F1, AUTH-001…016) y los ejes/familias de la F0. IANA como fuente de datos para servicios/puertos de transporte. |
| **Normalización** | Convención de claves: `PR-XXX` asignado por el pipeline; URN estable por protocolo (p. ej. `urn:proto:ietf:rfc9114`). Campos mínimos del catálogo: id, acrónimo, nombre, familia, estado, capas, fuente. |
| **Deduplicación** | Por norma: acrónimo + familia + referencia de estándar (RFC/ISO/IEEE…). Duplicados (p. ej. BGP vs. BGP-4) se resuelven por alias, no por doble registro. |
| **Versionado** | Cada `Version` con `valid_from/valid_to`; los cambios de RFC se registran con número concreto (RFC n → RFC m). |
| **Fuentes** | Campo `fuente` del catálogo en estado `pendiente` hasta que el pipeline asigne `Source` (R1–R11 + nuevas). |

### 2.2. Semilla del inventario (111 protocolos, 13 familias)

El catálogo machine-readable [`F3-Protocolos.json`](F3-Protocolos.json) contiene la semilla. Resumen por familia:

| Familia | Código | Nº en semilla | Ejemplos |
|---|---|---|---|
| Acceso y enlace | ACEL | 10 | Ethernet, Wi-Fi, PPP, L2TP, 802.1Q, STP/RSTP/MSTP, LACP |
| Direccionamiento, descubrimiento y configuración | ADCONF | 9 | IPv4, IPv6, ARP, NDP, DNS, DHCP, mDNS, LLMNR |
| Routing y forwarding | ROUT | 12 | OSPF, BGP, IS-IS, EIGRP, IGMP, PIM, MPLS, SR |
| Movilidad | MOV | 4 | GTP, Mobile IP, MIPv6, LISP |
| Transporte y sesión | TRAN | 7 | TCP, UDP, SCTP, DCCP, QUIC, RTP/RTCP |
| Aplicación | APP | 11 | HTTP/1.1–2–3, SMTP, FTP, SSH, SMB, NFS, SIP, XMPP |
| Gestión, monitorización y operaciones | GEST | 11 | SNMP, NETCONF, RESTCONF, gRPC, IPFIX, syslog, RADIUS, ICMP |
| Sincronización temporal | SYNC | 2 | NTP, PTP (IEEE 1588) |
| Almacenamiento/red y automatización | STOR | 4 | iSCSI, Fibre Channel, FCoE, NVMe-oF |
| Seguridad | SEG | 11 | IPsec/IKE, TLS/DTLS, Kerberos, EAP, 802.1X, DNSSEC, GRE, VXLAN, WireGuard |
| IoT/OT y tiempo real | IOT | 10 | MQTT, CoAP, Modbus, DNP3, PROFINET, EtherCAT, OPC UA, LoRaWAN, Zigbee, BACnet |
| Radio/móvil y satélite | RAD | 9 | GSM, UMTS, LTE, 5G NR, TETRA, DMR, DVB-S2, Link 16/11 (military/public) |
| Históricos y de transición | HIST | 11 | X.25, Frame Relay, ATM, Token Ring, FDDI, IPX/SPX, AppleTalk, SONET/SDH, ISDN |

> La semilla **no es una lista cerrada**: el pipeline IANA y las familias ampliarán el inventario de forma medible (sección 4).

## 3. Pipeline de sincronización IANA (diseño operativo)

### 3.1. Fuente y formato

- **Fuente:** IANA — Service Name and Transport Protocol Port Number Registry (`https://www.iana.org/assignments/service-names-port-numbers`), última actualización del registro según el plan: 17-08-2026.
- **Formato de entrada (CSV/plain-text oficial):** columnas del registro IANA — service name · port · transport protocol · description · assignee · contact · registration date · modification date · reference · status (y variantes oficiales).

### 3.2. Mapeo a datos del proyecto

| Registro IANA | Entidad/salida del pipeline | Notas |
|---|---|---|
| service name | `Protocol.acronimo` / alias | Normalizar a minúsculas |
| port + transport protocol | `Protocol.identificadores` (puertos, transportes) | Un servicio puede tener varios pares puerto/transporte |
| description | `Source`/`Protocol.nota` | Sin inferir semántica fuera de lo registrado |
| reference | `Source.url` (vinculación a RFC/autoridad) | |
| status (Registered/Unassigned/etc.) | ciclo de vida (`F0-Ejes` §3) | `Registered` → vigente; marcas especiales documentadas |

### 3.3. Etapas del pipeline

```
ingestion → normalization → deduplication → entity linking → validation → indexing → release snapshot
```

| Etapa | Comportamiento |
|---|---|
| Ingestion | Descarga del registro oficial con **fecha de consulta** obligatoria |
| Normalization | Case/encoding, separación de pares puerto/transporte, eliminación de ruido |
| Deduplication | Clave (service name, transporte); alias registrados sin duplicar entidades |
| Entity linking | Vínculo con familia/estado del inventario maestro y con `Source` |
| Validation | Esquema `Source` (F1 §3), enlaces vivos, integridad referencial |
| Indexing | Actualización de índices FTS5 (para búsqueda) |
| Release snapshot | Artefacto inmutable `{fecha, hash, procedencia, diff vs. previo}` + rollback |

### 3.4. Reglas de validación aplicadas al pipeline

1. **Puerto ≠ protocolo:** un puerto registrado (p. ej. 80) **no prueba** que el tráfico corresponda al servicio registrado (advertencia IANA). El catálogo distingue "registrado en IANA" de "uso real verificado".
2. **Protocolo ≠ servicio:** la entrada de IANA describe un *servicio*; el vínculo con el `Protocol` se hace solo cuando la referencia lo justifica.
3. **No inferir wire format** desde el registro IANA (solo referencia).
4. Los estados y fechas del registro se conservan como **datos versionables**.

### 3.5. Fixture de ejemplo (salida normalizada esperada)

| service name | port | transporte | descripción (abreviada) | estado mapeado | fuente |
|---|---|---|---|---|---|
| ssh | 22 | TCP | Secure Shell | vigente (registrado) | IANA 2026-08-17 |
| http | 80 | TCP | HTTP | vigente (registrado) | IANA 2026-08-17 |
| dns | 53 | TCP/UDP | DNS | vigente (registrado) | IANA 2026-08-17 |

> Ejemplo ilustrativo del diseño. La **verificación operativa** (volcado real, conteos, diff) se realiza con la primera corrida del pipeline en la validación F3/F8, y la implementación de software en la fase de producto (hitos D1/D5 del plan).

## 4. Métricas de cobertura base

Métricas definidas en `PLANREDES.md` §7.2; estado inicial con la semilla (objetivo: medirse en cada release):

| Métrica | Definición | Estado inicial | Objetivo (referencia) |
|---|---|---|---|
| Cobertura por organización/registro | % de ítems del registro presentes | 0 % (pipeline pendiente) | Medir en cada snapshot |
| Cobertura por familia de protocolos | % de familias con fichas completas | Semilla 13/13 familias | 100 % de fichas N1+ |
| Cobertura por capa | % de protocolos por capa | Semilla clasificada | — |
| Cobertura por dominio de red | % por dominio | Pendiente F4–F7 | — |
| % fichas con fuente primaria | Afirmación principal con fuente nivel 1 | 0 % (fuentes pendientes) | ≥ 90 % |
| % con wire format documentado | Estructura pública documentada | 0 % (se mide en F5) | ≥ 70 % en prioritarios |
| % con diagrama | Diagrama regenerable | 0 % (se mide en F4/F5) | 100 % prioritarios |
| % con implementación verificada | "Implementa" confirmado | Pendiente | ≥ 80 % |
| % con fecha de revisión reciente | Revisado en los últimos 12 meses | Semilla 26-08-2026 | 100 % |

## 5. Registro de incertidumbres inicial (F3)

| ID | Entidad | Campo/afirmación | Naturaleza | Fuentes consultadas | Decisión / estado |
|---|---|---|---|---|---|
| U-0001 | Protocolo (p. ej. protocolo propietario) | Wire format no publicado | `[n.p.d.]` | Documentación parcial del fabricante | Registrar nombre/ámbito; marcar "documentación pública insuficiente" |
| U-0002 | Servicio IANA | Puerto registrado ≠ uso real | Conflicto aparente (puerto vs. protocolo) | Registro IANA (R1) | Aplicar regla §3.4.1; no inferir protocolo desde el puerto |

## 6. Criterios de salida / aceptación de F3

- [x] Inventario maestro de protocolos semilla por familia y estado (111 protocolos, 13 familias) — `F3-Protocolos.json`.
- [x] Proceso de normalización/deduplicación/versionado definido (§2).
- [x] Diseño del pipeline IANA operativo (fuente, mapeo, etapas, validaciones, fixture) (§3).
- [x] Métricas de cobertura base definidas y medibles (§4).
- [x] Registro de incertidumbres iniciado (§5).
- [~] Verificación operativa del pipeline con datos reales de IANA: **planificada** (primera corrida en validación F3/F8; la implementación es fase de producto). Tarea registrada, no bloqueante para F4/F5.
- [x] Aprobación de la fase (sección 7).

## 7. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de conocimiento | *(por confirmar)* | | |

> **Estado:** la aprobación de F3 habilita la profundización **F4 — Profundización protocolar** (fichas normalizadas) y **F5 — Mensajería y PDU** (modelado de campos y formatos), ambas en curso.

---
Última actualización: 26-08-2026