# PLANTILLAS — Catálogo de plantillas del proyecto

Directorio para las **plantillas reutilizables** que el plan de investigación debe producir (según `PLANREDES.md` §6.3). Se crean a lo largo de las fases y se usan en el pipeline de fichas (regla: datos regenerables, nunca cientos de fichas escritas a mano).

## Plantillas comprometidas

| # | Plantilla | Ficha / matriz | Fase que la formaliza |
|---|---|---|---|
| 1 | Ficha de protocolo | Ficha normalizada de 18 campos (PLANREDES §6.2) | F3/F4 |
| 2 | Ficha de dispositivo | Taxonomía §5.3 del plan | F2 |
| 3 | Ficha de tipo de red | Taxonomía §5.4 del plan | F2 |
| 4 | Ficha de estándar | Organismo, publicación, estado, fecha | F1 |
| 5 | Ficha de mensaje / PDU | Estructura, campos, codificación, ejemplos normativos vs. implementación | F5 |
| 6 | Ficha de campo | Offset, longitud, tipo, valores, flags, obligatoriedad | F5 |
| 7 | Registro de fuente | URL/ID, versión, fechas, sección, nivel, confianza (política F0) | F1 |
| 8 | Matriz de dependencias | Qué necesita qué | F4 |
| 9 | Matriz de encapsulación | Quién va sobre quién; tunneling | F4 |
| 10 | Matriz de interoperabilidad | Perfiles, extensiones, problemas conocidos | F4 |
| 11 | Matriz de cobertura | Métricas de cobertura (§7 del plan) | F3/F8 |
| 12 | Registro de incertidumbres y contradicciones | Plantilla definida en `FASE-00-DEFINICION/F0-Politica-de-Incertidumbre.md` §4 | F0 (ya definida) |
| 13 | Catálogo de diagramas | 10 tipos de diagrama regenerables (§11 del plan) | F4/F5 |

## Reglas de uso

- Toda plantilla declara: **esquema de campos, obligatoriedad, validación y fuentes asociadas**.
- Las plantillas se versionan (versión + fecha) y cualquier cambio se refleja en el pipeline de importación/normalización.
- Una ficha que no cumple la plantilla se rechaza en la validación de esquema (control automático, PLANREDES §9.3).

---
Última actualización: 26-08-2026