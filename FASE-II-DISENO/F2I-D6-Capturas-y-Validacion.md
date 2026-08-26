# F2I-D6 — Capturas y Validación de Layouts

**Fase II — Épica D6 (Capturas y validación de layouts)**
**Documento rector:** `F2I-Diseno-de-Software.md` §C (MOD-07) · `F2I-Backlog-Detallado.json` (D6-1…D6-2) · Cierra la **laguna L-004** de la Fase I · Resultados **reales** del 26-08-2026.

| Campo | Valor |
|---|---|
| Documento | F2I-D6-Capturas-y-Validacion.md |
| Versión | 1.0 |
| Fecha | 26-08-2026 |
| Estado | ✅ Completada |

---

## 1. Resumen

| Hito | Resultado |
|---|---|
| **D6-1 — Adaptador PCAP/PCAPNG** | ✅ Lector propio (sin dependencias): **PCAP clásico** (ambos endianness) y **PCAPNG** (SHB/IDB/EPB, detección de endianness, bloques desconocidos omitidos); **dissection por capas** Ethernet→IPv4→TCP con campos reales |
| **D6-2 — Validación de layouts (L-004)** | ✅ Correspondencia **paquete real ↔ campos de `F5-Campos-PDU.json`**: TCP 10/10 en límites (Source Port `C000`, Dest `0050`, Seq `00000001`, Window `2000`), IPv4 (Protocol `06`, src `C0000201`), Ethernet con semántica de preámbulo (base 64) |
| **Calidad** | ✅ **52/52 pruebas** (7 nuevas de D6) |

## 2. Módulo (`Infrastructure/Capturas/`)

```
Capturas/
├── PcapCapture.cs         PcapPacket · PcapCapture · CampoDefinido
├── PcapCaptureReader.cs   PCAP clásico (magic 0xA1B2C3D4 ambos endianness) + PCAPNG (SHB/IDB/EPB)
└── PcapDissector.cs       Disectar (Ethernet→IPv4→TCP, IPs/puertos) · Validar (vs F5) · Resumen
```

**Diseño:** datos de prueba **construidos programáticamente** a partir de la matemática de los campos (un paquete Ethernet+IPv4+TCP de 62 bytes con 192.0.2.1 → 203.0.113.2, puertos 49152→80, SYN|ACK), serializados en **PCAP clásico** y **PCAPNG** reales para validar el parser de extremo a extremo.

## 3. Verificaciones (D6-2 / L-004)

| Layout | Resultado |
|---|---|
| **TCP** (F5, 10 campos con longitud) | `10/10` dentro de límites (OK) · hex verificados campo a campo |
| **IPv4** (F5, 13 campos) | `13/13` OK · `Protocol=06`, `Source Address=C0000201`, `EtherType=0800` |
| **Ethernet** (F5, base preámbulo) | Semántica honesta: `Preamble` **fuera de límites** (la captura no lo incluye, base=64) · `Destination MAC=001122334455` ✓ · `EtherType=0800` ✓ |
| **Dissection** | Ethernet→IPv4(proto 6)→TCP; IPs y puertos correctos; truncado inválido → `InvalidDataException` |

**Laguna L-004 (Fase I) cerrada:** el procedimiento "paquete real ↔ campos documentados" quedó implementado y probado; cuando haya corpus de capturas reales (Wireshark/tcpdump) basta ejecutar el mismo validador (D7/explotación).

## 4. Incidencias reales resueltas

1. `Assert.Equal(int, ushort?)` sin sobrecarga en xUnit → comparación con `.Value` casteada a `int`.
2. El fixture PCAPNG escribía el **BOM en big-endian** (`1A2B3C4D` como bytes); un archivo LE lleva bytes `4D 3C 2B 1A` → corregido (lectura `U32 LE == 0x1A2B3C4D` ✓, igual que los archivos reales).
3. Aviso de nombres de tupla en un test de D5 → limpiado (build 0 avisos).

## 5. Resultados de pruebas (reales)

```
dotnet test → Con error: 0, Superado: 52, Total: 52, Duración: 1 s
```

Nuevos de D6 (`CapturaTests`, 7): lectura PCAP clásico · lectura PCAPNG · cabecera inválida → lanza · dissección TCP completa · **validación layout TCP/F5** · **layout Ethernet (base preámbulo)** · **layout IPv4**.

## 6. Criterios de salida de D6

- [x] D6-1 adaptador PCAP **y PCAPNG** con aperture, listado y dissection por capas (referencia conceptual Wireshark, sin embebido).
- [x] D6-2 **validación de layouts contra F5** (L-004) con resumen de coincidencia por protocolo.
- [x] Capturas de prueba reales (clásico y NG) generadas por fixtures deterministas.
- [x] Pruebas 52/52 sin avisos de compilación.
- [~] Vista de captura en la UI de la app: se integra en **D7** (pantalla de observabilidad) usando este adaptador — no bloqueante.

## 7. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Arquitecto de software de escritorio | *(por confirmar)* | | |

> **Siguiente:** épica **D7 — Calidad, distribución y Release** (CI/CD + controles automáticos, instaladores por SO, modo offline y actualización de dataset) — última épica de la Fase II.

---
Última actualización: 26-08-2026