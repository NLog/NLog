Name "NSIS StrFunc Example"
OutFile "StrFunc.exe"
ShowInstDetails show
ShowUninstDetails show
XPStyle on
RequestExecutionLevel user

!include "StrFunc.nsh"

# Declare used functions
${StrCase}
${StrClb}
${StrIOToNSIS}
${StrLoc}
${StrNSISToIO}
${StrRep}
${StrStr}
${StrStrAdv}
${StrTok}
${StrTrimNewLines}
${StrSort}

${UnStrCase}
${UnStrClb}
${UnStrIOToNSIS}
${UnStrLoc}
${UnStrNSISToIO}
${UnStrRep}
${UnStrStr}
${UnStrStrAdv}
${UnStrTok}
${UnStrTrimNewLines}
${UnStrSort}

!macro StackVerificationStart
  StrCpy $0 S0
  StrCpy $1 S1
  StrCpy $2 S2
  StrCpy $3 S3
  StrCpy $4 S4
  StrCpy $5 S5
  StrCpy $6 S6
  StrCpy $7 S7
  StrCpy $8 S8
  StrCpy $9 S9
  StrCpy $R0 SR0
  StrCpy $R1 SR1
  StrCpy $R2 SR2
  StrCpy $R3 SR3
  StrCpy $R4 SR4
  StrCpy $R5 SR5
  StrCpy $R6 SR6
  StrCpy $R7 SR7
  StrCpy $R8 SR8
  StrCpy $R9 SR9
!macroend

!macro StackVerificationEnd
  ClearErrors
  ${If} $1 != "S1"
  ${OrIf} $2 != "S2"
  ${OrIf} $3 != "S3"
  ${OrIf} $4 != "S4"
  ${OrIf} $5 != "S5"
  ${OrIf} $6 != "S6"
  ${OrIf} $7 != "S7"
  ${OrIf} $8 != "S8"
  ${OrIf} $9 != "S9"
  ${OrIf} $R0 != "SR0"
  ${OrIf} $R1 != "SR1"
  ${OrIf} $R2 != "SR2"
  ${OrIf} $R3 != "SR3"
  ${OrIf} $R4 != "SR4"
  ${OrIf} $R5 != "SR5"
  ${OrIf} $R6 != "SR6"
  ${OrIf} $R7 != "SR7"
  ${OrIf} $R8 != "SR8"
  ${OrIf} $R9 != "SR9"
    SetErrors
  ${EndIf}
!macroend

Section

  # Test case conversion
  !insertmacro StackVerificationStart
  ${StrCase} $0 "This is just an example. A very simple one." ""
  StrCmp $0 "This is just an example. A very simple one." 0 strcaseerror
  ${StrCase} $0 "THIS IS JUST AN EXAMPLE. A VERY SIMPLE ONE." "S"
  StrCmp $0 "This is just an example. A very simple one." 0 strcaseerror
  ${StrCase} $0 "This is just an example. A very simple one." "L"
  StrCmp $0 "this is just an example. a very simple one." 0 strcaseerror
  ${StrCase} $0 "This is just an example. A very simple one." "U"
  StrCmp $0 "THIS IS JUST AN EXAMPLE. A VERY SIMPLE ONE." 0 strcaseerror
  ${StrCase} $0 "This is just an example. A very simple one." "T"
  StrCmp $0 "This Is Just An Example. A Very Simple One." 0 strcaseerror
  ${StrCase} $0 "This is just an example. A very simple one." "<>"
  StrCmp $0 "tHIS IS JUST AN EXAMPLE. a VERY SIMPLE ONE." 0 strcaseerror
  ${StrCase} $0 "123456789!@#%^&*()-_=+[]{};:,./<>?" "S"
  StrCmp $0 "123456789!@#%^&*()-_=+[]{};:,./<>?" 0 strcaseerror
  ${StrCase} $0 "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#%^&*()abcdefghijklmnopqrstuvwxyz-_=+[]{};:,./<>?" "<>"
  StrCmp $0 "123456789abcdefghijklmnopqrstuvwxyz!@#%^&*()ABCDEFGHIJKLMNOPQRSTUVWXYZ-_=+[]{};:,./<>?" 0 strcaseerror
  ${StrCase} $0 "what about taking a shower tomorrow? it's late to do so now! try to sleep now. Good Night!" "S"
  StrCmp $0 "What about taking a shower tomorrow? It's late to do so now! Try to sleep now. Good night!" 0 strcaseerror
  !insertmacro StackVerificationEnd
  IfErrors strcaseerror

  DetailPrint "PASSED StrCase test"
  Goto +2
