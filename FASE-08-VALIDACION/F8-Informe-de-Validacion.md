# F8 — Informe de Validación

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 8 — Validación
**Documento rector:** `PLANREDES.md` §8 (F8), §9 (matriz de calidad) · Política de fuentes e incertidumbre (F0)

| Campo | Valor |
|---|---|
| Documento | F8-Informe-de-Validacion.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 (fecha de las verificaciones: 26-08-2026) |
| Estado | **Aprobada (26-08-2026)** |
| Depende de | F3–F7 (aprobadas) |
| Fase siguiente | F9 — Especificación de producto (en curso) |

---

## 1. Objetivo y método

Comprobar la **consistencia, integridad y trazabilidad** de los entregables F0–F7 antes de pasar a la especificación de producto (plan §8 F8): revisión cruzada de fuentes, consistencia de nomenclatura, validación con fuentes/registros reales, comprobación de versiones e identificación de lagunas. Método: verificaciones automatizadas ejecutables (resultados reales en `F8-Verificaciones.json`) + revisión manual + registro de lagunas (`F8-Lagunas.json`).

## 2. Verificaciones automatizadas (resultados ejecutados el 26-08-2026)

| ID | Verificación | Resultado | Detalle |
|---|---|---|---|
| V-01 | Parseo JSON de todos los catálogos | ✅ OK | 9/9 archivos JSON válidos en el momento de la ejecución (11/11 tras añadir los catálogos de F8) |
| V-02 | Unicidad de IDs `PR-xxx` en F3 | ✅ OK | 113 IDs únicos tras v2 |
| V-03 | Integridad referencial F5 → F3 | ✅ OK | 6/6 protocolos existen |
| V-04 | Integridad referencial F6 → F3 | ✅ OK | 16/16 protocolos existen |
| V-05 | Integridad referencial F7 → F3 | ✅ OK | Todas las referencias existen |
| V-06 | Familias declaradas vs. usadas (F3) | ✅ OK | Ninguna familia no declarada |
| V-07 | Estados de ciclo de vida vs. vocabulario F0 | ✅ OK (corregido) | Se normalizó `historico` → `histórico` (12 entradas) |
| V-08 | Enlaces relativos del repositorio (.md) | ✅ OK | Sin enlaces rotos internos |
| V-09 | **Verificación operativa IANA (pipeline)** | ✅ OK | Fetch real: 15.402 filas (≈15.401 registros), 1.156.686 bytes; cabecera mapeada; muestras `ssh/22` (tcp, udp, sctp) y `domain/53` (tcp, udp). Estudio complementario: **7.683 service names únicos**, 6.606 filas tcp / 6.356 udp, 1.724 rangos no asignados |
| V-10 | Verificación de IDs MITRE ATT&CK contra R5 | ⚠️ ATENCION | Servicio de búsqueda web no disponible (saldo insuficiente). IDs de alta confianza por conocimiento de la taxonomía; confirmación formal registrada como **L-003** |

## 3. Revisión de fuentes y versiones

| Fuente | Estado revisado (26-08-2026) |
|---|---|
| R1 IANA — Service Names & Ports | ✅ Verificación operativa real (V-09); registro vivo descargado y mapeado |
| R2 RFC 9114 (HTTP/3) | ✅ Usada en ficha F-09 |
| R3 Wireshark Developer's Guide | ✅ Referencia conceptual de disección (F5 §4) |
| R4 NIST SP 800-207 | ✅ Principios usados en F6 §4 |
| R5 MITRE ATT&CK | ⚠️ Sin confirmación en línea en esta corrida (V-10) → L-003 |
| R6–R8 DLA ASSIST (MIL-STD-188/2045/6020) | ✅ Usadas en F7 §3.2 (v. doc 05-06-2026 según plan) |
| R9–R11 Avalonia/Electron docs | ✅ Usadas en plan (decisión tecnológica F9) |

**Regla aplicada:** puerto ≠ protocolo (V-09 mapea el registro sin inferir); norma ≠ implementación (fichas distinguen niveles); incertidumbre marcada (`[n.p.d.]`).

## 4. Cierre de tareas registradas (F2–F7)

