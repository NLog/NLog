//-------------------------------------------------------------------------------------------------
// <copyright file="IBurnCore.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//    
//    The use and distribution terms for this software are covered by the
//    Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
//    which can be found in the file CPL.TXT at the root of this distribution.
//    By using this software in any fashion, you are agreeing to be bound by
//    the terms of this license.
//    
//    You must not remove this notice, or any other, from this software.
// </copyright>
// 
// <summary>
//      IBurnCore, implemented by engine(core) and used by Burn UX
// </summary>
//-------------------------------------------------------------------------------------------------

#pragma once

#define IDERROR -1
#define IDNOACTION 0

enum BURN_ACTION
{
    BURN_ACTION_UNKNOWN,
    BURN_ACTION_HELP,
    BURN_ACTION_UNINSTALL,
    BURN_ACTION_INSTALL,
    BURN_ACTION_MODIFY,
    BURN_ACTION_REPAIR,
};

enum ACTION_STATE
{
    ACTION_STATE_NONE,
    ACTION_STATE_UNINSTALL,
    ACTION_STATE_INSTALL,
    ACTION_STATE_ADMIN_INSTALL,
    ACTION_STATE_MAINTENANCE,
    ACTION_STATE_RECACHE,
    ACTION_STATE_MINOR_UPGRADE,
    ACTION_STATE_MAJOR_UPGRADE,
    ACTION_STATE_PATCH,
};

enum PACKAGE_STATE
{
    PACKAGE_STATE_UNKNOWN,
    PACKAGE_STATE_ABSENT,
    PACKAGE_STATE_CACHED,
    PACKAGE_STATE_PRESENT,
};

enum REQUEST_STATE
{
    REQUEST_STATE_NONE,
    REQUEST_STATE_ABSENT,
    REQUEST_STATE_CACHE,
    REQUEST_STATE_PRESENT,
    REQUEST_STATE_REPAIR,
};

enum BURN_LOG_LEVEL
{
    BURN_LOG_LEVEL_NONE,      // turns off report (only valid for XXXSetLevel())
    BURN_LOG_LEVEL_STANDARD,  // written if reporting is on
    BURN_LOG_LEVEL_VERBOSE,   // written only if verbose reporting is on
    BURN_LOG_LEVEL_DEBUG,     // reporting useful when debugging code
    BURN_LOG_LEVEL_ERROR,     // always gets reported, but can never be specified
};


DECLARE_INTERFACE_IID_(IBurnCore, IUnknown, "6480D616-27A0-44D7-905B-81512C29C2FB")
{
    STDMETHOD(GetPackageCount)(
        __out DWORD* pcPackages
        ) = 0;

    STDMETHOD(GetCommandLineParameters)(
        __out_ecount_opt(*pcchCommandLine) LPWSTR psczCommandLine,
        __inout DWORD* pcchCommandLine
        ) = 0;

    STDMETHOD(GetVariableNumeric)(
        __in_z LPCWSTR wzVariable,
        __out LONGLONG* pllValue
        ) = 0;

    STDMETHOD(GetVariableString)(
        __in_z LPCWSTR wzVariable,
        __out_ecount_opt(*pcchValue) LPWSTR wzValue,
        __inout DWORD* pcchValue
        ) = 0;

    STDMETHOD(GetVariableVersion)(
        __in_z LPCWSTR wzVariable,
        __out DWORD64* pqwValue
        ) = 0;

    STDMETHOD(OpenStore)(
        __in_z LPCWSTR wzBundleId
        ) = 0;

    STDMETHOD(GetPriorVariableNumeric)(
        __in_z LPCWSTR wzVariable,
        __out LONGLONG* pllValue
        ) = 0;

    STDMETHOD(GetPriorVariableString)(
        __in_z LPCWSTR wzVariable,
        __out_ecount_opt(*pcchValue) LPWSTR wzValue,
        __inout DWORD* pcchValue
        ) = 0;

    STDMETHOD(GetPriorVariableVersion)(
        __in_z LPCWSTR wzVariable,
        __out DWORD64* pqwValue
        ) = 0;

    STDMETHOD_(void, CloseStore)(
        ) = 0;

    STDMETHOD(SetVariableNumeric)(
        __in_z LPCWSTR wzVariable,
        __in LONGLONG llValue
        ) = 0;

    STDMETHOD(SetVariableString)(
        __in_z LPCWSTR wzVariable,
        __in_z LPCWSTR wzValue
        ) = 0;

    STDMETHOD(SetVariableVersion)(
        __in_z LPCWSTR wzVariable,
        __in DWORD64 qwValue
        ) = 0;

    STDMETHOD(FormatString)(
        __in_z LPCWSTR wzIn,
        __out_ecount_opt(*pcchOut) LPWSTR wzOut,
        __inout DWORD* pcchOut
        ) = 0;

    STDMETHOD(EscapeString)(
        __in_z LPCWSTR wzIn,
        __out_ecount_opt(*pcchOut) LPWSTR wzOut,
        __inout DWORD* pcchOut
        ) = 0;

    STDMETHOD(EvaluateCondition)(
        __in_z LPCWSTR wzCondition,
        __out BOOL* pf
        ) = 0;

    STDMETHOD(Log)(
        __in BURN_LOG_LEVEL level,
        __in_z LPCWSTR wzMessage
        ) = 0;

    STDMETHOD(Elevate)(
        __in_opt HWND hwndParent
        ) = 0;

    STDMETHOD(Detect)() = 0;

    STDMETHOD(Plan)(
        __in BURN_ACTION action
        ) = 0;

    STDMETHOD(Apply)(
        __in_opt HWND hwndParent
        ) = 0;

    STDMETHOD(Suspend)() = 0;

    STDMETHOD(Reboot)() = 0;

    STDMETHOD(Shutdown)(
        __in DWORD dwExitCode
        ) = 0;

    STDMETHOD(SetSource)(
        __in    LPCWSTR wzSourcePath
        ) = 0;
}; //struct IBurnCore
