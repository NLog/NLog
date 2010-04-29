#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="fileutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Header for file helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

#define ReleaseFile(h) if (INVALID_HANDLE_VALUE != h) { ::CloseHandle(h); h = INVALID_HANDLE_VALUE; }
#define ReleaseFileHandle(h) if (INVALID_HANDLE_VALUE != h) { ::CloseHandle(h); h = INVALID_HANDLE_VALUE; }
#define ReleaseFileFindHandle(h) if (INVALID_HANDLE_VALUE != h) { ::FindClose(h); h = INVALID_HANDLE_VALUE; }

enum FILE_ARCHITECTURE
{
    FILE_ARCHITECTURE_UNKNOWN,
    FILE_ARCHITECTURE_X86,
    FILE_ARCHITECTURE_X64,
    FILE_ARCHITECTURE_IA64,
};


LPWSTR DAPI FileFromPath(
    __in_z LPCWSTR wzPath
    );
HRESULT DAPI FileResolvePath(
    __in_z LPCWSTR wzRelativePath,
    __out LPWSTR *ppwzFullPath
    );
HRESULT DAPI FileStripExtension(
    __in_z LPCWSTR wzFileName,
    __out LPWSTR *ppwzFileNameNoExtension
    );
HRESULT DAPI FileChangeExtension(
    __in_z LPCWSTR wzFileName,
    __in_z LPCWSTR wzNewExtension,
    __out LPWSTR *ppwzFileNameNewExtension
    );
HRESULT DAPI FileAddSuffixToBaseName(
    __in_z LPCWSTR wzFileName,
    __in_z LPCWSTR wzSuffix,
    __out_z LPWSTR* psczNewFileName
    );
HRESULT DAPI FileVersionFromString(
    __in_z LPCWSTR wzVersion, 
    __out DWORD *pdwVerMajor, 
    __out DWORD* pdwVerMinor
    );
HRESULT DAPI FileVersionFromStringEx(
    __in_z LPCWSTR wzVersion,
    __in DWORD cchVersion,
    __out DWORD64* pqwVersion
    );
HRESULT DAPI FileSetPointer(
    __in HANDLE hFile,
    __in DWORD64 dw64Move,
    __out_opt DWORD64* pdw64NewPosition,
    __in DWORD dwMoveMethod
    );
HRESULT DAPI FileSize(
    __in_z LPCWSTR pwzFileName,
    __out LONGLONG* pllSize
    );
HRESULT DAPI FileSizeByHandle(
    __in HANDLE hFile, 
    __out LONGLONG* pllSize
    );
BOOL DAPI FileExistsEx(
    __in_z LPCWSTR wzPath, 
    __out_opt DWORD *pdwAttributes
    );
HRESULT DAPI FileRead(
    __deref_out_bcount_full(*pcbDest) LPBYTE* ppbDest,
    __out DWORD* pcbDest,
    __in_z LPCWSTR wzSrcPath
    );
HRESULT DAPI FileReadUntil(
    __deref_out_bcount_full(*pcbDest) LPBYTE* ppbDest,
    __out_range(<=, cbMaxRead) DWORD* pcbDest,
    __in_z LPCWSTR wzSrcPath,
    __in DWORD cbMaxRead
    );
HRESULT DAPI FileReadPartial(
    __deref_out_bcount_full(*pcbDest) LPBYTE* ppbDest,
    __out_range(<=, cbMaxRead) DWORD* pcbDest,
    __in_z LPCWSTR wzSrcPath,
    __in BOOL fSeek,
    __in DWORD cbStartPosition,
    __in DWORD cbMaxRead,
    __in BOOL fPartialOK
    );
HRESULT DAPI FileWrite(
    __in_z LPCWSTR pwzFileName,
    __in DWORD dwFlagsAndAttributes,
    __in_bcount(cbData) LPCBYTE pbData,
    __in DWORD cbData,
    __out_opt HANDLE* pHandle
    );
HRESULT DAPI FileWriteHandle(
    __in HANDLE hFile,
    __in_bcount(cbData) LPCBYTE pbData,
    __in DWORD cbData
    );
HRESULT DAPI FileEnsureCopy(
    __in_z LPCWSTR wzSource,
    __in_z LPCWSTR wzTarget,
    __in BOOL fOverwrite
    );
HRESULT DAPI FileEnsureMove(
    __in_z LPCWSTR wzSource, 
    __in_z LPCWSTR wzTarget, 
    __in BOOL fOverwrite,
    __in BOOL fAllowCopy
    );
HRESULT DAPI FileCreateTemp(
    __in_z LPCWSTR wzPrefix,
    __in_z LPCWSTR wzExtension,
    __deref_opt_out_z LPWSTR* ppwzTempFile,
    __out_opt HANDLE* phTempFile
    );
HRESULT DAPI FileCreateTempW(
    __in_z LPCWSTR wzPrefix,
    __in_z LPCWSTR wzExtension,
    __deref_opt_out_z LPWSTR* ppwzTempFile,
    __out_opt HANDLE* phTempFile
    );
HRESULT DAPI FileVersion(
    __in_z LPCWSTR wzFilename, 
    __out DWORD *pdwVerMajor, 
    __out DWORD* pdwVerMinor
    );
HRESULT DAPI FileIsSame(
    __in_z LPCWSTR wzFile1,
    __in_z LPCWSTR wzFile2,
    __out LPBOOL lpfSameFile
    );
HRESULT DAPI FileEnsureDelete(
    __in_z LPCWSTR wzFile
    );
HRESULT DAPI FileGetTime(
    __in_z LPCWSTR wzFile,  
    __out_opt  LPFILETIME lpCreationTime,
    __out_opt  LPFILETIME lpLastAccessTime,
    __out_opt  LPFILETIME lpLastWriteTime
    );
HRESULT DAPI FileSetTime(
    __in_z LPCWSTR wzFile,
    __in_opt  const FILETIME *lpCreationTime,
    __in_opt  const FILETIME *lpLastAccessTime,
    __in_opt  const FILETIME *lpLastWriteTime
    );
HRESULT DAPI FileResetTime(
    __in_z LPCWSTR wzFile
    );
HRESULT FileExecutableArchitecture(
    __in_z LPCWSTR wzFile,
    __out FILE_ARCHITECTURE *pArchitecture
    );

#ifdef __cplusplus
}
#endif
