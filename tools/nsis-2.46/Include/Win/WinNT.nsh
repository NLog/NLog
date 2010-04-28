!ifndef __WIN_WINNT__INC
!define __WIN_WINNT__INC
!verbose push
!verbose 3
!ifndef __WIN_NOINC_WINNT


#define MINCHAR  0x80        
#define MAXCHAR  0x7f        
!define MINSHORT 0x8000      
!define MAXSHORT 0x7fff      
!define MINLONG  0x80000000  
!define MAXLONG  0x7fffffff  
!define MAXBYTE  0xff        
!define MAXWORD  0xffff      
!define MAXDWORD 0xffffffff  

!ifndef WIN32_NO_STATUS 
!define STATUS_WAIT_0                    0x00000000
!define STATUS_ABANDONED_WAIT_0          0x00000080
!define STATUS_USER_APC                  0x000000C0
!define STATUS_TIMEOUT                   0x00000102
!define STATUS_PENDING                   0x00000103
!define DBG_EXCEPTION_HANDLED            0x00010001
!define DBG_CONTINUE                     0x00010002
!define STATUS_SEGMENT_NOTIFICATION      0x40000005
!define DBG_TERMINATE_THREAD             0x40010003
!define DBG_TERMINATE_PROCESS            0x40010004
!define DBG_CONTROL_C                    0x40010005
!define DBG_CONTROL_BREAK                0x40010008
!define DBG_COMMAND_EXCEPTION            0x40010009
!define STATUS_GUARD_PAGE_VIOLATION      0x80000001
!define STATUS_DATATYPE_MISALIGNMENT     0x80000002
!define STATUS_BREAKPOINT                0x80000003
!define STATUS_SINGLE_STEP               0x80000004
!define DBG_EXCEPTION_NOT_HANDLED        0x80010001
!define STATUS_ACCESS_VIOLATION          0xC0000005
!define STATUS_IN_PAGE_ERROR             0xC0000006
!define STATUS_INVALID_HANDLE            0xC0000008
!define STATUS_NO_MEMORY                 0xC0000017
!define STATUS_ILLEGAL_INSTRUCTION       0xC000001D
!define STATUS_NONCONTINUABLE_EXCEPTION  0xC0000025
!define STATUS_INVALID_DISPOSITION       0xC0000026
!define STATUS_ARRAY_BOUNDS_EXCEEDED     0xC000008C
!define STATUS_FLOAT_DENORMAL_OPERAND    0xC000008D
!define STATUS_FLOAT_DIVIDE_BY_ZERO      0xC000008E
!define STATUS_FLOAT_INEXACT_RESULT      0xC000008F
!define STATUS_FLOAT_INVALID_OPERATION   0xC0000090
!define STATUS_FLOAT_OVERFLOW            0xC0000091
!define STATUS_FLOAT_STACK_CHECK         0xC0000092
!define STATUS_FLOAT_UNDERFLOW           0xC0000093
!define STATUS_INTEGER_DIVIDE_BY_ZERO    0xC0000094
!define STATUS_INTEGER_OVERFLOW          0xC0000095
!define STATUS_PRIVILEGED_INSTRUCTION    0xC0000096
!define STATUS_STACK_OVERFLOW            0xC00000FD
!define STATUS_CONTROL_C_EXIT            0xC000013A
!define STATUS_FLOAT_MULTIPLE_FAULTS     0xC00002B4
!define STATUS_FLOAT_MULTIPLE_TRAPS      0xC00002B5
!define STATUS_REG_NAT_CONSUMPTION       0xC00002C9
!define STATUS_SXS_EARLY_DEACTIVATION    0xC015000F
!define STATUS_SXS_INVALID_DEACTIVATION  0xC0150010
!endif /*WIN32_NO_STATUS*/

#define MAXIMUM_WAIT_OBJECTS 64  

!define DELETE                   0x00010000
!define READ_CONTROL             0x00020000
!define WRITE_DAC                0x00040000
!define WRITE_OWNER              0x00080000
!define SYNCHRONIZE              0x00100000
!define STANDARD_RIGHTS_REQUIRED 0x000F0000
!define STANDARD_RIGHTS_READ     ${READ_CONTROL}
!define STANDARD_RIGHTS_WRITE    ${READ_CONTROL}
!define STANDARD_RIGHTS_EXECUTE  ${READ_CONTROL}
!define STANDARD_RIGHTS_ALL      0x001F0000
!define SPECIFIC_RIGHTS_ALL      0x0000FFFF
!define ACCESS_SYSTEM_SECURITY   0x01000000
!define MAXIMUM_ALLOWED          0x02000000
!define GENERIC_READ             0x80000000
!define GENERIC_WRITE            0x40000000
!define GENERIC_EXECUTE          0x20000000
!define GENERIC_ALL              0x10000000

