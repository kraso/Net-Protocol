# PLANREDES — Plan Maestro de Investigación, Documentación y Desarrollo

## Plataforma de referencia, exploración y simulación de redes (aplicación de escritorio)

| Campo | Valor |
|---|---|
| **Documento** | PLANREDES.md — Plan maestro del proyecto |
| **Versión** | 1.0 (borrador para validación) |
| **Fecha** | 26 de agosto de 2026 |
| **Documento de origen** | `Prompt_Maestro_Aplicacion_Redes_Investigacion_y_Tecnologia.docx` (v. referencia ago-2026) |
| **Estado** | En revisión — los registros vivos citados son datos versionables y pueden cambiar |
| **Fase dentro del proyecto** | Fase de planificación: investigación y documentación (F0–F9) → después, plan de software |

> **Nota metodológica:** La fecha de referencia del documento maestro es el 26 de agosto de 2026. Los registros vivos (IANA, RFC, MIL-STD, etc.) deben tratarse como datos versionables: sus estados, fechas, números de puerto, versiones y disponibilidad pública pueden cambiar. Este plan incorpora esa exigencia como requisito de diseño.

---

## Índice

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [Principios y límites del proyecto](#2-principios-y-límites-del-proyecto)
3. [Mapa completo de dominios de investigación](#3-mapa-completo-de-dominios-de-investigación)
4. [Registro de organizaciones, autoridades y fuentes maestras](#4-registro-de-organizaciones-autoridades-y-fuentes-maestras)
5. [Taxonomía propuesta](#5-taxonomía-propuesta)
6. [Esquema de datos y plantillas de fichas](#6-esquema-de-datos-y-plantillas-de-fichas)
7. [Estrategia para aproximarse a la exhaustividad](#7-estrategia-para-aproximarse-a-la-exhaustividad)
8. [Plan de investigación fase a fase (F0–F9)](#8-plan-de-investigación-fase-a-fase-f0f9)
9. [Matriz de calidad, evidencia y trazabilidad](#9-matriz-de-calidad-evidencia-y-trazabilidad)
10. [Política para protocolos históricos, propietarios, experimentales y militares/públicos](#10-política-para-protocolos-históricos-propietarios-experimentales-y-militarespúblicos)
11. [Requisitos de diagramación y representación de mensajes](#11-requisitos-de-diagramación-y-representación-de-mensajes)
12. [Matriz comparativa de tecnologías](#12-matriz-comparativa-de-tecnologías)
13. [Recomendación tecnológica argumentada y arquitectura](#13-recomendación-tecnológica-argumentada-y-arquitectura)
14. [Hoja de ruta preliminar de desarrollo (a cerrar tras F8/F9)](#14-hoja-de-ruta-preliminar-de-desarrollo-a-cerrar-tras-f8f9)
15. [Riesgos, lagunas y decisiones que aún no deben cerrarse](#15-riesgos-lagunas-y-decisiones-que-aún-no-deben-cerrarse)
16. [Criterios de aceptación del plan](#16-criterios-de-aceptación-del-plan)
17. [Prompt maestro de diseño y generación de software (fase siguiente)](#17-prompt-maestro-de-diseño-y-generación-de-software-fase-siguiente)
18. [Fuentes de referencia verificadas](#18-fuentes-de-referencia-verificadas)
19. [Apéndice A — Reglas de conducta del modelo de investigación](#19-apéndice-a--reglas-de-conducta-del-modelo-de-investigación)
20. [Apéndice B — Glosario de unidades de datos (PDU)](#20-apéndice-b--glosario-de-unidades-de-datos-pdu)

---

## 1. Resumen ejecutivo

**Qué se va a construir.** Una aplicación de escritorio profesional, multiplataforma y de uso principalmente **local** (local-first, sin depender de Internet para consultar el conocimiento instalado) que actúe como **plataforma de conocimiento, exploración y representación técnica de redes de comunicaciones**.

**Qué cubre.** Documentará: dispositivos de red, tipos y arquitecturas de redes, capas y planos funcionales, protocolos, estándares, mensajes, PDU, formatos de datos, mecanismos de encapsulación, secuencias de comunicación, seguridad, observabilidad e interoperabilidad.

**Cómo se afronta (dos fases).** Siguiendo el mandato del documento maestro, **no se programa ni se diseñan las pantallas finales hasta que el inventario de conocimiento esté cerrado y validado**:

1. **Fase I — Plan de investigación y documentación (este documento):** construir la base de conocimiento rigurosa, trazable, versionada y extensible mediante las fases F0–F9.
2. **Fase II — Plan de diseño y generación de software:** solo después de completar y validar la Fase I, se elabora el plan de arquitectura, UX/UI, implementación, pruebas y distribución (para lo que se incluye un *prompt maestro* listo para reutilizar en la sección 17).

**Decisión tecnológica de partida.** Arquitectura de escritorio **C#/.NET + Avalonia** (UI XAML/MVVM multiplataforma), **SQLite + FTS5** como almacén local y búsqueda, **diagramas generables de forma determinista desde datos estructurados** (SVG como formato vectorial de intercambio), un **pipeline de importación/normalización** de registros oficiales (p. ej. IANA) que reconstruye el índice sin edición manual de cientos de fichas, e integración opcional con **PCAP/PCAPNG** tomando la disección por capas de Wireshark como referencia conceptual.

**Entregable inmediato.** Este plan, junto con los catálogos, esquemas de datos y plantillas de fichas que se especifican en las secciones 6 y 8, debe ser suficientemente preciso para que otro equipo implemente la aplicación **sin volver a descubrir qué investigar, qué fuentes consultar, qué entidades almacenar, qué criterios de calidad aplicar, qué diagramas construir y cómo decidir si una ficha técnica está completa**.

---

## 2. Principios y límites del proyecto

Estos principios son vinculantes para todas las fases. Son las "decisiones conceptuales" que el documento maestro impone al modelo y, por extensión, al producto.

| # | Principio | Implicación de diseño |
|---|---|---|
| P1 | **No es una enciclopedia estática ni un catálogo de puertos.** | Es una base de conocimiento técnica, versionada y navegable donde cada objeto se relaciona con otros objetos y con las fuentes que justifican su descripción. |
| P2 | **"Exhaustivo" = máxima cobertura verificable.** | La exhaustividad es un objetivo abierto medible por cobertura de registros y organizaciones de estandarización, no una afirmación de conocer "todos los protocolos jamás creados". |
| P3 | **Ciclo de vida explícito.** | Cada elemento lleva estado: vigente, actualizado, obsoleto, sustituido, experimental, propietario, restringido, histórico o desconocido, con fecha y fuente. |
| P4 | **Separación epistemológica.** | Protocolo ≠ estándar ≠ implementación ≠ servicio ≠ formato de datos ≠ algoritmo ≠ mecanismo de transporte ≠ interfaz ≠ tecnología física. No mezclarlos bajo una única etiqueta. |
| P5 | **Doble clasificación.** | OSI (ISO 7498) y TCP/IP deben coexistir con una representación funcional realista: plano de datos, control, gestión, seguridad, sincronización, señalización y aplicación. |
| P6 | **Los diagramas explican comportamiento.** | No se limitan a dibujar iconos: representan flujo, encapsulación, estados, mensajes y decisiones. |
| P7 | **Trazabilidad total.** | Toda afirmación técnica importante debe poder retroceder hasta una fuente. La fuente primaria prevalece sobre el resumen de terceros cuando exista. |
| P8 | **No inventar.** | No se rellenan huecos con especulación. Lo no documentado públicamente se marca como tal ("documentación pública insuficiente"). |
| P9 | **Datos regenerables frente a texto duplicado.** | Los diagramas y los catálogos se generan a partir de datos estructurados siempre que sea posible; no se mantienen cientos de fichas a mano. |
| P10 | **Actualización continua.** | La aplicación es un sistema de conocimiento que debe poder actualizarse durante años: separación ejecutable / base de conocimiento / fuentes / índices / assets. |

**Límites explícitos del proyecto:**

- No se reproducen instrucciones operativas clasificadas ni procedimientos que dependan de documentación no pública.
- No se confunde un puerto registrado con el uso legítimo de un protocolo (advertencia explícita de IANA).
- No se atribuye a OSI una exactitud que no tenga en todas las arquitecturas reales.
- No se distingue por dogma: la decisión tecnológica se justifica con una matriz ponderada (sección 12–13).
- Se separan hechos, inferencias y recomendaciones, y se usan fechas absolutas al describir vigencia.

---

## 3. Mapa completo de dominios de investigación

### 3.1. Familias de conocimiento (alcance funcional)

| Familia | Contenido |
|---|---|
| **Redes** | LAN, WAN, MAN, PAN, WLAN, WWAN, redes móviles, centros de datos, Internet, redes industriales/OT, IoT, redes vehiculares, satélite, radio, redes tácticas y otros dominios técnicamente documentables. |
| **Dispositivos** | Hosts, NIC, repetidores, hubs históricos, bridges, switches L2/L3, routers, gateways, firewalls, IDS/IPS, proxies, balanceadores, controladores inalámbricos, AP, modems, transceptores, concentradores, servidores de infraestructura, appliances de seguridad, dispositivos SD-WAN/SDN, elementos de redes móviles y equipos especializados. |
| **Tipos de red** | Clasificación por alcance, topología, medio, movilidad, administración, arquitectura, dominio de confianza, plano operativo, tecnología de acceso y uso profesional. |
| **Protocolos** | Desde enlace y acceso al medio hasta aplicación; señalización, descubrimiento, configuración, routing, multicast, movilidad, transporte, sesión, seguridad, gestión, sincronización, telemetría, almacenamiento/red, IoT/OT, radio/móvil y protocolos históricos. |
| **Mensajes y objetos transmitidos** | Trama, paquete, datagrama, segmento, celda, símbolo, flujo, registro, mensaje, PDU, SDU, ADU, objeto semántico, elemento de información, TLV, campo, atributo, etiqueta, encabezado, trailer y payload (ver Apéndice B). No se impone "paquete" como término universal. |
| **Capas y niveles** | OSI/ISO 7498, TCP/IP y modelos de fabricantes/estándares; funciones transversales: direccionamiento, encaminamiento, control de congestión, seguridad, autenticación, sincronización, gestión, calidad de servicio y observabilidad. |

### 3.2. Ejes simultáneos de investigación (taxonomía maestra)

La investigación se ejecuta en **nueve ejes simultáneos**; ningún objeto debe clasificarse por un único eje.

| Eje | Qué debe investigar el modelo |
|---|---|
| **A. Modelo por capas** | OSI 1–7; TCP/IP; modelos híbridos; correspondencias y límites. |
| **B. Plano funcional** | Data plane, control plane, management plane, security plane, synchronization/timing, signaling y orchestration. |
| **C. Dominio** | Enterprise, ISP/carrier, data center, cloud, industrial/OT, IoT, telecom móvil, radiocomunicaciones, satélite, vehicular, investigación/académico, defensa y táctico. |
| **D. Medio** | Cobre, fibra, radio, microondas, satélite, infrarrojo, acústico y otros medios relevantes. |
| **E. Alcance** | PAN, LAN, WLAN, CAN, MAN, WAN, Internet, interdominio, intercontinental y redes federadas. |
| **F. Administración** | Centralizada, distribuida, SDN, controller-based, ad-hoc, mesh, federada, peer-to-peer. |
| **G. Estado del estándar** | Standard, Proposed Standard, Internet Draft, Informational, Experimental, obsoleto, propietario, vendor-specific, military/public standard, histórico. |
| **H. Perspectiva temporal** | Tecnologías actuales, antecesoras, transición, deprecadas y tendencias emergentes. |
| **I. Perspectiva de seguridad** | Autenticación, autorización, confidencialidad, integridad, disponibilidad, anti-replay, key exchange, trust model, segmentación, observabilidad y respuesta. |

---

## 4. Registro de organizaciones, autoridades y fuentes maestras

Objetivo de la Fase 1: construir un **catálogo de autoridades y registros** que defina *de dónde sale el universo a investigar*. Cada entrada del registro lleva: organismo, ámbito, registros/publicaciones clave, método de acceso, ciclo de actualización y nivel de autoridad.

| Organización / Registro | Ámbito | Registros clave | Método de acceso / sincronización | Nivel de autoridad |
|---|---|---|---|---|
| **IETF / RFC Editor** | Estándares de Internet (protocolos, formatos, procedimientos) | RFC (normativas, proposed, informational, experimental, historic), Internet Drafts, grupos de trabajo | Datatracker + RFC Editor; descarga de índices y `.txt`/`.xml` | Primaria normativa |
| **IANA** | Registros centrales de Internet | Service Name & Transport Protocol Port Number Registry, Protocol Numbers, EtherTypes, MIME types, AS numbers, etc. | Descarga oficial / API; **sincronizar como fuente de datos, no copiar a mano** | Primaria normativa (registro oficial) |
| **IEEE** | LAN/MAN (802.x), estándares industriales | 802.1, 802.3, 802.11, P802.15, etc. | Get IEEE / SA; catálogo de estándares | Primaria normativa |
| **ISO/IEC** | Estándares internacionales | ISO/IEC 7498 (OSI), 11801 (cableado), etc. | Catálogo ISO | Primaria normativa |
| **ITU-T** | Telecomunicaciones | Recomendaciones X/Y/G (X.25, G.991…), señalización | Catálogo ITU-T | Primaria normativa |
| **3GPP** | Redes móviles | 5G/4G/3G, GSM; especificaciones por serie (TS 23.x, 24.x, 38.x…) | Portal 3GPP (descarga de specs) | Primaria normativa |
| **ETSI** | Telecomunicaciones europeas | ISDN, GSM, NFV, MEC, etc. | Catálogo ETSI | Primaria normativa |
| **W3C** | Web (cuando corresponda) | HTTP/HTML/URL/etc. | Recomendaciones W3C | Primaria normativa |
| **ICANN / registries** | Dominios y direccionamiento global | Registries de TLD, policy | Páginas oficiales | Registro |
| **Organismos industriales** | Perfiles de interoperabilidad | MEF, ONF, OpenStack, etc. | Sitios oficiales | Estándar industrial |
| **NIST** | Ciberseguridad y guías | SP 800-207 (Zero Trust), SP 800-series, NIST CSF | csrc.nist.gov (descarga PDF) | Guía normativa de referencia |
| **MITRE** | Amenazas y defensa | ATT&CK (tácticas/técnicas), CWE, CVE | attack.mitre.org | Base de conocimiento defensiva (complementaria) |
| **DLA ASSIST / QuickSearch** | Estándares militares públicos (EE. UU.) | MIL-STD-188, MIL-STD-2045, MIL-STD-6020, etc. | quicksearch.dla.mil | "Military/Public Standard" |
| **Proyectos open source** | Implementaciones y observabilidad | Wireshark (dissectors), tcpdump/libpcap, GNS3, FRRouting, etc. | Repositorios oficiales, docs | Primaria de implementación |
| **Fabricantes** | Documentación de implementación | Documentación técnica oficial, white papers, manuales | Sitios oficiales | Primaria de implementación |
| **Académico** | Investigación y estándares emergentes | Papers, RFCs experimentales, tesis de ingeniería | Bibliotecas digitales (IEEE Xplore, ACM, arXiv) | Secundaria especializada |

**Regla de sincronización:** los registros vivos (IANA, RFC index, etc.) se consumen mediante un **pipeline de importación/normalización** versionado que reconstruye el índice de forma reproducible. No se copian manualmente y no se fijan en el ejecutable.

---

## 5. Taxonomía propuesta

### 5.1. Familias funcionales de protocolos

El inventario se organiza por familias funcionales **además** de los ejes de la sección 3.2:

1. **Acceso y enlace:** Ethernet/IEEE 802.3, Wi-Fi/802.11, PPP, L2TP, VLAN (802.1Q), LAG, spanning tree (STP/RSTP/MSTP), control de acceso al medio (CSMA/CD, CSMA/CA, TDMA…).
2. **Direccionamiento, descubrimiento y configuración:** IP (IPv4/IPv6), ARP/NDP, DNS, DHCP, mDNS/LLMNR, netbios, zeroconf/service discovery.
3. **Routing y forwarding:** RIP, OSPF, IS-IS, BGP, EIGRP, multicast (IGMP, PIM), MPLS, segment routing, policy routing.
4. **Movilidad:** Mobile IP, LTE/5G (handover), roaming 802.11, LISP, etc.
5. **Transporte y sesión:** TCP, UDP, SCTP, DCCP, QUIC, TLS/DTLS, RTP/RTCP, ESP/AH (IPsec transporte).
6. **Aplicación:** HTTP/1.x, HTTP/2, HTTP/3, SMTP, FTP, SSH, DNS sobre transporte, NFS/SMB/CIFS, SIP, XMPP, etc.
7. **Gestión, monitorización y operaciones:** SNMP, NETCONF/YANG, gRPC/telemetría, syslog, IPFIX/NetFlow, RADIUS, TACACS+, ICMP.
8. **Sincronización temporal:** NTP, PTP/IEEE 1588, Precision Time Protocol para redes industriales.
9. **Almacenamiento/red y automatización:** iSCSI, FC/FCoE, NVMe-oF, SDN/OpenFlow, controladores, IaC para redes.
10. **Seguridad:** IPsec/IKE, TLS/DTLS, Kerberos, RADIUS/EAP, 802.1X, DNSSEC, túneles (GRE, VXLAN, WireGuard), segmentación.
11. **IoT/OT y tiempo real:** MQTT, CoAP, Modbus, DNP3, PROFINET, EtherCAT, OPC UA, LoRaWAN, Zigbee, BACnet.
12. **Radio/móvil y satélite:** GSM/UMTS/LTE/5G NR, TETRA, DMR, satélite (DVB, VSAT), táctico (enlace de datos).
13. **Históricos y de transición:** X.25, Frame Relay, ATM, Token Ring, FDDI, NetBIOS/NetBEUI, IPX/SPX, AppleTalk, ARCNET, SONET/SDH (transición), ISDN.

### 5.2. Vocabulario de unidades de datos

Cada protocolo debe identificar cuál es su **unidad pertinente**; está prohibido usar "paquete" como etiqueta genérica:

trama · paquete · datagrama · segmento · celda · símbolo · flujo · registro · mensaje · PDU · SDU · ADU · objeto semántico · elemento de información · TLV · campo · atributo · etiqueta · encabezado · trailer · payload.

### 5.3. Taxonomía de dispositivos

Para cada clase de dispositivo se documenta: **propósito, capa(s), plano(s), interfaces, medios, dirección del flujo, PDU que procesa, funciones de forwarding/control/management, tablas o estados internos, dependencia de protocolos, ejemplos de implementación, escenarios de uso y limitaciones.**

Clases raíz: hosts/endpoints · NIC · repetidor/hub · bridge/switch L2/L3 · router · gateway · firewall · IDS/IPS · proxy · balanceador · controlador inalámbrico/AP · modem/transceptor · concentrador · servidor de infraestructura · appliance de seguridad · SD-WAN/SDN · elementos de red móvil · equipos especializados (OT, satélite, radio táctico).

### 5.4. Taxonomía de tipos de red

Para cada tipo de red (por eje E de la sección 3.2 y por dominio): ámbito, topología, medios, direccionamiento, mecanismos de acceso, arquitectura, escalabilidad, latencia, movilidad, seguridad, dispositivos típicos, protocolos frecuentes y casos de uso.

---

## 6. Esquema de datos y plantillas de fichas

### 6.1. Modelo de dominio (entidades y relaciones)

Entidades núcleo (con claves estables tipo URN, p. ej. `urn:proto:ietf:rfc9114`):

| Entidad | Responsabilidad |
|---|---|
| `Protocol` | Ficha de protocolo (nombre, acrónimo, aliases, familia, estado, ciclo de vida). |
| `Standard` | Norma o estándar de referencia (RFC, ISO/IEC, IEEE, 3GPP, MIL-STD…), con versión, fecha y organismo. |
| `Version` | Versión concreta de un protocolo/estándar con vigencia temporal (valid_from/valid_to). |
| `MessageType` | Tipos de mensajes (nombre, propósito, dirección, condiciones de emisión). |
| `Field` | Campos: nombre, offset, longitud, tipo, semántica, valores, flags, obligatoriedad. |
| `PDU` | Unidad de datos (trama, paquete, datagrama, segmento, celda, TLV…), estructura, framing, endianness. |
| `Layer` | Modelo de capas (OSI 1–7, TCP/IP, híbrido) y pertenencias. |
| `Plane` | Plano funcional (datos, control, gestión, seguridad, sincronización, señalización, orquestación). |
| `Device` | Dispositivos y su taxonomía de funciones. |
| `NetworkType` | Tipos de red y sus atributos (alcance, topología, medio, movilidad…). |
| `AddressingScheme` | Esquemas de direccionamiento/naming (IPv4, IPv6, MAC, E.164, URI, FQDN…). |
| `Implementation` | Implementaciones (SO, librerías, fabricantes, appliances) distinguiendo "soporta" de "implementa completamente". |
| `Source` | Fuente: URL/identificador, versión, fecha de publicación, fecha de consulta, sección, nivel de autoridad. |
| `Capture` | Capturas PCAP/PCAPNG y enlace paquete → ficha de protocolo (dissection). |
| `Diagram` | Catálogo de diagramas regenerables desde datos estructurados. |
| `SecurityMechanism` | Mecanismos de seguridad (auth, cifrado, integridad, anti-replay, key exchange…). |
| `Relationship` | Relaciones tipadas entre entidades (encapsula, corre-sobre, depende-de, alternativa-a, implementa, documenta, es-version-de, sustituye-a…). |

**Reglas de diseño de datos:**

- **Claves estables e inmutables** (URN) separadas de los nombres mostrados (que pueden cambiar).
- **Versionado temporal** en todas las entidades con vigencia (`valid_from`, `valid_to`) y autoría.
- **Trazabilidad:** toda ficha referencia al menos una `Source`; los campos críticos pueden referenciar sección concreta.
- **Integridad referencial** verificada por CI (no se permiten enlaces a entidades inexistentes).
- **Separación de datos normativos vs. datos de implementación** (el esquema distingue `normative` de `informational`).

### 6.2. Ficha mínima normalizada de protocolo (obligatoria, ampliable)

1. **Identidad:** nombre completo, acrónimo, aliases, familia, organización/autoridad, referencia (RFC/ISO/IEEE/3GPP/ETSI/ITU/MIL-STD/STANAG u otra).
2. **Estado:** vigente, experimental, obsoleto, sustituido, propietario, histórico, restringido/no público — con **fecha y fuente**.
3. **Finalidad:** problema que resuelve, actores, casos de uso y **cuándo NO debe utilizarse**.
4. **Encapsulación:** protocolo inmediatamente inferior y superior; dependencias y tunneling.
5. **Capas:** OSI, TCP/IP y plano funcional.
6. **Transporte y direccionamiento:** TCP/UDP/SCTP/DCCP/QUIC u otro; puertos, EtherTypes, IP protocol numbers, next-header values o equivalentes.
7. **PDU y objeto transmitido:** nombre técnico, estructura, longitud, framing, endianness, codificación, alineamiento, MTU/MSS/tamaño cuando aplique.
8. **Mensajes:** tipos, propósito, dirección, condiciones de emisión, respuestas, temporizadores, estados y errores.
9. **Campos:** nombre, offset, longitud, tipo, semántica, valores permitidos, flags, obligatoriedad, compatibilidad y seguridad.
10. **Secuencia:** establecimiento, negociación, operación normal, actualización, cierre, excepciones y recuperación.
11. **Addressing/naming:** tipos y ámbito de direcciones, identificadores, nombres, IDs, etiquetas y resolución.
12. **Routing/forwarding/discovery:** mecanismos y algoritmos empleados si están definidos.
13. **Seguridad:** mecanismos nativos, opciones, dependencias criptográficas, amenazas conocidas y recomendaciones de configuración.
14. **QoS y rendimiento:** latencia, jitter, pérdida, control de congestión, retransmisión, prioridad y escalabilidad.
15. **Observabilidad:** cómo reconocer el protocolo en una captura; campos visibles; filtros; indicadores y métricas.
16. **Interoperabilidad:** perfiles, extensiones, diferencias de implementación y problemas conocidos.
17. **Implementaciones:** SO, librerías, fabricantes, appliances y herramientas públicas ("soporta" ≠ "implementa completamente").
18. **Fuentes y evidencia:** especificación primaria, versión, fecha de consulta, sección/página, nivel de autoridad y grado de confianza.

### 6.3. Otras plantillas de fichas (entregables de investigación)

- **Ficha de dispositivo** (taxonomía de la sección 5.3).
- **Ficha de tipo de red** (sección 5.4).
- **Ficha de estándar** (organismo, publicación, estado, fecha, relación con protocolos).
- **Ficha de mensaje/PDU** (estructura, campos, codificación, ejemplos normativos vs. de implementación).
- **Ficha de campo** (offset, longitud, tipo, valores, flags, obligatoriedad, compatibilidad, seguridad).
- **Registro de fuente** (URL/ID, versión, fecha publicación, fecha consulta, sección, autoridad, confianza).
- **Matriz de dependencias** (qué necesita qué para funcionar).
- **Matriz de encapsulación** (quién va sobre quién, tunneling).
- **Matriz de interoperabilidad** (perfiles, extensiones, problemas conocidos).
- **Matriz de cobertura** (métricas de la sección 7).
- **Registro de incertidumbres y contradicciones** (conflictos entre fuentes y resolución).
- **Catálogo de diagramas** (cada diagrama regenerable con su plantilla y datos de origen).

---

## 7. Estrategia para aproximarse a la exhaustividad

**Principio:** la exhaustividad no es una lista cerrada; es un **objetivo abierto y medible** por cobertura de registros y fuentes.

### 7.1. Universos de fuentes que permiten cobertura máxima

RFC/IETF · IANA · IEEE · ISO/IEC · ITU-T · 3GPP · ETSI · W3C · organismos de Internet · organismos de telecomunicaciones · estándares industriales · documentación oficial de fabricantes · proyectos open source relevantes · repositorios académicos · organismos gubernamentales/militares con documentación pública.

### 7.2. Métricas de cobertura (definidas y computadas por el pipeline)

| Métrica | Definición |
|---|---|
| Cobertura por organización/registro | % de ítems del registro (p. ej. IANA service names) presentes en la base |
| Cobertura por familia de protocolos | % de familias con fichas completas |
| Cobertura por capa | % de protocolos por capa OSI/TCP-IP con ficha válida |
| Cobertura por dominio de red | Idem por dominio (enterprise, OT, móvil…) |
| Cobertura por estado de ciclo de vida | Distribución por estado (vigente/obsoleto/…) |
| % de fichas con fuente primaria | Fichas cuya afirmación principal tiene fuente primaria normativa |
| % con wire format documentado | Protocolos con estructura binaria/textual pública documentada |
| % con diagrama | Protocolos prioritarios con diagrama regenerable |
| % con implementación verificada | Protocolos con implementación confirmada (no solo "soportado") |
| % con fecha de revisión reciente | Fichas revisadas en los últimos N meses (p. ej. 12) |

### 7.3. Reglas de sincronización

- Los registros oficiales se consumen por pipeline (IANA como fuente de datos con su versión/fecha), **nunca copiados a mano**.
- Cada release del dataset es un **snapshot versionado** con hash, fecha y procedencia.
- La actualización del conocimiento no requiere recompilar el ejecutable (separación ejecutable / dataset / fuentes / índices / assets).

---

## 8. Plan de investigación fase a fase (F0–F9)

**Visión de conjunto:** cada fase produce al menos uno de: documento, catálogo, esquema de datos y/o criterios de aceptación. Las dependencias son acumulativas (F_n consume resultados de F_{n-1}); las fases F3–F7 pueden ejecutarse en paralelo parcial una vez cerrada la taxonomía (F0–F2). Cada fase tiene **criterios de salida** explícitos antes de pasar a la siguiente.

| Fase | Propósito | Tareas principales | Depende de | Criterios de salida (mínimo) |
|---|---|---|---|---|
| **F0 — Definición y límites** | Objetivo, audiencia, profundidad, política de fuentes, taxonomía, nomenclatura, estado de ciclo de vida, alcance de protocolos públicos/militares. | Redactar carta de alcance; fijar glosario de PDU; definir ejes de clasificación; fijar política de fuentes y de incertidumbre. | — | Documento de alcance aprobado; glosario y ejes fijados. |
| **F1 — Inventario maestro de autoridades** | Catálogo de organizaciones y registros (sección 4). | Registrar IETF/RFC, IANA, IEEE, ISO/IEC, ITU-T, 3GPP, ETSI, W3C, ICANN, industriales, gubernamentales/militares públicos; documentar método de acceso y ciclo de actualización. | F0 | Registro de autoridades completo con URLs y versiones; política de sincronización definida. |
| **F2 — Universo de dispositivos y redes** | Taxonomía completa de dispositivos, arquitecturas, topologías, medios, tecnologías de acceso y escenarios. | Clasificar dispositivos (5.3), tipos de red (5.4), topologías y medios; fichas de ejemplo. | F0 | Catálogo de dispositivos y tipos de red validado; 3+ fichas piloto por clase. |
| **F3 — Inventario de protocolos** | Recolectar, normalizar, deduplicar y versionar protocolos por familia y dominio. | Importar IANA service names como fuente de datos; construir inventario maestro; deduplicar por URN; versionar. | F0, F1 | Inventario maestro con familias y estados; pipeline IANA operativo; métricas de cobertura base. |
| **F4 — Profundización protocolar** | Completar la ficha normalizada por protocolo; resolver dependencias, encapsulación y versiones; asociar estándares y capturas representativas. | Priorizar protocolos de alto valor (TCP/IP, HTTP/3, BGP, DNS…); completar fichas; resolver encapsulación y dependencias. | F3 | X% de fichas prioritarias completas con fuente primaria; matriz de encapsulación. |
| **F5 — Mensajería y PDU** | Modelar cabeceras, campos, mensajes, secuencias y máquinas de estado; distinguir formatos normativos de ejemplos. | Fichas de PDU/campo; máquinas de estado; ejemplos normativos vs. implementación; layouts regenerables. | F4 | Catálogo de PDU y campos; layouts de wire format regenerables validados contra capturas. |
| **F6 — Seguridad y operatividad** | Amenazas, autenticación, criptografía, hardening, segmentación, monitoring, troubleshooting; relación con NIST ZTA y MITRE ATT&CK (complementario). | Fichas de seguridad por protocolo; mapeo a NIST SP 800-207 y ATT&CK; guías de observabilidad. | F4 | Registro de seguridad por protocolo; mapeo a marcos de referencia documentado. |
| **F7 — Dominios profesionales y especiales** | OT/ICS, telecom, cloud, data center, IoT, satélite, radio y documentación militar pública (DLA ASSIST: MIL-STD-188, -2045, -6020). | Ciclo de autoridades militares públicas; fichas de dominios especiales solo con material público verificable. | F1, F4 | Cobertura de dominios especiales; política militar (sección 10) aplicada y auditada. |
| **F8 — Validación** | Revisión cruzada de fuentes, consistencia de nomenclatura, validación con capturas, comprobación de versiones, identificación de lagunas e incertidumbre. | Auditoría por pares; rechequeo de versiones; validación de wire formats contra capturas reales; registro de lagunas. | F3–F7 | Informe de validación; % de lagunas clasificadas; contradicciones resueltas o registradas. |
| **F9 — Especificación de producto** | Solo tras cerrar el plan de conocimiento: arquitectura de software, navegación, búsqueda, filtros, visualizaciones, almacenamiento, actualización de fuentes, pruebas y empaquetado. | Elaborar especificación de producto; backlog técnico listo (sin programar); ejecutar la matriz tecnológica (sección 12). | F0–F8 | Especificación de producto aprobada; backlog técnico priorizado; entrada a la Fase II (sección 17). |

**Nota de paralelización:** F3, F4 y F7 pueden solaparse parcialmente; F5 depende de F4; F8 es una compuerta de calidad; F9 nunca se inicia antes de que el plan de conocimiento esté validado.

---

## 9. Matriz de calidad, evidencia y trazabilidad

### 9.1. Jerarquía de evidencia

| Nivel | Tipo de fuente | Uso permitido |
|---|---|---|
| **1 — Primaria normativa** | RFC, estándar ISO/IEC, IEEE, ITU-T, 3GPP, ETSI, IETF draft/working group, registro oficial, MIL-STD público, documentación normativa de la autoridad competente | Base de afirmaciones críticas (wire format, semántica, valores). |
| **2 — Primaria de implementación** | Documentación oficial de fabricante/proyecto, código fuente, repositorios mantenidos por el proyecto, manuales técnicos | Comportamiento real, "soporta" vs. "implementa", divergencias especificación↔práctica. |
| **3 — Secundaria especializada** | Libros técnicos, white papers de calidad, artículos académicos, documentación de ingeniería reconocida | Contexto, comparativas, explicación. Nunca única base de un detalle crítico. |
| **4 — Terciaria** | Blogs, foros, tutoriales, resúmenes | Apoyo y referencia cruzada; nunca fuente única de una afirmación importante. |

### 9.2. Campos de evidencia obligatorios en cada `Source`

URL/URI o identificador · versión del documento · organismo · fecha de publicación · **fecha de consulta** · sección/página cuando sea posible · nivel de autoridad (1–4) · grado de confianza (alto/medio/bajo/desconocido).

### 9.3. Controles automáticos y manuales (CI + CD de datos)

| Control | Tipo | Qué detecta |
|---|---|---|
| Validación de esquemas | Auto (CI) | Fichas que no cumplen el esquema |
| Enlaces rotos | Auto (CI) | Sources con URL muerta |
| Fuentes obsoletas | Auto (pipeline) | Sources sin revisión reciente o versión desactualizada |
| Duplicados | Auto | URNs o acrónimos duplicados |
| Contradicciones | Auto + manual | Conflictos entre fuentes; exige resolución documentada |
| Campos sin descripción | Auto | Campos de PDU sin semántica |
| Protocolos sin fuente primaria | Auto | Fichas críticas sin fuente nivel 1 |
| Diagramas no regenerables | Auto | Diagramas sin datos de origen o sin regeneración determinista |
| Errores de nomenclatura | Auto | Inconsistencias OSI/TCP-IP/planos/nombres |
| Regresiones del dataset | Auto | Comparación contra snapshot previo (hash/diff) |
| Integridad referencial | Auto | Relaciones hacia entidades inexistentes |

**Regla de conflictos:** el modelo debe registrar los conflictos entre fuentes y explicar cuál prevalece y por qué. No se infiere un detalle de wire format que la especificación no publique; se marca "no documentado públicamente". No se confunde puerto registrado con uso legítimo del protocolo (advertencia IANA).

---

## 10. Política para protocolos históricos, propietarios, experimentales y militares/públicos

**Objetivo:** cobertura amplia en términos históricos y técnicos, limitada a información **legalmente pública y verificable**.

### 10.1. Clases de tratamiento

| Clase | Tratamiento |
|---|---|
| **Estándar público** | Documentar con fuente primaria: organismo, publicación, estado y fecha (p. ej. MIL-STD a través de DLA ASSIST/QuickSearch). |
| **Existencia referenciada, detalles no públicos** | Registrar nombre y ámbito si es verificable; declarar "documentación pública insuficiente". **No rellenar huecos con especulación.** |
| **Información histórica** | Documentar contexto, interoperabilidad y estructuras públicamente divulgadas sin reproducir procedimientos clasificados. |
| **Restringido / no verificable públicamente** | Registrar la limitación y marcar el dato como no verificable públicamente. |

### 10.2. Reglas específicas

- Crear la clase de fuente **"Military/Public Standard"** con organismo, publicación, estado y fecha.
- Priorizar repositorios oficiales: DLA ASSIST/QuickSearch (p. ej. MIL-STD-188 — comunicaciones tácticas, activo, doc. 05-06-2026; MIL-STD-2045 — transferencia de datos sin conexión para C4I; MIL-STD-6020 — interoperabilidad entre Tactical Data Links).
- Incluir familias tácticas, C4ISR, enlaces de datos, radio, interoperabilidad y gateways **solo hasta el nivel permitido por las fuentes públicas**.
- **MITRE ATT&CK** se usa de forma complementaria para modelar tácticas, técnicas y detecciones defensivas, **nunca** como sustituto de la documentación del protocolo.
- La documentación militar se hace con enfoque **defensivo y arquitectónico**.

---

## 11. Requisitos de diagramación y representación de mensajes

Los diagramas son parte central del producto: **plantillas reutilizables, no ilustraciones aisladas**, y generables de forma determinista desde datos estructurados siempre que sea posible (sin depender de imágenes rasterizadas manuales).

| # | Tipo de diagrama | Función |
|---|---|---|
| 1 | **Arquitectura física/lógica** | Hosts, enlaces, dominios y dispositivos con leyenda. |
| 2 | **Pila y encapsulación** | Encapsulación/decapsulación desde aplicación hasta medio y recorrido inverso. |
| 3 | **Secuencia temporal** | Mensajes en orden temporal entre al menos dos participantes (handshakes). |
| 4 | **Máquina de estados** | Estados y transiciones, especialmente con handshake o FSM. |
| 5 | **Mensaje / wire format** | Despiece bit/byte de encabezado, opciones, TLV y payload (cuando sea público). |
| 6 | **Flujo de decisión** | Comportamiento según flags, códigos, errores o capacidades; errores y recuperación. |
| 7 | **Seguridad y fronteras de confianza** | Autenticación, intercambio de claves, cifrado, integridad, límites de confianza. |
| 8 | **Ruta extremo a extremo** | Qué dispositivos inspeccionan, transforman o reencapsulan la PDU. |
| 9 | **Comparativo** | Versiones o protocolos alternativos: capa, PDU, latencia, seguridad, casos de uso. |
| 10 | **Captura / dissection** | Correspondencia entre un frame/packet real y sus campos documentados; compatible con el modelo de disección por capas de Wireshark (cada dissector decodifica su parte y entrega el payload al siguiente). |

**Requisitos técnicos (para la Fase de software):** modelo de grafo desacoplado del renderer; layouts deterministas; exportación a SVG/PNG/PDF; posibilidad de integrar Mermaid/Graphviz/Cytoscape.js según el tipo de vista.

---

## 12. Matriz comparativa de tecnologías

Criterios evaluados (según el mandato del master prompt): multiplataforma, rendimiento, consumo de recursos, facilidad de UI avanzada, gráficos/grafos, acceso a filesystem, integración con bases de datos, mantenimiento, testing, empaquetado, seguridad, ecosistema y adecuación al proyecto.

| Tecnología | Fortalezas | Veredicto | Consideraciones |
|---|---|---|---|
| **Avalonia + C#/.NET** | Escritorio cross-platform con UI propia (XAML), núcleo fuertemente tipado en C#, arquitectura MVVM, renderer propio; soporte Windows/macOS/Linux documentado. | **Excelente candidato principal** | Rico para escritorio, excelente integración .NET y almacenamiento local; para diagramas muy interactivos puede convenir integrar un motor web/graph especializado. |
| **Tauri + Rust + web UI** | Híbrido: webview para UI, backend Rust; paquete ligero y UI web moderna. | **Candidato alternativo** | Gran opción si se prioriza experiencia gráfica web y runtime compacto; mayor cambio tecnológico para un equipo centrado en C#. |
| **Electron + TypeScript** | Chromium + Node.js; una única base JS para Windows/macOS/Linux. | **Candidato fuerte** | Excelente ecosistema de visualización y prototipado; mayor consumo de memoria/paquete. |
| **WPF/.NET** | Muy maduro para Windows y C#. | **Solo si Windows es requisito absoluto** | Windows-first; limita el objetivo cross-platform. |
| **Qt/C++ o Qt/Python** | Potente para aplicaciones técnicas y multiplataforma. | **Alternativa especializada** | Gran capacidad de UI, networking y herramientas técnicas; más complejidad de stack y mantenimiento. |

### 12.1. Matriz ponderada de decisión

Puntuación 1–5 (5 = mejor). Recomendación de partida, sujeta a validación con prototipos (spike) en F9.

| Criterio | Peso | Avalonia/.NET | Tauri/Rust | Electron/TS | WPF/.NET | Qt |
|---|---|---|---|---|---|---|
| Multiplataforma | 10% | 5 | 5 | 5 | 2 | 5 |
| Rendimiento | 10% | 4 | 5 | 3 | 4 | 5 |
| Consumo de recursos | 8% | 4 | 5 | 2 | 4 | 4 |
| Facilidad de UI avanzada | 12% | 4 | 4 | 5 | 4 | 4 |
| Gráficos / grafos / diagramas | 12% | 4 | 5 | 5 | 3 | 4 |
| Acceso a filesystem | 7% | 5 | 5 | 5 | 5 | 5 |
| Integración con bases de datos | 8% | 5 | 4 | 4 | 5 | 4 |
| Mantenimiento | 8% | 5 | 4 | 4 | 4 | 3 |
| Testing | 7% | 5 | 4 | 4 | 5 | 3 |
| Empaquetado / distribución | 7% | 4 | 4 | 4 | 4 | 4 |
| Seguridad (superficie/aislamiento) | 6% | 4 | 5 | 3 | 4 | 4 |
| Ecosistema y comunidad | 5% | 4 | 4 | 5 | 5 | 4 |
| **Adecuación al proyecto (síntesis)** | — | **5** | 4 | 4 | 2 | 3 |
| **Puntuación ponderada** | 100% | **4,40** | 4,53 | 4,14 | 3,95 | 4,12 |

> **Lectura honesta de la matriz:** Tauri/Rust y Avalonia/.NET quedan muy próximos. La recomendación de partida se decanta por **Avalonia/.NET** no por dogma sino por: (a) dominio C# y reutilización de modelos de dominio, serialización, validación y servicios .NET; (b) capacidad de construir una UI de escritorio compleja multiplataforma; (c) menor salto tecnológico para el equipo. **La decisión final se cierra en F9** con dos spikes: UI rica (tablas, paneles, navegación por grafo) y renderer de diagramas determinista (SVG). Aunque la ponderación numérica favorece ligeramente a Tauri, el criterio de "adecuación al proyecto" y el coste de cambio .NET→Rust inclinan la balanza; el spike es el árbitro final.

---

## 13. Recomendación tecnológica argumentada y arquitectura

### 13.1. Recomendación de partida

**Arquitectura desktop en C#/.NET + Avalonia**, complementada con un motor de diagramación/grafos web embebido cuando aporte ventajas funcionales. Se apoya en cuatro factores: dominio de C#, capacidad de construir una UI de escritorio compleja, multiplataforma, y posibilidad de reutilizar modelos de dominio, serialización, validación y servicios .NET (Avalonia: framework .NET cross-platform con XAML y renderer propio; soporte objetivo Windows, macOS y Linux).

### 13.2. Pila técnica propuesta

| Componente | Elección |
|---|---|
| **UI** | Avalonia + XAML + MVVM; componentes propios para navegación jerárquica, búsqueda, filtros, tabs, paneles y fichas técnicas; temas claro/oscuro. |
| **Dominio** | C# con modelos inmutables donde convenga; validación, versionado y relaciones entre entidades. |
| **Datos** | SQLite como almacén local principal; JSON/YAML para fuentes importables y fixtures; FTS5 para búsqueda textual. |
| **Búsqueda avanzada** | Índice local con SQLite FTS5 o motor dedicado cuando el corpus crezca; búsqueda por protocolo, campo, RFC, puerto, capa, dispositivo, mensaje, fabricante, dominio o palabra clave. |
| **Grafos/diagramas** | Modelo de grafo desacoplado del renderer; exportación SVG/PNG/PDF; Mermaid/Graphviz/Cytoscape.js según tipo de vista. |
| **Capturas** | Integración opcional con PCAP/PCAPNG para abrir archivos y enlazar paquetes con fichas de protocolo (disección por capas de Wireshark como referencia conceptual; no se embebe Wireshark). |
| **Actualizaciones** | Pipeline de importación/normalización de registros oficiales que reconstruye el índice sin editar cientos de fichas. |
| **Versionado** | Cada protocolo y cada fuente admiten vigencia temporal. |
| **Pruebas** | Unitarias, integración, snapshot de esquemas de mensajes, validación de diagramas y pruebas de búsqueda. |

### 13.3. Arquitectura por capas (objetivo)

| Capa | Responsabilidad |
|---|---|
| **Presentation** | Avalonia UI; MVVM; temas claro/oscuro; layouts técnicos; navegación por grafo y panel de inspección. |
| **Application** | Casos de uso: buscar, explorar, comparar, visualizar, importar fuente, actualizar catálogo, abrir captura, exportar informe. |
| **Domain** | Entidades: `Protocol`, `Standard`, `MessageType`, `Field`, `PDU`, `Device`, `NetworkType`, `AddressingScheme`, `Layer`, `Plane`, `Source`, `Implementation`, `Capture`, `Diagram`, `Version`. |
| **Infrastructure** | SQLite, FTS5, serialización JSON/YAML, caché, importadores, descarga/actualización de fuentes y adaptadores PCAP. |
| **Visualization** | Renderer de diagramas desacoplado; SVG como formato vectorial de intercambio; layouts deterministas y plantillas por tipo de diagrama. |
| **Knowledge pipeline** | Ingestion → normalization → deduplication → entity linking → validation → indexing → release snapshot. |
| **Quality** | CI, unit tests, schema validation, link checking, source freshness checks, data completeness scoring y regression tests. |
| **Distribution** | Instaladores por SO; versión del dataset separada de la versión del ejecutable; modo offline con documentación embebida. |

**Separación de datos actualizables** (requisito de diseño): ejecutable de la aplicación · base de conocimiento · fuentes descargadas/caché · índices de búsqueda · assets de diagramas. La actualización del conocimiento se ejecuta **sin recompilar el ejecutable**.

---

## 14. Hoja de ruta preliminar de desarrollo (a cerrar tras F8/F9)

> **Advertencia conforme al master prompt:** esta hoja de ruta es **preliminar**. El plan detallado de arquitectura, UX/UI, implementación, pruebas y distribución se elabora en la Fase II (sección 17) una vez validado el plan de conocimiento. No se inicia la programación antes de F9.

| Hito | Contenido | Compuerta |
|---|---|---|
| **D0 — Decisiones** | Cierre de la matriz tecnológica (sección 12), spikes de UI y renderer de diagramas. | Spike validado |
| **D1 — Núcleo de dominio** | Modelo de dominio C# (sección 6.1), validación, versionado, SQLite + FTS5. | Esquema implementado y testeado |
| **D2 — Pozos de datos** | Pipeline de importación IANA/RFC y otros registros; snapshot versionado del dataset. | Pipeline reproducible |
| **D3 — UI básica** | Avalonia + MVVM: navegación jerárquica, ficha de protocolo, búsqueda FTS5, filtros. | Usable con dataset real |
| **D4 — Diagramas** | Renderer desacoplado: plantillas deterministas, exportación SVG/PNG/PDF. | Diagramas regenerables automatizados |
| **D5 — Exploración avanzada** | Grafo de relaciones, comparativas, fichas de dispositivo/red/mensaje/campo. | Navegación cruzada operativa |
| **D6 — Capturas** | Adaptador PCAP/PCAPNG; enlace paquete → ficha (disección conceptual tipo Wireshark). | Validación contra capturas reales |
| **D7 — Calidad y distribución** | CI/calidad (sección 9.3), instaladores por SO, modo offline, actualización de dataset sin recompilar. | Release 1.0 |

---

## 15. Riesgos, lagunas y decisiones que aún no deben cerrarse

### 15.1. Riesgos principales

| # | Riesgo | Mitigación |
|---|---|---|
| R1 | **Exhaustividad inalcanzable** → proyecto percibido como incompleto | Definir exhaustividad como cobertura medible por registros (sección 7); publicar métricas. |
| R2 | **Registros vivos cambiantes** (IANA, RFC, MIL-STD) | Pipeline versionado; snapshots con hash/fecha; fuentes como datos, no como texto fijo. |
| R3 | **Protocolos sin documentación pública** | Política de incertidumbre (sección 10): marcar y no inventar. |
| R4 | **Alcance demasiado amplio** (todos los dominios) | Priorización por valor; fichas "mínimas" obligatorias y ampliables. |
| R5 | **Rendimiento de UI con grandes volúmenes** | Virtualización, índices FTS5, páginas/lazy loading, renderer desacoplado. |
| R6 | **Regresión del dataset** | CI con snapshots, diffs y validación de esquema (sección 9.3). |
| R7 | **Error en la elección de stack** | Decisión no cerrada hasta F9; spikes de UI y diagramas como árbitros. |
| R8 | **Información militar/mal uso o clasificación** | Solo material público verificable; fuente "Military/Public Standard"; límites explícitos. |
| R9 | **Duplicación de esfuerzo de investigación** | Pipeline centralizado; fichas regenerables; deduplicación por URN. |
| R10 | **Coste de mantenimiento del corpus** | Automatización máxima: controles automáticos, freshness checks, completitud por scoring. |

### 15.2. Lagunas esperadas y cómo se declaran

- Formatos propietarios sin especificación pública → registro + "documentación pública insuficiente".
- Protocolos militares con detalles no públicos → clase "existencia referenciada".
- Wire formats no publicados → marcados, nunca inferidos.
- Fichas de protocolos muy recientes → confianza baja, revisión programada.

### 15.3. Decisiones que NO deben cerrarse todavía

- Elección definitiva de stack (se cierra en F9 con spikes).
- Alcance exacto del MVP (se cierra al validar F5/F8 y definir audiencia).
- Motor de diagramas concreto (Mermaid vs. Graphviz vs. Cytoscape.js) — depende de los tipos de vista priorizados.
- Estrategia de distribución (instaladores/marketplaces) — Fase II.
- Prioridad de dominios especiales (OT vs. satélite vs. táctico) — definida por el usuario/audiencia tras F0.

---

## 16. Criterios de aceptación del plan

El plan (y por extensión cada fase de investigación) se considera aceptado cuando cumple **todos** los siguientes criterios:

| # | Criterio de aceptación |
|---|---|
| C1 | No comienza por una lista arbitraria de protocolos; explica primero de dónde sale el universo a investigar (secciones 4, 7). |
| C2 | Existe una estrategia de sincronización con registros oficiales y de control de versiones (secciones 4, 7.3, 8-F1/F3). |
| C3 | Cada protocolo tiene un esquema de ficha que admite protocolos muy distintos entre sí (sección 6.2). |
| C4 | Queda claro qué se considera evidencia suficiente y cómo se expresa la incertidumbre (secciones 9.1, 10, 15.2). |
| C5 | Existe una matriz para localizar rápidamente qué capas, planos, dispositivos y mensajes intervienen en una comunicación (secciones 3.2, 6, 8). |
| C6 | Puede representarse una comunicación desde la intención de aplicación hasta el medio y el retorno, incluyendo encapsulación y cambios de PDU (secciones 11, 13.2). |
| C7 | Distingue información educativa de detalles de implementación y de configuración operacional (secciones 2, 6, 9.1). |
| C8 | Incluye un plan de mantenimiento: actualización de estándares, nuevas extensiones, deprecaciones y auditorías (secciones 7, 8, 9.3, 13.2). |
| C9 | Termina con una especificación lista para convertirse en backlog técnico, **sin** empezar prematuramente a programar (secciones 13, 14, 17). |

---

## 17. Prompt maestro de diseño y generación de software (fase siguiente)

> **Uso:** una vez aprobado y validado este plan de investigación (F0–F9), entregar este bloque a una futura sesión de IA como prompt maestro para la Fase II. Se entrega separado, conforme al mandato del documento maestro (sección 11, punto O).

---

```
PROMPT MAESTRO — FASE II: DISEÑO Y GENERACIÓN DE SOFTWARE
Contexto: el plan de investigación y documentación (PLANREDES.md) ha sido validado.
Actúa como un equipo multidisciplinar: arquitecto de software de escritorio, ingeniero
.NET senior, diseñador UX/UI para herramientas técnicas, especialista en visualización
de grafos y diagramas, ingeniero de datos/pipeline, ingeniero de calidad y responsable
de distribución multiplataforma.

PROYECTO
Construir el plan detallado (sin escribir aún el código completo del producto) para una
aplicación de escritorio profesional, multiplataforma y local-first que sirve como
plataforma de conocimiento, exploración y representación técnica de redes de
comunicaciones: dispositivos, tipos y arquitecturas de red, capas y planos funcionales,
protocolos, estándares, mensajes, PDU, formatos de datos, encapsulación, secuencias,
seguridad, observabilidad e interoperabilidad.

RESTRICCIONES VINCULANTES
1. Base de decisión tecnológica de partida: C#/.NET + Avalonia (XAML/MVVM), SQLite + FTS5,
   renderer de diagramas desacoplado con exportación SVG, pipeline de importación de
   registros oficiales y adaptador PCAP/PCAPNG opcional. No la aceptes por dogma: la matriz
   ponderada y los spikes (UI rica + diagramas deterministas) son el árbitro final.
2. El conocimiento (dataset) se actualiza SIN recompilar el ejecutable: ejecutable, base de
   conocimiento, fuentes/caché, índices y assets de diagramas son artefactos separados.
3. Plataformas objetivo: Windows, macOS y Linux; modo offline con documentación embebida.
4. Los diagramas se generan de forma determinista desde datos estructurados.
5. Todo dato importante mantiene trazabilidad hacia su fuente (fuente primaria normativa
   primero; incertidumbre explícita; sin invenciones).

ENTREGABLES DE ESTA FASE (en este orden)
A. Arquitectura de software detallada: capas (Presentation, Application, Domain,
   Infrastructure, Visualization, Knowledge pipeline, Quality, Distribution), módulos,
   contratos entre capas y decisiones de arquitectura (ADR) para: modelo de dominio,
   SQLite/FTS5, pipeline de datos, renderer de diagramas, capturas PCAP, actualización de
   dataset, versionado temporal y offline.
B. Diseño de UX/UI: mapa de navegación (explorar, buscar, comparar, ficha, grafo, captura,
   importar/actualizar, exportar), wireframes de las vistas principales, temas claro/oscuro,
   y patrones para volúmenes grandes de datos (virtualización, paneles, tabs, filtros).
C. Especificación de módulos: para cada módulo, responsabilidad, interfaz pública,
   entidades/ DTOs, dependencias y criterios de aceptación técnicos.
D. Plan de pruebas: unitarias, integración, snapshot de esquemas de mensajes, validación de
   diagramas regenerables, pruebas de búsqueda FTS5, golden-master del pipeline y pruebas
   de empaquetado por SO (incluida la CI/CD y los controles automáticos de calidad de datos:
   esquema, enlaces rotos, fuentes obsoletas, duplicados, contradicciones, integridad
   referencial y regresiones del dataset).
E. Plan de distribución y mantenimiento: instaladores por SO, versionado del dataset
   independiente del ejecutable, actualización de fuentes, políticas de deprecaciones y
   auditorías periódicas del corpus.
F. Backlog técnico priorizado (épicas → historias con criterios de aceptación) derivado de
   los hitos D0–D7 del plan aprobado, con dependencias y estimación relativa.
G. Riesgos técnicos de la fase de implementación y su mitigación.

FORMATO DE LA RESPUESTA
Devuelve primero el plan detallado de arquitectura y UX/UI (A y B), después la
especificación de módulos y pruebas (C y D), después distribución y backlog (E y F),
y termina con riesgos (G). NO escribas todavía el código de la aplicación completa:
entrega el plan listo para convertirse en backlog técnico ejecutable.

REGLAS DE CONDUCTA
- Separa especificación de implementación y de configuración operacional.
- Marca toda decisión con su justificación y sus alternativas descartadas.
- No asumas accesos a Internet en runtime; el conocimiento se consulta offline.
- Usa fechas absolutas para vigencia y versionado.
```

---

## 18. Fuentes de referencia verificadas

Fuentes consultadas para fundamentar esta propuesta (v. referencia: 26-08-2026). El pipeline de investigación (F1, F3) debe ampliarlas y registrar versiones/fechas de consulta.

| # | Fuente | Uso en el plan |
|---|---|---|
| [R1] | IANA — Service Name and Transport Protocol Port Number Registry — https://www.iana.org/assignments/service-names-port-numbers | Registro oficial fuente de datos (F3); advertencia puerto ≠ uso legítimo (secciones 2, 9) |
| [R2] | RFC Editor — RFC 9114 HTTP/3 — https://www.rfc-editor.org/info/rfc9114/ | Ejemplo de estándar IETF actual (HTTP/3 sobre QUIC) |
| [R3] | Wireshark Developer’s Guide — https://www.wireshark.org/docs/wsdg_html/ | Referencia conceptual de disección por capas, reensamblado, conversaciones y taps (secciones 11, 13.2) |
| [R4] | NIST SP 800-207 — Zero Trust Architecture — https://csrc.nist.gov/pubs/sp/800/207/final | Marco Zero Trust (secciones 8-F6, 10) |
| [R5] | MITRE ATT&CK — https://attack.mitre.org/ | Base de conocimiento defensivo complementaria (sección 10) |
| [R6] | DLA ASSIST — MIL-STD-188 — https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=35582 | Estándar activo de comunicaciones tácticas (doc. 05-06-2026) |
| [R7] | DLA ASSIST — MIL-STD-2045 — https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=117743 | Transferencia de datos sin conexión para C4I |
| [R8] | DLA ASSIST — MIL-STD-6020 — https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=215906 | Interoperabilidad/forwarding entre Tactical Data Links |
| [R9] | Avalonia Docs — https://docs.avaloniaui.net/docs/get-started/ | Framework .NET cross-platform con XAML y renderer propio |
| [R10] | Avalonia Supported Platforms — https://docs.avaloniaui.net/docs/supported-platforms | Plataformas soportadas y requisitos de .NET |
| [R11] | Electron Docs — https://www.electronjs.org/docs/latest/ | Framework basado en Chromium + Node.js (comparativa, sección 12) |

---

## 19. Apéndice A — Reglas de conducta del modelo de investigación

Reglas que el pipeline de investigación (humano + IA) debe cumplir en todo momento:

1. No inventar estándares.
2. No atribuir una especificación a una organización incorrecta.
3. No confundir protocolo con servicio o aplicación.
4. No confundir puerto con protocolo.
5. No confundir una implementación con el estándar.
6. No atribuir a OSI una exactitud que no tenga en todas las arquitecturas reales.
7. Identificar las diferencias entre especificación y práctica de campo.
8. Marcar incertidumbre explícitamente.
9. Usar fechas absolutas al describir vigencia.
10. Cuando una especificación haya cambiado, registrar la versión concreta.
11. Separar hechos, inferencias y recomendaciones.
12. No cerrar el universo de protocolos basándose en una sola fuente.
13. Preferir datos estructurados y regenerables frente a texto duplicado.
14. Pensar la aplicación como un sistema de conocimiento actualizable durante años.

---

## 20. Apéndice B — Glosario de unidades de datos (PDU)

Vocabulario controlado para nombrar el objeto transmitido de cada protocolo (nunca "paquete" como genérico):

| Término | Significado orientativo |
|---|---|
| **Trama (frame)** | Unidad de la capa de enlace (p. ej. Ethernet, Wi-Fi), incluye cabecera de enlace y trailer. |
| **Paquete (packet)** | Unidad de la capa de red (p. ej. IPv4/IPv6). |
| **Datagrama** | Unidad de servicio sin conexión (p. ej. UDP, IP clásico). |
| **Segmento** | Unidad de un transporte orientado a conexión (p. ej. TCP). |
| **Celda (cell)** | Unidad de longitud fija (p. ej. ATM, 53 bytes). |
| **Símbolo** | Unidad física de señalización (capa física). |
| **Flujo (stream/flow)** | Secuencia continua de bytes o tramas con contexto de conversación. |
| **Registro (record)** | Unidad estructurada dentro de un flujo (p. ej. DNS RR, TLS record). |
| **Mensaje (message)** | Unidad semántica de aplicación/señalización (p. ej. SIP, HTTP). |
| **PDU / SDU / ADU** | Protocol/Service/Application Data Unit según la capa. |
| **Objeto semántico** | Unidad significativa de la aplicación (p. ej. CoAP resource, OPC UA node). |
| **Elemento de información** | Bloque con tipo-tag (p. ej. IEs en GSM/LTE, TLVs de DHCP). |
| **TLV** | Estructura Tipo-Longitud-Valor. |
| **Campo / atributo / etiqueta** | Componentes internos de una unidad mayor. |
| **Encabezado / trailer / payload** | Partes estructurales de una PDU. |

---

*Fin del documento PLANREDES.md — versión 1.0. Registros vivos citados sujetos a cambio; el plan de investigación (F1/F3) gestiona su versionado.*