strcaseerror:
  DetailPrint "FAILED StrCase test"

  # Test clipboard function
  !insertmacro StackVerificationStart
  ${StrClb} $0 "StrFunc clipboard test" ">"
  StrCmp $0 "" 0 strclberror
  ${StrClb} $0 "StrFunc clipboard test #2" "<>"
  StrCmp $0 "StrFunc clipboard test" 0 strclberror
  ${StrClb} $0 "" "<"
  StrCmp $0 "StrFunc clipboard test #2" 0 strclberror
  ${StrClb} $0 "" ""
  StrCmp $0 "" 0 strclberror
  !insertmacro StackVerificationEnd
  IfErrors strclberror

  DetailPrint "PASSED StrClb test"
  Goto +2
strclberror:
  DetailPrint "FAILED StrClb test"

  # Test IO functions
  !insertmacro StackVerificationStart
  !macro testio str
  ${StrNSISToIO} $0 "${str}"
  ${StrIOToNSIS} $0 $0
  StrCmp $0 "${str}" 0 ioerror
  !macroend
  !insertmacro testio "$\rtest$\n"
  !insertmacro testio "test$\n"
  !insertmacro testio "$\rtest"
  !insertmacro testio "test"
  !insertmacro testio "$\r\$\t$\n"
  !insertmacro testio "$\r \ $\t $\n $$"
  !insertmacro testio ""
  !insertmacro testio " "
  !insertmacro StackVerificationEnd
  IfErrors ioerror

  DetailPrint "PASSED StrNSISToIO/StrIOToNSIS test"
  Goto +2
ioerror:
  DetailPrint "FAILED StrNSISToIO/StrIOToNSIS test"

  # Test string search functions
  !insertmacro StackVerificationStart
  ${StrLoc} $0 "This is just an example" "just" "<"
  StrCmp $0 "11" 0 strlocerror
  ${StrLoc} $0 a abc <
  StrCmp $0 "" 0 strlocerror
  ${StrLoc} $0 a abc >
  StrCmp $0 "" 0 strlocerror
  ${StrLoc} $0 abc a >
  StrCmp $0 "0" 0 strlocerror
  ${StrLoc} $0 abc b >
  StrCmp $0 "1" 0 strlocerror
  ${StrLoc} $0 abc c >
  StrCmp $0 "2" 0 strlocerror
  ${StrLoc} $0 abc a <
  StrCmp $0 "2" 0 strlocerror
  ${StrLoc} $0 abc b <
  StrCmp $0 "1" 0 strlocerror
  ${StrLoc} $0 abc c <
  StrCmp $0 "0" 0 strlocerror
  ${StrLoc} $0 abc d <
  StrCmp $0 "" 0 strlocerror
  !insertmacro StackVerificationEnd
  IfErrors strlocerror
  
  DetailPrint "PASSED StrLoc test"
  Goto +2
strlocerror:
  DetailPrint "FAILED StrLoc test"

  # Test string replacement
  !insertmacro StackVerificationStart
  ${StrRep} $0 "This is just an example" "an" "one"
  StrCmp $0 "This is just one example" 0 strreperror
  ${StrRep} $0 "test... test... 1 2 3..." "test" "testing"
  StrCmp $0 "testing... testing... 1 2 3..." 0 strreperror
  ${StrRep} $0 "" "test" "testing"
  StrCmp $0 "" 0 strreperror
  ${StrRep} $0 "test" "test" "testing"
  StrCmp $0 "testing" 0 strreperror
  ${StrRep} $0 "test" "test" ""
  StrCmp $0 "" 0 strreperror
  ${StrRep} $0 "test" "" "abc"
  StrCmp $0 "test" 0 strreperror
  ${StrRep} $0 "test" "" ""
  StrCmp $0 "test" 0 strreperror
  !insertmacro StackVerificationEnd
  IfErrors strreperror
  
  DetailPrint "PASSED StrRep test"
  Goto +2
