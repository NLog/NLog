!verbose push
!verbose 3

!include LogicLib.nsh
!include Sections.nsh

!ifndef ___MEMENTO_NSH___
!define ___MEMENTO_NSH___

#####################################
### Memento                       ###
#####################################

/*

Memento is a set of macros that allow installers to remember user selection
across separate runs of the installer. Currently, it can remember the state
of sections and mark new sections as bold. In the future, it'll integrate
InstallOptions and maybe even the Modern UI.

A usage example can be found in `Examples\Memento.nsi`.

*/

#####################################
### Usage Instructions            ###
#####################################

/*

1. Declare usage of Memento by including Memento.nsh at the top of the script.

      !include Memento.nsh

2. Define MEMENTO_REGISTRY_ROOT and MEMENTO_REGISTRY_KEY with the a registry key
   where sections' state should be saved.

      !define MEMENTO_REGISTRY_ROOT HKLM
      !define MEMENTO_REGISTRY_KEY \
                Software\Microsoft\Windows\CurrentVersion\Uninstall\MyProgram

3. Replace Section with ${MementoSection} and SectionEnd with ${MementoSectionEnd}
   for sections that whose state should be remembered by Memento.

   For sections that should be unselected by default, use ${MementoSection}'s
   brother - ${MementoUnselectedSection}.

   Sections that don't already have an identifier must be assigned one.

   Section identifiers must stay the same across different versions of the
   installer or their state will be forgotten.

4. Use ${MementoSectionDone} after the last ${MementoSection}.

5. Add a call to ${MementoSectionRestore} to .onInit to restore the state
   of all sections from the registry.

      Function .onInit

        ${MementoSectionRestore}

      FunctionEnd

6. Add a call to ${MementoSectionSave} to .onInstSuccess to save the state
   of all sections to the registry.

      Function .onInstSuccess

        ${MementoSectionSave}

      FunctionEnd

7. Tattoo the location of the chosen registry key on your arm.

*/

#####################################
### User API                      ###
#####################################

;
; ${MementoSection}
;
;   Defines a section whose state is remembered by Memento.
;
;   Usage is similar to Section.
;
;     ${MementoSection} "name" "some_id"
;

!define MementoSection "!insertmacro MementoSection"

;
; ${MementoSectionEnd}
;
;   Ends a section previously opened using ${MementoSection}.
;
;   Usage is similar to SectionEnd.
;
;     ${MementoSection} "name" "some_id"
;        # some code...
;     ${MementoSectionEnd}
;

;
; ${MementoUnselectedSection}
;
;   Defines a section whose state is remembered by Memento and is
;   unselected by default.
;
;   Usage is similar to Section with the /o switch.
;
;     ${MementoUnselectedSection} "name" "some_id"
;

!define MementoUnselectedSection "!insertmacro MementoUnselectedSection"

;
; ${MementoSectionEnd}
;
;   Ends a section previously opened using ${MementoSection}.
;
;   Usage is similar to SectionEnd.
;
;     ${MementoSection} "name" "some_id"
;        # some code...
;     ${MementoSectionEnd}
;

!define MementoSectionEnd "!insertmacro MementoSectionEnd"

;
; ${MementoSectionDone}
;
;   Used after all ${MementoSection} have been set.
;
;     ${MementoSection} "name1" "some_id1"
;        # some code...
;     ${MementoSectionEnd}
;
;     ${MementoSection} "name2" "some_id2"
;        # some code...
;     ${MementoSectionEnd}
;
;     ${MementoSection} "name3" "some_id3"
;        # some code...
;     ${MementoSectionEnd}
;
;     ${MementoSectionDone}
;

!define MementoSectionDone "!insertmacro MementoSectionDone"

;
; ${MementoSectionRestore}
;
;   Restores the state of all Memento sections from the registry.
;
;   Commonly used in .onInit.
;
;     Function .onInit
;
;       ${MementoSectionRestore}
;
;     FunctionEnd
;

!define MementoSectionRestore "!insertmacro MementoSectionRestore"

;
; ${MementoSectionSave}
;
;   Saves the state of all Memento sections to the registry.
;
;   Commonly used in .onInstSuccess.
;
;     Function .onInstSuccess
;
;       ${MementoSectionSave}
;
;     FunctionEnd
;

!define MementoSectionSave "!insertmacro MementoSectionSave"


#####################################
### Internal Defines              ###
#####################################

!define __MementoSectionIndex 1

#####################################
### Internal Macros               ###
#####################################

!macro __MementoCheckSettings

  !ifndef MEMENTO_REGISTRY_ROOT | MEMENTO_REGISTRY_KEY

    !error "MEMENTO_REGISTRY_ROOT and MEMENTO_REGISTRY_KEY must be defined before using any of Memento's macros"

  !endif

!macroend

!macro __MementoSection flags name id

  !insertmacro __MementoCheckSettings

  !ifndef __MementoSectionIndex

    !error "MementoSectionDone already used!"

  !endif

  !define __MementoSectionLastSectionId `${id}`

  !verbose pop

  Section ${flags} `${name}` `${id}`

  !verbose push
  !verbose 3

!macroend

#####################################
### User Macros                   ###
#####################################

!macro MementoSection name id

  !verbose push
  !verbose 3

  !insertmacro __MementoSection "" `${name}` `${id}`

  !verbose pop

!macroend

!macro MementoUnselectedSection name id

  !verbose push
  !verbose 3

  !insertmacro __MementoSection /o `${name}` `${id}`

  !define __MementoSectionUnselected

  !verbose pop

!macroend