!define SE_PRIVILEGE_ENABLED_BY_DEFAULT 0x00000001
!define SE_PRIVILEGE_ENABLED            0x00000002
!define SE_PRIVILEGE_REMOVED            0x00000004
!define SE_PRIVILEGE_USED_FOR_ACCESS    0x80000000

!define SE_CREATE_TOKEN_NAME        "SeCreateTokenPrivilege"
!define SE_ASSIGNPRIMARYTOKEN_NAME  "SeAssignPrimaryTokenPrivilege"
!define SE_LOCK_MEMORY_NAME         "SeLockMemoryPrivilege"
!define SE_INCREASE_QUOTA_NAME      "SeIncreaseQuotaPrivilege"
!define SE_UNSOLICITED_INPUT_NAME   "SeUnsolicitedInputPrivilege"
!define SE_MACHINE_ACCOUNT_NAME     "SeMachineAccountPrivilege"
!define SE_TCB_NAME                 "SeTcbPrivilege"
!define SE_SECURITY_NAME            "SeSecurityPrivilege"
!define SE_TAKE_OWNERSHIP_NAME      "SeTakeOwnershipPrivilege"
!define SE_LOAD_DRIVER_NAME         "SeLoadDriverPrivilege"
!define SE_SYSTEM_PROFILE_NAME      "SeSystemProfilePrivilege"
!define SE_SYSTEMTIME_NAME          "SeSystemtimePrivilege"
!define SE_PROF_SINGLE_PROCESS_NAME "SeProfileSingleProcessPrivilege"
!define SE_INC_BASE_PRIORITY_NAME   "SeIncreaseBasePriorityPrivilege"
!define SE_CREATE_PAGEFILE_NAME     "SeCreatePagefilePrivilege"
!define SE_CREATE_PERMANENT_NAME    "SeCreatePermanentPrivilege"
!define SE_BACKUP_NAME              "SeBackupPrivilege"
!define SE_RESTORE_NAME             "SeRestorePrivilege"
!define SE_SHUTDOWN_NAME            "SeShutdownPrivilege"
!define SE_DEBUG_NAME               "SeDebugPrivilege"
!define SE_AUDIT_NAME               "SeAuditPrivilege"
!define SE_SYSTEM_ENVIRONMENT_NAME  "SeSystemEnvironmentPrivilege"
!define SE_CHANGE_NOTIFY_NAME       "SeChangeNotifyPrivilege"
!define SE_REMOTE_SHUTDOWN_NAME     "SeRemoteShutdownPrivilege"
!define SE_UNDOCK_NAME              "SeUndockPrivilege"
!define SE_SYNC_AGENT_NAME          "SeSyncAgentPrivilege"
!define SE_ENABLE_DELEGATION_NAME   "SeEnableDelegationPrivilege"
!define SE_MANAGE_VOLUME_NAME       "SeManageVolumePrivilege"
!define SE_IMPERSONATE_NAME         "SeImpersonatePrivilege"
!define SE_CREATE_GLOBAL_NAME       "SeCreateGlobalPrivilege"

!define TOKEN_ASSIGN_PRIMARY    0x0001
!define TOKEN_DUPLICATE         0x0002
!define TOKEN_IMPERSONATE       0x0004
!define TOKEN_QUERY             0x0008
!define TOKEN_QUERY_SOURCE      0x0010
!define TOKEN_ADJUST_PRIVILEGES 0x0020
!define TOKEN_ADJUST_GROUPS     0x0040
!define TOKEN_ADJUST_DEFAULT    0x0080
!define TOKEN_ADJUST_SESSIONID  0x0100
!define TOKEN_ALL_ACCESS_P     0xF00FF
!define /math TOKEN_ALL_ACCESS  ${TOKEN_ALL_ACCESS_P} | ${TOKEN_ADJUST_SESSIONID}
!define /math TOKEN_READ        ${STANDARD_RIGHTS_READ} | ${TOKEN_QUERY}
!define TOKEN_WRITE      0x200E0 ;(STANDARD_RIGHTS_WRITE|TOKEN_ADJUST_PRIVILEGES|TOKEN_ADJUST_GROUPS|TOKEN_ADJUST_DEFAULT)
!define TOKEN_EXECUTE    ${STANDARD_RIGHTS_EXECUTE}

