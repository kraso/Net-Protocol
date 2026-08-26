# F6 — Seguridad y Operatividad

**Proyecto:** Plataforma de referencia, exploración y simulación de redes
**Fase:** 6 — Seguridad y operatividad
**Documento rector:** `PLANREDES.md` §8 (F6), §10 · `F0-Politica-de-Fuentes.md` · Fuentes R4 (NIST) y R5 (MITRE)

| Campo | Valor |
|---|---|
| Documento | F6-Seguridad-y-Operatividad.md |
| Versión | 1.0 (aprobada) |
| Fecha | 26-08-2026 |
| Estado | **Aprobada (26-08-2026)** |
| Depende de | F4 (aprobada), F5 (aprobada) |
| Fase siguiente | F7 — Dominios profesionales y especiales (aprobada) · F8 — Validación (en curso) |

---

## 1. Objetivo de la fase

Documentar **amenazas, autenticación, criptografía, hardening, segmentación, monitoring, troubleshooting** y la **relación con marcos de referencia** (NIST SP 800-207 Zero Trust; MITRE ATT&CK como catálogo complementario defensivo). Enfoque **defensivo y arquitectónico** (acuerdo F0 §6 y `F0-Politica-de-Fuentes` §4.3): se documentan propiedades, superficie de ataque y mecanismos; nunca instrucciones operativas clasificadas.

## 2. Modelo `SecurityMechanism` (esquema de datos)

| Campo | Tipo | Regla |
|---|---|---|
| `protocolo_id` | ref PR-xxx | Protocolo al que aplica |
| `mecanismo` | enum | autenticación · cifrado · integridad · anti-replay · intercambio de claves · segmentación · detección · respuesta |
| `descripcion` | string | Cómo funciona el mecanismo |
| `fortaleza_estado` | string | Algoritmos/suites y su vigencia |
| `dependencias_criptograficas` | string | Primitivas y parámetros |
| `amenazas` | string | Amenazas conocidas (sin operativa clasificada) |
| `recomendaciones` | string | Hardening/segmentación/observabilidad |
| `fuente` / `confianza` | string / enum | Nivel 1–3; ALTO/MEDIO/BAJO/DESCONOCIDO |

## 3. Registro de seguridad por protocolo (vista)

Fuente de datos: [`F6-Seguridad-Protocolos.json`](F6-Seguridad-Protocolos.json) (16 protocolos). Resumen:

| Protocolo | Autenticación | Cifrado | Integridad | Anti-replay | Intercambio de claves | Amenazas principales | Recomendaciones |
|---|---|---|---|---|---|---|---|
| TLS 1.3 | Certificados X.509 (mútua opcional) | AEAD (AES-GCM, ChaCha20-Poly1305) | AEAD + MAC | Tickets 0-RTT limitado | ECDHE (PFS) | Suites débiles (versiones antiguas), validación de certificado | TLS 1.3/1.2+, suites fuertes, HSTS, pinning de confianza |
| IPsec/IKEv2 | Pre-shared / certificados / EAP | ESP AEAD | ESP (AEAD) | Ventana de anti-replay (RFC 4303) | IKEv2 (RFC 7296) | Malas config, IKE por UDP 500/4500 | IKEv2, SA lifetimes, evitar mode transport innecesario |
| SSH | Password / claves PKI | ChaCha20-Poly1305, AES-CTR+HMAC | MAC | Contador en canal | Diffie-Hellman/ECDH | MITM si no se validan host keys, brute force | Host keys verificadas, claves fuertes, fail2ban |
| WireGuard | Claves de par (sin auth de usuario) | ChaCha20-Poly1305 | AEAD | Ventana 2^32-1 | Noise IK (X25519) | Falta de gestión de claves, sin auth multifactor | Gestión de claves, firewalls, documentación oficial |
| Kerberos | Tickets TGT/TGS, PKINIT opcional | Sesión (derivada) | Checksum de tickets | Timestamps | AS-REP (derivación) | Kerberoasting, golden/silver tickets, T1078/T1558 | Actualizar claves de service accounts, monitorizar AS-REP |
| 802.1X | EAP (métodos variados) | Según método EAP | Según método | Según método | Según método (EAP-TLS…) | Fallo de NAC, rogue APs | 802.1X-2020, RADIUS backend, santidad de puertos |
| DNSSEC | Firma de RRs (cadena desde raíz) | **no** (no confidencialidad) | RRSIG | NSEC/NSEC3 (existencia) | DS/KSK/ZSK | NSEC walking (mitigado NSEC3), delegación rota | Validación en resolvers, monitoreo de delegaciones |
| BGP | TCP-AO/MD5 opcional | no | TCP-AO/MD5 | no | no (precompartida) | Hijacking, route leaks, RPKI | RPKI/ROA, filtrado de prefijos, TCP-AO, monitoreo AS_PATH |
| DNS | no (DNSSEC aparte) | DoT/DoH opcional | no | no | no | Cache poisoning, exfiltración (T1071.004) | DNSSEC, Do53→DoT/DoH, monitoreo de consultas |
| DHCP | no | no | no | no | no | Rogue server (MITM), lease starvation | DHCP snooping, opción 82, 802.1X |
| ARP | no | no | no | no | no | ARP poisoning/spoofing (T1557.002) | DHCP snooping, 802.1X, monitoreo de ARP |
| TCP | no | no | checksum (no cripto) | no | no | Spoofing, SYN flood, session hijack | BCP38, stateful firewall, TLS encima |
| UDP | no | no | checksum (IPv4 opcional) | no | no | Spoofing, amplificación | Rate limiting, filtrado por aplicación |
| IPv4 | no | no | checksum de cabecera (no cripto) | no | no | Spoofing de origen, fragmentación | BCP38, filtrado de entrada/salida |
| IPv6 | no | no | no (sin checksum de cabecera) | no | no | Abuso de extension headers, SLAAC | Filtrado de extension headers, RA guard |
| HTTP/3 | TLS 1.3 integrado (QUIC) | TLS 1.3 (todo cifrado) | TLS 1.3 | Consideraciones 0-RTT | QUIC/TLS 1.3 | 0-RTT, retirada de push en RFC 9114 | Configuración QUIC/TLS seguras, monitoreo de streams |

