/*

VB6RunTime.nsh

Setup of Visual Basic 6.0 run-time files, including the Oleaut32.dll security update

Copyright 2008-2009 Joost Verburg

To obtain the run-time files, download and extract
http://nsis.sourceforge.net/vb6runtime.zip

Script code for installation:

!insertmacro InstallVB6RunTime FOLDER ALREADY_INSTALLED

in which FOLDER is the location of the run-time files and ALREADY_INSTALLED is the
name of a variable that is empty when the application is installed for the first time
and non-empty otherwise

Script code for uninstallation:

!insertmacro UnInstallVB6RunTime

Remarks:

* You may have to install additional files for such Visual Basic application to work,
  such as OCX files for user interface controls.
  
* Installation of the run-time files requires Administrator or Power User privileges.
  Use the Multi-User header file to verify whether these privileges are available.

* Add a Modern UI finish page or another check (see IfRebootFlag in the NSIS Users
  Manual) to allow the user to restart the computer when necessary.

*/

!ifndef VB6_INCLUDED
!define VB6_INCLUDED
!verbose push
!verbose 3

!include Library.nsh
!include WinVer.nsh

!macro VB6RunTimeInstall FOLDER ALREADY_INSTALLED

  !insertmacro InstallLib REGDLL "${ALREADY_INSTALLED}" REBOOT_PROTECTED "${FOLDER}\msvbvm60.dll" "$SYSDIR\msvbvm60.dll" "$SYSDIR"
  
  ;The files below will only be installed on Win9x/NT4
  
  !insertmacro InstallLib REGDLL "${ALREADY_INSTALLED}" REBOOT_PROTECTED    "${FOLDER}\olepro32.dll" "$SYSDIR\olepro32.dll" "$SYSDIR"
  !insertmacro InstallLib REGDLL "${ALREADY_INSTALLED}" REBOOT_PROTECTED    "${FOLDER}\comcat.dll"   "$SYSDIR\comcat.dll"   "$SYSDIR"
  !insertmacro InstallLib DLL    "${ALREADY_INSTALLED}" REBOOT_PROTECTED    "${FOLDER}\asycfilt.dll" "$SYSDIR\asycfilt.dll" "$SYSDIR"
  !insertmacro InstallLib TLB    "${ALREADY_INSTALLED}" REBOOT_PROTECTED    "${FOLDER}\stdole2.tlb"  "$SYSDIR\stdole2.tlb"  "$SYSDIR"
  
  Push $R0
  
  ${if} ${IsNT}
    ${if} ${IsWinNT4}
      ReadRegStr $R0 HKLM "System\CurrentControlSet\Control" "ProductOptions"
      ${if} $R0 == "Terminal Server"
        !insertmacro InstallLib REGDLL "${ALREADY_INSTALLED}" REBOOT_NOTPROTECTED "${FOLDER}\NT4TS\oleaut32.dll" "$SYSDIR\oleaut32.dll" "$SYSDIR"
      ${else}
        !insertmacro InstallLib REGDLL "${ALREADY_INSTALLED}" REBOOT_NOTPROTECTED "${FOLDER}\NT4\oleaut32.dll" "$SYSDIR\oleaut32.dll" "$SYSDIR"
      ${endif}
    ${endif}
  ${else}
    ;No Oleaut32.dll with the security update has been released for Windows 9x.
    ;The NT4 version is used because NT4 and Win9x used to share the same 2.40 version
    ;and version 2.40.4519.0 is reported to work fine on Win9x.
    !insertmacro InstallLib REGDLL "${ALREADY_INSTALLED}" REBOOT_NOTPROTECTED "${FOLDER}\NT4\oleaut32.dll" "$SYSDIR\oleaut32.dll" "$SYSDIR"
  ${endif}
  
  Pop $R0

!macroend

!macro VB6RunTimeUnInstall

   !insertmacro UnInstallLib REGDLL SHARED NOREMOVE "$SYSDIR\msvbvm60.dll"
   !insertmacro UnInstallLib REGDLL SHARED NOREMOVE "$SYSDIR\oleaut32.dll"
   !insertmacro UnInstallLib REGDLL SHARED NOREMOVE "$SYSDIR\olepro32.dll"
   !insertmacro UnInstallLib REGDLL SHARED NOREMOVE "$SYSDIR\comcat.dll"
   !insertmacro UnInstallLib DLL    SHARED NOREMOVE "$SYSDIR\asycfilt.dll"
   !insertmacro UnInstallLib TLB    SHARED NOREMOVE "$SYSDIR\stdole2.tlb"

!macroend

!verbose pop
!endif
