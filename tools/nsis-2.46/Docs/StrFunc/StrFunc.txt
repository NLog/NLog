String Functions Header File Readme
-----------------------------------

String Functions Header File contains a set of good string manipulation
functions in a much simpler way to include and call in NSIS scripts.

How to use
----------

  Basic Syntax
  ------------

  Parameters are specified in this format:
  required (required) (option1 | option2) [optional or add. options]
  [option1 | option2]

  The stars in command titles (*****) are the function usefulness in my
  opinion. The more starts, the more useful it is. 5 stars (*****) is the
  most useful.

  Any time when is mentioned "Default is" means that you can use the value
  mentioned or keep it blank, the result is the same.

  If you want a certain value (e.g. a text) to be language-specific, set a
  language string (using LangString) and define $(STRINGNAME) as value.

  If you want to add ` to a string, you should always escape it using $\`
  because the header file macro functions use ` to separate parameters.

  1. Include Header file
  ----------------------

    !include "StrFunc.nsh"

    StrFunc.nsh has to be inside Include directory, so you don't have to
    specify a path.

    You have to put this command before any command used in this header file.

  2. Defines
  ----------

    This header file contains defines that automate the life of some who
    fear a lot of changes sometimes imposed in this header file, or who have
    applications that put it to work at maximum capacity. Before you try
    these, take a look at the information below:

      - Every item on a define value is separated by a "|", and every subitem
        (items in an item) is separated by " ".

      - Use ${StrTok} $var "${DefineName}" "|" "$counter" "0" to get every
        item inside the define. For subitems, use ${StrTok} $var2 "$var" " "
        "$counter2" "0" after getting the value for a desired item.

      - ${StrFunc_List} is automatically made by the header file. The rest
        is manually added to the header.

    2.1 Defines List:
    -----------------

    StrFunc_List - Lists all function names currently available on StrFunc
                   header file.

    *_List       - Lists all parameter names currently available for "*"
                   function. (* = function name - i.e. StrTok_List).

    *_TypeList   - Lists the types of all parameters on "*" function.
                   (* = function name - i.e. StrTok_List). Possible types
                   for each parameter:

                   - Output - Needs a variable to output a function result.

                   - Text   - Needs text or number to be input.

                   - Mixed  - Needs text, number or option to be inputed.
                              Each subitem following the "Mixed" word is an
                              option. The first option is ever the default
                              one. Two following spaces "  " means that
                              that subitem is empty.

                   - Option - Needs an option to be inputed. Each subitem
                              following the "Option" word is an option.
                              The first option is ever the default one. Two
                              following spaces "  " means that that subitem
                              is empty.

  3. Commands
  -----------

  Some commands have special specifications to work. Consult command's
  documentation on "3.3 Commands" section.

    3.1 How To Use Commands In Install Sections and Functions
    ---------------------------------------------------------

    Every command used in install sections and functions have to be called
    first before and out of any sections and functions, and without
    parameters.

    Example:
    --------

    ${StrStr}
    
    3.2 How To Use Commands In Uninstall Sections and Functions
    -----------------------------------------------------------

    Commands with Uninstall Sections and Functions support have "Un" before
    the words inside curly brackets "{}".

    Example:
    --------
    
    ${UnStrStr}

    A complete example with both Install and Uninstall Commands:
    ------------------------------------------------------------


    !include "StrFunc.nsh"

    ${StrStr} # Supportable for Install Sections and Functions

    ${UnStrStr} # Supportable for Uninstall Sections and Functions

    Section

      ${StrStr} $0 "OK! Now what?" "wh"

    SectionEnd

    Section Uninstall

      ${UnStrStr} $0 "OK! Now what?" "wh"

    SectionEnd

    3.3 Commands
    ------------

    =========================================================================
    **                                                             ${StrCase}
    -------------------------------------------------------------------------
    ResultVar String Type(|L|U|T|S|<>)
    =========================================================================
    Converts "String" to "Type" Case. Uses LogicLib.

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String to convert to "Type" case.
      
      Type
      Type of string case to convert to:

        - "" = Original Case (same as "String")
        - L = Lower Case (this is just an example. a very simple one.)
        - U = Upper Case (THIS IS JUST AN EXAMPLE. A VERY SIMPLE ONE.)
        - T = Title Case (This Is Just An Example. A Very Simple One.)
        - S = Sentence Case (This is just an example. A very simple one.)
        - <> = Switch Case (This is just an example. A very simple one.)
        
      Default value is "" (Original Case).

    Result Value -> ResultVar:

      "String" in "Type" case.

    Example:

      ${StrCase} $0 '"Você" is "You" in English.' "U"
                    [__(_)__()___()__()__(____)_]

      $0 = '"VOCÊ" IS "YOU" IN ENGLISH.'

    =========================================================================
    *                                                               ${StrClb}
    -------------------------------------------------------------------------
    ResultVar String Action(|>|<|<>)
    =========================================================================
    Makes an action with the clipboard depending on value of parameter
    "Action". Uses LogicLib.

    Parameters:

      String
      If "Action" = ">" or "<>" - String to put on the clipboard.

      Action
      Can be one of the following values:

        - "" = Cleans the clipboard.
        - ">" = Set string to clipboard.
        - "<" = Get string from clipboard.
        - "<>" = Swap string with clipboard's.

    Result Value -> ResultVar:

      If "Action" = "<" or "<>" - String found on the clipboard.

    =========================================================================
    ***                                                        ${StrIOToNSIS}
    -------------------------------------------------------------------------
    ResultVar String
    =========================================================================
    Convert "String" from Install Options plugin to be supported by NSIS.
    Escape, back-slash, carriage return, line feed and tab characters are
    converted.

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String to convert to be supportable for NSIS.

    Result Value -> ResultVar:

      "String" supportable for NSIS.

    Example:

      ${StrIOToNSIS} $0 "\r\n\t\\This is just an example\\"
                        [()()()()_______________________()]

      $0 = "$\r$\n$\t\This is just an example\"

    =========================================================================
    *                                                               ${StrLoc}
    -------------------------------------------------------------------------
    ResultVar String StrToSearchFor CounterDirection(>|<)
    =========================================================================
    Searches for "StrToSearchFor" in "String" and returns its location,
    according to "CounterDirection".

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String where to search "StrToSearchFor".

      StrToSearchFor
      String to search in "String".

      CounterDirection(>|<)
      Direction where the counter increases to. Default is ">".
      (> = increases from left to right, < = increases from right to left)

    Result Value -> ResultVar:

      Where "StrToSearchFor" is, according to "OffsetDirection".

    Example: 

      ${StrLoc} $0 "This is just an example" "just" "<"
                            (__)<<<<<<<<<<<

      $0 = "11"

    =========================================================================
    ***                                                        ${StrNSISToIO}
    -------------------------------------------------------------------------
    ResultVar String
    =========================================================================
    Converts "String" from NSIS to be supported by Install Options plugin.
    Escape, back-slash, carriage return, line feed and tab characters are
    converted.

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String to convert to be supportable for Install Options plugin.

    Result Value -> ResultVar:

      "String" supportable for Install Options plugin.

    Example:

      ${StrNSISToIO} $0 "$\r$\n$\t\This is just an example\"
                        [(_)(_)(_)^_______________________^]

      $0 = "\r\n\t\\This is just an example\\"

    =========================================================================
    *****                                                           ${StrRep}
    -------------------------------------------------------------------------
    ResultVar String StrToReplace ReplacementString
    =========================================================================
    Searches for all "StrToReplace" in "String" replacing those with
    "ReplacementString".

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String where to search "StrToReplace".

      StrToReplaceFor
      String to search in "String".

      StringToBeReplacedWith
      String to replace "StringToReplace" when it is found in "String".

    Result Value -> ResultVar:

      "String" with all occurrences of "StringToReplace" replaced with
      "ReplacementString".

    Example: 

      ${StrRep} $0 "This is just an example" "an" "one"
                    [____________()_______]

      $0 = "This is just one example"

    =========================================================================
    ***                                                            ${StrSort}
    -------------------------------------------------------------------------
    ResultVar String LeftStr CenterStr RightStr IncludeLeftStr(1|0)
    IncludeCenterStr(1|0) IncludeRightStr(1|0)
    =========================================================================
    Searches for "CenterStr" in "String", and returns only the value
    between "LeftStr" and "RightStr", including or not the "CenterStr" using
    "IncludeCenterStr" and/or the "LeftStr" using "IncludeLeftStr" and
    "RightStr" using "IncludeRightStr".

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String where to search "CenterStr".

      LeftStr
      The first occurrence of "LeftStr" on the left of "CenterStr".
      If it is an empty value, or was not found, will return
      everything on the left of "CenterStr".

      CenterStr
      String to search in "String".

      RightStr
      The first occurrence of "RightStr" on the right of "CenterStr".
      If it is an empty value, or was not found, will return
      everything on the right of "CenterStr".

      IncludeLeftStr(1|0)
      Include or not the "LeftStr" in the result value. Default is 1
      (True). (1 = True, 0 = False)

      IncludeCenterStr(1|0)
      Include or not the "CenterStr" in the result value. Default is 1
      (True). (1 = True, 0 = False)

      IncludeRightStr(1|0)
      Include or not the "RightStr" in the result value. Default is 1
      (True). (1 = True, 0 = False)

    Result Value -> ResultVar:

      String between "LeftStr" and "RightStr" of a found "CenterStr"
      including or not the "LeftStr" and "RightStr" if
      "IncludeLeftRightStr" is 1 and/or the "CenterStr" if
      "IncludeCenterStr" is 1.

    Example: 

      ${StrSort} $0 "This is just an example" " just" "" "ple" "0" "0" "0"
                    [_______(___)_______]( )
                              C           R

      $0 = "This is an exam"

    =========================================================================
    *****                                                           ${StrStr}
    -------------------------------------------------------------------------
    ResultVar String StrToSearchFor
    =========================================================================
    Searches for "StrToSearchFor" in "String".

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String where to search "StrToSearchFor".

      StrToSearchFor
      String to search in "String".

    Result Value -> ResultVar:

      "StrToSearchFor" + the string after where "StrToSearchFor" was found in
      "String".

    Example: 

      ${StrStr} $0 "This is just an example" "just"
                   >>>>>>>>>{_)____________]

      $0 = "just an example"

    =========================================================================
    *****                                                        ${StrStrAdv}
    -------------------------------------------------------------------------
    ResultVar String StrToSearchFor SearchDirection(>|<)
    ResultStrDirection(>|<) DisplayStrToSearch(1|0) Loops CaseSensitive(0|1)
    =========================================================================
    Searches for "StrToSearchFor" in "String" in the direction specified by
    "SearchDirection" and looping "Loops" times.

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String where to search "StrToSearchFor".

      StrToSearchFor
      String to search in "String".

      SearchDirection (>|<)
      Where do you want to direct the search. Default is ">" (to right).
      (< = To left, > = To right)

      ResultStrDirection (>|<)
      Where the result string will be based on in relation of
      "StrToSearchFor"
      position. Default is ">" (to right). (< = To left, > = To right)

      DisplayStrToSearch (1|0)
      Display "StrToSearchFor" in the result. Default is "1" (True).
      (1 = True, 0 = False)

      Loops
      Number of times the code will search "StrToSearchFor" in "String" not
      including the original execution. Default is "0" (1 code execution).

      CaseSensitive(0|1)
      If "1" the search will be case-sensitive (differentiates between cases).
      If "0" it is case-insensitive (does not differentiate between cases).
      Default is "0" (Case-Insensitive).


    Result Value -> ResultVar:

      "StrToSearchFor" if "DisplayStrToSearch" is 1 + the result string after
      or before "StrToSearchFor", depending on "ResultStrDirection".

    Result with Errors:

      When "StrToSearchFor" was not found, will return an empty string.

      When you put nothing in "StrToSearchFor", will return "String" and set
      error flag.

      When you put nothing in "String", will return an empty string and set
      error flag.

    Example: 

      ${StrStrAdv} $0 "This IS really just an example" "IS " ">" ">" "0" "0" "1"
                       >>>>>( )[____________________]                       


      $0 = "really just an example"

    =========================================================================
    ****                                                            ${StrTok}
    -------------------------------------------------------------------------
    ResultVar String Separators ResultPart[L] SkipEmptyParts(1|0)
    =========================================================================
    Returns the part "ResultPart" between two "Separators" inside
    "String".

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String where to search for "Separators".

      Separators
      Characters to find on "String".

      ResultPart[L]
      The part want to be found on "StrToTokenize" between two "Separators".
      Can be any number, starting at 0, and "L" that is the last part.
      Default is L (Last part).

      SkipEmptyParts(1|0)
      Skips empty string parts between two "Separators". Default is 1 (True).
      (1 = True, 0 = False)

    Result Value -> ResultVar:

      "String" part number "Part" between two "Separators".

    Examples: 

      1) ${StrTok} $0 "This is, or is not, just an example" " ," "4" "1"
                       (  ) ()  () () [_]  (  ) () (     )
                       0    1   2  3  4    5    6  7 
         $0 = "not"

      2) ${StrTok} $0 "This is, or is not, just an example" " ," "4" "0"
                       (  ) () ^() [] ( ) ^(  ) () (     )
                       0    1  23  4  5   67    8  9
         $0 = "is"

    =========================================================================
    *                                                      ${StrTrimNewLines}
    -------------------------------------------------------------------------
    ResultVar String
    =========================================================================
    Deletes unnecessary new lines at end of "String".

    Parameters:

      ResultVar
      Destination where result is returned.

      String
      String where to search unnecessary new lines at end of "String".

    Result Value -> ResultVar:

      "String" with unnecessary end new lines removed.

    Example:

      ${StrTrimNewLines} $0 "$\r$\nThis is just an example$\r$\n$\r$\n"
                            [_____________________________(_)(_)(_)(_)]

      $0 = "$\r$\nThis is just an example"

