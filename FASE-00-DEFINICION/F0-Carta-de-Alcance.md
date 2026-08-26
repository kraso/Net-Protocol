# F0 — Carta de Alcance

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 0 — Definición y límites
**Documento rector:** `PLANREDES.md` v1.0 (26-08-2026)

| Campo | Valor |
|---|---|
| Documento | F0-Carta-de-Alcance.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 |
| Estado | **Aprobada (26-08-2026)** |
| Aprobado por | Responsable del proyecto (usuario) — 26-08-2026 |
| Fase siguiente | F1 — Inventario maestro de autoridades (se desbloquea al aprobar F0) |

---

## 1. Objetivo del proyecto

Construir una **aplicación de escritorio profesional, multiplataforma y de uso principalmente local** que actúe como plataforma de conocimiento, exploración y representación técnica de redes de comunicaciones. Cubrirá dispositivos de red, tipos y arquitecturas de redes, capas y planos funcionales, protocolos, estándares, mensajes, PDU, formatos de datos, mecanismos de encapsulación, secuencias de comunicación, seguridad, observabilidad e interoperabilidad — con trazabilidad a fuentes primarias y arquitectura de datos actualizable durante años.

## 2. Objetivo de la Fase 0

Cerrar las **decisiones conceptuales de partida** que condicionan todas las fases siguientes, sin programar nada:

1. Fijar **objetivo, audiencia y profundidad** del conocimiento.
2. Fijar el **alcance y el no-alcance** del universo a investigar.
3. Fijar la **nomenclatura** y el **glosario de unidades de datos** (glosario PDU).
4. Fijar los **ejes de clasificación** y los **estados de ciclo de vida**.
5. Fijar la **política de fuentes y evidencia** (incluida la política militar/pública).
6. Fijar la **política de incertidumbre**.
7. Definir los **criterios de aceptación de la fase**.

## 3. Audiencia y usuarios objetivo

| Perfil | Necesidad principal | Profundidad esperada |
|---|---|---|
| **P1. Ingeniero/a de redes y operaciones (NOC/SOC)** | Consulta rápida de protocolos, puertos, campos, troubleshooting y reconocimiento en capturas | N1–N2 (técnico y detalle completo) |
| **P2. Arquitecto/a de redes** | Visión transversal: capas, planos, dominios, encapsulación, comparativas y matrices | N1–N2 |
| **P3. Analista de ciberseguridad defensiva** | Propiedades de seguridad, amenazas, hardening, segmentación, mapeo NIST ZTA / MITRE ATT&CK | N1–N2 (seguridad ampliada) |
| **P4. Investigador/a y estudiante** | Contexto histórico, familias, estándares, estándar vs. implementación | N0–N1 (educativo y técnico) |
| **P5. Desarrollador/a de herramientas de red** | Wire formats binarios/textuales, máquinas de estado, secuencias, interoperabilidad | N2 (detalle completo) + N3 informativo |

## 4. Profundidad y niveles de detalle

| Nivel | Nombre | Qué incluye |
|---|---|---|
| **N0** | Educativo | Qué es, para qué sirve, cuándo usar y cuándo NO, actores, casos de uso. |
| **N1** | Técnico | Campos principales, mensajes, secuencias, puertos/identificadores, seguridad básica, observabilidad. |
| **N2** | Detalle completo | Wire format (bytes/bits) cuando es público, máquinas de estado, temporizadores, errores y recuperación, interoperabilidad. |
| **N3** | Implementación/operación | Configuración, hardening, perfiles de implementación, diferencias especificación↔práctica. **Informativo** (la app documenta, no sustituye guías operativas). |

**Regla:** los protocolos prioritarios deben alcanzar N2; el resto al menos N1; N3 solo cuando la fuente primaria lo permita y separando claramente "norma" de "práctica de campo".

## 5. Alcance del universo de conocimiento

### 5.1. Qué SÍ cubre (ámbito inicial)

- **Redes:** LAN, WAN, MAN, PAN, WLAN, WWAN, redes móviles, centros de datos, Internet, industrial/OT, IoT, vehiculares, satélite, radio y redes tácticas (solo material público).
- **Dispositivos:** hosts, NIC, repetidores, hubs, bridges, switches L2/L3, routers, gateways, firewalls, IDS/IPS, proxies, balanceadores, AP, controladores inalámbricos, modems, transceptores, concentradores, servidores de infraestructura, appliances de seguridad, SD-WAN/SDN, elementos móviles y equipos especializados.
- **Protocolos:** acceso/enlace, direccionamiento, descubrimiento, configuración, routing, multicast, movilidad, transporte, sesión, aplicación, gestión, monitorización, telemetría, sincronización, almacenamiento, seguridad, IoT/OT, radio/móvil e históricos.
- **Mensajes y objetos:** todas las unidades de datos según el Glosario PDU (`F0-Glosario-PDU.md`); nunca "paquete" como genérico.
- **Capas y planos:** OSI/ISO 7498, TCP/IP, modelos híbridos y planos funcionales (datos, control, gestión, seguridad, sincronización, señalización, orquestación).
- **Seguridad y observabilidad:** propiedades de seguridad por protocolo, reconocimiento en capturas (PCAP/PCAPNG), enlace paquete→ficha.
- **Comparativas y matrices:** dependencias, encapsulación, interoperabilidad, cobertura.

