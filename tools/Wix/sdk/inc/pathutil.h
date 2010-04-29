#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="pathutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Header for path helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

enum PATH_EXPAND
{
    PATH_EXPAND_ENVIRONMENT = 0x0001,
    PATH_EXPAND_FULLPATH    = 0x0002,
};

LPWSTR DAPI PathFile(
    __in_z LPCWSTR wzPath
    );
HRESULT DAPI PathGetDirectory(
    __in_z LPCWSTR wzPath,
    __out LPWSTR *psczDirectory
    );
HRESULT DAPI PathExpand(
    __out LPWSTR *psczFullPath,
    __in_z LPCWSTR wzRelativePath,
    __in DWORD dwResolveFlags
    );
HRESULT DAPI PathPrefix(
    __inout LPWSTR *psczFullPath
    );
HRESULT DAPI PathBackslashTerminate(
    __inout LPWSTR* psczPath
    );
HRESULT DAPI PathFixedBackslashTerminate(
    __inout_ecount_z(cchPath) LPWSTR wzPath,
    __in DWORD_PTR cchPath
    );
HRESULT DAPI PathForCurrentProcess(
    __inout LPWSTR *psczFullPath,
    __in_opt HMODULE hModule
    );
HRESULT DAPI PathRelativeToModule(
    __inout LPWSTR *psczFullPath,
    __in_opt LPCWSTR wzFileName,
    __in_opt HMODULE hModule
    );
HRESULT DAPI PathCreateTempFile(
    __in_opt LPCWSTR wzDirectory,
    __in_opt __format_string LPCWSTR wzFileNameTemplate,
    __in DWORD dwUniqueCount,
    __in DWORD dwFileAttributes,
    __out_opt LPWSTR* psczTempFile,
    __out_opt HANDLE* phTempFile
    );
HRESULT DAPI PathCreateTempDirectory(
    __in_opt LPCWSTR wzDirectory,
    __in __format_string LPCWSTR wzDirectoryNameTemplate,
    __in DWORD dwUniqueCount,
    __out LPWSTR* psczTempDirectory
    );
HRESULT DAPI PathGetKnownFolder(
    __in int csidl,
    __out LPWSTR* psczKnownFolder
    );
BOOL DAPI PathIsAbsolute(
    __in_z LPCWSTR wzPath
    );
HRESULT DAPI PathConcat(
    __in_opt LPCWSTR wzPath1,
    __in_opt LPCWSTR wzPath2,
    __deref_out_z LPWSTR* psczCombined
    );

#ifdef __cplusplus
}
#endif
