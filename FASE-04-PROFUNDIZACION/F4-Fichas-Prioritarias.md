# F4 — Fichas Prioritarias de Protocolos

**Fase:** 4 — Profundización protocolar · **Documento rector:** `PLANREDES.md` §6.2 · Plantilla: `PLANTILLAS/plantilla-ficha-protocolo.md`

Fichas completas (18 campos) de los protocolos prioritarios. **Fuentes:** RFC/estándares (nivel 1) y IANA (R1) con fecha de consulta 26-08-2026; puertos "registrados en IANA" marcados como tales (puerto ≠ protocolo). Campos sin confirmación posterior se marcan como en el original. Las fichas se regenerarán desde el pipeline (F8) a partir de datos estructurados.

---

## F-01 · TCP — Transmission Control Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | TCP; RFC 9293 (2022-08, obsoleta RFC 793); IETF; familia TRAN (Transporte y sesión) |
| 2 | Estado | Vigente (26-08-2026; RFC 9293) |
| 3 | Finalidad | Transporte fiable, ordenado, orientado a conexión, con control de congestión. Casos: aplicaciones que exigen fiabilidad. **No usar** para broadcast/multicast ni tráfico time-critical sin tolerancia a retransmisión |
| 4 | Encapsulación | Corre sobre IPv4/IPv6 (IP protocol number **6**); bajo él: HTTP, TLS, SSH, SMTP, BGP… |
| 5 | Capas | OSI 4 (Transporte); TCP/IP: Transporte; plano: datos |
| 6 | Transporte y direccionamiento | Es transporte; puertos 0–65535 (well-known 0–1023 registrados en IANA; verificar rango y asignaciones por pipeline) |
| 7 | PDU | **Segmento** (no "paquete") |
| 8 | Mensajes | Segmentos sin tipos formales; control por flags: SYN, ACK, FIN, RST, PSH, URG, ECE, CWR, NS |
| 9 | Campos | Cabecera 20–60 B. Detalle por campo en `F5-Campos-PDU.json` (PR-036) |
| 10 | Secuencia | 3-way handshake (SYN → SYN+ACK → ACK); datos; cierre FIN / FIN+ACK o RST |
| 11 | Addressing/naming | Combinación (IP origen/destino, puerto origen/destino); los puertos identifican servicios locales |
| 12 | Routing/forwarding | No participa (capa transporte) |
| 13 | Seguridad | Sin cifrado nativo (depende de TLS); checksum (obligatorio en IPv6); sin integridad criptográfica; exposición a spoofing según implementación |
| 14 | QoS/rendimiento | Control de congestión (RFC 5681 y sucesivas), retransmisión, ventana deslizante, temporizadores; MSS/MTU negotiation. Detalles de heurísticas de implementación: `[n.p.d.]` salvo RFC |
| 15 | Observabilidad | Puertos, flags, números de secuencia/acknowledgment visibles en capturas; filtros tcp.* (Wireshark) |
| 16 | Interoperabilidad | Extensiones: ventana escalada y timestamps (RFC 7323), SACK (RFC 2018); diferencias entre SO en valores por defecto |
| 17 | Implementaciones | Pilas de SO (Linux, Windows, BSD), lwIP, librerías de sockets; "soporta" ≠ "implementa completamente" |
| 18 | Fuentes | RFC 9293 (nivel 1); IANA R1 (puertos) — consulta 26-08-2026 |

## F-02 · UDP — User Datagram Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | UDP; RFC 768 (1980-08); IETF; familia TRAN |
| 2 | Estado | Vigente (26-08-2026; RFC 768) |
| 3 | Finalidad | Transporte sin conexión, sin fiabilidad garantizada, mínimo overhead. Casos: DNS, DHCP, RTP, QUIC, juegos. **No usar** cuando se exige entrega ordenada/fiable |
| 4 | Encapsulación | Corre sobre IPv4/IPv6 (IP protocol number **17**); bajo él: DNS, DHCP, RTP, QUIC… |
| 5 | Capas | OSI 4 (Transporte); TCP/IP: Transporte; plano: datos |
| 6 | Transporte y direccionamiento | Puertos 0–65535 (well-known registrados en IANA) |
| 7 | PDU | **Datagrama** |
| 8 | Mensajes | Datagramas; sin estados ni acuses |
| 9 | Campos | Cabecera de 8 B (4 campos). Detalle en `F5-Campos-PDU.json` (PR-037) |
| 10 | Secuencia | Sin establecimiento; envío directo; sin cierre |
| 11 | Addressing/naming | (IP origen/destino, puerto origen/destino) |
| 12 | Routing/forwarding | No participa |
| 13 | Seguridad | Sin cifrado ni integridad nativa; checksum opcional en IPv4, **obligatorio en IPv6** |
| 14 | QoS/rendimiento | Sin control de congestión propio; entrega best-effort |
| 15 | Observabilidad | Puertos y longitud visibles; filtros udp.* |
| 16 | Interoperabilidad | Alta; checksum IPv4 opcional puede causar diferencias entre SO |
| 17 | Implementaciones | Todas las pilas de SO; librerías sockets |
| 18 | Fuentes | RFC 768 (nivel 1); IANA R1 (puertos) — 26-08-2026 |

## F-03 · IPv4 — Internet Protocol version 4

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | IPv4; RFC 791 (1981-09); IETF; familia ADCONF |
| 2 | Estado | Vigente (26-08-2026; RFC 791 + actualizaciones) |
| 3 | Finalidad | Direccionamiento y entrega best-effort de datagramas a través de redes interconectadas. **No usar** (en diseño nuevo) cuando se requiere espacio de direcciones amplio → IPv6 |
| 4 | Encapsulación | Corre sobre enlace (EtherType 0x0800); encapsula TCP/UDP/ICMP/OSPF… (campo protocol) |
| 5 | Capas | OSI 3 (Red); TCP/IP: Internet; plano: datos |
| 6 | Transporte y direccionamiento | Cabecera con campo `protocol` (TCP=6, UDP=17, ICMP=1, OSPF=89); direcciones de 32 bits |
| 7 | PDU | **Paquete / datagrama IP** |
| 8 | Mensajes | Datagramas; no orientado a conexión; fragmentación posible |
| 9 | Campos | Cabecera 20–60 B (IHL × 32 bits). Detalle en `F5-Campos-PDU.json` (PR-011) |
| 10 | Secuencia | Sin establecimiento; fragmentación/reensamblado según MTU de ruta |
| 11 | Addressing/naming | Direcciones 32 bits con clases históricas → CIDR (RFC 4632); IPv4 privadas (RFC 1918) |
| 12 | Routing/forwarding | Encaminamiento por tabla de rutas; TTL decrementado por salto; posible fragmentación en routers (evitada por path MTU discovery, RFC 1191) |
| 13 | Seguridad | Sin seguridad nativa (IPsec se integra como opción); falsificación de origen posible sin protección |
| 14 | QoS/rendimiento | Campo DSCP/ECN; best-effort sin garantías |
| 15 | Observabilidad | Protocolo, direcciones, TTL, flags de fragmentación visibles; filtros ip.* |
| 16 | Interoperabilidad | Universal; coexistencia con IPv6 mediante dual-stack/túneles (F6 tratará transición) |
| 17 | Implementaciones | Todas las pilas; routers de todos los fabricantes |
| 18 | Fuentes | RFC 791, 4632, 1918, 1191 (nivel 1); IANA R1 — 26-08-2026 |

## F-04 · IPv6 — Internet Protocol version 6

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | IPv6; RFC 8200 (2017-07, obsoleta RFC 2460); IETF; familia ADCONF |
| 2 | Estado | Vigente (26-08-2026; RFC 8200) |
| 3 | Finalidad | Sucesor de IPv4: espacio de direcciones de 128 bits, cabecera simplificada, sin fragmentación en ruta por routers |
| 4 | Encapsulación | Corre sobre enlace (EtherType 0x86DD); encapsula TCP/UDP/ICMPv6… (next header) |
| 5 | Capas | OSI 3 (Red); TCP/IP: Internet; plano: datos |
| 6 | Transporte y direccionamiento | Campos `next header` y direcciones de 128 bits |
| 7 | PDU | **Paquete / datagrama IPv6** |
| 8 | Mensajes | Datagramas; extension headers en cadena |
| 9 | Campos | Cabecera fija de 40 B. Detalle en `F5-Campos-PDU.json` (PR-012) |
| 10 | Secuencia | Sin establecimiento; los routers no fragmentan (RFC 8200); path MTU discovery (RFC 8201) |
| 11 | Addressing/naming | 128 bits; prefijos/agregación global, link-local (fe80::/10), multicast (ff00::/8); NDP (PR-014) sustituye ARP |
| 12 | Routing/forwarding | Encaminamiento por tabla; hop limit; extension headers |
| 13 | Seguridad | IPsec concebido como parte del diseño; **sin** checksum de cabecera (menos recomputation) |
| 14 | QoS/rendimiento | Traffic class y flow label |
| 15 | Observabilidad | Next header, direcciones, hop limit visibles; filtros ipv6.* |
| 16 | Interoperabilidad | Transición dual-stack/túneles; fiabilidad de configuración automática (SLAAC, RFC 4862) |
| 17 | Implementaciones | Todas las pilas modernas; routers dual-stack |
| 18 | Fuentes | RFC 8200, 4862, 8201 (nivel 1); IANA R1 — 26-08-2026 |

## F-05 · ARP — Address Resolution Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | ARP; RFC 826 (1982-11); IETF; familia ADCONF |
| 2 | Estado | Vigente en IPv4 (26-08-2026; RFC 826) |
| 3 | Finalidad | Mapear dirección de red (IPv4, 32 bits) a dirección de enlace (MAC, 48 bits) dentro del mismo dominio de difusión |
| 4 | Encapsulación | Corre sobre Ethernet (EtherType 0x0806); dentro de trama; no es enrutable (link-local) |
| 5 | Capas | OSI 2/2.5 (mapeo entre Red y Enlace); plano: datos |
| 6 | Transporte y direccionamiento | Sin puertos; direcciones IPv4 ↔ MAC |
| 7 | PDU | **Mensaje ARP** (request/reply) dentro de trama |
| 8 | Mensajes | ARP Request (broadcast) y ARP Reply (unicast) |
| 9 | Campos | Cabecera: hardware type, protocol type, HLEN, PLEN, opcode, sender/protocol y target addresses. (Detalle en F5 si se cataloga) |
| 10 | Secuencia | Request → Reply (solo, sin confirmación); caché con envejecimiento |
| 11 | Addressing/naming | Direcciones IPv4 ↔ MAC; solo funciona en subnet local |
| 12 | Routing/forwarding | No (mapeo local; los routers responden por sí mismos en su interfaz) |
| 13 | Seguridad | Sin autenticación → vulnerable a ARP spoofing/poisoning (mitigación: herramientas de detección, puertos seguros) |
| 14 | QoS/rendimiento | Irrelevante; tráfico de control local |
| 15 | Observabilidad | Opcode y direcciones visibles; filtros arp.* |
| 16 | Interoperabilidad | IPv6 **no** usa ARP (usa NDP, PR-014) |
| 17 | Implementaciones | Pilas IPv4; caches ARP de SO |
| 18 | Fuentes | RFC 826 (nivel 1); IEEE 802 EtherTypes — 26-08-2026 |

## F-06 · DNS — Domain Name System

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | DNS; RFC 1035 (1987-11) + actualizaciones; IETF; familia ADCONF (servicio de nombres) |
| 2 | Estado | Vigente (26-08-2026) |
| 3 | Finalidad | Resolver nombres de dominio ↔ direcciones (A/AAAA) y otros registros; jerárquico y distribuido. **No usar** cuando se necesita un registro local efímero (mDNS/LLMNR) |
| 4 | Encapsulación | Corre sobre UDP (habitual) y TCP (truncación/zonas grandes), puerto **53** registrado en IANA; DNSSEC añade registros y firmas (PR-078) |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | UDP/TCP 53; RCODE, QTYPE/QCLASS |
| 7 | PDU | **Mensaje DNS** (petición/respuesta) |
| 8 | Mensajes | Query/Response; tipos de registro: A, AAAA, CNAME, MX, NS, SOA, PTR, TXT… |
| 9 | Campos | Cabecera de 12 B: ID, flags, QDCOUNT, ANCOUNT, NSCOUNT, ARCOUNT; secciones question/answer/authority/additional. Detalle en `F5-Campos-PDU.json` (PR-015) |
| 10 | Secuencia | Query → Response; iterativo/recursivo según configuración; caché con TTL; reintentos y timeout |
| 11 | Addressing/naming | FQDN jerárquico; resolvers y servidores autoritativos |
| 12 | Routing/forwarding | No (servicio de resolución; enroutable como aplicación) |
| 13 | Seguridad | DNS clásico sin integridad (spoofing); DNSSEC (PR-078) firma; Do53/DoT/DoH para confidencialidad |
| 14 | QoS/rendimiento | TTL/caché; latencia de resolución; EDNS0 (tamaño) |
| 15 | Observabilidad | ID, flags, QNAME, RCODE visibles; filtros dns.* |
| 16 | Interoperabilidad | Alta; variantes entre resolvers (RD/RA); EDNS0 |
| 17 | Implementaciones | BIND, Unbound, dnsmasq, resolvers de SO, librerías (libc, getaddrinfo) |
| 18 | Fuentes | RFC 1035 y relacionadas (nivel 1); IANA R1 (puerto 53) — 26-08-2026 |

## F-07 · DHCP — Dynamic Host Configuration Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | DHCP; RFC 2131 (1997-03); IETF; familia ADCONF |
| 2 | Estado | Vigente (26-08-2026; RFC 2131; DHCPv6 en RFC 8415) |
| 3 | Finalidad | Configuración automática de hosts: dirección IP, máscara, gateway, DNS, leases. **No usar** en redes donde se exige direccionamiento estático por seguridad/regulación (salvo reservas) |
| 4 | Encapsulación | Corre sobre UDP (puertos **67/68** registrados en IANA); basado en BOOTP (legado) |
| 5 | Capas | OSI 7 (Aplicación); plano: datos/gestión |
| 6 | Transporte y direccionamiento | UDP 67 (servidor) / 68 (cliente); option 53 (message type) |
| 7 | PDU | **Mensaje DHCP** (formato BOOTP ampliado con options) |
| 8 | Mensajes | DORA: Discover · Offer · Request · Ack (más NAK, Release, Decline, Inform) |
| 9 | Campos | Cabecera BOOTP + options TLV (subnet mask, router, DNS, lease time, message type…) |
| 10 | Secuencia | DORA: Discover (broadcast) → Offer → Request → Ack; renovación (T1/T2) y rebinding |
| 11 | Addressing/naming | MAC de cliente (chaddr), dirección ofrecida, leases con tiempo |
| 12 | Routing/forwarding | No; relay agent (opción 82) para atraviesar subredes |
| 13 | Seguridad | Sin autenticación nativa (suplantable en L2); mitigaciones: DHCP snooping, opción 82, 802.1X (PR-077) |
| 14 | QoS/rendimiento | Leases/renovación; retransmisión con backoff del cliente |
| 15 | Observabilidad | Message type, MAC, opciones y leases visibles; filtros dhcp.* / bootp.* |
| 16 | Interoperabilidad | Clientes/servidores muy estandarizados; DHCPv6 (RFC 8415) distinto |
| 17 | Implementaciones | ISC DHCP, Kea, dnsmasq, Windows Server, clientes de SO |
| 18 | Fuentes | RFC 2131, 8415 (nivel 1); IANA R1 (puertos 67/68) — 26-08-2026 |

## F-08 · TLS — Transport Layer Security

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | TLS (sucesor de SSL); RFC 8446 (TLS 1.3, 2018-08); IETF; familia SEG |
| 2 | Estado | Vigente (TLS 1.3, 26-08-2026; TLS 1.2 RFC 5246 en transición, deprecación de versiones antiguas en curso) |
| 3 | Finalidad | Confidencialidad, integridad y autenticación para protocolos de aplicación sobre transporte. **No usar** sin validación adecuada de certificados; no sustituye a IPsec en todos los escenarios |
| 4 | Encapsulación | Corre sobre TCP (habitual) y sobre UDP vía DTLS (PR-074); bajo: HTTP, SMTP, IMAP… |
| 5 | Capas | OSI 4.5/6 (entre Transporte y Aplicación); plano: seguridad |
| 6 | Transporte y direccionamiento | Sin puerto propio (el de la aplicación, p. ej. 443 para HTTPS registrado en IANA) |
| 7 | PDU | **Record TLS** (subprotocolos: handshake, alert, application data, change_cipher_spec) |
| 8 | Mensajes | Handshake: ClientHello, ServerHello, EncryptedExtensions, Certificate, Finished…; Alerts; Application Data |
| 9 | Campos | Cabecera de record (content type, version, length) + payload cifrado. (Detalle campo a campo se cataloga en F5/F6) |
| 10 | Secuencia | Handshake 1-RTT (TLS 1.3); 0-RTT opcional; renegociación limitada; cierre por close_notify |
| 11 | Addressing/naming | Identidad por certificado X.509 (SAN/CN); SNI para selección de certificado |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | AEAD (AES-GCM, ChaCha20-Poly1305), forward secrecy por ECDHE, Perfect Forward Secrecy; dependencias criptográficas críticas (randomness, validación de certificados, suites débiles prohibidas en 1.3) |
| 14 | QoS/rendimiento | Handshake 1-RTT, 0-RTT, session resumption; overhead de record |
| 15 | Observabilidad | SNI, version, cipher suite y certificado visibles (según inspección); tráfico cifrado [n.p.d.] para contenido |
| 16 | Interoperabilidad | TLS 1.3 retrocompatible en negociación; implementaciones amplias |
| 17 | Implementaciones | OpenSSL, BoringSSL, NSS, Schannel, librerías TLS de lenguajes; "soporta" ≠ "configuración segura" |
| 18 | Fuentes | RFC 8446, 5246 (nivel 1); IANA R1 — 26-08-2026 |

## F-09 · HTTP/3 — HTTP/3

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | HTTP/3; **RFC 9114** (2022-06); IETF; familia APP — estándar de referencia: fuente R2 del plan |
| 2 | Estado | Vigente (RFC 9114, 2022-06) |
| 3 | Finalidad | Semántica HTTP sobre transporte QUIC: menor latencia de establecimiento, multiplexación sin head-of-line blocking a nivel de transporte. **No usar** donde no exista soporte QUIC (coexistencia HTTP/1.1–2) |
| 4 | Encapsulación | Corre sobre **QUIC** (RFC 9000), que corre sobre UDP; no sobre TCP |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | Sobre QUIC (UDP); puerto 443 típico (HTTPS registrado en IANA); identificador de conexión QUIC |
| 7 | PDU | **Mensaje HTTP/3** (frames sobre streams QUIC) + cabeceras QPACK |
| 8 | Mensajes | Requests/Responses (métodos y estados de HTTP); frames: HEADERS, DATA, SETTINGS, GOAWAY…; **server push eliminado** |
| 9 | Campos | Cabeceras HTTP (compresión QPACK, RFC 9204); campos QUIC y frame headers (detalle en F5/F6) |
| 10 | Secuencia | QUIC handshake (1-RTT, incluye TLS 1.3) → streams → requests/responses; GOAWAY para cierre |
| 11 | Addressing/naming | URI + autoridad; ALT-SVC para descubrimiento HTTP/3 |
| 12 | Routing/forwarding | No (aplicación) |
| 13 | Seguridad | TLS 1.3 integrado en QUIC (cifrado de todo el contenido, cabecera parcialmente cifrada) |
| 14 | QoS/rendimiento | Multiplexación por streams; control de congestión de QUIC; 0-RTT |
| 15 | Observabilidad | UDID de conexión QUIC, streams, frame types; filtros http3/quic.* |
| 16 | Interoperabilidad | Soporte creciente en navegadores y servidores; migración de conexión QUIC |
| 17 | Implementaciones | navegadores (Chromium, Firefox, Safari), servidores (nginx/QUIC, LiteSpeed, Cloudflare), librerías (quiche, msquic) |
| 18 | Fuentes | RFC 9114, 9000, 9204 (nivel 1); R2 — 26-08-2026 |

## F-10 · BGP — Border Gateway Protocol (BGP-4)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | BGP-4; RFC 4271 (2006-01) + actualizaciones (RFC 7606, 8212…); IETF; familia ROUT |
| 2 | Estado | Vigente (26-08-2026; RFC 4271 línea) |
| 3 | Finalidad | Intercambio de rutas entre sistemas autónomos (eBGP) y dentro de ellos (iBGP); política de rutas. **No usar** dentro de un dominio como IGP (no es para topologías dinámicas internas; usa OSPF/IS-IS) |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **179** registrado en IANA) |
| 5 | Capas | OSI 7/Aplicación sobre transporte; plano: **control** |
| 6 | Transporte y direccionamiento | TCP 179; NLRI (prefijos), path attributes; ASNs |
| 7 | PDU | **Mensaje BGP** |
| 8 | Mensajes | OPEN · UPDATE · NOTIFICATION · KEEPALIVE (y ROUTE-REFRESH, RFC 2918) |
| 9 | Campos | UPDATE: withdrawn routes, path attributes, NLRI; attributes: AS_PATH, NEXT_HOP, ORIGIN, LOCAL_PREF, MED… |
| 10 | Secuencia | FSM: Idle → Connect → OpenSent → OpenConfirm → Established; KEEPALIVE periódico; NOTIFICATION en errores |
| 11 | Addressing/naming | ASN + prefijos; peering eBGP/iBGP |
| 12 | Routing/forwarding | Selección de mejores rutas por algoritmo (weight, LOCAL_PREF, AS_PATH, MED…); propagación de rutas |
| 13 | Seguridad | Vulnerable históricamente (hijacking): mitigaciones RPKI/ROA, BGPsec (parcial), filtrado de prefijos; TCP MD5/TCP-AO opcional |
| 14 | QoS/rendimiento | Convergencia más lenta que IGPs; route refresh y session fluctuation |
| 15 | Observabilidad | Mensajes, AS_PATH, NLRI visibles; filtros bgp.* |
| 16 | Interoperabilidad | Alta; diferencias en defaults y políticas entre implementaciones |
| 17 | Implementaciones | FRRouting, BIRD, Quagga, Cisco IOS-XR, Juniper JunOS; "soporta" ≠ "implementa todas las extensiones/rfc" |
| 18 | Fuentes | RFC 4271 y relacionadas (nivel 1); IANA R1 (puerto 179) — 26-08-2026 |

## F-11 · OSPF — Open Shortest Path First (v2, para IPv4)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | OSPF; RFC 2328 (1998-04), OSPFv3 en RFC 5340; IETF; familia ROUT |
| 2 | Estado | Vigente (26-08-2026; RFC 2328/5340) |
| 3 | Finalidad | IGP link-state dentro de un dominio (AS); cálculo SPF; áreas para escalar. **No usar** para interdominio (BGP) |
| 4 | Encapsulación | Corre **directamente sobre IPv4** (IP protocol number **89**); sin UDP/TCP |
| 5 | Capas | OSI 3 (Red); plano: **control** |
| 6 | Transporte y direccionamiento | Protocolo IP 89; multicast 224.0.0.5 (todos los routers OSPF) y 224.0.0.6 (DR/BDR); router-id |
| 7 | PDU | **Mensaje OSPF** (paquete OSPF) |
| 8 | Mensajes | Hello · Database Description (DD) · Link State Request (LSR) · Link State Update (LSU, contiene LSAs) · Link State Acknowledgment (LSAck) |
| 9 | Campos | Cabecera OSPF de 24 B: version, type, packet length, router-id, area-id, checksum, autype, authentication. (Mensajes/LSAs se detallan en ciclos F5 posteriores) |
| 10 | Secuencia | Hello → adyacencia → elección de DR/BDR en multiaccess → sincronización de LSDB (DD/Request/Update) → flooding de LSAs → convergencia SPF |
| 11 | Addressing/naming | Router-id (IPv4); area-id; tipos de red: broadcast, NBMA, point-to-point, point-to-multipoint |
| 12 | Routing/forwarding | Dijkstra (SPF) sobre LSDB; LSAs: router, network, summary (tipo 3), ASBR-summary (tipo 4), external (tipo 5), NSSA (tipo 7); coste por enlace |
| 13 | Seguridad | Autenticación por área: none (0), password claro (1), criptográfica MD5 (2) y criptográfica (mejor: HMAC-SHA, RFC 5709). Sin cifrado nativo |
| 14 | QoS/rendimiento | Convergencia rápida; Hello/Dead timers 10/40 s en broadcast; requerimiento de LSDB sincronizada |
| 15 | Observabilidad | Tipo de paquete, router-id, area, LSAs visibles en capturas; filtros ospf.* |
| 16 | Interoperabilidad | OSPFv2 (IPv4, RFC 2328) y OSPFv3 (IPv6, RFC 5340) no intercambiables entre sí; diferencias de autenticación |
| 17 | Implementaciones | FRRouting, Cisco IOS/IOS-XR, Juniper JunOS, MikroTik; "soporta" ≠ "implementa todas las extensiones" |
| 18 | Fuentes | RFC 2328, 5340, 5709 (nivel 1) — 26-08-2026 |