strreperror:
  DetailPrint "FAILED StrRep test"

  # Test sorting
  !insertmacro StackVerificationStart
  ${StrSort} $0 "This is just an example" "" " just" "ple" "0" "0" "0"
  StrCmp $0 "This is an exam" 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "j" " " "0" "" "0"
  StrCmp $0 "just" 0 strsorterror
  ${StrSort} $0 "This is just an example" "" "j" "" "0" "1" "0"
  StrCmp $0 "This is just an example" 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "us" "" "0" "1" "0"
  StrCmp $0 "just an example" 0 strsorterror
  ${StrSort} $0 "This is just an example" "" "u" " " "0" "1" "0"
  StrCmp $0 "This is just" 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "just" " " "0" "1" "0"
  StrCmp $0 "just" 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "t" " " "0" "1" "0"
  StrCmp $0 "This" 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "le" " " "0" "1" "0"
  StrCmp $0 "example" 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "le" " " "1" "0" "0"
  StrCmp $0 " examp" 0 strsorterror
  ${StrSort} $0 "an error has occurred" " " "e" " " "0" "1" "0"
  StrCmp $0 "error" 0 strsorterror
  ${StrSort} $0 "" " " "something" " " "0" "1" "0"
  StrCmp $0 "" 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "j" " " "" "" ""
  StrCmp $0 " just " 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "j" " " "1" "0" "1"
  StrCmp $0 " ust " 0 strsorterror
  ${StrSort} $0 "This is just an example" "" "j" "" "0" "0" "1"
  StrCmp $0 "This is ust an example" 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "us" "" "1" "0" "0"
  StrCmp $0 " jt an example" 0 strsorterror
  ${StrSort} $0 "This is just an example" "" "u" " " "0" "0" "1"
  StrCmp $0 "This is jst " 0 strsorterror
  ${StrSort} $0 "This is just an example" " " "just" " " "1" "0" "1"
  StrCmp $0 "  " 0 strsorterror
  ${StrSort} $0 "an error has occurred" " " "e" "h" "1" "0" "0"
  StrCmp $0 " rror " 0 strsorterror
  ${StrSort} $0 "" " " "something" " " "1" "0" "1"
  StrCmp $0 "" 0 strsorterror
  !insertmacro StackVerificationEnd
  IfErrors strsorterror
  
  DetailPrint "PASSED StrSort test"
  Goto +2
strsorterror:
  DetailPrint "FAILED StrSort test"

  !insertmacro StackVerificationStart
  ${StrStr} $0 "abcefghijklmnopqrstuvwxyz" "g"
  StrCmp $0 "ghijklmnopqrstuvwxyz" 0 strstrerror
  ${StrStr} $0 "abcefghijklmnopqrstuvwxyz" "ga"
  StrCmp $0 "" 0 strstrerror
  ${StrStr} $0 "abcefghijklmnopqrstuvwxyz" ""
  StrCmp $0 "abcefghijklmnopqrstuvwxyz" 0 strstrerror
  ${StrStr} $0 "a" "abcefghijklmnopqrstuvwxyz"
  StrCmp $0 "" 0 strstrerror
  !insertmacro StackVerificationEnd
  IfErrors strstrerror
  
  DetailPrint "PASSED StrStr test"
  Goto +2
