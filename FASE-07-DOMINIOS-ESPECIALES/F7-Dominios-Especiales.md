# F7 — Dominios Profesionales y Especiales

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 7 — Dominios profesionales y especiales
**Documento rector:** `PLANREDES.md` §8 (F7), §10 · `F0-Politica-de-Fuentes.md` §4 (política militar/pública)

| Campo | Valor |
|---|---|
| Documento | F7-Dominios-Especiales.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 |
| Estado | **Aprobada (26-08-2026)** |
| Depende de | F1 (aprobada), F4 (aprobada) |
| Fase siguiente | F8 — Validación (en curso) |

---

## 1. Objetivo de la fase

Cubrir los **dominios profesionales y especiales** (OT/ICS, telecom, cloud, data center, IoT, satélite, radio, vehicular y **documentación militar pública**), aplicando la política de la F0: **solo material legalmente público y verificable**; sin invenciones; `[n.p.d.]` donde no haya documentación pública.

## 2. Catálogo de dominios especiales (vista)

Fuente de datos: [`F7-Dominios.json`](F7-Dominios.json).

| ID | Dominio | Ámbito | Protocolos asociados (F3) | Autoridades (F1) | Estado de cobertura |
|---|---|---|---|---|---|
| DOM-01 | Industrial/OT (ICS) | Planta, fábrica, infraestructura crítica | Modbus (PR-084), DNP3 (PR-085), PROFINET (PR-086), EtherCAT (PR-087), OPC UA (PR-088), BACnet (PR-091) | AUTH-010 (industriales), AUTH-016 | Semilla — fichas en ciclos F4 posteriores |
| DOM-02 | Telecom móvil | Redes celulares | GSM (PR-092), UMTS (PR-093), LTE (PR-094), 5G NR (PR-095), GTP (PR-032), TETRA (PR-096) | AUTH-006 (3GPP), AUTH-007 (ETSI) | Semilla — profundización en F4 posterior |
| DOM-03 | Cloud | Infraestructura como servicio | VXLAN (PR-080), BGP (PR-025), TLS (PR-073), RESTCONF (PR-056) | AUTH-010 (ONF) | Semilla |
| DOM-04 | Data center | Centros de datos | iSCSI (PR-067), FC (PR-068), FCoE (PR-069), NVMe-oF (PR-070), VXLAN (PR-080), BGP-EVPN | AUTH-003 (IEEE), AUTH-010 | Semilla |
| DOM-05 | IoT | Dispositivos embebidos | MQTT (PR-082), CoAP (PR-083), LoRaWAN (PR-089), Zigbee (PR-090), Bluetooth | AUTH-010, AUTH-003 (802.15.4) | Semilla |
| DOM-06 | Satélite | Enlaces espaciales | DVB-S2 (PR-098) | AUTH-005 (ITU), AUTH-010 | Semilla |
| DOM-07 | Radio profesional | Radio móvil profesional | TETRA (PR-096), DMR (PR-097) | AUTH-007 | Semilla |
| DOM-08 | Vehicular (V2X) | Automoción y transporte | ITS-G5 (PR-112), C-V2X (PR-113) — incorporados en F3 v2 (F8) | AUTH-003 (802.11p), AUTH-016 | ✅ Cubierto (F8) |
| DOM-09 | Militar/táctico (público) | Defensa — solo documentación pública | Link 16 (PR-099), Link 11 (PR-100) | AUTH-013 (DLA ASSIST) | Semilla — política §3 |
| DOM-10 | Académico/investigación | Universidades, laboratorios | Protocolos experimentales de la semilla | AUTH-016 | Semilla — fuentes secundarias |

## 3. Política militar/pública aplicada (F0 §4 y plan §10)

### 3.1. Clases de tratamiento

| Clase | Aplicación en F7 |
|---|---|
| **Estándar público** | Documentar con fuente primaria: organismo, publicación, estado y fecha |
| **Existencia referenciada, detalles no públicos** | Registrar nombre/ámbito verificables; declarar "documentación pública insuficiente" |
| **Información histórica** | Contexto e interoperabilidad públicamente divulgados; sin procedimientos clasificados |
| **Restringido / no verificable** | Marcar el dato como no verificable públicamente |

### 3.2. Fuentes militares públicas (DLA ASSIST / QuickSearch, R6–R8)

| Estándar | Descripción | Registro (v. 05-06-2026 en plan) |
|---|---|---|
| MIL-STD-188 | Diseño/ingeniería de comunicaciones tácticas (activo) | https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=35582 |
| MIL-STD-2045 | Transferencia de datos sin conexión para C4I | https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=117743 |
| MIL-STD-6020 | Interoperabilidad/forwarding entre Tactical Data Links | https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=215906 |

**Reglas:** solo material legalmente público y verificable; MITRE ATT&CK (R5) como complemento defensivo, nunca sustituye la documentación del protocolo; está prohibido inventar formatos o detalles operativos no publicados.

## 4. Registro de incertidumbres (F7)

| ID | Entidad | Afirmación | Naturaleza | Decisión |
|---|---|---|---|---|
| U-0005 | Link 16 / enlaces tácticos | Detalles de wire format operativo | `[n.p.d.]` | Registrar existencia + marco (MIL-STD-6016 y -6020 orientan); sin especulación |
| U-0006 | Protocolos OT (p. ej. loops de campo) | Configuraciones de fabricante | `[n.p.d.]` parcial | Documentar la norma; marcar variantes de implementación como nivel 2 |
| U-0007 | ITS-G5 / C-V2X | No presentes en semilla F3 | Resuelto en F8 | Incorporados como PR-112/PR-113 en F3 v2 |

## 5. Criterios de salida / aceptación de F7

- [x] Catálogo de dominios especiales (10 dominios) — `F7-Dominios.json` (JSON válido).
- [x] Política militar/pública aplicada (§3) con fuentes DLA ASSIST (R6–R8).
- [x] Registro de incertidumbres ampliado (U-0005…U-0007; U-0007 resuelto en F8).
- [x] Vínculos entre dominios, protocolos (F3) y autoridades (F1) documentados.
- [x] Dominio vehicular (V2X): **incorporado en F3 v2** (PR-112 ITS-G5, PR-113 C-V2X) durante F8.
- [x] Aprobación de la fase (sección 6).

## 6. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de conocimiento | *(por confirmar)* | | |

> **Estado:** la aprobación de F7 habilita **F8 — Validación** (en curso), que consolida el cierre de la cobertura de dominios.

---
Última actualización: 26-08-2026