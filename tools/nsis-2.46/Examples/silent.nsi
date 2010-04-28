# This example shows how to handle silent installers.
# In short, you need IfSilent and the /SD switch for MessageBox to make your installer
# really silent when the /S switch is used.

Name "Silent"
OutFile "silent.exe"
RequestExecutionLevel user

# uncomment the following line to make the installer silent by default.
; SilentInstall silent

Function .onInit
  # `/SD IDYES' tells MessageBox to automatically choose IDYES if the installer is silent
  # in this case, the installer can only be silent if the user used the /S switch or if
  # you've uncommented line number 5
  MessageBox MB_YESNO|MB_ICONQUESTION "Would you like the installer to be silent from now on?" \
    /SD IDYES IDNO no IDYES yes

  # SetSilent can only be used in .onInit and doesn't work well along with `SetSilent silent'

  yes:
    SetSilent silent
    Goto done
  no:
    SetSilent normal
  done:
FunctionEnd

Section
  IfSilent 0 +2
    MessageBox MB_OK|MB_ICONINFORMATION 'This is a "silent" installer'

  # there is no need to use IfSilent for this one because the /SD switch takes care of that
  MessageBox MB_OK|MB_ICONINFORMATION "This is not a silent installer" /SD IDOK

  # when `SetOverwrite on' (which is the default) is used, the installer will show a message
  # if it can't open a file for writing. On silent installers, the ignore option will be
  # automatically selected. if `AllowSkipFiles off' (default is on) was used, there is no
  # ignore option and the cancel option will be automatically selected.

  # on is default
  ; AllowSkipFiles on

  # lock file
  FileOpen $0 $TEMP\silentOverwrite w
  # try to extract - will fail
  File /oname=$TEMP\silentOverwrite silent.nsi
  # unlcok
  FileClose $0

  # this will always show on silent installers because ignore is the option automatically
  # selected when a file can't be opened for writing on a silent installer
  MessageBox MB_OK|MB_ICONINFORMATION "This message box always shows if the installer is silent"

  AllowSkipFiles off

  # lock file
  FileOpen $0 $TEMP\silentOverwrite w
  # try to extract - will fail
  File /oname=$TEMP\silentOverwrite silent.nsi
  # unlcok
  FileClose $0
SectionEnd