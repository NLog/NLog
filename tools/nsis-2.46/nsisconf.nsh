;------------------------
;DEFAULT NSIS CONFIG FILE
;------------------------

;This header file will be included when compiling any NSIS installer,
;you can use it to add script code to every installer you compile.

;This file is treated as if it is in the directory of your script.
;When using relative paths, the files have to be in your build directory.

;------------------------
;EXAMPLES
;------------------------

;Compress installer exehead with an executable compressor (such as UPX / Petite).

;Paths should be absolute to allow building from any location.
;Note that your executable compressor should not compress the first icon.

;!packhdr temp.dat '"C:\Program Files\upx\upx" -9 -q temp.dat'
;!packhdr temp.dat '"C:\Program Files\petite\petite" -9 -b0 -r** -p0 -y temp.dat'

;------------------------

;Set default compressor

;SetCompressor bzip2

;------------------------

;Change the default icons

;Icon "${NSISDIR}\Contrib\Graphics\Icons\arrow-install.ico"
;UninstallIcon "${NSISDIR}\Contrib\Graphics\Icons\arrow-uninstall.ico"

;------------------------

;Define symbols

;!define COMPANYNAME "bla"

;------------------------
;MODERN UI
;------------------------

;The Modern UI will insert the MUI_NSISCONF macro just before processing the settings.
;Here you can set default settings for the Modern UI.

;------------------------

!define MUI_INSERT_NSISCONF

!macro MUI_NSISCONF

  ;Example: Change the default Modern UI icons
  
  ;!ifndef MUI_ICON & MUI_UNICON
  ;  !define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\arrow-install.ico"
  ;  !define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\arrow-uninstall.ico"
  ;!endif
  
!macroend