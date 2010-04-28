;InstallOptions Test Script
;Written by Joost Verburg
;--------------------------

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
ReserveFile "testimgs.ini"
ReserveFile "${NSISDIR}\Contrib\Graphics\Checks\colorful.bmp"
ReserveFile "${NSISDIR}\Contrib\Graphics\Checks\modern.bmp"
ReserveFile "${NSISDIR}\Contrib\Graphics\Icons\pixel-install.ico"

;Order of pages
Page custom SetCustom "" ": Testing InstallOptions" ;Custom page. InstallOptions gets called in SetCustom.
Page instfiles

Section  
SectionEnd

Function .onInit

  ;Extract InstallOptions files
  ;$PLUGINSDIR will automatically be removed when the installer closes
  
  InitPluginsDir
  File /oname=$PLUGINSDIR\testimgs.ini "testimgs.ini"
  File /oname=$PLUGINSDIR\image.bmp "${NSISDIR}\Contrib\Graphics\Checks\colorful.bmp"
  File /oname=$PLUGINSDIR\image2.bmp "${NSISDIR}\Contrib\Graphics\Checks\modern.bmp"
  File /oname=$PLUGINSDIR\icon.ico "${NSISDIR}\Contrib\Graphics\Icons\pixel-install.ico"

  ;Write image paths to the INI file

  WriteINIStr $PLUGINSDIR\testimgs.ini "Field 2" "Text" $PLUGINSDIR\image.bmp
  WriteINIStr $PLUGINSDIR\testimgs.ini "Field 3" "Text" $PLUGINSDIR\image2.bmp
  WriteINIStr $PLUGINSDIR\testimgs.ini "Field 4" "Text" $PLUGINSDIR\image.bmp
  WriteINIStr $PLUGINSDIR\testimgs.ini "Field 5" "Text" $PLUGINSDIR\image2.bmp
  WriteINIStr $PLUGINSDIR\testimgs.ini "Field 6" "Text" $PLUGINSDIR\icon.ico
  ;No Text for Field 7 so it'll show the installer's icon
  
FunctionEnd

Function SetCustom

  ;Display the InstallOptions dialog
  InstallOptions::dialog "$PLUGINSDIR\testimgs.ini"
  Pop $0

FunctionEnd
