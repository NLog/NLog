# This example tests the compile time aspect of the Library macros
# more than the runtime aspect. It is more of a syntax example,
# rather than a usage example.

!include "Library.nsh"

Name "Library Test"
OutFile "Library Test.exe"

InstallDir "$TEMP\Library Test"

Page directory
Page instfiles

XPStyle on

RequestExecutionLevel user

!define TestDLL '"${NSISDIR}\Plugins\LangDLL.dll"'
!define TestEXE '"${NSISDIR}\Contrib\UIs\default.exe"'

Section

!insertmacro InstallLib DLL       NOTSHARED REBOOT_PROTECTED      ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib DLL       NOTSHARED NOREBOOT_PROTECTED    ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib DLL       NOTSHARED REBOOT_NOTPROTECTED   ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib DLL       NOTSHARED NOREBOOT_NOTPROTECTED ${TestDLL} $INSTDIR\test.dll $INSTDIR

!insertmacro InstallLib REGDLL    NOTSHARED REBOOT_PROTECTED      ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLL    NOTSHARED NOREBOOT_PROTECTED    ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLL    NOTSHARED REBOOT_NOTPROTECTED   ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLL    NOTSHARED NOREBOOT_NOTPROTECTED ${TestDLL} $INSTDIR\test.dll $INSTDIR

!insertmacro InstallLib TLB       NOTSHARED REBOOT_PROTECTED      ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib TLB       NOTSHARED NOREBOOT_PROTECTED    ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib TLB       NOTSHARED REBOOT_NOTPROTECTED   ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib TLB       NOTSHARED NOREBOOT_NOTPROTECTED ${TestDLL} $INSTDIR\test.dll $INSTDIR

!insertmacro InstallLib REGDLLTLB NOTSHARED REBOOT_PROTECTED      ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLLTLB NOTSHARED NOREBOOT_PROTECTED    ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLLTLB NOTSHARED REBOOT_NOTPROTECTED   ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLLTLB NOTSHARED NOREBOOT_NOTPROTECTED ${TestDLL} $INSTDIR\test.dll $INSTDIR

!insertmacro InstallLib DLL       $0        REBOOT_PROTECTED      ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib DLL       $0        NOREBOOT_PROTECTED    ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib DLL       $0        REBOOT_NOTPROTECTED   ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib DLL       $0        NOREBOOT_NOTPROTECTED ${TestDLL} $INSTDIR\test.dll $INSTDIR

!insertmacro InstallLib REGDLL    $0        REBOOT_PROTECTED      ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLL    $0        NOREBOOT_PROTECTED    ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLL    $0        REBOOT_NOTPROTECTED   ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLL    $0        NOREBOOT_NOTPROTECTED ${TestDLL} $INSTDIR\test.dll $INSTDIR

!insertmacro InstallLib TLB       $0        REBOOT_PROTECTED      ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib TLB       $0        NOREBOOT_PROTECTED    ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib TLB       $0        REBOOT_NOTPROTECTED   ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib TLB       $0        NOREBOOT_NOTPROTECTED ${TestDLL} $INSTDIR\test.dll $INSTDIR

!insertmacro InstallLib REGDLLTLB $0        REBOOT_PROTECTED      ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLLTLB $0        NOREBOOT_PROTECTED    ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLLTLB $0        REBOOT_NOTPROTECTED   ${TestDLL} $INSTDIR\test.dll $INSTDIR
!insertmacro InstallLib REGDLLTLB $0        NOREBOOT_NOTPROTECTED ${TestDLL} $INSTDIR\test.dll $INSTDIR

!insertmacro InstallLib REGEXE    $0        REBOOT_PROTECTED      ${TestEXE} $INSTDIR\test.exe $INSTDIR
!insertmacro InstallLib REGEXE    $0        NOREBOOT_PROTECTED    ${TestEXE} $INSTDIR\test.exe $INSTDIR
!insertmacro InstallLib REGEXE    $0        REBOOT_NOTPROTECTED   ${TestEXE} $INSTDIR\test.exe $INSTDIR
!insertmacro InstallLib REGEXE    $0        NOREBOOT_NOTPROTECTED ${TestEXE} $INSTDIR\test.exe $INSTDIR

WriteUninstaller $INSTDIR\uninstall.exe

SectionEnd

Section uninstall

!insertmacro UninstallLib DLL       NOTSHARED NOREMOVE               $INSTDIR\test.dll
!insertmacro UninstallLib DLL       NOTSHARED REBOOT_PROTECTED       $INSTDIR\test.dll
!insertmacro UninstallLib DLL       NOTSHARED NOREBOOT_PROTECTED     $INSTDIR\test.dll
!insertmacro UninstallLib DLL       NOTSHARED REBOOT_NOTPROTECTED    $INSTDIR\test.dll
!insertmacro UninstallLib DLL       NOTSHARED NOREBOOT_NOTPROTECTED  $INSTDIR\test.dll

