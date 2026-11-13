# Guía de capturas reales — L-004 / v1.1 S4

Qué falta, con qué se captura y qué hay que montar. El disector **ya reconoce los 18
protocolos** (los detecta por puerto o por prefacio); lo que falta es el paquete real en
`FASE-08-VALIDACION/corpus/`.

## Lo que falta (18 de 28)

BGP, CoAP, GRE, GTP, HTTP/2, ICMPv6, IGMP, MPLS, MQTT, QUIC, RIP, RTP, SCTP, STP, Syslog,
Telnet, VRRP, VXLAN.

**Corrección importante:** ninguno de estos 18 requiere máquinas antiguas. Todos son
protocolos modernos y se capturan en una VM Linux. Los stacks viejos (NetBEUI, IPX/SPX,
AppleTalk) son otro asunto: afectan a las **fichas**, no a los layouts F5.

**Dos trampas que ya están en el corpus y no registran:**

- `coap-eap-failure.pcap` no aporta CoAP porque va sobre DTLS; el disector solo detecta CoAP
  claro en el puerto 5683.
- `http2_follow_multistream.pcapng` no aporta HTTP/2 porque va sobre TLS; el disector necesita
  ver el prefacio `PRI * HTTP/2.0` en claro.

## Herramientas

| Necesitas | Para qué |
|---|---|
| `dumpcap` (viene con Wireshark) | Capturar. Es el único que debe correr con privilegios |
| `tshark` | Comprobar el fichero antes de darlo por bueno |
| Wireshark (GUI) | Revisar a ojo que el paquete tiene la forma esperada |

Comando base:

```bash
dumpcap -i <interfaz> -w <protocolo>.pcapng -a duration:60 -f "<filtro BPF>"
```

Los filtros de abajo son **BPF de captura** (los de Wireshark son de visualización y no
sirven aquí). Comprobación rápida antes de entregar:

```bash
tshark -r <protocolo>.pcapng -Y "<filtro de visualización>" | head
```

## Tabla por protocolo

| Protocolo | Escenario mínimo | Filtro BPF | Dificultad |
|---|---|---|---|
| BGP | FRR (`bgpd`) entre dos netns con veth, o `exabgp` | `tcp port 179` | media |
| CoAP | `coap-server` (libcoap) + `coap-client -m get coap://…/.well-known/core` | `udp port 5683` | baja |
| GRE | `ip tunnel add gre1 mode gre remote <ip> local <ip>` + `ping` | `proto gre` | baja |
| GTP | Open5GS en Docker (el más pesado del lote) | `udp port 2152` | alta |
| HTTP/2 | `nghttpd 8080` + `nghttp -v http://127.0.0.1:8080` (**h2c, en claro**) | `tcp port 8080` | baja |
| ICMPv6 | `ping -6 ::1` o `ping -6 <host>` | `icmp6` | baja |
| IGMP | `iperf -u -c 239.1.1.1` o `smcroute` uniéndose a un grupo | `igmp` | baja |
| MPLS | Dos netns + `ip route add … encap mpls 100 via …` | `mpls` | media |
| MQTT | `mosquitto` + `mosquitto_pub` / `mosquitto_sub` | `tcp port 1883` | baja |
| QUIC | `curl --http3 https://cloudflare-quic.com` (cifrado da igual: se detecta por puerto) | `udp port 443` | baja |
| RIP | FRR (`ripd`) o `quagga` entre dos netns | `udp port 520` | media |
| RTP | `ffmpeg -re -i tono.wav -f rtp rtp://127.0.0.1:5004` | `udp portrange 5004-5005` | baja |
| SCTP | `modprobe sctp` + `socat` o `sctp_test` cliente/servidor | `sctp` | media |
| STP | Puente Linux con `stp_state 1` entre dos veth | `ether dst 01:80:c2:00:00:00` | media |
| Syslog | `logger --server 127.0.0.1` o reenvío de `rsyslog` | `udp port 514` | baja |
| Telnet | `telnetd` en un contenedor + `telnet` | `tcp port 23` | baja |
| VRRP | `keepalived` en dos netns con veth | `proto vrrp` | media |
| VXLAN | `ip link add vxlan0 type vxlan id 42 dstport 4789 remote <ip>` + `ping` | `udp port 4789` | media |

## La VM

Una sola VM Linux (Debian 12 o Ubuntu 24.04) cubre 13 de los 18. Base:

```bash
sudo apt install -y tshark mosquitto mosquitto-clients libcoap3-bin frr keepalived \
                    ffmpeg socat telnetd rsyslog iproute2 nghttp2-client nghttp2-server
sudo modprobe sctp mpls_router mpls_iptunnel vxlan ip_gre
```

Para los que necesitan dos extremos (BGP, RIP, VRRP, MPLS, STP, GRE, VXLAN) no hace falta una
segunda VM: con **network namespaces** y un `veth` por par se monta en la misma máquina:

```bash
ip netns add a && ip netns add b
ip link add va type veth peer name vb
ip link set va netns a && ip link set vb netns b
ip -n a addr add 10.0.0.1/24 dev va && ip -n b addr add 10.0.0.2/24 dev vb
ip -n a link set va up && ip -n b link set vb up && ip -n a link set lo up && ip -n b link set lo up
```

GTP es el único que pide algo más: `docker run` de Open5GS y un simulador de UE. Si sale caro,
déjalo para el final; el resto de v1.1 no depende de él.

## Validación

Antes de dar una captura por buena, comprueba que el fichero abre y que el protocolo aparece:

```bash
# 1. ¿el fichero parsea y trae el protocolo?
tshark -r <protocolo>.pcapng -Y "<filtro de visualización>" | head

# 2. ¿lo reconoce nuestro disector?
dotnet test FASE-II-DISENO/tests/Redes.Knowledge.Tests \
  --filter "FullyQualifiedName~Corpus_Reconoce_Protocolos_Esperados"
```

La validación completa por protocolo es `CorpusL004.Validar(carpeta, F5-Campos-PDU.json, informe)`,
que devuelve paquetes, campos, % en límites y si hay algún paquete 100 % OK. Hoy se invoca desde
las pruebas; si quieres un comando de consola para regenerar el informe, es un añadido pequeño.

## Checklist al entregar una captura

1. Nombre descriptivo y en minúsculas: `<protocolo>-<caso>.pcapng` (p. ej. `bgp-session.pcapng`).
2. Menos de ~200 KB: recorta con `editcap -c 50 entrada.pcapng salida.pcapng` si hace falta.
3. Sin datos personales ni IPs reales de tu red: usa netns o direcciones de documentación
   (192.0.2.0/24, 198.51.100.0/24, 203.0.113.0/24).
4. Anota en `FASE-08-VALIDACION/corpus/LEEME-corpus.md`: fichero, fuente, licencia si es de
   terceros, y qué demuestra.
