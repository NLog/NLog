SetCompress force
SetCompressor lzma

!include mui.nsh
!cd ..

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING
  !define MUI_COMPONENTSPAGE_SMALLDESC

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE License.txt
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
  !insertmacro MUI_LANGUAGE "English"

; The name of the installer
Name "NLog ${NLOGVERSION}"

; The file to write
OutFile "SetupNLog.exe"

; The default installation directory
InstallDir $PROGRAMFILES\NLog

; The text to prompt the user to enter a directory
DirText "This will install NLog version ${NLOGVERSION} on your computer. Choose a directory:"

InstType "Full"
InstType "Typical"
InstType "Minimal"

; The stuff to install
Section "NLog Core Files"
  SectionIn 1 2 3 RO
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  File LICENSE.txt

  CreateDirectory "$SMPROGRAMS\NLog"
  CreateShortCut  "$SMPROGRAMS\NLog\Uninstall.lnk" "$INSTDIR\Uninstall.exe" ""
  CreateShortCut  "$SMPROGRAMS\NLog\LICENSE.lnk" "$INSTDIR\License.txt" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Documentation.lnk" "$INSTDIR\help\NLog.chm" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Configuration Examples.lnk" "$INSTDIR\examples" ""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "" ""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "DisplayName" "NLog - A .NET Logging Library"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ExecShell open '$SMPROGRAMS\NLog'
SectionEnd ; end the section

SectionGroup ".NET Framework Support"

Section ".NET 1.0 / Visual Studio.NET 2002 (supports all frameworks)"
  SectionIn 1 2 3
  SetOutPath $INSTDIR\bin\net-1.0
  File /r /x _svn "build\net-1.0${OPTIONALDEBUG}\bin\*.*"
  WriteRegStr HKLM "Software\Microsoft\VisualStudio\7.0\AssemblyFolders\NLog" "" "$INSTDIR\bin\net-1.0"
SectionEnd

Section ".NET 1.1 / Visual Studio.NET 2003"
  SectionIn 1 2
  SetOutPath $INSTDIR\bin\net-1.1
  File /r /x _svn "build\net-1.1${OPTIONALDEBUG}\bin\*.*"
  WriteRegStr HKLM "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\NLog" "" "$INSTDIR\bin\net-1.1"

; install schema for intellisense
  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VS "VS7CommonDir"
  IfErrors novsnet
  DetailPrint "Visual Studio .NET 2003 installed in $0"
  SetOutPath "$0\Packages\schemas\xml"
  File "build\net-1.1${OPTIONALDEBUG}\bin\NLog.xsd"
novsnet:

SectionEnd

Section ".NET 2.0 / Visual Studio 2005"
  SectionIn 1 2
  SetOutPath $INSTDIR\bin\net-2.0
  File /r /x _svn "build\net-2.0${OPTIONALDEBUG}\bin\*.*"
  WriteRegStr HKLM "Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\NLog" "" "$INSTDIR\bin\net-2.0"

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\8.0\Setup\VS "ProductDir"
  IfErrors novsnet
  DetailPrint "Visual Studio .NET 2005 installed in $0"
  SetOutPath "$0\xml\schemas"
  File "build\net-2.0${OPTIONALDEBUG}\bin\NLog.xsd"
novsnet:

SectionEnd

SectionGroupEnd

SectionGroup ".NET Compact Framework Support"

Section ".NET Compact Framework 1.0 Support"
  SectionIn 1
  SetOutPath $INSTDIR\bin\netcf-1.0
  File /r /x _svn "build\netcf-1.0${OPTIONALDEBUG}\bin\*.*"
SectionEnd

Section ".NET Compact Framework 2.0 Support"
  SectionIn 1
  SetOutPath $INSTDIR\bin\netcf-2.0
  File /r /x _svn "build\netcf-2.0${OPTIONALDEBUG}\bin\*.*"
SectionEnd

SectionGroupEnd

SectionGroup "Mono Support"

Section "Mono 1.0 Profile Support"
  SectionIn 1
  SetOutPath $INSTDIR\bin\mono-1.0
  File /r /x _svn "build\mono-1.0${OPTIONALDEBUG}\bin\*.*"
SectionEnd

Section "Mono 2.0 Profile Support"
  SectionIn 1
  SetOutPath $INSTDIR\bin\mono-2.0
  File /r /x _svn "build\mono-2.0${OPTIONALDEBUG}\bin\*.*"
SectionEnd

SectionGroupEnd

Section "Examples"
  SectionIn 1
  SetOutPath $INSTDIR\examples
  File /r /x _svn examples\*.*
SectionEnd

Section "Documentation"
  SectionIn 1 2
  SetOutPath $INSTDIR\help
  File build\doc\help\NLog.chm
SectionEnd

Section "Uninstall"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog"
  DeleteRegKey HKLM "Software\Microsoft\VisualStudio\7.0\AssemblyFolders\NLog"
  DeleteRegKey HKLM "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\NLog"
  DeleteRegKey HKLM "Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\NLog"

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VS "VS7CommonDir"
  IfErrors novsnet2
  Delete "$0\Packages\schemas\xml\NLog.xsd"

novsnet2:
  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\8.0\Setup\VS "ProductDir"
  IfErrors novsnet3
  Delete "$0\xml\schemas\NLog.xsd"

novsnet3:
  Delete "$SMPROGRAMS\NLog\*.lnk"
  RMDir "$SMPROGRAMS\NLog"

  RMDir /r "$INSTDIR"
SectionEnd
; eof
