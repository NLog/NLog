Name "StartMenu.dll test"

OutFile "StartMenu Test.exe"

XPStyle on

Page directory
DirText "This installer will create some shortcuts to MakeNSIS in the start menu.$\nFor this it needs NSIS's path." \
  "Please specify the path in which you have installed NSIS:"
InstallDir "${NSISDIR}"
Function .onVerifyInstDir
	IfFileExists $INSTDIR\makensis.exe +2
		Abort
FunctionEnd

Page custom StartMenuGroupSelect "" ": Start Menu Folder"
Function StartMenuGroupSelect
	Push $R1

	StartMenu::Select /checknoshortcuts "Don't create a start menu folder" /autoadd /lastused $R0 "StartMenu.dll test"
	Pop $R1

	StrCmp $R1 "success" success
	StrCmp $R1 "cancel" done
		; error
		MessageBox MB_OK $R1
		StrCpy $R0 "StartMenu.dll test" # use default
		Return
	success:
	Pop $R0

	done:
	Pop $R1
FunctionEnd

Page instfiles
Section
	# this part is only necessary if you used /checknoshortcuts
	StrCpy $R1 $R0 1
	StrCmp $R1 ">" skip

		CreateDirectory $SMPROGRAMS\$R0
		CreateShortCut $SMPROGRAMS\$R0\MakeNSISw.lnk $INSTDIR\makensisw.exe

		SetShellVarContext All
		CreateDirectory $SMPROGRAMS\$R0
		CreateShortCut "$SMPROGRAMS\$R0\All users MakeNSISw.lnk" $INSTDIR\makensisw.exe

	skip:
SectionEnd