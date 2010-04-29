#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="osutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Operating system helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

enum OS_VERSION
{
    OS_VERSION_UNKNOWN,
    OS_VERSION_WINNT,
    OS_VERSION_WIN2000,
    OS_VERSION_WINXP,
    OS_VERSION_WIN2003,
    OS_VERSION_VISTA,
    OS_VERSION_WIN2008,
    OS_VERSION_WIN7,
    OS_VERSION_WIN2008_R2,
    OS_VERSION_FUTURE
};

void DAPI OsGetVersion(
    __out OS_VERSION* pVersion,
    __out DWORD* pdwServicePack
    );
HRESULT OsIsRunningPrivileged(
    __out BOOL* pfPrivileged
    );

#ifdef __cplusplus
}
#endif
