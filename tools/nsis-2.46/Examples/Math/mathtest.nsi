;NSIS Modern User Interface version 1.65
;InstallOptions Example Script
;Written by Joost Verburg

  !define MUI_BUTTONTEXT_NEXT      "Execute"

;---------------------
;Include Modern UI

  !include "MUI.nsh"

;--------------------------------
;Product Info

Name "Math::Script Test"

;--------------------------------
;Configuration

  ;General
  OutFile "MathTest.exe"

;--------------------------------
;Variables

  Var TEMP1
  Var TEMP2
  Var TEMP3

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "mathtest.txt"
  Page custom ScriptPageEnter
  Page instfiles
 
;--------------------------------
;Modern UI Configuration

;  !define MUI_ABORTWARNING
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Reserve Files
  
  ;Things that need to be extracted on first (keep these lines before any File command!)
  ;Only for BZIP2 compression
  
  ReserveFile "MathTest.ini"
  !insertmacro MUI_RESERVEFILE_INSTALLOPTIONS

;--------------------------------
;Installer Functions

LangString SCRIPTSAMPLE0 ${LANG_ENGLISH} "r0 = 'Hello'; r1 = 'Math::Script'\r\nr0 += ' from the ' + r1 + '!'; r1=''"
LangString SCRIPTSAMPLE1 ${LANG_ENGLISH} "a =0; b=1.0\r\n#{a++ < 100, b *= a}\r\nr0 = a; R0 = b; R1 = ff(b, 15)\r\nr1 = (a-1) + '! = ' + b"
LangString SCRIPTSAMPLE2 ${LANG_ENGLISH} 'pi=3.14159; \r\nangle = pi/4;\r\ntext = "x = " + ff(angle,16+3) \r\nr0 = text +=  ", sin x = " + sin(angle)'
LangString SCRIPTSAMPLE3 ${LANG_ENGLISH} "v1 = 123.456;  v2 = 123456789.1011\r\nr0 = v1; r1 = v2\r\nr2 = ff(v1, 3); r3 = ff(v2, 3); r4 = ff(v1, 3+16); r5 = ff(v2, 3+16)\r\nr6 = ff(v1, 3+32); r7 = ff(v2, 3+32); r8 = ff(v1, 3+32+64); r9 = ff(v2, 3+32+64)\r\n"
LangString SCRIPTSAMPLE4 ${LANG_ENGLISH} "a = 10000; b = 0; #{--a > 0, b+= a}; r0 = a; r1 = b\r\nz = 1.55; r2 = #[z > 1.5, 'Its greater', 'Its lower']\r\nz = 1.45; r3 = #[z > 1.5, 'Its greater', 'Its lower']"
LangString SCRIPTSAMPLE5 ${LANG_ENGLISH} 'r0 = "123a123"\r\nr1 = r0; \r\nr2 = s(r0); r3 = f(r0); r4 = i(r0); r5 = l(r0)' 

Function .onInit

  ;Extract InstallOptions INI files
  !insertmacro MUI_INSTALLOPTIONS_EXTRACT "MathTest.ini"

  Strcpy "$TEMP1" "$(SCRIPTSAMPLE0)"
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 2" "State" $TEMP1
  
FunctionEnd

LangString TEXT_IO_TITLE ${LANG_ENGLISH} "MathTest Script Page"
LangString TEXT_IO_SUBTITLE ${LANG_ENGLISH} "Try your scripting capapibilites or test one of sample scripts"


Function DumpVariables
  Strcpy "$TEMP1" "$$0='$0'\r\n$$1='$1'\r\n$$2='$2'\r\n$$3='$3'\r\n$$4='$4'\r\n$$5='$5'\r\n$$6='$6'\r\n$$7='$7'\r\n$$8='$8'\r\n$$9='$9'"
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 3" "State" $TEMP1  
  Strcpy "$TEMP1" "$$R0='$R0'\r\n$$R1='$R1'\r\n$$R2='$R2'\r\n$$R3='$R3'\r\n$$R4='$R4'\r\n$$R5='$R5'\r\n$$R6='$R6'\r\n$$R7='$R7'\r\n$$R8='$R8'\r\n$$R9='$R9'"
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 4" "State" $TEMP1  
FunctionEnd