strstrerror:
  DetailPrint "FAILED StrStr test"

  !insertmacro StackVerificationStart
  ${StrStrAdv} $0 "abcabcabc" "a" ">" ">" "1" "0" "0"
  StrCmp $0 "abcabcabc" 0 strstradverror
  ${StrStrAdv} $0 "abcabcabc" "a" ">" ">" "1" "1" "0"
  StrCmp $0 "abcabc" 0 strstradverror
  ${StrStrAdv} $0 "abcabcabc" "a" ">" ">" "1" "2" "0"
  StrCmp $0 "abc" 0 strstradverror
  ${StrStrAdv} $0 "abcabcabc" "a" ">" ">" "1" "3" "0"
  StrCmp $0 "" 0 strstradverror
  ${StrStrAdv} $0 "abcabcabc" "abc" ">" "<" "1" "1" "0"
  StrCmp $0 "abcabc" 0 strstradverror
  ${StrStrAdv} $0 "abcabcabc" "abc" ">" "<" "0" "1" "0"
  StrCmp $0 "abc" 0 strstradverror
  ${StrStrAdv} $0 "abcabcabc" "abc" "<" "<" "1" "0" "0"
  StrCmp $0 "abcabcabc" 0 strstradverror
  ${StrStrAdv} $0 "abcabcabc" "abc" "<" "<" "0" "0" "0"
  StrCmp $0 "abcabc" 0 strstradverror
  ${StrStrAdv} $0 "abcabcabc" "abc" "<" ">" "0" "0" "0"
  StrCmp $0 "" 0 strstradverror
  ${StrStrAdv} $0 "abcabcabc" "abc" "<" ">" "0" "1" "0"
  StrCmp $0 "abc" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "a" ">" ">" "1" "0" "1"
  StrCmp $0 "abcabc" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "a" ">" ">" "1" "1" "1"
  StrCmp $0 "abc" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "a" ">" ">" "1" "2" "1"
  StrCmp $0 "" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "a" ">" ">" "1" "3" "1"
  StrCmp $0 "" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "abc" ">" "<" "1" "1" "1"
  StrCmp $0 "ABCabcabc" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "abc" ">" "<" "0" "1" "1"
  StrCmp $0 "ABCabc" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "abc" "<" "<" "1" "0" "1"
  StrCmp $0 "ABCabcabc" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "abc" "<" "<" "0" "0" "1"
  StrCmp $0 "ABCabc" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "abc" "<" ">" "0" "0" "1"
  StrCmp $0 "" 0 strstradverror
  ${StrStrAdv} $0 "ABCabcabc" "abc" "<" ">" "0" "1" "1"
  StrCmp $0 "abc" 0 strstradverror
  !insertmacro StackVerificationEnd
  IfErrors strstradverror
  
  DetailPrint "PASSED StrStrAdv test"
  Goto +2
strstradverror:
  DetailPrint "FAILED StrStrAdv test"

  # Test tokenizer
  !insertmacro StackVerificationStart
  ${StrTok} $0 "This is, or is not, just an example" " ," "4" "1"
  StrCmp $0 "not" 0 strtokerror
  ${StrTok} $0 "This is, or is not, just an example" " ," "4" "0"
  StrCmp $0 "is" 0 strtokerror
  ${StrTok} $0 "This is, or is not, just an example" " ," "152" "0"
  StrCmp $0 "" 0 strtokerror
  ${StrTok} $0 "This is, or is not, just an example" " ," "" "0"
  StrCmp $0 "example" 0 strtokerror
  ${StrTok} $0 "This is, or is not, just an example" " ," "L" "0"
  StrCmp $0 "example" 0 strtokerror
  ${StrTok} $0 "This is, or is not, just an example" " ," "0" "0"
  StrCmp $0 "This" 0 strtokerror
  !insertmacro StackVerificationEnd
  IfErrors strtokerror
  
  DetailPrint "PASSED StrTok test"
  Goto +2
strtokerror:
  DetailPrint "FAILED StrTok test"

  # Test trim new lines
  !insertmacro StackVerificationStart
  ${StrTrimNewLines} $0 "$\r$\ntest$\r$\ntest$\r$\n"
  StrCmp $0 "$\r$\ntest$\r$\ntest" 0 strtrimnewlineserror
  !insertmacro StackVerificationEnd
  IfErrors strtrimnewlineserror

  DetailPrint "PASSED StrTrimNewLines test"
  Goto +2
strtrimnewlineserror:
  DetailPrint "FAILED StrTrimNewLines test"

  WriteUninstaller $EXEDIR\UnStrFunc.exe
  
  Exec $EXEDIR\UnStrFunc.exe

SectionEnd

