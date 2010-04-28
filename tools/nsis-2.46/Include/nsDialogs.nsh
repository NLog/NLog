/*

nsDialogs.nsh
Header file for creating custom installer pages with nsDialogs

*/

!ifndef NSDIALOGS_INCLUDED
!define NSDIALOGS_INCLUDED
!verbose push
!verbose 3

!include LogicLib.nsh
!include WinMessages.nsh

!define WS_EX_DLGMODALFRAME  0x00000001
!define WS_EX_NOPARENTNOTIFY 0x00000004
!define WS_EX_TOPMOST        0x00000008
!define WS_EX_ACCEPTFILES    0x00000010
!define WS_EX_TRANSPARENT    0x00000020
!define WS_EX_MDICHILD       0x00000040
!define WS_EX_TOOLWINDOW     0x00000080
!define WS_EX_WINDOWEDGE     0x00000100
!define WS_EX_CLIENTEDGE     0x00000200
!define WS_EX_CONTEXTHELP    0x00000400
!define WS_EX_RIGHT          0x00001000
!define WS_EX_LEFT           0x00000000
!define WS_EX_RTLREADING     0x00002000
!define WS_EX_LTRREADING     0x00000000
!define WS_EX_LEFTSCROLLBAR  0x00004000
!define WS_EX_RIGHTSCROLLBAR 0x00000000
!define WS_EX_CONTROLPARENT  0x00010000
!define WS_EX_STATICEDGE     0x00020000
!define WS_EX_APPWINDOW      0x00040000

!define WS_CHILD             0x40000000
!define WS_VISIBLE           0x10000000
!define WS_DISABLED          0x08000000
!define WS_CLIPSIBLINGS      0x04000000
!define WS_CLIPCHILDREN      0x02000000
!define WS_MAXIMIZE          0x01000000
!define WS_VSCROLL           0x00200000
!define WS_HSCROLL           0x00100000
!define WS_GROUP             0x00020000
!define WS_TABSTOP           0x00010000

!define ES_LEFT              0x00000000
!define ES_CENTER            0x00000001
!define ES_RIGHT             0x00000002
!define ES_MULTILINE         0x00000004
!define ES_UPPERCASE         0x00000008
!define ES_LOWERCASE         0x00000010
!define ES_PASSWORD          0x00000020
!define ES_AUTOVSCROLL       0x00000040
!define ES_AUTOHSCROLL       0x00000080
!define ES_NOHIDESEL         0x00000100
!define ES_OEMCONVERT        0x00000400
!define ES_READONLY          0x00000800
!define ES_WANTRETURN        0x00001000
!define ES_NUMBER            0x00002000

!define SS_LEFT              0x00000000
!define SS_CENTER            0x00000001
!define SS_RIGHT             0x00000002
!define SS_ICON              0x00000003
!define SS_BLACKRECT         0x00000004
!define SS_GRAYRECT          0x00000005
!define SS_WHITERECT         0x00000006
!define SS_BLACKFRAME        0x00000007
!define SS_GRAYFRAME         0x00000008
!define SS_WHITEFRAME        0x00000009
!define SS_USERITEM          0x0000000A
!define SS_SIMPLE            0x0000000B
!define SS_LEFTNOWORDWRAP    0x0000000C
!define SS_OWNERDRAW         0x0000000D
!define SS_BITMAP            0x0000000E
!define SS_ENHMETAFILE       0x0000000F
!define SS_ETCHEDHORZ        0x00000010
!define SS_ETCHEDVERT        0x00000011
!define SS_ETCHEDFRAME       0x00000012
!define SS_TYPEMASK          0x0000001F
!define SS_REALSIZECONTROL   0x00000040
!define SS_NOPREFIX          0x00000080
!define SS_NOTIFY            0x00000100
!define SS_CENTERIMAGE       0x00000200
!define SS_RIGHTJUST         0x00000400
!define SS_REALSIZEIMAGE     0x00000800
!define SS_SUNKEN            0x00001000
!define SS_EDITCONTROL       0x00002000
!define SS_ENDELLIPSIS       0x00004000
!define SS_PATHELLIPSIS      0x00008000
!define SS_WORDELLIPSIS      0x0000C000
!define SS_ELLIPSISMASK      0x0000C000

