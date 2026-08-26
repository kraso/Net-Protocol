# F0 — Glosario de PDU y Reglas de Nomenclatura

**Fase:** 0 — Definición y límites · **Estado:** ✅ aprobado (F0 cerrada el 26-08-2026)

---

## 1. Reglas de nomenclatura (obligatorias)

| # | Regla | Ejemplo / aclaración |
|---|---|---|
| N1 | **Nombre completo + acrónimo + aliases** en toda ficha. | "Transmission Control Protocol (TCP)" + aliases históricos si existen. |
| N2 | **Clave estable tipo URN** separada del nombre mostrado. | `urn:proto:ietf:rfc9114` (HTTP/3); el nombre mostrado puede cambiar, la URN no. |
| N3 | **No confundir protocolo con servicio o aplicación.** | HTTP (protocolo) ≠ servidor web Apache (implementación) ≠ puerto 80 (registro). |
| N4 | **No confundir puerto con protocolo.** | IANA asigna un puerto a un servicio registrado; el tráfico en ese puerto no prueba que corresponda al servicio (advertencia IANA). |
| N5 | **No confundir implementación con estándar.** | "soporta" ≠ "implementa completamente". |
| N6 | **Unidad de datos exacta por protocolo.** | Prohibido usar "paquete" como etiqueta genérica; cada protocolo declara su unidad pertinente (Tabla §3). |
| N7 | **Separación epistemológica entre clases.** | protocolo ≠ estándar ≠ implementación ≠ servicio ≠ formato ≠ algoritmo ≠ transporte ≠ interfaz ≠ tecnología física. |
| N8 | **Fechas absolutas y versiones concretas.** | "RFC 9114 (2022-06-06)" y no "HTTP/3 (reciente)". |
| N9 | **Ortografía de identificadores técnicos.** | EtherType 0x86DD, IP protocol number 6 (TCP), next-header, puertos TCP/UDP, etc., con su unidad de medida exacta. |
| N10 | **No atribuir a OSI una exactitud que no tenga.** | Las correspondencias OSI↔TCP/IP son orientativas (ver `F0-Ejes-de-Clasificacion.md` §4). |

## 2. Vocabulario controlado de unidades de datos

Para cada protocolo se identifica **cuál es su unidad pertinente** entre los siguientes términos (los valores son del estándar del proyecto, `PLANREDES.md` Apéndice B):

| Término | Significado orientativo | Ejemplos típicos |
|---|---|---|
| **Trama (frame)** | Unidad de capa de enlace (cabecera de enlace + payload + trailer) | Ethernet 802.3, Wi-Fi 802.11, PPP |
| **Paquete (packet)** | Unidad de la capa de red | IPv4, IPv6, IPX |
| **Datagrama** | Unidad de servicio sin conexión | UDP, IP (clásico) |
| **Segmento (segment)** | Unidad de un transporte orientado a conexión | TCP, SCTP |
| **Celda (cell)** | Unidad de longitud fija | ATM (53 bytes) |
| **Símbolo (symbol)** | Unidad física de señalización | Codificación de capa física (QAM, OFDM…) |
| **Flujo (stream / flow)** | Secuencia continua con contexto de conversación | TLS records sobre TCP, QUIC stream |
| **Registro (record)** | Unidad estructurada dentro de un flujo | DNS RR, TLS record, syslog |
| **Mensaje (message)** | Unidad semántica de aplicación/señalización | HTTP message, SIP, SMTP, CoAP |
| **PDU / SDU / ADU** | Protocol / Service / Application Data Unit según capa | PDU_SDU_encapsulación por capa (§3) |
| **Objeto semántico** | Unidad significativa de aplicación | Recurso CoAP, nodo OPC UA |
| **Elemento de información (IE)** | Bloque con tipo-tag | IEs GSM/LTE, opciones DHCP/BootP |
| **TLV** | Estructura Tipo-Longitud-Valor | DHCP options, BGP path attributes, SNMP |
| **Campo / atributo / etiqueta** | Componentes internos de una unidad mayor | Campos de cabecera TCP/IP |
| **Encabezado / trailer / payload** | Partes estructurales de una PDU | — |

## 3. Correspondencia PDU/SDU/ADU por capa (orientativa)

| Capa | Unidad que recibe (SDU) | Unidad que emite (PDU) |
|---|---|---|
| Aplicación | Datos de usuario | Mensaje / registro / objeto (ADU) |
| Transporte | Segmento/datagrama como SDU | Segmento (TCP), datagrama (UDP) |
| Red | Segmento como SDU | Paquete / datagrama |
| Enlace | Paquete como SDU | Trama |
| Física | Trama como SDU (bits) | Símbolo / bits |

> Regla: una PDU de la capa N se encapsula como SDU de la capa N−1. Cada protocolo documenta **encapsulación** (superior e inferior) y **tunneling** en su ficha.

## 4. Convenciones de representación de formatos

| Convención | Regla |
|---|---|
| Offsets | Se documentan en bits o bytes según la especificación; se indica la base (0 o 1) cuando el estándar la define. |
| Endianness | Se declara explícitamente (big-endian/little-endian/network order); nunca se asume. |
| Codificación | Tipo (binario, ASCII, UTF-8, BER/DER…), alineamiento y longitud (fija/variable). |
| Ejemplos | Se distingue **formato normativo** (manda el estándar) de **ejemplo de implementación** (ilustrativo). |
| No documentado | Un detalle ausente de la especificación se marca `[n.p.d.]` (no documentado públicamente) — ver Política de Incertidumbre. |

## 5. Criterios de fijación del glosario

El glosario se considera **fijado** cuando:

- [ ] El vocabulario de la Tabla §2 no tiene términos en conflicto con fichas piloto.
- [ ] Las reglas N1–N10 se aplican de forma consistente en el catálogo de prueba (3+ fichas piloto).
- [ ] Las convenciones de representación (§4) están aceptadas por el responsable.