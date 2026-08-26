# PLANTILLA — Ficha Normalizada de Protocolo

**Uso:** plantilla canónica para la profundización protocolar (F4) y el pipeline de fichas. Obligatoria como mínimo (18 campos); ampliable cuando el protocolo lo requiera. Normas de llenado según `FASE-00-DEFINICION/F0-Politica-de-Fuentes.md` y `F0-Politica-de-Incertidumbre.md`.

| Campo | Tipo | Obligatorio | Normas |
|---|---|---|---|
| **1. Identidad** | texto | ✅ | Nombre completo, acrónimo, aliases, familia, organización/autoridad, referencia (RFC / ISO / IEEE / 3GPP / ETSI / ITU / MIL-STD / STANAG u otra) |
| **2. Estado** | enum + fecha + fuente | ✅ | vigente · actualizado · obsoleto · sustituido · experimental · propietario · restringido · histórico · desconocido — con fecha absoluta y fuente |
| **3. Finalidad** | texto | ✅ | Problema que resuelve, actores, casos de uso y **cuándo NO** debe utilizarse |
| **4. Encapsulación** | relación | ✅ | Protocolo inmediatamente inferior y superior; dependencias y tunneling (matriz `F4-Matriz-Encapsulacion.json`) |
| **5. Capas** | enum | ✅ | OSI (1–7), TCP/IP y plano funcional (datos / control / gestión / seguridad / sincronización / señalización / orquestación) |
| **6. Transporte y direccionamiento** | lista | ✅ | TCP/UDP/SCTP/DCCP/QUIC u otro; puertos, EtherTypes, IP protocol numbers, next-header values, identificadores de servicio. **Puerto ≠ protocolo** (marcar "registrado en IANA" vs. "uso real verificado") |
| **7. PDU y objeto transmitido** | texto | ✅ | Nombre técnico exacto (trama/paquete/datagrama/segmento/…), estructura, longitud, framing, endianness, codificación, alineamiento, MTU/MSS/tamaño cuando aplique |
| **8. Mensajes** | lista | ✅ | Tipos, propósito, dirección, condiciones de emisión, respuestas, temporizadores, estados y errores |
| **9. Campos** | lista | ✅ | Nombre, offset, longitud, tipo, semántica, valores permitidos, flags, obligatoriedad, compatibilidad y seguridad (catálogo `F5-Campos-PDU.json`) |
| **10. Secuencia** | texto | ✅ | Establecimiento, negociación, operación normal, actualización, cierre, excepciones y recuperación |
| **11. Addressing/naming** | texto | ✅ | Tipos y ámbito de direcciones, identificadores, nombres, IDs, etiquetas y resolución |
| **12. Routing/forwarding/discovery** | texto | * | Mecanismos y algoritmos si están definidos (✱ si aplica) |
| **13. Seguridad** | texto | ✅ | Mecanismos nativos, opciones, dependencias criptográficas, amenazas conocidas, recomendaciones de configuración |
| **14. QoS y rendimiento** | texto | * | Latencia, jitter, pérdida, congestión, retransmisión, prioridad, escalabilidad (✱ si aplica) |
| **15. Observabilidad** | texto | ✅ | Cómo reconocerlo en una captura; campos visibles; filtros; indicadores y métricas |
| **16. Interoperabilidad** | texto | ✅ | Perfiles, extensiones, diferencias de implementación, problemas conocidos |
| **17. Implementaciones** | lista | ✅ | SO, librerías, fabricantes, appliances, herramientas públicas — distinguiendo **"soporta"** de **"implementa completamente"** |
| **18. Fuentes y evidencia** | lista | ✅ | Especificación primaria, versión, fecha de consulta, sección/página, nivel de autoridad, grado de confianza |

## Reglas de llenado

1. **Jerarquía de evidencia:** afirmaciones críticas (campos, valores, wire format) respaldadas por fuente nivel 1; comportamiento por nivel 2; contexto por 3–4.
2. **No inventar:** un detalle ausente de la especificación se marca `[n.p.d.]` (no documentado públicamente); sin fuente → `[no verificable públicamente]`.
3. **Hechos / inferencias / recomendaciones** separados explícitamente.
4. **Fechas absolutas** y **versión concreta** (RFC 9293 y no "TCP reciente").
5. **Unidad de datos exacta** (glosario `F0-Glosario-PDU.md`); prohibido "paquete" genérico.
6. Catálogos machine-readable (`F3-Protocolos.json`, `F4-Matriz-Encapsulacion.json`, `F5-Campos-PDU.json`) son la **fuente** de las vistas; la ficha es una vista estructurada regenerable.

## Criterios de completitud (mínimo para considerar la ficha "completa")

- 18 campos rellenados o marcados explícitamente (`[n.p.d.]` / no aplica).
- ≥ 1 fuente nivel 1 en los campos críticos.
- Fecha de consulta registrada.
- Campos enlazados al catálogo de campos de F5 cuando existan.

---
Versión: 0.1 · 26-08-2026