Functions included and not included
--------------------------------------------------

11 functions have been included
  They are not available on Archive
  They are on LogicLib format

15 functions have not been included
  12 were not included because of better functions
    6 were not included because of AdvStrTok (called here as StrTok)
      First String Part Function
      Save on Variables Function
      Sort Strings (1, 2 and 3) Functions
      StrTok Function
    2 were not included because of StrCase
      StrLower Function
      StrUpper Function
    2 were not included because of StrClb
      StrClbSet Function
      StrClbGet Function
    1 was not included because of NSISToIO and IOToNSIS
      Convert / to // in Paths Function
    1 was not included because of original String Replace Function (called
      here as StrRep)
      Another String Replace Function
  2 were not included because they aren't useful anymore
    Slash <-> Backslash Converter Function
    Trim Function
  1 was not included because of bugs
    Number to String Converter Function

Version History
---------------

1.09 - 10/22/2004

- Fixed stack problems involving: StrCase, StrRep, StrSort, StrTok.
- Fixed StrClb: When "Action" = "<>", handle was wrongly outputed as
  text.
- Fixed StrSort, StrStrAdv documentation examples.
- Fixed StrIOToNSIS, StrLoc, StrNSISToIO, StrRep, StrStr: sometimes
  didn't find "StrToSearch" at all.

