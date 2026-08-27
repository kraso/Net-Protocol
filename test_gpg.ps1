$f = Read-Host -AsSecureString 'Frase REAL de CD11DE8033B6E164'
$b = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($f)
$p = [Runtime.InteropServices.Marshal]::PtrToStringAuto($b)
[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($b)
Set-Content "$env:TEMP\np-test.txt" 'x' -NoNewline
$p | gpg --batch --yes --pinentry-mode loopback --passphrase-fd 0 -u CD11DE8033B6E164 --detach-sign "$env:TEMP\np-test.txt" 2>$null
if ($LASTEXITCODE -eq 0) {
  "FRASE OK -> subiendo"
  Set-Content -Path "$env:TEMP\np-upload.txt" -Value $p -Encoding utf8NoBOM -NoNewline
  Get-Content -Raw -Path "$env:TEMP\np-upload.txt" | gh secret set GPG_PASSPHRASE -R kraso/redes-knowledge
  Remove-Item "$env:TEMP\np-upload.txt" -Force
  "SECRETO ACTUALIZADO"
} else { "FRASE INCORRECTA - no es la de CD11; no se sube nada" }
Remove-Item "$env:TEMP\np-test.txt*" -Force -ErrorAction SilentlyContinue