Section Uninstall

  # Test case conversion
  !insertmacro StackVerificationStart
  ${UnStrCase} $0 "This is just an example. A very simple one." ""
  StrCmp $0 "This is just an example. A very simple one." 0 strcaseerror
  ${UnStrCase} $0 "THIS IS JUST AN EXAMPLE. A VERY SIMPLE ONE." "S"
  StrCmp $0 "This is just an example. A very simple one." 0 strcaseerror
  ${UnStrCase} $0 "This is just an example. A very simple one." "L"
  StrCmp $0 "this is just an example. a very simple one." 0 strcaseerror
  ${UnStrCase} $0 "This is just an example. A very simple one." "U"
  StrCmp $0 "THIS IS JUST AN EXAMPLE. A VERY SIMPLE ONE." 0 strcaseerror
  ${UnStrCase} $0 "This is just an example. A very simple one." "T"
  StrCmp $0 "This Is Just An Example. A Very Simple One." 0 strcaseerror
  ${UnStrCase} $0 "This is just an example. A very simple one." "<>"
  StrCmp $0 "tHIS IS JUST AN EXAMPLE. a VERY SIMPLE ONE." 0 strcaseerror
  ${UnStrCase} $0 "123456789!@#%^&*()-_=+[]{};:,./<>?" "S"
  StrCmp $0 "123456789!@#%^&*()-_=+[]{};:,./<>?" 0 strcaseerror
  ${UnStrCase} $0 "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#%^&*()abcdefghijklmnopqrstuvwxyz-_=+[]{};:,./<>?" "<>"
  StrCmp $0 "123456789abcdefghijklmnopqrstuvwxyz!@#%^&*()ABCDEFGHIJKLMNOPQRSTUVWXYZ-_=+[]{};:,./<>?" 0 strcaseerror
  ${UnStrCase} $0 "what about taking a shower tomorrow? it's late to do so now! try to sleep now. Good Night!" "S"
  StrCmp $0 "What about taking a shower tomorrow? It's late to do so now! Try to sleep now. Good night!" 0 strcaseerror
  !insertmacro StackVerificationEnd
  IfErrors strcaseerror

  DetailPrint "PASSED StrCase test"
  Goto +2
strcaseerror:
  DetailPrint "FAILED StrCase test"

  # Test clipboard function
  !insertmacro StackVerificationStart
  ${UnStrClb} $0 "StrFunc clipboard test" ">"
  StrCmp $0 "" 0 strclberror
  ${UnStrClb} $0 "StrFunc clipboard test #2" "<>"
  StrCmp $0 "StrFunc clipboard test" 0 strclberror
  ${UnStrClb} $0 "" "<"
  StrCmp $0 "StrFunc clipboard test #2" 0 strclberror
  ${UnStrClb} $0 "" ""
  StrCmp $0 "" 0 strclberror
  !insertmacro StackVerificationEnd
  IfErrors strclberror

  DetailPrint "PASSED StrClb test"
  Goto +2
strclberror:
  DetailPrint "FAILED StrClb test"

  # Test IO functions
  !insertmacro StackVerificationStart
  !macro untestio str
  ${UnStrNSISToIO} $0 "${str}"
  ${UnStrIOToNSIS} $0 $0
  StrCmp $0 "${str}" 0 ioerror
  !macroend
  !insertmacro untestio "$\rtest$\n"
  !insertmacro untestio "test$\n"
  !insertmacro untestio "$\rtest"
  !insertmacro untestio "test"
  !insertmacro untestio "$\r\$\t$\n"
  !insertmacro untestio "$\r \ $\t $\n $$"
  !insertmacro untestio ""
  !insertmacro untestio " "
  !insertmacro StackVerificationEnd
  IfErrors ioerror

  DetailPrint "PASSED StrNSISToIO/StrIOToNSIS test"
  Goto +2
ioerror:
  DetailPrint "FAILED StrNSISToIO/StrIOToNSIS test"

  # Test string search functions
  !insertmacro StackVerificationStart
  ${UnStrLoc} $0 "This is just an example" "just" "<"
  StrCmp $0 "11" 0 strlocerror
  ${UnStrLoc} $0 a abc <
  StrCmp $0 "" 0 strlocerror
  ${UnStrLoc} $0 a abc >
  StrCmp $0 "" 0 strlocerror
  ${UnStrLoc} $0 abc a >
  StrCmp $0 "0" 0 strlocerror
  ${UnStrLoc} $0 abc b >
  StrCmp $0 "1" 0 strlocerror
  ${UnStrLoc} $0 abc c >
  StrCmp $0 "2" 0 strlocerror
  ${UnStrLoc} $0 abc a <
  StrCmp $0 "2" 0 strlocerror
  ${UnStrLoc} $0 abc b <
  StrCmp $0 "1" 0 strlocerror
  ${UnStrLoc} $0 abc c <
  StrCmp $0 "0" 0 strlocerror
  ${UnStrLoc} $0 abc d <
  StrCmp $0 "" 0 strlocerror
  !insertmacro StackVerificationEnd
  IfErrors strlocerror

  DetailPrint "PASSED StrLoc test"
  Goto +2
