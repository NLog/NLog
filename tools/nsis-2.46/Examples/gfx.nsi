; gfx.nsi
;
; This script shows some examples of using all of the new
; graphic related additions introduced in NSIS 2
;
; Written by Amir Szkeley 22nd July 2002

;--------------------------------

!macro BIMAGE IMAGE PARMS
	Push $0
	GetTempFileName $0
	File /oname=$0 "${IMAGE}"
	SetBrandingImage ${PARMS} $0
	Delete $0
	Pop $0
!macroend

;--------------------------------

Name "Graphical effects"

OutFile "gfx.exe"

; Adds an XP manifest to the installer
XPStyle on

; Add branding image to the installer (an image placeholder on the side).
; It is not enough to just add the placeholder, we must set the image too...
; We will later set the image in every pre-page function.
; We can also set just one persistent image in .onGUIInit
AddBrandingImage left 100

; Sets the font of the installer
SetFont "Comic Sans MS" 8

; Just to make it three pages...
SubCaption 0 ": Yet another page..."
SubCaption 2 ": Yet another page..."
LicenseText "License page"
LicenseData "gfx.nsi"
DirText "Lets make a third page!"

; Install dir
InstallDir "${NSISDIR}\Examples"

; Request application privileges for Windows Vista
RequestExecutionLevel user

;--------------------------------

; Pages
Page license licenseImage
Page custom customPage
Page directory dirImage
Page instfiles instImage

;--------------------------------

Section ""
	; You can also use the BI_NEXT macro here...
	MessageBox MB_YESNO "We can change the branding image from within a section too!$\nDo you want me to change it?" IDNO done
		!insertmacro BIMAGE "${NSISDIR}\Contrib\Graphics\Wizard\nsis.bmp" ""
	done:
	WriteUninstaller uninst.exe
SectionEnd

;--------------------------------

Function licenseImage
	!insertmacro BIMAGE "${NSISDIR}\Contrib\Graphics\Header\nsis.bmp" /RESIZETOFIT
	MessageBox MB_YESNO 'Would you like to skip the license page?' IDNO no
		Abort
	no:
FunctionEnd

Function customPage
	!insertmacro BIMAGE "${NSISDIR}\Contrib\Graphics\Checks\modern.bmp" /RESIZETOFIT
	MessageBox MB_OK 'This is a nice custom "page" with yet another image :P'
	#insert install options/start menu/<insert plugin name here> here
FunctionEnd

Function dirImage
	!insertmacro BIMAGE "${NSISDIR}\Contrib\Graphics\Header\win.bmp" /RESIZETOFIT
FunctionEnd

Function instImage
	!insertmacro BIMAGE "${NSISDIR}\Contrib\Graphics\Wizard\llama.bmp" /RESIZETOFIT
FunctionEnd

;--------------------------------

; Uninstall pages

UninstPage uninstConfirm un.uninstImage
UninstPage custom un.customPage
UninstPage instfiles un.instImage

Function un.uninstImage
	!insertmacro BIMAGE "${NSISDIR}\Contrib\Graphics\Checks\modern.bmp" /RESIZETOFIT
FunctionEnd

Function un.customPage
	!insertmacro BIMAGE "${NSISDIR}\Contrib\Graphics\Header\win.bmp" /RESIZETOFIT
	MessageBox MB_OK 'This is a nice uninstaller custom "page" with yet another image :P'
	#insert install options/start menu/<insert plugin name here> here
FunctionEnd

Function un.instImage
	!insertmacro BIMAGE "${NSISDIR}\Contrib\Graphics\Wizard\llama.bmp" /RESIZETOFIT
FunctionEnd

;--------------------------------

; Uninstaller

; Another page for uninstaller
UninstallText "Another page..."

Section uninstall
	MessageBox MB_OK "Bla"
SectionEnd

