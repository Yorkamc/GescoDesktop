; Configuración adicional para el instalador NSIS
; Asegurar que los iconos y accesos directos funcionen correctamente

!macro customInstall
  ; Variables para rutas
  SetOutPath "$INSTDIR"
  
  ; Crear acceso directo en el escritorio
  CreateShortCut "$DESKTOP\GESCO Desktop.lnk" \
    "$INSTDIR\GESCODesktop.exe" \
    "" \
    "$INSTDIR\GESCODesktop.exe" \
    0 \
    SW_SHOWNORMAL \
    "" \
    "GESCO Desktop - Sistema de Gestión"
  
  ; Crear acceso directo en el menú inicio
  CreateDirectory "$SMPROGRAMS\GESCO"
  CreateShortCut "$SMPROGRAMS\GESCO\GESCO Desktop.lnk" \
    "$INSTDIR\GESCODesktop.exe" \
    "" \
    "$INSTDIR\GESCODesktop.exe" \
    0 \
    SW_SHOWNORMAL \
    "" \
    "GESCO Desktop - Sistema de Gestión"
  
  ; Crear acceso directo para desinstalar
  CreateShortCut "$SMPROGRAMS\GESCO\Desinstalar GESCO Desktop.lnk" \
    "$INSTDIR\Uninstall GESCODesktop.exe" \
    "" \
    "$INSTDIR\Uninstall GESCODesktop.exe" \
    0
  
  ; Registrar la aplicación
  WriteRegStr HKCU "Software\Classes\Applications\GESCODesktop.exe\shell\open\command" "" '"$INSTDIR\GESCODesktop.exe" "%1"'
  
  ; Establecer el icono para el tipo de archivo .gesco
  WriteRegStr HKCU "Software\Classes\.gesco" "" "GESCOFile"
  WriteRegStr HKCU "Software\Classes\GESCOFile\DefaultIcon" "" "$INSTDIR\GESCODesktop.exe,0"
  WriteRegStr HKCU "Software\Classes\GESCOFile\shell\open\command" "" '"$INSTDIR\GESCODesktop.exe" "%1"'
  
  ; Refrescar iconos del sistema
  System::Call 'shell32.dll::SHChangeNotify(i, i, i, i) v (0x08000000, 0, 0, 0)'
!macroend

!macro customUnInstall
  ; Eliminar accesos directos
  Delete "$DESKTOP\GESCO Desktop.lnk"
  Delete "$SMPROGRAMS\GESCO\GESCO Desktop.lnk"
  Delete "$SMPROGRAMS\GESCO\Desinstalar GESCO Desktop.lnk"
  RMDir "$SMPROGRAMS\GESCO"
  
  ; Limpiar registros
  DeleteRegKey HKCU "Software\Classes\.gesco"
  DeleteRegKey HKCU "Software\Classes\GESCOFile"
  DeleteRegKey HKCU "Software\Classes\Applications\GESCODesktop.exe"
  
  ; Refrescar iconos del sistema
  System::Call 'shell32.dll::SHChangeNotify(i, i, i, i) v (0x08000000, 0, 0, 0)'
!macroend