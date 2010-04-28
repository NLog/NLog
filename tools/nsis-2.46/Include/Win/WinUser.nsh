!ifndef __WIN_WINUSER__INC
!define __WIN_WINUSER__INC
!verbose push
!verbose 3
!ifndef __WIN_MS_NOUSER & __WIN_NOINC_WINUSER

!ifndef __WIN_MS_NOVIRTUALKEYCODES
!define VK_LBUTTON    0x01
!define VK_RBUTTON    0x02
!define VK_CANCEL     0x03
!define VK_MBUTTON    0x04 /* NOT contiguous with L & RBUTTON */
!define VK_XBUTTON1   0x05 /* NOT contiguous with L & RBUTTON */
!define VK_XBUTTON2   0x06 /* NOT contiguous with L & RBUTTON */
!define VK_BACK       0x08
!define VK_TAB        0x09
!define VK_CLEAR      0x0C
!define VK_RETURN     0x0D
!define VK_SHIFT      0x10
!define VK_CONTROL    0x11
!define VK_MENU       0x12
!define VK_PAUSE      0x13
!define VK_CAPITAL    0x14
!define VK_ESCAPE     0x1B
!define VK_CONVERT    0x1C
!define VK_NONCONVERT 0x1D
!define VK_ACCEPT     0x1E
!define VK_MODECHANGE 0x1F
!define VK_SPACE      0x20
!define VK_PRIOR      0x21
!define VK_NEXT       0x22
!define VK_END        0x23
!define VK_HOME       0x24
!define VK_LEFT       0x25
!define VK_UP         0x26
!define VK_RIGHT      0x27
!define VK_DOWN       0x28
!define VK_SELECT     0x29
!define VK_PRINT      0x2A
!define VK_EXECUTE    0x2B
!define VK_SNAPSHOT   0x2C
!define VK_INSERT     0x2D
!define VK_DELETE     0x2E
!define VK_HELP       0x2F
; VK_0 - VK_9 are the same as ASCII '0' - '9' (0x30 - 0x39)
; VK_A - VK_Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A)
!define VK_LWIN           0x5B
!define VK_RWIN           0x5C
!define VK_APPS           0x5D
!define VK_SLEEP          0x5F
!define VK_NUMPAD0        0x60
!define VK_NUMPAD1        0x61
!define VK_NUMPAD2        0x62
!define VK_NUMPAD3        0x63
!define VK_NUMPAD4        0x64
!define VK_NUMPAD5        0x65
!define VK_NUMPAD6        0x66
!define VK_NUMPAD7        0x67
!define VK_NUMPAD8        0x68
!define VK_NUMPAD9        0x69
!define VK_MULTIPLY       0x6A
!define VK_ADD            0x6B
!define VK_SEPARATOR      0x6C
!define VK_SUBTRACT       0x6D
!define VK_DECIMAL        0x6E
!define VK_DIVIDE         0x6F
!define VK_F1             0x70
!define VK_F2             0x71
!define VK_F3             0x72
!define VK_F4             0x73
!define VK_F5             0x74
!define VK_F6             0x75
!define VK_F7             0x76
!define VK_F8             0x77
!define VK_F9             0x78
!define VK_F10            0x79
!define VK_F11            0x7A
!define VK_F12            0x7B
!define VK_NUMLOCK        0x90
!define VK_SCROLL         0x91
!define VK_OEM_NEC_EQUAL  0x92   ; '=' key on numpad
!define VK_LSHIFT         0xA0
!define VK_RSHIFT         0xA1
!define VK_LCONTROL       0xA2
!define VK_RCONTROL       0xA3
!define VK_LMENU          0xA4
!define VK_RMENU          0xA5
!endif

!ifndef __WIN_MS_NOWINOFFSETS
/* in nsDialogs.nsh...
!define GWL_STYLE           -16
!define GWL_EXSTYLE         -20 */
!define GWLP_WNDPROC        -4
!define GWLP_HINSTANCE      -6
!define GWLP_HWNDPARENT     -8
!define GWLP_USERDATA       -21
!define GWLP_ID             -12
!define DWLP_MSGRESULT  0
!define /math DWLP_DLGPROC    ${DWLP_MSGRESULT} + ${__WIN_PTRSIZE} ;DWLP_MSGRESULT + sizeof(LRESULT) 
!define /math DWLP_USER       ${DWLP_DLGPROC} + ${__WIN_PTRSIZE} ;DWLP_DLGPROC + sizeof(DLGPROC)
!endif

!ifndef __WIN_MS_NONCMESSAGES
!define HTERROR       -2
!define HTTRANSPARENT -1
!define HTNOWHERE     0
!define HTCLIENT      1
!define HTCAPTION     2
!define HTSYSMENU     3
!define HTGROWBOX     4
!define HTSIZE        ${HTGROWBOX}
!define HTMENU        5
!define HTHSCROLL     6
!define HTVSCROLL     7
!define HTMINBUTTON   8
!define HTMAXBUTTON   9
!define HTLEFT        10
!define HTRIGHT       11
!define HTTOP         12
!define HTTOPLEFT     13
!define HTTOPRIGHT    14
!define HTBOTTOM      15
!define HTBOTTOMLEFT  16
!define HTBOTTOMRIGHT 17
!define HTBORDER      18
!define HTREDUCE      ${HTMINBUTTON}
!define HTZOOM        ${HTMAXBUTTON}
!define HTSIZEFIRST   ${HTLEFT}
!define HTSIZELAST    ${HTBOTTOMRIGHT}
!define HTOBJECT      19
!define HTCLOSE       20
!define HTHELP        21
!endif

!ifndef __WIN_MS_NOSYSCOMMANDS
!define SC_SIZE         0xF000
!define SC_MOVE         0xF010
!define SC_MINIMIZE     0xF020
!define SC_MAXIMIZE     0xF030
!define SC_NEXTWINDOW   0xF040
!define SC_PREVWINDOW   0xF050
!define SC_CLOSE        0xF060
!define SC_VSCROLL      0xF070
!define SC_HSCROLL      0xF080
!define SC_MOUSEMENU    0xF090
!define SC_KEYMENU      0xF100
!define SC_ARRANGE      0xF110
!define SC_RESTORE      0xF120
!define SC_TASKLIST     0xF130
!define SC_SCREENSAVE   0xF140
!define SC_HOTKEY       0xF150
!define SC_DEFAULT      0xF160
!define SC_MONITORPOWER 0xF170
!define SC_CONTEXTHELP  0xF180
!define SC_SEPARATOR    0xF00F
!endif

!define IDC_ARROW       32512
!define IDC_IBEAM       32513
!define IDC_WAIT        32514
!define IDC_CROSS       32515
!define IDC_UPARROW     32516
!define IDC_SIZENWSE    32642
!define IDC_SIZENESW    32643
!define IDC_SIZEWE      32644
!define IDC_SIZENS      32645
!define IDC_SIZEALL     32646
!define IDC_NO          32648 
!define IDC_HAND        32649
!define IDC_APPSTARTING 32650 
!define IDC_HELP        32651

/* in nsDialogs.nsh...
!define IMAGE_BITMAP 0
!define IMAGE_ICON   1
!define IMAGE_CURSOR 2*/

/* in nsDialogs.nsh...
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
!define LR_SHARED           0x8000*/

!define GA_PARENT    1
!define GA_ROOT      2
!define GA_ROOTOWNER 3

!endif /* __WIN_MS_NOUSER & __WIN_NOINC_WINUSER */
!verbose pop
!endif /* __WIN_WINUSER__INC */