!define PROCESS_TERMINATE         0x0001  
!define PROCESS_CREATE_THREAD     0x0002  
!define PROCESS_SET_SESSIONID     0x0004  
!define PROCESS_VM_OPERATION      0x0008  
!define PROCESS_VM_READ           0x0010  
!define PROCESS_VM_WRITE          0x0020  
!define PROCESS_DUP_HANDLE        0x0040  
!define PROCESS_CREATE_PROCESS    0x0080  
!define PROCESS_SET_QUOTA         0x0100  
!define PROCESS_SET_INFORMATION   0x0200  
!define PROCESS_QUERY_INFORMATION 0x0400  
!define PROCESS_SUSPEND_RESUME    0x0800  
!define PROCESS_ALL_ACCESS      0x1F0FFF ;(STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF)
!define THREAD_TERMINATE               0x0001  
!define THREAD_SUSPEND_RESUME          0x0002  
!define THREAD_GET_CONTEXT             0x0008  
!define THREAD_SET_CONTEXT             0x0010  
!define THREAD_SET_INFORMATION         0x0020  
!define THREAD_QUERY_INFORMATION       0x0040  
!define THREAD_SET_THREAD_TOKEN        0x0080
!define THREAD_IMPERSONATE             0x0100
!define THREAD_DIRECT_IMPERSONATION    0x0200
!define THREAD_ALL_ACCESS            0x1F03FF ;(STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x3FF)
!define JOB_OBJECT_ASSIGN_PROCESS           0x0001
!define JOB_OBJECT_SET_ATTRIBUTES           0x0002
!define JOB_OBJECT_QUERY                    0x0004
!define JOB_OBJECT_TERMINATE                0x0008
!define JOB_OBJECT_SET_SECURITY_ATTRIBUTES  0x0010
!define JOB_OBJECT_ALL_ACCESS             0x1F001F ;(STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1F )
!define EVENT_MODIFY_STATE      0x0002  
!define EVENT_ALL_ACCESS 0x1F0003 ;(STANDARD_RIGHTS_REQUIRED|SYNCHRONIZE|0x3) 
!define MUTANT_QUERY_STATE      0x0001
!define MUTANT_ALL_ACCESS 0x1F0001 ;(STANDARD_RIGHTS_REQUIRED|SYNCHRONIZE|MUTANT_QUERY_STATE)

!define FILE_SHARE_READ   0x00000001  
!define FILE_SHARE_WRITE  0x00000002  
!define FILE_SHARE_DELETE 0x00000004  
!define FILE_ATTRIBUTE_READONLY             0x00000001  
!define FILE_ATTRIBUTE_HIDDEN               0x00000002  
!define FILE_ATTRIBUTE_SYSTEM               0x00000004  
!define FILE_ATTRIBUTE_DIRECTORY            0x00000010  
!define FILE_ATTRIBUTE_ARCHIVE              0x00000020  
!define FILE_ATTRIBUTE_DEVICE               0x00000040  
!define FILE_ATTRIBUTE_NORMAL               0x00000080  
!define FILE_ATTRIBUTE_TEMPORARY            0x00000100  
!define FILE_ATTRIBUTE_SPARSE_FILE          0x00000200  
!define FILE_ATTRIBUTE_REPARSE_POINT        0x00000400  
!define FILE_ATTRIBUTE_COMPRESSED           0x00000800  
!define FILE_ATTRIBUTE_OFFLINE              0x00001000  
!define FILE_ATTRIBUTE_NOT_CONTENT_INDEXED  0x00002000  
!define FILE_ATTRIBUTE_ENCRYPTED            0x00004000  

!define DUPLICATE_CLOSE_SOURCE 0x00000001  
!define DUPLICATE_SAME_ACCESS  0x00000002  

!define VER_PLATFORM_WIN32s             0
!define VER_PLATFORM_WIN32_WINDOWS      1
!define VER_PLATFORM_WIN32_NT           2

!ifndef REG_SZ & NSIS_WINDOWS__NO_REGTYPES
!define REG_NONE                 0
!define REG_SZ                   1
!define REG_EXPAND_SZ            2
!define REG_BINARY               3
!define REG_DWORD                4
!define REG_DWORD_LITTLE_ENDIAN  4
!define REG_DWORD_BIG_ENDIAN     5
!define REG_LINK                 6
!define REG_MULTI_SZ             7
!endif


!endif /* __WIN_NOINC_WINNT */
!verbose pop
!endif /* __WIN_WINNT__INC */