#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="procutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Header for proces helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

HRESULT DAPI ProcExecute(
    __in_z LPWSTR wzCommand,
    __out HANDLE *phProcess,
    __out_opt HANDLE *phChildStdIn,
    __out_opt HANDLE *phChildStdOutErr
    );
HRESULT DAPI ProcWaitForCompletion(
    __in HANDLE hProcess,
    __in DWORD dwTimeout,
    __out DWORD *pReturnCode
    );
HRESULT DAPI ProcWaitForIds(
    __in_ecount(cProcessIds) const DWORD* pdwProcessIds,
    __in DWORD cProcessIds,
    __in DWORD dwMilliseconds
    );
HRESULT DAPI ProcCloseIds(
    __in_ecount(cProcessIds) const DWORD* pdwProcessIds,
    __in DWORD cProcessIds
    );

// following code in proc2utl.cpp due to dependency on PSAPI.DLL.
HRESULT DAPI ProcFindAllIdsFromExeName(
    __in_z LPCWSTR wzExeName,
    __out DWORD** ppdwProcessIds,
    __out DWORD* pcProcessIds
    );

#ifdef __cplusplus
}
#endif
