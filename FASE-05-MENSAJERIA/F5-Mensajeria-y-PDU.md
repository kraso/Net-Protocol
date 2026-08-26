# F5 — Mensajería y PDU

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 5 — Mensajería y PDU
**Documento rector:** `PLANREDES.md` §5.2, §8 (F5) · `F0-Glosario-PDU.md` · `PLANTILLAS/plantilla-ficha-protocolo.md`

| Campo | Valor |
|---|---|
| Documento | F5-Mensajeria-y-PDU.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 |
| Estado | **Aprobada (26-08-2026)** |
| Depende de | F3 (aprobada), F4 (aprobada) |
| Fase siguiente | F6 — Seguridad y operatividad (en curso) · F8 — Validación (layouts vs. capturas) |

---

## 1. Objetivo de la fase

Modelar **cabeceras, campos, mensajes, secuencias y máquinas de estado**, distinguiendo **formatos normativos** de **ejemplos de implementación** (plan §8 F5). Esta fase produce: inventario de unidades de datos, modelo de campos (`Field`), wire format regenerable y máquinas de estado de referencia.

## 2. Unidades de datos por protocolo (glosario F0 aplicado)

| Protocolo | Unidad pertinente | Notas |
|---|---|---|
| Ethernet (802.3) | **Trama** | Cabecera de enlace + payload + FCS |
| Wi-Fi (802.11) | **Trama** 802.11 | MPDU/PPDU |
| IPv4 / IPv6 | **Paquete / datagrama** | |
| TCP | **Segmento** | |
| UDP | **Datagrama** | |
| SCTP | **Mensaje/Chunk** | Unidad de datos: chunk |
| QUIC | **Paquete + frames sobre stream** | Flujo/stream como capa de datos |
| DNS | **Mensaje** | Query/Response |
| DHCP | **Mensaje** | Formato BOOTP + options (TLV) |
| HTTP/3 / HTTP | **Mensaje** | Request/Response |
| TLS | **Record** | Subprotocolos handshake/alert/data |
| BGP | **Mensaje** | OPEN/UPDATE/NOTIFICATION/KEEPALIVE |
| ATM (histórico) | **Celda** | 53 bytes |
| MPLS | **Paquete etiquetado** | Label stack sobre PDU |
| Modbus | **ADU/PDU** | ADU = cabecera + PDU + checksum |

> **Regla (glosario F0):** cada protocolo declara su unidad exacta; prohibido "paquete" genérico.

## 3. Modelo de campos (`Field`)

Esquema del catálogo `F5-Campos-PDU.json`:

| Campo del modelo | Tipo | Regla |
|---|---|---|
| `nombre` | string | Nombre exacto del estándar |
| `offset_bits` | int | Desde el inicio de la cabecera/PDU (base 0) |
| `longitud_bits` | int (o variable) | `null` si variable/opcional |
| `tipo` | enum | uint16, uint32, bitset/flags, address, opción TLV… |
| `semantica` | string | Significado normativo |
| `valores` | string | Valores permitidos / flags |
| `obligatorio` | bool | ¿Siempre presente en esa PDU? |
| `endianness` | enum | network order (big-endian) por defecto; declarado si difiere |
| `nota` | string | Compatibilidad, seguridad, referencias |

**Reglas:** (1) endianness siempre declarado, nunca asumido; (2) se distingue formato **normativo** (manda el estándar) de **ejemplo de implementación** (ilustrativo); (3) un detalle no público → `[n.p.d.]`.

## 4. Wire format regenerable (referencia: TCP, RFC 9293)

Los layouts se representan como diagramas deterministas regenerables (plan §11, diagrama de mensaje). Ejemplo normativo (TCP):

```
 0                   1                   2                   3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|          Source Port          |       Destination Port        |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                        Sequence Number                        |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                    Acknowledgment Number                      |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|  Data |  Res |C|E|U|A|P|R|S|F|                               |
| Offset|  rvd |W|C|R|C|S|S|Y|I|            Window             |
|       |      |R|E|G|K|H|T|N|N|                               |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|           Checksum            |         Urgent Pointer        |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           [Options]                           |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
```

> Layout canónico de cabecera TCP (RFC 9293). El flag NS ocupa el 4º bit de "Reserved" en RFC 9293. La representación regenerable (bit/byte, offsets) vive en `F5-Campos-PDU.json` (PR-036).

**Proceso:** los layouts se generan desde los datos del catálogo de campos (nunca a mano); cada layout se valida contra una captura real en F8 (correspondencia frame/packet ↔ campos documentados, objetivo compatible con la disección de Wireshark, fuente R3).

## 5. Máquinas de estado de referencia

| Protocolo | Estados | Transiciones clave |
|---|---|---|
| **TCP** | CLOSED, LISTEN, SYN-SENT, SYN-RECEIVED, ESTABLISHED, FIN-WAIT-1, FIN-WAIT-2, CLOSE-WAIT, CLOSING, LAST-ACK, TIME-WAIT | SYN → ESTABLISHED (3-way); cierre activo/pasivo; RST para abortar |
| **DHCP** | INIT, SELECTING, REQUESTING, BOUND, RENEWING, REBINDING | INIT→(Discover)→SELECTING→(Offer)→REQUESTING→(Ack)→BOUND; T1→RENEWING; T2→REBINDING |
| **BGP** | Idle, Connect, Active, OpenSent, OpenConfirm, Established | Established con KEEPALIVE; NOTIFICATION → Idle |

**Regla:** las FSMs se declaran en datos estructurados (tabla de transiciones: estado → evento → estado → acción) para regenerar diagramas de estado (plan §11, diagrama 4).

## 6. Secuencias de referencia (mínimas)

| Secuencia | Descripción |
|---|---|
| **TCP 3-way** | SYN → SYN+ACK → ACK |
| **TLS 1.3** | ClientHello → ServerHello (+Certificate, Finished) → Finished (1-RTT) |
| **DHCP DORA** | Discover → Offer → Request → Ack |
| **DNS** | Query → Response (UDP); reintento/timeout; TCP si truncación |
| **BGP** | OPEN ↔ OPEN → (KEEPALIVE…) → UPDATE/NOTIFICATION |

## 7. Normativo vs. implementación (regla de oro)

- **Normativo:** lo que define el estándar (offsets, valores, semántica). Fuente nivel 1.
- **Implementación:** valores por defecto, timers, heurísticas, codificaciones internas de un SO/vendor. Fuente nivel 2; **nunca** se presenta como norma.
- Los ejemplos de capturas son **ilustrativos**, nunca normativos.

## 8. Criterios de salida / aceptación de F5

- [x] Inventario de unidades de datos por protocolo (§2) — glosario F0 aplicado.
- [x] Modelo de campos `Field` definido y aplicado en catálogo — `F5-Campos-PDU.json` (JSON válido, 51 campos en 6 protocolos).
- [x] Wire format regenerable de referencia (TCP) y proceso definido (§4).
- [x] Máquinas de estado de referencia: TCP, DHCP, BGP (§5).
- [x] Secuencias mínimas registradas (§6) y regla normativo vs. implementación (§7).
- [~] Validación de layouts contra capturas reales: **tarea registrada para F8**. No bloqueante.
- [x] Aprobación de la fase (sección 9).

## 9. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de conocimiento | *(por confirmar)* | | |

> **Estado:** la aprobación de F5 habilita **F6 — Seguridad y operatividad** (en curso) y alimenta **F8 — Validación** (correspondencia campos ↔ capturas).

---
Última actualización: 26-08-2026