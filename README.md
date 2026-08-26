# Plataforma de referencia, exploración y simulación de redes

Aplicación de escritorio profesional, multiplataforma y **local-first** para el conocimiento, la exploración y la representación técnica de redes de comunicaciones: dispositivos, tipos y arquitecturas de red, capas y planos funcionales, protocolos, estándares, mensajes, PDU, formatos de datos, encapsulación, secuencias, seguridad, observabilidad e interoperabilidad.

> Documento rector del proyecto: **[PLANREDES.md](PLANREDES.md)** (Plan Maestro de Investigación, Documentación y Desarrollo, v1.0, 26-08-2026).
> Texto fuente original (extraído del master prompt): [`_prompt_extraido.txt`](_prompt_extraido.txt).

---

## Estado del proyecto

### Fase I — Investigación y documentación (✅ CERRADA el 26-08-2026)

| Fase | Título | Estado |
|---|---|---|
| F0 | Definición y límites | ✅ **Completada** (26-08-2026) |
| F1 | Inventario maestro de autoridades | ✅ **Completada** (26-08-2026) |
| F2 | Universo de dispositivos y redes | ✅ **Completada** (26-08-2026) |
| F3 | Inventario de protocolos | ✅ **Completada** (26-08-2026) · v2: 113 protocolos |
| F4 | Profundización protocolar | ✅ **Completada** (26-08-2026) · 12/12 fichas |
| F5 | Mensajería y PDU | ✅ **Completada** (26-08-2026) |
| F6 | Seguridad y operatividad | ✅ **Completada** (26-08-2026) |
| F7 | Dominios profesionales y especiales | ✅ **Completada** (26-08-2026) |
| F8 | Validación | ✅ **Completada** (26-08-2026) · compuerta de calidad |
| F9 | Especificación de producto | ✅ **Completada** (26-08-2026) · cierre de la Fase I |

### Fase II — Diseño y generación de software (🔶 En curso)

Plan de Fase II generado el 26-08-2026 (arquitectura, UX/UI, módulos, pruebas, distribución, backlog D0–D7). Pendiente de revisión/aprobación; **la programación comienza tras la aprobación y la épica D0 (spikes + ADR)**. Entorno: **VS 2022 Enterprise 17.14 + .NET SDK 9.0.316**.

Detalle: [REGISTRO/Estado-de-Fases.md](REGISTRO/Estado-de-Fases.md) · Historial: [REGISTRO/Bitacora.md](REGISTRO/Bitacora.md)

---

## Estructura del repositorio

```
protocolos/
├── PLANREDES.md                  Plan maestro del proyecto (v1.0)
├── _prompt_extraido.txt          Texto del master prompt original (.docx → texto)
├── README.md                     Este índice
├── FASE-00-DEFINICION/           Fase 0 — Definición y límites (✅)
├── FASE-01-AUTORIDADES/          Fase 1 — Inventario maestro de autoridades (✅)
├── FASE-02-DISPOSITIVOS/         Fase 2 — Universo de dispositivos y redes (✅)
├── FASE-03-INVENTARIO/           Fase 3 — Inventario de protocolos (✅ v2)
├── FASE-04-PROFUNDIZACION/       Fase 4 — Profundización protocolar (✅)
├── FASE-05-MENSAJERIA/           Fase 5 — Mensajería y PDU (✅)
├── FASE-06-SEGURIDAD/            Fase 6 — Seguridad y operatividad (✅)
├── FASE-07-DOMINIOS-ESPECIALES/  Fase 7 — Dominios profesionales y especiales (✅)
├── FASE-08-VALIDACION/           Fase 8 — Validación (✅)
├── FASE-09-PRODUCTO/             Fase 9 — Especificación de producto (✅)
├── FASE-II-DISENO/               Fase II — Diseño y generación de software (en curso)
├── PLANTILLAS/                   Plantillas (ficha de protocolo formalizada en F4)
├── ESQUEMA/                      Esquema de datos y modelo de dominio
└── REGISTRO/                     Estado de fases y bitácora del proyecto
```

---

## Entregables por fase

### Fase 0 — Definición y límites (✅)
[F0-Carta-de-Alcance.md](FASE-00-DEFINICION/F0-Carta-de-Alcance.md) · [F0-Glosario-PDU.md](FASE-00-DEFINICION/F0-Glosario-PDU.md) · [F0-Ejes-de-Clasificacion.md](FASE-00-DEFINICION/F0-Ejes-de-Clasificacion.md) · [F0-Politica-de-Fuentes.md](FASE-00-DEFINICION/F0-Politica-de-Fuentes.md) · [F0-Politica-de-Incertidumbre.md](FASE-00-DEFINICION/F0-Politica-de-Incertidumbre.md) · [F0-Criterios-de-Aceptacion.md](FASE-00-DEFINICION/F0-Criterios-de-Aceptacion.md)

