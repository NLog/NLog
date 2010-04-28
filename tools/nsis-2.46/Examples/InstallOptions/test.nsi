;InstallOptions Test Script
;Written by Joost Verburg
;--------------------------

!define TEMP1 $R0 ;Temp variable

;The name of the installer
Name "InstallOptions Test"

;The file to write
OutFile "Test.exe"

; Show install details
ShowInstDetails show

;Things that need to be extracted on startup (keep these lines before any File command!)
;Only useful for BZIP2 compression
;Use ReserveFile for your own InstallOptions INI files too!

ReserveFile "${NSISDIR}\Plugins\InstallOptions.dll"
ReserveFile "test.ini"

;Order of pages
Page custom SetCustom ValidateCustom ": Testing InstallOptions" ;Custom page. InstallOptions gets called in SetCustom.
Page instfiles

Section "Components"

  ;Get Install Options dialog user input

  ReadINIStr ${TEMP1} "$PLUGINSDIR\test.ini" "Field 2" "State"
  DetailPrint "Install X=${TEMP1}"
  ReadINIStr ${TEMP1} "$PLUGINSDIR\test.ini" "Field 3" "State"
  DetailPrint "Install Y=${TEMP1}"
  ReadINIStr ${TEMP1} "$PLUGINSDIR\test.ini" "Field 4" "State"
  DetailPrint "Install Z=${TEMP1}"
  ReadINIStr ${TEMP1} "$PLUGINSDIR\test.ini" "Field 5" "State"
  DetailPrint "File=${TEMP1}"
  ReadINIStr ${TEMP1} "$PLUGINSDIR\test.ini" "Field 6" "State"
  DetailPrint "Dir=${TEMP1}"
  ReadINIStr ${TEMP1} "$PLUGINSDIR\test.ini" "Field 8" "State"
  DetailPrint "Info=${TEMP1}"
  
SectionEnd

Function .onInit

  ;Extract InstallOptions files
  ;$PLUGINSDIR will automatically be removed when the installer closes
  
  InitPluginsDir
  File /oname=$PLUGINSDIR\test.ini "test.ini"
  
FunctionEnd

Function SetCustom

  ;Display the InstallOptions dialog

  Push ${TEMP1}

    InstallOptions::dialog "$PLUGINSDIR\test.ini"
    Pop ${TEMP1}
  
  Pop ${TEMP1}

FunctionEnd

Function ValidateCustom

  ReadINIStr ${TEMP1} "$PLUGINSDIR\test.ini" "Field 2" "State"
  StrCmp ${TEMP1} 1 done
  
  ReadINIStr ${TEMP1} "$PLUGINSDIR\test.ini" "Field 3" "State"
  StrCmp ${TEMP1} 1 done

  ReadINIStr ${TEMP1} "$PLUGINSDIR\test.ini" "Field 4" "State"
  StrCmp ${TEMP1} 1 done
    MessageBox MB_ICONEXCLAMATION|MB_OK "You must select at least one install option!"
    Abort

  done:
  
FunctionEnd