## F-12 · Ethernet (IEEE 802.3)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Ethernet; **IEEE 802.3** (familia con enmiendas); IEEE; familia ACEL |
| 2 | Estado | Vigente (26-08-2026; 802.3 en evolución continua) |
| 3 | Finalidad | Enlace LAN por excelencia; tramas sobre cobre/fibra; CSMA/CD histórico (irrelevante en full-duplex). **No usar** como WAN nativa sin transporte |
| 4 | Encapsulación | Corre sobre medios físicos (cobre/fibra); encapsula IPv4 (EtherType 0x0800), IPv6 (0x86DD), ARP (0x0806), VLAN (0x8100)… |
| 5 | Capas | OSI 2 (Enlace) + subcapas físicas (PCS/PMA); plano: datos |
| 6 | Transporte y direccionamiento | EtherType; direcciones MAC de 48 bits; sin puertos |
| 7 | PDU | **Trama** (preamble+SFD físicos; DA, SA, EtherType/Length, payload 46–1500 B, FCS 4 B) |
| 8 | Mensajes | Sin mensajes de control de aplicación; tramas de datos y pause frames (flow control) |
| 9 | Campos | 7 campos catalogados en `F5-Campos-PDU.json` (PR-001) |
| 10 | Secuencia | Sin establecimiento; half-duplex histórico con CSMA/CD; full-duplex actual; PAUSE para control de flujo |
| 11 | Addressing/naming | MAC 48 bits: unicast/multicast/broadcast (FF:FF:FF:FF:FF:FF); OUI/registro de fabricantes |
| 12 | Routing/forwarding | Switches/bridges aprenden MAC (FDB); STP/RSTP/MSTP en topologías redundantes |
| 13 | Seguridad | Sin seguridad nativa; 802.1X (PR-077) en puerto; autenticación de origen MAC no criptográfica |
| 14 | QoS/rendimiento | PCP (802.1Q) / mapeo DSCP; velocidades por enmienda (10M–400G+) |
| 15 | Observabilidad | DA/SA, EtherType, longitud, FCS; errores de FCS visibles; filtros eth.* |
| 16 | Interoperabilidad | Coexistencia EtherType vs. Length (≤1500); jumbo frames; interoperabilidad amplia |
| 17 | Implementaciones | NICs, switches (ASIC), sistemas embebidos; herramientas de análisis |
| 18 | Fuentes | IEEE 802.3 (nivel 1); EtherTypes según IEEE/IANA (R1) — 26-08-2026 |

## F-13 · SSH — Secure Shell

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | SSH; RFC 4251 (arquitectura), 4253 (transporte), 4254 (conexión); IETF; familia APP |
| 2 | Estado | Vigente (26-08-2026; RFC 4253) |
| 3 | Finalidad | Acceso remoto y transferencia seguros: shell, ejecución de comandos, túnel de puertos y SFTP. **No usar** sin verificación de host keys |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **22** registrado en IANA) |
| 5 | Capas | OSI 7 (Aplicación) sobre TCP; plano: datos/gestión |
| 6 | Transporte y direccionamiento | Puerto TCP 22; multiplexa canales dentro de una conexión cifrada |
| 7 | PDU | **Paquete SSH** binario (longitud, padding, payload, MAC) |
| 8 | Mensajes | Protocolo de transporte (KEXINIT, NEWKEYS), mensajes de autenticación (USERAUTH), canales de conexión |
| 9 | Campos | Paquete: packet_length, padding_length, payload, random padding, MAC. (Detalle campo a campo pendiente en F5) |
| 10 | Secuencia | Handshake TCP → version exchange → Key Exchange (curve25519/ECDH) → NEWKEYS → autenticación → canales |
| 11 | Addressing/naming | host:port; identidad del servidor por host key (curva Ed25519/ECDSA/RSA) |
| 12 | Routing/forwarding | No (aplicación); puede tuntelar tráfico (forwarding de puertos) |
| 13 | Seguridad | Cifrado AEAD (ChaCha20-Poly1305, AES-GCM), MAC, autenticación por claves PKI o password; ver F6 |
| 14 | QoS/rendimiento | Multiplexación de canales; latencia por cifrado; algoritmos negociados |
| 15 | Observabilidad | Version, kex, algoritmos y host key visibles en la negociación (cifrada después); filtros ssh.* |
| 16 | Interoperabilidad | Muy amplia en servidores/clientes; variantes de algoritmos por implementación |
| 17 | Implementaciones | OpenSSH (referencia), Dropbear, PuTTY, librerías libssh/SSH.NET |
| 18 | Fuentes | RFC 4251-4254 (nivel 1); IANA R1 (puerto 22) — 26-08-2026 |

## F-14 · SMTP — Simple Mail Transfer Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | SMTP; RFC 5321 (SMTP), RFC 5322 (formato); IETF; familia APP |
| 2 | Estado | Vigente (26-08-2026; RFC 5321) |
| 3 | Finalidad | Envío y relaying de correo electrónico entre MTA/cliente. **No usar** para lectura de buzón (usar IMAP/POP3) |
| 4 | Encapsulación | Corre sobre **TCP** (puertos **25** MTA, **587** submission, **465** SMTPS registrados en IANA); STARTTLS opcional |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | TCP 25/587/465; direcciones mailbox@dominio |
| 7 | PDU | **Comando/respuesta SMTP** (líneas ASCII con códigos 3 dígitos) |
| 8 | Mensajes | EHLO/HELO, MAIL FROM, RCPT TO, DATA, QUIT; respuestas 2xx/3xx/4xx/5xx; STARTTLS |
| 9 | Campos | Sin cabecera binaria fija; comandos textuales y cabeceras RFC 5322 (From, To, Subject, Message-ID…) |
| 10 | Secuencia | Handshake TCP → EHLO → (STARTTLS) → MAIL → RCPT → DATA → QUIT; errores 4xx/5xx |
| 11 | Addressing/naming | Mailbox (usuario@dominio); enrutado por MX del dominio |
| 12 | Routing/forwarding | Relaying MTA a MTA con colas y reintentos; no es routing IP |
| 13 | Seguridad | Sin cifrado nativo (STARTTLS/ESMTPS opcional); autenticación SMTP-AUTH; ver F6 |
| 14 | QoS/rendimiento | Colas, reintentos con backoff; tamizado de mensajes |
| 15 | Observabilidad | Comandos, códigos y cabeceras visibles en claro (excepto STARTTLS); filtros smtp.* |
| 16 | Interoperabilidad | Universal en el ecosistema de correo; variantes en políticas (banners, límites) |
| 17 | Implementaciones | Postfix, Exim, Sendmail, Microsoft Exchange, servidores SMTP de nubes |
| 18 | Fuentes | RFC 5321/5322 (nivel 1); IANA R1 (puertos) — 26-08-2026 |

## F-15 · FTP — File Transfer Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | FTP; RFC 959 + extensiones (RFC 2428 EPRT/EPSV); IETF; familia APP |
| 2 | Estado | Vigente pero en desuso (26-08-2026; RFC 959); sustituido por SFTP/HTTPS |
| 3 | Finalidad | Transferencia de archivos con control y datos separados (modelo cliente-servidor). **No usar** en diseño nuevo (SFTP/FTPS) |
| 4 | Encapsulación | Corre sobre **TCP**: control en **21** y datos en **20** (activo) o puerto negociado (pasivo); registrados en IANA |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | TCP 21 control; datos 20 o efímero; direccionamiento por host + credenciales |
| 7 | PDU | **Comando/respuesta FTP** (textual; códigos 3 dígitos) |
| 8 | Mensajes | USER, PASS, PASV/EPSV, PORT/EPRT, TYPE, STOR, RETR, LIST; respuestas 1xx-5xx |
| 9 | Campos | Textual; sin cabecera binaria fija |
| 10 | Secuencia | Conexión de control → login → negociación de datos → transferencia → QUIT |
| 11 | Addressing/naming | host:port; rutas remotas; usuarios del servidor |
| 12 | Routing/forwarding | No; NAT requiere modo pasivo (EPSV) |
| 13 | Seguridad | Sin cifrado (claro); FTPS (AUTH TLS) y SFTP (SSH) como alternativas; ver F6 |
| 14 | QoS/rendimiento | Modo binario vs ASCII; reintentos; limitado por retardo de control |
| 15 | Observabilidad | Comandos y códigos en claro; filtros ftp/ftp-data.* |
| 16 | Interoperabilidad | Amplia en clientes/servidores; problemas clásicos con NAT en modo activo |
| 17 | Implementaciones | vsftpd, ProFTPD, FileZilla, IIS FTP |
| 18 | Fuentes | RFC 959, 2428 (nivel 1); IANA R1 (puertos 20/21) — 26-08-2026 |

## F-16 · SNMP — Simple Network Management Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | SNMP; RFC 3411-3418 (SNMPv3), RFC 1157 (v1); IETF; familia GEST |
| 2 | Estado | Vigente (26-08-2026; v3 recomendado) |
| 3 | Finalidad | Gestión y monitorización de dispositivos de red (lectura/escritura de variables MIB, traps). **No usar** para datos masivos (usar telemetría/NetFlow) |
| 4 | Encapsulación | Corre sobre **UDP** (puertos **161** agente, **162** trap, **10161/10162** TLS registrados en IANA); también puede ir sobre TCP |
| 5 | Capas | OSI 7 (Aplicación de gestión); plano: gestión |
| 6 | Transporte y direccionamiento | UDP 161/162; OIDs (1.3.6.1.2.1…) |
| 7 | PDU | **Mensaje SNMP** (BER/ASN.1: version, community/usm, PDU) |
| 8 | Mensajes | GET, GETNEXT, GETBULK, SET, RESPONSE, TRAP/INFORM, REPORT (v3 modela USM) |
| 9 | Campos | Cabecera BER (version, community, PDU-type, request-id, error-status, error-index, varbind list). Detalle en F5 si se cataloga |
| 10 | Secuencia | Get/GetNext → Response; Trap generado por el agente; inform polling |
| 11 | Addressing/naming | host:port; objetos por OID en MIBs |
| 12 | Routing/forwarding | No (gestión); enrutable como aplicación UDP |
| 13 | Seguridad | v1/v2c community en claro; v3: USM (autenticación HMAC, cifrado AES/DES); ver F6 |
| 14 | QoS/rendimiento | Polling con tiempos de espera; GETBULK para eficiencia |
| 15 | Observabilidad | OIDs, valores y traps visibles; filtros snmp.* |
| 16 | Interoperabilidad | Amplia en gestión de red; variantes MIB por fabricante |
| 17 | Implementaciones | net-snmp (agente y herramientas), PRTG, Zabbix, SolarWinds |
| 18 | Fuentes | RFC 3411-3418, 1157 (nivel 1); IANA R1 — 26-08-2026 |

## F-17 · NTP — Network Time Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | NTP; RFC 5905 (NTPv4); IETF; familia SYNC |
| 2 | Estado | Vigente (26-08-2026; RFC 5905) |
| 3 | Finalidad | Sincronización de relojes en red con precisión de ms (statinos jerárquicos, estratos). **No usar** cuando se exige precisión de sub-µs (PTP) |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **123** registrado en IANA) |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | UDP 123; direcciones de servidores de tiempo |
| 7 | PDU | **Paquete NTP** de 48 B (cabecera fija) |
| 8 | Mensajes | Modos: symmetric active/passive, client, server, broadcast, control; mensajes de sincronización |
| 9 | Campos | LI, VN, Mode, Stratum, Poll, Precision, Root Delay/Dispersion, Reference ID, timestamps (48 B). Detalle en F5 |
| 10 | Secuencia | Cliente → Respuesta; selección de mejores fuentes (Marzullo), disciplina del reloj local |
| 11 | Addressing/naming | Servidores de estrato 0-15; pool de tiempo |
| 12 | Routing/forwarding | No (sincronización); enrutable como UDP |
| 13 | Seguridad | NTP clásico sin autenticación (vulnerable a spoofing); NTS y autokey como extensiones; ver F6 |
| 14 | QoS/rendimiento | Stratum, offsets, jitter; polling adaptativo |
| 15 | Observabilidad | Version/mode, stratum, timestamps visibles; filtros ntp.* |
| 16 | Interoperabilidad | Amplia en sistemas operativos y hardware |
| 17 | Implementaciones | ntpd, chrony, Windows Time, NTP pools |
| 18 | Fuentes | RFC 5905 (nivel 1); IANA R1 (puerto 123) — 26-08-2026 |

## F-18 · HTTP/1.1 — Hypertext Transfer Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | HTTP/1.1; RFC 9112 (semántica RFC 9110); IETF; familia APP |
| 2 | Estado | Vigente (26-08-2026; RFC 9112) |
| 3 | Finalidad | Transferencia de recursos (documentos, API) cliente-servidor con métodos y códigos de estado. **No usar** para streaming bidireccional continuo (WebSocket/gRPC) |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **80** registrado; **443** con TLS) |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | TCP 80/443; URIs y cabeceras Host; sin estado (cookies para sesiones) |
| 7 | PDU | **Mensaje HTTP** (request/response: request line o status line + cabeceras + cuerpo) |
| 8 | Mensajes | Request: métodos GET, POST, PUT, DELETE, HEAD, OPTIONS…; Response: 1xx-5xx |
| 9 | Campos | Línea de petición/estado, cabeceras (Host, Content-Length, Transfer-Encoding, Connection…) — textual, sin cabecera binaria fija |
| 10 | Secuencia | Request → Response; reutilización de conexiones (keep-alive) y pipelining limitado |
| 11 | Addressing/naming | URI + Host; redirecciones 3xx |
| 12 | Routing/forwarding | Proxies y gateways (Forwarded, Via); no es routing IP |
| 13 | Seguridad | En claro sin TLS (HTTPS encima); autenticación Basic/Digest; ver F6 |
| 14 | QoS/rendimiento | Conexiones persistentes; head-of-line blocking; compresión de cabeceras ausente (vs HTTP/2) |
| 15 | Observabilidad | Métodos, URIs, cabeceras y códigos visibles; filtros http.* |
| 16 | Interoperabilidad | Universal; variantes en cabeceras y límites de servidores |
| 17 | Implementaciones | Apache, nginx, IIS, browsers, librerías HTTP (libcurl, requests…) |
| 18 | Fuentes | RFC 9112/9110 (nivel 1); IANA R1 — 26-08-2026 |

## F-19 · HTTP/2 — Hypertext Transfer Protocol version 2

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | HTTP/2; RFC 9113; IETF; familia APP |
| 2 | Estado | Vigente (26-08-2026; RFC 9113) |
| 3 | Finalidad | HTTP sobre conexión única TCP multiplexada en streams con compresión de cabeceras (HPACK). **No usar** donde falte soporte (coexistencia HTTP/1.1) |
| 4 | Encapsulación | Corre sobre **TCP** (h2 con TLS, ALPN "h2"; h2c sin TLS en texto claro) |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | TCP con TLS (443 habitual); streams con priorización |
| 7 | PDU | **Frame HTTP/2** (cabecera 9 B: length, type, flags, stream-id + payload) |
| 8 | Mensajes | Frames: DATA, HEADERS, SETTINGS, WINDOW_UPDATE, PUSH_PROMISE (retirado server push en RFC 9113), GOAWAY |
| 9 | Campos | Cabecera de frame (9 B): Length(24) + Type(8) + Flags(8) + R/StreamID(32). (Detalle en F5) |
| 10 | Secuencia | Handshake TCP+TLS (ALPN h2) → settings de arranque → streams multiplexados → GOAWAY |
| 11 | Addressing/naming | URI + authority (pseudo-header :authority) |
| 12 | Routing/forwarding | No (aplicación); proxies h2 |
| 13 | Seguridad | h2 sin TLS (h2c) en claro; con TLS normalmente; ver F6 |
| 14 | QoS/rendimiento | Multiplexación elimina head-of-line blocking de HTTP/1.1; prioridades de streams; compresión HPACK |
| 15 | Observabilidad | Frames y streams visibles; filtros http2.* |
| 16 | Interoperabilidad | Amplia en navegadores y servidores; negociación por ALPN |
| 17 | Implementaciones | nginx, Apache, HTTP.sys, browsers, librerías (nghttp2, h2o) |
| 18 | Fuentes | RFC 9113 (nivel 1); IANA R1 — 26-08-2026 |

## F-20 · RIP — Routing Information Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | RIPv2; RFC 2453 (RIPv1 en RFC 1058); IETF; familia ROUT |
| 2 | Estado | Vigente en entornos legados (26-08-2026; RFC 2453); ámbito pequeño |
| 3 | Finalidad | Distribución de rutas por vector de distancias (métrica = saltos) dentro de un dominio pequeño. **No usar** en redes grandes (convergencia lenta) |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **520** registrado en IANA); RIPv1 broadcast; RIPv2 multicast 224.0.0.9 |
| 5 | Capas | OSI 7/Aplicación sobre transporte; plano: control |
| 6 | Transporte y direccionamiento | UDP 520; tablas de rutas con métricas 1-15 (16 = inalcanzable) |
| 7 | PDU | **Mensaje RIP** (cabecera + entradas de ruta de 20 B) |
| 8 | Mensajes | Request / Response; actualizaciones periódicas y trigged updates; poison reverse / split horizon |
| 9 | Campos | Cabecera: command(8) + version(8) + zero(16); entrada: AFI, route tag, address, mask, next hop, metric. (Detalle en F5) |
| 10 | Secuencia | Request → Response; convergencia por conteo a infinito mitigado con split horizon |
| 11 | Addressing/naming | Prefijos de red + métrica |
| 12 | Routing/forwarding | Selección por menor métrica; timers (update/expire/garbage) |
| 13 | Seguridad | Autenticación simple (passphrase) en RIPv2; sin cifrado; ver F6 |
| 14 | QoS/rendimiento | Convergencia lenta en topologías grandes; límite de 15 saltos |
| 15 | Observabilidad | Tablas y mensajes visibles; filtros rip.* |
| 16 | Interoperabilidad | RIPv1/RIPv2 compatibles en modos concretos; obsoleto frente a OSPF/IS-IS |
| 17 | Implementaciones | FRRouting, bird, RIP dentro de Linux (routed)/Windows legacy |
| 18 | Fuentes | RFC 2453, 1058 (nivel 1); IANA R1 — 26-08-2026 |

## F-21 · IS-IS — Intermediate System to Intermediate System

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | IS-IS; ISO 10589 (integrado, doble métrica); IETF RFC 7142; ISO/IEC; familia ROUT |
| 2 | Estado | Vigente (26-08-2026; RFC 7142/ISO 10589) |
| 3 | Finalidad | IGP link-state dentro de un dominio (también IPv4/IPv6 mediante extensions); cálculo SPF; áreas y niveles L1/L2. **No usar** para interdominio (BGP) |
| 4 | Encapsulación | Corre **directamente sobre la capa de enlace** (normalmente Ethernet 802.3; no usa IP para el protocolo base) |
| 5 | Capas | OSI 3 (Red); plano: control |
| 6 | Transporte y direccionamiento | Sin IP ni puertos: PDUs sobre LLC (SNAP 0xFE); dirección MAC multicast 01:80:c2:00:00:14/15 |
| 7 | PDU | **PDU IS-IS** (variantes: IIH, LSP, SNP) |
| 8 | Mensajes | Hello (IIH), LSPs, CSNP/PSNP (sincronización de LSDB); elección de DIS |
| 9 | Campos | Cabecera: intradomain routing protocol discriminator, length indicator, version, ID length, PDU type, version, reserved, max area. (Detalle en F5 si se cataloga) |
| 10 | Secuencia | IIH → adyacencias → intercambio de LSPs → SPF → routers IP y rutas |
| 11 | Addressing/naming | System ID (6 B) y NET; áreas |
| 12 | Routing/forwarding | Dijkstra (SPF); L1 dentro de área, L2 entre áreas; rutas por métrica |
| 13 | Seguridad | Autenticación simple de PDUs (clave/lanzar pasword); sin cifrado; ver F6 |
| 14 | QoS/rendimiento | Convergencia rápida; sin conteo de saltos limitado |
| 15 | Observabilidad | PDUs y LSDB visibles; filtros isis.* |
| 16 | Interoperabilidad | Común en transporte y proveedores; coexistencia con OSPF |
| 17 | Implementaciones | FRRouting, Junos, Cisco IOS, GoBGP (no, IS-IS: BIRD sí) |
| 18 | Fuentes | ISO 10589 / RFC 7142 (nivel 1) — 26-08-2026 |

## F-22 · SCTP — Stream Control Transmission Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | SCTP; RFC 9260 (obsoleta 4960); IETF; familia TRAN |
| 2 | Estado | Vigente (26-08-2026; RFC 9260) |
| 3 | Finalidad | Transporte orientado a conexión con múltiples streams y multi-homing, para señalización y datos. **No usar** cuando la pila no lo soporta (TCP/UDP alternativos) |
| 4 | Encapsulación | Corre sobre **IPv4/IPv6** (IP protocol number **132**); no usa puertos TCP/UDP |
| 5 | Capas | OSI 4 (Transporte); plano: datos |
| 6 | Transporte y direccionamiento | Número de protocolo IP 132; puertos SCTP; multi-homing |
| 7 | PDU | **Paquete SCTP** (cabecera común de 12 B + chunks) |
| 8 | Mensajes | Chunks: DATA, INIT, INIT-ACK, SACK, HEARTBEAT, SHUTDOWN, COOKIE_ECHO/ACK |
| 9 | Campos | Cabecera: source port(16) + dest port(16) + verification tag(32) + checksum(32); chunks con type(8) flags(8) length(16). (Detalle en F5) |
| 10 | Secuencia | Handshake 4-way (INIT → INIT-ACK → COOKIE_ECHO → COOKIE-ACK) → datos por streams → SHUTDOWN |
| 11 | Addressing/naming | Puertos SCTP (usados por SS7/SIGTRAN, WebRTC? no: WebRTC usa DTLS) |
| 12 | Routing/forwarding | No participa (transporte); multi-homing para resiliencia |
| 13 | Seguridad | Sin cifrado nativo; checksum CRC32c; DTLS-SCTP para seguridad; ver F6 |
| 14 | QoS/rendimiento | Streams independientes (sin HOL blocking), selectividad de retransmisión, path failover |
| 15 | Observabilidad | Chunks y transmisiones visibles; filtros sctp.* |
| 16 | Interoperabilidad | Amplia en telecom (SIGTRAN); menor en internet general |
| 17 | Implementaciones | Pilas Linux/BSD, lksctp; usados por Diameter/SIGTRAN |
| 18 | Fuentes | RFC 9260 (nivel 1); IANA R1 — 26-08-2026 |

## F-23 · DTLS — Datagram Transport Layer Security

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | DTLS; RFC 9147 (DTLS 1.3), RFC 6347 (1.2); IETF; familia SEG |
| 2 | Estado | Vigente (26-08-2026; RFC 9147) |
| 3 | Finalidad | Seguridad (confidencialidad, integridad, autenticación) sobre transportes de datagramas (UDP) sin entrega garantizada. **No usar** si el transporte es TCP (usar TLS) |
| 4 | Encapsulación | Corre sobre **UDP** (p. ej. puerto 443 para QUIC/WebRTC DTLS; cualquier puerto de la app) |
| 5 | Capas | OSI 4.5/6 (entre Transporte y Aplicación); plano: seguridad |
| 6 | Transporte y direccionamiento | UDP; reordenación/pérdida manejadas de forma explícita; puerto de la aplicación |
| 7 | PDU | **Record DTLS** (como TLS + epoch y sequence_number explícitos) |
| 8 | Mensajes | Handshake (con retransmisión y HelloVerifyRequest anti-amplificación), Alert, Application Data |
| 9 | Campos | Record: content type(8)+version(16)+epoch(16)+sequence(48)+length(16)+payload. (Detalle en F5) |
| 10 | Secuencia | HelloVerifyRequest → handshake con reintentos (timer) → datos; epoch cambia tras el handshake |
| 11 | Addressing/naming | Identidad por certificado X.509 |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | AEAD (AES-GCM, ChaCha20-Poly1305), PFS; protege frente a pérdidas/reordenación; ver F6 |
| 14 | QoS/rendimiento | Overhead por reintentos del handshake; latency por vuelos |
| 15 | Observabilidad | Handshake visible en claro (CN, version); tráfico de datos cifrado |
| 16 | Interoperabilidad | CoAP (RFC 7252), WebRTC (DTLS-SRTP), QUIC | 
| 17 | Implementaciones | OpenSSL, BoringSSL, NSS, mbedTLS, WolfSSL |
| 18 | Fuentes | RFC 9147/6347 (nivel 1) — 26-08-2026 |

## F-24 · IPsec (ESP) — Encapsulating Security Payload

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | IPsec ESP; RFC 4303 (ESP), RFC 4301 (arquitectura); IETF; familia SEG |
| 2 | Estado | Vigente (26-08-2026; RFC 4303) |
| 3 | Finalidad | Confidencialidad, integridad y autenticación a nivel de red (datagramas IP completos o transportes). **No usar** cuando se necesita seguridad por aplicación (TLS encima de transporte) |
| 4 | Encapsulación | Corre sobre **IPv4/IPv6** (IP protocol number **50**); modos transporte y túnel |
| 5 | Capas | OSI 3 (Red); plano: seguridad |
| 6 | Transporte y direccionamiento | Protocolo IP 50; SPI (Security Parameter Index); sin puertos |
| 7 | PDU | **Paquete ESP** (SPI, sequence, payload cifrado, padding, ICV) |
| 8 | Mensajes | Sin mensajes de aplicación; SA negociada por IKE; tráfico de datos cifrado |
| 9 | Campos | SPI(32)+Sequence(32)+Payload+PadLength(8)+NextHeader(8)+ICV. (Detalle en F5) |
| 10 | Secuencia | SA establecida por IKEv2 → datos protegidos; renegociación por lifetime |
| 11 | Addressing/naming | SPIs y SAs; identificadores de tráfico (selector) |
| 12 | Routing/forwarding | Túneles IPsec como rutas; integración con encaminamiento de red |
| 13 | Seguridad | AEAD (AES-GCM, ChaCha20-Poly1305), anti-replay por ventana; ver F6 |
| 14 | QoS/rendimiento | Overhead de cabecera ESP; procesado criptográfico por paquete |
| 15 | Observabilidad | SPI y sequence visibles en claro; payload cifrado |
| 16 | Interoperabilidad | Amplia en VPN gateway-to-gateway; problemas NAT con IKE/ESP (NAT-T) |
| 17 | Implementaciones | StrongSwan, libreswan, kernel Linux (XFRM), Windows RRAS |
| 18 | Fuentes | RFC 4301/4303 (nivel 1); IANA R1 — 26-08-2026 |

