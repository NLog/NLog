;Change this file to customize zip2exe generated installers

Name "${ZIP2EXE_NAME}"
OutFile "${ZIP2EXE_OUTFILE}"

AllowRootDirInstall true


!ifdef ZIP2EXE_COMPRESSOR_SOLID
  !define SETCOMPRESSOR_SWITCH /SOLID
!else
  !define SETCOMPRESSOR_SWITCH
!endif

!ifdef ZIP2EXE_COMPRESSOR_ZLIB
  SetCompressor ${SETCOMPRESSOR_SWITCH} zlib
!else ifdef ZIP2EXE_COMPRESSOR_BZIP2
  SetCompressor ${SETCOMPRESSOR_SWITCH} bzip2
!else ifdef ZIP2EXE_COMPRESSOR_LZMA
  SetCompressor ${SETCOMPRESSOR_SWITCH} lzma
!endif

!ifdef ZIP2EXE_INSTALLDIR

  InstallDir "${ZIP2EXE_INSTALLDIR}"

  Function zip2exe.SetOutPath
    SetOutPath "$INSTDIR"
  FunctionEnd

!else ifdef ZIP2EXE_INSTALLDIR_WINAMP

  InstallDir "$PROGRAMFILES\Winamp"
  InstallDirRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Winamp" "UninstallString"

  Function .onVerifyInstDir
    IfFileExists $INSTDIR\winamp.exe WinampInstalled
      Abort
    WinampInstalled:
  FunctionEnd

  !ifdef ZIP2EXE_INSTALLDIR_WINAMPMODE
 
    Var ZIP2EXE_TEMP1
    Var ZIP2EXE_TEMP2

    Function zip2exe.SetOutPath
       !ifdef ZIP2EXE_INSTALLDIR_SKINS
         StrCpy $ZIP2EXE_TEMP1 "$INSTDIR\Skins"
       !else
         StrCpy $ZIP2EXE_TEMP1 "$INSTDIR\Plugins"
       !endif
       ReadINIStr $ZIP2EXE_TEMP2 "$INSTDIR\winamp.ini" "Winamp" "${ZIP2EXE_INSTALLDIR_WINAMPMODE}"
         StrCmp $ZIP2EXE_TEMP2 "" End
         IfFileExists $ZIP2EXE_TEMP2 0 End
         StrCpy $ZIP2EXE_TEMP1 $ZIP2EXE_TEMP2
       End:
         SetOutPath $ZIP2EXE_TEMP1
    FunctionEnd

  !else

    Function zip2exe.SetOutPath
      !ifdef ZIP2EXE_INSTALLDIR_PLUGINS
        SetOutPath "$INSTDIR\Plugins"
      !else
        SetOutPath "$INSTDIR"
      !endif
    FunctionEnd

  !endif

!endif

!macro SECTION_BEGIN

  Section ""
  
    Call zip2exe.SetOutPath
  
!macroend

!macro SECTION_END

  SectionEnd
  
!macroend