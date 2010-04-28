!include LogicLib.nsh
!include Memento.nsh

Name Memento
OutFile Memento.exe

XPStyle on

ShowInstDetails show

Page components
Page instfiles

RequestExecutionLevel user

# settings

!define MEMENTO_REGISTRY_ROOT HKCU
!define MEMENTO_REGISTRY_KEY "Software\NSIS\Memento Test"

# restore

Function .onInit

  ${If} ${Cmd} `MessageBox MB_YESNO "Would you like to load an example state?" IDYES`
    
	DeleteRegKey  HKCU "Software\NSIS\Memento Test"

	WriteRegStr   HKCU "Software\NSIS\Memento Test" MementoSectionUsed ""
	WriteRegDWORD HKCU "Software\NSIS\Memento Test" MementoSection_sec_horse   1
	WriteRegDWORD HKCU "Software\NSIS\Memento Test" MementoSection_sec_chicken 1
	WriteRegDWORD HKCU "Software\NSIS\Memento Test" MementoSection_sec_donkey  0
	WriteRegDWORD HKCU "Software\NSIS\Memento Test" MementoSection_sec_croc    0

  ${EndIf}

  ${MementoSectionRestore}

FunctionEnd

# sections

${MementoSection} horse sec_horse
${MementoSectionEnd}

${MementoSection} donkey sec_donkey
${MementoSectionEnd}

${MementoSection} chicken sec_chicken
${MementoSectionEnd}

SectionGroup /e group

  SectionGroup /e group

    ${MementoSection} croc sec_croc
    ${MementoSectionEnd}

    ${MementoSection} cow sec_cow
    ${MementoSectionEnd}

  SectionGroupEnd

SectionGroupEnd

${MementoUnselectedSection} dinosaur sec_dinosaur
${MementoSectionEnd}

# done...

${MementoSectionDone}

# save

Function .onInstSuccess

  ${MementoSectionSave}

FunctionEnd
