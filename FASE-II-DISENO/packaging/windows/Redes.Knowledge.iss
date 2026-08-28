; Net Protocol — script de instalación (Inno Setup)
; Uso: compilar tras dotnet publish -c Release -r win-x64 --self-contained true -o dist\win-x64
; La versión se pasa con iscc /DMyAppVersion=<ver> (p. ej. -DMyAppVersion=1.0.2);
; por defecto usa la última publicada.

#ifndef MyAppVersion
  #define MyAppVersion "1.0.9"
#endif

; URL ascendente del proyecto (se muestra en el desinstalador y en "Programas y
; características"). CI la pasa con -DMyAppUrl=<url> derivada del repositorio actual
; (GITHUB_REPOSITORY), de modo que un rename del repo actualiza los paquetes solos;
; por defecto usa la actual.
#ifndef MyAppUrl
  #define MyAppUrl "https://github.com/kraso/Net-Protocol"
#endif

[Setup]
AppName=Net Protocol
AppVersion={#MyAppVersion}
AppPublisher=Proyecto Redes
AppPublisherURL={#MyAppUrl}
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
; Actualización segura de versiones anteriores:
; - Sin AppId explícito: Inno deriva uno estable de AppName+AppPublisher+DefaultDirName
;   (clave de desinstalación "Net Protocol_is1" en HKCU...), idéntico entre versiones,
;   así el instalador reconoce la instalación previa y la actualiza en el mismo
;   directorio. NO fijar aquí un GUID propio: rompería el reconocimiento de las
;   instalaciones ya hechas con el AppId autogenerado.
; - AppMutex: la app crea "NetProtocolMutex" mientras está abierta; el instalador
;   detecta que está en ejecución y (con CloseApplications=force) la cierra sola
;   antes de sobreescribir los archivos, evitando ficheros bloqueados.
; - [InstallDelete] (abajo): elimina huérfanos de versiones anteriores.
AppMutex=NetProtocolMutex
CloseApplications=force
CloseApplicationsFilter=NetProtocol.exe

[InstallDelete]
; Limpia la carpeta de instalación antes de copiar: archivos o DLLs que una
; versión anterior trajera y la nueva ya no incluya no quedan huérfanos.
; Los datos del usuario (DB y capturas) viven en %LOCALAPPDATA%\NetProtocol,
; fuera de {app}, así que no se pierden. (filesandordirs también elimina
; subdirectorios vacíos que hayan quedado de versiones previas.)
Type: filesandordirs; Name: "{app}\*"

[Files]
Source: "..\..\dist\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Net Protocol"; Filename: "{app}\NetProtocol.exe"
Name: "{group}\Desinstalar"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\NetProtocol.exe"; Description: "Ejecutar Net Protocol"; Flags: nowait postinstall skipifsilent