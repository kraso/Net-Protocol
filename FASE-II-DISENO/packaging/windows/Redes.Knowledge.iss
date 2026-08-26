; Redes Knowledge — script de instalación (Inno Setup)
; Uso: compilar tras dotnet publish -c Release -r win-x64 --self-contained true -o dist\win-x64

[Setup]
AppName=Redes Knowledge
AppVersion=1.0.0
AppPublisher=Proyecto Redes
DefaultDirName={autopf}\RedesKnowledge
DefaultGroupName=Redes Knowledge
OutputDir=..\..\dist
OutputBaseFilename=RedesKnowledge-Setup-1.0.0
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "..\..\dist\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Redes Knowledge"; Filename: "{app}\Redes.Knowledge.App.exe"
Name: "{group}\Desinstalar"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\Redes.Knowledge.App.exe"; Description: "Ejecutar Redes Knowledge"; Flags: nowait postinstall skipifsilent