#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="dictutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Header for string dict helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

typedef void* STRINGDICT_HANDLE;

HRESULT DAPI DictCreate(
    __out void **ppvHandle,
    __in DWORD dwNumExpectedItems,
    __in size_t cByteOffset
    );
HRESULT DAPI DictAdd(
    __in void *pvHandle,
    __in_z LPCWSTR szString,
    __in void *pvValue
    );
HRESULT DAPI DictGet(
    __in void *pvHandle,
    __in_z LPCWSTR szString,
    __out void **ppvValue
    );
void DAPI DictDestroy(
    __in void *pvHandle
    );

#ifdef __cplusplus
}
#endif
