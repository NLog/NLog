/*

WinCore.nsh & Win\*.nsh - Collection of common windows defines

!define __WIN_NOINC_xxx to exclude a windows header file
!define __WIN_MS_xxx to exclude specific things (The original #ifdef xxx checks can be found in the official Microsoft headers)

*/

!ifndef __WIN_WINDOWS__INC
!define __WIN_WINDOWS__INC
!verbose push
!verbose 3


!define __WIN_PTRSIZE 4 ;will we ever see a 64 bit version?


!include Win\WinDef.nsh
!include Win\WinError.nsh
!include Win\WinNT.nsh
!include Win\WinUser.nsh

!ifndef __WIN_MS_NOWINMESSAGES
!include WinMessages.nsh
!endif





/**************************************************
WinBase.h
**************************************************/
!ifndef __WIN_NOINC_WINBASE
!define INVALID_HANDLE_VALUE -1
!define INVALID_FILE_SIZE 0xFFFFFFFF
!define INVALID_SET_FILE_POINTER -1
!define INVALID_FILE_ATTRIBUTES -1

!define WAIT_FAILED 0xFFFFFFFF
!define WAIT_OBJECT_0       0 ;((STATUS_WAIT_0 ) + 0 )

!define WAIT_ABANDONED         0x80 ;((STATUS_ABANDONED_WAIT_0 ) + 0 )
!define WAIT_ABANDONED_0       0x80 ;((STATUS_ABANDONED_WAIT_0 ) + 0 )

!define DRIVE_UNKNOWN     0
!define DRIVE_NO_ROOT_DIR 1
!define DRIVE_REMOVABLE   2
!define DRIVE_FIXED       3
!define DRIVE_REMOTE      4
!define DRIVE_CDROM       5
!define DRIVE_RAMDISK     6

!define FILE_TYPE_UNKNOWN   0x0000
!define FILE_TYPE_DISK      0x0001
!define FILE_TYPE_CHAR      0x0002
!define FILE_TYPE_PIPE      0x0003
!define FILE_TYPE_REMOTE    0x8000

!define STD_INPUT_HANDLE   -10
!define STD_OUTPUT_HANDLE  -11
!define STD_ERROR_HANDLE   -12

#define IGNORE              0       ; Ignore signal
!define INFINITE            0xFFFFFFFF  ; Infinite timeout

!endif /* __WIN_NOINC_WINBASE */





/**************************************************
WinGDI.h
**************************************************/
!ifndef __WIN_MS_NOGDI & __WIN_NOINC_WINGDI
!define HORZRES       8 
!define VERTRES       10
!define BITSPIXEL     12
!define LOGPIXELSX    88
!define LOGPIXELSY    90
!define COLORRES     108
!define VREFRESH     116
!define DESKTOPVERTRES 117
!define DESKTOPHORZRES 118
!endif /* __WIN_MS_NOGDI & __WIN_NOINC_WINGDI */





/**************************************************
WinReg.h
**************************************************/
!ifndef __WIN_NOINC_WINREG
!ifndef __WIN_NOHKEY & HKEY_CLASSES_ROOT & HKEY_CURRENT_USER & HKEY_LOCAL_MACHINE & HKEY_USERS
!define HKEY_CLASSES_ROOT        0x80000000
!define HKEY_CURRENT_USER        0x80000001
!define HKEY_LOCAL_MACHINE       0x80000002
!define HKEY_USERS               0x80000003
!define HKEY_PERFORMANCE_DATA    0x80000004
!define HKEY_PERFORMANCE_TEXT    0x80000050
!define HKEY_PERFORMANCE_NLSTEXT 0x80000060
!define HKEY_CURRENT_CONFIG      0x80000005
!define HKEY_DYN_DATA            0x80000006
!ifndef __WIN_NOSHORTHKEY & HKCR & HKCU & HKLM
!define HKCR ${HKEY_CLASSES_ROOT}
!define HKCU ${HKEY_CURRENT_USER}
!define HKLM ${HKEY_LOCAL_MACHINE}
!endif
!endif
!endif /* __WIN_NOINC_WINREG */





/**************************************************
WindowsX.h
**************************************************/
!ifndef __WIN_NOINC_WINDOWSX
!ifndef GET_X_LPARAM & GET_Y_LPARAM
!macro _Win_GET_X_LPARAM _outvar _in
IntOp ${_outvar} "${_in}" << 16   ;We can't just use LOWORD, we need to keep the sign,
IntOp ${_outvar} ${_outvar} >> 16 ;so we let NSIS sign extend for us
!macroend
!define GET_X_LPARAM "!insertmacro _Win_GET_X_LPARAM "
!macro _Win_GET_Y_LPARAM _outvar _in
IntOp ${_outvar} "${_in}" >> 16
!macroend
!define GET_Y_LPARAM "!insertmacro _Win_GET_Y_LPARAM "
!endif
!endif /* __WIN_NOINC_WINDOWSX */





