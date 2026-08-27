; Net Protocol — script de instalación (Inno Setup)
; Uso: compilar tras dotnet publish -c Release -r win-x64 --self-contained true -o dist\win-x64
; La versión se pasa con iscc /DMyAppVersion=<ver> (p. ej. -DMyAppVersion=1.0.2);
; por defecto usa la última publicada.

#ifndef MyAppVersion
  #define MyAppVersion "1.0.1"
#endif

[Setup]
AppName=Net Protocol
AppVersion={#MyAppVersion}
AppPublisher=Proyecto Redes
DefaultDirName={autopf}\NetProtocol
DefaultGroupName=Net Protocol
OutputDir=..\..\dist
OutputBaseFilename=NetProtocol-Setup-{#MyAppVersion}
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