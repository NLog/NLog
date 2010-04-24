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
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "DisplayName" "NLog - Advanced .NET Logging - v${NLOGVERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "Publisher" "NLog Project"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "DisplayVersion" "${NLOGVERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "URLInfoAbout" "http://www.nlog-project.org/"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "HelpLink" "http://www.nlog-project.org/"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "Contact" "info@nlog-project.org"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteUninstaller "$INSTDIR\Uninstall.exe"
SectionEnd ; end the section

!ifdef HAVE_NET_1_0

Section "NLog for .NET 1.0 and Visual Studio.NET 2002"
  SectionIn 1 2 3
  SetOutPath $INSTDIR\bin\net-1.0
  File /r /x _svn /x .svn "build\net-1.0${OPTIONALDEBUG}\bin\*.*"
  WriteRegStr HKLM "Software\Microsoft\VisualStudio\7.0\AssemblyFolders\NLog" "" "$INSTDIR\bin\net-1.0"
SectionEnd

!endif

!ifdef HAVE_NET_1_1

Section "NLog for .NET 1.1 and Visual Studio.NET 2003"
  SectionIn 1 2
  SetOutPath $INSTDIR\bin\net-1.1
  File /r /x _svn /x .svn "build\net-1.1${OPTIONALDEBUG}\bin\*.*"
  WriteRegStr HKLM "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\NLog" "" "$INSTDIR\bin\net-1.1"

; install schema for intellisense
  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VS "VS7CommonDir"
  IfErrors novs2003
  DetailPrint "Visual Studio .NET 2003 installed in $0"
  SetOutPath "$0\Packages\schemas\xml"
  File "build\net-1.1${OPTIONALDEBUG}\bin\NLog.xsd"
novs2003:

SectionEnd

!endif

!ifdef HAVE_NET_2_0

Section "NLog for .NET 2.0 and Visual Studio 2005/2008/2010"
  SectionIn 1 2
  SetOutPath $INSTDIR\bin\net-2.0
  File /r /x _svn /x .svn "build\net-2.0${OPTIONALDEBUG}\bin\*.*"
  WriteRegStr HKLM "Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\NLog" "" "$INSTDIR\bin\net-2.0"
  WriteRegStr HKLM "Software\Microsoft\.NETFramework\v4.0\AssemblyFoldersEx\NLog" "" "$INSTDIR\bin\net-2.0"

  ; Visual Studio 2005 support

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\8.0\Setup\VS "ProductDir"
  IfErrors novs2005
  DetailPrint "Visual Studio 2005 installed in $0"
  SetOutPath "$0\xml\schemas"
  File "build\net-2.0${OPTIONALDEBUG}\bin\NLog.xsd"
novs2005:

  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VisualStudio\8.0 "UserItemTemplatesLocation"
  IfErrors novs2005_2
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual Studio 2005 item templates in $1"
  SetOutPath $1
  File "build\templates\*NLogConfig.zip"

novs2005_2:
  ReadRegStr $0 HKCU Software\Microsoft\VisualStudio\8.0 "VisualStudioLocation"
  IfErrors novs2005_3
  ExpandEnvStrings $1 $0

  IfFileExists "$1\Code Snippets\Visual C#\My Code Snippets" 0 novs2005_3
  SetOutPath "$1\Code Snippets\Visual C#\My Code Snippets"
  File "tools\VS2005Snippets\CSharp*.snippet"
  SetOutPath "$1\Code Snippets\Visual J#\My Code Snippets"
  File "tools\VS2005Snippets\VJSharp*.snippet"
  SetOutPath "$1\Code Snippets\Visual Basic\My Code Snippets"
  File "tools\VS2005Snippets\VB*.snippet"

  ; VC# Express 2005 support
novs2005_3:
  ReadRegStr $0 HKCU Software\Microsoft\VCSExpress\8.0 "VisualStudioLocation"
  IfErrors novs2005_4
  ExpandEnvStrings $1 $0

  IfFileExists "$1\Code Snippets\Visual C#\My Code Snippets" 0 novs2005_4
  SetOutPath "$1\Code Snippets\Visual C#\My Code Snippets"
  File "tools\VS2005Snippets\CSharp*.snippet"

novs2005_4:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VCSExpress\8.0 "UserItemTemplatesLocation"
  IfErrors novs2005_5
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual C# item templates in $1"
  SetOutPath $1
  File "build\templates\CSharp*NLogConfig.zip"

  ; VB.NET Express 2005 support
novs2005_5:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VBExpress\8.0 "UserItemTemplatesLocation"
  IfErrors novs2005_6
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual Basic item templates in $1"
  SetOutPath $1
  File "build\templates\VisualBasic*NLogConfig.zip"

novs2005_6:
  ReadRegStr $0 HKCU Software\Microsoft\VBExpress\8.0 "VisualStudioLocation"
  IfErrors novs2005_7
  ExpandEnvStrings $1 $0

  IfFileExists "$1\Code Snippets\Visual Basic\My Code Snippets" 0 novs2005_7
  SetOutPath "$1\Code Snippets\Visual Basic\My Code Snippets"
  File "tools\VS2005Snippets\VB*.snippet"

  ; VWD Express 2005 support
novs2005_7:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VWDExpress\8.0 "UserItemTemplatesLocation"
  IfErrors novs2005_8
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual Web Developer item templates in $1"
  SetOutPath $1
  File "build\templates\Web*NLogConfig.zip"

novs2005_8:

  ; Visual Studio 2008 support

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\9.0\Setup\VS "ProductDir"
  IfErrors novs2008
  DetailPrint "Visual Studio 2008 installed in $0"
  SetOutPath "$0\xml\schemas"
  File "build\net-2.0${OPTIONALDEBUG}\bin\NLog.xsd"
novs2008:

  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VisualStudio\9.0 "UserItemTemplatesLocation"
  IfErrors novs2008_2
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual Studio .NET 2008 item templates in $1"
  SetOutPath $1
  File "build\templates\*NLogConfig.zip"

novs2008_2:
  ReadRegStr $0 HKCU Software\Microsoft\VisualStudio\9.0 "VisualStudioLocation"
  IfErrors novs2008_3
  ExpandEnvStrings $1 $0

  IfFileExists "$1\Code Snippets\Visual C#\My Code Snippets" 0 novs2008_3
  SetOutPath "$1\Code Snippets\Visual C#\My Code Snippets"
  File "tools\VS2005Snippets\CSharp*.snippet"
  SetOutPath "$1\Code Snippets\Visual J#\My Code Snippets"
  File "tools\VS2005Snippets\VJSharp*.snippet"
  SetOutPath "$1\Code Snippets\Visual Basic\My Code Snippets"
  File "tools\VS2005Snippets\VB*.snippet"

  ; VC# Express 2008 support
novs2008_3:
  ReadRegStr $0 HKCU Software\Microsoft\VCSExpress\9.0 "VisualStudioLocation"
  IfErrors novs2008_4
  ExpandEnvStrings $1 $0

  IfFileExists "$1\Code Snippets\Visual C#\My Code Snippets" 0 novs2008_4
  SetOutPath "$1\Code Snippets\Visual C#\My Code Snippets"
  File "tools\VS2005Snippets\CSharp*.snippet"

novs2008_4:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VCSExpress\9.0 "UserItemTemplatesLocation"
  IfErrors novs2008_5
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual C# item templates in $1"
  SetOutPath $1
  File "build\templates\CSharp*NLogConfig.zip"

  ; VB.NET Express 2008 support
novs2008_5:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VBExpress\9.0 "UserItemTemplatesLocation"
  IfErrors novs2008_6
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual Basic item templates in $1"
  SetOutPath $1
  File "build\templates\VisualBasic*NLogConfig.zip"

novs2008_6:
  ReadRegStr $0 HKCU Software\Microsoft\VBExpress\9.0 "VisualStudioLocation"
  IfErrors novs2008_7
  ExpandEnvStrings $1 $0

  IfFileExists "$1\Code Snippets\Visual Basic\My Code Snippets" 0 novs2008_7
  SetOutPath "$1\Code Snippets\Visual Basic\My Code Snippets"
  File "tools\VS2005Snippets\VB*.snippet"

  ; VWD Express 2008 support
novs2008_7:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VWDExpress\9.0 "UserItemTemplatesLocation"
  IfErrors novs2008_8
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual Web Developer item templates in $1"
  SetOutPath $1
  File "build\templates\Web*NLogConfig.zip"

novs2008_8:

  ; Visual Studio 2010 support (experimental)

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\10.0\Setup\VS "ProductDir"
  IfErrors novs2010
  DetailPrint "Visual Studio 2010 installed in $0"
  SetOutPath "$0\xml\schemas"
  File "build\net-2.0${OPTIONALDEBUG}\bin\NLog.xsd"
novs2010:

  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VisualStudio\10.0 "UserItemTemplatesLocation"
  IfErrors novs2010_2
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual Studio .NET 2010 item templates in $1"
  SetOutPath $1
  File "build\templates\*NLogConfig.zip"

novs2010_2:
  ReadRegStr $0 HKCU Software\Microsoft\VisualStudio\10.0 "VisualStudioLocation"
  IfErrors novs2010_3
  ExpandEnvStrings $1 $0

  IfFileExists "$1\Code Snippets\Visual C#\My Code Snippets" 0 novs2010_3
  SetOutPath "$1\Code Snippets\Visual C#\My Code Snippets"
  File "tools\VS2005Snippets\CSharp*.snippet"
  SetOutPath "$1\Code Snippets\Visual J#\My Code Snippets"
  File "tools\VS2005Snippets\VJSharp*.snippet"
  SetOutPath "$1\Code Snippets\Visual Basic\My Code Snippets"
  File "tools\VS2005Snippets\VB*.snippet"

  ; VC# Express 2010 support
novs2010_3:
  ReadRegStr $0 HKCU Software\Microsoft\VCSExpress\10.0 "VisualStudioLocation"
  IfErrors novs2010_4
  ExpandEnvStrings $1 $0

  IfFileExists "$1\Code Snippets\Visual C#\My Code Snippets" 0 novs2010_4
  SetOutPath "$1\Code Snippets\Visual C#\My Code Snippets"
  File "tools\VS2005Snippets\CSharp*.snippet"

novs2010_4:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VCSExpress\10.0 "UserItemTemplatesLocation"
  IfErrors novs2010_5
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual C# item templates in $1"
  SetOutPath $1
  File "build\templates\CSharp*NLogConfig.zip"

  ; VB.NET Express 2010 support
novs2010_5:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VBExpress\10.0 "UserItemTemplatesLocation"
  IfErrors novs2010_6
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual Basic item templates in $1"
  SetOutPath $1
  File "build\templates\VisualBasic*NLogConfig.zip"

novs2010_6:
  ReadRegStr $0 HKCU Software\Microsoft\VBExpress\10.0 "VisualStudioLocation"
  IfErrors novs2010_7
  ExpandEnvStrings $1 $0

  IfFileExists "$1\Code Snippets\Visual Basic\My Code Snippets" 0 novs2010_7
  SetOutPath "$1\Code Snippets\Visual Basic\My Code Snippets"
  File "tools\VS2005Snippets\VB*.snippet"

  ; VWD Express 2010 support
novs2010_7:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VWDExpress\10.0 "UserItemTemplatesLocation"
  IfErrors novs2010_8
  ExpandEnvStrings $1 $0
  DetailPrint "Installing Visual Web Developer item templates in $1"
  SetOutPath $1
  File "build\templates\Web*NLogConfig.zip"

novs2010_8:


SectionEnd

!endif

!ifdef HAVE_NETCF_1_0

Section "NLog for .NET Compact Framework 1.0"
  SectionIn 1
  SetOutPath $INSTDIR\bin\netcf-1.0
  File /r /x _svn /x .svn "build\netcf-1.0${OPTIONALDEBUG}\bin\*.*"
SectionEnd

!endif

!ifdef HAVE_NETCF_2_0

Section "NLog for .NET Compact Framework 2.0"
  SectionIn 1
  SetOutPath $INSTDIR\bin\netcf-2.0
  File /r /x _svn /x .svn "build\netcf-2.0${OPTIONALDEBUG}\bin\*.*"
SectionEnd

!endif

!ifdef HAVE_MONO_1_0

Section "NLog for Mono 1.0 Profile"
  SectionIn 1
  SetOutPath $INSTDIR\bin\mono-1.0
  File /r /x _svn /x .svn "build\mono-1.0${OPTIONALDEBUG}\bin\*.*"
SectionEnd

!endif

!ifdef HAVE_MONO_2_0

Section "NLog for Mono 2.0 Profile"
  SectionIn 1
  SetOutPath $INSTDIR\bin\mono-2.0
  File /r /x _svn /x .svn "build\mono-2.0${OPTIONALDEBUG}\bin\*.*"
SectionEnd

!endif

Section "Examples"
  SectionIn 1
  SetOutPath $INSTDIR\examples
  File /r /x _svn /x .svn examples\*.*
SectionEnd

Section "Documentation"
  SectionIn 1 2
  SetOutPath $INSTDIR\help
  File build\doc\help\NLog.chm
SectionEnd

Section "Uninstall"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog"
!ifdef HAVE_NET_1_0
  ; .NET Framework 1.0 Cleanup
  DeleteRegKey HKLM "Software\Microsoft\VisualStudio\7.0\AssemblyFolders\NLog"
!endif
!ifdef HAVE_NET_1_1
  ; .NET Framework 1.1 Cleanup
  DeleteRegKey HKLM "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\NLog"
!endif
!ifdef HAVE_NET_2_0
  ; .NET Framework 2.0 Cleanup
  DeleteRegKey HKLM "Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\NLog"
  DeleteRegKey HKLM "Software\Microsoft\.NETFramework\v4.0\AssemblyFoldersEx\NLog"
!endif

 ; Visual Studio.NET 2003 Cleanup

!ifdef HAVE_NET_1_1
  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VS "VS7CommonDir"
  IfErrors novsnet2003
  Delete "$0\Packages\schemas\xml\NLog.xsd"

novsnet2003:
!endif

!ifdef HAVE_NET_2_0
 ; Visual Studio 2005 Cleanup

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\8.0\Setup\VS "ProductDir"
  IfErrors novsnet2005_1
  Delete "$0\xml\schemas\NLog.xsd"

novsnet2005_1:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VisualStudio\8.0 "UserItemTemplatesLocation"
  IfErrors novsnet2005_2
  ExpandEnvStrings $1 $0
  Delete "$1\*NLogConfig.zip"

novsnet2005_2:

  ; Visual Studio 2008 Cleanup

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\9.0\Setup\VS "ProductDir"
  IfErrors novsnet2008_1
  Delete "$0\xml\schemas\NLog.xsd"

novsnet2008_1:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VisualStudio\9.0 "UserItemTemplatesLocation"
  IfErrors novsnet2008_2
  ExpandEnvStrings $1 $0
  Delete "$1\*NLogConfig.zip"

novsnet2008_2:

  ; Visual Studio 2010 Cleanup

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\10.0\Setup\VS "ProductDir"
  IfErrors novsnet2010_1
  Delete "$0\xml\schemas\NLog.xsd"

novsnet2010_1:
  ClearErrors
  ReadRegStr $0 HKCU Software\Microsoft\VisualStudio\10.0 "UserItemTemplatesLocation"
  IfErrors novsnet2010_2
  ExpandEnvStrings $1 $0
  Delete "$1\*NLogConfig.zip"

novsnet2010_2:

!endif

  Delete "$SMPROGRAMS\NLog\*.lnk"
  RMDir "$SMPROGRAMS\NLog"

  RMDir /r "$INSTDIR"
SectionEnd
