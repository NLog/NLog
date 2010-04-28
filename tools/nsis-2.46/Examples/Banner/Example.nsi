# Look at Readme.txt for usage alongside with the Modern UI

!include "WinMessages.nsh"

Name "Banner.dll test"

OutFile "Banner Test.exe"

ShowInstDetails show

Function .onInit
	Banner::show "Calculating important stuff..."

	Banner::getWindow
	Pop $1

	again:
		IntOp $0 $0 + 1
		Sleep 1
		StrCmp $0 100 0 again

	GetDlgItem $2 $1 1030
	SendMessage $2 ${WM_SETTEXT} 0 "STR:Calculating more important stuff..."

	again2:
		IntOp $0 $0 + 1
		Sleep 1
		StrCmp $0 200 0 again2

	Banner::destroy
FunctionEnd

Section
	DetailPrint "Using previous calculations to quickly calculate 1*2000..."
	Sleep 1000
	DetailPrint "Eureka! It's $0!!!"
	DetailPrint ""
SectionEnd