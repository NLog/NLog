; Some useful functions based on System plugin
; 
; (c) brainsucker, 2002
; (r) BSForce

; Check for double includes
!ifndef SysFunc.NSH.Included
!define SysFunc.NSH.Included

!include "${NSISDIR}\Examples\System\System.nsh"

!verbose 3      ; For WinMessages especially
  !include "WinMessages.nsh"
!verbose 4

; ================= GetInstallerExeName implementation =================

; Adopted Get Parameter function -> now it gets full installer.exe path
; input - nothing, output -> full path at the top of the stack
Function GetInstallerExeName
   Push $R0
   Push $R1
   Push $R2
   StrCpy $R0 $CMDLINE 1
   StrCpy $R1 '"'
   StrCpy $R2 1
   StrCmp $R0 '"' loop
     StrCpy $R1 ' ' ; we're scanning for a space instead of a quote
   loop:
     StrCpy $R0 $CMDLINE 1 $R2
     StrCmp $R0 $R1 loop2
     StrCmp $R0 "" loop2
     IntOp $R2 $R2 + 1
     Goto loop
   loop2:

   ; Ok, have we found last exename character or string end?
   StrCmp $R0 "" "" +2
        IntOp $R2 $R2 - 1       ; last exename char
   StrCmp $R1 ' ' +3    ; was first character the '"', or something other?
        StrCpy $R1 1    ; it was quote
        Goto +2
        StrCpy $R1 0    
   IntOp $R2 $R2 - $R1
   StrCpy $R0 $CMDLINE $R2 $R1  

   SearchPath $R0 $R0      ; expand file name to full path

   Pop $R2
   Pop $R1
   Exch $R0
FunctionEnd

; ================= systemGetFileSysTime implementation =================

!macro smGetFileSysTime FILENAME
        Push ${FILENAME}
        Call systemGetFileSysTime
        Pop  $R0
!macroend

; -----------------------------------------------------------------
; systemGetFileSysTime (params on stack):
;       FILENAME        -       name of file to get file time
; returns to stack (SYSTEMTIME struct addr)
; -----------------------------------------------------------------

; uses original method from NSIS
Function systemGetFileSysTime
    System::Store "s r1"

    StrCpy $R0 0     

    ; create WIN32_FIND_DATA struct
    System::Call '*${stWIN32_FIND_DATA} .r2'

    ; Find file info
    System::Call '${sysFindFirstFile}(r1, r2) .r3'

    ; ok?
    IntCmp $3 ${INVALID_HANDLE_VALUE} sgfst_exit

    ; close file search
    System::Call '${sysFindClose}(r3)'

    ; Create systemtime struct for local time
    System::Call '*${stSYSTEMTIME} .R0'

    ; Get File time
    System::Call '*$2${stWIN32_FIND_DATA} (,,, .r3)'

    ; Convert file time (UTC) to local file time
    System::Call '${sysFileTimeToLocalFileTime}(r3, .r1)'

    ; Convert file time to system time
    System::Call '${sysFileTimeToSystemTime}(r1, R0)'

sgfst_exit:
    ; free used memory for WIN32_FIND_DATA struct
    System::Free $2    

    System::Store "P0 l"
FunctionEnd

; ================= systemMessageBox implementation =================

; return to $R0
!macro smMessageBox MODULE MSG CAPTION STYLE ICON
     Push "${ICON}"
     Push "${STYLE}"
     Push "${CAPTION}"
     Push "${MSG}"
     Push "${MODULE}"
     Call systemMessageBox
     Pop $R0
!macroend

; -----------------------------------------------------------------
; systemMessageBox (params on stack):
;       Module: either handle ("i HANDLE", HANDLE could be 0) or "modulename" 
;       Msg: text of message
;       Caption: caption of message box window
;       Style: style, buttons etc
;       Icon: either icon handle ("i HANDLE") or resource name 
; returns to stack
; -----------------------------------------------------------------
Function systemMessageBox
     System::Store "s r2r3r4r5r6"

     ; may be Module is module handle?
     StrCpy $1 $2
     IntCmp $1 0 0 smbnext smbnext

	 ; Get module handle
	 System::Call '${sysGetModuleHandle}($2) .r1'
	 IntCmp $1 0 loadlib libnotloaded libnotloaded

loadlib:
     ; Load module and get handle
     System::Call '${sysLoadLibrary}($2) .r1'
     IntCmp $1 0 0 smbnext smbnext

