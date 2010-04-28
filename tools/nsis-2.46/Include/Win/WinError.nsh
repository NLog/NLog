!ifndef __WIN_WINERROR__INC
!define __WIN_WINERROR__INC
!verbose push
!verbose 3
!ifndef __WIN_NOINC_WINERROR

#define NO_ERROR 0
!define ERROR_SUCCESS                 0
!define ERROR_INVALID_FUNCTION        1    
!define ERROR_FILE_NOT_FOUND          2
!define ERROR_PATH_NOT_FOUND          3
!define ERROR_TOO_MANY_OPEN_FILES     4
!define ERROR_ACCESS_DENIED           5
!define ERROR_INVALID_HANDLE          6
!define ERROR_ARENA_TRASHED           7
!define ERROR_NOT_ENOUGH_MEMORY       8    
!define ERROR_INVALID_BLOCK           9
!define ERROR_BAD_ENVIRONMENT         10
!define ERROR_BAD_FORMAT              11
!define ERROR_INVALID_ACCESS          12
!define ERROR_INVALID_DATA            13
!define ERROR_OUTOFMEMORY             14
!define ERROR_INVALID_DRIVE           15
!define ERROR_CURRENT_DIRECTORY       16
!define ERROR_NOT_SAME_DEVICE         17
!define ERROR_NO_MORE_FILES           18
!define ERROR_WRITE_PROTECT           19
!define ERROR_BAD_UNIT                20
!define ERROR_NOT_READY               21
!define ERROR_BAD_COMMAND             22
!define ERROR_CRC                     23
!define ERROR_BAD_LENGTH              24
!define ERROR_SEEK                    25
!define ERROR_NOT_DOS_DISK            26
!define ERROR_SECTOR_NOT_FOUND        27
!define ERROR_OUT_OF_PAPER            28
!define ERROR_WRITE_FAULT             29
!define ERROR_READ_FAULT              30
!define ERROR_GEN_FAILURE             31
!define ERROR_SHARING_VIOLATION       32
!define ERROR_LOCK_VIOLATION          33
!define ERROR_WRONG_DISK              34
!define ERROR_SHARING_BUFFER_EXCEEDED 36
!define ERROR_HANDLE_EOF              38
!define ERROR_HANDLE_DISK_FULL        39
!define ERROR_NOT_SUPPORTED           50

!define SEVERITY_SUCCESS 0
!define SEVERITY_ERROR   1
!define E_UNEXPECTED   0x8000FFFF
!define E_NOTIMPL      0x80004001
!define E_OUTOFMEMORY  0x8007000E
!define E_INVALIDARG   0x80070057
!define E_NOINTERFACE  0x80004002
!define E_POINTER      0x80004003
!define E_HANDLE       0x80070006
!define E_ABORT        0x80004004
!define E_FAIL         0x80004005
!define E_ACCESSDENIED 0x80070005
!define E_PENDING      0x8000000A

!endif /* __WIN_NOINC_WINERROR */
!verbose pop
!endif /* __WIN_WINERROR__INC */