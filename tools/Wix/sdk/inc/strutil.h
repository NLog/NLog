#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="strutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Header for string helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

#define ReleaseStr(pwz) if (pwz) { StrFree(pwz); }
#define ReleaseNullStr(pwz) if (pwz) { StrFree(pwz); pwz = NULL; }
#define ReleaseBSTR(bstr) if (bstr) { ::SysFreeString(bstr); }
#define ReleaseNullBSTR(bstr) if (bstr) { ::SysFreeString(bstr); bstr = NULL; }

#define DeclareConstBSTR(bstr_const, wz) const WCHAR bstr_const[] = { 0x00, 0x00, sizeof(wz)-sizeof(WCHAR), 0x00, wz }
#define UseConstBSTR(bstr_const) const_cast<BSTR>(bstr_const + 4)

HRESULT DAPI StrAlloc(
    __deref_out_ecount_part(cch, 0) LPWSTR* ppwz,
    __in DWORD_PTR cch
    );
HRESULT DAPI StrTrimCapacity(
    __deref_out LPWSTR* ppwz
    );
HRESULT DAPI StrAnsiAlloc(
    __deref_out_ecount_part(cch, 0) LPSTR* ppz,
    __in DWORD_PTR cch
    );
HRESULT DAPI StrAnsiTrimCapacity(
    __deref_out LPSTR* ppz
    );
HRESULT DAPI StrAllocString(
    __deref_out_ecount_z(cchSource+1) LPWSTR* ppwz,
    __in_z LPCWSTR wzSource,
    __in DWORD_PTR cchSource
    );
HRESULT DAPI StrAnsiAllocString(
    __deref_out_ecount_z(cchSource+1) LPSTR* ppsz,
    __in_z LPCWSTR wzSource,
    __in DWORD_PTR cchSource,
    __in UINT uiCodepage
    );
HRESULT DAPI StrAllocStringAnsi(
    __deref_out_ecount_z(cchSource+1) LPWSTR* ppwz,
    __in_z LPCSTR szSource,
    __in DWORD_PTR cchSource,
    __in UINT uiCodepage
    );
HRESULT DAPI StrAllocPrefix(
    __deref_out_z LPWSTR* ppwz,
    __in_z LPCWSTR wzPrefix,
    __in DWORD_PTR cchPrefix
    );
HRESULT DAPI StrAllocConcat(
    __deref_out_z LPWSTR* ppwz,
    __in_z LPCWSTR wzSource,
    __in DWORD_PTR cchSource
    );
HRESULT DAPI StrAnsiAllocConcat(
    __deref_out_z LPSTR* ppz,
    __in_z LPCSTR pzSource,
    __in DWORD_PTR cchSource
    );
HRESULT __cdecl StrAllocFormatted(
    __deref_out_z LPWSTR* ppwz,
    __in __format_string LPCWSTR wzFormat,
    ...
    );
HRESULT __cdecl StrAnsiAllocFormatted(
    __deref_out_z LPSTR* ppsz,
    __in __format_string LPCSTR szFormat,
    ...
    );
HRESULT DAPI StrAllocFormattedArgs(
    __deref_out_z LPWSTR* ppwz,
    __in __format_string LPCWSTR wzFormat,
    __in va_list args
    );
HRESULT DAPI StrAnsiAllocFormattedArgs(
    __deref_out_z LPSTR* ppsz,
    __in __format_string LPCSTR szFormat,
    __in va_list args
    );
HRESULT DAPI StrAllocFromError(
    __inout LPWSTR *ppwzMessage,
    __in HRESULT hrError,
    __in_opt HMODULE hModule,
    ...
    );

HRESULT DAPI StrMaxLength(
    __in LPCVOID p,
    __out DWORD_PTR* pcch
    );
HRESULT DAPI StrSize(
    __in LPCVOID p,
    __out DWORD_PTR* pcb
    );

HRESULT DAPI StrFree(
    __in LPVOID p
    );

HRESULT DAPI StrCurrentTime(
    __deref_out_z LPWSTR* ppwz,
    __in BOOL fGMT
    );
HRESULT DAPI StrCurrentDateTime(
    __deref_out_z LPWSTR* ppwz,
    __in BOOL fGMT
    );