> Detalles por campo en el JSON. Cifrado/DB de configuraciones específicas si no son públicas: `[n.p.d.]` (política F0).

## 4. Mapeo a NIST SP 800-207 (Zero Trust, R4)

| Principio (SP 800-207) | Cómo se documenta en la base de conocimiento |
|---|---|
| Proteger **recursos**, no la ubicación de red | Fichas de seguridad por recurso/protocolo; segmentación por zonas y microsegmentación (VXLAN, 802.1Q) |
| Separación **PDP/PE** (decisión de ejecución) | Marcar en cada mecanismo quién decide (PDP: RADIUS, controlador) y quién ejecuta (PE: switch, FW) |
| Autenticación y autorización **continuas** | 802.1X/EAP, RADIUS, sesiones de corta vida; monitorización continua |
| **Mínimo privilegio** | Recomendaciones de hardening y segmentación por protocolo |
| **Asumir brecha** | Observabilidad, detección (ATT&CK), response/recovery en fichas de operatividad |

**Catálogo machine-readable:** [`F6-Mapeo-NIST-ATTACK.json`](F6-Mapeo-NIST-ATTACK.json).

## 5. Mapeo complementario: MITRE ATT&CK (R5)

Uso **complementario y defensivo** (no sustituye la documentación del protocolo; acuerdo F0 §4.3). **Verificación de IDs completada en F8** (§3 del informe de validación):

| Vector por protocolo | Técnica ATT&CK (verificada) | Detección / monitoreo |
|---|---|---|
| ARP poisoning | T1557 — Adversary-in-the-Middle (.002 ARP Cache Poisoning); T1040 — Network Sniffing | DHCP snooping, 802.1X, tablas ARP estáticas, alertas de duplicados |
| DHCP spoofing | T1557 (MITM) | DHCP snooping, opción 82 |
| DNS exfiltration/poisoning | T1071.004 — DNS (Application Layer Protocol) | Monitoreo de consultas anómalas, DNSSEC, logs de resolver |
| Escaneo de servicios | T1046 — Network Service Discovery | IDS/IPS, firewall stateful |
| Brute force / credenciales | T1110 — Brute Force; T1078 — Valid Accounts | Rate limiting, MFA, monitoreo de eventos de login |
| Kerberos (tickets dorados/plata) | T1558 — Steal or Forge Kerberos Tickets | Monitoreo de AS-REP/Kerberoasting, rotation de claves |
| Cifrado legítimo (túnel) | Sin mapeo directo (cifrado esperado) | Metadatos de conexión, reputación de destinos |
| BGP hijacking | Sin mapeo directo en ATT&CK | RPKI/ROA, monitoreo de AS_PATH, RIPE RIS/ROA freshness |

> Nota: los IDs se verificaron contra `attack.mitre.org` (R5) el 26-08-2026 (ver F8); si la taxonomía cambia, se actualiza el catálogo (datos versionables).

## 6. Operatividad: hardening, segmentación y observabilidad

Reglas transversales (aplican a todas las fichas):

1. **Hardening:** versiones mínimas soportadas, deshabilitación de mecanismos débiles (p. ej. TLS <1.2), gestión de claves (rotación, almacenamiento).
2. **Segmentación:** zonas de confianza (plan §11 diagrama 7), microsegmentación con overlays (VXLAN), NAC (802.1X).
3. **Observabilidad:** cómo reconocer cada protocolo en captura (ficha §15 de protocolo), métricas y logs (syslog, IPFIX/NetFlow), correspondencia con herramientas de análisis.
4. **Troubleshooting:** secuencias y máquinas de estado (F5) como base para diagnóstico determinista.

## 7. Registro de incertidumbres (F6)

| ID | Entidad | Afirmación | Naturaleza | Decisión |
|---|---|---|---|---|
| U-0003 | Suites criptográficas (TLS) | Estado de vigencia de suites antiguas | Fechas de deprecación cambiantes | Marcar `[fecha]`; consultar NIST/IETF por pipeline |
| U-0004 | Protocolos militares | Detalles criptográficos operativos | `[n.p.d.]` | Registrar existencia; sin especulación (política F0 §10) |

## 8. Criterios de salida / aceptación de F6

- [x] Modelo `SecurityMechanism` definido (§2).
- [x] Registro de seguridad por protocolo (16 protocolos) — `F6-Seguridad-Protocolos.json` (JSON válido).
- [x] Mapeo a NIST SP 800-207 documentado (§4) — `F6-Mapeo-NIST-ATTACK.json` (JSON válido).
- [x] Uso complementario de MITRE ATT&CK con **verificación de IDs completada en F8** (§5).
- [x] Reglas de hardening/segmentación/observabilidad (§6) e incertidumbres registradas (§7).
- [x] Aprobación de la fase (sección 9).

## 9. Aprobación

| Rol | Nombre | Fecha | Firma / Visto bueno |
|---|---|---|---|
| Responsable del proyecto | Usuario / responsable del proyecto | 26-08-2026 | ✅ Aprobado |
| Analista de ciberseguridad defensiva | *(por confirmar)* | | |

> **Estado:** la aprobación de F6 habilita **F8 — Validación** (en curso), que verifica los IDs de ATT&CK contra R5.

---
Última actualización: 26-08-2026