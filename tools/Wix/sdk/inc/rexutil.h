#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="rexutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Resource Cabinet Extract Utilities
// </summary>
//-------------------------------------------------------------------------------------------------

#include <sys\stat.h>
#include <fdi.h>

#ifdef __cplusplus
extern "C" {
#endif

// defines
#define FILETABLESIZE 40

// structs
struct MEM_FILE 
{
    LPCBYTE vpStart;
    UINT  uiCurrent;
    UINT  uiLength;
};

enum FAKE_FILE_TYPE { NORMAL_FILE, MEMORY_FILE };

typedef HRESULT (*REX_CALLBACK_PROGRESS)(BOOL fBeginFile, LPCWSTR wzFileId, LPVOID pvContext);
typedef VOID (*REX_CALLBACK_WRITE)(UINT cb);


struct FAKE_FILE // used __in internal file table
{
    BOOL fUsed;
    FAKE_FILE_TYPE fftType;
    MEM_FILE mfFile; // State for memory file
    HANDLE hFile; // Handle for disk  file
};

// functions
HRESULT RexInitialize();
void RexUninitialize();

HRESULT RexExtract(
    __in_z LPCSTR szResource,
    __in_z LPCWSTR wzExtractId,
    __in_z LPCWSTR wzExtractDir,
    __in_z LPCWSTR wzExtractName,
    __in REX_CALLBACK_PROGRESS pfnProgress,
    __in REX_CALLBACK_WRITE pfnWrite,
    __in LPVOID pvContext
    );

#ifdef __cplusplus
}
#endif

