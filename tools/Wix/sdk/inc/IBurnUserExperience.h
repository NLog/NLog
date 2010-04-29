//-------------------------------------------------------------------------------------------------
// <copyright file="IBurnUserExperience.h" company="Microsoft">
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
//      IBurnUserExperience, implemented by Burn UX and used by Burn engine/core.
// </summary>
//-------------------------------------------------------------------------------------------------

#pragma once


enum BURN_DISPLAY
{
    BURN_DISPLAY_UNKNOWN,
    BURN_DISPLAY_NONE,
    BURN_DISPLAY_PASSIVE,
    BURN_DISPLAY_FULL,
};


enum BURN_RESTART
{
    BURN_RESTART_UNKNOWN,
    BURN_RESTART_NEVER,
    BURN_RESTART_PROMPT,
    BURN_RESTART_AUTOMATIC,
    BURN_RESTART_ALWAYS,
};


enum BURN_RESUME_TYPE
{
    BURN_RESUME_TYPE_NONE,
    BURN_RESUME_TYPE_INVALID,        // resume information is present but invalid
    BURN_RESUME_TYPE_UNEXPECTED,     // relaunched after an unexpected interruption
    BURN_RESUME_TYPE_REBOOT_PENDING, // reboot has not taken place yet
    BURN_RESUME_TYPE_REBOOT,         // relaunched after reboot
    BURN_RESUME_TYPE_SUSPEND,        // relaunched after suspend
    BURN_RESUME_TYPE_ARP,            // launched from ARP
};


struct BURN_COMMAND
{
    BURN_ACTION action;
    BURN_DISPLAY display;
    BURN_RESTART restart;

    BOOL fResumed;
};


DECLARE_INTERFACE_IID_(IBurnUserExperience, IUnknown, "53C31D56-49C0-426B-AB06-099D717C67FE")
{
    STDMETHOD(Initialize)(
        __in IBurnCore* pCore,
        __in int nCmdShow,
        __in BURN_RESUME_TYPE resumeType
        ) = 0;

    STDMETHOD_(void, Uninitialize)() = 0;

    STDMETHOD_(int, OnDetectBegin)(
        __in DWORD cPackages
        ) = 0;

    STDMETHOD_(int, OnDetectPriorBundle)(
        __in_z LPCWSTR wzBundleId
        ) = 0;

    STDMETHOD_(int, OnDetectPackageBegin)(
        __in_z LPCWSTR wzPackageId
        ) = 0;

    STDMETHOD_(void, OnDetectPackageComplete)(
        __in LPCWSTR wzPackageId,
        __in HRESULT hrStatus,
        __in PACKAGE_STATE state
        ) = 0;

    STDMETHOD_(void, OnDetectComplete)(
        __in HRESULT hrStatus
        ) = 0;

    STDMETHOD_(int, OnPlanBegin)(
        __in DWORD cPackages
        ) = 0;

    STDMETHOD_(int, OnPlanPriorBundle)(
        __in_z LPCWSTR wzBundleId,
        __inout_z REQUEST_STATE* pRequestedState
        ) = 0;

    STDMETHOD_(int, OnPlanPackageBegin)(
        __in_z LPCWSTR wzPackageId,
        __inout_z REQUEST_STATE* pRequestedState
        ) = 0;

    STDMETHOD_(void, OnPlanPackageComplete)(
        __in LPCWSTR wzPackageId,
        __in HRESULT hrStatus,
        __in PACKAGE_STATE state,
        __in REQUEST_STATE requested,
        __in ACTION_STATE execute,
        __in ACTION_STATE rollback
        ) = 0;

    STDMETHOD_(void, OnPlanComplete)(
        __in HRESULT hrStatus
        ) = 0;

    STDMETHOD_(int, OnApplyBegin)() = 0;

    STDMETHOD_(int, OnRegisterBegin)() = 0;

    STDMETHOD_(void, OnRegisterComplete)(
        __in HRESULT hrStatus
        ) = 0;

    STDMETHOD_(void, OnUnregisterBegin)() = 0;

    STDMETHOD_(void, OnUnregisterComplete)(
        __in HRESULT hrStatus
        ) = 0;

    STDMETHOD_(int, OnCacheBegin)() = 0;

    STDMETHOD_(void, OnCacheComplete)(
        __in HRESULT hrStatus
        ) = 0;

    STDMETHOD_(int, OnExecuteBegin)(
        __in DWORD cExecutingPackages
        ) = 0;

    STDMETHOD_(int, OnExecutePackageBegin)(
        __in LPCWSTR wzPackageId,
        __in BOOL fExecute
        ) = 0;

    STDMETHOD_(int, OnError)(
        __in LPCWSTR wzPackageId,
        __in DWORD dwCode,
        __in_z LPCWSTR wzError,
        __in DWORD dwUIHint
        ) = 0;

    STDMETHOD_(int, OnProgress)(
        __in DWORD dwProgressPercentage,
        __in DWORD dwOverallPercentage
        ) = 0;

    STDMETHOD_(int, OnExecuteMsiMessage)(
        __in_z LPCWSTR wzPackageId,
        __in INSTALLMESSAGE mt,
        __in UINT uiFlags,
        __in_z LPCWSTR wzMessage
        ) = 0;

    STDMETHOD_(int, OnExecuteMsiFilesInUse)(
        __in_z LPCWSTR wzPackageId,
        __in DWORD cFiles,
        __in LPCWSTR* rgwzFiles
        ) = 0;

    STDMETHOD_(void, OnExecutePackageComplete)(
        __in LPCWSTR wzPackageId,
        __in HRESULT hrExitCode
        ) = 0;

    STDMETHOD_(void, OnExecuteComplete)(
        __in HRESULT hrStatus
        ) = 0;

    STDMETHOD_(BOOL, OnRestartRequired)() = 0;

    STDMETHOD_(void, OnApplyComplete)(
        __in HRESULT hrStatus
        ) = 0;

    STDMETHOD_(int, ResolveSource)(
        __in    LPCWSTR wzPackageId ,
        __in    LPCWSTR wzPackageOrContainerPath
        ) = 0;

    STDMETHOD_(BOOL, CanPackagesBeDownloaded)(void) = 0;

    STDMETHOD_(int, OnCachePackageBegin)(
        __in LPCWSTR wzPackageId,
        __in DWORD64 dw64PackageCacheSize
        )  = 0;

    STDMETHOD_(void, OnCachePackageComplete)(
        __in LPCWSTR wzPackageId,
        __in HRESULT hrStatus
        )  = 0;

    STDMETHOD_(int, OnDownloadPayloadBegin)(
        __in LPCWSTR wzPayloadId,
        __in LPCWSTR wzPayloadFileName
        )  = 0;

    STDMETHOD_(void, OnDownloadPayloadComplete)(
        __in LPCWSTR wzPayloadId,
        __in LPCWSTR wzPayloadFileName,
        __in HRESULT hrStatus
        )  = 0;

    STDMETHOD_(int, OnDownloadProgress)(
        __in DWORD dwProgressPercentage,
        __in DWORD dwOverallPercentage
        ) = 0;

    STDMETHOD_(int, OnExecuteProgress)(
        __in DWORD dwProgressPercentage,
        __in DWORD dwOverallPercentage
        ) = 0;

}; //struct IBurnUserExperience


extern "C" typedef HRESULT (WINAPI *PFN_CREATE_USER_EXPERIENCE)(
    __in BURN_COMMAND* pCommand,
    __out IBurnUserExperience** ppUX
    );
extern "C" typedef void (WINAPI *PFN_DESTROY_USER_EXPERIENCE)();