## F-25 · IKE — Internet Key Exchange

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | IKEv2; RFC 7296 (obsoleta 5996); IETF; familia SEG |
| 2 | Estado | Vigente (26-08-2026; RFC 7296) |
| 3 | Finalidad | Establecimiento y gestión de SAs IPsec: autenticación, intercambio de claves y negociación de algoritmos. **No usar** sin IPsec (es su plano de control) |
| 4 | Encapsulación | Corre sobre **UDP** (puertos **500** y **4500** NAT-T, registrados en IANA) |
| 5 | Capas | OSI 7/Aplicación de control; plano: seguridad/control |
| 6 | Transporte y direccionamiento | UDP 500/4500; IKE SA + Child SAs |
| 7 | PDU | **Paquete IKE** (cabecera + payloads; intercambios IKE_SA_INIT/CREATE_CHILD_SA) |
| 8 | Mensajes | IKE_SA_INIT (offers), IKE_AUTH (IDENTITY, AUTH, certificados), CREATE_CHILD_SA, INFORMATIONAL, DELETE |
| 9 | Campos | Cabecera: IKE_SA_initiator SPI(64)+responder SPI(64)+next payload(8)+version(8)+exchange type(8)+flags(8)+message ID(32)+length(32). (Detalle en F5) |
| 10 | Secuencia | IKE_SA_INIT (2 mensajes) → IKE_AUTH (2) → Child SA (1-2); rekeying DPD |
| 11 | Addressing/naming | Identidades (IP, FQDN, DN); SPI pairs |
| 12 | Routing/forwarding | No (control); el tráfico protegido va por ESP/AH |
| 13 | Seguridad | Autenticación PSK/certificados/EAP; PFS en Child SAs; anti-replay en ESP; ver F6 |
| 14 | QoS/rendimiento | Rekeying y DPD; latencia del handshake (2 round-trips) |
| 15 | Observabilidad | SPIs y cifrado negociado visibles; contenido cifrado |
| 16 | Interoperabilidad | Muy amplia en VPNs; variantes en propuestas de algoritmos |
| 17 | Implementaciones | StrongSwan, libreswan, Windows, Cisco/Juniper |
| 18 | Fuentes | RFC 7296 (nivel 1); IANA R1 — 26-08-2026 |

## F-26 · Kerberos

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Kerberos v5; RFC 4120; IETF; familia SEG |
| 2 | Estado | Vigente (26-08-2026; RFC 4120; extensión PKINIT RFC 4556) |
| 3 | Finalidad | Autenticación de red basada en tickets (TGT/TGS) sin transmitir contraseñas por la red. **No usar** para confidencialidad de datos (solo autenticación) |
| 4 | Encapsulación | Corre sobre **UDP/TCP** (puerto **88** registrado en IANA) |
| 5 | Capas | OSI 7 (Aplicación de autenticación); plano: seguridad |
| 6 | Transporte y direccionamiento | UDP 88 (preferido) / TCP 88; realm + principal |
| 7 | PDU | **Mensaje Kerberos** (AS-REQ/AS-REP, TGS-REQ/REP, AP-REQ/REP) codificado ASN.1 |
| 8 | Mensajes | AS-REQ/REP (TGT), TGS-REQ/REP (service ticket), AP-REQ/REP (aplicación), KRB_ERROR |
| 9 | Campos | Tipos de mensaje, realm, principal names, tickets cifrados con claves de sesión. (ASN.1; detalle en F5 si se cataloga) |
| 10 | Secuencia | AS-REQ → AS-REP (TGT); TGS-REQ → TGS-REP; AP-REQ en la aplicación; renovación |
| 11 | Addressing/naming | Principals (user@REALM); realm jerárquicos |
| 12 | Routing/forwarding | No (autenticación) |
| 13 | Seguridad | Cifrado de tickets (AES), timestamps anti-replay, preauthentication; ver F6 |
| 14 | QoS/rendimiento | Tickets con validez (TGT por defecto ~10h); caché de credenciales |
| 15 | Observabilidad | Message types y realms visibles; tickets cifrados |
| 16 | Interoperabilidad | Dominio de Microsoft (Active Directory), MIT Kerberos, Heimdal |
| 17 | Implementaciones | Windows AD, MIT krb5, Heimdal, SSSD |
| 18 | Fuentes | RFC 4120 (nivel 1); IANA R1 — 26-08-2026 |

## F-27 · RADIUS — Remote Authentication Dial-In User Service

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | RADIUS; RFC 2865 (auten/accounting RFC 2866); IETF; familia GEST |
| 2 | Estado | Vigente (26-08-2026; RFC 2865; RFC 5080 compat) |
| 3 | Finalidad | AAA (autenticación, autorización, accounting) para acceso a red (Wi-Fi, VPN, NAS). **No usar** cuando se requiere reenvío de atributos complejos (RADIUS) sin extensiones (usar Diameter para carga alta) |
| 4 | Encapsulación | Corre sobre **UDP** (puertos **1812** autenticación, **1813** accounting registrados en IANA; 1645/1646 legacy) |
| 5 | Capas | OSI 7 (Aplicación AAA); plano: gestión/seguridad |
| 6 | Transporte y direccionamiento | UDP 1812/1813; atributos TLV; shared secret |
| 7 | PDU | **Paquete RADIUS** (code + identifier + length + authenticator + atributos) |
| 8 | Mensajes | Access-Request, Access-Accept/Reject/Challenge, Accounting-Request/Response, CoA/Disconnect |
| 9 | Campos | Code(8)+Identifier(8)+Length(16)+Authenticator(128)+Attrs (Type(8)+Len(8)+Value). (Detalle en F5) |
| 10 | Secuencia | Access-Request → Accept/Reject/Challenge; reenvío por proxies; Retransmission |
| 11 | Addressing/naming | NAS + user; atributos (User-Name, NAS-IP, Framed-IP…) |
| 12 | Routing/forwarding | Proxies y realm routing (pero no routing IP) |
| 13 | Seguridad | Authenticator/hash del shared secret; password cifrada con MD5 (método antiguo); ver F6 |
| 14 | QoS/rendimiento | Timeouts y reintentos; attribute size limits |
| 15 | Observabilidad | Codes y atributos visibles; password ofuscada |
| 16 | Interoperabilidad | Estándar en Wi-Fi empresarial (WPA-Enterprise), VPN, NAS; backend para 802.1X |
| 17 | Implementaciones | FreeRADIUS, Windows NPS, Cisco ACS/ISE, PacketFence |
| 18 | Fuentes | RFC 2865/2866 (nivel 1); IANA R1 — 26-08-2026 |

## F-28 · MQTT — Message Queuing Telemetry Transport

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | MQTT; OASIS MQTT 3.1.1/5.0 (ISO/IEC 20922); familia IOT |
| 2 | Estado | Vigente (26-08-2026; MQTT 5.0 OASIS) |
| 3 | Finalidad | Mensajería publish/subscribe ligera para IoT/telemetría con brokers y topics. **No usar** para streaming de alta tasa (usar Kafka/AMQP) |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **1883** registrado; **8883** con TLS) |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | TCP 1883/8883; topics jerárquicos (a/b/c) y QoS 0-2 |
| 7 | PDU | **Paquete MQTT** (cabecera fija 2-5 B + variable header + payload) |
| 8 | Mensajes | CONNECT, CONNACK, PUBLISH, SUBSCRIBE, SUBACK, PINGREQ/PINGRESP, DISCONNECT |
| 9 | Campos | Cabecera: type+flags(8) + remaining length (varint) + variable header (protocol name/level, flags, keepalive…). (Detalle en F5) |
| 10 | Secuencia | CONNECT → CONNACK → PUBLISH/SUBSCRIBE; keepalive; DISCONNECT |
| 11 | Addressing/naming | Topics + Client ID; sesiones persistentes |
| 12 | Routing/forwarding | Brokers enrutan por topics (no routing IP) |
| 13 | Seguridad | Sin cifrado nativo (TLS encima); autenticación user/pass; ver F6 |
| 14 | QoS/rendimiento | QoS 0-2 (at most once, at least once, exactly once); retain; wills |
| 15 | Observabilidad | Paquetes y topics visibles; filtros mqtt.* |
| 16 | Interoperabilidad | Muy amplia en IoT; brokers y clientes estandarizados OASIS |
| 17 | Implementaciones | Mosquitto, HiveMQ, EMQX; clientes (paho, mosquitto clients) |
| 18 | Fuentes | OASIS MQTT 5.0 (nivel 1); IANA R1 — 26-08-2026 |

## F-29 · CoAP — Constrained Application Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | CoAP; RFC 7252 (CoAP), RFC 7641 (observe), RFC 7959 (blockwise); IETF; familia IOT |
| 2 | Estado | Vigente (26-08-2026; RFC 7252) |
| 3 | Finalidad | Web de las cosas sobre UDP para dispositivos restringidos: métodos HTTP-like y observación. **No usar** cuando el dispositivo soporta HTTP/TCP sin restricciones |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **5683** registrado; **5684** con DTLS); también puede usar CoAP over TCP/TLS (RFC 8323) |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | UDP 5683/5684; URIs coap:// y opciones |
| 7 | PDU | **Mensaje CoAP** (cabecera 4 B: ver/type/TKL + code + message ID + tokens + opciones) |
| 8 | Mensajes | Confirmed/Non-confirmed, ACK, RST; métodos GET, PUT, POST, DELETE; code 2.05, 4.04… |
| 9 | Campos | Ver(2)+Type(2)+TKL(4)+Code(8)+Message ID(16)+Token+Options (UDP format). (Detalle en F5) |
| 10 | Secuencia | Confirmable → ACK con retransmisión; observe (RFC 7641) notificaciones |
| 11 | Addressing/naming | URIs CoAP; multicast para descubrimiento (/.well-known/core) |
| 12 | Routing/forwarding | No (aplicación); proxies CoAP→HTTP |
| 13 | Seguridad | DTLS (CoAPs), OSCORE (RFC 8613) end-to-end; ver F6 |
| 14 | QoS/rendimiento | Message size pequeño; blockwise para payloads grandes; retransmisión |
| 15 | Observabilidad | Codes y opciones visibles; filtros coap.* |
| 16 | Interoperabilidad | Estándar en IoT (CoAP/DTLS); interop con HTTP vía proxy |
| 17 | Implementaciones | Californium, CoAPthon, libcoap, RIOT/Contiki |
| 18 | Fuentes | RFC 7252 (nivel 1); IANA R1 — 26-08-2026 |

## F-30 · QUIC

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | QUIC; RFC 9000 (transporte), RFC 9001 (TLS), RFC 9002 (pérdidas); IETF; familia TRAN |
| 2 | Estado | Vigente (26-08-2026; RFC 9000) |
| 3 | Finalidad | Transporte sobre UDP con cifrado integrado (TLS 1.3), conexión multiplexada y menor latencia de establecimiento. **No usar** donde se exige compatibilidad total (HTTP/1.1–2 siguen en TCP) |
| 4 | Encapsulación | Corre sobre **UDP** (puerto 443 habitual; registrado como UDP QUIC) |
| 5 | Capas | OSI 4 (Transporte), integra TLS 1.3; plano: datos |
| 6 | Transporte y direccionamiento | UDP 443; connection ID (migración de conexión) |
| 7 | PDU | **Paquete QUIC** (cabecera larga/corta + frames) |
| 8 | Mensajes | Frames: STREAM, ACK, CRYPTO, NEW_CONNECTION_ID, HANDSHAKE_DONE… |
| 9 | Campos | Cabecera: version, DCID/SCID, packet number, tipo de paquete. (Detalle en F5) |
| 10 | Secuencia | Handshake TLS 1.3 integrado (1-RTT); 0-RTT opcional; streams sobre conexión única |
| 11 | Addressing/naming | Connection ID; URIs de aplicación (HTTP/3) |
| 12 | Routing/forwarding | No (transporte); migración entre redes |
| 13 | Seguridad | TLS 1.3 integrado (todo cifrado salvo cabecera de paquete); ver F6 |
| 14 | QoS/rendimiento | 0-RTT, multiplexación sin HOL blocking, control de congestión propio |
| 15 | Observabilidad | CID, version y frames visibles en claro; filtros quic.* |
| 16 | Interoperabilidad | Crecimiento rápido en web (HTTP/3, RFC 9114) |
| 17 | Implementaciones | nginx/QUIC, msquic, quiche, browsers (Chromium, Firefox, Safari) |
| 18 | Fuentes | RFC 9000 / 9001 / 9002 (nivel 1); IANA R1 — 26-08-2026 |

## F-31 · RTP — Real-time Transport Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | RTP; RFC 3550 (perfiles RFC 3551); IETF; familia TRAN |
| 2 | Estado | Vigente (26-08-2026; RFC 3550) |
| 3 | Finalidad | Transporte de flujos multimedia en tiempo real (audio/vídeo) con marcas de tiempo y secuencia. **No usar** para datos fiables (usar TCP/QUIC) |
| 4 | Encapsulación | Corre sobre **UDP** (puertos negociados por RTSP/SIP; típicamente 1024-65535) |
| 5 | Capas | OSI 7/Aplicación sobre transporte; plano: datos |
| 6 | Transporte y direccionamiento | UDP; ports pares (RTP) e impares (RTCP), p. ej. 5004/5005 |
| 7 | PDU | **Paquete RTP** (cabecera fija de 12 B + payload) |
| 8 | Mensajes | Paquetes de datos (perfil: G.711, H.264, Opus…); RTCP para control |
| 9 | Campos | V(2)+P+X+CC(8)+M+PT(8)+Sequence(16)+Timestamp(32)+SSRC(32)+CSRC. (Detalle en F5) |
| 10 | Secuencia | Flujo continuo de paquetes; RTCP sender/receiver reports periódicos |
| 11 | Addressing/naming | SSRC/CSRC; sesiones multicast posibles |
| 12 | Routing/forwarding | No (los routers no tocan RTP); mixers/translators |
| 13 | Seguridad | Sin cifrado nativo (SRTP añade AEAD); ver F6 |
| 14 | QoS/rendimiento | Jitter buffer, timestamps, sequence para reordenación |
| 15 | Observabilidad | Payload type y SSRC visibles; filtros rtp.* |
| 16 | Interoperabilidad | Amplia en VoIP/videoconferencia; perfil por PT |
| 17 | Implementaciones | GStreamer, FFmpeg, WebRTC (sobre SRTP), libcover? (libav) |
| 18 | Fuentes | RFC 3550/3551 (nivel 1) — 26-08-2026 |

## F-32 · RTCP — RTP Control Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | RTCP; RFC 3550 (mismo documento que RTP); IETF; familia TRAN |
| 2 | Estado | Vigente (26-08-2026; RFC 3550) |
| 3 | Finalidad | Control y métricas de la sesión RTP: reportes de calidad, sincronización, participantes. **No usar** de forma independiente (acompaña a RTP) |
| 4 | Encapsulación | Corre sobre **UDP** (puerto siguiente al del RTP, normalmente par+1) |
| 5 | Capas | OSI 7/Aplicación sobre transporte; plano: control |
| 6 | Transporte y direccionamiento | UDP; puerto RTP+1 |
| 7 | PDU | **Paquete RTCP** (cabecera + bloques; SR, RR, SDES, BYE, APP) |
| 8 | Mensajes | Sender Report (SR), Receiver Report (RR), SDES, BYE, APP |
| 9 | Campos | V(2)+P+RC(8)+PT(8)+Length(16)+SSRC(32)+bloques de recepción. (Detalle en F5) |
| 10 | Secuencia | Reportes periódicos (~5% del ancho de banda); BYE al salir |
| 11 | Addressing/naming | SSRC; CANONICAL NAME (SDES CNAME) para correlación |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | Sin cifrado nativo (SRTP incluye SRTCP); ver F6 |
| 14 | QoS/rendimiento | Round-trip estimates, pérdida/jitter reportados |
| 15 | Observabilidad | Tipos SR/RR y métricas visibles; filtros rtcp.* |
| 16 | Interoperabilidad | Amplia; escalado por intervalos |
| 17 | Implementaciones | GStreamer, FFmpeg, stacks VoIP |
| 18 | Fuentes | RFC 3550 (nivel 1) — 26-08-2026 |

## F-33 · SIP — Session Initiation Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | SIP; RFC 3261 (y actualizaciones); IETF; familia APP |
| 2 | Estado | Vigente (26-08-2026; RFC 3261) |
| 3 | Finalidad | Señalización para establecer, modificar y terminar sesiones multimedia (VoIP, vídeo). **No usar** para transportar el contenido (va por RTP) |
| 4 | Encapsulación | Corre sobre **UDP** (habitual, puerto **5060**; **5061** TLS, registrados en IANA) y **TCP** |
| 5 | Capas | OSI 7 (Señalización); plano: control |
| 6 | Transporte y direccionamiento | UDP/TCP 5060/5061; URIs sip:user@domain |
| 7 | PDU | **Mensaje SIP** (request/response textual con cabeceras) |
| 8 | Mensajes | INVITE, ACK, BYE, CANCEL, REGISTER, OPTIONS, SUBSCRIBE/NOTIFY; respuestas 1xx-6xx |
| 9 | Campos | Cabeceras: Via, From, To, Call-ID, CSeq, Contact, Max-Forwards… (textual, sin cabecera binaria fija) |
| 10 | Secuencia | REGISTER → (INVITE → 100/180/200 → ACK) → BYE; diálogos y transacciones |
| 11 | Addressing/naming | URIs SIP; AOR y contactos |
| 12 | Routing/forwarding | Proxies (forking), record-route; no es routing IP |
| 13 | Seguridad | Sin cifrado nativo (SIPS/TLS opcional; digest auth); ver F6 |
| 14 | QoS/rendimiento | Integración con SDP para codecs; retransmisión sobre UDP |
| 15 | Observabilidad | Métodos, URIs y cabeceras visibles; filtros sip.* |
| 16 | Interoperabilidad | Amplia en VoIP; interoperabilidad con H.323 decreciente |
| 17 | Implementaciones | Asterisk, FreeSWITCH, Kamailio, microSIP, Jitsi |
| 18 | Fuentes | RFC 3261 (nivel 1); IANA R1 — 26-08-2026 |

## F-34 · XMPP — Extensible Messaging and Presence Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | XMPP (Jabber); RFC 6120 (core), 6121 (messaging), 6122 (addr); IETF/XSF; familia APP |
| 2 | Estado | Vigente (26-08-2026; RFC 6120) |
| 3 | Finalidad | Mensajería instantánea y presencia en tiempo real basada en XML (streams). **No usar** para APIs de propósito general (JSON/REST más simples) |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **5222** clientes, **5269** servidores, registrados en IANA); STARTTLS obligatorio en la práctica |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | TCP 5222/5269; JID (local@domain/resource) |
| 7 | PDU | **Stanza XML** (message, presence, iq) entre streams |
| 8 | Mensajes | <message/>, <presence/>, <iq type="get|set|result|error"/>; extensiones XEP |
| 9 | Campos | Elementos XML: from, to, id, type + payload (etiquetas). (Detalle en F5) |
| 10 | Secuencia | TCP → stream header → STARTTLS → SASL → bind → presence/message |
| 11 | Addressing/naming | JID; recursos; bare vs full JID |
| 12 | Routing/forwarding | Servidores enrutan por dominio (S2S); no es routing IP |
| 13 | Seguridad | TLS para streams, SASL (SCRAM), OMEMO para E2E; ver F6 |
| 14 | QoS/rendimiento | Presencia/buddy lists; QoS simple (sin garantías fuertes) |
| 15 | Observabilidad | Stanzas y JIDs visibles; filtros xmpp/jabber.* |
| 16 | Interoperabilidad | Estándar abierto; federación de servidores |
| 17 | Implementaciones | Prosody, Ejabberd, Openfire; clientes (Gajim, Conversations) |
| 18 | Fuentes | RFC 6120/6121 (nivel 1); IANA R1 — 26-08-2026 |

## F-35 · NFS — Network File System

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | NFS; NFSv4.2 RFC 7862 (v4 RFC 7530, v3 RFC 1813); IETF; familia STOR |
| 2 | Estado | Vigente (26-08-2026; RFC 7530/7862) |
| 3 | Finalidad | Acceso a archivos remotos sobre la red como filesystem (clientes montan directorios). **No usar** para transferencia puntual (SCP/HTTP) |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **2049** registrado) y UDP (v3); v4 solo TCP |
| 5 | Capas | OSI 7 (Aplicación/archivos); plano: datos |
| 6 | Transporte y direccionamiento | TCP 2049; RPC (RFC 5531) como transporte de llamadas |
| 7 | PDU | **RPC NFS** (OP números; encabezados RPC + procedimiento) |
| 8 | Mensajes | Operaciones: OPEN, READ, WRITE, GETATTR, LOOKUP, COMMIT… |
| 9 | Campos | Cabecera RPC (versión, procedimiento, auth) + payload con atributos. (Detalle en F5) |
| 10 | Secuencia | Mount → OPEN/LOOKUP → READ/WRITE → CLOSE; leases y delegaciones (v4) |
| 11 | Addressing/naming | Export paths; file handles (FH) |
| 12 | Routing/forwarding | No (los archivos viajan por la red) |
| 13 | Seguridad | Auth simple (AUTH_SYS) débil; v4 admite RPCSEC_GSS (Kerberos); ver F6 |
| 14 | QoS/rendimiento | Caché de cliente, delegaciones, write coalescing |
| 15 | Observabilidad | Operaciones y filehandles visibles; filtros nfs.* |
| 16 | Interoperabilidad | Amplia en entornos UNIX/Linux; interop con pNFS |
| 17 | Implementaciones | Linux NFS (nfsd/kernel), FreeBSD, Solaris; clientes de SO |
| 18 | Fuentes | RFC 7530/7862 (nivel 1); IANA R1 — 26-08-2026 |

## F-36 · SMB — Server Message Block

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | SMB (SMB2/3); MS-SMB2 (Open Specifications); Microsoft; familia APP |
| 2 | Estado | Vigente (26-08-2026; protocolo de especificación pública de Microsoft) |
| 3 | Finalidad | Compartición de archivos, impresoras y IPC en redes Windows (y Samba). **No usar** fuera del ecosistema sin Samba equivalente |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **445** registrado; NetBIOS 139 legacy) |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | TCP 445; árboles y rutas UNC (\\server\share) |
| 7 | PDU | **Mensaje SMB2** (cabecera fija 64 B + comandos) |
| 8 | Mensajes | NEGOTIATE, SESSION_SETUP, TREE_CONNECT, CREATE, READ, WRITE, CLOSE… |
| 9 | Campos | Cabecera SMB2: protocol id (0xFE 'SMB'), command, status, flags, message id, session id, tree id. (Detalle en F5) |
| 10 | Secuencia | Negociación → sesión (auth) → tree connect → create/read/write → close |
| 11 | Addressing/naming | UNC paths; SIDs y credenciales |
| 12 | Routing/forwarding | No (aplicación) |
| 13 | Seguridad | Autenticación NTLM/Kerberos, cifrado SMB3 (AES-CCM/GCM) y signing; ver F6 |
| 14 | QoS/rendimiento | Dialects, oplocks/leases, multichannel (SMB3) |
| 15 | Observabilidad | Comandos y árboles visibles; filtros smb2.* |
| 16 | Interoperabilidad | Windows/Samba; dialectos negociados |
| 17 | Implementaciones | Windows Server, Samba, ksmbd |
| 18 | Fuentes | MS-SMB2 (Open Specifications) — 26-08-2026 |

## F-37 · iSCSI — Internet Small Computer System Interface

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | iSCSI; RFC 7143; IETF; familia STOR |
| 2 | Estado | Vigente (26-08-2026; RFC 7143) |
| 3 | Finalidad | Transporte de comandos SCSI sobre red IP (SAN por Ethernet). **No usar** cuando se exige latencia ultrabaja dedicada (FC) |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **3260** registrado en IANA) |
| 5 | Capas | OSI 7 (Aplicación/block); plano: datos |
| 6 | Transporte y direccionamiento | TCP 3260; IQN/EUI targets |
| 7 | PDU | **PDU iSCSI** (cabecera fija 48 B + payload) |
| 8 | Mensajes | Operaciones: SCSI CDB (READ/WRITE), login, NOP, task management |
| 9 | Campos | Cabecera: opcode(8)+flags(8)+totalAHSLength+dataSegLen+...ISID, CID, CmdSN. (Detalle en F5) |
| 10 | Secuencia | Login (CHAP) → sesión → comandos SCSI → logout |
| 11 | Addressing/naming | IQN (iqn.2026-…:target); portals |
| 12 | Routing/forwarding | No (SAN por red TCP) |
| 13 | Seguridad | CHAP en login, IPsec recomendado para tráfico; ver F6 |
| 14 | QoS/rendimiento | Interdependencia con TCP (HoL); header/data digest opcional |
| 15 | Observabilidad | Opcodes y CmdSN visibles; filtros iscsi.* |
| 16 | Interoperabilidad | Amplia en storage; multipath con MPIO |
| 17 | Implementaciones | Linux kernel (open-iscsi), Microsoft iSCSI initiator, storage arrays |
| 18 | Fuentes | RFC 7143 (nivel 1); IANA R1 — 26-08-2026 |

