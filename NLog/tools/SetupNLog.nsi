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
  File License.txt

  SetOutPath $INSTDIR\bin
  ; Put file there
  File bin\net-1.0\NLog*.dll
  File bin\net-1.0\NLog*.xml

  SetOutPath $INSTDIR\docs

  File /oname=NLog.chm doc\help\Documentation.chm
  File doc\website\*.*

  CreateDirectory "$SMPROGRAMS\NLog"
  CreateShortCut  "$SMPROGRAMS\NLog\Uninstall.lnk" "$INSTDIR\Uninstall.exe" ""
  CreateShortCut  "$SMPROGRAMS\NLog\LICENSE.lnk" "$INSTDIR\License.txt" ""
  CreateShortCut  "$SMPROGRAMS\NLog\Class Library Reference.lnk" "$INSTDIR\docs\NLog.chm" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Tutorial.lnk" "$INSTDIR\docs\tutorial.html" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Documentation.lnk" "$INSTDIR\docs\index.html" ""

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

  Delete "$INSTDIR\LICENSE.txt"
  Delete "$INSTDIR\bin\*.dll"
  Delete "$INSTDIR\bin\*.exe.config"
  Delete "$INSTDIR\bin\*.exe.manifest"
  Delete "$INSTDIR\bin\*.exe"
  Delete "$INSTDIR\docs\*.*"
  Delete "$INSTDIR\Uninstall.exe"

  RMDir "$INSTDIR\bin"
  RMDir "$INSTDIR\docs"
  RMDir "$INSTDIR"
SectionEnd
; eof
