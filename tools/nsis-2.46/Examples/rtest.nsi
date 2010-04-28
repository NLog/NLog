; rtest.nsi
;
; This script tests some advanced NSIS functions.

;--------------------------------

Name "rtest"
OutFile "rtest.exe"

ComponentText "Select tests!"
ShowInstDetails show

RequestExecutionLevel user

;--------------------------------

Section "Test 1"

  StrCpy $R0 "a"
  
  GetFunctionAddress $R1 test1
  Call $R1
  
  StrCmp $R0 "a182345678" success
  
  DetailPrint "Test 1 failed (output: $R0)"
  Goto end
  
  success:
  DetailPrint "Test 1 succeded (output: $R0)"
  
  end:
  
SectionEnd

Function test1

  GetLabelAddress $9 skip
  
  IntOp $9 $9 - 1
  StrCpy $R0 $R01
  
  Call $9
  
  StrCpy $R0 $R02
  StrCpy $R0 $R03
  StrCpy $R0 $R04
  StrCpy $R0 $R05
  StrCpy $R0 $R06
  StrCpy $R0 $R07
  StrCpy $R0 $R08
  
  skip:
  
FunctionEnd

;--------------------------------

Section "Test 2"

  StrCpy $R0 "0"
  StrCpy $R1 "11"
  
  Call test2
  
  StrCmp $R1 "11,10,9,8,7,6,5,4,3,2,1" success
  
  DetailPrint "Test 2 failed (output: $R1)"
  Goto end
  
  success:
  DetailPrint "Test 2 succeded (output: $R1)"
  
  end:

SectionEnd

Function test2

  IntOp $R0 $R0 + 1
  IntCmp $R0 10 done
  
  Push $R0
  
  GetFunctionAddress $R2 test2
  Call $R2
  
  Pop $R0
  
  done:
  StrCpy $R1 "$R1,$R0"
  
FunctionEnd