; Net Protocol — script de instalación (Inno Setup)
; Uso: compilar tras dotnet publish -c Release -r win-x64 --self-contained true -o dist\win-x64

[Setup]
AppName=Net Protocol
AppVersion=1.0.0
AppPublisher=Proyecto Redes
DefaultDirName={autopf}\NetProtocol
DefaultGroupName=Net Protocol
OutputDir=..\..\dist
OutputBaseFilename=NetProtocol-Setup-1.0.0
SetupIconFile=NetProtocol.ico
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "..\..\dist\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Net Protocol"; Filename: "{app}\NetProtocol.exe"
Name: "{group}\Desinstalar"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\NetProtocol.exe"; Description: "Ejecutar Net Protocol"; Flags: nowait postinstall skipifsilent