## F-38 · MPLS — Multiprotocol Label Switching

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | MPLS; RFC 3031 (arquitectura), RFC 3032 (codificación); IETF; familia ROUT |
| 2 | Estado | Vigente (26-08-2026; RFC 3031/3032; evolución SR-MPLS RFC 8660) |
| 3 | Finalidad | Reenvío por etiquetas (label switching) en núcleos de red para velocidad y ingeniería de tráfico. **No usar** en acceso simple (IP basta) |
| 4 | Encapsulación | Corre **entre capas 2 y 3**: sobre enlace (Ethernet, PPP) y debajo de IP; EtherType 0x8847 |
| 5 | Capas | OSI 2.5 (Red); plano: datos + control (LDP/RSVP-TE) |
| 6 | Transporte y direccionamiento | Sin puertos; label stack (shim 32 bits) |
| 7 | PDU | **Paquete MPLS** (labels apilados + payload) |
| 8 | Mensajes | Datos etiquetados; FEC mapeada por LDP/BGP/RSVP |
| 9 | Campos | Shim 32 bits: Label(20)+TC(3)+S(1)+TTL(8). (Detalle en F5) |
| 10 | Secuencia | LSP establecido (LDP/RSVP-TE) → reenvío por etiqueta hop-by-hop; pop/push/swap |
| 11 | Addressing/naming | Labels locales por LSR; FEC |
| 12 | Routing/forwarding | LFIB por etiqueta; tráfico-tecnic en MPLS-TE |
| 13 | Seguridad | Sin cifrado; puede transportar casi cualquier protocolo (incluye IPsec opcional); ver F6 |
| 14 | QoS/rendimiento | TC para QoS; penalización por encabezado (MTU) |
| 15 | Observabilidad | Labels y TTL visibles; filtros mpls.* |
| 16 | Interoperabilidad | Núcleos de proveedores y grandes redes |
| 17 | Implementaciones | Cisco, Juniper, Nokia, FRRouting (LDP) |
| 18 | Fuentes | RFC 3031/3032 (nivel 1) — 26-08-2026 |

## F-39 · GRE — Generic Routing Encapsulation

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | GRE; RFC 2784 (GRE), RFC 7676 (IPv6 GRE); IETF; familia SEG |
| 2 | Estado | Vigente (26-08-2026; RFC 2784) |
| 3 | Finalidad | Encapsulación de una red/paquete sobre otra (túneles punto a punto). **No usar** cuando se requiere cifrado (combinar con IPsec) |
| 4 | Encapsulación | Corre sobre **IPv4/IPv6** (IP protocol number **47**); encapsula IP o cualquier protocolo |
| 5 | Capas | OSI 2.5/3 (tunneling); plano: datos |
| 6 | Transporte y direccionamiento | IP protocol 47; sin puertos |
| 7 | PDU | **Paquete GRE** (cabecera fija 4 B + opciones + payload) |
| 8 | Mensajes | Sin mensajes de control (túnel stateless); multipoint GRE (mGRE) con NHRP |
| 9 | Campos | C(1)+R(1)+K(1)+S(1)+reserved(4)+version(3)+protocol type(16)+(key/seq opcionales). (Detalle en F5) |
| 10 | Secuencia | Túnel configurado → encapsulación/desencapsulación por paquete |
| 11 | Addressing/naming | Endpoints del túnel (IP origen/destino) |
| 12 | Routing/forwarding | Rutas a través del túnel; keepalive opcional |
| 13 | Seguridad | Sin cifrado (GRE puro); GRE-over-IPsec para seguridad; ver F6 |
| 14 | QoS/rendimiento | Overhead de 4-8 B; fragmentación a considerar |
| 15 | Observabilidad | Cabecera y payload visibles; filtros gre.* |
| 16 | Interoperabilidad | Amplia en túneles (VPN, DMVPN, 6in4…) |
| 17 | Implementaciones | Kernel Linux (ip tunnel), routers |
| 18 | Fuentes | RFC 2784/7676 (nivel 1) — 26-08-2026 |

## F-40 · VXLAN — Virtual Extensible LAN

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | VXLAN; RFC 7348 (y actualizaciones); IETF; familia SEG |
| 2 | Estado | Vigente (26-08-2026; RFC 7348) |
| 3 | Finalidad | Overlay L2 sobre red L3 (data center) para segmentación y movilidad de VM. **No usar** para WAN (alternativas EVPN/VXLAN con control plano) |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **4789** registrado en IANA) sobre IP |
| 5 | Capas | OSI 2.5/4 (overlay); plano: datos |
| 6 | Transporte y direccionamiento | UDP 4789; VNI de 24 bits |
| 7 | PDU | **Trama VXLAN** (cabecera 8 B + trama Ethernet interna) |
| 8 | Mensajes | Datos L2 encapsulados; (solo-multicast o EVPN para BUM) |
| 9 | Campos | Flags(8: I flag=1)+reserved(24)+VNI(24)+reserved(8). (Detalle en F5) |
| 10 | Secuencia | Underlay IP → UDP/VXLAN → trama Ethernet interna |
| 11 | Addressing/naming | VNI; MAC de la trama interior |
| 12 | Routing/forwarding | VTEPs: encap/decap en límites; BUM por multicast o EVPN |
| 13 | Seguridad | Sin cifrado (IPsec/ESP opcional); segmentación por VNI; ver F6 |
| 14 | QoS/rendimiento | Overhead 50 B; jumbo frames necesarios |
| 15 | Observabilidad | VNI y VTEP visibles; filtros vxlan.* |
| 16 | Interoperabilidad | Amplia en nubes/data centers; EVPN para control |
| 17 | Implementaciones | Linux (VXLAN), VMware NSX, Cisco/Juniper/Arista |
| 18 | Fuentes | RFC 7348 (nivel 1); IANA R1 — 26-08-2026 |

## F-41 · WireGuard

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | WireGuard; protocolo del proyecto WireGuard (Jason Donenfeld); documentación oficial del proyecto; familia SEG |
| 2 | Estado | Vigente (26-08-2026; implementación de referencia y adopción amplia) |
| 3 | Finalidad | VPN moderna, minimalista y rápida con criptografía moderna (Curve25519, ChaCha20-Poly1305, BLAKE2s). **No usar** si la organización requiere IKEv2/IPsec estándar |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **51820** habitual, registrado) |
| 5 | Capas | OSI 3 (túnel VPN sobre UDP); plano: seguridad/datos |
| 6 | Transporte y direccionamiento | UDP 51820; claves públicas de par |
| 7 | PDU | **Paquete WireGuard** (mensajes handshake y data AEAD) |
| 8 | Mensajes | Handshake init/response/cookie; mensajes de datos cifrados |
| 9 | Campos | Tipos de mensaje, índice de receptor, nonce, cifrado. (Detalle en F5) |
| 10 | Secuencia | Handshake Noise IK → sesión → datos cifrados; rekey periódico |
| 11 | Addressing/naming | Claves públicas (25519); IPs de la red interna del túnel |
| 12 | Routing/forwarding | Tabla de rutas del túnel; AllowedIPs |
| 13 | Seguridad | E2E cifrado AEAD, PFS, sin identidades dinámicas; ver F6 |
| 14 | QoS/rendimiento | Muy rápido (kernel); menor overhead |
| 15 | Observabilidad | Manos visibles (handshake espaciado); payload cifrado |
| 16 | Interoperabilidad | Adopción en Linux, Android, iOS, routers; interoperable entre implementaciones del protocolo |
| 17 | Implementaciones | wireguard-go, kernel Linux, wg-quick, clientes móviles |
| 18 | Fuentes | Documentación oficial del proyecto WireGuard (RFC 8883* pendiente) — 26-08-2026 |

## F-42 · GTP — GPRS Tunnelling Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | GTP (GTP-C/GTP-U); 3GPP TS 29.060/29.281; 3GPP; familia MOV |
| 2 | Estado | Vigente (26-08-2026; 3GPP Release) |
| 3 | Finalidad | Tunelización de datos de usuario (GTP-U) y señalización (GTP-C) en redes móviles (4G/5G core). **No usar** fuera del ámbito 3GPP sin justificación |
| 4 | Encapsulación | Corre sobre **UDP** (puertos **2123** GTP-C y **2152** GTP-U, registrados en IANA) |
| 5 | Capas | OSI 2.5/4 (tunneling móvil); plano: datos + control |
| 6 | Transporte y direccionamiento | UDP 2123/2152; TEID para identificación de túnel |
| 7 | PDU | **PDU GTP** (cabecera + mensaje; GTP-U con payload IP) |
| 8 | Mensajes | GTP-C: Create Session, Modify, Delete, Echo; GTP-U: G-PDU, Echo |
| 9 | Campos | Flags(8: PT, version)+MT(8)+Length(16)+TEID(32)+sequence. (Detalle en F5) |
| 10 | Secuencia | Sesión PDP/PDN establecida (Create Session) → datos GTP-U → Delete |
| 11 | Addressing/naming | TEIDs; APN; IMSI |
| 12 | Routing/forwarding | Túneles entre SGW/PGW/UPF; no es routing IP |
| 13 | Seguridad | Sin cifrado nativo (core network trusted); protección en transporte según operador; ver F6 |
| 14 | QoS/rendimiento | QoS por bearer (QCI); overhead de cabecera |
| 15 | Observabilidad | TEID y tipo de mensaje visibles; filtros gtp.* |
| 16 | Interoperabilidad | Estándar 3GPP en móviles |
| 17 | Implementaciones | Núcleos móviles (Open5GS, Magma, Cisco, Ericsson) |
| 18 | Fuentes | 3GPP TS 29.281 (nivel 1); IANA R1 — 26-08-2026 |

## F-43 · Modbus

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Modbus (RTU/TCP); Modbus Organization (IDA); familia IOT |
| 2 | Estado | Vigente (26-08-2026; Modbus TCP especificación pública) |
| 3 | Finalidad | Comunicación industrial maestro-esclavo (PLC/HMI/SCADA): lectura/escritura de registros. **No usar** en redes críticas sin segmentación |
| 4 | Encapsulación | Modbus TCP corre sobre **TCP** (puerto **502** registrado en IANA); Modbus RTU sobre serie |
| 5 | Capas | OSI 7 (Aplicación industrial); plano: datos |
| 6 | Transporte y direccionamiento | TCP 502; unit IDs y direcciones de registros |
| 7 | PDU | **ADU Modbus TCP** (MBAP 7 B + PDU de función) |
| 8 | Mensajes | Funciones: 0x01/0x02 read coils/discretes, 0x03/0x04 regs, 0x05/0x06 write, 0x10 write multiple |
| 9 | Campos | MBAP: transaction id(16)+protocol id(16=0)+length(16)+unit id(8); function code+data. (Detalle en F5) |
| 10 | Secuencia | TCP → request (function) → response (o exception) |
| 11 | Addressing/naming | Unit ID + dirección de registro |
| 12 | Routing/forwarding | No (aplicación industrial) |
| 13 | Seguridad | Sin autenticación ni cifrado nativos (históricamente); segmentar y usar gateways; ver F6 |
| 14 | QoS/rendimiento | Simple y determinista; polling típico |
| 15 | Observabilidad | Function codes y direcciones visibles; filtros modbus.* |
| 16 | Interoperabilidad | Estándar industrial de facto |
| 17 | Implementaciones | Librerías libmodbus, nodemodbus, PLCs y HMIs |
| 18 | Fuentes | Modbus TCP especificación (nivel 1); IANA R1 (502) — 26-08-2026 |

## F-44 · DNP3 — Distributed Network Protocol 3

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | DNP3; IEEE 1815; IEEE/fracaso comercial (DNP Users Group); familia IOT |
| 2 | Estado | Vigente (26-08-2026; IEEE 1815) |
| 3 | Finalidad | SCADA/OT: telemetría y control entre RTU/PLC y centros de control con fiabilidad. **No usar** en redes corporativas IT |
| 4 | Encapsulación | Corre sobre **TCP** (puertos **20000** por convenio registrado) y serie (DNP3 seriado) |
| 5 | Capas | OSI 7 (Aplicación SCADA); plano: datos |
| 6 | Transporte y direccionamiento | TCP 20000; direcciones de enlace (2 B) y de capa de aplicación |
| 7 | PDU | **Trama DNP3** (12-16 B cabecera + LPDU) |
| 8 | Mensajes | Requests/responses: READ, WRITE, UNSOLICITED responses, time sync, integrity poll |
| 9 | Campos | Start(2)=0x0564+Len(1)+control(1)+dest(2)+src(2)+CRC(2)… (Detalle en F5) |
| 10 | Secuencia | Link layer con confirmaciones/reintentos; application layer fragmentación |
| 11 | Addressing/naming | Direcciones de enlace maestro/outstation |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | Autenticación DNP3 Secure Authentication (IEEE 1815-2012); cifrado no nativo; ver F6 |
| 14 | QoS/rendimiento | Confirmable/retries; timeouts SCADA |
| 15 | Observabilidad | Funciones y puntos visibles; filtros dnp3.* |
| 16 | Interoperabilidad | Estándar en utilities eléctricas |
| 17 | Implementaciones | OpenDNP3, libiec61850? (NO: OpenDNP3), RTUs |
| 18 | Fuentes | IEEE 1815 (nivel 1); IANA R1 — 26-08-2026 |

## F-45 · OPC UA — OPC Unified Architecture

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | OPC UA; IEC 62541; OPC Foundation; familia IOT |
| 2 | Estado | Vigente (26-08-2026; IEC 62541) |
| 3 | Finalidad | Interoperabilidad industrial (acceso a datos, métodos, alarmas) entre máquinas y sistemas con modelo de información. **No usar** si solo se necesita MQTT ligero (casos simples) |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **4840** registrado) y opcionalmente OPC UA PubSub (UDP/MQTT) |
| 5 | Capas | OSI 7 (Aplicación industrial); plano: datos |
| 6 | Transporte y direccionamiento | TCP 4840; endpoint URLs y SecurityPolicy |
| 7 | PDU | **Mensaje OPC UA** (Secure Conversation, chunks binarios) |
| 8 | Mensajes | Hello, OpenSecureChannel, CreateSession, Read, Write, Browse, Subscribe… |
| 9 | Campos | Chunks message header + security header + sequence header + payload (binario/UADP). (Detalle en F5) |
| 10 | Secuencia | TCP → Hello → OpenSecureChannel → CreateSession → ActivateSession → Read/Subscribe |
| 11 | Addressing/naming | NodeIds y BrowseNames del modelo de información |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | Seguridad por capas: firma y cifrado (SecurityPolicy), aplicaciones con certificados; ver F6 |
| 14 | QoS/rendimiento | Publishing intervals, sampling; perfiles de transporte |
| 15 | Observabilidad | Mensajes y NodeIds visibles (binario); filtros opcua.* |
| 16 | Interoperabilidad | Alta en industria (era de perfiles de compatibilidad) |
| 17 | Implementaciones | open62541, .NET OPC UA, UA-.NETStandard |
| 18 | Fuentes | IEC 62541 (nivel 1); IANA R1 — 26-08-2026 |

## F-46 · WIFI — Wi-Fi (IEEE 802.11)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Wi-Fi; familia IEEE 802.11 (a/b/g/n/ac/ax/be); IEEE; familia ACEL |
| 2 | Estado | Vigente (26-08-2026; 802.11 en evolución continua) |
| 3 | Finalidad | Acceso inalámbrico LAN por radio (CSMA/CA, OFDM) en bandas 2,4/5/6 GHz. **No usar** en entornos de latencia crítica sin planificación de RF |
| 4 | Encapsulación | Corre sobre el medio radio (ondas); encapsula tramas Ethernet-like (802.3) |
| 5 | Capas | OSI 1-2 (Física + Enlace/MAC); plano: datos |
| 6 | Transporte y direccionamiento | Sin puertos; direcciones MAC; SSID/BSSID; canales |
| 7 | PDU | **Trama 802.11** (MAC header + payload + FCS) |
| 8 | Mensajes | Data, Control (RTS/CTS/ACK), Management (Beacon, Probe, Auth, Assoc) |
| 9 | Campos | Frame control(16)+duration(16)+addresses(3×48)+seq(16)+payload+FCS. (Detalle en F5) |
| 10 | Secuencia | Scanning → Authentication → Association → datos (CSMA/CA); roaming |
| 11 | Addressing/naming | MAC 48 bits; SSID |
| 12 | Routing/forwarding | APs y mesh (802.11s); no participa en routing IP |
| 13 | Seguridad | WPA2/WPA3 (CCMP/GCMP, SAE); WEP obsoleto; ver F6 |
| 14 | QoS/rendimiento | EDCA (WMM), agregación de tramas; interferencia RF |
| 15 | Observabilidad | Beacons y tramas visibles; filtros wlan.* |
| 16 | Interoperabilidad | Amplia entre APs y clientes certificados Wi-Fi Alliance |
| 17 | Implementaciones | APs (Cisco, Aruba, Ubiquiti), clientes de SO, drivers |
| 18 | Fuentes | IEEE 802.11 (nivel 1); Wi-Fi Alliance — 26-08-2026 |

## F-47 · PPP — Point-to-Point Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | PPP; RFC 1661 (+ RFC 1662 HDLC-like); IETF; familia ACEL |
| 2 | Estado | Vigente (26-08-2026; RFC 1661; uso en PPPoE/telefonía) |
| 3 | Finalidad | Transporte multi-protocolo sobre enlaces punto a punto (serie, DSL, tunneling). **No usar** en LAN (Ethernet) |
| 4 | Encapsulación | Corre sobre enlace físico (serie/ADSL); encapsula IP, MPLS y otros |
| 5 | Capas | OSI 2 (Enlace); plano: datos |
| 6 | Transporte y direccionamiento | Sin puertos; negociación de protocolo (protocol field) |
| 7 | PDU | **Trama PPP** (flag + address + control + protocol + payload + FCS) |
| 8 | Mensajes | LCP (configuración), NCP (IPCP), PAP/CHAP/EAP auth; datagramas |
| 9 | Campos | Protocol(16: 0x0021 IPv4, 0x0057 IPv6, 0x8021 IPCP)+payload+FCS(16/32). (Detalle en F5) |
| 10 | Secuencia | LCP Configure → Autenticación → NCP (IPCP) → datos; Echo requests |
| 11 | Addressing/naming | Por LCP (addresses opcionales); IP por IPCP |
| 12 | Routing/forwarding | Enlace punto a punto |
| 13 | Seguridad | PAP en claro / CHAP / EAP; MPPE para cifrado en PPPoE; ver F6 |
| 14 | QoS/rendimiento | MTU negociable; compresión (CCP) |
| 15 | Observabilidad | Protocolos y LCP visibles; filtros ppp.* |
| 16 | Interoperabilidad | Amplia en acceso conmutado/DSL |
| 17 | Implementaciones | Kernel Linux (pppd), Windows RAS, routers |
| 18 | Fuentes | RFC 1661 (nivel 1) — 26-08-2026 |

## F-48 · 802.1Q — VLAN (IEEE 802.1Q)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | VLAN tagging; IEEE 802.1Q; IEEE; familia ACEL |
| 2 | Estado | Vigente (26-08-2026; 802.1Q en revisión con 802.1Q-2018) |
| 3 | Finalidad | Segmentación L2 en switches mediante etiquetas VLAN (4094 IDs). **No usar** como control de seguridad (es segmentación lógica) |
| 4 | Encapsulación | Corre sobre **Ethernet** (EtherType **0x8100**); se inserta entre SA y EtherType/Length |
| 5 | Capas | OSI 2 (Enlace); plano: datos |
| 6 | Transporte y direccionamiento | Sin puertos; tag 32 bits (TPID+PCP+DEI+VID) |
| 7 | PDU | **Trama 802.1Q** (trama Ethernet + tag) |
| 8 | Mensajes | Tramas etiquetadas; GVRP/MVRP para registro (legacy) |
| 9 | Campos | TPID(16: 0x8100)+PCP(3)+DEI(1)+VID(12). (Detalle en F5) |
| 10 | Secuencia | Inserción/extracción del tag en el switch; forwarding por VLAN |
| 11 | Addressing/naming | VID; MAC sin cambio |
| 12 | Routing/forwarding | Switches L2 por VLAN; enrutamiento inter-VLAN en L3 |
| 13 | Seguridad | No cifra; mitigación de double-tagging; VLAN hopping mitigado; ver F6 |
| 14 | QoS/rendimiento | PCP para prioridad (8 clases) |
| 15 | Observabilidad | TPID y VID visibles; filtros vlan.* |
| 16 | Interoperabilidad | Universal en switching |
| 17 | Implementaciones | Switches gestionados, NICs (VLAN interfaces) |
| 18 | Fuentes | IEEE 802.1Q (nivel 1) — 26-08-2026 |

## F-49 · STP — Spanning Tree Protocol (IEEE 802.1D)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | STP; IEEE 802.1D; IEEE; familia ACEL |
| 2 | Estado | Vigente en legados; sustituido por RSTP (26-08-2026; 802.1D-2004) |
| 3 | Finalidad | Evitar bucles en topologías L2 redundantes (árbol de spanning). **No usar** en diseño nuevo (usar RSTP/MSTP) |
| 4 | Encapsulación | Corre sobre **Ethernet** (multicast 01:80:c2:00:00:00) |
| 5 | Capas | OSI 2 (Enlace, control); plano: control |
| 6 | Transporte y direccionamiento | Sin puertos; BPDUs con bridge/port ID |
| 7 | PDU | **BPDU** (Configuration TCN BPDU) |
| 8 | Mensajes | BPDUs de configuración y Topology Change Notification |
| 9 | Campos | Protocol ID(16)+version(8)+type(8)+flags(8)+root/root path cost/bridge id+port id+message age. (Detalle en F5) |
| 10 | Secuencia | Root election → cálculo del árbol → bloqueo de puertos redundantes; TCN en cambios |
| 11 | Addressing/naming | Bridge ID (prioridad+MAC) |
| 12 | Routing/forwarding | Path cost y estados (Blocking/Listening/Learning/Forwarding) |
| 13 | Seguridad | No autentica BPDUs (BPDU guard/root guard); ver F6 |
| 14 | QoS/rendimiento | Convergencia lenta (30-50 s) |
| 15 | Observabilidad | BPDUs visibles; filtros stp.* |
| 16 | Interoperabilidad | Amplia; coexistencia con RSTP/MSTP |
| 17 | Implementaciones | Switches gestionados |
| 18 | Fuentes | IEEE 802.1D (nivel 1) — 26-08-2026 |

## F-50 · RSTP — Rapid Spanning Tree (IEEE 802.1w)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | RSTP; IEEE 802.1w (integrado en 802.1D-2004); IEEE; familia ACEL |
| 2 | Estado | Vigente (26-08-2026) |
| 3 | Finalidad | Convergencia rápida (≈1 s) en topologías L2 redundantes. **No usar** cuando se requiere múltiples árboles (MSTP) |
| 4 | Encapsulación | Corre sobre **Ethernet** (BPDUs RSTP) |
| 5 | Capas | OSI 2 (Enlace, control); plano: control |
| 6 | Transporte y direccionamiento | Sin puertos; BPDUs |
| 7 | PDU | **BPDU RSTP** (tipo 2, version 2) |
| 8 | Mensajes | Proposals/agreements, edge ports, alternates/backups |
| 9 | Campos | Como STP + flags de proposal/agreement(8) y role. (Detalle en F5) |
| 10 | Secuencia | Proposal/Agreement handshake; roles root/designated/alternate/backup |
| 11 | Addressing/naming | Bridge/port IDs |
| 12 | Routing/forwarding | Estados: Discarding/Learning/Forwarding |
| 13 | Seguridad | Igual que STP (guardas de puerto); ver F6 |
| 14 | QoS/rendimiento | Convergencia ~1 s |
| 15 | Observabilidad | BPDUs RSTP visibles; filtros rstp.* |
| 16 | Interoperabilidad | Retrocompatibilidad con STP |
| 17 | Implementaciones | Switches gestionados |
| 18 | Fuentes | IEEE 802.1w (nivel 1) — 26-08-2026 |

## F-51 · MSTP — Multiple Spanning Tree (IEEE 802.1s)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | MSTP; IEEE 802.1s (integrado 802.1Q-2005); IEEE; familia ACEL |
| 2 | Estado | Vigente (26-08-2026) |
| 3 | Finalidad | Múltiples instancias de spanning tree sobre regiones VLAN (utilización de enlaces redundantes). **No usar** en topologías simples (RSTP basta) |
| 4 | Encapsulación | Corre sobre **Ethernet** (BPDUs MST) |
| 5 | Capas | OSI 2 (Enlace, control); plano: control |
| 6 | Transporte y direccionamiento | Sin puertos; MSTI + region digest |
| 7 | PDU | **BPDU MST** (extiende RSTP con instancias) |
| 8 | Mensajes | MST BPDUs con config digest y MSTI records |
| 9 | Campos | BPDU MST: MST extension + MSTI flags. (Detalle en F5) |
| 10 | Secuencia | CIST (regional root) + MSTIs por grupo de VLANs |
| 11 | Addressing/naming | Region, MSTI IDs |
| 12 | Routing/forwarding | Instancias independientes |
| 13 | Seguridad | Igual que STP/RSTP; injerencia de regiones; ver F6 |
| 14 | QoS/rendimiento | Convergencia por instancia |
| 15 | Observabilidad | BPDUs MST visibles; filtros mstp.* |
| 16 | Interoperabilidad | Compatible con RSTP/STP en CIST |
| 17 | Implementaciones | Switches enterprise |
| 18 | Fuentes | IEEE 802.1s (nivel 1) — 26-08-2026 |

