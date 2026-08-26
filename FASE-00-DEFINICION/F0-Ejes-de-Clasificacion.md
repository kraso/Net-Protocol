# F0 — Ejes de Clasificación y Ciclo de Vida

**Fase:** 0 — Definición y límites · **Estado:** ✅ aprobado (F0 cerrada el 26-08-2026)

Fuente: `PLANREDES.md` §3.2 (ejes A–I), §5.1 (familias) y principio P3 (ciclo de vida).

---

## 1. Ejes de clasificación (nueve ejes simultáneos)

Ningún objeto se clasifica por un único eje; todos los ejes aplicables se registran en la ficha.

| Eje | Descripción | Valores controlados (iniciales) |
|---|---|---|
| **A. Modelo por capas** | OSI, TCP/IP, híbridos | OSI L1–L7; TCP/IP: Acceso/Internet/Transporte/Aplicación (modelo 4 capas) y variantes de 5 capas; híbrido |
| **B. Plano funcional** | Función dentro de la red | Datos · Control · Gestión · Seguridad · Sincronización/timing · Señalización · Orquestación |
| **C. Dominio** | Contexto de uso | Enterprise · ISP/carrier · Data center · Cloud · Industrial/OT · IoT · Telecom móvil · Radiocomunicaciones · Satélite · Vehicular · Investigación/académico · Defensa/táctico |
| **D. Medio** | Medio físico | Cobre · Fibra · Radio · Microondas · Satélite · Infrarrojo · Acústico · Otros |
| **E. Alcance** | Extensión geográfica/lógica | PAN · LAN · WLAN · CAN · MAN · WAN · Internet · Interdominio · Intercontinental · Federado |
| **F. Administración** | Modelo de control | Centralizada · Distribuida · SDN/controller-based · Ad-hoc · Mesh · Federada · Peer-to-peer |
| **G. Estado del estándar** | Madurez normativa | Standard · Proposed Standard · Internet Draft · Informational · Experimental · Obsoleto · Propietario · Vendor-specific · Military/Public Standard · Histórico |
| **H. Perspectiva temporal** | Momento en el tiempo | Actuales · Antecesoras · Transición · Deprecadas · Emergentes |
| **I. Perspectiva de seguridad** | Propiedades de seguridad | Autenticación · Autorización · Confidencialidad · Integridad · Disponibilidad · Anti-replay · Intercambio de claves · Modelo de confianza · Segmentación · Observabilidad · Respuesta |

> Los valores controlados son **iniciales**; ampliables por catálogo en fases posteriores (F2, F3) sin romper los ejes ya fijados.

## 2. Familias funcionales de protocolos (agrupación primaria del inventario)

1. Acceso y enlace (Ethernet/802.3, Wi-Fi/802.11, PPP, L2TP, VLAN/802.1Q, LAG, STP/RSTP/MSTP, control de acceso al medio).
2. Direccionamiento, descubrimiento y configuración (IPv4/IPv6, ARP/NDP, DNS, DHCP, mDNS/LLMNR, zeroconf).
3. Routing y forwarding (RIP, OSPF, IS-IS, BGP, EIGRP, multicast IGMP/PIM, MPLS, segment routing).
4. Movilidad (Mobile IP, handover 3GPP, roaming 802.11, LISP).
5. Transporte y sesión (TCP, UDP, SCTP, DCCP, QUIC, TLS/DTLS, RTP/RTCP, IPsec transporte).
6. Aplicación (HTTP/1.x–2–3, SMTP, FTP, SSH, DNS sobre transporte, NFS/SMB/CIFS, SIP, XMPP).
7. Gestión, monitorización y operaciones (SNMP, NETCONF/YANG, gRPC/telemetría, syslog, IPFIX/NetFlow, RADIUS, TACACS+, ICMP).
8. Sincronización temporal (NTP, PTP/IEEE 1588).
9. Almacenamiento/red y automatización (iSCSI, FC/FCoE, NVMe-oF, SDN/OpenFlow).
10. Seguridad (IPsec/IKE, TLS/DTLS, Kerberos, RADIUS/EAP, 802.1X, DNSSEC, GRE/VXLAN/WireGuard).
11. IoT/OT y tiempo real (MQTT, CoAP, Modbus, DNP3, PROFINET, EtherCAT, OPC UA, LoRaWAN, Zigbee, BACnet).
12. Radio/móvil y satélite (GSM/UMTS/LTE/5G NR, TETRA, DMR, DVB/VSAT, enlaces de datos tácticos).
13. Históricos y de transición (X.25, Frame Relay, ATM, Token Ring, FDDI, NetBIOS/NetBEUI, IPX/SPX, AppleTalk, ARCNET, SONET/SDH, ISDN).