!define BS_PUSHBUTTON        0x00000000
!define BS_DEFPUSHBUTTON     0x00000001
!define BS_CHECKBOX          0x00000002
!define BS_AUTOCHECKBOX      0x00000003
!define BS_RADIOBUTTON       0x00000004
!define BS_3STATE            0x00000005
!define BS_AUTO3STATE        0x00000006
!define BS_GROUPBOX          0x00000007
!define BS_USERBUTTON        0x00000008
!define BS_AUTORADIOBUTTON   0x00000009
!define BS_PUSHBOX           0x0000000A
!define BS_OWNERDRAW         0x0000000B
!define BS_TYPEMASK          0x0000000F
!define BS_LEFTTEXT          0x00000020
!define BS_TEXT              0x00000000
!define BS_ICON              0x00000040
!define BS_BITMAP            0x00000080
!define BS_LEFT              0x00000100
!define BS_RIGHT             0x00000200
!define BS_CENTER            0x00000300
!define BS_TOP               0x00000400
!define BS_BOTTOM            0x00000800
!define BS_VCENTER           0x00000C00
!define BS_PUSHLIKE          0x00001000
!define BS_MULTILINE         0x00002000
!define BS_NOTIFY            0x00004000
!define BS_FLAT              0x00008000
!define BS_RIGHTBUTTON       ${BS_LEFTTEXT}

!define CBS_SIMPLE            0x0001
!define CBS_DROPDOWN          0x0002
!define CBS_DROPDOWNLIST      0x0003
!define CBS_OWNERDRAWFIXED    0x0010
!define CBS_OWNERDRAWVARIABLE 0x0020
!define CBS_AUTOHSCROLL       0x0040
!define CBS_OEMCONVERT        0x0080
!define CBS_SORT              0x0100
!define CBS_HASSTRINGS        0x0200
!define CBS_NOINTEGRALHEIGHT  0x0400
!define CBS_DISABLENOSCROLL   0x0800
!define CBS_UPPERCASE         0x2000
!define CBS_LOWERCASE         0x4000

!define LBS_NOTIFY            0x0001
!define LBS_SORT              0x0002
!define LBS_NOREDRAW          0x0004
!define LBS_MULTIPLESEL       0x0008
!define LBS_OWNERDRAWFIXED    0x0010
!define LBS_OWNERDRAWVARIABLE 0x0020
!define LBS_HASSTRINGS        0x0040
!define LBS_USETABSTOPS       0x0080
!define LBS_NOINTEGRALHEIGHT  0x0100
!define LBS_MULTICOLUMN       0x0200
!define LBS_WANTKEYBOARDINPUT 0x0400
!define LBS_EXTENDEDSEL       0x0800
!define LBS_DISABLENOSCROLL   0x1000
!define LBS_NODATA            0x2000
!define LBS_NOSEL             0x4000
!define LBS_COMBOBOX          0x8000

!define LR_DEFAULTCOLOR     0x0000
!define LR_MONOCHROME       0x0001
!define LR_COLOR            0x0002
!define LR_COPYRETURNORG    0x0004
!define LR_COPYDELETEORG    0x0008
!define LR_LOADFROMFILE     0x0010
!define LR_LOADTRANSPARENT  0x0020
!define LR_DEFAULTSIZE      0x0040
!define LR_VGACOLOR         0x0080
!define LR_LOADMAP3DCOLORS  0x1000
!define LR_CREATEDIBSECTION 0x2000
!define LR_COPYFROMRESOURCE 0x4000
!define LR_SHARED           0x8000

!define IMAGE_BITMAP        0
!define IMAGE_ICON          1
!define IMAGE_CURSOR        2
!define IMAGE_ENHMETAFILE   3

!define GWL_STYLE           -16
!define GWL_EXSTYLE         -20

!define DEFAULT_STYLES ${WS_CHILD}|${WS_VISIBLE}|${WS_CLIPSIBLINGS}

