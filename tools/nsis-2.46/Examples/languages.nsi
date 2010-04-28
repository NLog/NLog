; languages.nsi
;
; This is an example of a multilingual installer
; The user can select the language on startup

;--------------------------------

OutFile languages.exe

XPStyle on

RequestExecutionLevel user

;--------------------------------

Page license
Page components
Page instfiles

;--------------------------------

; First is default
LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\Dutch.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\French.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\German.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\Korean.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\Russian.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\Spanish.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\Swedish.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\TradChinese.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\SimpChinese.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\Slovak.nlf"

; License data
; Not exactly translated, but it shows what's needed
LicenseLangString myLicenseData ${LANG_ENGLISH} "bigtest.nsi"
LicenseLangString myLicenseData ${LANG_DUTCH} "waplugin.nsi"
LicenseLangString myLicenseData ${LANG_FRENCH} "example1.nsi"
LicenseLangString myLicenseData ${LANG_GERMAN} "example2.nsi"
LicenseLangString myLicenseData ${LANG_KOREAN} "gfx.nsi"
LicenseLangString myLicenseData ${LANG_RUSSIAN} "languages.nsi"
LicenseLangString myLicenseData ${LANG_SPANISH} "LogicLib.nsi"
LicenseLangString myLicenseData ${LANG_SWEDISH} "makensis.nsi"
LicenseLangString myLicenseData ${LANG_TRADCHINESE} "one-section.nsi"
LicenseLangString myLicenseData ${LANG_SIMPCHINESE} "primes.nsi"
LicenseLangString myLicenseData ${LANG_SLOVAK} "silent.nsi"

LicenseData $(myLicenseData)

; Set name using the normal interface (Name command)
LangString Name ${LANG_ENGLISH} "English"
LangString Name ${LANG_DUTCH} "Dutch"
LangString Name ${LANG_FRENCH} "French"
LangString Name ${LANG_GERMAN} "German"
LangString Name ${LANG_KOREAN} "Korean"
LangString Name ${LANG_RUSSIAN} "Russian"
LangString Name ${LANG_SPANISH} "Spanish"
LangString Name ${LANG_SWEDISH} "Swedish"
LangString Name ${LANG_TRADCHINESE} "Traditional Chinese"
LangString Name ${LANG_SIMPCHINESE} "Simplified Chinese"
LangString Name ${LANG_SLOVAK} "Slovak"

Name $(Name)

; Directly change the inner lang strings (Same as ComponentText)
LangString ^ComponentsText ${LANG_ENGLISH} "English component page"
LangString ^ComponentsText ${LANG_DUTCH} "Dutch component page"
LangString ^ComponentsText ${LANG_FRENCH} "French component page"
LangString ^ComponentsText ${LANG_GERMAN} "German component page"
LangString ^ComponentsText ${LANG_KOREAN} "Korean component page"
LangString ^ComponentsText ${LANG_RUSSIAN} "Russian component page"
LangString ^ComponentsText ${LANG_SPANISH} "Spanish component page"
LangString ^ComponentsText ${LANG_SWEDISH} "Swedish component page"
LangString ^ComponentsText ${LANG_TRADCHINESE} "Traditional Chinese component page"
LangString ^ComponentsText ${LANG_SIMPCHINESE} "Simplified Chinese component page"
LangString ^ComponentsText ${LANG_SLOVAK} "Slovak component page"

