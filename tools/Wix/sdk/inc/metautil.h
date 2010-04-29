#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="metautil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    IIS Metabase helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#include <iadmw.h>
#include <iiscnfg.h>
#include <iwamreg.h>
#include <mddefw.h>

#ifdef __cplusplus
extern "C" {
#endif

// structs

// prototypes
HRESULT DAPI MetaFindWebBase(
    __in IMSAdminBaseW* piMetabase, 
    __in_z LPCWSTR wzIP, 
    __in int iPort, 
    __in_z LPCWSTR wzHeader,
    __in BOOL fSecure,
    __out_ecount(cchWebBase) LPWSTR wzWebBase, 
    __in DWORD cchWebBase
    );
HRESULT DAPI MetaFindFreeWebBase(
    __in IMSAdminBaseW* piMetabase, 
    __out_ecount(cchWebBase) LPWSTR wzWebBase, 
    __in DWORD cchWebBase
    );

HRESULT DAPI MetaOpenKey(
    __in IMSAdminBaseW* piMetabase, 
    __in METADATA_HANDLE mhKey, 
    __in_z LPCWSTR wzKey,
    __in DWORD dwAccess,
    __in DWORD cRetries,
    __out METADATA_HANDLE* pmh
    );
HRESULT DAPI MetaGetValue(
    __in IMSAdminBaseW* piMetabase, 
    __in METADATA_HANDLE mhKey, 
    __in_z LPCWSTR wzKey, 
    __inout METADATA_RECORD* pmr
    );
void DAPI MetaFreeValue(
    __in METADATA_RECORD* pmr
    );

#ifdef __cplusplus
}
#endif