!insertmacro UninstallLib REGDLL    NOTSHARED NOREMOVE               $INSTDIR\test.dll
!insertmacro UninstallLib REGDLL    NOTSHARED REBOOT_PROTECTED       $INSTDIR\test.dll
!insertmacro UninstallLib REGDLL    NOTSHARED NOREBOOT_PROTECTED     $INSTDIR\test.dll
!insertmacro UninstallLib REGDLL    NOTSHARED REBOOT_NOTPROTECTED    $INSTDIR\test.dll
!insertmacro UninstallLib REGDLL    NOTSHARED NOREBOOT_NOTPROTECTED  $INSTDIR\test.dll

!insertmacro UninstallLib TLB       NOTSHARED NOREMOVE               $INSTDIR\test.dll
!insertmacro UninstallLib TLB       NOTSHARED REBOOT_PROTECTED       $INSTDIR\test.dll
!insertmacro UninstallLib TLB       NOTSHARED NOREBOOT_PROTECTED     $INSTDIR\test.dll
!insertmacro UninstallLib TLB       NOTSHARED REBOOT_NOTPROTECTED    $INSTDIR\test.dll
!insertmacro UninstallLib TLB       NOTSHARED NOREBOOT_NOTPROTECTED  $INSTDIR\test.dll

!insertmacro UninstallLib REGDLLTLB NOTSHARED NOREMOVE               $INSTDIR\test.dll
!insertmacro UninstallLib REGDLLTLB NOTSHARED REBOOT_PROTECTED       $INSTDIR\test.dll
!insertmacro UninstallLib REGDLLTLB NOTSHARED NOREBOOT_PROTECTED     $INSTDIR\test.dll
!insertmacro UninstallLib REGDLLTLB NOTSHARED REBOOT_NOTPROTECTED    $INSTDIR\test.dll
!insertmacro UninstallLib REGDLLTLB NOTSHARED NOREBOOT_NOTPROTECTED  $INSTDIR\test.dll

!insertmacro UninstallLib DLL       SHARED    NOREMOVE               $INSTDIR\test.dll
!insertmacro UninstallLib DLL       SHARED    REBOOT_PROTECTED       $INSTDIR\test.dll
!insertmacro UninstallLib DLL       SHARED    NOREBOOT_PROTECTED     $INSTDIR\test.dll
!insertmacro UninstallLib DLL       SHARED    REBOOT_NOTPROTECTED    $INSTDIR\test.dll
!insertmacro UninstallLib DLL       SHARED    NOREBOOT_NOTPROTECTED  $INSTDIR\test.dll

!insertmacro UninstallLib REGDLL    SHARED    NOREMOVE               $INSTDIR\test.dll
!insertmacro UninstallLib REGDLL    SHARED    REBOOT_PROTECTED       $INSTDIR\test.dll
!insertmacro UninstallLib REGDLL    SHARED    NOREBOOT_PROTECTED     $INSTDIR\test.dll
!insertmacro UninstallLib REGDLL    SHARED    REBOOT_NOTPROTECTED    $INSTDIR\test.dll
!insertmacro UninstallLib REGDLL    SHARED    NOREBOOT_NOTPROTECTED  $INSTDIR\test.dll

!insertmacro UninstallLib TLB       SHARED    NOREMOVE               $INSTDIR\test.dll
!insertmacro UninstallLib TLB       SHARED    REBOOT_PROTECTED       $INSTDIR\test.dll
!insertmacro UninstallLib TLB       SHARED    NOREBOOT_PROTECTED     $INSTDIR\test.dll
!insertmacro UninstallLib TLB       SHARED    REBOOT_NOTPROTECTED    $INSTDIR\test.dll
!insertmacro UninstallLib TLB       SHARED    NOREBOOT_NOTPROTECTED  $INSTDIR\test.dll

!insertmacro UninstallLib REGDLLTLB SHARED    NOREMOVE               $INSTDIR\test.dll
!insertmacro UninstallLib REGDLLTLB SHARED    REBOOT_PROTECTED       $INSTDIR\test.dll
!insertmacro UninstallLib REGDLLTLB SHARED    NOREBOOT_PROTECTED     $INSTDIR\test.dll
!insertmacro UninstallLib REGDLLTLB SHARED    REBOOT_NOTPROTECTED    $INSTDIR\test.dll
!insertmacro UninstallLib REGDLLTLB SHARED    NOREBOOT_NOTPROTECTED  $INSTDIR\test.dll

!insertmacro UninstallLib REGEXE    SHARED    NOREMOVE               $INSTDIR\test.exe
!insertmacro UninstallLib REGEXE    SHARED    REBOOT_PROTECTED       $INSTDIR\test.exe
!insertmacro UninstallLib REGEXE    SHARED    NOREBOOT_PROTECTED     $INSTDIR\test.exe
!insertmacro UninstallLib REGEXE    SHARED    REBOOT_NOTPROTECTED    $INSTDIR\test.exe
!insertmacro UninstallLib REGEXE    SHARED    NOREBOOT_NOTPROTECTED  $INSTDIR\test.exe

SectionEnd