### Fase 1 — Inventario maestro de autoridades (✅)
[F1-Registro-de-Autoridades.md](FASE-01-AUTORIDADES/F1-Registro-de-Autoridades.md) · [F1-Autoridades.json](FASE-01-AUTORIDADES/F1-Autoridades.json)

### Fase 2 — Universo de dispositivos y redes (✅)
[F2-Taxonomia-Dispositivos-y-Redes.md](FASE-02-DISPOSITIVOS/F2-Taxonomia-Dispositivos-y-Redes.md) · [F2-Catalogo-Dispositivos.json](FASE-02-DISPOSITIVOS/F2-Catalogo-Dispositivos.json) · [F2-Catalogo-Redes.json](FASE-02-DISPOSITIVOS/F2-Catalogo-Redes.json)

### Fase 3 — Inventario de protocolos (✅ · v2)
[F3-Inventario-de-Protocolos.md](FASE-03-INVENTARIO/F3-Inventario-de-Protocolos.md) · [F3-Protocolos.json](FASE-03-INVENTARIO/F3-Protocolos.json)

### Fase 4 — Profundización protocolar (✅)
[F4-Profundizacion-Protocolar.md](FASE-04-PROFUNDIZACION/F4-Profundizacion-Protocolar.md) · [F4-Fichas-Prioritarias.md](FASE-04-PROFUNDIZACION/F4-Fichas-Prioritarias.md) · [F4-Matriz-Encapsulacion.json](FASE-04-PROFUNDIZACION/F4-Matriz-Encapsulacion.json)

### Fase 5 — Mensajería y PDU (✅)
[F5-Mensajeria-y-PDU.md](FASE-05-MENSAJERIA/F5-Mensajeria-y-PDU.md) · [F5-Campos-PDU.json](FASE-05-MENSAJERIA/F5-Campos-PDU.json)

### Fase 6 — Seguridad y operatividad (✅)
[F6-Seguridad-y-Operatividad.md](FASE-06-SEGURIDAD/F6-Seguridad-y-Operatividad.md) · [F6-Seguridad-Protocolos.json](FASE-06-SEGURIDAD/F6-Seguridad-Protocolos.json) · [F6-Mapeo-NIST-ATTACK.json](FASE-06-SEGURIDAD/F6-Mapeo-NIST-ATTACK.json)

### Fase 7 — Dominios profesionales y especiales (✅)
[F7-Dominios-Especiales.md](FASE-07-DOMINIOS-ESPECIALES/F7-Dominios-Especiales.md) · [F7-Dominios.json](FASE-07-DOMINIOS-ESPECIALES/F7-Dominios.json)

### Fase 8 — Validación (✅)
[F8-Informe-de-Validacion.md](FASE-08-VALIDACION/F8-Informe-de-Validacion.md) · [F8-Verificaciones.json](FASE-08-VALIDACION/F8-Verificaciones.json) · [F8-Lagunas.json](FASE-08-VALIDACION/F8-Lagunas.json)

### Fase 9 — Especificación de producto (✅ · cierre de la Fase I)
[F9-Especificacion-de-Producto.md](FASE-09-PRODUCTO/F9-Especificacion-de-Producto.md) · [F9-Backlog.json](FASE-09-PRODUCTO/F9-Backlog.json) (épicas D0–D7)

### Fase II — Diseño y generación de software (🔶 En curso)
[F2I-Diseno-de-Software.md](FASE-II-DISENO/F2I-Diseno-de-Software.md) (arquitectura + ADR-001…005, UX/UI, módulos, pruebas, distribución, riesgos) · [F2I-Backlog-Detallado.json](FASE-II-DISENO/F2I-Backlog-Detallado.json) (8 épicas, 24 historias estimadas) · [F2I-Entorno-de-Desarrollo.md](FASE-II-DISENO/F2I-Entorno-de-Desarrollo.md) (VS 2022 Enterprise 17.14, .NET 9.0.316, git) · [F2I-D0-Spikes-y-ADR.md](FASE-II-DISENO/F2I-D0-Spikes-y-ADR.md) (épica D0 completada: Avalonia 12.1.1/net9.0 confirmados, determinismo del renderer demostrado)

### Plantillas
[PLANTILLAS/README.md](PLANTILLAS/README.md) · [plantilla-ficha-protocolo.md](PLANTILLAS/plantilla-ficha-protocolo.md) (formalizada en F4)

---

## Reglas de convivencia del repositorio

1. **Trazabilidad:** toda afirmación técnica importante remite a una fuente (nivel 1–4) con URL/versión/fecha de consulta (Política de Fuentes).
2. **No inventar:** lo no documentado públicamente se marca explícitamente (Política de Incertidumbre).
3. **Datos regenerables:** los catálogos y diagramas se generan desde datos estructurados; nunca a mano en cientos de fichas.
4. **Versionado:** cada documento registra versión, fecha y estado; los registros vivos (IANA, RFC, MIL-STD) son datos versionables.
---

Estado repo: publicado en GitHub (CI activo). 26-08-2026.
