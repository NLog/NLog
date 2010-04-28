; primes.nsi
;
; This is an example of the possibities of the NSIS Script language.
; It calculates prime numbers.

;--------------------------------

Name "primes"
AllowRootDirInstall true
OutFile "primes.exe"
Caption "Prime number generator"
ShowInstDetails show
AllowRootDirInstall true
InstallDir "$EXEDIR"
RequestExecutionLevel user

DirText "Select a directory to write primes.txt. $_CLICK"

;--------------------------------

;Pages

Page directory
Page instfiles

;--------------------------------

Section ""
  SetOutPath $INSTDIR
  Call DoPrimes 
SectionEnd

;--------------------------------

Function DoPrimes

; we put this in here so it doesn't update the progress bar (faster)

!define PPOS $0 ; position in prime searching
!define PDIV $1 ; divisor
!define PMOD $2 ; the result of the modulus
!define PCNT $3 ; count of how many we've printed
  FileOpen $9 $INSTDIR\primes.txt w

  DetailPrint "2 is prime!"
  FileWrite $9 "2 is prime!$\r$\n"
  DetailPrint "3 is prime!"
  FileWrite $9 "3 is prime!$\r$\n"
  Strcpy ${PPOS} 3
  Strcpy ${PCNT} 2
outerloop:
   StrCpy ${PDIV} 3
   innerloop:
     IntOp ${PMOD} ${PPOS} % ${PDIV}
     IntCmp ${PMOD} 0 notprime
     IntOp ${PDIV} ${PDIV} + 2
     IntCmp ${PDIV} ${PPOS} 0 innerloop 0
       DetailPrint "${PPOS} is prime!"
       FileWrite $9 "${PPOS} is prime!$\r$\n"
       IntOp ${PCNT} ${PCNT} + 1
       IntCmp ${PCNT} 100 0 innerloop
       StrCpy ${PCNT} 0
       MessageBox MB_YESNO "Process more?" IDNO stop
     notprime:
       IntOp ${PPOS} ${PPOS} + 2
     Goto outerloop
   stop:
  FileClose $9
  
FunctionEnd