!define __NSD_HLine_CLASS STATIC
!define __NSD_HLine_STYLE ${DEFAULT_STYLES}|${SS_ETCHEDHORZ}|${SS_SUNKEN}
!define __NSD_HLine_EXSTYLE ${WS_EX_TRANSPARENT}

!define __NSD_VLine_CLASS STATIC
!define __NSD_VLine_STYLE ${DEFAULT_STYLES}|${SS_ETCHEDVERT}|${SS_SUNKEN}
!define __NSD_VLine_EXSTYLE ${WS_EX_TRANSPARENT}

!define __NSD_Label_CLASS STATIC
!define __NSD_Label_STYLE ${DEFAULT_STYLES}|${SS_NOTIFY}
!define __NSD_Label_EXSTYLE ${WS_EX_TRANSPARENT}

!define __NSD_Icon_CLASS STATIC
!define __NSD_Icon_STYLE ${DEFAULT_STYLES}|${SS_ICON}|${SS_NOTIFY}
!define __NSD_Icon_EXSTYLE 0

!define __NSD_Bitmap_CLASS STATIC
!define __NSD_Bitmap_STYLE ${DEFAULT_STYLES}|${SS_BITMAP}|${SS_NOTIFY}
!define __NSD_Bitmap_EXSTYLE 0

!define __NSD_BrowseButton_CLASS BUTTON
!define __NSD_BrowseButton_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}
!define __NSD_BrowseButton_EXSTYLE 0

!define __NSD_Link_CLASS LINK
!define __NSD_Link_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${BS_OWNERDRAW}
!define __NSD_Link_EXSTYLE 0

!define __NSD_Button_CLASS BUTTON
!define __NSD_Button_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}
!define __NSD_Button_EXSTYLE 0

!define __NSD_GroupBox_CLASS BUTTON
!define __NSD_GroupBox_STYLE ${DEFAULT_STYLES}|${BS_GROUPBOX}
!define __NSD_GroupBox_EXSTYLE ${WS_EX_TRANSPARENT}

!define __NSD_CheckBox_CLASS BUTTON
!define __NSD_CheckBox_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${BS_TEXT}|${BS_VCENTER}|${BS_AUTOCHECKBOX}|${BS_MULTILINE}
!define __NSD_CheckBox_EXSTYLE 0

!define __NSD_RadioButton_CLASS BUTTON
!define __NSD_RadioButton_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${BS_TEXT}|${BS_VCENTER}|${BS_AUTORADIOBUTTON}|${BS_MULTILINE}
!define __NSD_RadioButton_EXSTYLE 0

!define __NSD_Text_CLASS EDIT
!define __NSD_Text_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${ES_AUTOHSCROLL}
!define __NSD_Text_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}

!define __NSD_Password_CLASS EDIT
!define __NSD_Password_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${ES_AUTOHSCROLL}|${ES_PASSWORD}
!define __NSD_Password_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}

!define __NSD_Number_CLASS EDIT
!define __NSD_Number_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${ES_AUTOHSCROLL}|${ES_NUMBER}
!define __NSD_Number_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}

!define __NSD_FileRequest_CLASS EDIT
!define __NSD_FileRequest_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${ES_AUTOHSCROLL}
!define __NSD_FileRequest_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}

!define __NSD_DirRequest_CLASS EDIT
!define __NSD_DirRequest_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${ES_AUTOHSCROLL}
!define __NSD_DirRequest_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}

!define __NSD_ComboBox_CLASS COMBOBOX
!define __NSD_ComboBox_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${WS_VSCROLL}|${WS_CLIPCHILDREN}|${CBS_AUTOHSCROLL}|${CBS_HASSTRINGS}|${CBS_DROPDOWN}
!define __NSD_ComboBox_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}

!define __NSD_DropList_CLASS COMBOBOX
!define __NSD_DropList_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${WS_VSCROLL}|${WS_CLIPCHILDREN}|${CBS_AUTOHSCROLL}|${CBS_HASSTRINGS}|${CBS_DROPDOWNLIST}
!define __NSD_DropList_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}

