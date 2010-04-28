/*

LangFile.nsh

Header file to create langauge files that can be
included with a single command.

Copyright 2008-2009 Joost Verburg

* Either LANGFILE_INCLUDE or LANGFILE_INCLUDE_WITHDEFAULT
  can be called from the script to include a language
  file.

  - LANGFILE_INCLUDE takes the language file name as parameter.
  - LANGFILE_INCLUDE_WITHDEFAULT takes as additional second
    parameter the default language file to load missing strings
    from.

* A language file start with:
  !insertmacro LANGFILE_EXT "English"
  using the same name as the standard NSIS language file.

* Language strings in the language file have the format:
  ${LangFileString} LANGSTRING_NAME "Text"

*/

!ifndef LANGFILE_INCLUDED
!define LANGFILE_INCLUDED

!macro LANGFILE_INCLUDE FILENAME

  ;Called from script: include a langauge file

  !ifdef LangFileString
    !undef LangFileString
  !endif

  !define LangFileString "!insertmacro LANGFILE_SETSTRING"

  !define LANGFILE_SETNAMES
  !include "${FILENAME}"
  !undef LANGFILE_SETNAMES

  ;Create language strings

  !undef LangFileString
  !define LangFileString "!insertmacro LANGFILE_LANGSTRING"
  !include "${FILENAME}"

!macroend

!macro LANGFILE_INCLUDE_WITHDEFAULT FILENAME FILENAME_DEFAULT

  ;Called from script: include a langauge file
  ;Obtains missing strings from a default file

  !ifdef LangFileString
    !undef LangFileString
  !endif

  !define LangFileString "!insertmacro LANGFILE_SETSTRING"

  !define LANGFILE_SETNAMES
  !include "${FILENAME}"
  !undef LANGFILE_SETNAMES

  ;Include default language for missing strings
  !include "${FILENAME_DEFAULT}"
  
  ;Create language strings
  !undef LangFileString
  !define LangFileString "!insertmacro LANGFILE_LANGSTRING"
  !include "${FILENAME_DEFAULT}"

!macroend

!macro LANGFILE IDNAME NAME

  ;Start of standard NSIS language file

  !ifdef LANGFILE_SETNAMES

    !ifdef LANGFILE_IDNAME
      !undef LANGFILE_IDNAME
    !endif

    !define LANGFILE_IDNAME "${IDNAME}"

    !ifndef "LANGFILE_${IDNAME}_NAME"
      !define "LANGFILE_${IDNAME}_NAME" "${NAME}"
    !endif

  !endif

!macroend

!macro LANGFILE_EXT IDNAME

  ;Start of installer language file
  
  !ifdef LANGFILE_SETNAMES

    !ifdef LANGFILE_IDNAME
      !undef LANGFILE_IDNAME
    !endif

    !define LANGFILE_IDNAME "${IDNAME}"

  !endif

!macroend

!macro LANGFILE_SETSTRING NAME VALUE

  ;Set define with translated string

  !ifndef ${NAME}
    !define "${NAME}" "${VALUE}"
  !endif

!macroend

!macro LANGFILE_LANGSTRING NAME DUMMY

  ;Create a language string from a define and undefine

  LangString "${NAME}" "${LANG_${LANGFILE_IDNAME}}" "${${NAME}}"
  !undef "${NAME}"

!macroend

!endif
