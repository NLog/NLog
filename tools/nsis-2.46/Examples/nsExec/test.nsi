Name "nsExec Test"

OutFile "nsExec Test.exe"

ShowInstDetails show

Section "Silent MakeNSIS"
	nsExec::Exec '"${NSISDIR}\makensis.exe"'
	Pop $0 # return value/error/timeout
	DetailPrint ""
	DetailPrint "       Return value: $0"
	DetailPrint ""
SectionEnd

Section "MakeNSIS commands help"
	nsExec::ExecToLog '"${NSISDIR}\makensis.exe" /CMDHELP'
	Pop $0 # return value/error/timeout
	DetailPrint ""
	DetailPrint "       Return value: $0"
	DetailPrint ""
SectionEnd

Section "Output to variable"
	nsExec::ExecToStack '"${NSISDIR}\makensis.exe" /VERSION'
	Pop $0 # return value/error/timeout
	Pop $1 # printed text, up to ${NSIS_MAX_STRLEN}
	DetailPrint '"${NSISDIR}\makensis.exe" /VERSION printed: $1'
	DetailPrint ""
	DetailPrint "       Return value: $0"
	DetailPrint ""
SectionEnd