!define __NSD_ListBox_CLASS LISTBOX
!define __NSD_ListBox_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${WS_VSCROLL}|${LBS_DISABLENOSCROLL}|${LBS_HASSTRINGS}|${LBS_NOINTEGRALHEIGHT}|${LBS_NOTIFY}
!define __NSD_ListBox_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}

!define __NSD_ProgressBar_CLASS msctls_progress32
!define __NSD_ProgressBar_STYLE ${DEFAULT_STYLES}
!define __NSD_ProgressBar_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}

!macro __NSD_DefineControl NAME

	!define NSD_Create${NAME} "nsDialogs::CreateControl ${__NSD_${Name}_CLASS} ${__NSD_${Name}_STYLE} ${__NSD_${Name}_EXSTYLE}"

!macroend

!insertmacro __NSD_DefineControl HLine
!insertmacro __NSD_DefineControl VLine
!insertmacro __NSD_DefineControl Label
!insertmacro __NSD_DefineControl Icon
!insertmacro __NSD_DefineControl Bitmap
!insertmacro __NSD_DefineControl BrowseButton
!insertmacro __NSD_DefineControl Link
!insertmacro __NSD_DefineControl Button
!insertmacro __NSD_DefineControl GroupBox
!insertmacro __NSD_DefineControl CheckBox
!insertmacro __NSD_DefineControl RadioButton
!insertmacro __NSD_DefineControl Text
!insertmacro __NSD_DefineControl Password
!insertmacro __NSD_DefineControl Number
!insertmacro __NSD_DefineControl FileRequest
!insertmacro __NSD_DefineControl DirRequest
!insertmacro __NSD_DefineControl ComboBox
!insertmacro __NSD_DefineControl DropList
!insertmacro __NSD_DefineControl ListBox
!insertmacro __NSD_DefineControl ProgressBar

!macro __NSD_OnControlEvent EVENT HWND FUNCTION

	Push $0
	Push $1

	StrCpy $1 "${HWND}"

	GetFunctionAddress $0 "${FUNCTION}"
	nsDialogs::On${EVENT} $1 $0

	Pop $1
	Pop $0

!macroend

!macro __NSD_DefineControlCallback EVENT

	!define NSD_On${EVENT} `!insertmacro __NSD_OnControlEvent ${EVENT}`

!macroend

!macro __NSD_OnDialogEvent EVENT FUNCTION

	Push $0

	GetFunctionAddress $0 "${FUNCTION}"
	nsDialogs::On${EVENT} $0

	Pop $0

!macroend

!macro __NSD_DefineDialogCallback EVENT

	!define NSD_On${EVENT} `!insertmacro __NSD_OnDialogEvent ${EVENT}`

!macroend

!insertmacro __NSD_DefineControlCallback Click
!insertmacro __NSD_DefineControlCallback Change
!insertmacro __NSD_DefineControlCallback Notify
!insertmacro __NSD_DefineDialogCallback Back

!macro _NSD_CreateTimer FUNCTION INTERVAL

	Push $0

	GetFunctionAddress $0 "${FUNCTION}"
	nsDialogs::CreateTimer $0 "${INTERVAL}"

	Pop $0

!macroend

!define NSD_CreateTimer `!insertmacro _NSD_CreateTimer`

!macro _NSD_KillTimer FUNCTION

	Push $0

	GetFunctionAddress $0 "${FUNCTION}"
	nsDialogs::KillTimer $0

	Pop $0

!macroend

!define NSD_KillTimer `!insertmacro _NSD_KillTimer`

!macro _NSD_AddStyle CONTROL STYLE

	Push $0

	System::Call "user32::GetWindowLong(i ${CONTROL}, i ${GWL_STYLE}) i .r0"
	System::Call "user32::SetWindowLong(i ${CONTROL}, i ${GWL_STYLE}, i $0|${STYLE})"

	Pop $0

!macroend

!define NSD_AddStyle "!insertmacro _NSD_AddStyle"

