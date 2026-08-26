# Registro de estado de fases

Control de progreso del plan maestro (`PLANREDES.md`, sección 8). Cada fase produce al menos uno de: **documento, catálogo, esquema de datos y/o criterios de aceptación**.

## Tabla de estado

| Fase | Título | Estado | Documentos / entregables | Depende de | Observaciones |
|---|---|---|---|---|---|
| **F0** | Definición y límites | ✅ **Completada** | Carta de alcance, glosario PDU, ejes de clasificación, política de fuentes, política de incertidumbre, criterios de aceptación | — | **Aprobada el 26-08-2026** (ver F0-Criterios-de-Aceptacion) |
| **F1** | Inventario maestro de autoridades | ✅ **Completada** | F1-Registro-de-Autoridades.md; F1-Autoridades.json | F0 | **Aprobada el 26-08-2026** |
| **F2** | Universo de dispositivos y redes | ✅ **Completada** | F2-Taxonomia-Dispositivos-y-Redes.md; F2-Catalogo-Dispositivos.json; F2-Catalogo-Redes.json | F0 | **Aprobada el 26-08-2026** (tarea registrada: 3+ fichas por clase en F8) |
| **F3** | Inventario de protocolos | ✅ **Completada** | F3-Inventario-de-Protocolos.md; F3-Protocolos.json | F0, F1, F2 | **Aprobada el 26-08-2026** (tarea registrada: verificación operativa pipeline IANA en F8) |
| **F4** | Profundización protocolar | ✅ **Completada** | F4-Profundizacion-Protocolar.md; F4-Fichas-Prioritarias.md; F4-Matriz-Encapsulacion.json | F3, F2 | **Aprobada el 26-08-2026** (tarea registrada: fichas OSPF/Ethernet en F8) |
| **F5** | Mensajería y PDU | ✅ **Completada** | F5-Mensajeria-y-PDU.md; F5-Campos-PDU.json | F4 | **Aprobada el 26-08-2026** (tarea registrada: validación layouts vs. capturas en F8) |
| **F6** | Seguridad y operatividad | ✅ **Completada** | F6-Seguridad-y-Operatividad.md; F6-Seguridad-Protocolos.json; F6-Mapeo-NIST-ATTACK.json | F4 | **Aprobada el 26-08-2026** (verificación de IDs ATT&CK en F8) |
| **F7** | Dominios profesionales y especiales | ✅ **Completada** | F7-Dominios-Especiales.md; F7-Dominios.json | F1, F4 | **Aprobada el 26-08-2026** (V2X incorporado en F3 v2 durante F8) |
| **F8** | Validación | ✅ **Completada** | F8-Informe-de-Validacion.md; F8-Lagunas.json; F8-Verificaciones.json | F3–F7 | **Aprobada el 26-08-2026** (compuerta de calidad superada) |
| **F9** | Especificación de producto | ✅ **Completada** | F9-Especificacion-de-Producto.md; F9-Backlog.json | F0–F8 | **Aprobada el 26-08-2026** — cierra la **Fase I (investigación y documentación)** |
| — | **Fase II — Diseño y generación de software** | ✅ **Iteración inicial completada** | F2I-Diseno-de-Software.md; F2I-Backlog-Detallado.json; F2I-Entorno-de-Desarrollo.md; F2I-D0-Spikes-y-ADR.md; …; F2I-D7-Calidad-Distribucion-Release.md | F0–F9 | **D0–D7 completadas el 26-08-2026** · solución 5 proyectos, **61/61 tests**; CI activo (quality + build self-contained x3 SO + instaladores); **Release oficial v1.0.0 publicado** con instaladores Windows (Inno `.exe`), Linux (`.deb` + `.rpm`) y macOS (`.dmg`). Pendientes (no bloqueantes): **firma de código/instaladores** (certificados), pulido 1.1 (PNG, vista captura, panel grafo, dedup fina) |

## Leyenda de estado

- ⚪ Pendiente — no iniciada.
- 🔶 En curso — documentos generados y/o en revisión.
- ✅ Completada — criterios de salida cumplidos y aprobados.

## Regla de avance

Una fase se considera **completada** solo cuando sus criterios de salida están cumplidos y registrados en la bitácora con fecha. No se inicia F_n+1 que dependa de F_n sin aprobación de la fase anterior.

---
Última actualización: 26-08-2026