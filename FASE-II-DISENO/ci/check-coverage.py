#!/usr/bin/env python3
"""Umbral de cobertura para la compuerta de calidad (D7-1).

Lee los informes Cobertura que genera `dotnet test --collect:"XPlat Code Coverage"`
y falla si la cobertura agregada queda por debajo de los umbrales.

Uso:
    python3 check-coverage.py <directorio-resultados> [--lineas 90] [--ramas 75]

Sin dependencias externas: solo la biblioteca estándar.
"""

from __future__ import annotations

import argparse
import glob
import os
import sys
import xml.etree.ElementTree as ET


def _agregar(rutas: list[str]) -> tuple[int, int, int, int]:
    lineas_validas = lineas_cubiertas = ramas_validas = ramas_cubiertas = 0
    for ruta in rutas:
        raiz = ET.parse(ruta).getroot()
        for linea in raiz.iter("line"):
            if linea.get("hits") is None:
                continue
            lineas_validas += 1
            if int(linea.get("hits")) > 0:
                lineas_cubiertas += 1
            condicion = linea.get("condition-coverage")
            if condicion and "(" in condicion:
                cubiertas, totales = condicion.split("(")[1].rstrip(")").split("/")
                ramas_cubiertas += int(cubiertas)
                ramas_validas += int(totales)
    return lineas_validas, lineas_cubiertas, ramas_validas, ramas_cubiertas


def main() -> int:
    parser = argparse.ArgumentParser(description="Umbral de cobertura Cobertura")
    parser.add_argument("directorio", help="directorio con coverage.cobertura.xml")
    parser.add_argument("--lineas", type=float, default=90.0, help="mínimo de línea (%%)")
    parser.add_argument("--ramas", type=float, default=75.0, help="mínimo de rama (%%)")
    args = parser.parse_args()

    rutas = sorted(glob.glob(os.path.join(args.directorio, "**", "coverage.cobertura.xml"), recursive=True))
    if not rutas:
        print(f"::error::No se encontró coverage.cobertura.xml en {args.directorio}")
        return 1

    lineas_validas, lineas_cubiertas, ramas_validas, ramas_cubiertas = _agregar(rutas)
    if lineas_validas == 0:
        print("::error::El informe de cobertura no contiene líneas medibles")
        return 1

    pct_lineas = 100.0 * lineas_cubiertas / lineas_validas
    pct_ramas = 100.0 * ramas_cubiertas / ramas_validas if ramas_validas else 100.0

    print(f"Informes leídos: {len(rutas)}")
    print(f"Cobertura de línea:  {pct_lineas:6.2f} %  ({lineas_cubiertas}/{lineas_validas})  mínimo {args.lineas:.2f} %")
    print(f"Cobertura de rama:   {pct_ramas:6.2f} %  ({ramas_cubiertas}/{ramas_validas})  mínimo {args.ramas:.2f} %")

    fallos = []
    if pct_lineas + 1e-9 < args.lineas:
        fallos.append(f"línea {pct_lineas:.2f} % < {args.lineas:.2f} %")
    if pct_ramas + 1e-9 < args.ramas:
        fallos.append(f"rama {pct_ramas:.2f} % < {args.ramas:.2f} %")

    if fallos:
        for fallo in fallos:
            print(f"::error::Cobertura por debajo del umbral: {fallo}")
        return 1

    print("Umbral de cobertura cumplido.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
