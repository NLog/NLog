#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="inetutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Internet utilites.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

#define ReleaseInternet(h) if (h) { ::InternetCloseHandle(h); h = NULL; }
#define ReleaseNullInternet(h) if (h) { ::InternetCloseHandle(h); h = NULL; }


// functions
HRESULT DAPI InternetGetSizeByHandle(
    __in HINTERNET hiFile,
    __out LONGLONG* pllSize
    );

HRESULT DAPI InternetGetCreateTimeByHandle(
    __in HINTERNET hiFile,
    __out LPFILETIME pft
    );

HRESULT DAPI InternetQueryInfoString(
    __in HINTERNET h,
    __in DWORD dwInfo,
    __deref_out_z LPWSTR* psczValue
    );

HRESULT DAPI InternetQueryInfoNumber(
    __in HINTERNET h,
    __in DWORD dwInfo,
    __out LONG* plInfo
    );

#ifdef __cplusplus
}
#endif