| # | Tarea | Estado |
|---|---|---|
| T1 | 3+ fichas piloto por clase de dispositivo (F2) | ✅ **Cerrada (parcial documentado)**: `F2-Catalogo-Dispositivos.json` ampliado a **34 fichas piloto** (FP-001…FP-034): 12 clases prioritarias ≥3; 10 restantes ≥2 — laguna menor L-002 (mantenimiento) |
| T2 | Verificación operativa del pipeline IANA (F3) | ✅ **Cerrada**: fetch real OK (V-09) con mapeo y muestras verificadas |
| T3 | Fichas OSPF y Ethernet/802.3 completas (F4) | ✅ **Cerrada**: F-11 (OSPF) y F-12 (Ethernet) en `F4-Fichas-Prioritarias.md` → **12/12 fichas prioritarias** |
| T4 | Validación de layouts contra capturas (F5) | ⚠️ **Parcial**: procedimiento definido y layout TCP validado contra catálogo de campos; sin corpus PCAP → L-004 (fase de producto, hito D6) |
| T5 | Verificación de IDs MITRE ATT&CK (F6) | ⚠️ **Pendiente formal**: sin acceso a búsqueda web (V-10) → L-003 |
| T6 | Incorporación V2X al inventario F3 v2 (F7) | ✅ **Cerrada**: `PR-112` (ITS-G5) y `PR-113` (C-V2X); DOM-08 → "semilla" |

## 5. Registro de lagunas clasificadas

Vista resumida (detalle en [`F8-Lagunas.json`](F8-Lagunas.json)):

| ID | Laguna | Tipo | Severidad | Estado |
|---|---|---|---|---|
| L-001 | Wire formats no públicos (propietarios/militares) | `[n.p.d.]` | Baja | Marcado; sin especulación |
| L-002 | Fichas de dispositivo: ≥3 en clases no prioritarias | Cobertura | Baja | Mantenimiento continuo |
| L-003 | Confirmación formal de IDs ATT&CK contra R5 | Verificación | Baja | Pendiente (pipeline) |
| L-004 | Corpus de capturas PCAP para validar layouts | Validación | Media | Pendiente (fase de producto D6) |
| L-005 | Asignación de `Source` (nivel 1) a las fichas | Pipeline | Media | Pendiente (pipeline/fase producto) |

## 6. Métricas de cobertura (tras F8)

| Métrica | Valor |
|---|---|
| Fichas prioritarias completas (F4) | **12/12 (100 %)** |
| Clases de dispositivo con fichas (F2) | 22/22 (≥1); **12 prioritarias ≥3** |
| Tipos de red (F2) | 16/16 |
| Inventario de protocolos (F3 v2) | **113** (13 familias) |
| Dominios especiales (F7) | 10/10 con cobertura semilla o superior |
| % fichas con fuente primaria | 0 % asignadas por pipeline (L-005; las fichas referencian RFC/estándar con fecha de consulta) |
| % con wire format documentado | 6 protocolos catalogados en `F5-Campos-PDU.json` |
| Catálogos machine-readable | 9/9 JSON válidos en ejecución; 11/11 en el repositorio tras F8 |
| Enlaces internos del repositorio | 0 rotos |

## 7. Consistencia de nomenclatura (control)

- Vocabulario de unidades de datos (glosario F0) aplicado en F5 §2, sin "paquete" genérico.
- Estados de ciclo de vida normalizados (V-07).
- IDs cruzados entre catálogos verificados (V-03…V-05).
- Convención `PR-XXX` / `DEV-XX` / `NET-XX` / `DOM-XX` / `FP-XXX` con unicidad verificada.

## 8. Criterios de salida / aceptación de F8

- [x] Verificaciones automatizadas ejecutadas con resultados reales — `F8-Verificaciones.json`.
- [x] Revisión cruzada de fuentes y versiones (§3).
- [x] Tareas registradas F2–F7 cerradas o clasificadas (§4).
- [x] Lagunas clasificadas con severidad y estado — `F8-Lagunas.json`.
- [x] Contradicciones resueltas o registradas (normalización de estados; puerto ≠ protocolo respetado).
- [x] Métricas de cobertura actualizadas (§6).
- [x] **Aprobación de la compuerta de calidad (sección 9)** → desbloquea F9.

## 9. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de conocimiento | *(por confirmar)* | | |

> **Estado:** la compuerta de calidad de F8 queda **superada**; la fase de investigación continúa con **F9 — Especificación de producto** (en curso).

---
Última actualización: 26-08-2026