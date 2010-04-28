; viewhtml.nsi
;
; This script creates a silent installer which extracts one (or more) HTML
; files to a temporary directory, opens Internet Explorer to view the file(s),
; and when Internet Explorer has quit, deletes the file(s).

;--------------------------------

; The name of the installer (not really used in a silent install)
Name "ViewHTML"

; Set to silent mode
SilentInstall silent

; The file to write
OutFile "viewhtml.exe"

; Request application privileges for Windows Vista
RequestExecutionLevel user

;--------------------------------

; The stuff to install
Section ""

  ; Get a temporary filename (in the Windows Temp directory)
  GetTempFileName $R0
  
  ; Extract file
  ; Lets skip this one, it's not built to be showin in IE
  ; File /oname=$R0 "..\Menu\compiler.html"
  ; and write our own! :)
  FileOpen $0 $R0 "w"
  FileWrite $0 "<HTML><BODY><H1>HTML page for viewhtml.nsi</H1></BODY></HTML>"
  FileClose $0
  
  ; View file
  ExecWait '"$PROGRAMFILES\Internet Explorer\iexplore.exe" "$R0"'

  ; Note: another way of doing this would be to use ExecShell, but then you
  ; really couldn't get away with deleting the files. Here is the ExecShell
  ; line that you would want to use:
  ;
  ; ExecShell "open" '"$R0"'
  ;
  ; The advantage of this way is that it would use the default browser to
  ; open the HTML.
  ;
  
  ; Delete the files (on reboot if file is in use)
  Delete /REBOOTOK $R0

SectionEnd