libnotloaded:
	 ; Indicate that LoadLibrary wasn't used
	 StrCpy $2 1

smbnext:
     ; Create MSGBOXPARAMS structure
     System::Call '*${stMSGBOXPARAMS}(, $HWNDPARENT, r1, r3, r4, "$5|${MB_USERICON}", $6, _) .r0'
     ; call MessageBoxIndirect
     System::Call '${sysMessageBoxIndirect}(r0) .R0'
     ; free MSGBOXPARAMS structure

     System::Free $0

     ; have we used load library at start?
     IntCmp $2 0 0 smbskipfree smbskipfree
     ; No, then free the module
     System::Call '${sysFreeLibrary}(r1)'

smbskipfree:
     System::Store "P0 l"   
FunctionEnd

; ================= systemSplash implementation =================

; returns to $R0
!macro smSystemSplash DELAY FILE
    Push ${FILE}
    Push ${DELAY}
    call systemSplash
    Pop $R0    
!macroend

; -----------------------------------------------------------------
; systemSplash (params on stack):
;       Delay - time in ms to show the splash
;       File - bitmap (& audio) file name (without extension)
; returns to stack
; -----------------------------------------------------------------

Function _systemSplashWndCB
   ; Callback receives 4 values
   System::Store "s r2r5r7r9"

   ; Message branching
   IntCmp $5 ${WM_CLOSE} m_Close
   IntCmp $5 ${WM_TIMER} m_Timer
   IntCmp $5 ${WM_LBUTTONDOWN} m_Lbtn
   IntCmp $5 ${WM_CREATE} m_Create
   IntCmp $5 ${WM_PAINT} m_Paint
   goto default

m_Create:
   ; Create structures
   System::Call "*${stRECT} (_) .R8"
   System::Call "*${stBITMAP} (_, &l0 .R7) .R9"

   ; Get bitmap info
   System::Call "${sysGetObject} (r6, R7, R9)" 
   
   ; Get desktop info
   System::Call "${sysSystemParametersInfo} (${SPI_GETWORKAREA}, 0, R8, 0)" 

   ; Style (callbacked)
   System::Call "${sysSetWindowLong} (r2, ${GWL_STYLE}, 0) .s" 
   !insertmacro SINGLE_CALLBACK 5 $R7 1 _systemSplashWndCB

   ; Calculate and set window pos

   ; Get bmWidth(R2) and bmHeight(R3)
   System::Call "*$R9${stBITMAP} (,.R2,.R3)"
   ; Get left(R4), top(R5), right(R6), bottom(R7)
   System::Call "*$R8${stRECT} (.R4,.R5,.R6,.R7)"

   ; Left pos
   IntOp $R0 $R6 - $R4
   IntOp $R0 $R0 - $R2
   IntOp $R0 $R0 / 2
   IntOp $R0 $R0 + $R4

   ; Top pos
   IntOp $R1 $R7 - $R5
   IntOp $R1 $R1 - $R3
   IntOp $R1 $R1 / 2
   IntOp $R1 $R1 + $R5

   System::Call "${sysSetWindowPos} (r2, 0, R0, R1, R2, R3, ${SWP_NOZORDER}) .s" 
   !insertmacro SINGLE_CALLBACK 6 $R7 1 _systemSplashWndCB

   ; Show window
   System::Call "${sysShowWindow} (r2, ${SW_SHOW}) .s" 
   !insertmacro SINGLE_CALLBACK 7 $R7 1 _systemSplashWndCB

   ; Set Timer
   System::Call "${sysSetTimer} (r2, 1, r8,)"

   ; Free used memory
   System::Free $R8
   System::Free $R9

   StrCpy $R0 0
   goto exit

m_Paint:
   ; Create structures
   System::Call "*${stRECT} (_) .R8"
   System::Call "*${stPAINTSTRUCT} (_) .R9"

   ; Begin Paint
   System::Call "${sysBeginPaint} (r2, R9) .R7"

   ; CreateCompatibleDC
   System::Call "${sysCreateCompatibleDC} (R7) .R6"

   ; GetClientRect
   System::Call "${sysGetClientRect} (r2, R8)"
  
   ; Select new bitmap
   System::Call "${sysSelectObject} (R6, r6) .R5"

   ; Get left(R0), top(R1), right(R2), bottom(R3)
   System::Call "*$R8${stRECT} (.R0,.R1,.R2,.R3)"
     
   ; width=right-left  
   IntOp $R2 $R2 - $R0
   ; height=bottom-top
   IntOp $R3 $R3 - $R1

   System::Call "${sysBitBlt} (R7, R0, R1, R2, R3, R6, 0, 0, ${SRCCOPY})" 

   ; Select old bitmap
   System::Call "${sysSelectObject} (R6, R5)"
   
   ; Delete compatible DC
   System::Call "${sysDeleteDC} (R6)"

   ; End Paint
   System::Call "${sysEndPaint} (r2, R9)"

   ; Free used memory
   System::Free $R8
   System::Free $R9

   StrCpy $R0 0
   goto exit