## F-52 · LACP — Link Aggregation (IEEE 802.1AX)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | LACP; IEEE 802.1AX-2008 (antes 802.3ad); IEEE; familia ACEL |
| 2 | Estado | Vigente (26-08-2026) |
| 3 | Finalidad | Agregación de enlaces físicos en un lógico (bonding) con negociación dinámica. **No usar** sin soporte en ambos extremos |
| 4 | Encapsulación | Corre sobre **Ethernet** (EtherType **0x8809**; Slow Protocols) |
| 5 | Capas | OSI 2 (Enlace, control); plano: control/datos |
| 6 | Transporte y direccionamiento | Sin puertos; LACPDUs; actor/partner key |
| 7 | PDU | **LACPDU** (68 B) |
| 8 | Mensajes | LACPDUs periódicos; Marker PDUs |
| 9 | Campos | Subtype(1)+version(1)+TLV actor(20)+TLV partner(20)+collector+pads. (Detalle en F5) |
| 10 | Secuencia | Negociación LACP → agregación; selección por key/link |
| 11 | Addressing/naming | System ID; port keys |
| 12 | Routing/forwarding | Distribución por hashing (MAC/IP) |
| 13 | Seguridad | No autentica LACPDUs (posible spoofing); ver F6 |
| 14 | QoS/rendimiento | Throughput agregado + redundancia |
| 15 | Observabilidad | LACPDUs y estados visibles; filtros lacp.* |
| 16 | Interoperabilidad | Amplia en NICs/switches |
| 17 | Implementaciones | NICs (bonding), switches, hypervisores |
| 18 | Fuentes | IEEE 802.1AX (nivel 1) — 26-08-2026 |

## F-53 · NDP — Neighbor Discovery Protocol (IPv6)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | NDP; RFC 4861 (y RFC 4862 SLAAC); IETF; familia ADCONF |
| 2 | Estado | Vigente (26-08-2026; RFC 4861) |
| 3 | Finalidad | Descubrimiento de vecinos en IPv6: resolución dirección/MAC, detección de routers y configuración (SLAAC). **No usar** en IPv4 (usar ARP) |
| 4 | Encapsulación | Corre sobre **IPv6** (mensajes ICMPv6 tipo 133-137) |
| 5 | Capas | OSI 3 (Red); plano: datos/control |
| 6 | Transporte y direccionamiento | ICMPv6; multicast ff02::1/ff02::2; link-local |
| 7 | PDU | **Mensaje ICMPv6 NDP** (RS/RA/NS/NA/Redirect) |
| 8 | Mensajes | Router Solicitation/Advertisement, Neighbor Solicitation/Advertisement, Redirect |
| 9 | Campos | ICMPv6 header + options (source/target LL addr, MTU, prefix info, RDNSS). (Detalle en F5) |
| 10 | Secuencia | RS→RA (autoconfiguración); NS→NA (resolución); DAD |
| 11 | Addressing/naming | Link-local fe80::/10; EUI-64 o aleatorias (RFC 7217) |
| 12 | Routing/forwarding | Default router por RA; sección enrutable |
| 13 | Seguridad | NDP vulnerable (spoofing); SEND (RFC 3971) y RA guard; ver F6 |
| 14 | QoS/rendimiento | Caché ND, timers (reachable 30 s) |
| 15 | Observabilidad | Mensajes y opciones visibles; filtros icmpv6/nd.* |
| 16 | Interoperabilidad | Esencial en IPv6 |
| 17 | Implementaciones | Pilas IPv6 de SO y routers |
| 18 | Fuentes | RFC 4861 (nivel 1) — 26-08-2026 |

## F-54 · mDNS — Multicast DNS

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | mDNS; RFC 6762 (y RFC 6763 DNS-SD); IETF; familia ADCONF |
| 2 | Estado | Vigente (26-08-2026; RFC 6762) |
| 3 | Finalidad | Resolución de nombres .local sin servidor central (Zeroconf) en LAN. **No usar** en redes grandes/enrutadas |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **5353** registrado) con dirección multicast 224.0.0.251/ff02::fb |
| 5 | Capas | OSI 7 (Aplicación/nombres); plano: datos |
| 6 | Transporte y direccionamiento | UDP 5353; nombres .local |
| 7 | PDU | **Mensaje DNS mDNS** (formato DNS sobre multicast) |
| 8 | Mensajes | Queries (con suppression), responses, announcements (probes) |
| 9 | Campos | Igual que DNS: header+questions+answers (con cache-flush 0x8000). (Detalle en F5) |
| 10 | Secuencia | Probe (detección de conflictos) → Announce → respuesta; cache con TTL |
| 11 | Addressing/naming | .local; SRV/TXT (DNS-SD) |
| 12 | Routing/forwarding | Local al enlace (TTL limitado); no enroutable |
| 13 | Seguridad | Sin autenticación (spo0ofing en LAN); ver F6 |
| 14 | QoS/rendimiento | Known Answer suppression; responder rápidamente |
| 15 | Observabilidad | Queries/respuestas visibles; filtros mdns.* |
| 16 | Interoperabilidad | Zeroconf (Bonjour) amplio |
| 17 | Implementaciones | Avahi, Bonjour, Windows (LLMNR+), responde a .local |
| 18 | Fuentes | RFC 6762 (nivel 1); IANA R1 — 26-08-2026 |

## F-55 · IGMP — Internet Group Management Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | IGMP; RFC 3376 (v3); IETF; familia ROUT |
| 2 | Estado | Vigente (26-08-2026; RFC 3376) |
| 3 | Finalidad | Gestión de grupos multicast IPv4 en redes de acceso (host→router). **No usar** en IPv6 (usar MLD) |
| 4 | Encapsulación | Corre sobre **IPv4** (IP protocol number **2**) |
| 5 | Capas | OSI 3 (Red, control); plano: control |
| 6 | Transporte y direccionamiento | IP protocol 2; grupos 224.0.0.0/4 |
| 7 | PDU | **Mensaje IGMP** |
| 8 | Mensajes | Membership Query, Membership Report (v1/v2/v3), Leave |
| 9 | Campos | Type(8)+Max Resp Time(8)+Checksum(16)+Group(32) (+records en v3). (Detalle en F5) |
| 10 | Secuencia | Report del host → router arma el grupo; queries periódicas; leave v2 |
| 11 | Addressing/naming | Direcciones de grupo multicast |
| 12 | Routing/forwarding | Enrutamiento de miembros por grupo |
| 13 | Seguridad | Sin autenticación (injección de membership); ver F6 |
| 14 | QoS/rendimiento | Query interval, fast leave, IGMP snooping |
| 15 | Observabilidad | Types y grupos visibles; filtros igmp.* |
| 16 | Interoperabilidad | v1/v2/v3 retrocompatibles |
| 17 | Implementaciones | Pilas de SO, switches con snooping |
| 18 | Fuentes | RFC 3376 (nivel 1); IANA R1 — 26-08-2026 |

## F-56 · PIM — Protocol Independent Multicast

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | PIM; RFC 7761 (PIM-SM); IETF; familia ROUT |
| 2 | Estado | Vigente (26-08-2026; RFC 7761) |
| 3 | Finalidad | Enrutamiento multicast independiente del protocolo unicast (SM/DM) con árboles compartidos y de fuente. **No usar** sin necesidad de multicast |
| 4 | Encapsulación | Corre sobre **IPv4/IPv6** (protocol **103**; IPv6 RFC 8736) |
| 5 | Capas | OSI 3 (Red, control); plano: control |
| 6 | Transporte y direccionamiento | IP protocol 103; RP (rendezvous point) |
| 7 | PDU | **Mensaje PIM** (cabecera + tipos) |
| 8 | Mensajes | Hello, Join/Prune, Assert, Register/Register-Stop, BSR |
| 9 | Campos | Ver/Type(8)+reserved(8)+checksum(16)+encapsulated. (Detalle en F5) |
| 10 | Secuencia | Hello → RP tree (shared) → switch al source tree; Joins/Prunes |
| 11 | Addressing/naming | Grupos multicast + fuentes; RP |
| 12 | Routing/forwarding | (S,G) / (*,G) estados; RPF check |
| 13 | Seguridad | Sin autenticación (injetos); protección con IPsec/ACLs; ver F6 |
| 14 | QoS/rendimiento | Árboles óptimos; hello timers |
| 15 | Observabilidad | Mensajes y grupos visibles; filtros pim.* |
| 16 | Interoperabilidad | Principal protocolo multicast de la industria |
| 17 | Implementaciones | Routers (Cisco, Juniper), FRRouting |
| 18 | Fuentes | RFC 7761 (nivel 1); IANA R1 — 26-08-2026 |

## F-57 · VRRP — Virtual Router Redundancy Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | VRRP; RFC 5798 (v3); IETF; familia ROUT |
| 2 | Estado | Vigente (26-08-2026; RFC 5798; alternativas no propietarias) |
| 3 | Finalidad | Redundancia de gateway (VIP compartida entre routers). **No usar** cuando se requiere equilibrio de carga simultáneo (usar ECMP) |
| 4 | Encapsulación | Corre sobre **IPv4/IPv6** (protocol **112** v2; v3 sobre IPv6) |
| 5 | Capas | OSI 3 (Red, control); plano: control |
| 6 | Transporte y direccionamiento | IP protocol 112; grupo multicast 224.0.0.18 (v2) / ff02::12 (v3) |
| 7 | PDU | **Mensaje VRRP** (advertisement) |
| 8 | Mensajes | VRRP Advertisement |
| 9 | Campos | Versión/Type(8)+VRID(8)+Priority(8)+count IPs(8)+auth/reservado(16)+checksum(16)+VIPs. (Detalle en F5) |
| 10 | Secuencia | Elección por prioridad → maestro anuncia; failover si pierde advert |
| 11 | Addressing/naming | VRID + VIP |
| 12 | Routing/forwarding | VIP como gateway; preempt |
| 13 | Seguridad | Auth v2 débil (borrado en v3); ACLs; ver F6 |
| 14 | QoS/performance | Intervals de advertisement (1 s típico) |
| 15 | Observabilidad | Advertisements y VRID visibles; filtros vrrp.* |
| 16 | Interoperabilidad | Estándar (vs HSRP/CARP propietarios) |
| 17 | Implementaciones | keepalived, routers |
| 18 | Fuentes | RFC 5798 (nivel 1); IANA R1 — 26-08-2026 |

## F-58 · OSPFv3 — Open Shortest Path First v3 (IPv6)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | OSPFv3; RFC 5340; IETF; familia ROUT |
| 2 | Estado | Vigente (26-08-2026; RFC 5340) |
| 3 | Finalidad | IGP link-state para IPv6 (y IPv4 con address families) dentro de un AS. **No usar** para interdominio (BGP) |
| 4 | Encapsulación | Corre **directamente sobre IPv6** (IP protocol number **89**) |
| 5 | Capas | OSI 3 (Red); plano: control |
| 6 | Transporte y direccionamiento | IP protocol 89; multicast ff02::5/ff02::6; área-link |
| 7 | PDU | **Paquete OSPF** (tipo 2/3 como OSPFv2) |
| 8 | Mensajes | Hello, DD, LSR, LSU, LSAck |
| 9 | Campos | Cabecera: version(3)+type+length+router-id+area-id+checksum+instance id. (Detalle en F5) |
| 10 | Secuencia | Hello → adyacencias → LSDB → SPF; áreas |
| 11 | Addressing/naming | Router-id; link-local en interfaces |
| 12 | Routing/forwarding | Dijkstra; LSAs (Link, Intra-Area-Prefix…) |
| 13 | Seguridad | Autenticación IPsec o criptográfica (RFC 7166); ver F6 |
| 14 | QoS/rendimiento | Convergencia rápida; timers |
| 15 | Observabilidad | Tipos y LSAs visibles; filtros ospf6.* |
| 16 | Interoperabilidad | Estándar; con SO; |
| 17 | Implementaciones | FRRouting, Cisco IOS-XR, Juniper, MikroTik |
| 18 | Fuentes | RFC 5340 (nivel 1); IANA R1 — 26-08-2026 |

## F-59 · DCCP — Datagram Congestion Control Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | DCCP; RFC 4340 (y RFC 4341/4342 CCIDs); IETF; familia TRAN |
| 2 | Estado | Experimental/vigente limitado (26-08-2026; RFC 4340) |
| 3 | Finalidad | Datagramas con control de congestión (para streaming), con handshake de opciones. **No usar** donde UDP+sobrecarga es suficiente |
| 4 | Encapsulación | Corre sobre **IPv4/IPv6** (IP protocol number **33**) |
| 5 | Capas | OSI 4 (Transporte); plano: datos |
| 6 | Transporte y direccionamiento | IP protocol 33; puertos DCCP |
| 7 | PDU | **Paquete DCCP** (cabecera + opciones) |
| 8 | Mensajes | Sync/SyncAck, Data, Close/CloseReq |
| 9 | Campos | Cabecera: source/dest ports(16+16)+type/reserved/data offset/CCval/CsCov(8)+checksum(16)+seq. (Detalle en F5) |
| 10 | Secuencia | DCCP-Request/Response → datos → Close |
| 11 | Addressing/naming | Puertos DCCP; service codes |
| 12 | Routing/forwarding | No participa |
| 13 | Seguridad | Sin cifrado nativo; ver F6 |
| 14 | QoS/rendimiento | CCID 2/3: TCP-like / TFRC |
| 15 | Observabilidad | Cabeceras y options visibles; filtros dccp.* |
| 16 | Interoperabilidad | Uso real escaso (IEEE 802.11e legacy) |
| 17 | Implementaciones | Kernel Linux (dccp) |
| 18 | Fuentes | RFC 4340 (nivel 1); IANA R1 — 26-08-2026 |

## F-60 · Telnet

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Telnet; RFC 854; IETF; familia APP |
| 2 | Estado | Obsoleto (26-08-2026; RFC 854); sustituido por SSH |
| 3 | Finalidad | Terminal remota en claro (NVT). **No usar** en producción (SSH) |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **23** registrado en IANA) |
| 5 | Capas | OSI 7 (Aplicación); plano: datos |
| 6 | Transporte y direccionamiento | TCP 23 |
| 7 | PDU | **Secuencia de bytes NVT** + comandos IAC |
| 8 | Mensajes | IAC WILL/WONT/DO/DONT (negociación), Go-Ahead, Interrupt |
| 9 | Campos | Byte-oriented; opciones 0-255. (Detalle en F5) |
| 10 | Secuencia | TCP → negociación de opciones → sesión → logout |
| 11 | Addressing/naming | host:23; login local |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | Credenciales en claro (T1071.001); ver F6 |
| 14 | QoS/rendimiento | Sin control de flujo propio (TCP) |
| 15 | Observabilidad | IAC y opciones visibles; filtros telnet.* |
| 16 | Interoperabilidad | Legacy amplio |
| 17 | Implementaciones | Clientelnet, inetd (legacy) |
| 18 | Fuentes | RFC 854 (nivel 1); IANA R1 — 26-08-2026 |

## F-61 · NETCONF — Network Configuration Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | NETCONF; RFC 6241; IETF; familia GEST |
| 2 | Estado | Vigente (26-08-2026; RFC 6241) |
| 3 | Finalidad | Configuración y gestión de dispositivos de red basada en operaciones RPC sobre XML, con datastores. **No usar** para streams de telemetría alta (gNMI) |
| 4 | Encapsulación | Corre sobre **SSH** (puerto **830** habitual; también TLS 6513) |
| 5 | Capas | OSI 7 (Aplicación de gestión); plano: gestión |
| 6 | Transporte y direccionamiento | SSH 830 (NETCONF over SSH, RFC 6242) |
| 7 | PDU | **Mensaje XML** (hello + RPC/rpc-reply + notification) |
| 8 | Mensajes | get, get-config, edit-config, copy-config, lock/unlock, commit, validate |
| 9 | Campos | XML: <rpc>, <message-id>, operación, <data> con YANG. (Detalle en F5) |
| 10 | Secuencia | Hello con capabilities → RPC → rpc-reply (ok o error) |
| 11 | Addressing/naming | Datastores (candidate/running/startup); YANG models |
| 12 | Routing/forwarding | No (gestión) |
| 13 | Seguridad | Transporte SSH/TLS; NACM (RFC 8341) para control de acceso; ver F6 |
| 14 | QoS/rendimiento | Commit confirm; transacciones |
| 15 | Observabilidad | RPCs y réplicas visibles; filtros netconf.* |
| 16 | Interoperabilidad | Amplia en equipos modernos con YANG |
| 17 | Implementaciones | OpenCONFIG? (no), junos, IOS-XR, yangtools, Netopeer |
| 18 | Fuentes | RFC 6241/6242 (nivel 1); IANA R1 — 26-08-2026 |

## F-62 · Syslog

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Syslog; RFC 5424 (protocolo), RFC 3164 (BSD legado); IETF; familia GEST |
| 2 | Estado | Vigente (26-08-2026; RFC 5424) |
| 3 | Finalidad | Transporte de mensajes de registro (eventos) desde dispositivos a centros de log. **No usar** como bus de datos (estructurado) |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **514** registrado; también TCP/TLS 6514) |
| 5 | Capas | OSI 7 (Aplicación de gestión); plano: gestión |
| 6 | Transporte y direccionamiento | UDP 514 (TLS 6514 con syslog-tls) |
| 7 | PDU | **Mensaje syslog** (textual con estructura RFC 5424) |
| 8 | Mensajes | Mensajes PRI + cabecera (timestamp, hostname, app, msgid) + structured data + MSG |
| 9 | Campos | PRI(8: facility×8+severity)+version+timestamp+hostname+app-name+procid+msgid+SD+MSG. (Detalle en F5) |
| 10 | Secuencia | Emisor → receptor (UDP fire-and-forget; TCP con retransmisión) |
| 11 | Addressing/naming | Host/App; facility/severity |
| 12 | Routing/forwarding | Relays y forwards |
| 13 | Seguridad | En claro por defecto (TLS opcional); ver F6 |
| 14 | QoS/rendimiento | Pérdida posible en UDP; rate limiting |
| 15 | Observabilidad | Mensajes y PRI visibles; filtros syslog.* |
| 16 | Interoperabilidad | Amplia |
| 17 | Implementaciones | rsyslog, syslog-ng, Windows Event Collector |
| 18 | Fuentes | RFC 5424 (nivel 1); IANA R1 — 26-08-2026 |

## F-63 · TACACS+ — Terminal Access Controller Access-Control System Plus

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | TACACS+; protocolo propietario Cisco (TACACS+ RFC 8907); familia GEST |
| 2 | Estado | Vigente (26-08-2026; RFC 8907 estandarizado) |
| 3 | Finalidad | AAA para administración de dispositivos (autenticación, autorización, accounting) con cifrado total del cuerpo. **No usar** para acceso de red (RADIUS) |
| 4 | Encapsulación | Corre sobre **TCP** (puerto **49** registrado en IANA) |
| 5 | Capas | OSI 7 (Aplicación AAA); plano: gestión/seguridad |
| 6 | Transporte y direccionamiento | TCP 49; shared secret |
| 7 | PDU | **Paquete TACACS+** (cabecera + cuerpo cifrado) |
| 8 | Mensajes | Authentication (Start/Reply), Authorization (Request/Response), Accounting (Request/Reply) |
| 9 | Campos | Cabecera: version(8)+type(8)+seq(8)+flags(8)+session_id(32)+length(32). (Detalle en F5) |
| 10 | Secuencia | Auth Start → Reply (prompts) → Cont; Authorization; Accounting |
| 11 | Addressing/naming | Session ID; users/groups |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | Cuerpo cifrado (XOR con MD5 del shared secret); no oculta length; ver F6 |
| 14 | QoS/rendimiento | TCP fiable; por comando (authorization) |
| 15 | Observabilidad | Tipos y server responses visibles en claro; filtros tacacs.* |
| 16 | Interoperabilidad | Estándar AAA en administración de red |
| 17 | Implementaciones | Cisco ACS/ISE, tac_plus, FreeRADIUS? (no, tacacs+ daemon) |
| 18 | Fuentes | RFC 8907 (nivel 1); IANA R1 — 26-08-2026 |

## F-64 · PTP — Precision Time Protocol (IEEE 1588)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | PTP; IEEE 1588-2019 (y 802.1AS/gPTP); IEEE; familia SYNC |
| 2 | Estado | Vigente (26-08-2026; IEEE 1588-2019) |
| 3 | Finalidad | Sincronización temporal de precisión sub-µs (perfiles: telecom, industrial, AVB). **No usar** si ms bastan (NTP) |
| 4 | Encapsulación | Corre sobre **UDP** (multicast 224.0.1.129; puertos **319** event, **320** general) y Ethernet (802.1AS) |
| 5 | Capas | OSI 7 (Aplicación de tiempo); plano: datos |
| 6 | Transporte y direccionamiento | UDP 319/320; boundary/transparent clocks |
| 7 | PDU | **Mensaje PTP** (cabecera v2) |
| 8 | Mensajes | Sync, Follow_Up, Delay_Req, Delay_Resp, Announce, PDelay_Req/Resp |
| 9 | Campos | Cabecera: version(4 bits)+msgType(4)+reserved+field flags(16)+correction(64)+sourcePortIdentity(80)+sequence(16)+logMessageInterval(8). (Detalle en F5) |
| 10 | Secuencia | BMCA (best master) → Sync/Follow_Up → Delay requests; corrección por hardware timestamps |
| 11 | Addressing/naming | Clock identity; domains |
| 12 | Routing/forwarding | No (tiempo) |
| 13 | Seguridad | Sin autenticación por defecto (IEEE 1588-2019 añade opciones); ver F6 |
| 14 | QoS/rendimiento | Precisión sub-µs con hardware timestamping; sync interval |
| 15 | Observabilidad | Mensajes y correcciones visibles; filtros ptp.* |
| 16 | Interoperabilidad | Perfiles definidos (telecom G.8275, AVB 802.1AS) |
| 17 | Implementaciones | PTPd, linuxptp, NICs con timestamping, switches G.8275 |
| 18 | Fuentes | IEEE 1588-2019 (nivel 1) — 26-08-2026 |

## F-65 · FC — Fibre Channel

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Fibre Channel; T11 (INCITS); ANSI/ISO; familia STOR |
| 2 | Estado | Vigente (26-08-2026; FC-PI, FC-FS) |
| 3 | Finalidad | SAN dedicada de alta velocidad para bloques SCSI (FC-SCSI). **No usar** en redes convergentes simples (iSCSI) |
| 4 | Encapsulación | Red dedicada (no IP): fibras 8/16/32/64G; capas FC-0..FC-4 |
| 5 | Capas | Modelo FC (FC-0 físico … FC-4 protocolos) |
| 6 | Transporte y direccionamiento | WWN (World Wide Name); sin puertos TCP/IP |
| 7 | PDU | **FC Frame** (240 bytes de payload máximo, FCP_CMND/transfer) |
| 8 | Mensajes | FC-2: frames, sequences, exchanges; FCP: SCSI commands |
| 9 | Campos | Start/EOFs+header: routing(8)+dest(24)+src+hdrctl+seq id+cnt+exchange+len+CRC. (Detalle en F5) |
| 10 | Secuencia | Fabric login (FLOGI) → port login (PLOGI) → procesos (name server) → I/O |
| 11 | Addressing/naming | WWNN/WWPN; FCID 24 bits |
| 12 | Routing/forwarding | Fabric switches (FSPF) |
| 13 | Seguridad | Zonas y LUN masking; FC-SP auth (DH-CHAP) opcional; ver F6 |
| 14 | QoS/rendimiento | Determinista, sin pérdidas (CC), baja latencia |
| 15 | Observabilidad | Frames y SOF/EOF visibles con analysis tools; FLGI |
| 16 | Interoperabilidad | Estandarizada (conformance) |
| 17 | Implementaciones | Switches (Brocade, Cisco MDS), HBAs, SAN storage |
| 18 | Fuentes | T11/INCITS (nivel 1) — 26-08-2026 |

## F-66 · EAP — Extensible Authentication Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | EAP; RFC 3748 (y RFC 5247); IETF; familia SEG |
| 2 | Estado | Vigente (26-08-2026; RFC 3748; EAP2 RFC 9190) |
| 3 | Finalidad | Marco de autenticación portable sobre múltiples transportes (802.1X, P2P, IKEv2). **No usar** para autorización |
| 4 | Encapsulación | Corre sobre enlace (802.1X/EAPoL) o transportes (EAP-PEAP sobre TLS…) |
| 5 | Capas | OSI 2/7 (autenticación); plano: seguridad |
| 6 | Transporte y direccionamiento | Sin puertos; transportes según método |
| 7 | PDU | **Mensaje EAP** (code+id+length+payload) |
| 8 | Mensajes | Request, Response, Success, Failure; types: Identity, MD5-Challenge, TLS, TTLS, PEAP |
| 9 | Campos | Code(8)+Identifier(8)+Length(16)+Type(8)+data (en Request/Response). (Detalle en F5) |
| 10 | Secuencia | Request Identity → conversaciones de método → Success/Failure; NAK |
| 11 | Addressing/naming | Identidades del método |
| 12 | Routing/forwarding | Passthrough del autenticador |
| 13 | Seguridad | Depende del método (EAP-TLS fuerte; MD5 débil); session key derivation; ver F6 |
| 14 | QoS/rendimiento | Retransmisión; tiempo de método |
| 15 | Observabilidad | Codes y types visibles; filtros eap.* |
| 16 | Interoperabilidad | Amplia en Wi-Fi (WPA-Enterprise), VPN |
| 17 | Implementaciones | wpa_supplicant, RADIUS servers (FreeRADIUS), hostapd |
| 18 | Fuentes | RFC 3748/9190 (nivel 1) — 26-08-2026 |

