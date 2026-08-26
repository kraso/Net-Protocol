# F4 — Profundización Protocolar

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 4 — Profundización protocolar
**Documento rector:** `PLANREDES.md` §6.2, §8 (F4) · Plantilla `PLANTILLAS/plantilla-ficha-protocolo.md`

| Campo | Valor |
|---|---|
| Documento | F4-Profundizacion-Protocolar.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 |
| Estado | **Aprobada (26-08-2026)** |
| Depende de | F2 (aprobada), F3 (aprobada) |
| Fase siguiente | F5 — Mensajería y PDU (aprobada) · F6 — Seguridad y operatividad (en curso) |

---

## 1. Objetivo de la fase

Para cada protocolo, **completar la ficha normalizada** (plantilla de 18 campos), **resolver dependencias, encapsulación y versiones**, y **asociar estándares y capturas representativas** (plan §8 F4). Esta fase produce: plantilla de ficha, fichas prioritarias completas y matriz de encapsulación/dependencias.

## 2. Priorización de protocolos (primer ciclo)

| # | Protocolo | Motivo de prioridad | Ficha |
|---|---|---|---|
| 1 | TCP | Transporte base de Internet | ✅ `F4-Fichas-Prioritarias.md` (F-01) |
| 2 | UDP | Transporte base de Internet | ✅ (F-02) |
| 3 | IPv4 | Red base | ✅ (F-03) |
| 4 | IPv6 | Red base/actual | ✅ (F-04) |
| 5 | ARP | Mapeo L2/L3 | ✅ (F-05) |
| 6 | DNS | Servicio de nombres esencial | ✅ (F-06) |
| 7 | DHCP | Configuración automática | ✅ (F-07) |
| 8 | TLS | Seguridad de las aplicaciones | ✅ (F-08) |
| 9 | HTTP/3 | Estándar IETF actual (fuente R2) | ✅ (F-09) |
| 10 | BGP | Routing interdominio | ✅ (F-10) |
| 11 | OSPF | Routing intra-dominio | ⬜ tarea registrada (campos/mensajes en F5; ficha completa en F8) |
| 12 | Ethernet/802.3 | Enlace universal | ⬜ tarea registrada (campos en `F5-Campos-PDU.json`; ficha completa en F8) |

**Estado: 10/12 prioritarios con ficha completa (83 %); 2 registrados como tarea de F8** (sus datos de campos/mensajes se entregan en F5). Los 99 protocolos restantes de la semilla se profundizan en ciclos posteriores de F4 (por familia, priorizando los de valor operativo alto).

## 3. Proceso de completado de fichas

| Etapa | Regla |
|---|---|
| 1. Selección | Por priorización (§2) y valor operativo; nunca por orden alfabético ciego |
| 2. Recolección | Fuentes nivel 1 (RFC/estándar) y nivel 2 (implementación de referencia) |
| 3. Llenado | Plantilla de 18 campos; unidad de datos exacta (glosario F0) |
| 4. Incertidumbre | Marcas `[n.p.d.]` / `[no verificable públicamente]` + registro U-xxxx |
| 5. Encapsulación | Registro en `F4-Matriz-Encapsulacion.json` (relaciones tipadas) |
| 6. Campos | Registro en `F5-Campos-PDU.json` (cuando proceda) |
| 7. Validación | Controles de esquema, coherencia y fuentes (F8 consolida) |

## 4. Matriz de encapsulación y dependencias

Vista resumida (la fuente de datos es [`F4-Matriz-Encapsulacion.json`](F4-Matriz-Encapsulacion.json)):

| Origen | Sobre / encapsula | Tipo | Nota |
|---|---|---|---|
| Ethernet (802.3) | Cobre / fibra | corre_sobre | Medio físico |
| IPv4 | Ethernet | corre_sobre | EtherType 0x0800 |
| IPv6 | Ethernet | corre_sobre | EtherType 0x86DD |
| ARP | Ethernet | corre_sobre | EtherType 0x0806; solo IPv4 (IPv6 usa NDP) |
| TCP / UDP | IPv4 / IPv6 | corre_sobre | IP protocol numbers 6 / 17 |
| QUIC | UDP | corre_sobre | RFC 9000 |
| HTTP/3 | QUIC | corre_sobre | RFC 9114; **depende** de QUIC |
| TLS | TCP (y DTLS sobre UDP) | corre_sobre | RFC 8446 |
| DNS | UDP / TCP | corre_sobre | Puerto 53; truncación → TCP |
| DHCP | UDP | corre_sobre | Puertos 67/68; legado BOOTP |
| BGP | TCP | corre_sobre | Puerto 179; RFC 4271 |
| OSPF | IPv4 (directo) | corre_sobre | IP protocol number 89 |
| ICMP / ICMPv6 | IPv4 / IPv6 | corre_sobre | Números de protocolo 1 / 58 |

**Regla:** la matriz distingue `encapsula` (una PDU contiene otra), `corre_sobre` (dependencia de transporte/medio) y `depende_de` (requisito funcional). La vista de recorrido extremo a extremo (plan §11, diagrama de ruta) se construirá a partir de esta matriz.

## 5. Criterios de salida / aceptación de F4

- [x] Plantilla de ficha de protocolo (18 campos) formalizada — `PLANTILLAS/plantilla-ficha-protocolo.md`.
- [x] Fichas prioritarias completas con fuente primaria: **10/12** (83 %) — `F4-Fichas-Prioritarias.md`.
- [x] Matriz de encapsulación/dependencias machine-readable — `F4-Matriz-Encapsulacion.json` (JSON válido, 20 relaciones + 7 dependencias).
- [~] Fichas de OSPF y Ethernet/802.3: **tarea registrada para F8** (campos y mensajes entregados en F5; ficha completa en la validación). No bloqueante.
- [x] Fuentes y fechas de consulta registradas por ficha (nivel 1 en campos críticos).
- [x] Aprobación de la fase (sección 6).

## 6. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de conocimiento | *(por confirmar)* | | |

> **Estado:** la aprobación de F4 habilita **F5 — Mensajería y PDU** (aprobada) y **F6 — Seguridad y operatividad** (en curso).

---
Última actualización: 26-08-2026