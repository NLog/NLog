#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="wiutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Header for Windows Installer helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

#define MAX_DARWIN_KEY 73
#define MAX_DARWIN_COLUMN 255

#define ReleaseMsi(h) if (h) { ::MsiCloseHandle(h); }
#define ReleaseNullMsi(h) if (h) { ::MsiCloseHandle(h); h = NULL; }

HRESULT DAPI WiuGetComponentPath(
    __in_z LPCWSTR wzProductCode,
    __in_z LPCWSTR wzComponentId,
    __out LPWSTR* ppwzPath
    );

HRESULT DAPI WiuGetProductInfo(
    __in_z LPCWSTR wzProductCode,
    __in_z LPCWSTR wzProperty,
    __out LPWSTR* ppwzValue
    );

HRESULT DAPI WiuGetProductProperty(
    __in MSIHANDLE hProduct,
    __in_z LPCWSTR wzProperty,
    __out LPWSTR* ppwzValue
    );

#ifdef __cplusplus
}
#endif