## F-67 · DNSSEC — DNS Security Extensions

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | DNSSEC; RFC 4033/4034/4035 (+ NSEC3 RFC 5155); IETF; familia SEG |
| 2 | Estado | Vigente (26-08-2026; RFC 4033-4035) |
| 3 | Finalidad | Integridad y autenticación de respuestas DNS mediante firmas (cadena desde la raíz). **No usar** para confidencialidad |
| 4 | Encapsulación | Corre sobre **DNS** (registros RRSIG/NSEC/DS; mismo transporte UDP/TCP 53) |
| 5 | Capas | OSI 7 (Aplicación/seguridad); plano: seguridad |
| 6 | Transporte y direccionamiento | Igual que DNS; claves DNSKEY |
| 7 | PDU | **Registros DNSSEC** dentro del mensaje DNS (RRSIG, DNSKEY, DS, NSEC/NSEC3) |
| 8 | Mensajes | Firma de RRsets (RRSIG), clave pública (DNSKEY), DS en delegaciones; respuestas con AD/DO |
| 9 | Campos | RRSIG: type covered, algorithm, labels, original TTL, expiration, inception, key tag, signer, signature. (Detalle en F5) |
| 10 | Secuencia | Publicación de DNSKEY/DS → resolución firma y valida (chain of trust); NSEC walking mitigado con NSEC3 |
| 11 | Addressing/naming | Zonas y delegaciones firmadas; KSK/ZSK |
| 12 | Routing/forwarding | Validadores en resolvers |
| 13 | Seguridad | Firma (RSA/ECDSA/EdDSA) + claves; no cifra queries; ver F6 |
| 14 | QoS/rendimiento | Respuestas más grandes; validación coste |
| 15 | Observabilidad | RRSIG/DNSKEY en queries con DO=1; filtros dnssec.* |
| 16 | Interoperabilidad | Amplia en TLDs y resolvers |
| 17 | Implementaciones | BIND, Knot, Unbound (validator), dnssec-tools |
| 18 | Fuentes | RFC 4033-4035 (nivel 1) — 26-08-2026 |

## F-68 · PROFINET — PROFINET (PNIO)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | PROFINET; IEC 61158 (PI, PROFIBUS/PROFINET International); familia IOT |
| 2 | Estado | Vigente (26-08-2026; IEC 61158) |
| 3 | Finalidad | Automatización industrial en tiempo real sobre Ethernet (IRT con hardware). **No usar** si no se requiere RT industrial |
| 4 | Encapsulación | Corre sobre **Ethernet** (EtherType **0x8892**); ciclos IRT |
| 5 | Capas | OSI 7 + extensiones de tiempo real (RTC/IRT); plano: datos |
| 6 | Transporte y direccionamiento | EtherType 0x8892; sin puertos (DCP para discovery) |
| 7 | PDU | **Frame PROFINET** (RTC: frameID + datos de ciclo; DCP: configuración) |
| 8 | Mensajes | RTC (ciclo), RTA, DCP (identify/get/set), alarms |
| 9 | Campos | Cabecera: frameID(16)+data; DCP con service/options. (Detalle en F5) |
| 10 | Secuencia | Nombre→dirección (DCP) → arranque → datos cíclicos; alarmas |
| 11 | Addressing/naming | Station name; MAC; slot/subslot |
| 12 | Routing/forwarding | Switches con IRT (hardware clock) |
| 13 | Seguridad | PROFINET Security (IEC 62443); VLAN/ACL; ver F6 |
| 14 | QoS/rendimiento | Determinista; ciclo 31,25 µs-4 ms |
| 15 | Observabilidad | Frames y frameIDs visibles; filtros pn.* |
| 16 | Interoperabilidad | Certificación PI |
| 17 | Implementaciones | PLCs Siemens, stacks (p-net), PacketLogger |
| 18 | Fuentes | IEC 61158 (nivel 1) — 26-08-2026 |

## F-69 · DVB-S2 — DVB-S2 (satélite)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | DVB-S2; ETSI EN 302 307; ETSI; familia RAD |
| 2 | Estado | Vigente (26-08-2026; EN 302 307; enlaces a DVB-S2X) |
| 3 | Finalidad | Difusión/retorno por satélite: modulación adaptativa (QPSK-8PSK-16/32APSK). **No usar** en redes terrestres |
| 4 | Encapsulación | Corre sobre el enlace RF satelital (bandas Ku/Ka) |
| 5 | Capas | OSI 1-2 (Física + enlace satelital); plano: datos |
| 6 | Transporte y direccionamiento | Sin IP propia; BBFRAME + TS o GSE |
| 7 | PDU | **BBFRAME DVB-S2** + FECFRAME + PLFRAME |
| 8 | Mensajes | Tramas base (BBFRAME), physical layer framing, PLHEADER de sincronización |
| 9 | Campos | PLHEADER: SOF(26)+PLSC(7)+pilotos…; MODCOD streams. (Detalle en F5) |
| 10 | Secuencia | Modulación/COD seleccionada por MODCOD → codificación (BCH+LDPC) → PLFRAME |
| 11 | Addressing/naming | Streams TS/GSE; 8PSK/APSK modo |
| 12 | Routing/forwarding | Satelital (beams) |
| 13 | Seguridad | DVB-S2 no cifra por defecto (CAS opcional); ver F6 |
| 14 | QoS/rendimiento | VCM/ACM adaptativo; retardo elevado de enlace |
| 15 | Observabilidad | Constelaciones y MODCOD visibles con tools |
| 16 | Interoperabilidad | Estandarizada ETSI |
| 17 | Implementaciones | Moduladores/demoduladores profesionales (Newtec, Work Microwave) |
| 18 | Fuentes | ETSI EN 302 307 (nivel 1) — 26-08-2026 |

## F-70 · L2TP — Layer 2 Tunneling Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | L2TP; RFC 3931 (L2TPv3), RFC 2661 (v2); IETF; familia ACEL |
| 2 | Estado | Vigente (26-08-2026; RFC 3931; uso VPN LNS típico con IPsec) |
| 3 | Finalidad | Túneles de capa 2 (PPP/ethernet) sobre IP para VPN de acceso. **No usar** sin IPsec para transporte inseguro |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **1701** registrado; 500/4500 con IPsec) |
| 5 | Capas | OSI 2/3 (tunneling); plano: datos |
| 6 | Transporte y direccionamiento | UDP 1701; session/tunnel IDs |
| 7 | PDU | **Mensaje L2TP** (cabecera + AVP) |
| 8 | Mensajes | Control: Start-Control-Connection, Call/Set-Link-Info; datos PPP |
| 9 | Campos | Cabecera: flags(16: T/L/S/O/P)+ver+length+tunnel ID+session ID+Ns/Nr. (Detalle en F5) |
| 10 | Secuencia | SCCRQ → SCCRP → SCCCN → sesiones; datos |
| 11 | Addressing/naming | Tunnel/session IDs |
| 12 | Routing/forwarding | LNS concentra sesiones |
| 13 | Seguridad | Sin cifrado (cooperativa con IPsec ESP); ver F6 |
| 14 | QoS/rendimiento | Encapsulación PPP; overhead |
| 15 | Observabilidad | Mensajes y AVPs visibles; filtros l2tp.* |
| 16 | Interoperabilidad | VPNs L2TP/IPsec entre SO y routers |
| 17 | Implementaciones | Linux (xl2tpd), Windows, routers |
| 18 | Fuentes | RFC 3931/2661 (nivel 1); IANA R1 — 26-08-2026 |

## F-71 · CSMA/CD — Ethernet clásico

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | CSMA/CD; IEEE 802.3 (half-duplex); IEEE; familia ACEL |
| 2 | Estado | Histórico (26-08-2026; irrelevante en full-duplex moderno) |
| 3 | Finalidad | Acceso al medio en Ethernet half-duplex con detección de colisiones. **No usar**: redes modernas son full-duplex |
| 4 | Encapsulación | Corre sobre el medio (cobre coaxial/par); bajo Ethernet |
| 5 | Capas | OSI 1-2 (control de acceso al medio); plano: datos |
| 6 | Transporte y direccionamiento | Sin puertos; MAC; slot time |
| 7 | PDU | Trama Ethernet (sin cambio respecto a 802.3) |
| 8 | Mensajes | Tramas de datos; jam signal en colisión |
| 9 | Campos | Trama 802.3 estándar (DA/SA/EtherType+payload+FCS) |
| 10 | Secuencia | Carrier sense → transmisión → colisión → backoff exponencial (BEB) |
| 11 | Addressing/naming | MAC 48 bits |
| 12 | Routing/forwarding | No aplica (hub/segmento compartido) |
| 13 | Seguridad | Ninguna (medio compartido); promiscuo posible |
| 14 | QoS/rendimiento | Decremento de eficiencia al cargar; half-duplex limitado |
| 15 | Observabilidad | Colisiones y errores en NICs/hubs |
| 16 | Interoperabilidad | Histórica |
| 17 | Implementaciones | NICs legacy, hubs |
| 18 | Fuentes | IEEE 802.3 (nivel 1) — 26-08-2026 |

## F-72 · LLMNR — Link-Local Multicast Name Resolution

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | LLMNR; RFC 4795; IETF; familia ADCONF |
| 2 | Estado | Vigente con uso decreciente (26-08-2026; RFC 4795; desplazado por mDNS) |
| 3 | Finalidad | Resolución de nombres en LAN sin DNS (hosts Windows legacy). **No usar** en diseño nuevo (mDNS) |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **5355**) y TCP (mismo puerto); multicast 224.0.0.252/ff02::1:3 |
| 5 | Capas | OSI 7 (Aplicación/nombres); plano: datos |
| 6 | Transporte y direccionamiento | UDP/TCP 5355 |
| 7 | PDU | **Mensaje LLMNR** (formato DNS) |
| 8 | Mensajes | Query/Response (formato DNS con flags especiales) |
| 9 | Campos | Cabecera DNS (ID, flags, counts) |
| 10 | Secuencia | Query multicast → respuesta unicast del propietario |
| 11 | Addressing/naming | Nombres de host sin sufijo de dominio |
| 12 | Routing/forwarding | Link-local |
| 13 | Seguridad | Sin autenticación (spoofing de respuestas); ver F6 |
| 14 | QoS/rendimiento | Cache corta |
| 15 | Observabilidad | Queries visibles; filtros llmnr.* |
| 16 | Interoperabilidad | Windows; suplantado por mDNS en modernos |
| 17 | Implementaciones | Windows, systemd-resolved (mDNS/LLMNR) |
| 18 | Fuentes | RFC 4795 (nivel 1); IANA R1 — 26-08-2026 |

## F-73 · NetBIOS — NetBIOS sobre TCP/IP

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | NetBIOS; RFC 1001/1002 (NetBIOS over TCP/IP); IETF; familia ADCONF |
| 2 | Estado | Histórico (26-08-2026; RFC 1001/1002; uso decreciente en Windows) |
| 3 | Finalidad | Nombres y sesiones NetBIOS sobre IP (legacy Windows). **No usar** en diseño nuevo (DNS/SMB directo) |
| 4 | Encapsulación | Corre sobre **UDP/TCP** (puertos **137** name, **138** datagram, **139** session) |
| 5 | Capas | OSI 7 (sesión/nombres legacy); plano: datos |
| 6 | Transporte y direccionamiento | UDP/TCP 137-139; nombres de 16 bytes |
| 7 | PDU | **Mensaje NetBIOS** (name service/datagram/session) |
| 8 | Mensajes | Name Query/Register/Release, Datagrams, Session Messages |
| 9 | Campos | Cabeceras de name service (NAME_TRN_ID, opcode…) y session (tipo, length). (Detalle en F5) |
| 10 | Secuencia | Registro/consulta de nombres → datagramas o sesiones TCP |
| 11 | Addressing/naming | NetBIOS names (15+1 tipo) |
| 12 | Routing/forwarding | No (sesión) |
| 13 | Seguridad | Sin autenticación; expuesto (WannaCry vía 445/139); desactivar; ver F6 |
| 14 | QoS/rendimiento | Broadcast frecuente; sin cifrado |
| 15 | Observabilidad | Queries y sesiones visibles; filtros nbns/netbios.* |
| 16 | Interoperabilidad | Legacy Windows |
| 17 | Implementaciones | Windows (NetBIOS sobre TCP), Samba |
| 18 | Fuentes | RFC 1001/1002 (nivel 1); IANA R1 — 26-08-2026 |

## F-74 · RIPv2 — Routing Information Protocol v2

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | RIPv2; RFC 2453; IETF; familia ROUT |
| 2 | Estado | Vigente en legados (26-08-2026; RFC 2453) |
| 3 | Finalidad | Distribución de rutas vector-distancia con máscaras (CIDR) y autenticación. **No usar** en redes medianas (OSPF/IS-IS) |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **520**) con multicast 224.0.0.9 |
| 5 | Capas | OSI 7/Aplicación; plano: control |
| 6 | Transporte y direccionamiento | UDP 520; métricas 1-15 |
| 7 | PDU | **Mensaje RIPv2** (tabla completa) |
| 8 | Mensajes | Request/Response; actualizaciones periódicas 30 s |
| 9 | Campos | Cabecera: cmd+ver; entradas con máscara y next hop. (Detalle en F5) |
| 10 | Secuencia | Solicitud → respuesta; split horizon/poison |
| 11 | Addressing/naming | Prefijos CIDR |
| 12 | Routing/forwarding | Menor métrica; timers |
| 13 | Seguridad | Passphrase plaintext/MD5 (RFC 4822); ver F6 |
| 14 | QoS/rendimiento | Convergencia lenta; 15 saltos |
| 15 | Observabilidad | Mensajes visibles; filtros rip.* |
| 16 | Interoperabilidad | RDOM v1/v2 |
| 17 | Implementaciones | FRRouting, bird, routers |
| 18 | Fuentes | RFC 2453 (nivel 1); IANA R1 — 26-08-2026 |

## F-75 · SR — Segment Routing (SR-MPLS)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Segment Routing; RFC 8402 (arch), RFC 8660 (SR-MPLS); IETF; familia ROUT |
| 2 | Estado | Vigente (26-08-2026; RFC 8402/8660; también SRv6 RFC 8986) |
| 3 | Finalidad | Codificación de rutas como listas de segmentos (labels MPLS o SRv6) con fuente del camino. **No usar** sin controlador/SR-capable |
| 4 | Encapsulación | Corre sobre **MPLS** (lista de labels) o **IPv6** (SRH) |
| 5 | Capas | OSI 2.5/3 (Red); plano: datos + control |
| 6 | Transporte y direccionamiento | Label stack; SIDs |
| 7 | PDU | **Paquete SR-MPLS** (stack de labels) |
| 8 | Mensajes | Datos con SID list; IGP/BGP-LS anuncian SIDs |
| 9 | Campos | Por label MPLS: label+TC+S+TTL (ver F5 MPLS) |
| 10 | Secuencia | Segment list impuesto → forwarding por SID; TI-LFA |
| 11 | Addressing/naming | SIDs globales/locales |
| 12 | Routing/forwarding | SR policies, endpoint SID |
| 13 | Seguridad | Herencia del plano (control protect); ver F6 |
| 14 | QoS/rendimiento | Ingeniería de tráfico sin RSVP-TE; SRv6 overhead |
| 15 | Observabilidad | Segment lists visibles; filtros mpls/srv6.* |
| 16 | Interoperabilidad | Crecimiento en SP/DC |
| 17 | Implementaciones | Cisco, Juniper, FRRouting (SR), Linux SRv6 |
| 18 | Fuentes | RFC 8402/8660 (nivel 1) — 26-08-2026 |

## F-76 · MIP — Mobile IP (IPv4)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Mobile IPv4; RFC 5944 (y 3344 actualizado); IETF; familia MOV |
| 2 | Estado | Sustituido en la práctica (26-08-2026; RFC 5944; movilidad 3GPP domina) |
| 3 | Finalidad | Mantener conectividad IP al cambiar de red (home agent/conexión por care-of). **No usar** en diseño nuevo |
| 4 | Encapsulación | Corre sobre **IPv4** (túneles IP-in-IP; protocol 55 y 4) |
| 5 | Capas | OSI 3 (Red, movilidad); plano: control/datos |
| 6 | Transporte y direccionamiento | CoA binding; home address |
| 7 | PDU | **Mensajes de control MIP** (Registration Request/Reply) sobre UDP 434 |
| 8 | Mensajes | Registration Request/Reply/Revocation; advertisements de agente |
| 9 | Campos | Extensiones de registro (CoA, lifetime, flags). (Detalle en F5) |
| 10 | Secuencia | Agente IRDP → registro → túneles al CoA |
| 11 | Addressing/naming | Home address + CoA |
| 12 | Routing/forwarding | HA intermedia el tráfico |
| 13 | Seguridad | Autenticación de registros (MN-HA key); ver F6 |
| 14 | QoS/rendimiento | Triangle routing; over header IP-in-IP |
| 15 | Observabilidad | Registros visibles; filtros mobile-ip.* |
| 16 | Interoperabilidad | Implementaciones escasas hoy |
| 17 | Implementaciones | Kernel (mipv6?), stacks legacy |
| 18 | Fuentes | RFC 5944 (nivel 1); IANA R1 — 26-08-2026 |

## F-77 · MIPv6 — Mobile IPv6

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Mobile IPv6; RFC 6275; IETF; familia MOV |
| 2 | Estado | Vigente con uso limitado (26-08-2026; RFC 6275) |
| 3 | Finalidad | Movilidad IPv6 nativa (sin agentes extranjeros; routing optimization). **No usar** en entornos 3GPP (usar PMIP/GTP) |
| 4 | Encapsulación | Corre sobre **IPv6** (extensión dest options + túneles IP6) |
| 5 | Capas | OSI 3 (Red, movilidad); plano: control/datos |
| 6 | Transporte y direccionamiento | Binding updates; home address option |
| 7 | PDU | **Mensajes MIPv6** (BU/BA) en extension headers |
| 8 | Mensajes | Binding Update/Ack, Route Optimization, Mobility Header (MH) |
| 9 | Campos | Mobility Header (MH type, checksum…) + binding info. (Detalle en F5) |
| 10 | Secuencia | Detección de movimiento → Binding Update → túneles/RO |
| 11 | Addressing/naming | Home address + CoA |
| 12 | Routing/forwarding | HA; directo tras RO |
| 13 | Seguridad | IPsec de bindings (RFC 3776); ver F6 |
| 14 | QoS/rendimiento | Routable optimization; overhead |
| 15 | Observabilidad | MHs y bindings visibles; filtros mipv6.* |
| 16 | Interoperabilidad | Adopción limitada |
| 17 | Implementaciones | NEMO, kernels experimentales |
| 18 | Fuentes | RFC 6275 (nivel 1) — 26-08-2026 |

## F-78 · LISP — Locator/ID Separation Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | LISP; RFC 9300 (y push RFC 9301); IETF; familia MOV |
| 2 | Estado | Experimental (26-08-2026; RFC 9300/9301) |
| 3 | Finalidad | Separar identidad (EID) de localización (RLOC) para movilidad/multihoming a gran escala. **No usar** sin caso de uso de movilidad |
| 4 | Encapsulación | Corre sobre **UDP** (puertos **4341** (datos) y 4342 (control), registrados) |
| 5 | Capas | OSI 3 (overlay); plano: datos + control |
| 6 | Transporte y direccionamiento | UDP 4341/4342; EID/RLOC |
| 7 | PDU | **Paquete LISP** (cabecera + EID payload) |
| 8 | Mensajes | Datos encapsulados; Map-Register/Map-Request/Map-Reply (control) |
| 9 | Campos | Cabecera 8 B: N/L/E/flags+nonce+instance. (Detalle en F5) |
| 10 | Secuencia | ITR consulta mapping → encap hacia ETR; caching |
| 11 | Addressing/naming | EID prefixes; RLOCs |
| 12 | Routing/forwarding | Overlay entre ITR/ETR |
| 13 | Seguridad | Sin cifrado nativo (IPsec opcional); ver F6 |
| 14 | QoS/rendimiento | Mapping cache; encap overhead |
| 15 | Observabilidad | Map messages visibles; filtros lisp.* |
| 16 | Interoperabilidad | Experimental; OpenOverlayRouter |
| 17 | Implementaciones | OOR (OpenOverlayRouter), Cisco |
| 18 | Fuentes | RFC 9300 (nivel 1); IANA R1 — 26-08-2026 |

## F-79 · RESTCONF

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | RESTCONF; RFC 8040; IETF; familia GEST |
| 2 | Estado | Vigente (26-08-2026; RFC 8040) |
| 3 | Finalidad | API REST sobre YANG para configuración de dispositivos (datastores). **No usar** para transacciones complejas (NETCONF) |
| 4 | Encapsulación | Corre sobre **HTTP(S)** (HTTPS habitual; puerto 443/8443) |
| 5 | Capas | OSI 7 (Aplicación de gestión); plano: gestión |
| 6 | Transporte y direccionamiento | HTTPS; endpoints /restconf/data |
| 7 | PDU | **Petición/respuesta HTTP** (JSON/XML + YANG) |
| 8 | Mensajes | GET, POST, PUT, PATCH, DELETE sobre recursos YANG |
| 9 | Campos | Cabeceras HTTP + cuerpo (content-type; etag opcional). (Detalle F5) |
| 10 | Secuencia | Auth HTTPS → CRUD sobre recursos |
| 11 | Addressing/naming | URI de recursos YANG |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | HTTPS/mTLS; NACM (RFC 8341); ver F6 |
| 14 | QoS/rendimiento | Simplicidad REST |
| 15 | Observabilidad | Requests/responses; filtros HTTP |
| 16 | Interoperabilidad | Amplia en equipos modernos |
| 17 | Implementaciones | Clientes RESTCONF (curl, postman), servidores YANG |
| 18 | Fuentes | RFC 8040 (nivel 1) — 26-08-2026 |

## F-80 · gRPC — gRPC (telemetría)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | gRPC; protocolo de Google sobre HTTP/2 (CNCF); familia GEST |
| 2 | Estado | Vigente (26-08-2026; gRPC spec; telemetría de redes) |
| 3 | Finalidad | RPCs de alta eficiencia con protobuf sobre HTTP/2 (telemetría gNMI en redes). **No usar** para APIs REST simples |
| 4 | Encapsulación | Corre sobre **HTTP/2** (TLS opcional; puerto 443/57400 típico) |
| 5 | Capas | OSI 7 (Aplicación); plano: gestión |
| 6 | Transporte y direccionamiento | HTTP/2 streams; path /package.Service/Method |
| 7 | PDU | **Mensaje gRPC** (5-byte prefix: compresión+length + protobuf) |
| 8 | Mensajes | Unary, server/client streaming; gNMI Get/Subscribe |
| 9 | Campos | Prefix(40 bits: flag+length) + protobuf body. (Detalle en F5) |
| 10 | Secuencia | HTTP/2 → método → streams; trailers de estado |
| 11 | Addressing/naming | Service/Method; protobuf schemas |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | mTLS recomendado; no cifra por defecto; ver F6 |
| 14 | QoS/rendimiento | Multiplexación HTTP/2; protobuf compacto |
| 15 | Observabilidad | Métodos y payloads visibles; filtros grpc.* |
| 16 | Interoperabilidad | Estándar en cloud/microservicios |
| 17 | Implementaciones | grpc-go, grpc-java, gNMIc, OpenConfig gNMI |
| 18 | Fuentes | gRPC spec / gNMI (CNCF) — 26-08-2026 |

## F-81 · IPFIX — IP Flow Information Export

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | IPFIX; RFC 7011 (protocolo), 7012 (información); IETF; familia GEST |
| 2 | Estado | Vigente (26-08-2026; RFC 7011) |
| 3 | Finalidad | Exportación de flujos (NetFlow evolución estándar) con plantillas y campos extensibles. **No usar** sin metering en el router |
| 4 | Encapsulación | Corre sobre **UDP/TCP/SCTP** (puertos típicos 4739/4740; exporter definido) |
| 5 | Capas | OSI 7 (Aplicación de gestión); plano: gestión |
| 6 | Transporte y direccionamiento | SCTP recomendado para fiabilidad; observation domain |
| 7 | PDU | **Mensaje IPFIX** (cabecera + sets de plantilla/datos) |
| 8 | Mensajes | Template Sets, Data Sets, Options; export de flujos |
| 9 | Campos | Cabecera: version(10), length, export time, sequence, domain. (Detalle en F5) |
| 10 | Secuencia | Metrado → plantillas → data records; reexport TTL |
| 11 | Addressing/naming | Observation domains; Enterprise-specific IEs |
| 12 | Routing/forwarding | No (exportación) |
| 13 | Seguridad | Sin cifrado nativo (DTLS/SCTP opcional); ver F6 |
| 14 | QoS/rendimiento | Template flexible; batch export |
| 15 | Observabilidad | Flujos y plantillas visibles; filtros ipfix.* |
| 16 | Interoperabilidad | Estándar (sucede a NetFlow) |
| 17 | Implementaciones | nfdump, go-ipfix, collectors (pmacct) |
| 18 | Fuentes | RFC 7011/7012 (nivel 1); IANA R1 — 26-08-2026 |

