# F2 — Taxonomía de Dispositivos y Tipos de Red

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 2 — Universo de dispositivos y redes
**Documento rector:** `PLANREDES.md` §5.3, §5.4 y §8 (F2) · `F0-Ejes-de-Clasificacion.md`

| Campo | Valor |
|---|---|
| Documento | F2-Taxonomia-Dispositivos-y-Redes.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 |
| Estado | **Aprobada (26-08-2026)** |
| Depende de | F0 (aprobada), F1 (aprobada) |
| Fase siguiente | F3 — Inventario de protocolos (aprobada) · F4 — Profundización protocolar (en curso) |

---

## 1. Objetivo de la fase

Construir la **taxonomía completa de dispositivos, arquitecturas, topologías, medios, tecnologías de acceso y escenarios** (plan §8 F2), con catálogo machine-readable y fichas piloto. Cada clase de dispositivo documenta: propósito, capa(s), plano(s), interfaces, medios, dirección del flujo, PDU que procesa, funciones de forwarding/control/management, tablas o estados internos, dependencia de protocolos, ejemplos de implementación, escenarios de uso y limitaciones.

## 2. Taxonomía de dispositivos (22 clases)

| ID | Clase | Propósito | Capas | Plano(s) | PDU procesada | Ejemplos típicos | Limitaciones |
|---|---|---|---|---|---|---|---|
| DEV-01 | Host / endpoint | Terminal o servidor final | 5–7 (pila completa) | Datos | Mensaje / ADU | PCs, servidores, móviles | Depende de la pila completa |
| DEV-02 | NIC | Interfaz de red física | 1–2 | Datos | Trama | NIC Ethernet, adaptadores Wi-Fi | Solo capas 1–2 |
| DEV-03 | Repetidor | Regenera señal | 1 | Datos | Símbolo / bit | Repetidores (históricos) | No segmenta dominios |
| DEV-04 | Hub | Difusión capa 1 (histórico) | 1 | Datos | Símbolo / bit | Hubs 10/100 | Dominio de colisión único |
| DEV-05 | Bridge | Segmentación capa 2 básica | 2 | Datos | Trama | Bridges de 2 puertos | Pocos puertos, sin enrutado |
| DEV-06 | Switch L2 | Conmutación de tramas por MAC | 2 | Datos / Control | Trama | Catalyst, acceso/agregación | Sin función de capa 3 |
| DEV-07 | Switch L3 | Conmutación + encaminamiento | 2–3 | Datos / Control | Trama / Paquete | Nexus, switches core | Más coste/complejidad |
| DEV-08 | Router | Encaminamiento entre redes | 3 | Datos / Control / Gestión | Paquete | ASR/ISR, MX | Requiere planificación de ruteo |
| DEV-09 | Gateway | Traducción entre dominios/protocolos | 3–7 | Datos / Control | Paquete / Mensaje | Gateways VoIP, IoT, tácticos | Específico del dominio |
| DEV-10 | Firewall | Filtrado y control de tráfico | 3–4 (hasta 7) | Seguridad | Paquete / Flujo | FortiGate, PAN, ASA | Latencia/perfomance según inspección |
| DEV-11 | IDS/IPS | Detección / prevención de intrusos | 3–7 | Seguridad | Paquete / Flujo | Snort/Suricata, appliances | Falsos positivos, capacidad |
| DEV-12 | Proxy | Intermediario de aplicación | 4–7 (o 3) | Seguridad | Mensaje / Flujo | Squid, proxies corporativos | Punto de fallo único (redundancia) |
| DEV-13 | Balanceador | Distribución de carga | 4–7 (L4/L7) | Datos | Flujo / Mensaje | F5, HAProxy, NGINX | Configuración de sesiones |
| DEV-14 | Controlador inalámbrico | Gestión central de APs | 2 (control) | Control / Gestión | Trama (control) | Cisco WLC, Aruba | SSID/densidad limitada |
| DEV-15 | AP (Access Point) | Acceso inalámbrico | 1–2 | Datos / Control | Trama 802.11 | APs empresariales | Cobertura e interferencia |
| DEV-16 | Modem / transceptor | Modulación/demodulación | 1 | Datos | Símbolo | Módems DSL, SFP/transceptores | Solo capa física |
| DEV-17 | Concentrador | Agregación de enlaces/líneas | 1–2 | Datos | Trama | DSLAM, CMTS | Específico del acceso |
| DEV-18 | Servidor de infraestructura | Servicios base (DNS/DHCP/NTP…) | 5–7 | Datos / Gestión | Mensaje | Servidores DNS/DHCP/AD/NTP | Disponibilidad crítica |
| DEV-19 | Appliance de seguridad | Seguridad especializada | 3–7 | Seguridad | Paquete / Flujo | WAF, SIEM appliances | Alcance acotado |
| DEV-20 | SD-WAN/SDN | Control centralizado y overlays | 2–7 | Control / Orquestación | Flujo / Overlay | SD-WAN edge, controladores SDN | Dependencia del controlador |
| DEV-21 | Elemento de red móvil | Nodos de red celular | 1–3 (hasta 7) | Datos / Control | Celda / Paquete | eNodeB/gNB, MME/AMF, EPC | Dominio 3GPP |
| DEV-22 | Equipo especializado | OT / satélite / radio táctico | 1–3 | Datos / Control | Trama / Símbolo | PLC, routers satelitales, radios tácticas | Documentación pública limitada |

## 3. Plantilla de ficha de dispositivo (atributos obligatorios)