strlocerror:
  DetailPrint "FAILED StrLoc test"

  # Test string replacement
  !insertmacro StackVerificationStart
  ${UnStrRep} $0 "This is just an example" "an" "one"
  StrCmp $0 "This is just one example" 0 strreperror
  ${UnStrRep} $0 "test... test... 1 2 3..." "test" "testing"
  StrCmp $0 "testing... testing... 1 2 3..." 0 strreperror
  ${UnStrRep} $0 "" "test" "testing"
  StrCmp $0 "" 0 strreperror
  ${UnStrRep} $0 "test" "test" "testing"
  StrCmp $0 "testing" 0 strreperror
  ${UnStrRep} $0 "test" "test" ""
  StrCmp $0 "" 0 strreperror
  ${UnStrRep} $0 "test" "" "abc"
  StrCmp $0 "test" 0 strreperror
  ${UnStrRep} $0 "test" "" ""
  StrCmp $0 "test" 0 strreperror
  !insertmacro StackVerificationEnd
  IfErrors strreperror

  DetailPrint "PASSED StrRep test"
  Goto +2
strreperror:
  DetailPrint "FAILED StrRep test"

  # Test sorting
  !insertmacro StackVerificationStart
  ${UnStrSort} $0 "This is just an example" "" " just" "ple" "0" "0" "0"
  StrCmp $0 "This is an exam" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "j" " " "0" "" "0"
  StrCmp $0 "just" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" "" "j" "" "0" "1" "0"
  StrCmp $0 "This is just an example" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "us" "" "0" "1" "0"
  StrCmp $0 "just an example" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" "" "u" " " "0" "1" "0"
  StrCmp $0 "This is just" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "just" " " "0" "1" "0"
  StrCmp $0 "just" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "t" " " "0" "1" "0"
  StrCmp $0 "This" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "le" " " "0" "1" "0"
  StrCmp $0 "example" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "le" " " "1" "0" "0"
  StrCmp $0 " examp" 0 strsorterror
  ${UnStrSort} $0 "an error has occurred" " " "e" " " "0" "1" "0"
  StrCmp $0 "error" 0 strsorterror
  ${UnStrSort} $0 "" " " "something" " " "0" "1" "0"
  StrCmp $0 "" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "j" " " "" "" ""
  StrCmp $0 " just " 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "j" " " "1" "0" "1"
  StrCmp $0 " ust " 0 strsorterror
  ${UnStrSort} $0 "This is just an example" "" "j" "" "0" "0" "1"
  StrCmp $0 "This is ust an example" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "us" "" "1" "0" "0"
  StrCmp $0 " jt an example" 0 strsorterror
  ${UnStrSort} $0 "This is just an example" "" "u" " " "0" "0" "1"
  StrCmp $0 "This is jst " 0 strsorterror
  ${UnStrSort} $0 "This is just an example" " " "just" " " "1" "0" "1"
  StrCmp $0 "  " 0 strsorterror
  ${UnStrSort} $0 "an error has occurred" " " "e" "h" "1" "0" "0"
  StrCmp $0 " rror " 0 strsorterror
  ${UnStrSort} $0 "" " " "something" " " "1" "0" "1"
  StrCmp $0 "" 0 strsorterror
  !insertmacro StackVerificationEnd
  IfErrors strsorterror

  DetailPrint "PASSED StrSort test"
  Goto +2
strsorterror:
  DetailPrint "FAILED StrSort test"

  !insertmacro StackVerificationStart
  ${UnStrStr} $0 "abcefghijklmnopqrstuvwxyz" "g"
  StrCmp $0 "ghijklmnopqrstuvwxyz" 0 strstrerror
  ${UnStrStr} $0 "abcefghijklmnopqrstuvwxyz" "ga"
  StrCmp $0 "" 0 strstrerror
  ${UnStrStr} $0 "abcefghijklmnopqrstuvwxyz" ""
  StrCmp $0 "abcefghijklmnopqrstuvwxyz" 0 strstrerror
  ${UnStrStr} $0 "a" "abcefghijklmnopqrstuvwxyz"
  StrCmp $0 "" 0 strstrerror
  !insertmacro StackVerificationEnd
  IfErrors strstrerror

  DetailPrint "PASSED StrStr test"
  Goto +2
