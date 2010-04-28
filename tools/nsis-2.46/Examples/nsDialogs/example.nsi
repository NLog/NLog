!include nsDialogs.nsh
!include LogicLib.nsh

Name "nsDialogs Example"
OutFile "nsDialogs Example.exe"

XPStyle on

Page license
Page custom nsDialogsPage

Var BUTTON
Var EDIT
Var CHECKBOX

Function nsDialogsPage

	nsDialogs::Create 1018
	Pop $0

	GetFunctionAddress $0 OnBack
	nsDialogs::OnBack $0

	${NSD_CreateButton} 0 0 100% 12u Test
	Pop $BUTTON
	GetFunctionAddress $0 OnClick
	nsDialogs::OnClick $BUTTON $0

	${NSD_CreateText} 0 35 100% 12u hello
	Pop $EDIT
	GetFunctionAddress $0 OnChange
	nsDialogs::OnChange $EDIT $0

	${NSD_CreateCheckbox} 0 -50 100% 8u Test
	Pop $CHECKBOX
	GetFunctionAddress $0 OnCheckbox
	nsDialogs::OnClick $CHECKBOX $0

	${NSD_CreateLabel} 0 40u 75% 40u "* Type `hello there` above.$\n* Click the button.$\n* Check the checkbox.$\n* Hit the Back button."
	Pop $0

	nsDialogs::Show

FunctionEnd

Function OnClick

	Pop $0 # HWND

	MessageBox MB_OK clicky

FunctionEnd

Function OnChange

	Pop $0 # HWND

	System::Call user32::GetWindowText(i$EDIT,t.r0,i${NSIS_MAX_STRLEN})

	${If} $0 == "hello there"
		MessageBox MB_OK "right back at ya"
	${EndIf}

FunctionEnd

Function OnBack

	MessageBox MB_YESNO "are you sure?" IDYES +2
	Abort

FunctionEnd

Function OnCheckbox

	Pop $0 # HWND

	MessageBox MB_OK "checkbox clicked"

FunctionEnd

Section
SectionEnd