## F-82 · NetFlow — NetFlow (Cisco)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | NetFlow; protocolo propietario Cisco (v5/v9/rigid); familia GEST |
| 2 | Estado | Propietario/legacy (26-08-2026; v9 base de IPFIX) |
| 3 | Finalidad | Exportación de flujos IP (metadatos de sesión) para visibilidad. **No usar** en diseño nuevo (IPFIX) |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **2055** típico; 9996 v9) |
| 5 | Capas | OSI 7 (Aplicación de gestión); plano: gestión |
| 6 | Transporte y direccionamiento | UDP 2055/9996 |
| 7 | PDU | **Paquete NetFlow** (cabecera + records; v5 fijo 24 B) |
| 8 | Mensajes | Export v5 (fixed) o v9 (template) |
| 9 | Campos | v5: version+count+uptime+secs+...records(24B). (Detalle en F5) |
| 10 | Secuencia | Cache de flujos → export periódico |
| 11 | Addressing/naming | Observation point; IPs del flujo |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | Sin cifrado; ver F6 |
| 14 | QoS/rendimiento | Sampling frecuente |
| 15 | Observabilidad | Flujos exportados; filtros netflow.* |
| 16 | Interoperabilidad | Collectors comunes (nfdump, ELK) |
| 17 | Implementaciones | Cisco, ntop, pmacct |
| 18 | Fuentes | Cisco NetFlow (documentación) — 26-08-2026 |

## F-83 · ICMP — Internet Control Message Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | ICMP; RFC 792; IETF; familia GEST |
| 2 | Estado | Vigente (26-08-2026; RFC 792) |
| 3 | Finalidad | Diagnóstico y señalización IP (errores, echo, TTL excedido). **No usar** como transporte confiable |
| 4 | Encapsulación | Corre sobre **IPv4** (IP protocol number **1**) |
| 5 | Capas | OSI 3 (Red, control); plano: control |
| 6 | Transporte y direccionamiento | IP protocol 1; tipos 0/3/5/8/11/13 |
| 7 | PDU | **Mensaje ICMP** (ver F5) |
| 8 | Mensajes | Echo request/reply, dest unreachable, redirect, time exceeded |
| 9 | Campos | Type(8)+Code(8)+Checksum(16)+Rest. (Detalle en F5) |
| 10 | Secuencia | Ej.: ping request → reply; error → cabecera IP original |
| 11 | Addressing/naming | IP origen/destino |
| 12 | Routing/forwarding | Redirect para actualización de rutas |
| 13 | Seguridad | Riesgo de scanning/fuzzing; turnos a ICMP tunneling (T1095); ver F6 |
| 14 | QoS/rendimiento | Sin garantías |
| 15 | Observabilidad | Tipos visibles; filtros icmp.* |
| 16 | Interoperabilidad | Universal |
| 17 | Implementaciones | Pilas IP de SO, ping/traceroute |
| 18 | Fuentes | RFC 792 (nivel 1); IANA R1 — 26-08-2026 |

## F-84 · ICMPv6 — ICMP for IPv6

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | ICMPv6; RFC 4443; IETF; familia GEST |
| 2 | Estado | Vigente (26-08-2026; RFC 4443) |
| 3 | Finalidad | Señalización y control IPv6 (errores, echo, NDP/MLD). **No usar** como transporte |
| 4 | Encapsulación | Corre sobre **IPv6** (Next Header **58**) |
| 5 | Capas | OSI 3 (Red, control); plano: control |
| 6 | Transporte y direccionamiento | Next header 58; tipos 1-4 errores, 128/129 echo, 133-137 NDP |
| 7 | PDU | **Mensaje ICMPv6** (ver F5) |
| 8 | Mensajes | Packet too big, time exceeded, echo, ND (RS/RA/NS/NA), MLD |
| 9 | Campos | Type+Code+Checksum (pseudoheader) + body. (Detalle en F5) |
| 10 | Secuencia | Depende del tipo (NDP para autoconfiguración) |
| 11 | Addressing/naming | Direcciones IPv6 |
| 12 | Routing/forwarding | PMTUD via packet too big |
| 13 | Seguridad | Abuso NDP; fragmentación; RA guard; ver F6 |
| 14 | QoS/rendimiento | PMTUD sin routers |
| 15 | Observabilidad | Tipos visibles; filtros icmpv6.* |
| 16 | Interoperabilidad | Esencial IPv6 |
| 17 | Implementaciones | Pilas IPv6, ping6/traceroute6 |
| 18 | Fuentes | RFC 4443 (nivel 1); IANA R1 — 26-08-2026 |

## F-85 · FCoE — Fibre Channel over Ethernet

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | FCoE; FC-BB-5 (T11); ANSI/INCITS; familia STOR |
| 2 | Estado | Vigente con uso decreciente (26-08-2026; FC-BB-5; convergencia con iSCSI/NVMe-oF) |
| 3 | Finalidad | FC sobre Ethernet lossless (DCB) para SAN convergente. **No usar** sin red Ethernet lossless (PFC) |
| 4 | Encapsulación | Corre sobre **Ethernet** (EtherType **0x8906**) con VLAN (FCoE VLANs) |
| 5 | Capas | OSI 2/4 (encap FC); plano: datos |
| 6 | Transporte y direccionamiento | EtherType 0x8906; WWN; sin IP |
| 7 | PDU | **FCoE frame** (SOF + cabecera FCoE + FC frame + EOF) |
| 8 | Mensajes | Frames FC (FLOGI, datos FCP) encapsulados; FIP para descubrimiento |
| 9 | Campos | Ethernet/VLAN + FCoE header (version, SOF, FC header). (Detalle en F5) |
| 10 | Secuencia | FIP (discover/vlink) → FC login → datos |
| 11 | Addressing/naming | WWPN/WWNN, FCID |
| 12 | Routing/forwarding | Switches DCB |
| 13 | Seguridad | Igual que FC: zoning; ver F6 |
| 14 | QoS/rendimiento | Lossless PFC; MTU 2500 |
| 15 | Observabilidad | Frames FCoE/FIP visibles; filtros fcoe.* |
| 16 | Interoperabilidad | Convergente fabric |
| 17 | Implementaciones | Switches DCB, HBAs convergidas |
| 18 | Fuentes | FC-BB-5 (T11) — 26-08-2026 |

## F-86 · NVMe-oF — NVMe over Fabrics

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | NVMe-oF; NVM Express (NVMf, RDMA/FC/TCP); familia STOR |
| 2 | Estado | Vigente (26-08-2026; NVMe-oF 1.1; NVMe/TCP RFC-unlike spec) |
| 3 | Finalidad | Comandos NVMe sobre redes (RDMA, TCP, FC) para SAN de alta velocidad. **No usar** sin soporte NVMe |
| 4 | Encapsulación | NVMe/TCP corre sobre **TCP** (puerto **4420** registrado); también RDMA (RoCE/iWARP) |
| 5 | Capas | OSI 7 (aplicación/block sobre transporte); plano: datos |
| 6 | Transporte y direccionamiento | TCP 4420; NVMe queues |
| 7 | PDU | **PDU NVMe/TCP** (cabecera con tipo y data) |
| 8 | Mensajes | Comandos (READ/WRITE), ICReq/Resp (conexión), capsules |
| 9 | Campos | PDU header: type, flags, hlen, pdo, length, CID… (Detalle en F5) |
| 10 | Secuencia | TCP → ICReq/ICResp → comandos por queue → desconexión |
| 11 | Addressing/naming | NQN (NVMe Qualified Name); subsystem |
| 12 | Routing/forwarding | No |
| 13 | Seguridad | Sin cifrado nativo (TLS en NVMe/TCP en progreso); ver F6 |
| 14 | QoS/rendimiento | Ultra low latency; RDMA |
| 15 | Observabilidad | PDUs y CIDs visibles; filtros nvme-tcp.* |
| 16 | Interoperabilidad | Crecimiento en SAN moderna |
| 17 | Implementaciones | Linux nvme-tcp, SPDK, storage arrays |
| 18 | Fuentes | NVMe-oF spec (nivel 1); IANA R1 — 26-08-2026 |

## F-87 · 802.1X — IEEE 802.1X

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | 802.1X; IEEE 802.1X-2020; IEEE; familia SEG |
| 2 | Estado | Vigente (26-08-2026; 802.1X-2020) |
| 3 | Finalidad | Control de acceso al puerto (autenticación antes de datos) con EAP sobre LAN. **No usar** sin autenticador (switch/AP) y backend RADIUS |
| 4 | Encapsulación | Corre sobre **Ethernet** (EAPoL, EtherType **0x888E**) |
| 5 | Capas | OSI 2 (Enlace, acceso); plano: seguridad |
| 6 | Transporte y direccionamiento | EAPoL 0x888E; sin puertos IP |
| 7 | PDU | **EAPoL frame** (type: EAP-Packet, Start, Logoff, Key) |
| 8 | Mensajes | EAPoL-Start, EAP-Packet, EAPoL-Logoff, EAPoL-Key (4-way) |
| 9 | Campos | EAPoL header: ver(8)+type(8)+length(16)+payload EAP. (Detalle en F5) |
| 10 | Secuencia | Start → EAP identity → método (TLS) → success → 4-way handshake |
| 11 | Addressing/naming | Supplicant MAC; identities EAP |
| 12 | Routing/forwarding | No (puerto) |
| 13 | Seguridad | EAP-TLS fuerte; 802.1X-2020 defiende MKA; ver F6 |
| 14 | QoS/rendimiento | Reauth periods |
| 15 | Observabilidad | EAPoL y EAP types visibles; filtros eapol.* |
| 16 | Interoperabilidad | Estándar Wi-Fi/cable empresarial |
| 17 | Implementaciones | wpa_supplicant, hostapd, switches, FreeRADIUS |
| 18 | Fuentes | IEEE 802.1X (nivel 1) — 26-08-2026 |

## F-88 · EtherCAT — EtherCAT

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | EtherCAT; IEC 61158 (ETG); familia IOT |
| 2 | Estado | Vigente (26-08-2026; IEC 61158) |
| 3 | Finalidad | Automatización en tiempo real industrial (procesado en slave, "on the fly"). **No usar** fuera del ámbito de movimiento industrial |
| 4 | Encapsulación | Corre sobre **Ethernet** (EtherType **0x88A4**) |
| 5 | Capas | OSI 2 (+ procesado en acoplamiento); plano: datos |
| 6 | Transporte y direccionamiento | EtherType 0x88A4; datagramas con addresses |
| 7 | PDU | **Trama EtherCAT** (cabecera de datagramas, 2-12 datagramas) |
| 8 | Mensajes | Datagramas con comandos (NOP, APRD, APWR, BRD…) |
| 9 | Campos | Cabecera: length(11)+res(1)+type(4)+index(8)+address(32)+len+MR+IRQ. (Detalle en F5) |
| 10 | Secuencia | Procesado secuencial slave por slave; "on the fly" al final de la trama |
| 11 | Addressing/naming | Slave position/station address |
| 12 | Routing/forwarding | Topología en anillo/línea |
| 13 | Seguridad | Sin seguridad nativa; segmentar; ver F6 |
| 14 | QoS/rendimiento | Determinista; ciclo 100 µs típico |
| 15 | Observabilidad | Datagramas visibles; filtros ethercat.* |
| 16 | Interoperabilidad | ETG certificación |
| 17 | Implementaciones | TwinCAT (Beckhoff), stacks ecu (SSC) |
| 18 | Fuentes | IEC 61158 (nivel 1) — 26-08-2026 |

## F-89 · LoRaWAN — LoRaWAN

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | LoRaWAN; LoRa Alliance (spec 1.0.4/1.1); familia IOT |
| 2 | Estado | Vigente (26-08-2026; LoRaWAN 1.0.4) |
| 3 | Finalidad | LPWAN de largo alcance y bajo consumo para IoT (sensores). **No usar** para datos de alta tasa |
| 4 | Encapsulación | Corre sobre el enlace LoRa (radio sub-GHz); red de gateways → NS (UDP/IP) |
| 5 | Capas | OSI 1-2 (radio) + aplicación; plano: datos |
| 6 | Transporte y direccionamiento | DevEUI/DevAddr/AppKey; sin puertos IP en el dispositivo |
| 7 | PDU | **PHYPayload LoRaWAN** (MHDR + MACPayload + MIC) |
| 8 | Mensajes | Join-Request/Join-Accept, uplink/downlink (confirmed/unconfirmed) |
| 9 | Campos | MHDR(8)+DevAddr(32)+FCtrl+FPort+FrmPayload+MIC. (Detalle F5) |
| 10 | Secuencia | OTAA join → uplink → downlink; slots |
| 11 | Addressing/naming | DevAddr de 32 bits; AppKey |
| 12 | Routing/forwarding | Gateways → Network Server |
| 13 | Seguridad | AES-128 (NwkSKey/AppSKey), MIC; join; ver F6 |
| 14 | QoS/rendimiento | Data rate regional (EU868 da dracona); duty cycle |
| 15 | Observabilidad | Payloads en el NS; packets en gateway |
| 16 | Interoperabilidad | LoRaWAN Alliance certificación |
| 17 | Implementaciones | ChirpStack, The Things Network, LoRa basics (Semtech) |
| 18 | Fuentes | LoRaWAN spec (nivel 1) — 26-08-2026 |

## F-90 · Zigbee — Zigbee (IEEE 802.15.4)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Zigbee; IEEE 802.15.4 (radio) + Zigbee (stack); familia IOT |
| 2 | Estado | Vigente (26-08-2026; 802.15.4 + Zigbee 3.0) |
| 3 | Finalidad | Redes mesh de bajo consumo para domótica/IoT (dispositivos). **No usar** para aplicaciones de alta tasa |
| 4 | Encapsulación | Corre sobre **IEEE 802.15.4** (radio 2,4 GHz) |
| 5 | Capas | OSI 1-2 (802.15.4) + NWK/APS/ZDO; plano: datos |
| 6 | Transporte y direccionamiento | Direcciones de 16 bits (PAN); EUI-64 |
| 7 | PDU | **Trama 802.15.4** + NWK frame + APS payload |
| 8 | Mensajes | Beacon, Data, ACK, MAC command; APS data, ZDO discovery |
| 9 | Campos | 802.15.4 MAC header + NWK header (SDN, seq, dst/src). (Detalle en F5) |
| 10 | Secuencia | Unión a PAN → datos mesh → sleep/beacon |
| 11 | Addressing/naming | PAN ID + short addr; EUI-64 |
| 12 | Routing/forwarding | Mesh NWK con route discovery (AODV-like) |
| 13 | Seguridad | AES-CCM (network key), link keys; ver F6 |
| 14 | QoS/rendimiento | Bajo consumo; rates 250 kbps |
| 15 | Observabilidad | Tramas y redes visibles; filtros zigbee.* |
| 16 | Interoperabilidad | Zigbee 3.0 perfiles (HA, ZLL…) |
| 17 | Implementaciones | Stack de Nordic/TI (Z-Stack), zigbee2mqtt |
| 18 | Fuentes | IEEE 802.15.4 (nivel 1) — 26-08-2026 |

## F-91 · BACnet — BACnet

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | BACnet; ASHRAE 135 (ISO 16484-5); familia IOT |
| 2 | Estado | Vigente (26-08-2026; ASHRAE 135-2020) |
| 3 | Finalidad | Automatización de edificios (HVAC, iluminación, alarmas) con modelos de objetos. **No usar** fuera del ámbito de edificios |
| 4 | Encapsulación | Corre sobre **UDP** (puerto **47808** = 0xBAC0) y **IP** (BACnet/IP); también MSTP |
| 5 | Capas | OSI 7 (Aplicación edificios); plano: datos |
| 6 | Transporte y direccionamiento | UDP 47808; device instance + objects |
| 7 | PDU | **Mensaje BACnet** (APDU: BVLC/IP + NPDU + APDU) |
| 8 | Mensajes | ReadProperty, WriteProperty, Who-Is/I-Am, COV notifications |
| 9 | Campos | BVLC header (type 0x81, function, length…) + NPDU + APDU (service, tags). (Detalle en F5) |
| 10 | Secuencia | Who-Is → I-Am (discovery) → Read/Write → COV |
| 11 | Addressing/naming | Device instance; object IDs (point type) |
| 12 | Routing/forwarding | No (aplicación) |
| 13 | Seguridad | Sin cifrado por defecto (BACnet/SC añade TLS); ver F6 |
| 14 | QoS/rendimiento | Polling y COV |
| 15 | Observabilidad | Servicios y objetos visibles; filtros bacnet.* |
| 16 | Interoperabilidad | Estándar en BAS |
| 17 | Implementaciones | BACnet stacks (bacnet-stack), BAS controllers |
| 18 | Fuentes | ASHRAE 135 (nivel 1); IANA R1 — 26-08-2026 |

## F-92 · GSM — GSM (2G)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | GSM; 3GPP TS 45.x (radio), TS 44.x (L2/L3); 3GPP; familia RAD |
| 2 | Estado | Vigente en retirada (26-08-2026; plante de cierre en operadores) |
| 3 | Finalidad | Telefonía móvil 2G con conmutación de circuitos y GPRS/EDGE de datos. **No usar** en diseño nuevo |
| 4 | Encapsulación | Radio GSM (bandas 900/1800); A-bis/A-ter over TDM; GPRS sobre PCU |
| 5 | Capas | OSI 1-3 (radio + core) |
| 6 | Transporte y direccionamiento | IMSI, TMSI; canales |
| 7 | PDU | **Bloque radio (burst)**; LAPDm frames; MM/RR/RR messages |
| 8 | Mensajes | L3: CM Service Request, Call Control; radio: paging, handover |
| 9 | Campos | LAPDm: address+control+length; L3: protocol discriminator+ti+msg type. (Detalle F5) |
| 10 | Secuencia | Attach → location update → call setup (CC) |
| 11 | Addressing/naming | IMSI/MSISDN/TMSI; Cell ID |
| 12 | Routing/forwarding | MSC/VLR routing |
| 13 | Seguridad | A5/1-3 cifra radio (A5/1 roto); SIM (Ki); ver F6 |
| 14 | QoS/rendimiento | GPRS teórico 171 kbps/EDGE 384 |
| 15 | Observabilidad | Señalización L3 en abis/Um; IMSI catchers |
| 16 | Interoperabilidad | Global legacy |
| 17 | Implementaciones | BTS/BSC legacy, Osmocom (OpenBSC) |
| 18 | Fuentes | 3GPP TS 45.x/44.x — 26-08-2026 |

## F-93 · UMTS — UMTS (3G)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | UMTS (3G WCDMA); 3GPP TS 25.x; 3GPP; familia RAD |
| 2 | Estado | Sustituido en la práctica (26-08-2026; API menos; cierre progresivo) |
| 3 | Finalidad | Datos y voz móvil 3G (WCDMA/HSPA). **No usar** en diseño nuevo |
| 4 | Encapsulación | Radio WCDMA (UMTS); UTRAN; Iu/Iub |
| 5 | Capas | OSI 1-3 (radio); RLC/MAC/PDCP |
| 6 | Transporte y direccionamiento | IMSI/TMSI; RRC states |
| 7 | PDU | **PDU RLC/MAC-d**; RRC messages |
| 8 | Mensajes | RRC Connection Setup, RAB assignments; RLC AMD/UMD |
| 9 | Campos | RRC: protocol discriminator, msg type; RLC header (SN, LI). (Detalle F5) |
| 10 | Secuencia | RRC connection → RAB → datos |
| 11 | Addressing/naming | IMSI/TMSI; RNTI (C-RNTI) |
| 12 | Routing/forwarding | SRNC/DRNC |
| 13 | Seguridad | KASUMI (f8/f9); UEA/UIA; ver F6 |
| 14 | QoS/rendimiento | HSPA+ hasta 42 Mbps |
| 15 | Observabilidad | Señalización RRC en Iub/Iu |
| 16 | Interoperabilidad | Coexistió con GSM/LTE |
| 17 | Implementaciones | NodeB/RNC legacy, OpenAirInterface |
| 18 | Fuentes | 3GPP TS 25.x — 26-08-2026 |

## F-94 · LTE — LTE (E-UTRA, 4G)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | LTE; 3GPP TS 36.x (E-UTRA), TS 23.401; 3GPP; familia RAD |
| 2 | Estado | Vigente (26-08-2026; LTE/LTE-A; convive con 5G NR) |
| 3 | Finalidad | Datos móviles 4G (OFDMA/SC-FDMA), baja latencia, all-IP core (EPC). **No usar** cuando se requiere slicing ultra (5G) |
| 4 | Encapsulación | Radio LTE (OFDMA); S1/X2; GTP-U en core |
| 5 | Capas | OSI 1-3 (radio); PDCP/RLC/MAC; NAS (MME) |
| 6 | Transporte y direccionamiento | RNTIs; EPS bearers (QCI); IMSI/GUTI |
| 7 | PDU | **TB (transport block)**; RLC PDUs; NAS messages |
| 8 | Mensajes | RRC: RRCConnectionSetup; NAS: Attach, Bearer Setup |
| 9 | Campos | RLC/MAC headers; RRC/NAS msg types. (Detalle F5) |
| 10 | Secuencia | Attach (NAS) → default bearer → datos; handover X2 |
| 11 | Addressing/naming | GUTI/IMSI; RNTI |
| 12 | Routing/forwarding | eNB → S-GW → P-GW |
| 13 | Seguridad | AES (EEA2/128), SNOW3D; IAS; ver F6 |
| 14 | QoS/rendimiento | Cat 12+ varias cientos de Mbps; latencia ~20-30 ms |
| 15 | Observabilidad | Señalización S1 (S1AP) y NAS |
| 16 | Interoperabilidad | Global |
| 17 | Implementaciones | srsRAN, Open5GS, eNodeB commercial |
| 18 | Fuentes | 3GPP TS 36.x — 26-08-2026 |

## F-95 · 5G NR — 5G NR

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | 5G NR; 3GPP TS 38.x (Release 15-18); 3GPP; familia RAD |
| 2 | Estado | Vigente (26-08-2026; Rel-17/18; SA/NSA) |
| 3 | Finalidad | Banda ancha móvil 5G, URLLC, mMTC con slicing y service-based core (5GC). **No usar** cuando 4G basta para el caso |
| 4 | Encapsulación | Radio NR (FR1/FR2, mmWave); NG/F1/Xn; GTP |
| 5 | Capas | OSI 1-3 (NR); SDAP/PDCP/RLC/MAC; NAS (AMF) |
| 6 | Transporte y direccionamiento | RNTIs; QoS flows/DRBs; IMSI/SUCI |
| 7 | PDU | **TB NR**; RLC PDUs; NAS messages (MM/CM) |
| 8 | Mensajes | RRCSetup, RRC Reconfig; NAS Registration |
| 9 | Campos | RLC/MAC headers; RRC/NAS types. (Detalle F5) |
| 10 | Secuencia | Registration (NAS) → PDU session → data |
| 11 | Addressing/naming | SUCI/GUTI; RNTI |
| 12 | Routing/forwarding | gNB → UPF (N3) |
| 13 | Seguridad | AES (NEA2/128), ZUC (NEA3) uso en 5G; SUPI protection; ver F6 |
| 14 | QoS/rendimiento | Multi-Gbps; latencia <10 ms; slicing |
| 15 | Observabilidad | Señalización NGAP/NAS |
| 16 | Interoperabilidad | Global |
| 17 | Implementaciones | srsRAN, Open5GS, OpenAirInterface, vendors |
| 18 | Fuentes | 3GPP TS 38.x — 26-08-2026 |

## F-96 · TETRA — TETRA (ETSI)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | TETRA; ETSI EN 300 392; ETSI; familia RAD |
| 2 | Estado | Vigente (26-08-2026; EN 300 392; radio profesional) |
| 3 | Finalidad | Radio profesional P2MP con llamadas directas/modo trunking y seguridad. **No usar** para datos de alta tasa |
| 4 | Encapsulación | Radio TETRA (300 MHz band); DMO/TMO |
| 5 | Capas | OSI 1-3 (radio); MAC/LLC; MLE |
| 6 | Transporte y direccionamiento | TETRA address (0xFF…) + GSSI |
| 7 | PDU | **PDU TETRA** (LLC + SNDCP/MLE) |
| 8 | Mensajes | SETUP, DISCONNECT (call control), registration (MM) |
| 9 | Campos | LLC header + PDU type. (Detalle F5) |
| 10 | Secuencia | Registration → call setup → traffic |
| 11 | Addressing/naming | ITSI (0-16M), GSSI (grupo) |
| 12 | Routing/forwarding | DMO repeaters; TMO switches |
| 13 | Seguridad | Air-interface encryption (TEA1-3), end-to-end opcional; ver F6 |
| 14 | QoS/rendimiento | Llamada rápida (<0,5 s), altas tasas en trunking |
| 15 | Observabilidad | Señalización visible con tools TETRA |
| 16 | Interoperabilidad | Estandarizada ETSI |
| 17 | Implementaciones | Motorola, Sepura, Hytera; research (gr-tetra) |
| 18 | Fuentes | ETSI EN 300 392 — 26-08-2026 |

