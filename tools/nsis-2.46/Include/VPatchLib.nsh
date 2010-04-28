; PatchLib v3.0
; =============
;
; Library with macro for use with VPatch (DLL version) in NSIS 2.0.5+
; Created by Koen van de Sande

!include LogicLib.nsh

!macro VPatchFile PATCHDATA SOURCEFILE TEMPFILE

  Push $1
  Push $2
  Push $3
  Push $4

  Push ${SOURCEFILE}
  Push ${TEMPFILE}

  Pop $2 # temp file
  Pop $3 # source file

  InitPluginsDir
  GetTempFileName $1 $PLUGINSDIR
  File /oname=$1 ${PATCHDATA}

  vpatch::vpatchfile $1 $3 $2
  Pop $4
  DetailPrint $4

  StrCpy $4 $4 2
  ${Unless} $4 == "OK"
    SetErrors
  ${EndIf}

  ${If} ${FileExists} $2
    Delete $3
    Rename /REBOOTOK $2 $3
  ${EndIf}

  Delete $1

  Pop $4
  Pop $3
  Pop $2
  Pop $1

!macroend