1.08 - 10/12/2004

- Converted all the functions to LogicLib.
- StrSort: Totally remade and it can break old scripts. See
  documentation for details.
- StrTok: "ResultPart" has to start from 0 and it can break old scripts.
  See documentation for details.
- Added defines: StrFunc_List, *_List and *_TypeList.
- Fixed StrStrAdv: Variables $R0-$R3 couldn't be used on scripts before
  calling.
- StrRep: Cut down some variables.
- Arranged correctly the order of StrSort on the documentation.

1.07 - 09/21/2004

- Removed ${UnStrFunc} command. Now you can just include uninstall
  functions commands like ${UnStrStr} to be supported by uninstall functions
  and sections.
- Added case-sensitive comparation option for StrStrAdv.
- StrCase now uses System.dll which makes case conversions effective with
all latin letters (i.e. ê).
- Added switch case and original case for StrCase.
- StrClbSet and StrClbGet removed, added StrClb.
- Made compact the most usual operations inside the header file. File size
reduced.

1.06 - 03/26/2004

- StrNumToStr removed due to complex number handling on some languages.
- Fixed the bug where the old string was attached to string returned by
  StrCase when $R5 variable was used.

1.05 - 03/17/2004

- Fixed a bug with StrCase, Title Case wasn't working as should be.
- Fixed a bug with StrStrAdv, previous fix created another bug, string not
  returned correctly when using backwards search with "DisplayStrToSearch" as
  "0".

