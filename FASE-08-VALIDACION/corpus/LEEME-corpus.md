# Corpus de capturas reales (L-004)

Capturas **reales** descargadas del repositorio oficial de Wireshark
(`test/captures`), usadas para validar los layouts F5 contra tráfico real.

Fuente: https://gitlab.com/wireshark/wireshark/-/raw/master/test/captures/<archivo>

| Archivo | Bytes | Protocolos que aporta |
|---|---|---|
| arp.pcap | 70 | (ARP; fuera de F5) |
| coap-eap-failure.pcap | 2799 | sin Ethernet/IPv4 estándar (capa de enlace distinta) |
| dhcp.pcap | 1400 | DHCP ✓ (100 %) |
| dns-ooo.pcap | 418 | DNS sobre TCP puerto 53 ✓ (100 %) |
| dns_port.pcap | 1318 | DNS sobre puertos NO estándar — limitación documentada |
| gitOverTCP.pcap | 582 | TCP ✓ |
| http.pcap | 247 | TCP ✓ |
| http2_follow_multistream.pcapng | 247716 | HTTP/2 (no detectado: TLS oculta el prefacio) |
| icmp_ascii.pcapng | 1184 | ICMP ✓ (100 %) |
| ipv6.pcap | 126 | IPv6 ✓ (100 %) |
| ntp.pcap | 130 | NTP ✓ (100 %) |

Fecha de descarga: 2026-08-27. Cada archivo se valida con
`NetProtocol.exe --l004 <carpeta> [salida]` → `L004-informe-YYYY-MM-DD.md`.