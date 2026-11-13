#!/usr/bin/env python3
"""Deriva el catálogo de fuentes (L-005) a partir del campo 18 de las fichas F4.

Lee `FASE-04-PROFUNDIZACION/F4-Fichas-Prioritarias.json`, interpreta el campo 18
("Fuentes") de cada ficha, y emite `FASE-08-VALIDACION/F8-Fuentes-Fichas.json` con:

  - `fuentes`: entidades Source normalizadas (id, titulo, url, version, organismo,
    nivel de autoridad, fecha de consulta), deduplicadas por (organismo, titulo).
  - `asignaciones`: ficha -> ids de fuente, en el orden en que aparecen.
  - `metricas`: cobertura por registro y global frente al objetivo del 90 %.

El script es idempotente: la misma entrada produce byte a byte la misma salida, y
`--check` falla si el JSON del repositorio no coincide con lo derivado.

Uso:
    python derivar-fuentes.py            # escribe el JSON
    python derivar-fuentes.py --check    # verifica sin escribir
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from pathlib import Path

RAIZ = Path(__file__).resolve().parent.parent
FICHAS = RAIZ / "FASE-04-PROFUNDIZACION" / "F4-Fichas-Prioritarias.json"
SALIDA = Path(__file__).resolve().parent / "F8-Fuentes-Fichas.json"

OBJETIVO = 90.0

# Organismo -> (prefijo de id, URL base, nivel por defecto).
# El nivel 1 es "primaria normativa" según F0-Politica-de-Fuentes.md §1.
ORGANISMOS = [
    ("IETF / RFC Editor", "RFC", "https://www.rfc-editor.org/", 1),
    ("IANA", "IANA", "https://www.iana.org/", 1),
    ("IEEE", "IEEE", "https://standards.ieee.org/", 1),
    ("ISO/IEC", "ISO", "https://www.iso.org/", 1),
    ("ITU-T", "ITU-T", "https://www.itu.int/", 1),
    ("3GPP", "3GPP", "https://www.3gpp.org/", 1),
    ("ETSI", "ETSI", "https://www.etsi.org/", 1),
    ("IEC", "IEC", "https://www.iec.ch/", 1),
    ("OASIS", "OASIS", "https://www.oasis-open.org/", 1),
    ("ANSI", "ANSI", "https://www.ansi.org/", 1),
    ("INCITS/T11", "T11", "https://www.incits.org/", 1),
    ("ASHRAE", "ASHRAE", "https://www.ashrae.org/", 1),
    ("NVM Express", "NVMe", "https://nvmexpress.org/", 1),
    ("LoRa Alliance", "LoRaWAN", "https://lora-alliance.org/", 1),
    ("Wi-Fi Alliance", "WiFi", "https://www.wi-fi.org/", 1),
    ("CNCF", "CNCF", "https://www.cncf.io/", 2),
    ("Microsoft", "MS", "https://learn.microsoft.com/", 2),
    ("Cisco", "Cisco", "https://www.cisco.com/", 2),
    ("Apple", "Apple", "https://developer.apple.com/", 2),
    ("Novell", "Novell", "https://www.novell.com/", 2),
    ("WireGuard", "WireGuard", "https://www.wireguard.com/", 2),
    ("Modbus", "Modbus", "https://modbus.org/", 1),
    ("DoD", "MIL-STD", "https://quicksearch.dla.mil/", 1),
    ("Referencia secundaria", "R", "https://example.invalid/", 3),
]

# Orden de comprobación: las claves más específicas primero.
CLAVES = [
    ("MIL-STD", "DoD"),
    ("RFC", "IETF / RFC Editor"),
    ("IANA", "IANA"),
    ("IEEE", "IEEE"),
    ("ISO", "ISO/IEC"),
    ("ITU-T", "ITU-T"),
    ("3GPP", "3GPP"),
    ("ETSI", "ETSI"),
    ("IEC", "IEC"),
    ("OASIS", "OASIS"),
    ("ANSI", "ANSI"),
    ("T11", "INCITS/T11"),
    ("FC-BB", "INCITS/T11"),
    ("ASHRAE", "ASHRAE"),
    ("NVMe", "NVM Express"),
    ("LoRaWAN", "LoRa Alliance"),
    ("Wi-Fi Alliance", "Wi-Fi Alliance"),
    ("CNCF", "CNCF"),
    ("MS-", "Microsoft"),
    ("Microsoft", "Microsoft"),
    ("Cisco", "Cisco"),
    ("Apple", "Apple"),
    ("Novell", "Novell"),
    ("WireGuard", "WireGuard"),
    ("Modbus", "Modbus"),
]

# Referencias secundarias citadas como abreviatura en el campo 18 (p. ej. "R2").
SECUNDARIAS = {"R1": 3, "R2": 3}

POR_ORGANISMO = {nombre: (pref, url, nivel) for nombre, pref, url, nivel in ORGANISMOS}

FECHA_RE = re.compile(r"(\d{2})-(\d{2})-(\d{4})")
NIVEL_RE = re.compile(r"\(nivel\s*(\d)", re.IGNORECASE)


def organismo_de(segmento: str) -> str:
    for clave, organismo in CLAVES:
        if clave.lower() in segmento.lower():
            return organismo
    if _etiqueta(segmento).upper() in SECUNDARIAS:
        return "Referencia secundaria"
    return "Sin clasificar"


def _etiqueta(segmento: str) -> str:
    """Segmento sin fecha de consulta ni guion final, para comparaciones exactas."""
    s = FECHA_RE.sub("", segmento)
    return re.sub(r"[—–-]\s*$", "", s).strip(" ;,")


def nivel_de(segmento: str, organismo: str) -> int:
    m = NIVEL_RE.search(segmento)
    if m:
        return int(m.group(1))
    etiqueta = _etiqueta(segmento).upper()
    if etiqueta in SECUNDARIAS:
        return SECUNDARIAS[etiqueta]
    return POR_ORGANISMO.get(organismo, ("", "", 3))[2]


def version_de(segmento: str, organismo: str) -> str:
    """Extrae la designación concreta: 'RFC 9293', 'IEEE 802.3', '3GPP TS 29.281'..."""
    s = segmento.strip()
    s = FECHA_RE.sub("", s)
    s = re.sub(r"[—–-]\s*(consulta\s*)?$", "", s).strip()
    s = re.sub(r"\(nivel\s*\d[^)]*\)", "", s, flags=re.IGNORECASE).strip()
    s = re.sub(r"\(R\d\)", "", s).strip()
    s = re.sub(r"\s*;\s*$", "", s).strip(" ;,")
    if organismo == "IETF / RFC Editor":
        nums = re.findall(r"\b(\d{3,4})\b", s)
        if nums:
            return "RFC " + ", ".join(nums)
    return s or organismo


def url_de(organismo: str, version: str) -> str:
    if organismo == "IETF / RFC Editor":
        m = re.search(r"RFC\s+(\d{3,4})", version)
        if m:
            return f"https://www.rfc-editor.org/rfc/rfc{m.group(1)}.html"
    return POR_ORGANISMO.get(organismo, ("", "https://example.invalid/", 3))[1]


def titulo_de(organismo: str, version: str) -> str:
    if organismo == "IETF / RFC Editor":
        return version
    return f"{organismo} — {version}" if version and version != organismo else organismo


def segmentos(campo18: str) -> list[str]:
    partes = [p.strip() for p in campo18.split(";")]
    return [p for p in partes if p and not FECHA_RE.fullmatch(p.strip())]


def derivar() -> dict:
    datos = json.loads(FICHAS.read_text(encoding="utf-8"))
    fecha_catalogo = datos.get("fecha_consulta", "")

    fuentes: dict[tuple[str, str], dict] = {}
    asignaciones: list[dict] = []

    for ficha in datos["fichas"]:
        campo = (ficha.get("campos", {}).get("18") or {}).get("Valor", "")
        ids: list[str] = []
        for seg in segmentos(campo):
            organismo = organismo_de(seg)
            version = version_de(seg, organismo)
            nivel = nivel_de(seg, organismo)
            clave = (organismo, version)
            if clave not in fuentes:
                pref = POR_ORGANISMO.get(organismo, ("FU", "", 3))[0]
                fuentes[clave] = {
                    "id": f"FU-{len(fuentes) + 1:03d}",
                    "titulo": titulo_de(organismo, version),
                    "url": url_de(organismo, version),
                    "version": version,
                    "organismo": organismo,
                    "nivel_autoridad": nivel,
                    "fecha_consulta": fecha_catalogo,
                    "_pref": pref,
                }
            fid = fuentes[clave]["id"]
            if fid not in ids:
                ids.append(fid)
        asignaciones.append(
            {
                "ficha": ficha["id"],
                "acronimo": ficha["acronimo"],
                "fuentes": ids,
                "nivel1": any(fuentes[(organismo_de(s), version_de(s, organismo_de(s)))]["nivel_autoridad"] == 1
                              for s in segmentos(campo)),
            }
        )

    lista = sorted(fuentes.values(), key=lambda f: f["id"])
    for f in lista:
        f.pop("_pref", None)

    con_fuente = sum(1 for a in asignaciones if a["fuentes"])
    con_nivel1 = sum(1 for a in asignaciones if a["nivel1"])
    total = len(asignaciones)
    cobertura = round(100.0 * con_nivel1 / total, 2) if total else 0.0

    return {
        "version": "1.0",
        "fecha_consulta": fecha_catalogo,
        "documento_rector": "F0-Politica-de-Fuentes.md",
        "fase": "F8 — Validación (laguna L-005: asignación de Source nivel 1)",
        "nota": "Derivado del campo 18 de F4-Fichas-Prioritarias.json por derivar-fuentes.py. "
                "Regenerable; no editar a mano.",
        "objetivo_cobertura": OBJETIVO,
        "fuentes": lista,
        "asignaciones": asignaciones,
        "metricas": {
            "fichas_totales": total,
            "fichas_con_fuente": con_fuente,
            "fichas_con_nivel1": con_nivel1,
            "cobertura_nivel1_pct": cobertura,
            "objetivo_cumplido": cobertura >= OBJETIVO,
            "fuentes_distintas": len(lista),
        },
    }


def serializar(doc: dict) -> str:
    return json.dumps(doc, ensure_ascii=False, indent=2, sort_keys=False) + "\n"


def main() -> int:
    ap = argparse.ArgumentParser(description="Deriva F8-Fuentes-Fichas.json desde F4.")
    ap.add_argument("--check", action="store_true", help="verifica sin escribir")
    args = ap.parse_args()

    doc = derivar()
    texto = serializar(doc)
    m = doc["metricas"]

    if args.check:
        if not SALIDA.exists():
            print(f"FALLO: no existe {SALIDA.name}", file=sys.stderr)
            return 1
        actual = SALIDA.read_text(encoding="utf-8")
        if actual != texto:
            print("FALLO: el JSON del repositorio no coincide con lo derivado.", file=sys.stderr)
            return 1
        print(f"OK: {SALIDA.name} al día "
              f"({m['fichas_con_nivel1']}/{m['fichas_totales']} fichas con nivel 1, "
              f"{m['cobertura_nivel1_pct']} %)")
        return 0

    SALIDA.write_text(texto, encoding="utf-8")
    print(f"escrito {SALIDA.name}: {m['fuentes_distintas']} fuentes, "
          f"{m['fichas_con_nivel1']}/{m['fichas_totales']} fichas con nivel 1 "
          f"({m['cobertura_nivel1_pct']} %), objetivo {OBJETIVO} % "
          f"{'cumplido' if m['objetivo_cumplido'] else 'NO cumplido'}")
    print(f"sha256={hashlib.sha256(texto.encode('utf-8')).hexdigest()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