!macro _NSD_AddExStyle CONTROL EXSTYLE

	Push $0

	System::Call "user32::GetWindowLong(i ${CONTROL}, i ${GWL_EXSTYLE}) i .r0"
	System::Call "user32::SetWindowLong(i ${CONTROL}, i ${GWL_EXSTYLE}, i $0|${EXSTYLE})"

	Pop $0

!macroend

!define NSD_AddExStyle "!insertmacro _NSD_AddExStyle"

!macro __NSD_GetText CONTROL VAR

	System::Call user32::GetWindowText(i${CONTROL},t.s,i${NSIS_MAX_STRLEN})
	Pop ${VAR}

!macroend

!define NSD_GetText `!insertmacro __NSD_GetText`

!macro __NSD_SetText CONTROL TEXT

	SendMessage ${CONTROL} ${WM_SETTEXT} 0 `STR:${TEXT}`

!macroend

!define NSD_SetText `!insertmacro __NSD_SetText`

!macro _NSD_SetTextLimit CONTROL LIMIT

	SendMessage ${CONTROL} ${EM_SETLIMITTEXT} ${LIMIT} 0

!macroend

!define NSD_SetTextLimit "!insertmacro _NSD_SetTextLimit"

!macro __NSD_GetState CONTROL VAR

	SendMessage ${CONTROL} ${BM_GETCHECK} 0 0 ${VAR}

!macroend

!define NSD_GetState `!insertmacro __NSD_GetState`

!macro __NSD_SetState CONTROL STATE

	SendMessage ${CONTROL} ${BM_SETCHECK} ${STATE} 0

!macroend

!define NSD_SetState `!insertmacro __NSD_SetState`

!macro __NSD_Check CONTROL

	${NSD_SetState} ${CONTROL} ${BST_CHECKED}

!macroend

!define NSD_Check `!insertmacro __NSD_Check`

!macro __NSD_Uncheck CONTROL

	${NSD_SetState} ${CONTROL} ${BST_UNCHECKED}

!macroend

!define NSD_Uncheck `!insertmacro __NSD_Uncheck`

!macro __NSD_SetFocus HWND

	System::Call "user32::SetFocus(i${HWND})"
  
!macroend

!define NSD_SetFocus `!insertmacro __NSD_SetFocus`

!macro _NSD_CB_AddString CONTROL STRING

	SendMessage ${CONTROL} ${CB_ADDSTRING} 0 `STR:${STRING}`

!macroend

!define NSD_CB_AddString "!insertmacro _NSD_CB_AddString"

!macro _NSD_CB_SelectString CONTROL STRING

	SendMessage ${CONTROL} ${CB_SELECTSTRING} -1 `STR:${STRING}`

!macroend

!define NSD_CB_SelectString "!insertmacro _NSD_CB_SelectString"

!macro _NSD_LB_AddString CONTROL STRING

	SendMessage ${CONTROL} ${LB_ADDSTRING} 0 `STR:${STRING}`

!macroend

!define NSD_LB_AddString "!insertmacro _NSD_LB_AddString"

!macro __NSD_LB_DelString CONTROL STRING

	SendMessage ${CONTROL} ${LB_DELETESTRING} 0 `STR:${STRING}`

!macroend

!define NSD_LB_DelString `!insertmacro __NSD_LB_DelString`

!macro __NSD_LB_Clear CONTROL VAR

	SendMessage ${CONTROL} ${LB_RESETCONTENT} 0 0 ${VAR}

!macroend

!define NSD_LB_Clear `!insertmacro __NSD_LB_Clear`

!macro __NSD_LB_GetCount CONTROL VAR

	SendMessage ${CONTROL} ${LB_GETCOUNT} 0 0 ${VAR}

!macroend

!define NSD_LB_GetCount `!insertmacro __NSD_LB_GetCount`

!macro _NSD_LB_SelectString CONTROL STRING

	SendMessage ${CONTROL} ${LB_SELECTSTRING} -1 `STR:${STRING}`

!macroend

!define NSD_LB_SelectString "!insertmacro _NSD_LB_SelectString"

