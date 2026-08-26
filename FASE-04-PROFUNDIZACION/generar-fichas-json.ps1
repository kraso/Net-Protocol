# Genera F4-Fichas-Prioritarias.json derivado de F4-Fichas-Prioritarias.md
# (regenerable: mismo MD -> mismo JSON; regla de datasets regenerables del repositorio).
# Uso: pwsh FASE-04-PROFUNDIZACION/generar-fichas-json.ps1
$ErrorActionPreference = 'Stop'
$md   = Join-Path $PSScriptRoot 'F4-Fichas-Prioritarias.md'
$out  = Join-Path $PSScriptRoot 'F4-Fichas-Prioritarias.json'

$lines = Get-Content $md -Encoding UTF8
$fichas = New-Object System.Collections.Generic.List[object]

for ($i = 0; $i -lt $lines.Count; $i++) {
    $l = $lines[$i]
    if ($l -notmatch '^##\s+F-\d+\s+·\s+(.+)$') { continue }
    $title = $Matches[1].Trim()
    # Título: "ACR — Nombre" (separador con espacios a ambos lados, p. ej. "IS-IS — …")
    # o solo nombre (p. ej. "Ethernet (IEEE 802.3)").
    if ($title -match '^(.+?)\s+[—-]\s+(.+)$') {
        $acronimo = $Matches[1].Trim()
        $nombre   = $Matches[2].Trim()
    } else {
        $acronimo = ($title -split '\s')[0]
        $nombre   = $title
    }
    if ($acronimo -eq $nombre) { $nombre = $acronimo }
    # Alias de acrónimo para igualar el catálogo F3
    $alias = @{
        'Ethernet'    = 'ETH'
        'IPsec (ESP)' = 'IPsec'
        'IS'          = 'IS-IS'
    }
    if ($alias.ContainsKey($acronimo)) { $acronimo = $alias[$acronimo] }

    # Recoger filas de la tabla siguiente hasta el próximo ## o el fin
    $campos = [ordered]@{}
    for (($i = $i + 1); $i -lt $lines.Count; $i++) {
        $fila = $lines[$i].Trim()
        if ($fila -match '^##\s') { $i--; break }
        if ($fila -notmatch '^\|\s*\d+\s*\|') { continue }
        $parts = $fila -split '\|' | ForEach-Object { $_.Trim() }
        # [ , #, Campo, Valor, ]   -> parts[1]=numero, parts[2]=campo, parts[3]=valor
        $num   = $parts[1]
        $campo = $parts[2]
        $valor = if ($parts.Count -ge 4) { $parts[3] } else { '' }
        if ($num -eq '' -or $campo -eq '') { continue }
        # Limpiar la notación Markdown fuerte (negritas) del valor
        $valor = $valor -replace '\*\*', ''
        $campos[$num] = [pscustomobject]@{ Campo = $campo; Valor = $valor }
    }

    $fichas.Add([pscustomobject]@{
        id       = "F-" + (($campos.Keys | Select-Object -First 1) -replace '.*', '') # placeholder
        acronimo = $acronimo
        nombre   = $nombre
        campos   = $campos
    })
}

# id real: buscar en la primera fila (1 | Identidad) el patrón "TCP; RFC..." no sirve;
# usar ordinal del bloque: F-01..F-12 según orden
$contador = 0
foreach ($f in $fichas) {
    $contador++
    $f.id = ('F-{0:D2}' -f $contador)
}

$doc = [ordered]@{
    version          = '0.2'
    fecha_consulta   = '2026-08-26'
    documento_rector = 'PLANREDES.md'
    fase             = 'F4 — Profundización protocolar (fichas prioritarias)'
    nota             = 'Derivación estructurada de F4-Fichas-Prioritarias.md (regenerable). Cada ficha conserva los 18 campos con su valor textual; la app la consume para finalidad, estado/fecha y fuentes sin duplicar datos.'
    fichas           = $fichas
}
$json = $doc | ConvertTo-Json -Depth 8
[System.IO.File]::WriteAllText($out, $json, (New-Object System.Text.UTF8Encoding($false)))
Write-Host "OK: $($fichas.Count) fichas -> $out"