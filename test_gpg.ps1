$f = Read-Host -AsSecureString 'Frase de la clave 84A27DF8CF75FE62'
$b = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($f)
$p = [Runtime.InteropServices.Marshal]::PtrToStringAuto($b)
[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($b)
Set-Content "$env:TEMP\np-test.txt" 'x' -NoNewline
$p | gpg --batch --yes --pinentry-mode loopback --passphrase-fd 0 -u 84A27DF8CF75FE62 --detach-sign "$env:TEMP\np-test.txt" 2>$null
if ($LASTEXITCODE -eq 0) { "FRASE OK -> subiendo al secreto"; $p | gh secret set GPG_PASSPHRASE; "SECRETO ACTUALIZADO" }
else { "FRASE INCORRECTA - revisa espacios/mayusculas y reintenta" }
Remove-Item "$env:TEMP\np-test.txt*" -Force -ErrorAction SilentlyContinue