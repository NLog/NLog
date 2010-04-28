!verbose 2

Name "NSIS LogicLib Example"
OutFile "LogicLib.exe"
ShowInstDetails show
RequestExecutionLevel user

!include "LogicLib.nsh"

;!undef LOGICLIB_VERBOSITY
;!define LOGICLIB_VERBOSITY 4   ; For debugging - watch what logiclib does with your code!

Page components "" "" ComponentsLeave
Page instfiles

Section /o "Run tests" TESTS

  ; kinds of if other than "value1 comparison value2"
  ClearErrors
  FindFirst $R1 $R2 "$PROGRAMFILES\*"
  ${Unless} ${Errors}
    ${Do}
      ${Select} $R2
        ${Case2} "." ".."
          ; Do nothing
        ${CaseElse}
          DetailPrint "Found $PROGRAMFILES\$R2"
      ${EndSelect}
      FindNext $R1 $R2
    ${LoopUntil} ${Errors}
    FindClose $R1
  ${EndUnless}

  ${If} ${FileExists} "${__FILE__}"
    DetailPrint 'Source file "${__FILE__}" still exists'
  ${Else}
    DetailPrint 'Source file "${__FILE__}" has gone'
  ${EndIf}

  ; if..endif
  StrCpy $R1 1
  StrCpy $R2 ""
  ${If} $R1 = 1
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} $R1 = 2
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} $R1 < 2
    StrCpy $R2 $R2C
  ${EndIf}
  ${If} $R1 < -2
    StrCpy $R2 $R2D
  ${EndIf}
  ${If} $R1 > 2
    StrCpy $R2 $R2E
  ${EndIf}
  ${If} $R1 > -2
    StrCpy $R2 $R2F
  ${EndIf}
  ${If} $R1 <> 1
    StrCpy $R2 $R2G
  ${EndIf}
  ${If} $R1 <> 2
    StrCpy $R2 $R2H
  ${EndIf}
  ${If} $R1 >= 2
    StrCpy $R2 $R2I
  ${EndIf}
  ${If} $R1 >= -2
    StrCpy $R2 $R2J
  ${EndIf}
  ${If} $R1 <= 2
    StrCpy $R2 $R2K
  ${EndIf}
  ${If} $R1 <= -2
    StrCpy $R2 $R2L
  ${EndIf}
  ${If} $R2 == "ACFHJK"
    DetailPrint "PASSED If..EndIf test"
  ${Else}
    DetailPrint "FAILED If..EndIf test"
  ${EndIf}

  ; if..elseif..else..endif
  StrCpy $R1 A
  StrCpy $R2 ""
  ${If} $R1 == A
    StrCpy $R2 $R2A
  ${ElseIf} $R1 == B
    StrCpy $R2 $R2B
  ${ElseUnless} $R1 != C
    StrCpy $R2 $R2C
  ${Else}
    StrCpy $R2 $R2D
  ${EndIf}
  ${If} $R1 == D
    StrCpy $R2 $R2D
  ${ElseIf} $R1 == A
    StrCpy $R2 $R2A
  ${ElseUnless} $R1 != B
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2C
  ${EndIf}
  ${If} $R1 == C
    StrCpy $R2 $R2C
  ${ElseIf} $R1 == D
    StrCpy $R2 $R2D
  ${ElseUnless} $R1 != A
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} $R1 == B
    StrCpy $R2 $R2B
  ${ElseIf} $R1 == C
    StrCpy $R2 $R2C
  ${ElseUnless} $R1 != D
    StrCpy $R2 $R2D
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} $R2 == "$R1$R1$R1$R1"
    DetailPrint "PASSED If..ElseIf..Else..EndIf test"
  ${Else}
    DetailPrint "FAILED If..ElseIf..Else..EndIf test"
  ${EndIf}

  ; if..andif..orif..endif
  StrCpy $R2 ""
  ${If} 1 = 1
  ${AndIf} 2 = 2
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 1
  ${AndIf} 2 = 3
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} 1 = 2
  ${AndIf} 2 = 2
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} 1 = 2
  ${AndIf} 2 = 3
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}

  ${If} 1 = 1
  ${OrIf} 2 = 2
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 1
  ${OrIf} 2 = 3
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 2
  ${OrIf} 2 = 2
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 2
  ${OrIf} 2 = 3
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}

  ${If} 1 = 1
  ${AndIf} 2 = 2
  ${OrIf} 3 = 3
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 1
  ${AndIf} 2 = 3
  ${OrIf} 3 = 3
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 2
  ${AndIf} 2 = 2
  ${OrIf} 3 = 3
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 2
  ${AndIf} 2 = 3
  ${OrIf} 3 = 3
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 1
  ${AndIf} 2 = 2
  ${OrIf} 3 = 4
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 1
  ${AndIf} 2 = 3
  ${OrIf} 3 = 4
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} 1 = 2
  ${AndIf} 2 = 2
  ${OrIf} 3 = 4
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} 1 = 2
  ${AndIf} 2 = 3
  ${OrIf} 3 = 4
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}

  ${If} 1 = 1
  ${OrIf} 2 = 2
  ${AndIf} 3 = 3
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 1
  ${OrIf} 2 = 3
  ${AndIf} 3 = 3
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 2
  ${OrIf} 2 = 2
  ${AndIf} 3 = 3
    StrCpy $R2 $R2A
  ${Else}
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 1 = 2
  ${OrIf} 2 = 3
  ${AndIf} 3 = 3
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} 1 = 1
  ${OrIf} 2 = 2
  ${AndIf} 3 = 4
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} 1 = 1
  ${OrIf} 2 = 3
  ${AndIf} 3 = 4
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} 1 = 2
  ${OrIf} 2 = 2
  ${AndIf} 3 = 4
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} 1 = 2
  ${OrIf} 2 = 3
  ${AndIf} 3 = 4
    StrCpy $R2 $R2B
  ${Else}
    StrCpy $R2 $R2A
  ${EndIf}

  ${If} $R2 == "AAAAAAAAAAAAAAAAAAAAAAAA"
    DetailPrint "PASSED If..AndIf..OrIf..Else..EndIf test"
  ${Else}
    DetailPrint "FAILED If..AndIf..OrIf..Else..EndIf test"
  ${EndIf}

  ; ifthen..|..|
  StrCpy $R1 1
  StrCpy $R2 ""
  ${IfThen} $R1 = 1 ${|} StrCpy $R2 $R2A ${|}
  ${IfThen} $R1 = 2 ${|} StrCpy $R2 $R2B ${|}
  ${IfNotThen} $R1 = 1 ${|} StrCpy $R2 $R2C ${|}
  ${IfNotThen} $R1 = 2 ${|} StrCpy $R2 $R2D ${|}
  ${If} $R2 == "AD"
    DetailPrint "PASSED IfThen test"
  ${Else}
    DetailPrint "FAILED IfThen test"
  ${EndIf}

  ; ifcmd..||..| and if/unless cmd
  StrCpy $R2 ""
  ${IfCmd} MessageBox MB_YESNO "Please click Yes" IDYES ${||} StrCpy $R2 $R2A ${|}
  ${Unless} ${Cmd} `MessageBox MB_YESNO|MB_DEFBUTTON2 "Please click No" IDYES`
    StrCpy $R2 $R2B
  ${EndUnless}
  ${If} $R2 == "AB"
    DetailPrint "PASSED IfCmd/If Cmd test"
  ${Else}
    DetailPrint "FAILED IfCmd/If Cmd test"
  ${EndIf}

  ; select..case..case2..case3..case4..case5..caseelse..endselect
  StrCpy $R1 1
  StrCpy $R2 ""
  ${Select} $R1
    ${Case} "1"
      StrCpy $R2 $R2A
    ${Case} "2"
      StrCpy $R2 $R2B
    ${Case2} "3" "4"
      StrCpy $R2 $R2C
    ${CaseElse}
      StrCpy $R2 $R2D
  ${EndSelect}
  ${Select} $R1
    ${Case} "2"
      StrCpy $R2 $R2A
    ${Case} "3"
      StrCpy $R2 $R2B
    ${Case2} "4" "5"
      StrCpy $R2 $R2C
    ${CaseElse}
      StrCpy $R2 $R2D
  ${EndSelect}
  ${Select} $R1
    ${Case} "3"
      StrCpy $R2 $R2A
    ${Case} "4"
      StrCpy $R2 $R2B
    ${Case2} "5" "1"
      StrCpy $R2 $R2C
    ${CaseElse}
      StrCpy $R2 $R2D
  ${EndSelect}
  ${Select} $R1
    ${Case} "4"
      StrCpy $R2 $R2A
    ${Case} "5"
      StrCpy $R2 $R2B
    ${Case2} "1" "2"
      StrCpy $R2 $R2C
    ${CaseElse}
      StrCpy $R2 $R2D
  ${EndSelect}
  ${If} $R2 == "ADCC"
    DetailPrint "PASSED Select..Case*..EndSelect test"
  ${Else}
    DetailPrint "FAILED Select..Case*..EndSelect test"
  ${EndIf}

  ; switch..case..caseelse..endswitch
  StrCpy $R2 ""
  ${For} $R1 1 10
    ${Switch} $R1
      ${Case} 3
        StrCpy $R2 $R2A
      ${Case} 4
        StrCpy $R2 $R2B
        ${Break}
      ${Case} 5
        StrCpy $R2 $R2C
    ${EndSwitch}
    ${Switch} $R1
      ${Case} 1
        StrCpy $R2 $R2D
      ${Default}
        StrCpy $R2 $R2E
        ${Break}
      ${Case} 2
        StrCpy $R2 $R2F
    ${EndSwitch}
    ${Switch} $R1
      ${Case} 6
      ${Case} 7
        StrCpy $R2 $R2G
        ${If} $R1 = 6
      ${Case} 8
          StrCpy $R2 $R2H
          ${Switch} $R1
            ${Case} 6
              StrCpy $R2 $R2I
              ${Break}
            ${Case} 8
              StrCpy $R2 $R2J
          ${EndSwitch}
        ${EndIf}
        StrCpy $R2 $R2K
        ${Break}
      ${Default}
        StrCpy $R2 $R2L
      ${Case} 9
        StrCpy $R2 $R2M
    ${EndSwitch}
  ${Next}
  ${If} $R2 == "DELMFLMABELMBELMCELMEGHIKEGKEHJKEMELM"
    DetailPrint "PASSED Switch..Case*..EndSwitch test"
  ${Else}
    DetailPrint "FAILED Switch..Case*..EndSwitch test"
  ${EndIf}

  ; for[each]..exitfor..next
  StrCpy $R2 ""
  ${For} $R1 1 5
    StrCpy $R2 $R2$R1
  ${Next}
  ${ForEach} $R1 10 1 - 1
    StrCpy $R2 $R2$R1
  ${Next}
  ${For} $R1 1 0
    StrCpy $R2 $R2$R1
  ${Next}
  ${If} $R2 == "1234510987654321"
    DetailPrint "PASSED For[Each]..Next test"
  ${Else}
    DetailPrint "FAILED For[Each]..Next test"
  ${EndIf}

  ; do..loop
  StrCpy $R1 0
  Call DoLoop
  ${If} $R1 == 5
    DetailPrint "PASSED Do..Loop test"
  ${Else}
    DetailPrint "FAILED Do..Loop test"
  ${EndIf}

  ; do..exitdo..loop
  StrCpy $R1 0
  StrCpy $R2 ""
  ${Do}
    StrCpy $R2 $R2$R1
    IntOp $R1 $R1 + 1
    ${If} $R1 > 10
      ${ExitDo}
    ${EndIf}
  ${Loop}
  ${If} $R2 == "012345678910"
    DetailPrint "PASSED Do..ExitDo..Loop test"
  ${Else}
    DetailPrint "FAILED Do..ExitDo..Loop test"
  ${EndIf}

  ; do..exitdo..loopuntil
  StrCpy $R1 0
  StrCpy $R2 ""
  ${Do}
    StrCpy $R2 $R2$R1
    IntOp $R1 $R1 + 1
  ${LoopUntil} $R1 >= 5
  ${If} $R2 == "01234"
    DetailPrint "PASSED Do..ExitDo..LoopUntil test"
  ${Else}
    DetailPrint "FAILED Do..ExitDo..LoopUntil test"
  ${EndIf}

  ; dountil..exitdo..loop
  StrCpy $R1 0
  StrCpy $R2 ""
  ${DoUntil} $R1 >= 5
    StrCpy $R2 $R2$R1
    IntOp $R1 $R1 + 1
  ${Loop}
  ${If} $R2 == "01234"
    DetailPrint "PASSED DoUntil..ExitDo..Loop test"
  ${Else}
    DetailPrint "FAILED DoUntil..ExitDo..Loop test"
  ${EndIf}

  ; nested do test
  StrCpy $R1 0
  StrCpy $R2 0
  StrCpy $R3 ""
  ${Do}
    StrCpy $R3 $R3$R1$R2
    IntOp $R1 $R1 + 1
    ${If} $R1 > 5
      ${ExitDo}
    ${EndIf}
    StrCpy $R2 0
    ${Do}
      StrCpy $R3 $R3$R1$R2
      IntOp $R2 $R2 + 1
      ${If} $R2 >= 5
        ${ExitDo}
      ${EndIf}
    ${Loop}
  ${Loop}
  ${If} $R3 == "00101112131415202122232425303132333435404142434445505152535455"
    DetailPrint "PASSED nested Do test"
  ${Else}
    DetailPrint "FAILED nested Do test"
  ${EndIf}

  ; while..exitwhile..endwhile (exact replica of dowhile..enddo}
  StrCpy $R1 0
  StrCpy $R2 ""
  ${While} $R1 < 5
    StrCpy $R2 $R2$R1
    IntOp $R1 $R1 + 1
  ${EndWhile}
  ${If} $R2 == "01234"
    DetailPrint "PASSED While..ExitWhile..EndWhile test"
  ${Else}
    DetailPrint "FAILED While..ExitWhile..EndWhile test"
  ${EndIf}

  ; Unsigned integer tests
  StrCpy $R2 ""
  ${If} -1 < 1
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} -1 U< 1
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 0xFFFFFFFF > 1
    StrCpy $R2 $R2C
  ${EndIf}
  ${If} 0xFFFFFFFF U> 1
    StrCpy $R2 $R2D
  ${EndIf}
  ${If} $R2 == "AD"
    DetailPrint "PASSED unsigned integer test"
  ${Else}
    DetailPrint "FAILED unsigned integer test"
  ${EndIf}

  ; 64-bit integer tests (uses System.dll)
  StrCpy $R2 ""
  ${If} 0x100000000 L= 4294967296
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} 0x100000000 L< 0x200000000
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} 0x500000000 L>= 0x500000000
    StrCpy $R2 $R2C
  ${EndIf}
  ${If} $R2 == "ABC"
    DetailPrint "PASSED 64-bit integer test"
  ${Else}
    DetailPrint "FAILED 64-bit integer test"
  ${EndIf}

  ; Extra string tests (uses System.dll)
  StrCpy $R2 ""
  ${If} "A" S< "B"
    StrCpy $R2 $R2A
  ${EndIf}
  ${If} "b" S> "A"
    StrCpy $R2 $R2B
  ${EndIf}
  ${If} "a" S<= "B"
    StrCpy $R2 $R2C
  ${EndIf}
  ${If} "B" S< "B"
    StrCpy $R2 $R2D
  ${EndIf}
  ${If} "A" S== "A"
    StrCpy $R2 $R2E
  ${EndIf}
  ${If} "A" S== "a"
    StrCpy $R2 $R2F
  ${EndIf}
  ${If} "A" S!= "a"
    StrCpy $R2 $R2G
  ${EndIf}
  ${If} $R2 == "ABCEG"
    DetailPrint "PASSED extra string test"
  ${Else}
    DetailPrint "FAILED extra string test"
  ${EndIf}

SectionEnd

Function ComponentsLeave
  ; Section flags tests (requires sections.nsh be included)
  ${Unless} ${SectionIsSelected} ${TESTS}
    MessageBox MB_OK "Please select the component"
    Abort
  ${EndIf}
FunctionEnd

Function DoLoop

  ${Do}
    IntOp $R1 $R1 + 1
    ${If} $R1 == 5
      Return
    ${EndIf}
  ${Loop}

FunctionEnd

!verbose 3
