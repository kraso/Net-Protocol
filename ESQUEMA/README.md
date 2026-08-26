# ESQUEMA — Modelo de datos del proyecto

Directorio para el **esquema de datos y modelo de dominio** de la base de conocimiento (según `PLANREDES.md` §6.1). Se formaliza a partir de la Fase 1 y se consolida en F3–F5; este índice describe la intención.

## Modelo de dominio objetivo (entidades núcleo)

| Entidad | Responsabilidad |
|---|---|
| `Protocol` | Ficha de protocolo (nombre, acrónimo, aliases, familia, estado, ciclo de vida) |
| `Standard` | Norma de referencia (RFC, ISO/IEC, IEEE, 3GPP, MIL-STD…) |
| `Version` | Versión concreta con vigencia temporal (valid_from / valid_to) |
| `MessageType` | Tipos de mensajes (nombre, propósito, dirección, condiciones de emisión) |
| `Field` | Campos: offset, longitud, tipo, semántica, valores, flags, obligatoriedad |
| `PDU` | Unidad de datos (trama, paquete, datagrama, segmento, celda, TLV…) |
| `Layer` | Modelo de capas (OSI, TCP/IP, híbrido) |
| `Plane` | Plano funcional (datos, control, gestión, seguridad, sincronización, señalización, orquestación) |
| `Device` | Dispositivos y su taxonomía de funciones |
| `NetworkType` | Tipos de red y atributos |
| `AddressingScheme` | Esquemas de direccionamiento/naming |
| `Implementation` | Implementaciones (SO, librerías, fabricantes) |
| `Source` | Fuente: URL/ID, versión, fechas, sección, nivel, confianza |
| `Capture` | Capturas PCAP/PCAPNG y enlace paquete → ficha |
| `Diagram` | Catálogo de diagramas regenerables |
| `SecurityMechanism` | Mecanismos de seguridad |
| `Relationship` | Relaciones tipadas (encapsula, corre-sobre, depende-de, sustituye-a…) |

## Reglas de diseño de datos (aprobadas en el plan)

1. **Claves estables tipo URN** separadas de los nombres mostrados: `urn:proto:ietf:rfc9114`.
2. **Versionado temporal** en todas las entidades (valid_from / valid_to) y autoría.
3. **Trazabilidad:** toda ficha referencia ≥ 1 `Source`; los campos críticos pueden referenciar sección concreta.
4. **Integridad referencial** verificada por CI (sin enlaces a entidades inexistentes).
5. **Separación normativo vs. informativo** (estándar ≠ implementación).

## Entregables previstos en este directorio

| Entregable | Fase |
|---|---|
| JSON Schema / esquema validable del modelo | F1–F3 |
| DDL SQLite (candidato almacén local) + FTS5 | F3/F9 |
| Serialización JSON/YAML de fuentes y fixtures | F3 |
| Validación de esquema en CI (control automático) | F3/F8 |

## Relación con otros documentos

- Ejes de clasificación y ciclo de vida: `FASE-00-DEFINICION/F0-Ejes-de-Clasificacion.md`.
- Vocabulario de unidades de datos: `FASE-00-DEFINICION/F0-Glosario-PDU.md`.
- Registro de fuente: `FASE-00-DEFINICION/F0-Politica-de-Fuentes.md` §2.
- Incertidumbre y conflictos: `FASE-00-DEFINICION/F0-Politica-de-Incertidumbre.md`.

---
Última actualización: 26-08-2026