Function ClearVariables
  Math::Script "r0=r1=r2=r3=r4=r5=r6=r7=r8=r9=R0=R1=R2=R3=R4=R5=R6=R7=R8=R9=''"
FunctionEnd

Function GetLine
  push $TEMP1
  Math::Script "mtsDL()"
  pop $TEMP2
  pop $TEMP1
FunctionEnd

Function ExecuteScript
  !insertmacro MUI_INSTALLOPTIONS_READ $TEMP1 "MathTest.ini" "Field 2" "State" 

  Math::Script "mtsTQ(s) (s = s(NS); #[s[0]=='$\"',s=s[1,]]; #[s[-1]=='$\"',s=s[,-2]]; NS = s)"
  Math::Script "mtsP(s,e, p,i) (p=-1;i=0; #{(i<l(s))&&(p<0), #[s[i,i+l(e)-1]==e, p=i]; i++}; p)"
  Math::Script "mtsDL(s) (s=s(NS); p=mtsP(s,'\r\n'); #[p>=0, (NS=s[p+4,]; NS=#[p>0,s[,p-1],'']), (NS='';NS=s)])"

  push  $TEMP1
  ; remove ""
  Math::Script "mtsTQ()"
  pop   $TEMP1

  ; script at $TEMP1
Go:
   StrLen $TEMP3 $TEMP1
   IntCmp $TEMP3 0 End
   ; get single line to $TEMP2
   Call GetLine
;   MessageBox MB_OK "'$TEMP2'      '$TEMP1'"
   Math::Script "$TEMP2"
   goto Go
End:
   Math::Script ""
FunctionEnd

Function ScriptPageEnter
      
  !insertmacro MUI_HEADER_TEXT "$(TEXT_IO_TITLE)" "$(TEXT_IO_SUBTITLE)"

Again:
  Call ClearVariables
  Call ExecuteScript
  Call DumpVariables

  !insertmacro MUI_INSTALLOPTIONS_DISPLAY_RETURN "mathtest.ini"
  pop $TEMP3

  !insertmacro MUI_INSTALLOPTIONS_READ $TEMP1 "MathTest.ini" "Field 5" "State"
  IntCmp $TEMP1 1 Test

  Strcpy "$TEMP2" "$(SCRIPTSAMPLE1)"
  !insertmacro MUI_INSTALLOPTIONS_READ $TEMP1 "MathTest.ini" "Field 6" "State"
  IntCmp $TEMP1 1 Write

  Strcpy "$TEMP2" "$(SCRIPTSAMPLE2)"
  !insertmacro MUI_INSTALLOPTIONS_READ $TEMP1 "MathTest.ini" "Field 7" "State"
  IntCmp $TEMP1 1 Write

  Strcpy "$TEMP2" "$(SCRIPTSAMPLE3)"
  !insertmacro MUI_INSTALLOPTIONS_READ $TEMP1 "MathTest.ini" "Field 8" "State"
  IntCmp $TEMP1 1 Write

  Strcpy "$TEMP2" "$(SCRIPTSAMPLE4)"
  !insertmacro MUI_INSTALLOPTIONS_READ $TEMP1 "MathTest.ini" "Field 9" "State"
  IntCmp $TEMP1 1 Write

  Strcpy "$TEMP2" "$(SCRIPTSAMPLE5)"

Write:
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 2" "State" "$TEMP2"
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 5" "State" "1"
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 6" "State" "0"
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 7" "State" "0"
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 8" "State" "0"
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 9" "State" "0"
  !insertmacro MUI_INSTALLOPTIONS_WRITE "MathTest.ini" "Field 10" "State" "0"

Test:
  Strcmp $TEMP3 "success" Again

FunctionEnd

Section "Dummy Section" SecDummy  
SectionEnd