HRESULT DAPI StrReplaceStringAll(
    __inout LPWSTR* ppwzOriginal,
    __in_z LPCWSTR wzOldSubString,
    __in_z LPCWSTR wzNewSubString
    );
HRESULT DAPI StrReplaceString(
    __inout LPWSTR* ppwzOriginal,
    __inout DWORD* pdwStartIndex,
    __in_z LPCWSTR wzOldSubString,
    __in_z LPCWSTR wzNewSubString
    );

HRESULT DAPI StrHexEncode(
    __in_ecount(cbSource) const BYTE* pbSource,
    __in DWORD_PTR cbSource,
    __out_ecount(cchDest) LPWSTR wzDest,
    __in DWORD_PTR cchDest
    );
HRESULT DAPI StrHexDecode(
    __in_z LPCWSTR wzSource,
    __out_bcount(cbDest) BYTE* pbDest,
    __in DWORD_PTR cbDest
    );

HRESULT DAPI StrAllocBase85Encode(
    __in_bcount(cbSource) const BYTE* pbSource,
    __in DWORD_PTR cbSource,
    __deref_out_z LPWSTR* pwzDest
    );
HRESULT DAPI StrAllocBase85Decode(
    __in_z LPCWSTR wzSource,
    __deref_out_bcount(*pcbDest) BYTE** hbDest,
    __out DWORD_PTR* pcbDest
    );

HRESULT DAPI MultiSzLen(
    __in_z LPCWSTR pwzMultiSz,
    __out DWORD_PTR* pcch
    );
HRESULT DAPI MultiSzPrepend(
    __deref_inout_z LPWSTR* ppwzMultiSz,
    __inout_opt DWORD_PTR *pcchMultiSz,
    __in_z LPCWSTR pwzInsert
    );
HRESULT DAPI MultiSzFindSubstring(
    __in_z LPCWSTR pwzMultiSz,
    __in_z LPCWSTR pwzSubstring,
    __out_opt DWORD_PTR* pdwIndex,
    __deref_opt_out_z LPCWSTR* ppwzFoundIn
    );
HRESULT DAPI MultiSzFindString(
    __in_z LPCWSTR pwzMultiSz,
    __in_z LPCWSTR pwzString,
    __out_opt DWORD_PTR* pdwIndex,
    __deref_opt_out_z LPCWSTR* ppwzFound
    );
HRESULT DAPI MultiSzRemoveString(
    __deref_inout_z LPWSTR* ppwzMultiSz,
    __in DWORD_PTR dwIndex
    );
HRESULT DAPI MultiSzInsertString(
    __deref_inout_z LPWSTR* ppwzMultiSz,
    __inout_opt DWORD_PTR *pcchMultiSz,
    __in DWORD_PTR dwIndex,
    __in_z LPCWSTR pwzInsert
    );
HRESULT DAPI MultiSzReplaceString(
    __deref_inout_z LPWSTR* ppwzMultiSz,
    __in DWORD_PTR dwIndex,
    __in_z LPCWSTR pwzString
    );

LPCWSTR wcsistr(
    __in_z LPCWSTR wzString,
    __in_z LPCWSTR wzCharSet
    );

HRESULT DAPI StrStringToInt16(
    __in_z LPCWSTR wzIn,
    __in DWORD cchIn,
    __out SHORT* psOut
    );
HRESULT DAPI StrStringToUInt16(
    __in_z LPCWSTR wzIn,
    __in DWORD cchIn,
    __out USHORT* pusOut
    );
HRESULT DAPI StrStringToInt32(
    __in_z LPCWSTR wzIn,
    __in DWORD cchIn,
    __out INT* piOut
    );
HRESULT DAPI StrStringToUInt32(
    __in_z LPCWSTR wzIn,
    __in DWORD cchIn,
    __out UINT* puiOut
    );
HRESULT DAPI StrStringToInt64(
    __in_z LPCWSTR wzIn,
    __in DWORD cchIn,
    __out LONGLONG* pllOut
    );
HRESULT DAPI StrStringToUInt64(
    __in_z LPCWSTR wzIn,
    __in DWORD cchIn,
    __out ULONGLONG* pullOut
    );
void DAPI StrStringToUpper(
    __inout_z LPWSTR wzIn
    );
void DAPI StrStringToLower(
    __inout_z LPWSTR wzIn
    );

#ifdef __cplusplus
}
#endif
