# F0 — Política de Fuentes y Evidencia

**Fase:** 0 — Definición y límites · **Estado:** ✅ aprobado (F0 cerrada el 26-08-2026)

Fuente: `PLANREDES.md` §9 (matriz de calidad, evidencia y trazabilidad) y §10 (política militar/pública).

---

## 1. Jerarquía de evidencia

Toda afirmación técnica importante se clasifica por nivel; **la fuente primaria prevalece sobre el resumen de terceros cuando existe**.

| Nivel | Tipo de fuente | Uso permitido |
|---|---|---|
| **1 — Primaria normativa** | RFC, estándar ISO/IEC, IEEE, ITU-T, 3GPP, ETSI, IETF draft/working group, registro oficial (IANA…), MIL-STD público, documentación normativa de la autoridad competente | Base de afirmaciones críticas: wire format, semántica de campos, valores, números de puerto. |
| **2 — Primaria de implementación** | Documentación oficial de fabricante/proyecto, código fuente, repositorios mantenidos por el proyecto, manuales técnicos | Comportamiento real, "soporta" vs. "implementa", divergencias especificación↔práctica. |
| **3 — Secundaria especializada** | Libros técnicos, white papers de calidad, artículos académicos, documentación de ingeniería reconocida | Contexto, explicación, comparativas. Nunca base única de un detalle crítico. |
| **4 — Terciaria** | Blogs, foros, tutoriales, resúmenes | Apoyo y contraste. Nunca fuente única de una afirmación importante. |

## 2. Campos obligatorios del registro de fuente

Cada `Source` registra:

| Campo | Obligatorio | Notas |
|---|---|---|
| URL/URI o identificador | ✅ | |
| Organismo/autoridad | ✅ | IETF, IANA, IEEE, ISO, 3GPP, DLA… |
| Versión del documento | ✅ | Número de RFC/estándar/revisión |
| Fecha de publicación | ✅ | Fecha absoluta |
| Fecha de consulta | ✅ | Distinta de la publicación |
| Sección/página relevante | Cuando sea posible | |
| Nivel de autoridad | ✅ | 1–4 (Tabla §1) |
| Grado de confianza | ✅ | Alto / Medio / Bajo / Desconocido (Política de Incertidumbre) |

## 3. Reglas de evidencia y trazabilidad

1. **Priorizar fuentes primarias normativas** sobre resúmenes de terceros cuando existan.
2. **Diferenciar norma, implementación, tutorial y opinión** en el registro de fuente.
3. **Conflictos entre fuentes:** se registra el conflicto, se explica qué prevalece y por qué (procedimiento en Política de Incertidumbre §4).
4. **No rellenar huecos con invenciones:** si la especificación no publica un detalle de wire format, se marca "no documentado públicamente".
5. **Puerto ≠ protocolo:** un puerto registrado en IANA no demuestra que el tráfico corresponda al servicio registrado (advertencia explícita de IANA). La ficha distingue "registrado en IANA" de "uso real verificado".
6. **Protocolo ≠ servicio:** la ficha del protocolo no se contamina con el servicio/aplicación que lo usa.
7. **Estándar ≠ implementación:** "Soporta el estándar" no equivale a "implementa completamente el estándar"; ambos hechos se registran por separado.
8. **Registros vivos como datos:** IANA (Service Name and Transport Protocol Port Number Registry) y similares se sincronizan como **fuente de datos versionable** vía pipeline (F3); nunca se copian a mano ni se fijan en el ejecutable.

## 4. Política específica: fuentes militares, gubernamentales y propietarias

### 4.1. Clase de fuente "Military/Public Standard"

Se crea esta clase con campos: **organismo, publicación, estado y fecha**. Repositorios preferentes de estándares estadounidenses públicos: **DLA ASSIST / QuickSearch**.

| Referencia | Descripción | Fuente |
|---|---|---|
| MIL-STD-188 | Diseño/ingeniería de comunicaciones tácticas (activo; doc. 05-06-2026) | https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=35582 |
| MIL-STD-2045 | Transferencia de datos sin conexión para intercambio digital en redes tácticas/C4I | https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=117743 |
| MIL-STD-6020 | Interoperabilidad y forwarding entre Tactical Data Links | https://quicksearch.dla.mil/qsDocDetails.aspx?ident_number=215906 |

### 4.2. Clases de tratamiento de material militar/profesional

| Clase | Tratamiento |
|---|---|
| **Estándar público** | Documentar con fuente primaria (organismo, publicación, estado, fecha). |
| **Existencia referenciada, detalles no públicos** | Registrar nombre/ámbito verificables; declarar "documentación pública insuficiente". No rellenar huecos. |
| **Información histórica** | Contexto, interoperabilidad y estructuras públicamente divulgadas; sin reproducir procedimientos clasificados. |
| **Restringido / no verificable públicamente** | Registrar la limitación y marcar el dato como no verificable públicamente. |

### 4.3. Reglas

- La cobertura militar es amplia en lo **histórico y técnico**, limitada a información **legalmente pública y verificable**.
- **MITRE ATT&CK** se usa como complemento para modelar tácticas, técnicas y detecciones defensivas; **nunca** como sustituto de la documentación del protocolo.
- Está prohibido inventar formatos o detalles operativos no publicados.

## 5. Sincronización de registros vivos (estrategia)

| Registro | Frecuencia sugerida | Método |
|---|---|---|
| IANA Service Names & Ports | Mensual / bajo demanda | Descarga oficial, normalización, versionado en el pipeline |
| Índices RFC (RFC Editor) | Mensual | Datatracker / index oficial |
| DLA ASSIST (MIL-STD) | Trimestral / bajo demanda | QuickSearch (verificación estado/fecha) |
| Otros (IEEE, 3GPP, ETSI…) | Según pipeline de la Fase 1 | Catálogos oficiales |

Cada sincronización genera un **snapshot versionado** (hash + fecha + procedencia) para auditoría y rollback.

## 6. Criterios de fijación de la política de fuentes

- [ ] La jerarquía de evidencia (§1) y los campos del registro de fuente (§2) están aprobados.
- [ ] La política militar/pública (§4) está aprobada y es aplicable en plantillas de ficha.
- [ ] La estrategia de sincronización (§5) está aceptada como requisito del pipeline (F3).