!macro __NSD_LB_GetSelection CONTROL VAR

	SendMessage ${CONTROL} ${LB_GETCURSEL} 0 0 ${VAR}
	System::Call 'user32::SendMessage(i ${CONTROL}, i ${LB_GETTEXT}, i ${VAR}, t .s)'
	Pop ${VAR}

!macroend

!define NSD_LB_GetSelection `!insertmacro __NSD_LB_GetSelection`


!macro __NSD_LoadAndSetImage _LIHINSTMODE _IMGTYPE _LIHINSTSRC _LIFLAGS CONTROL IMAGE HANDLE
	
	Push $0
	Push $R0

	StrCpy $R0 ${CONTROL} # in case ${CONTROL} is $0
	
	!if "${_LIHINSTMODE}" == "exeresource"
		System::Call 'kernel32::GetModuleHandle(i0) i.r0'
		!undef _LIHINSTSRC
		!define _LIHINSTSRC r0
	!endif
	
	System::Call 'user32::LoadImage(i ${_LIHINSTSRC}, ts, i ${_IMGTYPE}, i0, i0, i${_LIFLAGS}) i.r0' "${IMAGE}"
	SendMessage $R0 ${STM_SETIMAGE} ${_IMGTYPE} $0

	Pop $R0
	Exch $0

	Pop ${HANDLE}

!macroend

!macro __NSD_SetIconFromExeResource CONTROL IMAGE HANDLE
	!insertmacro __NSD_LoadAndSetImage exeresource ${IMAGE_ICON} 0 ${LR_DEFAULTSIZE} "${CONTROL}" "${IMAGE}" ${HANDLE}
!macroend

!macro __NSD_SetIconFromInstaller CONTROL HANDLE
	!insertmacro __NSD_SetIconFromExeResource "${CONTROL}" "#103" ${HANDLE}
!macroend

!define NSD_SetImage `!insertmacro __NSD_LoadAndSetImage file ${IMAGE_BITMAP} 0 "${LR_LOADFROMFILE}"`
!define NSD_SetBitmap `${NSD_SetImage}`

!define NSD_SetIcon `!insertmacro __NSD_LoadAndSetImage file ${IMAGE_ICON} 0 "${LR_LOADFROMFILE}|${LR_DEFAULTSIZE}"`
!define NSD_SetIconFromExeResource `!insertmacro __NSD_SetIconFromExeResource`
!define NSD_SetIconFromInstaller `!insertmacro __NSD_SetIconFromInstaller`


!macro __NSD_SetStretchedImage CONTROL IMAGE HANDLE

	Push $0
	Push $1
	Push $2
	Push $R0

	StrCpy $R0 ${CONTROL} # in case ${CONTROL} is $0

	StrCpy $1 ""
	StrCpy $2 ""

	System::Call '*(i, i, i, i) i.s'
	Pop $0

	${If} $0 <> 0
	
		System::Call 'user32::GetClientRect(iR0, ir0)'
		System::Call '*$0(i, i, i .s, i .s)'
		System::Free $0
		Pop $1
		Pop $2

	${EndIf}

	System::Call 'user32::LoadImage(i0, ts, i ${IMAGE_BITMAP}, ir1, ir2, i${LR_LOADFROMFILE}) i.s' "${IMAGE}"
	Pop $0
    SendMessage $R0 ${STM_SETIMAGE} ${IMAGE_BITMAP} $0

	Pop $R0
	Pop $2
	Pop $1
	Exch $0

	Pop ${HANDLE}

!macroend

!define NSD_SetStretchedImage `!insertmacro __NSD_SetStretchedImage`

!macro __NSD_FreeImage IMAGE

	${If} ${IMAGE} <> 0

		System::Call gdi32::DeleteObject(is) ${IMAGE}

	${EndIf}

!macroend

!define NSD_FreeImage `!insertmacro __NSD_FreeImage`
!define NSD_FreeBitmap `${NSD_FreeImage}`

!macro __NSD_FreeIcon IMAGE
	System::Call user32::DestroyIcon(is) ${IMAGE}