; Set one text for all languages (simply don't use a LangString)
CompletedText "Languages example completed"

; A LangString for the section name
LangString Sec1Name ${LANG_ENGLISH} "English section #1"
LangString Sec1Name ${LANG_DUTCH} "Dutch section #1"
LangString Sec1Name ${LANG_FRENCH} "French section #1"
LangString Sec1Name ${LANG_GERMAN} "German section #1"
LangString Sec1Name ${LANG_KOREAN} "Korean section #1"
LangString Sec1Name ${LANG_RUSSIAN} "Russian section #1"
LangString Sec1Name ${LANG_SPANISH} "Spanish section #1"
LangString Sec1Name ${LANG_SWEDISH} "Swedish section #1"
LangString Sec1Name ${LANG_TRADCHINESE} "Trandional Chinese section #1"
LangString Sec1Name ${LANG_SIMPCHINESE} "Simplified Chinese section #1"
LangString Sec1Name ${LANG_SLOVAK} "Slovak section #1"

; A multilingual message
LangString Message ${LANG_ENGLISH} "English message"
LangString Message ${LANG_DUTCH} "Dutch message"
LangString Message ${LANG_FRENCH} "French message"
LangString Message ${LANG_GERMAN} "German message"
LangString Message ${LANG_KOREAN} "Korean message"
LangString Message ${LANG_RUSSIAN} "Russian message"
LangString Message ${LANG_SPANISH} "Spanish message"
LangString Message ${LANG_SWEDISH} "Swedish message"
LangString Message ${LANG_TRADCHINESE} "Trandional Chinese message"
LangString Message ${LANG_SIMPCHINESE} "Simplified Chinese message"
LangString Message ${LANG_SLOVAK} "Slovak message"

;--------------------------------

;Section names set by Language strings
;It works with ! too
Section !$(Sec1Name) sec1
	MessageBox MB_OK $(Message)
SectionEnd

; The old, slow, wasteful way
; Look at this section and see why LangString is so much easier
Section "Section number two"
	StrCmp $LANGUAGE ${LANG_ENGLISH} 0 +2
		MessageBox MB_OK "Installing English stuff"
	StrCmp $LANGUAGE ${LANG_DUTCH} 0 +2
		MessageBox MB_OK "Installing Dutch stuff"
	StrCmp $LANGUAGE ${LANG_FRENCH} 0 +2
		MessageBox MB_OK "Installing French stuff"
	StrCmp $LANGUAGE ${LANG_GERMAN} 0 +2
		MessageBox MB_OK "Installing German stuff"
	StrCmp $LANGUAGE ${LANG_KOREAN} 0 +2
		MessageBox MB_OK "Installing Korean stuff"
	StrCmp $LANGUAGE ${LANG_RUSSIAN} 0 +2
		MessageBox MB_OK "Installing Russian stuff"
	StrCmp $LANGUAGE ${LANG_SPANISH} 0 +2
		MessageBox MB_OK "Installing Spanish stuff"
	StrCmp $LANGUAGE ${LANG_SWEDISH} 0 +2
		MessageBox MB_OK "Installing Swedish stuff"
	StrCmp $LANGUAGE ${LANG_TRADCHINESE} 0 +2
		MessageBox MB_OK "Installing Traditional Chinese stuff"
	StrCmp $LANGUAGE ${LANG_SIMPCHINESE} 0 +2
		MessageBox MB_OK "Installing Simplified Chinese stuff"
	StrCmp $LANGUAGE ${LANG_SLOVAK} 0 +2
		MessageBox MB_OK "Installing Slovak stuff"
SectionEnd

;--------------------------------

Function .onInit

	;Language selection dialog

	Push ""
	Push ${LANG_ENGLISH}
	Push English
	Push ${LANG_DUTCH}
	Push Dutch
	Push ${LANG_FRENCH}
	Push French
	Push ${LANG_GERMAN}
	Push German
	Push ${LANG_KOREAN}
	Push Korean
	Push ${LANG_RUSSIAN}
	Push Russian
	Push ${LANG_SPANISH}
	Push Spanish
	Push ${LANG_SWEDISH}
	Push Swedish
	Push ${LANG_TRADCHINESE}
	Push "Traditional Chinese"
	Push ${LANG_SIMPCHINESE}
	Push "Simplified Chinese"
	Push ${LANG_SLOVAK}
	Push Slovak
	Push A ; A means auto count languages
	       ; for the auto count to work the first empty push (Push "") must remain
	LangDLL::LangDialog "Installer Language" "Please select the language of the installer"

	Pop $LANGUAGE
	StrCmp $LANGUAGE "cancel" 0 +2
		Abort
FunctionEnd