1.04 - 03/07/2004

- Added new StrCase, removed StrLower and StrUpper.
- Organized by name commands inside header and readme files.

1.03 - 02/12/2004

- Added commands support for uninstall sections and functions.
- Fixed variables switch in "StrLoc" and "StrTok" after using these.

1.02 - 02/07/2004

- Fixed StrLoc.
- Fixed Documentation about StrLoc. "Direction" is really "OffsetDirection".
- Added my new AdvStrSort, and removed the old one.

1.01 - 02/05/2004

- Fixed Documentation about StrSort and StrTok.
- Fixed StrTok default value for the string part. Now it's "L".
- Fixed StrStrAdv fixed wrong search when had a combination of same
  substrings one after another in a string.
- Fixed StrLoc: when a string isn't found, don't return any value at all.

1.00 - 02/01/2004

- Added documentation.
- Renamed header file to "StrFunc.nsh".
- Added 1 function, StrLoc.
- Modified StrStrAdv, removed some lines.
- Fixed StrTok, 2 simple numbers made it loop everytime.
- Fixed some small issues on the header file.

0.02 - 01/24/2004

- Completed StrFunc.nsh file. Need some tests and the readme.

0.01 - 01/22/2004

- First version to test ideas...

Credits
-------

  Made by Diego Pedroso (aka deguix).

Functions Credits
-----------------

- All functions are made by Diego Pedroso on LogicLib format. They
  are based on functions by Amir Szekely, Dave Laundon, Hendri
  Adriaens, Nik Medved, Joost Verburg, Stuart Welch, Ximon Eighteen,
  "bigmac666" and "bluenet". "bluenet"'s version of StrIOToNSIS and
  StrNSISToIO on LogicLib format were included.

License
-------

This header file is provided 'as-is', without any express or implied
warranty. In no event will the author be held liable for any damages
arising from the use of this header file.

Permission is granted to anyone to use this header file for any purpose,
including commercial applications, and to alter it and redistribute
it freely, subject to the following restrictions:

1. The origin of this header file must not be misrepresented;
   you must not claim that you wrote the original header file.
   If you use this header file in a product, an acknowledgment in the
   product documentation would be appreciated but is not required.
2. Altered versions must be plainly marked as such,
   and must not be misrepresented as being the original header file.
3. This notice may not be removed or altered from any distribution.