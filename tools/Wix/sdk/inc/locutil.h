//-------------------------------------------------------------------------------------------------
// <copyright file="locutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Header for localization helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------
#pragma once

#ifdef __cplusplus
extern "C" {
#endif

struct LOC_STRING
{
    LPWSTR wzID;
    LPWSTR wzText;
    BOOL bOverridable;
};

struct LOC_STRINGSET
{
    DWORD cLocStrings;
    LOC_STRING* rgLocStrings;
};

HRESULT DAPI LocLoadFromFile(
    __in_z LPCWSTR wzWxlFile,
    __out LOC_STRINGSET** ppLocStringSet
    );

HRESULT DAPI LocLocalizeString(
    __in const LOC_STRINGSET* pLocStringSet,
    __inout LPWSTR* ppInput
    );

void DAPI LocFree(
    __in_opt LOC_STRINGSET* pLocStringSet
    );

#ifdef __cplusplus
}
#endif