/**************************************************
ShlObj.h
**************************************************/
!ifndef __WIN_NOINC_SHLOBJ
!ifndef __WIN_NOSHELLFOLDERCSIDL
!define CSIDL_DESKTOP                 0x0000
!define CSIDL_INTERNET                0x0001 ;Internet Explorer (icon on desktop)
!define CSIDL_PROGRAMS                0x0002 ;Start Menu\Programs
!define CSIDL_CONTROLS                0x0003 ;My Computer\Control Panel
!define CSIDL_PRINTERS                0x0004 ;My Computer\Printers
!define CSIDL_PERSONAL                0x0005 ;My Documents
!define CSIDL_FAVORITES               0x0006 ;<user name>\Favorites
!define CSIDL_STARTUP                 0x0007 ;Start Menu\Programs\Startup
!define CSIDL_RECENT                  0x0008 ;<user name>\Recent
!define CSIDL_SENDTO                  0x0009 ;<user name>\SendTo
!define CSIDL_BITBUCKET               0x000a ;<desktop>\Recycle Bin
!define CSIDL_STARTMENU               0x000b ;<user name>\Start Menu
!define CSIDL_MYDOCUMENTS             0x000c ;logical "My Documents" desktop icon
!define CSIDL_MYMUSIC                 0x000d ;"My Music" folder
!define CSIDL_MYVIDEO                 0x000e ;"My Videos" folder
!define CSIDL_DESKTOPDIRECTORY        0x0010 ;<user name>\Desktop
!define CSIDL_DRIVES                  0x0011 ;My Computer
!define CSIDL_NETWORK                 0x0012 ;Network Neighborhood
!define CSIDL_NETHOOD                 0x0013 ;<user name>\nethood
!define CSIDL_FONTS                   0x0014 ;windows\fonts
!define CSIDL_TEMPLATES               0x0015
!define CSIDL_COMMON_STARTMENU        0x0016 ;All Users\Start Menu
!define CSIDL_COMMON_PROGRAMS         0x0017 ;All Users\Start Menu\Programs
!define CSIDL_COMMON_STARTUP          0x0018 ;All Users\Startup
!define CSIDL_COMMON_DESKTOPDIRECTORY 0x0019 ;All Users\Desktop
!define CSIDL_APPDATA                 0x001a ;<user name>\Application Data
!define CSIDL_PRINTHOOD               0x001b ;<user name>\PrintHood
!define CSIDL_LOCAL_APPDATA           0x001c ;<user name>\Local Settings\Applicaiton Data (non roaming)
!define CSIDL_ALTSTARTUP              0x001d ;non localized startup
!define CSIDL_COMMON_ALTSTARTUP       0x001e ;non localized common startup
!define CSIDL_COMMON_FAVORITES        0x001f
!define CSIDL_INTERNET_CACHE          0x0020
!define CSIDL_COOKIES                 0x0021
!define CSIDL_HISTORY                 0x0022
!define CSIDL_COMMON_APPDATA          0x0023 ;All Users\Application Data
!define CSIDL_WINDOWS                 0x0024 ;GetWindowsDirectory
!define CSIDL_SYSTEM                  0x0025 ;GetSystemDirectory
!define CSIDL_PROGRAM_FILES           0x0026 ;C:\Program Files
!define CSIDL_MYPICTURES              0x0027
!define CSIDL_PROFILE                 0x0028 ;USERPROFILE
!define CSIDL_SYSTEMX86               0x0029 ;x86 system directory on RISC
!define CSIDL_PROGRAM_FILESX86        0x002a ;x86 C:\Program Files on RISC
!define CSIDL_PROGRAM_FILES_COMMON    0x002b ;C:\Program Files\Common
!define CSIDL_PROGRAM_FILES_COMMONX86 0x002c ;x86 Program Files\Common on RISC
!define CSIDL_COMMON_TEMPLATES        0x002d ;All Users\Templates
!define CSIDL_COMMON_DOCUMENTS        0x002e ;All Users\Documents
!define CSIDL_COMMON_ADMINTOOLS       0x002f ;All Users\Start Menu\Programs\Administrative Tools
!define CSIDL_ADMINTOOLS              0x0030 ;<user name>\Start Menu\Programs\Administrative Tools
!define CSIDL_CONNECTIONS             0x0031 ;Network and Dial-up Connections
!define CSIDL_COMMON_MUSIC            0x0035 ;All Users\My Music
!define CSIDL_COMMON_PICTURES         0x0036 ;All Users\My Pictures
!define CSIDL_COMMON_VIDEO            0x0037 ;All Users\My Video
!define CSIDL_RESOURCES               0x0038 ;Resource Direcotry
!define CSIDL_RESOURCES_LOCALIZED     0x0039 ;Localized Resource Direcotry
!define CSIDL_COMMON_OEM_LINKS        0x003a ;Links to All Users OEM specific apps
!define CSIDL_CDBURN_AREA             0x003b ;USERPROFILE\Local Settings\Application Data\Microsoft\CD Burning
!define CSIDL_COMPUTERSNEARME         0x003d ;Computers Near Me (computered from Workgroup membership)
!define CSIDL_FLAG_CREATE             0x8000 ;combine with CSIDL_ value to force folder creation in SHGetFolderPath()
!define CSIDL_FLAG_DONT_VERIFY        0x4000 ;combine with CSIDL_ value to return an unverified folder path
!define CSIDL_FLAG_NO_ALIAS           0x1000 ;combine with CSIDL_ value to insure non-alias versions of the pidl
!define CSIDL_FLAG_PER_USER_INIT      0x0800 ;combine with CSIDL_ value to indicate per-user init (eg. upgrade)
!define CSIDL_FLAG_MASK               0xFF00
!endif /* __WIN_NOSHELLFOLDERCSIDL */
!endif /* __WIN_NOINC_SHLOBJ */




!verbose pop
!endif /* __WIN_WINDOWS__INC */