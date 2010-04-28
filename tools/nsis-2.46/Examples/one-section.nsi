; one-section.nsi
;
; This example demonstrates how to control section selection.
; It allows only one of the sections of a group to be selected.

;--------------------------------

; Section define/macro header file
; See this header file for more info

!include "Sections.nsh"

;--------------------------------

Name "One Section"
OutFile "one-section.exe"
RequestExecutionLevel user

;--------------------------------

; Pages

Page components

;--------------------------------

; Sections

Section !Required
  SectionIn RO
SectionEnd

Section "Group 1 - Option 1" g1o1
SectionEnd

Section /o "Group 1 - Option 2" g1o2
SectionEnd

Section /o "Group 1 - Option 3" g1o3
SectionEnd

Section "Group 2 - Option 1" g2o1
SectionEnd

Section /o "Group 2 - Option 2" g2o2
SectionEnd

Section /o "Group 2 - Option 3" g2o3
SectionEnd

;--------------------------------

; Functions

; $1 stores the status of group 1
; $2 stores the status of group 2

Function .onInit

  StrCpy $1 ${g1o1} ; Group 1 - Option 1 is selected by default
  StrCpy $2 ${g2o1} ; Group 2 - Option 1 is selected by default

FunctionEnd

Function .onSelChange

  !insertmacro StartRadioButtons $1
    !insertmacro RadioButton ${g1o1}
    !insertmacro RadioButton ${g1o2}
    !insertmacro RadioButton ${g1o3}
  !insertmacro EndRadioButtons
	
  !insertmacro StartRadioButtons $2
    !insertmacro RadioButton ${g2o1}
    !insertmacro RadioButton ${g2o2}
    !insertmacro RadioButton ${g2o3}
  !insertmacro EndRadioButtons
	
FunctionEnd