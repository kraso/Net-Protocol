# F0 — Política de Incertidumbre

**Fase:** 0 — Definición y límites · **Estado:** ✅ aprobado (F0 cerrada el 26-08-2026)

Fuente: `PLANREDES.md` §9.1, §10 y Apéndice A (reglas de conducta).

---

## 1. Principio

**No se rellenan huecos con invenciones.** La incertidumbre es un dato de primera clase: se registra, se marca y se audita. Un conocimiento honestamente incompleto vale más que un dato falso presentado como verificado.

## 2. Grados de confianza (por afirmación o ficha)

| Grado | Significado | Criterio de asignación |
|---|---|---|
| **ALTO** | Afirmación verificada contra fuente primaria normativa (nivel 1) con versión y fecha | Ficha o campo respaldado por especificación oficial vigente |
| **MEDIO** | Respaldada por fuente primaria de implementación (nivel 2) o secundaria especializada (nivel 3) de calidad | Comportamiento de implementación conocida, white paper serio, academia |
| **BAJO** | Solo fuente terciaria (nivel 4) o fuente antigua sin confirmación reciente | Blog/foro/tutorial; requiere doble comprobación |
| **DESCONOCIDO** | No existe fuente verificable | No asumir nada; marcar explícitamente |

Se registra además **la causa** de la incertidumbre: versión antigua, conflicto entre fuentes, documentación no pública, falta de verificación con captura, etc.

## 3. Marcas estándar en fichas y documentos

| Marca | Significado | Cuándo se usa |
|---|---|---|
| `[n.p.d.]` | **No documentado públicamente** (not publicly documented) | Detalle de wire format ausente de la especificación pública |
| `[no verificable públicamente]` | Existencia/semántica referenciada pero sin fuente pública | Protocolos restringidos, militares cerrados, propietarios |
| `[conflicto: ver registro]` | Fuentes en conflicto; remite al registro de conflictos (§4) | Afirmaciones con fuentes divergentes |
| `[inferencia]` | No es hecho documentado; es razonamiento del investigador | Aparece siempre separado de hechos, nunca como wire format |

Regla: **hechos, inferencias y recomendaciones se separan explícitamente** en toda ficha.

## 4. Registro de incertidumbres y contradicciones

Plantilla del registro (uno por proyecto; se mantiene en `REGISTRO/` desde F3):

| ID | Entidad | Campo/afirmación | Naturaleza (n.p.d. / conflicto / no verificable / inferencia) | Fuentes consultadas (nivel, versiones) | Decisión / estado | Revisado |
|---|---|---|---|---|---|---|
| U-0001 | … | … | … | … | … | |

**Procedimiento de resolución de conflictos entre fuentes:**

1. **Identificar** el conflicto (misma afirmación, fuentes divergentes).
2. **Aplicar jerarquía de evidencia** (nivel 1 > 2 > 3 > 4).
3. **Comparar versión y fecha** de cada fuente; la más reciente de mayor autoridad prevalece si no hay errata conocida.
4. Si el conflicto **persiste**, registrar ambas versiones en la ficha con `[conflicto: ver registro]`, explicar el criterio usado y dejar la decisión documentada en el registro.

## 5. Reglas de fechas y versiones

| Regla | Detalle |
|---|---|
| Fechas absolutas | "RFC 9114 publicado 2022-06-06" — nunca "recientemente" |
| Versión concreta | Cuando una especificación cambia, se registra la versión concreta (RFC N vs. RFC M) |
| Fecha de consulta | Siempre distinta de la fecha de publicación; se registra en cada Source |
| Vigencia en vivo | Los estados (vigente/obsoleto…) se marcan con fecha; los registros vivos pueden cambiar |

## 6. Procedimiento ante lagunas de información

1. Buscar en **fuentes de nivel 1–2** (registro oficial, especificación, implementación de referencia).
2. Si no existe: comprobar **fuentes secundarias/terciarias** — no se usa como base de afirmación crítica.
3. Si sigue sin resolverse: **marcar** según Tabla §3 y **añadir al registro** (§4).
4. **Nunca** inferir wire format; nunca atribuir especificación a organización incorrecta.
5. Las lagunas se revisan en **F8 (Validación)** y en las auditorías periódicas de mantenimiento.

## 7. Criterios de fijación de la política de incertidumbre

- [ ] Grados de confianza (§2) y marcas (§3) aprobados e incorporados a las plantillas de ficha.
- [ ] Plantilla del registro de incertidumbres/contradicciones (§4) aprobada.
- [ ] Procedimiento de conflictos (§4) y de lagunas (§6) aprobados.