; VersionInfo.nsi
;
; This script shows you how to add version information to an installer.
; Windows shows this information on the Version tab of the File properties.

;--------------------------------

Name "Version Info"

OutFile "VersionInfo.exe"

LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"
;--------------------------------
;Version Information

  VIProductVersion "1.2.3.4"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "Test Application"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "A test comment"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Fake company"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalTrademarks" "Test Application is a trademark of Fake company"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright Fake company"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "Test Application"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "1.2.3"

;--------------------------------

Section ""

SectionEnd
