!macro customInstall
  ; Forzar el icono del ejecutable
  WriteRegStr HKCU "Software\Classes\Applications\GESCO Desktop.exe\shell\open\command" "" "$INSTDIR\GESCO Desktop.exe"
  
  ; Establecer el icono para el tipo de archivo
  WriteRegStr HKCU "Software\Classes\.gesco" "" "GESCOFile"
  WriteRegStr HKCU "Software\Classes\GESCOFile\DefaultIcon" "" "$INSTDIR\GESCO Desktop.exe,0"
  
  ; Asegurar que los accesos directos usen el icono correcto
  CreateShortCut "$DESKTOP\GESCO Desktop.lnk" "$INSTDIR\GESCO Desktop.exe" "" "$INSTDIR\GESCO Desktop.exe" 0
  CreateShortCut "$SMPROGRAMS\GESCO Desktop.lnk" "$INSTDIR\GESCO Desktop.exe" "" "$INSTDIR\GESCO Desktop.exe" 0
!macroend

!macro customUnInstall
  ; Limpiar registros al desinstalar
  DeleteRegKey HKCU "Software\Classes\.gesco"
  DeleteRegKey HKCU "Software\Classes\GESCOFile"
  DeleteRegKey HKCU "Software\Classes\Applications\GESCO Desktop.exe"
  
  ; Eliminar accesos directos
  Delete "$DESKTOP\GESCO Desktop.lnk"
  Delete "$SMPROGRAMS\GESCO Desktop.lnk"
!macroend