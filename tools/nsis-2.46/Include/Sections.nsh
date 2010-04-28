; Sections.nsh
;
; Defines and macros for section control
;
; Include in your script using:
; !include "Sections.nsh"

;--------------------------------

!ifndef SECTIONS_INCLUDED

!define SECTIONS_INCLUDED

;--------------------------------

; Generic section defines

# section or section group is selected
!define SF_SELECTED   1
# section group
!define SF_SECGRP     2
!define SF_SUBSEC     2  # deprecated
# section group end marker
!define SF_SECGRPEND  4
!define SF_SUBSECEND  4  # deprecated
# bold text (Section !blah)
!define SF_BOLD       8
# read only (SectionIn RO)
!define SF_RO         16
# expanded section group (SectionGroup /e blah)
!define SF_EXPAND     32
# section group is partially selected
!define SF_PSELECTED  64  # internal
# internal
!define SF_TOGGLED    128 # internal
!define SF_NAMECHG    256 # internal

# mask to toggle off the selected flag
!define SECTION_OFF   0xFFFFFFFE

;--------------------------------

; Select / unselect / reserve section

!macro SelectSection SECTION

  Push $0
  Push $1
    StrCpy $1 "${SECTION}"
    SectionGetFlags $1 $0
    IntOp $0 $0 | ${SF_SELECTED}
    SectionSetFlags $1 $0
  Pop $1
  Pop $0

!macroend

!macro UnselectSection SECTION

  Push $0
  Push $1
    StrCpy $1 "${SECTION}"
    SectionGetFlags $1 $0
    IntOp $0 $0 & ${SECTION_OFF}
    SectionSetFlags $1 $0
  Pop $1
  Pop $0

!macroend

; If section selected, will unselect, if unselected, will select

!macro ReverseSection SECTION

  Push $0
  Push $1
    StrCpy $1 "${SECTION}"
    SectionGetFlags $1 $0
    IntOp $0 $0 ^ ${SF_SELECTED}
    SectionSetFlags $1 $0
  Pop $1
  Pop $0

!macroend

;--------------------------------

; Macros for mutually exclusive section selection
; Written by Tim Gallagher
;
; See one-section.nsi for an example of usage

; Starts the Radio Button Block
; You should pass a variable that keeps the selected section
; as the first parameter for this macro. This variable should
; be initialized to the default section's index.
;
; As this macro uses $R0 and $R1 you can't use those two as the
; varible which will keep the selected section.

!macro StartRadioButtons var

  !define StartRadioButtons_Var "${var}"

  Push $R0
  
   SectionGetFlags "${StartRadioButtons_Var}" $R0
   IntOp $R0 $R0 & ${SECTION_OFF}
   SectionSetFlags "${StartRadioButtons_Var}" $R0
   
  Push $R1
  
    StrCpy $R1 "${StartRadioButtons_Var}"
   
!macroend

; A radio button

!macro RadioButton SECTION_NAME

  SectionGetFlags ${SECTION_NAME} $R0
  IntOp $R0 $R0 & ${SF_SELECTED}
  IntCmp $R0 ${SF_SELECTED} 0 +2 +2
  StrCpy "${StartRadioButtons_Var}" ${SECTION_NAME}

!macroend

; Ends the radio button block

!macro EndRadioButtons
  
  StrCmp $R1 "${StartRadioButtons_Var}" 0 +4 ; selection hasn't changed
    SectionGetFlags "${StartRadioButtons_Var}" $R0
    IntOp $R0 $R0 | ${SF_SELECTED}
    SectionSetFlags "${StartRadioButtons_Var}" $R0

  Pop $R1
  Pop $R0
  
  !undef StartRadioButtons_Var

!macroend

;--------------------------------

; These are two macros you can use to set a Section in an InstType
; or clear it from an InstType.
;
; Written by Robert Kehl
;
; For details, see http://nsis.sourceforge.net/wiki/SetSectionInInstType%2C_ClearSectionInInstType
;
; Use the defines below for the WANTED_INSTTYPE paramter.

!define INSTTYPE_1 1
!define INSTTYPE_2 2
!define INSTTYPE_3 4
!define INSTTYPE_4 8
!define INSTTYPE_5 16
!define INSTTYPE_6 32
!define INSTTYPE_7 64
!define INSTTYPE_8 128
!define INSTTYPE_9 256
!define INSTTYPE_10 512
!define INSTTYPE_11 1024
!define INSTTYPE_12 2048
!define INSTTYPE_13 4096
!define INSTTYPE_14 8192
!define INSTTYPE_15 16384
!define INSTTYPE_16 32768
!define INSTTYPE_17 65536
!define INSTTYPE_18 131072
!define INSTTYPE_19 262144
!define INSTTYPE_20 524288
!define INSTTYPE_21 1048576
!define INSTTYPE_22 2097152
!define INSTTYPE_23 4194304
!define INSTTYPE_24 8388608
!define INSTTYPE_25 16777216
!define INSTTYPE_26 33554432
!define INSTTYPE_27 67108864
!define INSTTYPE_28 134217728
!define INSTTYPE_29 268435456
!define INSTTYPE_30 536870912
!define INSTTYPE_31 1073741824
!define INSTTYPE_32 2147483648

!macro SetSectionInInstType SECTION_NAME WANTED_INSTTYPE

  Push $0
  Push $1
    StrCpy $1 "${SECTION_NAME}"
    SectionGetInstTypes $1 $0
    IntOp $0 $0 | ${WANTED_INSTTYPE}
    SectionSetInstTypes $1 $0
  Pop $1
  Pop $0

!macroend

!macro ClearSectionInInstType SECTION_NAME WANTED_INSTTYPE

  Push $0
  Push $1
  Push $2
    StrCpy $2 "${SECTION_NAME}"
    SectionGetInstTypes $2 $0
    StrCpy $1 ${WANTED_INSTTYPE}
    IntOp $1 $1 ~
    IntOp $0 $0 & $1
    SectionSetInstTypes $2 $0
  Pop $2
  Pop $1
  Pop $0

!macroend

;--------------------------------

; Set / clear / check bits in a section's flags
; Written by derekrprice

; Set one or more bits in a sections's flags

!macro SetSectionFlag SECTION BITS

  Push $R0
  Push $R1
    StrCpy $R1 "${SECTION}"
    SectionGetFlags $R1 $R0
    IntOp $R0 $R0 | "${BITS}"
    SectionSetFlags $R1 $R0
  Pop $R1
  Pop $R0
 
!macroend

; Clear one or more bits in section's flags

!macro ClearSectionFlag SECTION BITS

  Push $R0
  Push $R1
  Push $R2
    StrCpy $R2 "${SECTION}"
    SectionGetFlags $R2 $R0
    IntOp $R1 "${BITS}" ~
    IntOp $R0 $R0 & $R1
    SectionSetFlags $R2 $R0
  Pop $R2
  Pop $R1
  Pop $R0

!macroend

; Check if one or more bits in section's flags are set
; If they are, jump to JUMPIFSET
; If not, jump to JUMPIFNOTSET

!macro SectionFlagIsSet SECTION BITS JUMPIFSET JUMPIFNOTSET
	Push $R0
	SectionGetFlags "${SECTION}" $R0
	IntOp $R0 $R0 & "${BITS}"
	IntCmp $R0 "${BITS}" +3
	Pop $R0
	StrCmp "" "${JUMPIFNOTSET}" +3 "${JUMPIFNOTSET}"
	Pop $R0
	Goto "${JUMPIFSET}"
!macroend

;--------------------------------

!endif