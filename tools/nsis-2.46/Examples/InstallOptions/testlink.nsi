;InstallOptions Test Script
;Written by Ramon
;This script demonstrates the power of the new control "LINK"
;that allows you to execute files, send mails, open wepsites, etc.
;--------------------------

!define TEMP1 $R0 ;Temp variable

;The name of the installer
Name "InstallOptions Test Link"

;The file to write
OutFile "TestLink.exe"

; Show install details
ShowInstDetails show

;Things that need to be extracted on startup (keep these lines before any File command!)
;Only useful for BZIP2 compression
;Use ReserveFile for your own InstallOptions INI files too!

ReserveFile "${NSISDIR}\Plugins\InstallOptions.dll"
ReserveFile "testlink.ini"

;Order of pages
Page custom SetCustom
Page instfiles

Section "Components"

  ;Get Install Options dialog user input

SectionEnd

Function .onInit

  ;Extract InstallOptions files
  ;$PLUGINSDIR will automatically be removed when the installer closes
  
  InitPluginsDir
  File /oname=$PLUGINSDIR\test.ini "testlink.ini"
  WriteIniStr $PLUGINSDIR\test.ini "Field 2" "State" "$WINDIR\Notepad.exe"
  
FunctionEnd

Function SetCustom

  ;Display the InstallOptions dialog

  Push ${TEMP1}

    InstallOptions::dialog "$PLUGINSDIR\test.ini"
    Pop ${TEMP1}
  
  Pop ${TEMP1}

FunctionEnd