!macro MementoSectionEnd

  SectionEnd

  !verbose push
  !verbose 3

  !insertmacro __MementoCheckSettings

  !ifndef __MementoSectionIndex

    !error "MementoSectionDone already used!"

  !endif

  !define /MATH __MementoSectionIndexNext \
      ${__MementoSectionIndex} + 1

  Function __MementoSectionMarkNew${__MementoSectionIndex}

    ClearErrors
    ReadRegDWORD $0 ${MEMENTO_REGISTRY_ROOT} `${MEMENTO_REGISTRY_KEY}` `MementoSection_${__MementoSectionLastSectionId}`

    ${If} ${Errors}

      !insertmacro SetSectionFlag `${${__MementoSectionLastSectionId}}` ${SF_BOLD}

    ${EndIf}

    GetFunctionAddress $0 __MementoSectionMarkNew${__MementoSectionIndexNext}
    Goto $0

  FunctionEnd

  Function __MementoSectionRestoreStatus${__MementoSectionIndex}

    ClearErrors
    ReadRegDWORD $0 ${MEMENTO_REGISTRY_ROOT} `${MEMENTO_REGISTRY_KEY}` `MementoSection_${__MementoSectionLastSectionId}`

    !ifndef __MementoSectionUnselected

      ${If} ${Errors}
      ${OrIf} $0 != 0

        !insertmacro SelectSection `${${__MementoSectionLastSectionId}}`

      ${Else}

        !insertmacro UnselectSection `${${__MementoSectionLastSectionId}}`

      ${EndIf}

    !else

      !undef __MementoSectionUnselected

      ${If} ${Errors}
      ${OrIf} $0 == 0

        !insertmacro UnselectSection `${${__MementoSectionLastSectionId}}`

      ${Else}

        !insertmacro SelectSection `${${__MementoSectionLastSectionId}}`

      ${EndIf}

    !endif

    GetFunctionAddress $0 __MementoSectionRestoreStatus${__MementoSectionIndexNext}
    Goto $0

  FunctionEnd

  Function __MementoSectionSaveStatus${__MementoSectionIndex}

    ${If} ${SectionIsSelected} `${${__MementoSectionLastSectionId}}`

      WriteRegDWORD ${MEMENTO_REGISTRY_ROOT} `${MEMENTO_REGISTRY_KEY}` `MementoSection_${__MementoSectionLastSectionId}` 1

    ${Else}

      WriteRegDWORD ${MEMENTO_REGISTRY_ROOT} `${MEMENTO_REGISTRY_KEY}` `MementoSection_${__MementoSectionLastSectionId}` 0

    ${EndIf}

    GetFunctionAddress $0 __MementoSectionSaveStatus${__MementoSectionIndexNext}
    Goto $0

  FunctionEnd

  !undef __MementoSectionIndex
  !define __MementoSectionIndex ${__MementoSectionIndexNext}
  !undef __MementoSectionIndexNext

  !undef __MementoSectionLastSectionId

  !verbose pop

!macroend

!macro MementoSectionDone

  !verbose push
  !verbose 3

  !insertmacro __MementoCheckSettings

  Function __MementoSectionMarkNew${__MementoSectionIndex}
  FunctionEnd

  Function __MementoSectionRestoreStatus${__MementoSectionIndex}
  FunctionEnd

  Function __MementoSectionSaveStatus${__MementoSectionIndex}
  FunctionEnd

  !undef __MementoSectionIndex

  !verbose pop

!macroend

!macro MementoSectionRestore

  !verbose push
  !verbose 3

  !insertmacro __MementoCheckSettings

  Push $0
  Push $1
  Push $2
  Push $3

    # check for first usage

    ClearErrors

    ReadRegStr $0 ${MEMENTO_REGISTRY_ROOT} `${MEMENTO_REGISTRY_KEY}` MementoSectionUsed

    ${If} ${Errors}

      # use script defaults on first run
      Goto done

    ${EndIf}

    # mark new components in bold
    
    Call __MementoSectionMarkNew1

    # mark section groups in bold

    StrCpy $0 0
    StrCpy $1 ""
    StrCpy $2 ""
    StrCpy $3 ""

    loop:

      ClearErrors

      ${If} ${SectionIsBold} $0

        ${If} $1 != ""

          !insertmacro SetSectionFlag $1 ${SF_BOLD}

        ${EndIf}

        ${If} $2 != ""

          !insertmacro SetSectionFlag $2 ${SF_BOLD}

        ${EndIf}

        ${If} $3 != ""

          !insertmacro SetSectionFlag $3 ${SF_BOLD}

        ${EndIf}

      ${ElseIf} ${Errors}

        Goto loop_end

      ${EndIf}

      ${If} ${SectionIsSectionGroup} $0

        ${If} $1 == ""

          StrCpy $1 $0

        ${ElseIf} $2 == ""

          StrCpy $2 $0

        ${ElseIf} $3 == ""

          StrCpy $3 $0

        ${EndIf}

      ${EndIf}

      ${If} ${SectionIsSectionGroupEnd} $0

        ${If} $3 != ""

          StrCpy $3 ""

        ${ElseIf} $2 != ""

          StrCpy $2 ""

        ${ElseIf} $1 != ""

          StrCpy $1 ""

        ${EndIf}

      ${EndIf}

      IntOp $0 $0 + 1

    Goto loop
    loop_end:

    # restore sections' status

    Call __MementoSectionRestoreStatus1

  # all done

  done:

  Pop $3
  Pop $2
  Pop $1
  Pop $0

  !verbose pop

!macroend

!macro MementoSectionSave

  !verbose push
  !verbose 3

  !insertmacro __MementoCheckSettings

  Push $0

    WriteRegStr ${MEMENTO_REGISTRY_ROOT} `${MEMENTO_REGISTRY_KEY}` MementoSectionUsed ""
  
    Call __MementoSectionSaveStatus1

  Pop $0

  !verbose pop

!macroend



!endif # ___MEMENTO_NSH___

!verbose pop