## F-97 · DMR — DMR (Digital Mobile Radio)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | DMR; ETSI TS 102 361; ETSI; familia RAD |
| 2 | Estado | Vigente (26-08-2026; TS 102 361) |
| 3 | Finalidad | Radio digital profesional (trunking TIER III, sin licencia motivos) de bajo costo. **No usar** para alta seguridad gubernamental (TETRA) |
| 4 | Encapsulación | Radio DMR (VHF/UHF), TDMA 2 ranuras |
| 5 | Capas | OSI 1-3 (radio) |
| 6 | Transporte y direccionamiento | ID de radio (24 bits) + color code |
| 7 | PDU | **Burst DMR** (108 bits) con voice/data |
| 8 | Mensajes | Registration, call; CSBK, LC |
| 9 | Campos | Sync + EMB + payload (voice codewords). (Detalle F5) |
| 10 | Secuencia | Slot → voz codificada (AMBE) o datos |
| 11 | Addressing/naming | Radio ID; talkgroups |
| 12 | Routing/forwarding | Repeaters TDMA |
| 13 | Seguridad | Sin cifrado estándar fuerte (ARCS/BP opcional); ver F6 |
| 14 | QoS/rendimiento | TDMA 2× en 12,5 kHz |
| 15 | Observabilidad | Bursts visibles con SDR |
| 16 | Interoperabilidad | ETSI; marcas compatibles |
| 17 | Implementaciones | Mototrbo, Hytera, codeplug tools |
| 18 | Fuentes | ETSI TS 102 361 — 26-08-2026 |

## F-98 · Link 16 — Link 16 (TADIL-J)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Link 16; MIL-STD-6016 (TADIL-J); NATO STANAG 5516; familia RAD |
| 2 | Estado | military_public (26-08-2026; MIL-STD-6016) |
| 3 | Finalidad | Intercambio táctico de datos militares (voz/datos) con FHSS y TDMA. **No usar** fuera de entorno de defensa |
| 4 | Encapsulación | Radio en banda L (960-1215 MHz), FHSS 51 hops, TDMA slots |
| 5 | Capas | OSI 1-3 (radio táctica) |
| 6 | Transporte y direccionamiento | Time slots (NPG), net numbers; JU IDs |
| 7 | PDU | **Símbolos Link 16** (tiempos, frecuencias) |
| 8 | Mensajes | J-series: J2.0/J2.2 (posiciones de pista…), J3.0… |
| 9 | Campos | Palabras de mensaje (J-series units). (Detalle F5: restringido) |
| 10 | Secuencia | Net entry (PPLI) → intercambio de pistas |
| 11 | Addressing/naming | JU (Participating Unit) numbers |
| 12 | Routing/forwarding | Relay en slots |
| 13 | Seguridad | Cryptography (TRANSEC/COMSEc - KGV-8); ver F6 |
| 14 | QoS/rendimiento | Alta latencia por slots; robusto anti-jam |
| 15 | Observabilidad | Documentación pública parcial |
| 16 | Interoperabilidad | NATO |
| 17 | Implementaciones | Equipos militares; research (gr-link16) |
| 18 | Fuentes | MIL-STD-6016 (nivel 1, público parcial) — 26-08-2026 |

## F-99 · Link 11 — Link 11 (TADIL-A)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Link 11; MIL-STD-6011 (TADIL-A); NATO STANAG 5511; familia RAD |
| 2 | Estado | Histórico operativo (26-08-2026; MIL-STD-6011; transición a Link 16) |
| 3 | Finalidad | Intercambio táctico de datos (HF/UHF, simplex) consola de comando. **No usar** en diseño nuevo |
| 4 | Encapsulación | HF/UHF radio; loops (canal serial) |
| 5 | Capas | OSI 1-3 (radio táctica) |
| 6 | Transporte y direccionamiento | Net control station; address 1-128 |
| 7 | PDU | **Trama Link 11** (preámbulo + msg + códigos) |
| 8 | Mensajes | M-series (M1-M24; data words) |
| 9 | Campos | Data words. (Detalle F5: restringido) |
| 10 | Secuencia | Roll call: NCS llama → unidades responden |
| 11 | Addressing/naming | Direcciones de unidad |
| 12 | Routing/forwarding | Net management |
| 13 | Seguridad | Cripto opcional; ver F6 |
| 14 | QoS/rendimiento | Bajo throughput (HF) |
| 15 | Observabilidad | Documentación pública parcial |
| 16 | Interoperabilidad | NATO legacy |
| 17 | Implementaciones | Equipos militares legacy |
| 18 | Fuentes | MIL-STD-6011 (nivel 1, público parcial) — 26-08-2026 |

## F-100 · ITS-G5 — ITS-G5 (ETSI; radio vehicular)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | ITS-G5; ETSI EN 303 613 / IEEE 802.11p/802.11bd; ETSI; familia RAD |
| 2 | Estado | Vigente (26-08-2026; incorporado en F3 v2); compite con C-V2X |
| 3 | Finalidad | Comunicaciones vehiculares V2X (seguridad, platooning) basadas en Wi-Fi dedicado (5,9 GHz, no conectado). **No usar** en corredores administrados por operador sin decisión de estándar |
| 4 | Encapsulación | Radio 802.11p (OCB mode) en banda 5,9 GHz; GeoNetworking/DCC |
| 5 | Capas | OSI 1-2 (radio) + GeoNetworking + BTP + CAM/DENM |
| 6 | Transporte y direccionamiento | MAC + GeoNet addressing (station) |
| 7 | PDU | **Paquete ITS-G5** (GeoNetwork header + BTP + CAM) |
| 8 | Mensajes | CAM (cooperative awareness), DENM (danger), SPAT/MAP (routing de semáforos) |
| 9 | Campos | GeoNetworking header (type, hop limit, position). (Detalle F5) |
| 10 | Secuencia | Beacons CAM periódicos; DENM bajo evento |
| 11 | Addressing/naming | Station ID; geohash positioning |
| 12 | Routing/forwarding | GeoNetworking (geocast) |
| 13 | Seguridad | Certificados PKI (IEEE 1609.2); ver F6 |
| 14 | QoS/rendimiento | Baja latencia; rango ~300 m |
| 15 | Observabilidad | Mensajes ITS visibles; filtros its.* |
| 16 | Interoperabilidad | ETSI C-ITS vs 3GPP C-V2X (decisión de mercado) |
| 17 | Implementaciones | Cohda, NXP, chipsets 802.11p |
| 18 | Fuentes | ETSI EN 303 613 / IEEE 802.11p — 26-08-2026 |

## F-101 · C-V2X — C-V2X (3GPP; LTE-V2X/NR-V2X)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | C-V2X; 3GPP TS 23.285 / TS 36.xxx (LTE-V2X PC5), NR-V2X (TS 38.xxx); 3GPP; familia RAD |
| 2 | Estado | Vigente (26-08-2026; incorporado en F3 v2) |
| 3 | Finalidad | Comunicaciones vehiculares V2V/V2I/V2N sobre espectro celular (PC5 directo y Uu). **No usar** cuando la política regional opta por ITS-G5 |
| 4 | Encapsulación | Radio LTE/NR en PC5 (sidelink) y Uu (uplink); V2X layer sobre PDCP |
| 5 | Capas | OSI 1-3 (radio) + V2X layer + CAM/DENM/BTP-like |
| 6 | Transporte y direccionamiento | L2 (PC5) destination ID; UE ID |
| 7 | PDU | **PDU V2X** (SDAP/PDCP + messages CAM/DENM/BSP) |
| 8 | Mensajes | CAM/DENM; BSM (US); SPAT/MAP |
| 9 | Campos | Header V2X + IP/UDP (u) + payload CAM. (Detalle F5) |
| 10 | Secuencia | PC5 directo para baja latencia; Uu con red |
| 11 | Addressing/naming | Destination Layer-2 ID |
| 12 | Routing/forwarding | Sidelink ressource pool |
| 13 | Seguridad | PKI (1609.2-like; 3GPP TS communicate), por red; ver F6 |
| 14 | QoS/rendimiento | Baja latencia (<100 ms crítico); rango mayor que ITS-G5 |
| 15 | Observabilidad | Señalización PC5/Uu |
| 16 | Interoperabilidad | Decide en EU vs ITS-G5; US plensa 5,9 GHz |
| 17 | Implementaciones | Qualcomm 9150 (modem), 5GAA |
| 18 | Fuentes | 3GPP TS 23.285 (nivel 1) — 26-08-2026 |

## F-102 · X.25 — X.25

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | X.25; ITU-T X.25; familia HIST |
| 2 | Estado | Histórico (26-08-2026; retirado de redes públicas) |
| 3 | Finalidad | Conmutación de paquetes con circuitos virtuales en redes legadas (PDN). **No usar** en diseño nuevo |
| 4 | Encapsulación | Sobre enlaces serie (LAPB); red X.25 |
| 5 | Capas | OSI 1-3 (con conmutación de paquetes) |
| 6 | Transporte y direccionamiento | ICI/RCI (call id); X.121 |
| 7 | PDU | **Paquete X.25** (LAPB frames + PLP) |
| 8 | Mensajes | Call Request/Connected; DATA, RR/RNR |
| 9 | Campos | LAPB + PLP header (GFI-LCI-P(S)/P(R)). (Detalle F5) |
| 10 | Secuencia | Call setup → datos por VC → clear |
| 11 | Addressing/naming | X.121 |
| 12 | Routing/forwarding | DCE routing |
| 13 | Seguridad | Ninguna moderna; ver F6 |
| 14 | QoS/rendimiento | Bajo throughput (~64 kbps) |
| 15 | Observabilidad | Frames X.25 con analyzers legacy |
| 16 | Interoperabilidad | Histórica |
| 17 | Implementaciones | Retiradas (rareza) |
| 18 | Fuentes | ITU-T X.25 — 26-08-2026 |

## F-103 · FR — Frame Relay

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Frame Relay; ITU-T Q.922 (ANSI T1.618); familia HIST |
| 2 | Estado | Histórico (26-08-2026; sustituido por IP/MPLS) |
| 3 | Finalidad | Transporte L2 con PVC/SVC sobre redes de carrier. **No usar** en diseño nuevo |
| 4 | Encapsulación | Sobre enlace (T1/E1); DLCI |
| 5 | Capas | OSI 2 (Enlace) |
| 6 | Transporte y direccionamiento | DLCI (10 bits); sin puertos |
| 7 | PDU | **Trama Frame Relay** (flag + header + datos + FCS) con Q.922 |
| 8 | Mensajes | Tramas de datos; LMI (status enquiry) |
| 9 | Campos | Header FRLI: EA+C/R+DE+FECN+BECN+DLCI. (Detalle F5) |
| 10 | Secuencia | PVC establecida → datos; LMI stato |
| 11 | Addressing/naming | DLCI |
| 12 | Routing/forwarding | PVC switch |
| 13 | Seguridad | Sin cifrado; ver F6 |
| 14 | QoS/rendimiento | CIR/Bc; bajo CWDM |
| 15 | Observabilidad | Frames con analyzers legacy |
| 16 | Interoperabilidad | Histórica |
| 17 | Implementaciones | Chassis legacy |
| 18 | Fuentes | ITU-T Q.922 — 26-08-2026 |

## F-104 · ATM — Asynchronous Transfer Mode

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | ATM; ITU-T I.150/I.361; familia HIST |
| 2 | Estado | Histórico (26-08-2026; celdas 53 B; sustituido por IP/Ethernet) |
| 3 | Finalidad | Multiplexación de tráfico en celdas de 53 bytes con QoS (CBR/VBR). **No usar** en diseño nuevo |
| 4 | Encapsulación | Sobre fibra/físico (SONET/SDH); PVC/SVC |
| 5 | Capas | OSI 2.5 (AAL + ATM layer) |
| 6 | Transporte y direccionamiento | VPI/VCI; AAL5 para IP |
| 7 | PDU | **Celda ATM** (5 B cabecera + 48 B payload) |
| 8 | Mensajes | Celdas UNI/NNI |
| 9 | Campos | Cabecera: GFC/VPI(12-16)+VCI(16)+PT+CLP+HEC. (Detalle F5) |
| 10 | Secuencia | PVC → celdas AAL5 (SAR) |
| 11 | Addressing/naming | VPI/VCI, NSAP |
| 12 | Routing/forwarding | Switch fabric (PNNI) |
| 13 | Seguridad | Sin cifrado; ver F6 |
| 14 | QoS/rendimiento | QoS fuerte, bajo overhead en voz |
| 15 | Observabilidad | Celdas con analyzers legacy |
| 16 | Interoperabilidad | Histórica (ADSL legacy) |
| 17 | Implementaciones | DSLAM legacy |
| 18 | Fuentes | ITU-T I.361 — 26-08-2026 |

## F-105 · Token Ring — Token Ring (IEEE 802.5)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | Token Ring; IEEE 802.5; familia HIST |
| 2 | Estado | Histórico (26-08-2026; retirado) |
| 3 | Finalidad | LAN con paso de testigo (token) para evitar colisiones. **No usar** |
| 4 | Encapsulación | Sobre el medio (anillo físico) |
| 5 | Capas | OSI 1-2 |
| 6 | Transporte y direccionamiento | MAC de 6 B; token |
| 7 | PDU | **Trama 802.5** + token (AC + FC + DA/SA/RIF + payload) |
| 8 | Mensajes | Token y tramas; beaconing |
| 9 | Campos | AC(8)+FC(8)+DA(48)+SA(48+RIF)+LLC+FCS. (Detalle F5) |
| 10 | Secuencia | Captura del token → transmisión → liberación |
| 11 | Addressing/naming | MAC; RIF para bridges |
| 12 | Routing/forwarding | Source routing bridges |
| 13 | Seguridad | Ninguna; ver F6 |
| 14 | QoS/rendimiento | Determinista a baja carga |
| 15 | Observabilidad | Tramas legacy |
| 16 | Interoperabilidad | Histórica (IBM) |
| 17 | Implementaciones | NICs legacy |
| 18 | Fuentes | IEEE 802.5 — 26-08-2026 |

## F-106 · FDDI — Fiber Distributed Data Interface

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | FDDI; ANSI X3.166 (ISO 9314); familia HIST |
| 2 | Estado | Histórico (26-08-2026; retirado) |
| 3 | Finalidad | LAN de fibra con anillo dual (100 Mbps). **No usar** |
| 4 | Encapsulación | Fibra (anillo dual); token passing |
| 5 | Capas | OSI 1-2 (FDDI) |
| 6 | Transporte y direccionamiento | MAC 6 B; token timers |
| 7 | PDU | **Trama FDDI** (FDDI header + LLC) |
| 8 | Mensajes | Token y tramas; SMT (station management) |
| 9 | Campos | PA+SD+FC+DA+SA+LLC+FCS+ED/FS. (Detalle F5) |
| 10 | Secuencia | Token → trama → reclaim |
| 11 | Addressing/naming | MAC |
| 12 | Routing/forwarding | Anillo dual; wrap en fallos |
| 13 | Seguridad | Ninguna; ver F6 |
| 14 | QoS/rendimiento | TTRT para QoS |
| 15 | Observabilidad | Legacy |
| 16 | Interoperabilidad | Histórica |
| 17 | Implementaciones | NICs legacy |
| 18 | Fuentes | ANSI X3.166 — 26-08-2026 |

## F-107 · NetBEUI — NetBEUI

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | NetBEUI; protocolo de Microsoft (NetBIOS frames); familia HIST |
| 2 | Estado | Histórico (26-08-2026; no enrutable) |
| 3 | Finalidad | Transporte sencillo no enrutable para LANs Windows legacy. **No usar** |
| 4 | Encapsulación | Directo sobre enlace (LLC 802.2) |
| 5 | Capas | OSI 3-4 (protocolo simple) |
| 6 | Transporte y direccionamiento | Session numbers |
| 7 | PDU | **NBF frame** (tipo + ID) |
| 8 | Mensajes | Session control (SSP), data |
| 9 | Campos | Header NBF: tipo(4)+prioridad+respuesta. (Detalle F5) |
| 10 | Secuencia | Session establish → data |
| 11 | Addressing/naming | NetBIOS names |
| 12 | Routing/forwarding | No enrutable |
| 13 | Seguridad | Ninguna; ver F6 |
| 14 | QoS/rendimiento | Broadcast alto |
| 15 | Observabilidad | Legacy |
| 16 | Interoperabilidad | Histórica |
| 17 | Implementaciones | Windows legacy |
| 18 | Fuentes | Microsoft NetBEUI (doc legacy) — 26-08-2026 |

## F-108 · IPX/SPX — IPX/SPX

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | IPX/SPX; protocolos Novell (IPX, SPX, NCP); familia HIST |
| 2 | Estado | Histórico (26-08-2026; NetWare legacy) |
| 3 | Finalidad | Pila de red de NetWare (IPX, SPX orientado a conexión, NCP). **No usar** |
| 4 | Encapsulación | Sobre enlace (Ethernet 802.3/802.2, LLC) |
| 5 | Capas | OSI 3-4 (IPX/SPX) |
| 6 | Transporte y direccionamiento | Network+node+socket (IPX) |
| 7 | PDU | **Paquete IPX** (cabecera 30 B) / SPX + data |
| 8 | Mensajes | IPX datagramas; SPX connection mgmt; NCP |
| 9 | Campos | IPX: checksum(16)+length+transport control+packet type+net/node/socket. (Detalle F5) |
| 10 | Secuencia | IPX sin conexión; SPX sureste seq |
| 11 | Addressing/naming | Net.node.socket |
| 12 | Routing/forwarding | RIPX/NLSP routing |
| 13 | Seguridad | Ninguna moderna; ver F6 |
| 14 | QoS/rendimiento | Legacy |
| 15 | Observabilidad | Legacy |
| 16 | Interoperabilidad | Histórica (NetWare) |
| 17 | Implementaciones | Novell stack legacy |
| 18 | Fuentes | Novell IPX/SPX — 26-08-2026 |

## F-109 · AppleTalk — AppleTalk

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | AppleTalk; protocolos Apple (DDP, ATP, NBP); familia HIST |
| 2 | Estado | Histórico (26-08-2026; retirado) |
| 3 | Finalidad | Pila de red plug-and-play de Apple (pre-TCP/IP). **No usar** |
| 4 | Encapsulación | LLAP sobre enlace; ELAP sobre Ethernet |
| 5 | Capas | OSI 3-5 (DDP, ATP, NBP) |
| 6 | Transporte y direccionamiento | Network.node.socket |
| 7 | PDU | **Datagrama DDP** sobre LLAP/ELAP |
| 8 | Mensajes | DDP; NBP (name binding), ATP |
| 9 | Campos | LLAP: dst/src node+type; DDP header. (Detalle F5) |
| 10 | Secuencia | AARP → zone/name → sessions |
| 11 | Addressing/naming | AppleTalk addresses; zones |
| 12 | Routing/forwarding | RTMP routers |
| 13 | Seguridad | Ninguna; ver F6 |
| 14 | QoS/rendimiento | Little |
| 15 | Observabilidad | Legacy |
| 16 | Interoperabilidad | Histórica |
| 17 | Implementaciones | Mac classic |
| 18 | Fuentes | AppleTalk (apple spec legacy) — 26-08-2026 |

## F-110 · ARCNET — ARCNET

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | ARCNET; ANSI/ATA 878.1; familia HIST |
| 2 | Estado | Histórico (26-08-2026; uso residual industrial) |
| 3 | Finalidad | LAN token-bus determinista (2,5 Mbps). **No usar** |
| 4 | Encapsulación | Coaxial/Hub; token passing |
| 5 | Capas | OSI 1-2 |
| 6 | Transporte y direccionamiento | Node ID (8 bits) |
| 7 | PDU | **Paquete ARCNET** (header + data) |
| 8 | Mensajes | ITT (token), FBE, PAC, data |
| 9 | Campos | Header: dst/src/len/CRC. (Detalle F5) |
| 10 | Secuencia | Token → transmisión por ID |
| 11 | Addressing/naming | Node ID |
| 12 | Routing/forwarding | Hub/red token |
| 13 | Seguridad | Ninguna; ver F6 |
| 14 | QoS/rendimiento | Determinista |
| 15 | Observabilidad | Legacy |
| 16 | Interoperabilidad | Residual |
| 17 | Implementaciones | Controllers legacy |
| 18 | Fuentes | ANSI/ATA 878.1 — 26-08-2026 |

## F-111 · SONET/SDH — SONET/SDH

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | SONET/SDH; ITU-T G.707/SONET GR-253; familia HIST |
| 2 | Estado | Sustituido en transición (26-08-2026; nuevos deployments rare) |
| 3 | Finalidad | Transporte óptico sincronizado (jerarquías STS/STM) para voz/datos de carrier. **No usar** en diseño nuevo (OTN/100G) |
| 4 | Encapsulación | Fibra (OC-n/STM-n); frame 125 µs |
| 5 | Capas | OSI 1 (Física/transport) |
| 6 | Transporte y direccionamiento | STS-1/3-n paths; TUG/TU |
| 7 | PDU | **Frame SONET** (synchronous payload envelope) |
| 8 | Mensajes | Overheads (SOH, pointers) |
| 9 | Campos | Transport overhead + payload (SPE). (Detalle F5) |
| 10 | Secuencia | Sincronización de reloj → multiplexación |
| 11 | Addressing/naming | STS paths; DCC |
| 12 | Routing/forwarding | Add/drop multiplexing |
| 13 | Seguridad | Sin cifrado; ver F6 |
| 14 | QoS/rendimiento | Garantías de timing |
| 15 | Observabilidad | Overheads y errores |
| 16 | Interoperabilidad | Carrier legacy |
| 17 | Implementaciones | ADMs (legacy) |
| 18 | Fuentes | ITU-T G.707 — 26-08-2026 |

## F-112 · ISDN — ISDN (Q.931)

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | ISDN; ITU-T Q.931 (señalización) y I.430/I.431; familia HIST |
| 2 | Estado | Histórico (26-08-2026; retirado en muchas regiones) |
| 3 | Finalidad | Servicios integrados voz/datos digitales (BRI/PRI). **No usar** |
| 4 | Encapsulación | BRI (2B+D) / PRI (30B+D) sobre pares/fibra |
| 5 | Capas | OSI 1-3 (señalización D-channel) |
| 6 | Transporte y direccionamiento | TEI/SAPI; números E.164 |
| 7 | PDU | **Mensaje LAPD + Q.931** |
| 8 | Mensajes | SETUP, CONNECT, RELEASE; LAPD I-frames |
| 9 | Campos | Q.931: protocol discriminator+call ref+msg type+IE. (Detalle F5) |
| 10 | Secuencia | SETUP → CONNECT → datos → RELEASE |
| 11 | Addressing/naming | E.164; TEI |
| 12 | Routing/forwarding | D-channel ruteo |
| 13 | Seguridad | Ninguna; ver F6 |
| 14 | QoS/rendimiento | Digital determinista |
| 15 | Observabilidad | Q.931 con analyzers legacy |
| 16 | Interoperabilidad | Histórica |
| 17 | Implementaciones | NT/TA legacy |
| 18 | Fuentes | ITU-T Q.931 — 26-08-2026 |

## F-113 · EIGRP — Enhanced Interior Gateway Routing Protocol

| # | Campo | Valor |
|---|---|---|
| 1 | Identidad | EIGRP; protocolo propietario Cisco (RFC 7868 lo documenta); familia ROUT |
| 2 | Estado | Propietario (26-08-2026; RFC 7868 con informativo; soporte limitado) |
| 3 | Finalidad | IGP híbrido (distancia+estado) de Cisco con convergencia por DUAL. **No usar** en entornos no Cisco o multi-vendor sin rediseño |
| 4 | Encapsulación | Corre sobre **IP** (protocol **88**); puede correr sobre RTP (reliable transport) |
| 5 | Capas | OSI 3 (Red); plano: control |
| 6 | Transporte y direccionamiento | IP protocol 88; multicast 224.0.0.10 (Hellos); AS number |
| 7 | PDU | **Paquete EIGRP** (cabecera + TLVs) |
| 8 | Mensajes | Hello/ACK, Update, Query, Reply, SIA-Query/Reply |
| 9 | Campos | Cabecera: version(8)+opcode(8)+checksum(16)+flags(32)+seq(32)+ack(32+ASN(32). (Detalle en F5) |
| 10 | Secuencia | Inicio con reliable transport (RTP) → Updates → DUAL; queries para caminos alternos |
| 11 | Addressing/naming | Feasible successors; topology table |
| 12 | Routing/forwarding | DUAL: succesor/feasible sucessor; redistribución |
| 13 | Seguridad | Autenticación MD5 (RFC 7868); sin cifrado; ver F6 |
| 14 | QoS/rendimiento | Convergencia rápida; métrica compleja (bandwidth/delay/load/reliability) |
| 15 | Observabilidad | Paquetes y TLVs visibles; filtros eigrp.* |
| 16 | Interoperabilidad | Solo Cisco (RFC informativo) |
| 17 | Implementaciones | Cisco IOS/IOS-XR, FRRouting (EIGRP experimental) |
| 18 | Fuentes | RFC 7868 (nivel 1, informativo) — 26-08-2026 |

---

## Estado de cobertura (F4 ampliado)

- ✅ Fichas completas: **113/113** — catálogo F3 cubierto al **100 %**.
- Orden de cobertura: 12 iniciales → 17 (lote 1) → 29 (L2) → 45 (L3) → 69 (L4) → 113 (L5, finales: L2TP, CSMA/CD, LLMNR, NetBIOS, RIPv2, SR, MIP, MIPv6, LISP, RESTCONF, gRPC, IPFIX, NetFlow, ICMP, ICMPv6, FCoE, NVMe-oF, 802.1X, EtherCAT, LoRaWAN, Zigbee, BACnet, GSM, UMTS, LTE, 5G NR, TETRA, DMR, Link 16, Link 11, ITS-G5, C-V2X, X.25, FR, ATM, Token Ring, FDDI, NetBEUI, IPX/SPX, AppleTalk, ARCNET, SONET/SDH, ISDN, EIGRP).
- Fuentes: nivel 1 en campos críticos; fecha de consulta 26-08-2026 registrada por ficha.

---
Última actualización: 26-08-2026