Cada ficha de dispositivo (para el catálogo y la app) declara: **id (URN) · clase · propósito · capas · planos · interfaces · medios · dirección del flujo · PDU que procesa · funciones (forwarding/control/management) · tablas o estados internos · dependencia de protocolos · ejemplos de implementación (con fuente, "soporta" ≠ "implementa") · escenarios de uso · limitaciones · fuentes y fecha de consulta.**

> La plantilla formal (reutilizable en el pipeline) se consolidará en `PLANTILLAS/plantilla-dispositivo.md` durante la fase de fichas; los campos quedan fijados aquí.

## 4. Fichas piloto

Catálogo con **12 fichas piloto** en `F2-Catalogo-Dispositivos.json` (campo `fichas_piloto`). **Entregable inicial aprobado:** 12 fichas cubren las clases prioritarias (≥3 en prioritarias, ≥1 en el resto). El completado de "3+ fichas por clase" para las 22 clases se ejecuta en la **validación F8** con fuentes verificadas (tarea registrada, no bloqueante). Muestras completas en el documento anterior v0.1 (FP-001 Host, FP-002 Switch L2, FP-003 Router, FP-004 Firewall).

## 5. Taxonomía de tipos de red (16 tipos)

| ID | Tipo | Ámbito (eje E) | Topología | Medios | Latencia típica | Movilidad | Protocolos frecuentes | Casos de uso |
|---|---|---|---|---|---|---|---|---|
| NET-01 | PAN | Personal | Estrella / mesh | Radio (BT/IR) | Baja | Alta | Bluetooth, Zigbee, IR | Wearables, periféricos |
| NET-02 | LAN | Edificio/campus | Estrella / jerárquica | Cobre / fibra | Muy baja | Baja | Ethernet, 802.1Q, DHCP, DNS | Oficinas |
| NET-03 | WLAN | Área local inalámbrica | Infraestructura / mesh | Radio | Baja | Alta | 802.11, 802.1X, RADIUS | Movilidad interior |
| NET-04 | CAN | Campus/corporativo | Jerárquica | Fibra / cobre | Baja | Media | OSPF, VLAN, STP | Campus |
| NET-05 | MAN | Metropolitana | Malla / anillo | Fibra, radio/microondas | Media | Media | MPLS, OSPF, QinQ | Interconexión urbana |
| NET-06 | WAN | Interurbana | Malla (core) | Fibra, satélite, microondas | Media–alta | Media | BGP, MPLS, IPsec | Sucursales |
| NET-07 | Internet | Global | Malla de AS | Heterogéneo | Variable | N/A | BGP, DNS, TCP/IP, TLS | Público |
| NET-08 | Data center | Centro de datos | Spine-leaf | Fibra (25/100/400G) | Muy baja | Baja | VXLAN, BGP-EVPN, NVMe-oF, iSCSI | DC |
| NET-09 | Enterprise | Corporativo integral | Multinivel | Mixto | Baja | Media | OSPF/BGP, 802.1X, IPsec | Empresa |
| NET-10 | ISP/carrier | Proveedor | Jerárquica de AS | Fibra | Media | N/A | BGP, MPLS, L2VPN | Conectividad |
| NET-11 | Industrial/OT | Planta/fábrica | Bus / anillo / estrella | Cobre, fibra, radio | Baja (determinista) | Baja | Modbus, PROFINET, EtherCAT, OPC UA | Automatización |
| NET-12 | IoT | Dispositivos embebidos | Estrella / mesh | Radio (LPWAN/802.15.4) | Variable | Media–alta | MQTT, CoAP, LoRaWAN, Zigbee | Sensórica |
| NET-13 | Móvil (WWAN) | Celular | Celular (hex) | Radio | Media | Alta | LTE/5G NR, GTP | Telefonía/datos |
| NET-14 | Satélite | Cobertura global | Estrella (hub-spoke) | Espacio / radio | Alta | Baja | DVB-S2, TCP acelerado | Remoto/marítimo |
| NET-15 | Vehicular (V2X) | Vehículos | Ad-hoc / infraestructura | Radio | Media | Alta | C-V2X, ITS-G5 | Automoción |
| NET-16 | Radio/táctico | Campo de batalla | Malla / ad-hoc | Radio | Variable | Alta | Link 16, MIL-STD-188, IP táctico | Defensa (solo público) |

> Los valores son **indicativos** (catálogo `F2-Catalogo-Redes.json`); el refinamiento con fuentes se ejecuta en F4–F8. Las redes móviles y tácticas se tratan según la política militar/pública de la F0.

## 6. Criterios de salida / aceptación de F2

- [x] Catálogo de dispositivos generado (22 clases con atributos y ejemplos) — `F2-Catalogo-Dispositivos.json`.
- [x] Catálogo de tipos de red generado (16 tipos con atributos) — `F2-Catalogo-Redes.json`.
- [x] Plantilla de ficha de dispositivo fijada (sección 3).
- [x] Fichas piloto: entregable inicial de 12 fichas (clases prioritarias) aprobado; **tarea registrada**: completar 3+ fichas por clase en la validación F8.
- [x] Catálogos JSON válidos y versionados.
- [x] Aprobación de la fase (sección 7).

## 7. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de conocimiento | *(por confirmar)* | | |

> **Estado:** la aprobación de F2 desbloquea su integración con **F3** (inventario de protocolos, aprobada) y **F4 — Profundización protocolar** (en curso).

---
Última actualización: 26-08-2026