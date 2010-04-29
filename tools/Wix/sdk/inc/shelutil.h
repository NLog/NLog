#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="shelutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Header for proces helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifndef REFKNOWNFOLDERID
#define REFKNOWNFOLDERID REFGUID
#endif

#ifdef __cplusplus
extern "C" {
#endif

HRESULT DAPI ShelExec(
    __in_z LPCWSTR wzTargetPath,
    __in_opt LPCWSTR wzParameters,
    __in_opt LPCWSTR wzVerb,
    __in_opt LPCWSTR wzWorkingDirectory,
    __in int nShowCmd,
    __out_opt HINSTANCE* phInstance
    );

HRESULT DAPI ShelExecEx(
    __in_z LPCWSTR wzTargetPath,
    __in_z_opt LPCWSTR wzParameters,
    __in_z_opt LPCWSTR wzVerb,
    __in_z_opt LPCWSTR wzWorkingDirectory,
    __in int nShowCmd,
    __out_opt HINSTANCE* phInstance
    );

HRESULT DAPI ShelGetFolder(
    __out_z LPWSTR* psczFolderPath,
    __in int csidlFolder
    );

HRESULT DAPI ShelGetKnownFolder(
    __out_z LPWSTR* psczFolderPath,
    __in REFKNOWNFOLDERID rfidFolder
    );

#ifdef __cplusplus
}
#endif
