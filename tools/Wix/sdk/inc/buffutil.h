//-------------------------------------------------------------------------------------------------
// <copyright file="buffutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//    Binary serialization helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#pragma once


#ifdef __cplusplus
extern "C" {
#endif


// macro definitions

#define ReleaseBuffer ReleaseMem
#define ReleaseNullBuffer ReleaseNullMem
#define BuffFree MemFree


// function declarations

HRESULT BuffReadNumber(
    __in_bcount(cbBuffer) const BYTE* pbBuffer,
    __in SIZE_T cbBuffer,
    __inout SIZE_T* piBuffer,
    __out DWORD* pdw
    );
HRESULT BuffReadNumber64(
    __in_bcount(cbBuffer) const BYTE* pbBuffer,
    __in SIZE_T cbBuffer,
    __inout SIZE_T* piBuffer,
    __out DWORD64* pdw64
    );
HRESULT BuffReadString(
    __in_bcount(cbBuffer) const BYTE* pbBuffer,
    __in SIZE_T cbBuffer,
    __inout SIZE_T* piBuffer,
    __deref_out_z LPWSTR* pscz
    );
HRESULT BuffReadStream(
    __in_bcount(cbBuffer) const BYTE* pbBuffer,
    __in SIZE_T cbBuffer,
    __inout SIZE_T* piBuffer,
    __deref_out_bcount(*pcbStream) BYTE** ppbStream,
    __out SIZE_T* pcbStream
    );

HRESULT BuffWriteNumber(
    __deref_out_bcount(*piBuffer) BYTE** ppbBuffer,
    __inout SIZE_T* piBuffer,
    __in DWORD dw
    );
HRESULT BuffWriteNumber64(
    __deref_out_bcount(*piBuffer) BYTE** ppbBuffer,
    __inout SIZE_T* piBuffer,
    __in DWORD64 dw64
    );
HRESULT BuffWriteString(
    __deref_out_bcount(*piBuffer) BYTE** ppbBuffer,
    __inout SIZE_T* piBuffer,
    __in_z LPCWSTR scz
    );
HRESULT BuffWriteStream(
    __deref_out_bcount(*piBuffer) BYTE** ppbBuffer,
    __inout SIZE_T* piBuffer,
    __in_bcount(cbStream) const BYTE* pbStream,
    __in SIZE_T cbStream
    );

#ifdef __cplusplus
}
#endif
