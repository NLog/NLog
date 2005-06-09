!cd ..

; The name of the installer
Name "NLog ${NLOGVERSION}"

SetCompress force
SetCompressor lzma

; The file to write
OutFile "SetupNLog.exe"

; The default installation directory
InstallDir $PROGRAMFILES\NLog

; The text to prompt the user to enter a directory
DirText "This will install NLog version ${NLOGVERSION} on your computer. Choose a directory:"

; The stuff to install
Section "Main"
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  File LICENSE.txt

  SetOutPath $INSTDIR\bin
  ; Put file there
  File /r "build\${BUILDSUBDIR}\bin\*.*"

  SetOutPath $INSTDIR\web
  File /r build\${BUILDSUBDIR}\web\*.*

  SetOutPath $INSTDIR\help
  File build\${BUILDSUBDIR}\help\NLog.chm

  CreateDirectory "$SMPROGRAMS\NLog"
  CreateShortCut  "$SMPROGRAMS\NLog\Uninstall.lnk" "$INSTDIR\Uninstall.exe" ""
  CreateShortCut  "$SMPROGRAMS\NLog\LICENSE.lnk" "$INSTDIR\License.txt" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Class Library Reference.lnk" "$INSTDIR\help\NLog.chm" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Tutorial.lnk" "$INSTDIR\web\tutorial.html" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Archived Website.lnk" "$INSTDIR\web\index.html" ""
  CreateShortCut  "$SMPROGRAMS\NLog\NLog Viewer (preview).lnk" "$INSTDIR\bin\NLog.Viewer.exe" ""

  WriteRegStr HKCU "Software\Microsoft\VisualStudio\7.0\AssemblyFolders\NLog" "" "$INSTDIR\Bin"
  WriteRegStr HKCU "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\NLog" "" "$INSTDIR\Bin"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "" ""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NLog" "DisplayName" "NLog - A .NET Logging Library"
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

  RMDir /r "$INSTDIR"
SectionEnd
; eof
