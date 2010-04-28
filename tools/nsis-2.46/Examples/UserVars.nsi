; UserVars.nsi
;
; This script shows you how to declare and user variables.

;--------------------------------

  Name "User Variables Text"
  OutFile "UserVars.exe"
  
  InstallDir "$PROGRAMFILES\User Variables Test"
  
  RequestExecutionLevel admin
  
;--------------------------------

  ;Pages
  Page directory
  Page instfiles
  
  UninstPage uninstConfirm
  UninstPage instfiles

;--------------------------------
; Declaration of user variables (Var command), allowed charaters for variables names : [a-z][A-Z][0-9] and '_'

  Var "Name"
  Var "Serial"
  Var "Info"

;--------------------------------
; Installer

Section "Dummy Section" SecDummy

     StrCpy $0 "Admin"
     StrCpy "$Name" $0
     StrCpy "$Serial" "12345"
     MessageBox MB_OK "User Name: $Name $\n$\nSerial Number: $Serial"

     CreateDirectory $INSTDIR
     WriteUninstaller "$INSTDIR\Uninst.exe"
     
SectionEnd

Section "Another Section"

     Var /GLOBAL "AnotherVar"

     StrCpy $AnotherVar "test"

SectionEnd

;--------------------------------
; Uninstaller

Section "Uninstall"

     StrCpy $Info "User variables test uninstalled successfully."
     Delete "$INSTDIR\Uninst.exe"
     RmDir $INSTDIR

SectionEnd

Function un.OnUninstSuccess

     HideWindow
     MessageBox MB_OK "$Info"
     
FunctionEnd