## 3. Estados de ciclo de vida (por elemento)

Cada elemento (protocolo, estándar, dispositivo, red…) declara **un** estado vigente con fecha y fuente.

| Estado | Definición | Ejemplo ilustrativo |
|---|---|---|
| **Vigente** | En uso activo y soportado por su autoridad/registro | TCP/IP, HTTP/3 |
| **Actualizado** | Versión vigente posterior que cambia el elemento base (se registra la versión concreta) | IPv6 vs. IPv4 |
| **Obsoleto** | Superado formalmente; puede seguir operando | IPX/SPX |
| **Sustituido** | Reemplazado por otro elemento conocido | FTP→SFTP (parcial), X.25→Frame Relay→ATM |
| **Experimental** | Sin estatus normativo definitivo | Internet Drafts experimentales |
| **Propietario** | De una organización sin especificación pública completa | protocolos vendor-specific |
| **Restringido** | Existencia verificable, documentación no pública | estándares militares cerrados |
| **Histórico** | Sin uso activo, relevancia documental | Token Ring, NetBEUI |
| **Desconocido** | No se ha podido verificar estado | — |

### 3.1. Transiciones típicas

```
Experimental → Proposed Standard → Standard → Actualizado → Obsoleto / Sustituido → Histórico
      │                │                              │
      └── Retirado (no publicado)                     └── Propietario / Restringido (paralelo)
```

Toda transición se registra en la ficha con **fecha absoluta + fuente** (p. ej. RFC que deprecia un mecanismo).

## 4. Correspondencia OSI ↔ TCP/IP (orientativa, no dogmática)

| OSI (ISO 7498) | TCP/IP (4 capas) | TCP/IP (5 capas, frecuente) | Planos funcionales típicos |
|---|---|---|---|
| 7 Aplicación | Aplicación | Aplicación | Datos/Control/Gestión |
| 6 Presentación | Aplicación | Aplicación | — |
| 5 Sesión | Aplicación | Aplicación | Señalización |
| 4 Transporte | Transporte | Transporte | Datos |
| 3 Red | Internet | Red | Datos/Control |
| 2 Enlace | Acceso | Enlace | Datos/Control |
| 1 Física | Acceso | Física | Sincronización |

> **Advertencia (regla del proyecto):** la correspondencia es **orientativa**. Muchos protocolos no encajan limpiamente (p. ej. TLS actúa entre Transporte y Aplicación; QUIC fusiona transporte+seguridad). La ficha registra capas OSI/TCP-IP **y** planos funcionales por separado.

## 5. Criterios de fijación de los ejes

Los ejes y el ciclo de vida se consideran **fijados** cuando:

- [ ] Los 9 ejes tienen valores controlados aprobados (Tabla §1).
- [ ] Las 13 familias funcionales están aprobadas y no hay solapamiento ambiguo conocido.
- [ ] Los 9 estados de ciclo de vida con sus transiciones están aprobados.
- [ ] Se realizó una prueba de clasificación sobre 5+ objetos de ejemplo (protocolos y dispositivos de distintas familias).