m_Timer:
m_Lbtn:
   StrCpy $4 0
   IntCmp $5 ${WM_TIMER} destroy
        StrCpy $4 1

destroy:
   System::Call "${sysDestroyWindow} (r2) .s"
   !insertmacro SINGLE_CALLBACK 12 $R4 1 _systemSplashWndCB

default:
   ; Default
   System::Call "${sysDefWindowProc} (r2, r5, r7, r9) .s"
   !insertmacro SINGLE_CALLBACK 14 $R0 1 _systemSplashWndCB
   goto exit

m_Close:
   StrCpy $R0 0
   goto exit

exit:
   ; Restore
   System::Store "p4P0 l R0r4"

   ; Return from callback
   System::Call "$3" $R0
FunctionEnd

Function systemSplash

   ; Save registers and get input 
   System::Store "s r8r9"

   ; Get module instance
   System::Call "${sysGetModuleHandle} (i) .r7"

   ; Get arrow cursor
   System::Call "${sysLoadCursor} (0, i ${IDC_ARROW}) .R9" 

   ; Get callback
   System::Get "${sysWNDPROC}"
   Pop $3

   ; Create window class
   System::Call "*${stWNDCLASS} (0,r3,0,0,r7,0,R9,0,i 0,'_sp') .R9"

   ; Register window class
   System::Call "${sysRegisterClass} (R9) .R9"
   IntCmp $R9 0 errorexit ; Class registered ok?

   ; Load Image (LR_CREATEDIBSECTION|LR_LOADFROMFILE = 0x2010)
   System::Call '${sysLoadImage} (, s, ${IMAGE_BITMAP}, 0, 0, ${LR_CREATEDIBSECTION}|${LR_LOADFROMFILE}) .r6' "$9.bmp"
   IntCmp $6 0 errorexit        ; Image loaded ok?

   ; Start the sound (SND_ASYNC|SND_FILENAME|SND_NODEFAULT = 0x20003)
   System::Call "${sysPlaySound} (s,,${SND_ASYNC}|${SND_FILENAME}|${SND_NODEFAULT})" "$9.wav" 

   ; Create window
   System::Call "${sysCreateWindowEx} (${WS_EX_TOOLWINDOW}, s, s,,,,,, $HWNDPARENT,,r7,) .s" "_sp" "_sp" 
   !insertmacro SINGLE_CALLBACK 1 $5 1 _systemSplashWndCB

   ; Create MSG struct
   System::Call "*${stMSG} (_) i.R9"

   ; -------------------------
repeat:
        ; Check for window
        System::Call "${sysIsWindow} (r5) .s"
        !insertmacro SINGLE_CALLBACK 2 $R8 1 _systemSplashWndCB
        IntCmp $R8 0 finish

        ; Get message
        System::Call "${sysGetMessage} (R9, r5,_) .s"
        !insertmacro SINGLE_CALLBACK 3 $R8 1 _systemSplashWndCB
        IntCmp $R8 0 finish

        ; Dispatch message
        System::Call "${sysDispatchMessage} (R9) .s"
        !insertmacro SINGLE_CALLBACK 4 $R8 1 _systemSplashWndCB

        ; Repeat dispatch cycle
        goto repeat
   ; -------------------------

finish:
   ; Stop the sound
   System::Call "${sysPlaySound} (i 0, i 0, i 0)"

   ; Delete bitmap object
   System::Call "${sysDeleteObject} (r6)"

   ; Delete the callback queue
   System::Free $3

   ; Dialog return
   StrCpy $R0 $4
   goto exit

; Exit in case of error
errorexit:
   StrCpy $R0 -1
   goto exit

exit:
   ; Restore register and put output
   System::Store "P0 l"
FunctionEnd

!verbose 4

!endif