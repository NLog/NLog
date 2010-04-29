//-------------------------------------------------------------------------------------------------
// <copyright file="dirutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Directory helper funtions.
// </summary>
//-------------------------------------------------------------------------------------------------

#pragma once

#ifdef __cplusplus
extern "C" {
#endif

BOOL DAPI DirExists(
    __in_z LPCWSTR wzPath, 
    __out_opt DWORD *pdwAttributes
    );

HRESULT DAPI DirCreateTempPath(
    __in_z LPCWSTR wzPrefix,
    __out_ecount_z(cchPath) LPWSTR wzPath,
    __in DWORD cchPath
    );

HRESULT DAPI DirEnsureExists(
    __in_z LPCWSTR wzPath, 
    __in_opt LPSECURITY_ATTRIBUTES psa
    );

HRESULT DAPI DirEnsureDelete(
    __in_z LPCWSTR wzPath,
    __in BOOL fDeleteFiles,
    __in BOOL fRecurse
    );

HRESULT DAPI DirGetCurrent(
    __deref_out_z LPWSTR* psczCurrentDirectory
    );

HRESULT DAPI DirSetCurrent(
    __in_z LPCWSTR wzDirectory
    );

#ifdef __cplusplus
}
#endif

