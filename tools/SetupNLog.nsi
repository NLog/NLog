!cd ..

; The name of the installer
Name "NLog"

SetCompress force
SetCompressor lzma

; The file to write
OutFile "SetupNLog.exe"

; The default installation directory
InstallDir $PROGRAMFILES\NLog

; The text to prompt the user to enter a directory
DirText "This will install NLog Library and tools on your computer. Choose a directory:"

; The stuff to install
Section "Main"
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  File LICENSE.txt

  SetOutPath $INSTDIR\bin
  ; Put file there
  File /r build\net-1.0\bin\*.*

  SetOutPath $INSTDIR\doc
  File build\net-1.0\doc\*.*

  SetOutPath $INSTDIR\doc\help
  File build\net-1.0\doc\help\NLog.chm

  CreateDirectory "$SMPROGRAMS\NLog"
  CreateShortCut  "$SMPROGRAMS\NLog\Uninstall.lnk" "$INSTDIR\Uninstall.exe" ""
  CreateShortCut  "$SMPROGRAMS\NLog\LICENSE.lnk" "$INSTDIR\License.txt" ""
  CreateShortCut  "$SMPROGRAMS\NLog\Class Library Reference.lnk" "$INSTDIR\doc\help\NLog.chm" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Tutorial.lnk" "$INSTDIR\doc\tutorial.html" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Documentation.lnk" "$INSTDIR\doc\index.html" ""

  WriteRegStr HKCU "Software\Microsoft\VisualStudio\7.0\AssemblyFolders\NLog" "" "$INSTDIR\Bin"
  WriteRegStr HKCU "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\NLog" "" "$INSTDIR\Bin"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "" ""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "DisplayName" "NLog Class Library"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ExecShell open '$SMPROGRAMS\NLog'
SectionEnd ; end the section

Section "Uninstall"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog"
  DeleteRegKey HKCU "Software\Microsoft\VisualStudio\7.0\AssemblyFolders\NLog"
  DeleteRegKey HKCU "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\NLog"

  Delete "$SMPROGRAMS\NLog\*.lnk"
  RMDir "$SMPROGRAMS\NLog"

  RMDir /r "$INSTDIR\bin"
  RMDir /r "$INSTDIR\doc\help"
  RMDir /r "$INSTDIR\doc"
  Delete "$INSTDIR\*.*"
  RMDir "$INSTDIR"
SectionEnd
; eof