### 5.2. Qué NO cubre (no-alcance)

1. **Documentación clasificada** ni procedimientos operativos no públicos (militar, gubernamental o corporativo).
2. **Detalles de wire format no publicados** (se marcan como "documentación pública insuficiente"; nunca se infieren).
3. **Claim de "todos los protocolos del mundo"** como lista cerrada: la exhaustividad es cobertura medible por registros.
4. **Reemplazo de herramientas de análisis en vivo** (la app documenta y explora; no es un sniffer de producción — capturas solo como entrada de datos).
5. **Dependencia de Internet en runtime** para consultar el conocimiento instalado (local-first; las actualizaciones del corpus son pipeline, no uso diario).
6. **Copias manuales de registros oficiales** (IANA y similares se sincronizan como datos versionables).
7. **Embeber Wireshark**: se toma su modelo de disección por capas como referencia conceptual, sin integrarlo como dependencia.

## 6. Principios rectores del proyecto

Vinculantes en todas las fases (detalle en `PLANREDES.md` §2):

1. No es una enciclopedia estática ni un catálogo de puertos.
2. "Exhaustivo" = máxima cobertura verificable mediante registros y fuentes.
3. Ciclo de vida explícito en cada elemento (9 estados posibles).
4. Separación epistemológica: protocolo ≠ estándar ≠ implementación ≠ servicio ≠ formato ≠ algoritmo ≠ transporte ≠ interfaz ≠ tecnología física.
5. OSI y TCP/IP coexisten con los planos funcionales.
6. Los diagramas explican comportamiento y flujo, no solo iconos.
7. Toda afirmación técnica con procedencia; la fuente primaria prevalece.
8. No inventar datos; la incertidumbre se marca.
9. Datos estructurados y regenerables antes que texto duplicado.
10. La aplicación es un sistema de conocimiento actualizable durante años.

## 7. Supuestos y restricciones

| # | Supuesto / restricción |
|---|---|
| S1 | Fecha de referencia del proyecto: **26 de agosto de 2026**. Los registros vivos (IANA, RFC, MIL-STD…) son **datos versionables** y pueden cambiar. |
| S2 | Se usan **fechas absolutas** al describir vigencia; cada versión de especificación se registra con su número concreto. |
| S3 | El conocimiento se entrega **offline**; la sincronización de fuentes es un proceso de mantenimiento (pipeline), no del uso diario. |
| S4 | Stack de partida (a validar en F9): C#/.NET + Avalonia + SQLite/FTS5, renderer de diagramas desacoplado y exportación SVG. |
| S5 | Cada fase produce documento, catálogo, esquema de datos y/o criterios de aceptación (según `PLANREDES.md` §8). |
| S6 | No se inicia la programación antes de cerrar F9. |

## 8. Entregables de la Fase 0

| Documento | Contenido |
|---|---|
| F0-Carta-de-Alcance.md | Este documento |
| F0-Glosario-PDU.md | Nomenclatura y vocabulario de unidades de datos |
| F0-Ejes-de-Clasificacion.md | Ejes de clasificación y estados de ciclo de vida |
| F0-Politica-de-Fuentes.md | Jerarquía de evidencia y trazabilidad |
| F0-Politica-de-Incertidumbre.md | Grados de confianza y registro de conflictos |
| F0-Criterios-de-Aceptacion.md | Criterios de salida y checklist de aceptación |

## 9. Criterios de salida de la Fase 0

- [x] Documento de alcance redactado y **aprobado** (sección 11).
- [ ] Glosario de PDU y reglas de nomenclatura **fijados**.
- [ ] Ejes de clasificación y estados de ciclo de vida **fijados**.
- [ ] Política de fuentes **fijada** (incl. política militar/pública).
- [ ] Política de incertidumbre **fijada**.
- [ ] Checklist C1–C9 de `PLANREDES.md` §16 evaluado sobre los entregables F0 (`F0-Criterios-de-Aceptacion.md`).

## 10. Revisión de la fase (guía para el revisor)

Solicitud de revisión al responsable:

1. Validar los **perfiles de audiencia** y los **niveles de profundidad** (§3, §4).
2. Confirmar **alcance y no-alcance** (§5) — especialmente lo militar/público y lo "no cubierto".
3. Confirmar **supuestos** (§7), en particular el stack de partida (se cerrará en F9, no ahora).
4. Firmar la aprobación (§11) e informar desbloqueo de la Fase 1.

## 11. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de conocimiento | *(por confirmar)* | | |

> **Estado:** La aprobación de esta carta desbloquea **Fase 1 — Inventario maestro de autoridades** (en curso desde el 26-08-2026) y la formalización del esquema de datos (`ESQUEMA/README.md`).