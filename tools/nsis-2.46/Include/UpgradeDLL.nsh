/*

NOTE:
-----
This macro is provided for backwards compatibility with NSIS 2.0 scripts.
It's recommended you update your scripts to use the new Library.nsh macros.


Macro - Upgrade DLL File
Written by Joost Verburg
------------------------

Parameters:
LOCALFILE		Location of the new DLL file (on the compiler system)
DESTFILE		Location of the DLL file that should be upgraded (on the user's system)
TEMPBASEDIR		Directory on the user's system to store a temporary file when the system has
				to be rebooted.
				For Win9x/ME support, this should be on the same volume as DESTFILE.
				The Windows temp directory could be located on any volume, so you cannot use
				this directory.

Define UPGRADEDLL_NOREGISTER if you want to upgrade a DLL that does not have to be registered.

Notes:

* If you want to support Windows 9x/ME, you can only use short filenames (8.3).

* This macro uses the GetDLLVersionLocal command to retrieve the version of local libraries.
  This command is only supported when compiling on a Windows system.

------------------------

Example:

!insertmacro UpgradeDLL "dllname.dll" "$SYSDIR\dllname.dll" "$SYSDIR"

*/

!ifndef UPGRADEDLL_INCLUDED

!define UPGRADEDLL_INCLUDED

!macro __UpgradeDLL_Helper_AddRegToolEntry mode filename tempdir

  Push $R0
  Push $R1
  Push $R2
  Push $R3

  ;------------------------
  ;Copy the parameters

  Push "${filename}"
  Push "${tempdir}"

  Pop $R2 ; temporary directory
  Pop $R1 ; file name to register

  ;------------------------
  ;Advance counter

  StrCpy $R0 0
  ReadRegDWORD $R0 HKLM "Software\NSIS.Library.RegTool.v2\UpgradeDLLSession" "count"
  IntOp $R0 $R0 + 1
  WriteRegDWORD HKLM "Software\NSIS.Library.RegTool.v2\UpgradeDLLSession" "count" "$R0"

  ;------------------------
  ;Setup RegTool

  ReadRegStr $R3 HKLM "Software\Microsoft\Windows\CurrentVersion\RunOnce" "NSIS.Library.RegTool.v2"
  StrCpy $R3 $R3 -4 1
  IfFileExists $R3 +3

    File /oname=$R2\NSIS.Library.RegTool.v2.$HWNDPARENT.exe "${NSISDIR}\Bin\RegTool.bin"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\RunOnce" \
      "NSIS.Library.RegTool.v2" '"$R2\NSIS.Library.RegTool.v2.$HWNDPARENT.exe" /S'

  ;------------------------
  ;Add RegTool entry

  WriteRegStr HKLM "Software\NSIS.Library.RegTool.v2\UpgradeDLLSession" "$R0.file" "$R1"
  WriteRegStr HKLM "Software\NSIS.Library.RegTool.v2\UpgradeDLLSession" "$R0.mode" "${mode}"

  Pop $R3
  Pop $R2
  Pop $R1
  Pop $R0

!macroend

!macro UpgradeDLL LOCALFILE DESTFILE TEMPBASEDIR

  Push $R0
  Push $R1
  Push $R2
  Push $R3
  Push $R4
  Push $R5

  !define UPGRADEDLL_UNIQUE "${__FILE__}${__LINE__}"

  SetOverwrite try

  ;------------------------
  ;Copy the parameters used on run-time to a variable
  ;This allows the usage of variables as paramter

  StrCpy $R4 "${DESTFILE}"
  StrCpy $R5 "${TEMPBASEDIR}"

  ;------------------------
  ;Get version information

  IfFileExists $R4 0 "upgradedll.copy_${UPGRADEDLL_UNIQUE}"

  ClearErrors
    GetDLLVersionLocal "${LOCALFILE}" $R0 $R1
    GetDLLVersion $R4 $R2 $R3
  IfErrors "upgradedll.upgrade_${UPGRADEDLL_UNIQUE}"

  IntCmpU $R0 $R2 0 "upgradedll.done_${UPGRADEDLL_UNIQUE}" "upgradedll.upgrade_${UPGRADEDLL_UNIQUE}"
  IntCmpU $R1 $R3 "upgradedll.done_${UPGRADEDLL_UNIQUE}" "upgradedll.done_${UPGRADEDLL_UNIQUE}" \
    "upgradedll.upgrade_${UPGRADEDLL_UNIQUE}"

  ;------------------------
  ;Upgrade

  "upgradedll.upgrade_${UPGRADEDLL_UNIQUE}:"
    !ifndef UPGRADEDLL_NOREGISTER
      ;Unregister the DLL
      UnRegDLL $R4
    !endif

  ;------------------------
  ;Copy

  ClearErrors
    StrCpy $R0 $R4
    Call ":upgradedll.file_${UPGRADEDLL_UNIQUE}"
  IfErrors 0 "upgradedll.noreboot_${UPGRADEDLL_UNIQUE}"

  ;------------------------
  ;Copy on reboot

  GetTempFileName $R0 $R5
    Call ":upgradedll.file_${UPGRADEDLL_UNIQUE}"
  Rename /REBOOTOK $R0 $R4

  ;------------------------
  ;Register on reboot

  !insertmacro __UpgradeDLL_Helper_AddRegToolEntry 'D' $R4 $R5

  Goto "upgradedll.done_${UPGRADEDLL_UNIQUE}"

  ;------------------------
  ;DLL does not exist

  "upgradedll.copy_${UPGRADEDLL_UNIQUE}:"
    StrCpy $R0 $R4
    Call ":upgradedll.file_${UPGRADEDLL_UNIQUE}"

  ;------------------------
  ;Register

  "upgradedll.noreboot_${UPGRADEDLL_UNIQUE}:"
    !ifndef UPGRADEDLL_NOREGISTER
      RegDLL $R4
    !endif

  ;------------------------
  ;Done

  "upgradedll.done_${UPGRADEDLL_UNIQUE}:"

  Pop $R5
  Pop $R4
  Pop $R3
  Pop $R2
  Pop $R1
  Pop $R0

  ;------------------------
  ;End

  Goto "upgradedll.end_${UPGRADEDLL_UNIQUE}"

  ;------------------------
  ;Extract

  "upgradedll.file_${UPGRADEDLL_UNIQUE}:"
    File /oname=$R0 "${LOCALFILE}"
    Return

  "upgradedll.end_${UPGRADEDLL_UNIQUE}:"

  SetOverwrite lastused
  
  !undef UPGRADEDLL_UNIQUE

!macroend

!endif