strstrerror:
  DetailPrint "FAILED StrStr test"

  !insertmacro StackVerificationStart
  ${UnStrStrAdv} $0 "abcabcabc" "a" ">" ">" "1" "0" "0"
  StrCmp $0 "abcabcabc" 0 strstradverror
  ${UnStrStrAdv} $0 "abcabcabc" "a" ">" ">" "1" "1" "0"
  StrCmp $0 "abcabc" 0 strstradverror
  ${UnStrStrAdv} $0 "abcabcabc" "a" ">" ">" "1" "2" "0"
  StrCmp $0 "abc" 0 strstradverror
  ${UnStrStrAdv} $0 "abcabcabc" "a" ">" ">" "1" "3" "0"
  StrCmp $0 "" 0 strstradverror
  ${UnStrStrAdv} $0 "abcabcabc" "abc" ">" "<" "1" "1" "0"
  StrCmp $0 "abcabc" 0 strstradverror
  ${UnStrStrAdv} $0 "abcabcabc" "abc" ">" "<" "0" "1" "0"
  StrCmp $0 "abc" 0 strstradverror
  ${UnStrStrAdv} $0 "abcabcabc" "abc" "<" "<" "1" "0" "0"
  StrCmp $0 "abcabcabc" 0 strstradverror
  ${UnStrStrAdv} $0 "abcabcabc" "abc" "<" "<" "0" "0" "0"
  StrCmp $0 "abcabc" 0 strstradverror
  ${UnStrStrAdv} $0 "abcabcabc" "abc" "<" ">" "0" "0" "0"
  StrCmp $0 "" 0 strstradverror
  ${UnStrStrAdv} $0 "abcabcabc" "abc" "<" ">" "0" "1" "0"
  StrCmp $0 "abc" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "a" ">" ">" "1" "0" "1"
  StrCmp $0 "abcabc" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "a" ">" ">" "1" "1" "1"
  StrCmp $0 "abc" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "a" ">" ">" "1" "2" "1"
  StrCmp $0 "" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "a" ">" ">" "1" "3" "1"
  StrCmp $0 "" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "abc" ">" "<" "1" "1" "1"
  StrCmp $0 "ABCabcabc" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "abc" ">" "<" "0" "1" "1"
  StrCmp $0 "ABCabc" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "abc" "<" "<" "1" "0" "1"
  StrCmp $0 "ABCabcabc" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "abc" "<" "<" "0" "0" "1"
  StrCmp $0 "ABCabc" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "abc" "<" ">" "0" "0" "1"
  StrCmp $0 "" 0 strstradverror
  ${UnStrStrAdv} $0 "ABCabcabc" "abc" "<" ">" "0" "1" "1"
  StrCmp $0 "abc" 0 strstradverror
  !insertmacro StackVerificationEnd
  IfErrors strstradverror

  DetailPrint "PASSED StrStrAdv test"
  Goto +2
strstradverror:
  DetailPrint "FAILED StrStrAdv test"

  # Test tokenizer
  !insertmacro StackVerificationStart
  ${UnStrTok} $0 "This is, or is not, just an example" " ," "4" "1"
  StrCmp $0 "not" 0 strtokerror
  ${UnStrTok} $0 "This is, or is not, just an example" " ," "4" "0"
  StrCmp $0 "is" 0 strtokerror
  ${UnStrTok} $0 "This is, or is not, just an example" " ," "152" "0"
  StrCmp $0 "" 0 strtokerror
  ${UnStrTok} $0 "This is, or is not, just an example" " ," "" "0"
  StrCmp $0 "example" 0 strtokerror
  ${UnStrTok} $0 "This is, or is not, just an example" " ," "L" "0"
  StrCmp $0 "example" 0 strtokerror
  ${UnStrTok} $0 "This is, or is not, just an example" " ," "0" "0"
  StrCmp $0 "This" 0 strtokerror
  !insertmacro StackVerificationEnd
  IfErrors strtokerror

  DetailPrint "PASSED StrTok test"
  Goto +2
strtokerror:
  DetailPrint "FAILED StrTok test"

  # Test trim new lines
  !insertmacro StackVerificationStart
  ${UnStrTrimNewLines} $0 "$\r$\ntest$\r$\ntest$\r$\n"
  StrCmp $0 "$\r$\ntest$\r$\ntest" 0 strtrimnewlineserror
  !insertmacro StackVerificationEnd
  IfErrors strtrimnewlineserror

  DetailPrint "PASSED StrTrimNewLines test"
  Goto +2
strtrimnewlineserror:
  DetailPrint "FAILED StrTrimNewLines test"

SectionEnd
