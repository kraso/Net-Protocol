# F0 — Criterios de Aceptación de la Fase

**Fase:** 0 — Definición y límites · **Estado:** ✅ aprobado (F0 cerrada el 26-08-2026)

---

## 1. Criterios de salida de la Fase 0 (del plan maestro §8)

| # | Criterio | Evidencia | Estado |
|---|---|---|---|
| S1 | Documento de alcance redactado | `F0-Carta-de-Alcance.md` | ✅ |
| S2 | Documento de alcance **aprobado** | Tabla de aprobación de la Carta de Alcance | ✅ Aprobado 26-08-2026 |
| S3 | Glosario de PDU y nomenclatura fijados | `F0-Glosario-PDU.md` §5 | ✅ |
| S4 | Ejes de clasificación y ciclo de vida fijados | `F0-Ejes-de-Clasificacion.md` §5 | ✅ |
| S5 | Política de fuentes fijada (incl. militar/pública) | `F0-Politica-de-Fuentes.md` §6 | ✅ |
| S6 | Política de incertidumbre fijada | `F0-Politica-de-Incertidumbre.md` §7 | ✅ |
| S7 | Checklist C1–C9 del plan evaluado | Sección 2 de este documento | ✅ |

## 2. Evaluación de los criterios de aceptación del plan (C1–C9, `PLANREDES.md` §16)

| # | Criterio del plan | Evaluación en F0 | Evidencia |
|---|---|---|---|
| C1 | No comienza por lista arbitraria de protocolos; el universo sale de registros | ✅ Cumplido: el universo se deriva de autoridades/registros (F1) y familias funcionales | F0-Carta §5, F0-Politica-Fuentes §5 |
| C2 | Estrategia de sincronización con registros y versionado | ✅ Cumplido (política definida; pipeline en F1/F3) | F0-Politica-Fuentes §5, Estado-de-Fases |
| C3 | Esquema de ficha que admite protocolos muy distintos | ⚠️ Parcial: el esquema de ficha se formaliza en F4 (ficha mínima ya definida en PLANREDES §6.2) | PLANREDES §6.2 |
| C4 | Evidencia suficiente e incertidumbre explícita | ✅ Cumplido | F0-Politica-Incertidumbre |
| C5 | Matriz para localizar capas/planos/dispositivos/mensajes | ⚠️ Parcial: ejes fijados; matrices en F2–F5 | F0-Ejes §1–§2 |
| C6 | Representación comunicación extremo a extremo con encapsulación | ⚠️ Parcial: requisitos de diagramación definidos; implementación en F4–F5 | PLANREDES §11 |
| C7 | Distingue educativo / implementación / operación | ✅ Cumplido (niveles N0–N3) | F0-Carta §4 |
| C8 | Plan de mantenimiento | ⚠️ Parcial: política de sincronización y auditorías definidas; plan operativo completo en F9/distribución | F0-Politica-Fuentes §5 |
| C9 | Termina en especificación lista para backlog, sin programar | ✅ Cumplido (compuertas F9; no se programa antes) | F0-Carta §7-S6, PLANREDES §16.C9 |

> Leyenda: ✅ cumplido en F0 · ⚠️ parcial (se completa en fases posteriores, ya enrutado) · ⬜ pendiente.
> **Ningún criterio impide aprobar F0**: los "parciales" son obligaciones enrutadas a F1–F9 y no bloquean la fase.

## 3. Verificación de coherencia interna de la Fase 0

| Verificación | Estado |
|---|---|
| Todos los documentos F0 referencian `PLANREDES.md` como documento rector | ✅ |
| La nomenclatura (F0-Glosario) es consistente con los ejes (F0-Ejes) | ✅ |
| La jerarquía de evidencia (F0-Politica-Fuentes §1) es consistente con los grados de confianza (F0-Politica-Incertidumbre §2) | ✅ |
| El alcance/no-alcance (F0-Carta §5) incorpora la política militar/pública | ✅ |
| Los criterios de salida (S1–S7) son verificables y están registrados | ✅ |

## 4. Proceso de aprobación y desbloqueo

1. **Revisión por el responsable** de los documentos F0 (guía de revisión en Carta de Alcance §10).
2. **Firma de la aprobación** en la Carta de Alcance (§11) y registro en `REGISTRO/Bitacora.md`.
3. **Actualización** de `REGISTRO/Estado-de-Fases.md` (F0 → ✅ Completada).
4. **Desbloqueo de Fase 1:** inventario maestro de autoridades (org. + registros + política de sincronización) y formalización del esquema de datos (`ESQUEMA/README.md`).

## 5. Cierre

La Fase 0 fue **aprobada el 26-08-2026** (S2 firmado en la Carta de Alcance §11). Queda registrada como **completada** en `REGISTRO/Estado-de-Fases.md`; su cierre desbloquea la **Fase 1 — Inventario maestro de autoridades** (en curso).

---
Última actualización: 26-08-2026