!macroend

!define NSD_FreeIcon `!insertmacro __NSD_FreeIcon`

!macro __NSD_ClearImage _IMGTYPE CONTROL

	SendMessage ${CONTROL} ${STM_SETIMAGE} ${_IMGTYPE} 0

!macroend

!define NSD_ClearImage `!insertmacro __NSD_ClearImage ${IMAGE_BITMAP}`
!define NSD_ClearIcon  `!insertmacro __NSD_ClearImage ${IMAGE_ICON}`


!define DEBUG `System::Call kernel32::OutputDebugString(ts)`

!macro __NSD_ControlCase TYPE

	${Case} ${TYPE}
		${NSD_Create${TYPE}} $R3u $R4u $R5u $R6u $R7
		Pop $R9
		${Break}

!macroend

!macro __NSD_ControlCaseEx TYPE

	${Case} ${TYPE}
		Call ${TYPE}
		${Break}

!macroend

!macro NSD_FUNCTION_INIFILE

	!insertmacro NSD_INIFILE ""

!macroend

!macro NSD_UNFUNCTION_INIFILE

	!insertmacro NSD_INIFILE un.

!macroend

!macro NSD_INIFILE UNINSTALLER_FUNCPREFIX

	;Functions to create dialogs based on old InstallOptions INI files

	Function ${UNINSTALLER_FUNCPREFIX}CreateDialogFromINI

		# $0 = ini

		ReadINIStr $R0 $0 Settings RECT
		${If} $R0 == ""
			StrCpy $R0 1018
		${EndIf}

		nsDialogs::Create $R0
		Pop $R9

		ReadINIStr $R0 $0 Settings RTL
		nsDialogs::SetRTL $R0

		ReadINIStr $R0 $0 Settings NumFields

		${DEBUG} "NumFields = $R0"

		${For} $R1 1 $R0
			${DEBUG} "Creating field $R1"
			ReadINIStr $R2 $0 "Field $R1" Type
			${DEBUG} "  Type = $R2"
			ReadINIStr $R3 $0 "Field $R1" Left
			${DEBUG} "  Left = $R3"
			ReadINIStr $R4 $0 "Field $R1" Top
			${DEBUG} "  Top = $R4"
			ReadINIStr $R5 $0 "Field $R1" Right
			${DEBUG} "  Right = $R5"
			ReadINIStr $R6 $0 "Field $R1" Bottom
			${DEBUG} "  Bottom = $R6"
			IntOp $R5 $R5 - $R3
			${DEBUG} "  Width = $R5"
			IntOp $R6 $R6 - $R4
			${DEBUG} "  Height = $R6"
			ReadINIStr $R7 $0 "Field $R1" Text
			${DEBUG} "  Text = $R7"
			${Switch} $R2
				!insertmacro __NSD_ControlCase   HLine
				!insertmacro __NSD_ControlCase   VLine
				!insertmacro __NSD_ControlCase   Label
				!insertmacro __NSD_ControlCase   Icon
				!insertmacro __NSD_ControlCase   Bitmap
				!insertmacro __NSD_ControlCaseEx Link
				!insertmacro __NSD_ControlCase   Button
				!insertmacro __NSD_ControlCase   GroupBox
				!insertmacro __NSD_ControlCase   CheckBox
				!insertmacro __NSD_ControlCase   RadioButton
				!insertmacro __NSD_ControlCase   Text
				!insertmacro __NSD_ControlCase   Password
				!insertmacro __NSD_ControlCaseEx FileRequest
				!insertmacro __NSD_ControlCaseEx DirRequest
				!insertmacro __NSD_ControlCase   ComboBox
				!insertmacro __NSD_ControlCase   DropList
				!insertmacro __NSD_ControlCase   ListBox
			${EndSwitch}

			WriteINIStr $0 "Field $R1" HWND $R9
		${Next}

		nsDialogs::Show

	FunctionEnd

	Function ${UNINSTALLER_FUNCPREFIX}UpdateINIState

		${DEBUG} "Updating INI state"

		ReadINIStr $R0 $0 Settings NumFields

		${DEBUG} "NumField = $R0"

		${For} $R1 1 $R0
			ReadINIStr $R2 $0 "Field $R1" HWND
			ReadINIStr $R3 $0 "Field $R1" "Type"
			${Switch} $R3
				${Case} "CheckBox"
				${Case} "RadioButton"
					${DEBUG} "  HWND = $R2"
					${NSD_GetState} $R2 $R2
					${DEBUG} "  Window selection = $R2"
				${Break}
				${CaseElse}
					${DEBUG} "  HWND = $R2"
					${NSD_GetText} $R2 $R2
					${DEBUG} "  Window text = $R2"
				${Break}
			${EndSwitch}
			WriteINIStr $0 "Field $R1" STATE $R2
		${Next}

	FunctionEnd

	Function ${UNINSTALLER_FUNCPREFIX}FileRequest

		IntOp $R5 $R5 - 15
		IntOp $R8 $R3 + $R5

		${NSD_CreateBrowseButton} $R8u $R4u 15u $R6u ...
		Pop $R8

		nsDialogs::SetUserData $R8 $R1 # remember field id

		WriteINIStr $0 "Field $R1" HWND2 $R8

		${NSD_OnClick} $R8 ${UNINSTALLER_FUNCPREFIX}OnFileBrowseButton

		ReadINIStr $R9 $0 "Field $R1" State

		${NSD_CreateFileRequest} $R3u $R4u $R5u $R6u $R9
		Pop $R9

	FunctionEnd

	Function ${UNINSTALLER_FUNCPREFIX}DirRequest

		IntOp $R5 $R5 - 15
		IntOp $R8 $R3 + $R5

		${NSD_CreateBrowseButton} $R8u $R4u 15u $R6u ...
		Pop $R8

		nsDialogs::SetUserData $R8 $R1 # remember field id

		WriteINIStr $0 "Field $R1" HWND2 $R8

		${NSD_OnClick} $R8 ${UNINSTALLER_FUNCPREFIX}OnDirBrowseButton

		ReadINIStr $R9 $0 "Field $R1" State

		${NSD_CreateFileRequest} $R3u $R4u $R5u $R6u $R9
		Pop $R9

	FunctionEnd

	Function ${UNINSTALLER_FUNCPREFIX}OnFileBrowseButton

		Pop $R0

		nsDialogs::GetUserData $R0
		Pop $R1

		ReadINIStr $R2 $0 "Field $R1" HWND
		ReadINIStr $R4 $0 "Field $R1" Filter

		${NSD_GetText} $R2 $R3

		nsDialogs::SelectFileDialog save $R3 $R4
		Pop $R3

		${If} $R3 != ""
			SendMessage $R2 ${WM_SETTEXT} 0 STR:$R3
		${EndIf}

	FunctionEnd

	Function ${UNINSTALLER_FUNCPREFIX}OnDirBrowseButton

		Pop $R0

		nsDialogs::GetUserData $R0
		Pop $R1

		ReadINIStr $R2 $0 "Field $R1" HWND
		ReadINIStr $R3 $0 "Field $R1" Text

		${NSD_GetText} $R2 $R4

		nsDialogs::SelectFolderDialog $R3 $R4
		Pop $R3

		${If} $R3 != error
			SendMessage $R2 ${WM_SETTEXT} 0 STR:$R3
		${EndIf}

	FunctionEnd

	Function ${UNINSTALLER_FUNCPREFIX}Link

		${NSD_CreateLink} $R3u $R4u $R5u $R6u $R7
		Pop $R9

		nsDialogs::SetUserData $R9 $R1 # remember field id

		${NSD_OnClick} $R9 ${UNINSTALLER_FUNCPREFIX}OnLink

	FunctionEnd

	Function ${UNINSTALLER_FUNCPREFIX}OnLink

		Pop $R0

		nsDialogs::GetUserData $R0
		Pop $R1

		ReadINIStr $R1 $0 "Field $R1" STATE

		ExecShell "" $R1

	FunctionEnd

!macroend

!verbose pop
!endif
